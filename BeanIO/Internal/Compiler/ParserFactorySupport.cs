using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;

using BeanIO.Builder;
using BeanIO.Internal.Compiler.Accessor;
using BeanIO.Internal.Config;
using BeanIO.Internal.Parser;
using BeanIO.Internal.Parser.Message;
using BeanIO.Internal.Util;

using JetBrains.Annotations;

namespace BeanIO.Internal.Compiler
{
    /// <summary>
    /// Base <see cref="IParserFactory"/> implementation
    /// </summary>
    /// <remarks>
    /// A <see cref="StreamConfig"/> is "compiled" into a <see cref="Parser.Stream"/> in two passes.  First, a
    /// <see cref="Preprocessor"/> is used to validate and set default configuration settings. And
    /// secondly, the finalized configuration is walked again (using a <see cref="ProcessorSupport"/>,
    /// to create the parser and property tree structure.  As components are initialized they can
    /// be added to the tree structure using stacks with the <see cref="PushParser"/> and
    /// <see cref="PushProperty"/> methods.  After a component is finalized, it should be
    /// removed from the stack using the <see cref="PopParser"/> or <see cref="PopProperty"/> method.
    /// </remarks>
    public abstract class ParserFactorySupport : ProcessorSupport, IParserFactory
    {
        private static readonly string CONSTRUCTOR_PREFIX = "#";

        private static readonly bool AllowProtectedPropertyAccess = Settings.Instance.GetBoolean(Settings.ALLOW_PROTECTED_PROPERTY_ACCESS);

        private static readonly Component _unbound = new UnboundComponent();

        private readonly Stack<Component> _parserStack = new Stack<Component>();

        private readonly Stack<Component> _propertyStack = new Stack<Component>();

        private IPropertyAccessorFactory _accessorFactory;

        private Parser.Stream _stream;

        private string _streamFormat;

        private bool _readEnabled = true;

        private bool _writeEnabled = true;

        /// <summary>
        /// Gets or sets the type handler factory to use for resolving type handlers
        /// </summary>
        public TypeHandlerFactory TypeHandlerFactory { get; set; }

        /// <summary>
        /// Returns a value indicating whether the stream definition must support reading an input stream.
        /// </summary>
        public bool IsReadEnabled
        {
            get { return _readEnabled; }
        }

        /// <summary>
        /// Returns a value indicating whether the stream definition must support writing to an output stream.
        /// </summary>
        public bool IsWriteEnabled
        {
            get { return _writeEnabled; }
        }

        /// <summary>
        /// Gets a value indicating whether a property has been pushed onto the property stack, indicating
        /// that further properties will be bound to a parent property.
        /// </summary>
        protected bool IsBound
        {
            get { return _propertyStack.Count != 0 && _propertyStack.Peek() != _unbound; }
        }

        /// <summary>
        /// Creates a new stream parser from a given stream configuration
        /// </summary>
        /// <param name="config">the stream configuration</param>
        /// <returns>the created <see cref="Parser.Stream"/></returns>
        public Parser.Stream CreateStream(StreamConfig config)
        {
            if (config.Name == null)
                throw new BeanIOConfigurationException("stream name not configured");

            // pre-process configuration settings to set defaults and validate as much as possible 
            CreatePreprocessor(config).Process(config);

            _accessorFactory = new ReflectionAccessorFactory();

            try
            {
                Process(config);
            }
            catch (BeanIOConfigurationException)
            {
                // TODO: Use C# 6 exception filters
                throw;
            }
            catch (Exception ex)
            {
                throw new BeanIOConfigurationException(string.Format("Failed to compile stream '{0}'", config.Name), ex);
            }

            // calculate the heap size
            _stream.Init();

            return _stream;
        }

        /// <summary>
        /// Creates a stream configuration pre-processor
        /// </summary>
        /// <remarks>May be overridden to return a format specific version</remarks>
        /// <param name="config">the stream configuration to pre-process</param>
        /// <returns>the new <see cref="Preprocessor"/></returns>
        protected virtual Preprocessor CreatePreprocessor(StreamConfig config)
        {
            return new Preprocessor(config);
        }

