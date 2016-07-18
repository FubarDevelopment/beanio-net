// <copyright file="JsonParserFactory.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Reflection;

using BeanIO.Internal.Config;
using BeanIO.Internal.Parser;
using BeanIO.Internal.Parser.Format.Json;
using BeanIO.Stream;
using BeanIO.Stream.Json;

using Newtonsoft.Json.Linq;

namespace BeanIO.Internal.Compiler.Json
{
    /// <summary>
    /// A <see cref="IParserFactory"/> for the JSON stream format.
    /// </summary>
    internal class JsonParserFactory : ParserFactorySupport
    {
        /// <summary>
        /// depth starts at one to accommodate the root JSON object
        /// </summary>
        private int _maxDepth = 1;

        /// <summary>
        /// Creates a new stream parser from a given stream configuration
        /// </summary>
        /// <param name="config">the stream configuration</param>
        /// <returns>the created <see cref="Parser.Stream"/></returns>
        public override Parser.Stream CreateStream(StreamConfig config)
        {
            var stream = base.CreateStream(config);
            ((JsonStreamFormat)stream.Format).MaxDepth = _maxDepth;
            return stream;
        }

        /// <summary>
        /// Creates the default <see cref="IRecordParserFactory"/>.
        /// </summary>
        /// <returns>
        /// The new <see cref="IRecordParserFactory"/>
        /// </returns>
        protected override IRecordParserFactory CreateDefaultRecordParserFactory()
        {
            return new JsonRecordParserFactory();
        }

        protected override IStreamFormat CreateStreamFormat(StreamConfig config)
        {
            return new JsonStreamFormat()
                {
                    Name = config.Name,
                    RecordParserFactory = CreateRecordParserFactory(config),
                };
        }

        protected override IRecordFormat CreateRecordFormat(RecordConfig config)
        {
            return null;
        }

        protected override IFieldFormat CreateFieldFormat(FieldConfig config, Type type)
        {
            var format = new JsonFieldFormat()
                {
                    Name = config.Name,
                    JsonName = config.JsonName,
                    IsJsonArray = config.IsJsonArray,
                    JsonArrayIndex = config.JsonArrayIndex,
                    IsLazy = config.MinOccurs != null && config.MinOccurs == 0,
                    IsNillable = true, // for now, allow any JSON field to be nullable
                };

            var determinedJsonType = DetermineTokenType(type);
            if (string.IsNullOrEmpty(config.JsonType))
            {
                // default the JSON type based on the property type
                format.JsonType = determinedJsonType;
                format.BypassTypeHandler = determinedJsonType != JTokenType.String;
            }
            else
            {
                // or set if explicitly configured
                var jsonType = (JTokenType)Enum.Parse(typeof(JTokenType), config.JsonType, true);
                format.BypassTypeHandler = jsonType != JTokenType.String && jsonType == determinedJsonType;
                format.JsonType = jsonType;
            }

            return format;
        }

        /// <summary>
        /// Creates a stream configuration pre-processor
        /// </summary>
        /// <remarks>May be overridden to return a format specific version</remarks>
        /// <param name="config">the stream configuration to pre-process</param>
        /// <returns>the new <see cref="Preprocessor"/></returns>
        protected override Preprocessor CreatePreprocessor(StreamConfig config)
        {
            return new JsonPreprocessor(config);
        }

        /// <summary>
        /// Called by <see cref="ParserFactorySupport.InitializeSegment"/> to initialize segment iteration.
        /// </summary>
        /// <param name="config">the segment configuration</param>
        /// <param name="property">the <see cref="IProperty"/> bound to the segment, or null if no bean was bound</param>
        protected override void InitializeSegmentIteration(SegmentConfig config, IProperty property)
        {
            var wrapper = new JsonWrapper()
                {
                    Name = config.Name,
                    JsonName = config.JsonName,
                    JsonType = JTokenType.Array,
                    JsonArrayIndex = config.JsonArrayIndex,
                    IsNillable = true,
                };
            wrapper.SetOptional(config.MinOccurs == 0);

            PushParser(wrapper);
            ++_maxDepth;

            base.InitializeSegmentIteration(config, property);
        }

        /// <summary>
        /// Called by <see cref="ParserFactorySupport.FinalizeSegment(BeanIO.Internal.Config.SegmentConfig)"/> to finalize segment iteration.
        /// </summary>
        /// <param name="config">the segment configuration</param>
        /// <param name="property">the property bound to the segment, or null if no property was bound</param>
        protected override void FinalizeSegmentIteration(SegmentConfig config, IProperty property)
        {
            base.FinalizeSegmentIteration(config, property);
            PopParser(); // pop the wrapper
        }

        /// <summary>
        /// Called by <see cref="ParserFactorySupport.InitializeSegment"/> to initialize the segment.
        /// </summary>
        /// <param name="config">the segment configuration</param>
        /// <param name="property">the property bound to the segment, or null if no property was bound</param>
        protected override void InitializeSegmentMain(SegmentConfig config, IProperty property)
        {
            if (IsWrappingRequired(config))
            {
                var wrapper = new JsonWrapper()
                    {
                        Name = config.Name,
                        JsonName = config.JsonName,
                        JsonType = ConvertJsonType(config.JsonType),
                        JsonArrayIndex = config.JsonArrayIndex,
                        IsNillable = true,
                    };
                wrapper.SetOptional(config.MinOccurs == 0);
                PushParser(wrapper);
                ++_maxDepth;
            }

            base.InitializeSegmentMain(config, property);
        }

        /// <summary>
        /// Called by <see cref="ParserFactorySupport.FinalizeSegment(BeanIO.Internal.Config.SegmentConfig)"/> to finalize the segment component.
        /// </summary>
        /// <param name="config">the segment configuration</param>
        /// <returns>the target property</returns>
        protected override IProperty FinalizeSegmentMain(SegmentConfig config)
        {
            var property = base.FinalizeSegmentMain(config);
            if (IsWrappingRequired(config))
            {
                PopParser(); // pop the wrapper
            }

            return property;
        }

        protected override bool IsSegmentRequired(SegmentConfig config)
        {
            if (config.IsConstant)
                return false;
            if (!string.IsNullOrEmpty(config.Type))
                return true;
            if (config.Children.Count > 1)
                return true;
            return false;
        }

        private static JTokenType DetermineTokenType(Type type)
        {
            if (!type.GetTypeInfo().IsPrimitive)
                return JTokenType.String;
            var baseType = Nullable.GetUnderlyingType(type) ?? type;
            if (baseType == typeof(bool))
            {
                return JTokenType.Boolean;
            }

            if (baseType == typeof(float) || baseType == typeof(double) || baseType == typeof(decimal))
            {
                return JTokenType.Float;
            }

            return JTokenType.Integer;
        }

        private bool IsWrappingRequired(SegmentConfig config)
        {
            return !string.IsNullOrEmpty(config.JsonType)
                   && !string.Equals(config.JsonType, JTokenType.None.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private JTokenType ConvertJsonType(string type)
        {
            var tokenType = (JTokenType)Enum.Parse(typeof(JTokenType), type, true);
            if (tokenType != JTokenType.Array && tokenType != JTokenType.Object)
                throw new BeanIOConfigurationException($"Invalid jsonType '{type}'");
            return tokenType;
        }
    }
}
