using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Resources;

using BeanIO.Builder;
using BeanIO.Internal.Compiler.Accessor;
using BeanIO.Internal.Config;
using BeanIO.Internal.Parser;
using BeanIO.Internal.Parser.Message;
using BeanIO.Internal.Util;
using BeanIO.Types;

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

        /// <summary>
        /// Finalizes a group configuration after its children have been processed
        /// </summary>
        /// <param name="config">the group configuration to finalize</param>
        protected override void FinalizeGroup(GroupConfig config)
        {
            var property = FinalizeGroupMain(config);
            if (config.IsBound && config.IsRepeating)
                FinalizeGroupIteration(config, property);
        }

        protected virtual IProperty FinalizeGroupMain(GroupConfig config)
        {
            IProperty property = null;

            // pop the group bean from the property stack
            if (config.Type != null)
            {
                property = PopProperty();
                ReflectPropertyType(config, property);
            }

            // pop the record from the parser stack
            FinalizeGroup(config, (Group)PopParser());

            return property;
        }

        protected virtual void FinalizeGroupIteration(GroupConfig config, IProperty property)
        {
            // pop the iteration from the parser stack
            var aggregation = (RecordAggregation)PopParser();

            // pop the collection from the property stack
            if (config.Type != null)
            {
                PopProperty();
                ReflectRecordAggregationType(config, aggregation, property);
            }
        }

        /// <summary>
        /// Invoked by <see cref="FinalizeGroupMain"/> to allow subclasses to perform
        /// further finalization of the created <see cref="Group"/>.
        /// </summary>
        /// <param name="config">the group configuration</param>
        /// <param name="group">the <see cref="Group"/> being finalized</param>
        protected virtual void FinalizeGroup(GroupConfig config, Group group)
        {
            var target = config.Target;
            if (target != null)
                group.Property = FindTarget(group, target);
        }

        /// <summary>
        /// Initializes a record configuration before its children have been processed
        /// </summary>
        /// <param name="config">the record configuration to process</param>
        protected override void InitializeRecord(RecordConfig config)
        {
            // determine and validate the bean class
            var bean = CreateProperty(config);

            // handle bound repeating records
            if (config.IsBound && config.IsRepeating)
            {
                InitializeRecordIteration(config, bean);
            }

            InitializeRecordMain(config, bean);
        }

        protected virtual void InitializeRecordIteration(RecordConfig config, IProperty property)
        {
            // wrap the segment in an iteration
            var collection = CreateRecordAggregation(config, property);

            PushParser(collection);
            if (property != null || config.Target != null)
            {
                PushProperty(collection);
            }
        }

        protected virtual void InitializeRecordMain(RecordConfig config, IProperty property)
        {
            Debug.Assert(config.MaxSize != null, "config.MaxSize != null");

            var record = new Record()
                {
                    Name = config.Name,
                    MinOccurs = config.MinOccurs ?? 0,
                    MaxOccurs = config.MaxOccurs,
                    IsRepeating = config.IsRepeating,
                    RecordFormat = CreateRecordFormat(config),
                    Order = config.Order ?? 0,
                    IsIdentifier = config.IsIdentifier,
                    Property = property,
                };

            record.SetOptional(config.MaxOccurs == null || (config.MinOccurs ?? 0) < config.MaxOccurs);
            record.SetSize(config.MaxSize.Value);

            if (property != null)
            {
                PushProperty((Component)property);
            }
            else if (config.Target != null)
            {
                PushProperty(_unbound);
            }

            PushParser(record);
        }

        /// <summary>
        /// Finalizes a record configuration after its children have been processed
        /// </summary>
        /// <param name="config">the record configuration to finalize</param>
        protected override void FinalizeRecord(RecordConfig config)
        {
            var property = FinalizeRecordMain(config);
            if (config.IsBound && config.IsRepeating)
                FinalizeRecordIteration(config, property);
        }

        protected virtual IProperty FinalizeRecordMain(RecordConfig config)
        {
            IProperty property = null;
            var record = (Record)PopParser();

            // pop the record bean from the property stack
            if (config.Type != null || config.Target != null)
            {
                property = PopProperty(); // could be null if 'target' was set

                var target = config.Target;
                if (target != null)
                {
                    property = FindTarget(record, target);
                    PushProperty((Component)property);
                    PopProperty();
                    record.Property = property;
                }

                if (property != null)
                {
                    ReflectPropertyType(config, property);
                }
            }

            // pop the record from the parser stack
            FinalizeRecord(config, record);

            return property;
        }

        protected virtual void FinalizeRecordIteration(RecordConfig config, IProperty property)
        {
            // pop the iteration from the parser stack
            var aggregation = (RecordAggregation)PopParser();

            // pop the collection from the property stack
            if (config.Type != null || config.Target != null)
            {
                PopProperty();
                ReflectRecordAggregationType(config, aggregation, property);
            }

            // assumes key is not null only for map aggregation
            var key = config.Key;
            if (key != null)
            {
                // aggregations only have a single descendant so calling getFirst() is safe
                Component c = FindDescendant("key", aggregation.First, key);
                if (c == null)
                    throw new BeanIOConfigurationException(string.Format("Key '{0}' not found", key));

                IProperty keyProperty = c as IProperty;
                if (keyProperty == null || keyProperty.PropertyType == null)
                    throw new BeanIOConfigurationException(string.Format("Key '{0}' is not a property", key));

                ((RecordMap)aggregation).Key = keyProperty;
            }
        }

        /// <summary>
        /// Invoked by <see cref="FinalizeRecord(RecordConfig)"/> to allow subclasses to perform
        /// further finalization of the created <see cref="Record"/>.
        /// </summary>
        /// <param name="config">the record configuration</param>
        /// <param name="record">the <see cref="Record"/> being finalized</param>
        protected virtual void FinalizeRecord(RecordConfig config, Record record)
        {
        }

        /// <summary>
        /// Initializes a segment configuration before its children have been processed
        /// </summary>
        /// <param name="config">the segment configuration to process</param>
        protected override void InitializeSegment(SegmentConfig config)
        {
            var bean = CreateProperty(config);
            if (config.IsRepeating)
                InitializeSegmentIteration(config, bean);
            InitializeSegmentMain(config, bean);
        }

        /// <summary>
        /// Called by <see cref="InitializeSegment"/> to initialize segment iteration.
        /// </summary>
        /// <param name="config">the segment configuration</param>
        /// <param name="property">the <see cref="IProperty"/> bound to the segment, or null if no bean was bound</param>
        protected virtual void InitializeSegmentIteration(SegmentConfig config, IProperty property)
        {
            // wrap the segment in an aggregation component
            var aggregation = CreateAggregation(config, property);

            if (config.OccursRef != null)
            {
                var occurs = FindDynamicOccurs(_parserStack.Peek(), config.OccursRef);
                aggregation.setOccurs(occurs);
            }

            PushParser(aggregation);
            if (property != null || config.Target != null)
                PushProperty(aggregation);
        }

        /// <summary>
        /// Called by <see cref="InitializeSegment"/> to initialize the segment.
        /// </summary>
        /// <param name="config">the segment configuration</param>
        /// <param name="property">the property bound to the segment, or null if no property was bound</param>
        protected virtual void InitializeSegmentMain(SegmentConfig config, IProperty property)
        {
            var name = config.Name;
            if (name == null)
                throw new BeanIOConfigurationException("Segment name not set");

            Debug.Assert(config.MaxSize != null, "config.MaxSize != null");

            var segment = new Segment()
                {
                    Name = name,
                    IsIdentifier = config.IsIdentifier,
                    IsRepeating = config.IsRepeating,
                    Property = property,
                    IsExistencePredetermined = config.IsDefaultExistence,
                };

            segment.SetSize(config.MaxSize.Value);
            segment.SetOptional(config.MaxOccurs == null || (config.MinOccurs ?? 0) < config.MaxOccurs);

            if (IsSegmentRequired(config))
                PushParser(segment);

            if (property != null)
            {
                PushProperty((Component)property);
            }
            else if (config.Target != null)
            {
                PushProperty(_unbound);
            }
        }

        protected virtual bool IsSegmentRequired(SegmentConfig config)
        {
            return config.Type != null || config.Target != null;
        }

        /// <summary>
        /// Finalizes a segment configuration after its children have been processed
        /// </summary>
        /// <param name="config">the segment configuration to finalize</param>
        protected override void FinalizeSegment(SegmentConfig config)
        {
            var property = FinalizeSegmentMain(config);
            if (config.IsRepeating)
                FinalizeSegmentIteration(config, property);
        }

        /// <summary>
        /// Called by <see cref="FinalizeSegment"/> to finalize segment iteration.
        /// </summary>
        /// <param name="config">the segment configuration</param>
        /// <param name="property">the property bound to the segment, or null if no property was bound</param>
        protected virtual void FinalizeSegmentIteration(SegmentConfig config, IProperty property)
        {
            var aggregation = (Aggregation)PopParser();
            if (config.Type != null || config.Target != null)
            {
                PopProperty(); // pop the iteration
                ReflectAggregationType(config, aggregation, property);
            }

            // assumes key is not null only for map aggregation
            var key = config.Key;
            if (key != null)
            {
                // aggregations only have a single descendant so calling getFirst() is safe
                Component c = FindDescendant("key", aggregation.First, key);
                if (c == null)
                    throw new BeanIOConfigurationException(string.Format("Key '{0}' not found", key));

                var keyProperty = c as IProperty;
                if (keyProperty == null || keyProperty.PropertyType == null)
                    throw new BeanIOConfigurationException(string.Format("Key '{0}' is not a property", key));

                ((MapParser)aggregation).Key = keyProperty;
            }
        }

        /// <summary>
        /// Called by <see cref="FinalizeSegment"/> to finalize the segment component.
        /// </summary>
        /// <param name="config">the segment configuration</param>
        /// <returns>the target property</returns>
        protected virtual IProperty FinalizeSegmentMain(SegmentConfig config)
        {
            IProperty property = null;
            Segment segment = null;

            if (IsSegmentRequired(config))
            {
                segment = (Segment)PopParser();
                FinalizeSegment(config, segment);
            }

            if (config.Type != null || config.Target != null)
            {
                property = PopProperty(); // could be null if 'target' was set

                var target = config.Target;
                if (target != null)
                {
                    property = FindTarget(segment, target);
                    PushProperty((Component)property);
                    PopProperty();
                    segment.Property = property;
                }

                if (property != null)
                    ReflectPropertyType(config, property);
            }

            return property;
        }

        /// <summary>
        /// Invoked by <see cref="FinalizeSegmentMain"/> to allow subclasses to perform
        /// further finalization of the created <see cref="Segment"/>.
        /// </summary>
        /// <param name="config">the segment configuration</param>
        /// <param name="segment">the new <see cref="Segment"/></param>
        protected virtual void FinalizeSegment(SegmentConfig config, Segment segment)
        {
        }

        /// <summary>
        /// Processes a field configuration
        /// </summary>
        /// <param name="field">the field configuration to process</param>
        protected override void HandleField(FieldConfig config)
        {
            if (config.Name == null)
                throw new BeanIOConfigurationException("Missing field name");

            var field = new Field()
                {
                    Name = config.Name,
                    IsIdentifier = config.IsIdentifier,
                    IsRequired = config.IsRequired,
                    IsTrim = config.IsTrim,
                    IsLazy = config.IsLazy,
                    Literal = config.Literal,
                    MinLength = config.MinLength ?? 0,
                    MaxLength = config.MaxLength,
                    IsBound = config.IsBound,
                };

            try
            {
                field.Regex = config.RegEx;
            }
            catch (Exception ex)
            {
                throw new BeanIOConfigurationException("Invalid regex pattern", ex);
            }

            // set the property type if explicitly configured
            if (config.Type != null)
            {
                var propertyType = config.Type.ToType();
                if (propertyType == null)
                    throw new BeanIOConfigurationException(string.Format("Invalid type or type alias '{0}'", config.Type));
                field.PropertyType = propertyType;
            }

            // whether or not this property is bound to a bean property, Collections targets are not
            var bind = IsBound && (config.IsBound && !config.IsRepeating);

            Aggregation aggregation = null;
            if (config.IsRepeating)
            {
                aggregation = CreateAggregation(config, field);

                if (config.OccursRef != null)
                {
                    Field occurs = FindDynamicOccurs(_parserStack.Peek(), config.OccursRef);
                    aggregation.Occurs = occurs;
                }

                PushParser(aggregation);
                if (aggregation.IsProperty)
                    PushProperty(aggregation);
            }
            else
            {
                if (bind)
                    ReflectPropertyType(config, field);
            }

            // if not already determined, this will update the field type
            field.Handler = FindTypeHandler(config, field));

            // set the default field value using the configured type handler
            field.DefaultValue  = ParseDefaultValue(field, config.Default));

            field.Format = CreateFieldFormat(config, field.PropertyType);

            PushParser(field);
            if (bind)
            {
                PushProperty(field);
                PopProperty();
            }
            PopParser();

            if (aggregation != null)
            {
                PopParser();
                if (aggregation.IsProperty)
                    PopProperty();

                ReflectAggregationType(config, aggregation, field);
            }
        }

        /// <summary>
        /// Processes a constant configuration
        /// </summary>
        /// <param name="config">the constant configuration to process</param>
        protected override void HandleConstant(ConstantConfig config)
        {
            var constant = new Constant()
                {
                    Name = config.Name,
                    IsIdentifier = config.IsIdentifier,
                };

            // determine the property type
            Type propertyType = null;
            if (config.Type != null)
            {
                propertyType = config.Type.ToType();
                if (propertyType == null)
                    throw new BeanIOConfigurationException(string.Format("Invalid type or type alias '{0}'", config.Type));
            }

            ReflectPropertyType(config, constant);
            ITypeHandler handler = FindTypeHandler(config, constant);

            // set the property value using the configured type handler
            var text = config.Value;
            if (text != null)
            {
                try
                {
                    constant.Value = handler.Parse(text);
                }
                catch (TypeConversionException ex)
                {
                    throw new BeanIOConfigurationException(string.Format("Type conversion failed for configured value '{0}': {1}", text, ex.Message), ex);
                }
            }

            PushProperty(constant);
            PopProperty();
        }

        /// <summary>
        /// Creates an iteration for a repeating segment or field.
        /// </summary>
        /// <param name="config">the property configuration</param>
        /// <param name="property">the property component, may be null if the iteration is not a property of its parent bean</param>
        /// <returns>the iteration component</returns>
        protected virtual Aggregation CreateAggregation(PropertyConfig config, IProperty property)
        {
            
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

        private IProperty FindTarget(Component segment, string name)
        {
            Component c = FindDescendant("value", segment, name);
            if (c == null)
                throw new BeanIOConfigurationException(string.Format("Descendant value '{0}' not found", name));

            var property = c as IProperty;
            if (property == null || property.PropertyType == null)
                throw new BeanIOConfigurationException(string.Format("No class defined for value '{0}'", name));

            return property;
        }

        private Component FindDescendant(string type, Component c, string name)
        {
            if (string.Equals(name, c.Name, StringComparison.Ordinal))
                return c;

            foreach (var child in c.Children)
            {
                var match = FindDescendant(type, child, name);
                if (match != null)
                {
                    if (c is IIteration)
                        throw new BeanIOConfigurationException(string.Format("Referenced component '{0}' may not repeat, or belong to a segment that repeats", name));
                    return match;
                }
            }

            return null;
        }

        private Field FindDynamicOccurs(Component segment, string name)
        {
            var c = FindDescendant("value", segment, name);
            var f = c as Field;
            if (f == null)
                throw new BeanIOConfigurationException(string.Format("Referenced field '{0}' not found", name));
            if (!f.PropertyType.IsNumber())
                throw new BeanIOConfigurationException(string.Format("Referenced field '{0}' must be assignable to a number", name));
            return f;
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