        protected abstract IStreamFormat CreateStreamFormat(StreamConfig config);

        protected abstract IRecordFormat CreateRecordFormat(RecordConfig config);

        protected abstract IFieldFormat CreateFieldFormat(FieldConfig config);

        protected virtual void PushParser(Component component)
        {
            if (_parserStack.Count != 0)
                _parserStack.Peek().Add(component);
            _parserStack.Push(component);
        }

        protected virtual Component PopParser()
        {
            return _parserStack.Pop();
        }

        protected virtual void PushProperty(Component component)
        {
            if (IsBound && component != _unbound)
            {
                // add properties to their parent bean or Map
                var parent = _propertyStack.Peek();
                switch (((IProperty)parent).Type)
                {
                    case PropertyType.Simple:
                        throw new InvalidOperationException();
                    case PropertyType.Collection:
                    case PropertyType.Complex:
                    case PropertyType.Map:
                        parent.Add(component);
                        break;
                }

                // if the parent property is an array or collection, the parser already holds
                // a reference to the child component when pushParser was called
            }

            _propertyStack.Push(component);
        }

        protected virtual IProperty PopProperty()
        {
            var c = _propertyStack.Pop();
            if (c == _unbound)
                return null;

            var last = (IProperty)c;
            if (_propertyStack.Count != 0)
            {
                if (last.IsIdentifier)
                {
                    if (_propertyStack.Any(x => x != _unbound))
                        ((IProperty)_propertyStack.Peek()).IsIdentifier = true;
                }
            }

            return last;
        }

        /// <summary>
        /// Updates a <see cref="Bean"/>'s constructor if one or more of its properties are
        /// constructor arguments.
        /// </summary>
        /// <param name="bean">the <see cref="Bean"/> to check</param>
        protected virtual void UpdateConstructor(Bean bean)
        {
            var args = bean.Children.Cast<IProperty>().Where(x => x.Accessor.IsConstructorArgument).OrderBy(x => x.Accessor.ConstructorArgumentIndex).ToList();

            // return if no constructor arguments were found
            if (args.Count == 0)
                return;

            var count = args.Count;

            // verify the number of constructor arguments matches the provided constructor index
            if (count != args[count - 1].Accessor.ConstructorArgumentIndex + 1)
                throw new BeanIOConfigurationException(string.Format("Missing constructor argument for bean class '{0}'", bean.GetType().GetFullName()));

            // find a suitable constructor
            ConstructorInfo constructor = null;
            foreach (var testConstructor in bean.GetType().GetTypeInfo().DeclaredConstructors.Where(x => x.GetParameters().Length == count))
            {
                var argsMatching = testConstructor.GetParameters().Select((p, i) => p.ParameterType.IsAssignableFrom(args[i].PropertyType)).All(x => x);
                if (argsMatching && (testConstructor.IsPublic || AllowProtectedPropertyAccess))
                {
                    constructor = testConstructor;
                    break;
                }
            }

            // verify a constructor was found
            if (constructor == null)
                throw new BeanIOConfigurationException(string.Format("No suitable constructor found for bean class '{0}'", bean.PropertyType.GetFullName()));

            bean.Constructor = constructor;
        }

