// <copyright file="ParserFactorySupport.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Resources;

using BeanIO.Builder;
using BeanIO.Config;
using BeanIO.Internal.Compiler.Accessor;
using BeanIO.Internal.Config;
using BeanIO.Internal.Parser;
using BeanIO.Internal.Parser.Accessor;
using BeanIO.Internal.Parser.Message;
using BeanIO.Internal.Util;
using BeanIO.Stream;
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
    internal abstract class ParserFactorySupport : ProcessorSupport, IParserFactory
    {
        private static readonly string CONSTRUCTOR_PREFIX = "#";

        private static readonly bool _allowProtectedPropertyAccess = Settings.Instance.GetBoolean(Settings.ALLOW_PROTECTED_PROPERTY_ACCESS);

        private static readonly string _propertyAccessorFactory = Settings.Instance.GetProperty(Settings.PROPERTY_ACCESSOR_METHOD);

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
        /// Gets a value indicating whether the stream definition must support reading an input stream.
        /// </summary>
        public bool IsReadEnabled => _readEnabled;

        /// <summary>
        /// Gets a value indicating whether the stream definition must support writing to an output stream.
        /// </summary>
        public bool IsWriteEnabled => _writeEnabled;

        /// <summary>
        /// Gets a value indicating whether a property has been pushed onto the property stack, indicating
        /// that further properties will be bound to a parent property.
        /// </summary>
        protected bool IsBound => _propertyStack.Count != 0 && _propertyStack.Peek() != _unbound;

        /// <summary>
        /// Creates a new stream parser from a given stream configuration
        /// </summary>
        /// <param name="config">the stream configuration</param>
        /// <returns>the created <see cref="Parser.Stream"/></returns>
        public virtual Parser.Stream CreateStream(StreamConfig config)
        {
            if (config.Name == null)
                throw new BeanIOConfigurationException("stream name not configured");

            // pre-process configuration settings to set defaults and validate as much as possible
            CreatePreprocessor(config).Process(config);

            if (_propertyAccessorFactory == "asm")
            {
                _accessorFactory = new CompiledExpressionAccessorFactory();
            }
            else if (string.IsNullOrEmpty(_propertyAccessorFactory) || _propertyAccessorFactory == "reflection")
            {
                _accessorFactory = new ReflectionAccessorFactory();
            }
            else
            {
                _accessorFactory = (IPropertyAccessorFactory)BeanUtil.CreateBean(_propertyAccessorFactory);
            }

            try
            {
                Process(config);
            }
            catch (Exception ex) when ((ex as BeanIOConfigurationException) == null)
            {
                throw new BeanIOConfigurationException($"Failed to compile stream '{config.Name}'", ex);
            }

            // calculate the heap size
            _stream.Init();

            return _stream;
        }

        /// <summary>
        /// Creates the default <see cref="IRecordParserFactory"/>.
        /// </summary>
        /// <returns>
        /// The new <see cref="IRecordParserFactory"/>
        /// </returns>
        protected abstract IRecordParserFactory CreateDefaultRecordParserFactory();

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

        protected abstract IFieldFormat CreateFieldFormat(FieldConfig config, Type type);

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
                throw new BeanIOConfigurationException($"Missing constructor argument for bean class '{bean.GetType().GetAssemblyQualifiedName()}'");

            // find a suitable constructor
            ConstructorInfo constructor = null;
            var beanType = bean.PropertyType;
            foreach (var testConstructor in beanType.GetTypeInfo().DeclaredConstructors.Where(x => x.GetParameters().Length == count))
            {
                var argsMatching = testConstructor.GetParameters().Select((p, i) => p.ParameterType.IsAssignableFromThis(args[i].PropertyType)).All(x => x);
                if (argsMatching && (testConstructor.IsPublic || _allowProtectedPropertyAccess))
                {
                    constructor = testConstructor;
                    break;
                }
            }

            // verify a constructor was found
            if (constructor == null)
            {
                throw new BeanIOConfigurationException(
                    $"No suitable constructor found for bean class '{bean.PropertyType.GetAssemblyQualifiedName()}'");
            }

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
                throw new BeanIOConfigurationException($"Invalid mode '{mode}'");
            }

            var messageFactory = new ResourceBundleMessageFactory();
            var bundleName = Settings.Instance.GetProperty($"org.beanio.{config.Format}.messages");
            if (bundleName != null)
            {
                messageFactory.DefaultResourceBundle = LoadResource(bundleName, config.Format);
            }

            // load the stream resource bundle
            bundleName = config.ResourceBundle;
            if (bundleName != null)
            {
                messageFactory.ResourceBundle = LoadResource(bundleName, config.Format);
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
            _stream.Layout = (ISelector)_parserStack.Last();
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
            record.SetSize(config.MaxSize);

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
            {
                FinalizeRecordIteration(config, property);
            }
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

            var recordMap = aggregation as RecordMap;

            // assumes key is not null only for map aggregation
            var key = config.Key;
            if (key != null)
            {
                // aggregations only have a single descendant so calling getFirst() is safe
                Component c = FindDescendant("key", aggregation.First, key);
                if (c == null)
                    throw new BeanIOConfigurationException($"Key '{key}' not found");

                IProperty keyProperty = c as IProperty;
                if (keyProperty?.PropertyType == null)
                    throw new BeanIOConfigurationException($"Key '{key}' is not a property");

                Debug.Assert(recordMap != null, "recordMap != null");
                recordMap.KeyProperty = keyProperty;
            }

            if (recordMap != null)
            {
                var propertyTypeInfo = recordMap.PropertyType.GetTypeInfo();
                if (propertyTypeInfo.IsGenericTypeDefinition)
                {
                    var keyType = recordMap.KeyProperty != null ? recordMap.KeyProperty.PropertyType : typeof(object);
                    var valueType = recordMap.ValueProperty != null ? recordMap.ValueProperty.PropertyType : typeof(object);
                    recordMap.PropertyType = recordMap.PropertyType.MakeGenericType(keyType, valueType);
                }
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
                aggregation.Occurs = occurs;
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

            var segment = new Segment()
                {
                    Name = name,
                    IsIdentifier = config.IsIdentifier,
                    IsRepeating = config.IsRepeating,
                    Property = property,
                    IsExistencePredetermined = config.IsDefaultExistence,
                };

            segment.SetSize(config.MaxSize);
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
        /// Called by <see cref="FinalizeSegment(SegmentConfig)"/> to finalize segment iteration.
        /// </summary>
        /// <param name="config">the segment configuration</param>
        /// <param name="property">the property bound to the segment, or null if no property was bound</param>
        protected virtual void FinalizeSegmentIteration(SegmentConfig config, IProperty property)
        {
            var aggregation = (Aggregation)PopParser();
            var mapParser = aggregation as MapParser;
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
                    throw new BeanIOConfigurationException($"Key '{key}' not found");

                var keyProperty = c as IProperty;
                if (keyProperty?.PropertyType == null)
                    throw new BeanIOConfigurationException($"Key '{key}' is not a property");

                Debug.Assert(mapParser != null, "mapParser != null");
                mapParser.KeyProperty = keyProperty;
            }

            if (mapParser != null)
            {
                var propertyTypeInfo = mapParser.PropertyType.GetTypeInfo();
                if (propertyTypeInfo.IsGenericTypeDefinition)
                {
                    var keyType = mapParser.KeyProperty != null ? mapParser.KeyProperty.PropertyType : typeof(object);
                    var valueType = mapParser.ValueProperty != null ? mapParser.ValueProperty.PropertyType : typeof(object);
                    mapParser.PropertyType = mapParser.PropertyType.MakeGenericType(keyType, valueType);
                }
            }
        }

        /// <summary>
        /// Called by <see cref="FinalizeSegment(SegmentConfig)"/> to finalize the segment component.
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
                    Debug.Assert(segment != null, "segment != null");
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
        /// <param name="config">the field configuration to process</param>
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

            if (config.ParseDefault.HasValue)
                field.ParseDefault = config.ParseDefault.Value;

            if (config.ValidateOnMarshal != null)
                field.ValidateOnMarshal = config.ValidateOnMarshal.Value;

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
                    throw new BeanIOConfigurationException($"Invalid type or type alias '{config.Type}'");
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
            field.Handler = FindTypeHandler(config, field);

            // set the default field value using the configured type handler
            field.DefaultValue = ParseDefaultValue(field, config.DefaultValue);

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
            if (config.Type != null)
            {
                Type propertyType = config.Type.ToType();
                if (propertyType == null)
                    throw new BeanIOConfigurationException($"Invalid type or type alias '{config.Type}'");
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
                    throw new BeanIOConfigurationException($"Type conversion failed for configured value '{text}': {ex.Message}", ex);
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
            var isMap = false;

            var collection = config.Collection;

            // determine the collection type
            Type collectionType = null;
            if (collection != null)
            {
                collectionType = collection.ToAggregationType(property);
                if (collectionType == null)
                {
                    throw new BeanIOConfigurationException($"Invalid collection type or type alias '{collection}'");
                }

                isMap = collectionType.IsMap();
                if (isMap && config.ComponentType == ComponentType.Field)
                {
                    throw new BeanIOConfigurationException("Map type collections are not supported for fields");
                }

                if (isMap && ((SegmentConfig)config).Key == null)
                {
                    throw new BeanIOConfigurationException("Key required for Map type collection");
                }

                collectionType = GetConcreteAggregationType(collectionType);

                if (config.IsNillable && collectionType.IsConstructedGenericType && !collectionType.IsMap())
                {
                    var elementType = collectionType.GenericTypeArguments[0];
                    if (elementType.GetTypeInfo().IsPrimitive)
                    {
                        elementType = typeof(Nullable<>).MakeGenericType(elementType);
                        collectionType = collectionType.GetGenericTypeDefinition().MakeGenericType(elementType);
                    }
                }
            }

            // create the appropriate iteration type
            Aggregation aggregation;
            if (collectionType.IsArray())
            {
                var elementType = property.PropertyType ?? typeof(object);
                var collParser = new ArrayParser
                    {
                        ElementType = elementType
                    };
                aggregation = collParser;
                collectionType = elementType.MakeArrayType();
            }
            else if (isMap)
            {
                aggregation = new MapParser();
            }
            else
            {
                aggregation = new CollectionParser();
            }

            aggregation.Name = config.Label;
            if (config.OccursRef != null)
            {
                aggregation.MinOccurs = config.MinOccursRef ?? 0;
                aggregation.MaxOccurs = config.MaxOccursRef;
            }
            else
            {
                aggregation.MinOccurs = config.MinOccurs ?? 0;
                aggregation.MaxOccurs = config.MaxOccurs;
            }

            aggregation.IsLazy = config.IsLazy;
            aggregation.PropertyType = collectionType;
            return aggregation;
        }

        protected virtual void ReflectAggregationType(PropertyConfig config, Aggregation aggregation, IProperty property)
        {
            var collectionType = aggregation.PropertyType;

            // if collection was set, then this is a property of its parent
            if (collectionType != null)
            {
                var reflectedType = ReflectCollectionType(aggregation, property, config.Getter, config.Setter);

                // descriptor may be null if the parent was Map or Collection
                var collectionParser = aggregation as CollectionParser;
                if (collectionParser != null)
                {
                    var arrayType = property.PropertyType;

                    // reflectedType may be null if our parent is a Map
                    if (reflectedType != null)
                    {
                        if (collectionType.IsArray)
                        {
                            // use the reflected component type for an array
                            arrayType = reflectedType.GetElementType();
                        }
                        else if (reflectedType.IsConstructedGenericType)
                        {
                            arrayType = reflectedType.GenericTypeArguments[0];
                        }

                        if (arrayType != null && !arrayType.IsAssignableFromThis(property.PropertyType))
                        {
                            // override target type if we were able to reflect its value and the property type
                            // cannot be cast to the array type
                            property.PropertyType = arrayType;
                        }
                    }
                    else if (arrayType == null)
                    {
                        // default to String
                        arrayType = typeof(string);
                    }

                    collectionParser.ElementType = arrayType;
                }
                else
                {
                    var mapParser = aggregation as MapParser;
                    if (mapParser != null)
                    {
                        mapParser.ValueProperty = property;
                    }
                }
            }
        }

        protected virtual RecordAggregation CreateRecordAggregation(PropertyConfig config, IProperty property)
        {
            var isMap = false;
            var collection = config.Collection;

            // determine the collection type
            Type collectionType = null;
            if (collection != null)
            {
                collectionType = collection.ToAggregationType(property);
                if (collectionType == null)
                    throw new BeanIOConfigurationException($"Invalid collection type or type alias '{collection}'");

                isMap = collectionType.IsMap();
                if (isMap && config.Key == null)
                    throw new BeanIOConfigurationException("Key required for Map type collection");

                collectionType = GetConcreteAggregationType(collectionType);
            }

            // create the appropriate iteration type
            RecordAggregation aggregation;
            if (collectionType.IsArray())
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

        protected virtual void ReflectRecordAggregationType(PropertyConfig config, RecordAggregation aggregation, IProperty property)
        {
            var collectionType = aggregation.PropertyType;
            if (collectionType == null)
                return;

            // if collection was set, then this is a property of its parent
            var reflectedType = ReflectCollectionType(aggregation, property, config.Getter, config.Setter);

            // descriptor may be null if the parent was Map or Collection
            if (collectionType.IsArray())
            {
                var arrayType = property.PropertyType;

                // reflected type may be null if the parent bean is a Map
                if (reflectedType != null)
                {
                    // use the reflected component type for an array
                    arrayType = reflectedType.GetElementType();

                    // override target type if we were able to reflect its value
                    property.PropertyType = arrayType;
                }
                else if (arrayType == null)
                {
                    // default to String
                    arrayType = typeof(string);
                }

                ((RecordArray)aggregation).ElementType = arrayType;
            }
            else
            {
                var recordMap = aggregation as RecordMap;
                if (recordMap != null)
                {
                    recordMap.ValueProperty = property;
                }
            }
        }

        protected virtual Type ReflectCollectionType(IProperty iteration, IProperty property, string getter, string setter)
        {
            if (!IsBound)
                return null;

            var parent = (IProperty)_propertyStack.Peek();
            switch (parent.Type)
            {
                case PropertyType.Simple:
                    throw new BeanIOConfigurationException("Cannot add property to attribute");
                case PropertyType.Collection:
                case PropertyType.AggregationArray:
                case PropertyType.AggregationCollection:
                case PropertyType.AggregationMap:
                    return null;
                case PropertyType.Map:
                    iteration.Accessor = new MapAccessor(iteration.Name);
                    return null;
            }

            // parse the constructor argument index from the 'setter'
            int? construtorArgumentIndex = null;
            if (setter != null && setter.StartsWith(CONSTRUCTOR_PREFIX))
            {
                try
                {
                    construtorArgumentIndex = int.Parse(setter.Substring(1)) - 1;
                    if (construtorArgumentIndex < 0)
                        throw new BeanIOConfigurationException("Invalid setter method");
                }
                catch (FormatException ex)
                {
                    throw new BeanIOConfigurationException("Invalid setter method", ex);
                }

                setter = null;
            }

            // set the property descriptor on the field
            var descriptor = GetPropertyDescriptor(iteration.Name, getter, setter, (construtorArgumentIndex ?? -1) >= 0);
            var reflectedType = descriptor.PropertyType;

            iteration.Accessor = _accessorFactory.CreatePropertyAccessor(parent.PropertyType, descriptor, construtorArgumentIndex);

            // reflectedType may be null for read-only streams using a constructor argument
            if (reflectedType == null)
                return null;

            var type = property.PropertyType;
            if (iteration.PropertyType.IsArray())
            {
                if (!reflectedType.IsArray())
                {
                    throw new BeanIOConfigurationException(
                        $"Collection type 'array' does not match bean property type '{reflectedType.GetAssemblyQualifiedName()}'");
                }

                var arrayType = reflectedType.GetElementType();
                if (type == null)
                {
                    property.PropertyType = arrayType;
                }
                else if (!arrayType.IsAssignableFromThis(type))
                {
                    throw new BeanIOConfigurationException(
                        $"Configured field array of type '{type}' is not assignable to bean property array of type '{arrayType.GetAssemblyQualifiedName()}'");
                }
            }
            else
            {
                if (!reflectedType.IsAssignableFromThis(iteration.PropertyType))
                {
                    if (reflectedType.IsConstructedGenericType)
                    {
                        if (iteration.PropertyType.IsConstructedGenericType)
                        {
                            var propType = iteration.PropertyType;
                            propType = propType.GetGenericTypeDefinition();
                            propType = propType.MakeGenericType(reflectedType.GenericTypeArguments);
                            iteration.PropertyType = propType;
                        }
                    }
                    else if (reflectedType.IsInstanceOf(typeof(IList)) && iteration.PropertyType.IsList())
                    {
                        iteration.PropertyType = reflectedType;
                    }
                }

                if (!reflectedType.IsAssignableFromThis(iteration.PropertyType))
                {
                    string beanPropertyTypeName;
                    if (reflectedType.IsArray())
                    {
                        beanPropertyTypeName = reflectedType.GetElementType().GetAssemblyQualifiedName() + "[]";
                    }
                    else
                    {
                        beanPropertyTypeName = reflectedType.GetAssemblyQualifiedName();
                    }

                    throw new BeanIOConfigurationException(
                        $"Configured collection type '{iteration.PropertyType.GetAssemblyQualifiedName()}' is not assignable to bean property type '{beanPropertyTypeName}'");
                }
            }

            return reflectedType;
        }

        /// <summary>
        /// Sets the property type and accessor using bean introspection.
        /// </summary>
        /// <param name="config">the property configuration</param>
        /// <param name="property">the property</param>
        protected virtual void ReflectPropertyType(PropertyConfig config, IProperty property)
        {
            // check for constructor arguments
            if (property.Type == PropertyType.Complex)
                UpdateConstructor((Bean)property);

            if (!IsBound)
                return;

            var parent = (IProperty)_propertyStack.Peek();
            switch (parent.Type)
            {
                case PropertyType.Simple:
                    throw new BeanIOConfigurationException("Cannot add a property to a simple property");
                case PropertyType.Collection:
                case PropertyType.AggregationArray:
                case PropertyType.AggregationCollection:
                case PropertyType.AggregationMap:
                    return;
                case PropertyType.Map:
                    property.Accessor = new MapAccessor(config.Name);
                    return;
            }

            var setter = config.Setter;
            var getter = config.Getter;

            // parse the constructor argument index from the 'setter'
            int? construtorArgumentIndex = null;
            if (setter != null && setter.StartsWith(CONSTRUCTOR_PREFIX))
            {
                try
                {
                    construtorArgumentIndex = int.Parse(setter.Substring(1)) - 1;
                    if (construtorArgumentIndex < 0)
                        throw new BeanIOConfigurationException("Invalid setter method");
                }
                catch (FormatException ex)
                {
                    throw new BeanIOConfigurationException("Invalid setter method", ex);
                }

                setter = null;
            }

            // set the property descriptor on the field
            var descriptor = GetPropertyDescriptor(config.Name, getter, setter, (construtorArgumentIndex ?? -1) >= 0);
            var reflectedType = descriptor.PropertyType;

            property.Accessor = _accessorFactory.CreatePropertyAccessor(parent.PropertyType, descriptor, construtorArgumentIndex);

            // validate the reflected type
            var type = property.PropertyType;
            if (type == null)
            {
                property.PropertyType = reflectedType;
            }
            else if (reflectedType != null)
            {
                if (!reflectedType.IsAssignableFromThis(type))
                {
                    // reflectedType may be null if for read-only streams using a constructor argument
                    throw new BeanIOConfigurationException($"Property type '{config.Type}' is not assignable to bean property type '{reflectedType.GetAssemblyQualifiedName()}'");
                }

                if (reflectedType.GetTypeInfo().IsPrimitive)
                {
                    property.PropertyType = reflectedType;
                }
                else if (!type.IsConstructedGenericType && type.GetTypeInfo().IsGenericType)
                {
                    if (reflectedType.IsConstructedGenericType)
                    {
                        property.PropertyType = type.Instantiate(reflectedType);
                    }
                }
            }
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
            if (beanClass.IsList())
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
                    throw new BeanIOConfigurationException($"Invalid bean class '{config.Type}'");
                var beanTypeInfo = beanClass.GetTypeInfo();
                if (IsReadEnabled && (beanTypeInfo.IsInterface || beanTypeInfo.IsAbstract))
                    throw new BeanIOConfigurationException($"Class must be concrete unless stream mode is set to '{AccessMode.Write}'");
            }

            return beanClass;
        }

        /// <summary>
        /// Parses a default field value.
        /// </summary>
        /// <param name="field">the field</param>
        /// <param name="text">the text to parse</param>
        /// <returns>the default value</returns>
        protected virtual object ParseDefaultValue(Field field, string text)
        {
            if (text == null)
                return null;

            var handler = field.Handler;
            if (handler == null)
                return text;

            if (!field.ParseDefault)
                return text;

            try
            {
                return handler.Parse(text);
            }
            catch (TypeConversionException ex)
            {
                throw new BeanIOConfigurationException($"Type conversion failed for configured default '{text}': {ex.Message}", ex);
            }
            catch (FormatException ex)
            {
                throw new BeanIOConfigurationException($"Type conversion failed for configured default '{text}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates the <see cref="IRecordParserFactory"/> for a stream configuration.
        /// </summary>
        /// <param name="config">the stream configuration</param>
        /// <returns>the created <see cref="IRecordParserFactory"/></returns>
        protected virtual IRecordParserFactory CreateRecordParserFactory(StreamConfig config)
        {
            IRecordParserFactory factory;

            // configure the record writer factory
            BeanConfig<IRecordParserFactory> parserFactoryBean = config.ParserFactory;
            if (parserFactoryBean == null)
            {
                factory = CreateDefaultRecordParserFactory();
            }
            else if (parserFactoryBean.Create != null)
            {
                factory = parserFactoryBean.Create();
            }
            else
            {
                if (parserFactoryBean.ClassName == null)
                {
                    factory = CreateDefaultRecordParserFactory();
                }
                else
                {
                    factory = BeanUtil.CreateBean(parserFactoryBean.ClassName) as IRecordParserFactory;
                    if (factory == null)
                    {
                        throw new BeanIOConfigurationException(
                            $"Configured writer factory class '{parserFactoryBean.ClassName}' does not implement RecordWriterFactory");
                    }
                }

                BeanUtil.Configure(factory, parserFactoryBean.Properties);
            }

            try
            {
                Debug.Assert(factory != null, "factory != null");
                factory.Init();
                return factory;
            }
            catch (ArgumentException ex)
            {
                throw new BeanIOConfigurationException($"Invalid parser setting(s): {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Load a resource manager for a given bundle name and format
        /// </summary>
        /// <param name="bundleName">The (assembly qualified) name of the resource</param>
        /// <param name="format">The format for this resource</param>
        /// <returns>The resource manager</returns>
        protected virtual ResourceManager LoadResource(string bundleName, string format)
        {
            try
            {
                ResourceManager resMgr;
                var resType = Type.GetType(bundleName, false);
                if (resType == null)
                {
                    var parts = bundleName.Split(new[] { ',' }, 2);
                    if (parts.Length != 2)
                        throw new BeanIOConfigurationException($"Missing assembly name in resource bundle name {bundleName} for stream format '{format}'");

                    var assemblyName = parts[1];
                    var asm = Assembly.Load(new AssemblyName(assemblyName));
                    resMgr = new ResourceManager(bundleName, asm);
                }
                else
                {
                    // The type must contain a "ResourceManager" property
                    var prop = resType.GetRuntimeProperty("ResourceManager");
                    if (prop == null)
                        throw new BeanIOConfigurationException($"Missing property ResourceManager in resource bundle {bundleName} for stream format '{format}'");

                    resMgr = (ResourceManager)prop.GetValue(null);
                }

                // Test if the resource data could be loaded successfully
                resMgr.GetString("ignore");

                return resMgr;
            }
            catch (MissingManifestResourceException ex)
            {
                throw new BeanIOConfigurationException($"Missing default resource bundle '{bundleName}' for stream format '{format}'", ex);
            }
            catch (TypeLoadException ex)
            {
                throw new BeanIOConfigurationException($"Missing default resource bundle '{bundleName}' for stream format '{format}'", ex);
            }
        }

        private IProperty FindTarget([NotNull] Component segment, [NotNull] string name)
        {
            Component c = FindDescendant("value", segment, name);
            if (c == null)
                throw new BeanIOConfigurationException($"Descendant value '{name}' not found");

            var property = c as IProperty;
            if (property?.PropertyType == null)
                throw new BeanIOConfigurationException($"No class defined for value '{name}'");

            return property;
        }

        private Component FindDescendant([NotNull] string type, [NotNull] Component c, [NotNull] string name)
        {
            if (string.Equals(name, c.Name, StringComparison.Ordinal))
                return c;

            foreach (var child in c.Children)
            {
                var match = FindDescendant(type, child, name);
                if (match != null)
                {
                    if (c is IIteration)
                    {
                        throw new BeanIOConfigurationException(
                            $"Referenced component '{name}' of type '{type}' may not repeat, or belong to a segment that repeats");
                    }

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
                throw new BeanIOConfigurationException($"Referenced field '{name}' not found");
            if (!f.PropertyType.IsNumber())
                throw new BeanIOConfigurationException($"Referenced field '{name}' must be assignable to a number");
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
            if (!type.IsArray() && (type.GetTypeInfo().IsInterface || type.GetTypeInfo().IsAbstract))
            {
                if (type.IsSet())
                    return typeof(HashSet<>).Instantiate(type);
                if (type.IsMap())
                    return typeof(Dictionary<,>).Instantiate(type);
                if (type.IsList())
                    return typeof(List<>).Instantiate(type);
                return typeof(List<object>);
            }

            return type;
        }

        /// <summary>
        /// Returns the <see cref="PropertyDescriptor"/> for getting and setting a property value from
        /// current bean class on the property stack.
        /// </summary>
        /// <param name="property">the property name</param>
        /// <param name="getter">the getter method name, or null to use the default</param>
        /// <param name="setter">the setter method name, or null to use the default</param>
        /// <param name="isConstructorArgument">is this a constructor argument?</param>
        /// <returns>the <see cref="PropertyDescriptor"/></returns>
        private PropertyDescriptor GetPropertyDescriptor(string property, string getter, string setter, bool isConstructorArgument)
        {
            var beanClass = ((IProperty)_propertyStack.Peek()).PropertyType;
            var descriptor = BeanUtil.GetPropertyDescriptor(beanClass, property, getter, setter, isConstructorArgument);

            // validate a read method is found for mapping configurations that write streams
            if (!isConstructorArgument && IsReadEnabled && !descriptor.HasSetter)
            {
                throw new BeanIOConfigurationException(
                    $"No writeable access for property or field '{property}' in class '{beanClass.GetAssemblyQualifiedName()}'");
            }

            if (IsWriteEnabled && !descriptor.HasGetter)
            {
                throw new BeanIOConfigurationException(
                    $"No readable access for property or field '{property}' in class '{beanClass.GetAssemblyQualifiedName()}'");
            }

            return descriptor;
        }

        /// <summary>
        /// Updates a simple property with its type and accessor, and returns a type handler for it.
        /// </summary>
        /// <param name="config">the property configuration</param>
        /// <param name="field">the property to update</param>
        /// <returns>a type handler for the property</returns>
        private ITypeHandler FindTypeHandler(SimplePropertyConfig config, IProperty field)
        {
            var propertyType = field.PropertyType;

            // configure type handler properties
            Properties typeHandlerProperties = null;
            if (config.Format != null)
            {
                typeHandlerProperties = new Properties(new Dictionary<string, string>()
                    {
                        { DefaultTypeConfigurationProperties.FORMAT_SETTING, config.Format },
                    });
            }

            // determine the type handler based on the named handler or the field class
            ITypeHandler handler = null;
            if (config.TypeHandlerInstance != null)
            {
                handler = config.TypeHandlerInstance;
            }
            else if (config.TypeHandler != null)
            {
                handler = TypeHandlerFactory.GetTypeHandler(config.TypeHandler, typeHandlerProperties);
                if (handler == null)
                    throw new BeanIOConfigurationException($"No configured type handler named '{config.TypeHandler}'");
            }

            if (handler != null)
            {
                // if the property type was not already determine, use the type from the type handler
                if (propertyType == null)
                {
                    propertyType = handler.TargetType;
                    field.PropertyType = propertyType;
                }
                else if (!propertyType.IsAssignableFromThis(handler.TargetType))
                {
                    // otherwise validate the property type is compatible with the type handler
                    throw new BeanIOConfigurationException(
                        $"Field property type '{propertyType.GetAssemblyQualifiedName()}' is not compatible with assigned type handler named '{config.TypeHandler}'");
                }
            }
            else
            {
                // assume String type if the property type was not determined any other way
                if (propertyType == null)
                {
                    propertyType = typeof(string);
                    field.PropertyType = propertyType;
                }

                // get a type handler for the the property type
                var typeName = config.Type;
                try
                {
                    if (typeName == null)
                    {
                        typeName = propertyType.GetAssemblyQualifiedName();
                        handler = TypeHandlerFactory.GetTypeHandlerFor(propertyType, _streamFormat, typeHandlerProperties);
                    }
                    else
                    {
                        handler = TypeHandlerFactory.GetTypeHandlerFor(typeName, _streamFormat, typeHandlerProperties);
                    }
                }
                catch (ArgumentException ex)
                {
                    throw new BeanIOConfigurationException(ex.Message, ex);
                }

                if (handler == null)
                {
                    throw new BeanIOConfigurationException($"Type handler not found for type '{typeName}'");
                }
            }

            return handler;
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