        /// <summary>
        /// Initializes a stream configuration before its children have been processed
        /// </summary>
        /// <param name="config">the stream configuration to process</param>
        protected override void InitializeStream(StreamConfig config)
        {
            _streamFormat = config.Format;
            var format = CreateStreamFormat(config);
            _stream = new Parser.Stream(format);

            // set the stream mode, defaults to read/write, the stream mode may be used
            // to enforce or relax validation rules specific to marshalling or unmarshalling
            var mode = config.Mode;
            if (mode == null || mode == AccessMode.ReadWrite)
            {
                _stream.Mode = AccessMode.ReadWrite;
            }
            else if (mode == AccessMode.Read)
            {
                _stream.Mode = AccessMode.Read;
                _writeEnabled = false;
            }
            else if (mode == AccessMode.Write)
            {
                _stream.Mode = AccessMode.Write;
                _readEnabled = false;
            }
            else
            {
                throw new BeanIOConfigurationException(string.Format("Invalid mode '{0}'", mode));
            }

            var messageFactory = new ResourceBundleMessageFactory();
            var bundleName = Settings.Instance.GetProperty(string.Format("org.beanio.{0}.messages", config.Format));
            if (bundleName != null)
            {
                try
                {
                    var resType = Type.GetType(bundleName, true);
                    var resMgr = new ResourceManager(bundleName, resType.GetTypeInfo().Assembly);
                    resMgr.GetString("ignore");
                    messageFactory.DefaultResourceBundle = resMgr;
                }
                catch (MissingManifestResourceException ex)
                {
                    // TODO: Use C# 6 exception filtering
                    throw new BeanIOConfigurationException(string.Format("Missing default resource bundle '{0}' for stream format '{1}'", bundleName, config.Format), ex);
                }
                catch (TypeLoadException ex)
                {
                    throw new BeanIOConfigurationException(string.Format("Missing default resource bundle '{0}' for stream format '{1}'", bundleName, config.Format), ex);
                }
            }

            // load the stream resource bundle
            bundleName = config.ResourceBundle;
            if (bundleName != null)
            {
                try
                {
                    var resType = Type.GetType(bundleName, true);
                    var resMgr = new ResourceManager(bundleName, resType.GetTypeInfo().Assembly);
                    resMgr.GetString("ignore");
                    messageFactory.ResourceBundle = resMgr;
                }
                catch (MissingManifestResourceException ex)
                {
                    // TODO: Use C# 6 exception filtering
                    throw new BeanIOConfigurationException(string.Format("Missing default resource bundle '{0}'", bundleName), ex);
                }
                catch (TypeLoadException ex)
                {
                    throw new BeanIOConfigurationException(string.Format("Missing default resource bundle '{0}'", bundleName), ex);
                }
            }

            _stream.MessageFactory = messageFactory;
            _stream.IgnoreUnidentifiedRecords = config.IgnoreUnidentifiedRecords;
            InitializeGroup(config);
        }

        /// <summary>
        /// Finalizes a stream configuration after its children have been processed
        /// </summary>
        /// <param name="config">the stream configuration to finalize</param>
        protected override void FinalizeStream(StreamConfig config)
        {
            _stream.Layout = (ISelector)_parserStack.Peek();
            FinalizeGroup(config);
        }

        /// <summary>
        /// Initializes a group configuration before its children have been processed
        /// </summary>
        /// <param name="config">the group configuration to process</param>
        protected override void InitializeGroup(GroupConfig config)
        {
            if (config.Children.Count == 0)
                throw new BeanIOConfigurationException("At least one record or group is required.");

            // determine and validate the bean class
            var bean = CreateProperty(config);

            // handle bound repeating groups
            if (config.IsBound && config.IsRepeating)
                InitializeGroupIteration(config, bean);

            InitializeGroupMain(config, bean);
        }

        protected virtual void InitializeGroupIteration(GroupConfig config, IProperty property)
        {
            // wrap the segment in an iteration
            var aggregation = CreateRecordAggregation(config, property);
            PushParser(aggregation);
            if (property != null || config.Target != null)
                PushProperty(aggregation);
        }

        protected virtual void InitializeGroupMain(GroupConfig config, IProperty property)
        {
            var group = new Group()
                {
                    Name = config.Name,
                    MinOccurs = config.MinOccurs ?? 0,
                    MaxOccurs = config.MaxOccurs,
                    Order = config.Order.GetValueOrDefault(),
                    Property = property,
                };
            PushParser(group);
            if (property != null)
                PushProperty((Component)property);
        }

        protected virtual RecordAggregation CreateRecordAggregation(PropertyConfig config, IProperty property)
        {
            var isMap = false;
            var collection = config.Collection;

            // determine the collection type
            Type collectionType = null;
            if (collection != null)
            {
                collectionType = TypeUtil.ToAggregationType(collection);
                if (collectionType == null)
                    throw new BeanIOConfigurationException(string.Format("Invalid collection type or type alias '{0}'", collection));

                isMap = typeof(IDictionary).IsAssignableFrom(collectionType);
                if (isMap && config.Key == null)
                    throw new BeanIOConfigurationException("Key required for Map type collection");

                collectionType = GetConcreteAggregationType(collectionType);
            }

            // create the appropriate iteration type
            RecordAggregation aggregation;
            if (collectionType == typeof(Array))
            {
                aggregation = new RecordArray();
            }
            else if (isMap)
            {
                aggregation = new RecordMap();
            }
            else
            {
                aggregation = new RecordCollection();
            }
            aggregation.Name = config.Name;
            aggregation.PropertyType = collectionType;
            aggregation.IsLazy = config.IsLazy;
            return aggregation;
        }

        /// <summary>
        /// Creates a property for holding other properties
        /// </summary>
        /// <param name="config">the <see cref="PropertyConfig"/></param>
        /// <returns>the created <see cref="IProperty"/> or null if the
        /// <see cref="PropertyConfig"/> was not bound to a bean class</returns>
        protected virtual IProperty CreateProperty(PropertyConfig config)
        {
            var beanClass = GetBeanClass(config);
            if (beanClass == null)
                return null;

            IProperty property;
            if (typeof(IList).IsAssignableFrom(beanClass))
            {
                var required = _propertyStack.Count == 0;
                if (config.ComponentType == ComponentType.Segment)
                    required = config.MinOccurs.GetValueOrDefault() > 0 && !config.IsNillable;
                var matchNull = !required && config.MinOccurs.GetValueOrDefault() == 0;

                var collection = new CollectionBean()
                    {
                        Name = config.Name,
                        PropertyType = beanClass,
                        IsRequired = required,
                        IsMatchNull = matchNull,
                    };
                property = collection;
            }
            else
            {
                var required = _propertyStack.Count == 0;
                var matchNull = !required && config.MinOccurs.GetValueOrDefault() == 0;

                var bean = new Bean()
                    {
                        Name = config.Name,
                        PropertyType = beanClass,
                        IsLazy = config.IsLazy,
                        IsRequired = required,
                        IsMatchNull = matchNull,
                    };

                property = bean;
            }

            return property;
        }

        /// <summary>
        /// Returns the bean class for a segment configuration
        /// </summary>
        /// <param name="config">the property configuration</param>
        /// <returns>the bean class</returns>
        protected virtual Type GetBeanClass(PropertyConfig config)
        {
            // determine the bean class associated with this record
            Type beanClass = null;
            if (config.Type != null)
            {
                beanClass = config.Type.ToBeanType();
                if (beanClass == null)
                    throw new BeanIOConfigurationException(string.Format("Invalid bean class '{0}'", config.Type));
                var beanTypeInfo = beanClass.GetTypeInfo();
                if (IsReadEnabled && (beanTypeInfo.IsInterface || beanTypeInfo.IsAbstract))
                    throw new BeanIOConfigurationException(string.Format("Class must be concrete unless stream mode is set to '{0}'", AccessMode.Write));
            }
            return beanClass;
        }

        /// <summary>
        /// Returns a concrete <see cref="Type"/> implementation for an aggregation type
        /// </summary>
        /// <param name="type">the configured <see cref="IDictionary"/> or <see cref="ICollection"/> type</param>
        /// <returns>the concrete aggregation Class type</returns>
        private Type GetConcreteAggregationType(Type type)
        {
            if (type == null)
                return null;
            if (type != typeof(Array) && (type.GetTypeInfo().IsInterface || type.GetTypeInfo().IsAbstract))
            {
                if (typeof(ISet<>).IsAssignableFrom(type))
                    return typeof(HashSet<>);
                if (typeof(IDictionary).IsAssignableFrom(type))
                    return typeof(Dictionary<,>);
                return typeof(List<>);
            }
            return type;
        }

        private class UnboundComponent : Component
        {
            public UnboundComponent()
            {
                Name = "unbound";
            }
        }
    }
}
