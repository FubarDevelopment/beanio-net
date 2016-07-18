// <copyright file="JsonPreprocessor.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

using BeanIO.Internal.Config;

using Newtonsoft.Json.Linq;

namespace BeanIO.Internal.Compiler.Json
{
    /// <summary>
    /// Configuration <see cref="Preprocessor"/> for the JSON stream format.
    /// </summary>
    internal class JsonPreprocessor : Preprocessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPreprocessor"/> class.
        /// </summary>
        /// <param name="stream">the <see cref="StreamConfig"/> to preprocess</param>
        public JsonPreprocessor(StreamConfig stream)
            : base(stream)
        {
        }

        /// <summary>
        /// Initializes a record configuration before its children have been processed
        /// </summary>
        /// <param name="record">the record configuration to process</param>
        protected override void InitializeRecord(RecordConfig record)
        {
            // default 'jsonName' to the record name
            if (record.JsonName == null)
            {
                record.JsonName = record.Name;
            }

            // default the JSON type to 'none'
            if (string.IsNullOrEmpty(record.JsonType))
            {
                record.JsonType = "None";
            }
            else
            {
                if (record.JsonType.EndsWith("[]", StringComparison.Ordinal))
                {
                    throw new BeanIOConfigurationException($"Invalid jsonType '{record.JsonType}', [] not supported");
                }
            }

            base.InitializeRecord(record);
        }

        /// <summary>
        /// Initializes a segment configuration before its children have been processed
        /// </summary>
        /// <param name="segment">the segment configuration to process</param>
        protected override void InitializeSegment(SegmentConfig segment)
        {
            base.InitializeSegment(segment);

            // default the JSON name to the segment name
            if (segment.JsonName == null)
                segment.JsonName = segment.Name;

            // default the JSON type to 'object' if the segment is bound to bean object, or 'none' otherwise
            if (string.IsNullOrEmpty(segment.JsonType))
            {
                if (!string.IsNullOrEmpty(segment.Type))
                {
                    segment.JsonType = JTokenType.Object.ToString();
                    segment.IsJsonArray = segment.IsRepeating;
                }
                else
                {
                    segment.JsonType = JTokenType.None.ToString();
                }
            }
            else
            {
                // otherwise validate the type
                var type = segment.JsonType;
                if (type.EndsWith("[]", StringComparison.Ordinal))
                {
                    type = type.Substring(0, type.Length - 2);
                    segment.IsJsonArray = true;
                }
                else if (segment.IsRepeating && segment.ComponentType != ComponentType.Record)
                {
                    throw new BeanIOConfigurationException($"Invalid jsonType '{segment.JsonType}', expected 'object[]'");
                }

                JTokenType tokenType;
                if (!Enum.TryParse(type, true, out tokenType))
                    tokenType = JTokenType.Undefined;
                switch (tokenType)
                {
                    case JTokenType.Object:
                    case JTokenType.Array:
                    case JTokenType.None:
                        break;
                    default:
                        throw new BeanIOConfigurationException($"Invalid jsonType '{segment.JsonType}'");
                }

                segment.JsonType = type;
            }
        }

        /// <summary>
        /// Finalizes a segment configuration after its children have been processed
        /// </summary>
        /// <param name="segment">the segment configuration to finalize</param>
        protected override void FinalizeSegment(SegmentConfig segment)
        {
            base.FinalizeSegment(segment);

            if (string.Equals(segment.JsonType, JTokenType.Array.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                int n = 0;
                foreach (var property in segment.PropertyList)
                {
                    property.JsonArrayIndex = n++;
                }
            }
        }

        /// <summary>
        /// Processes a field configuration
        /// </summary>
        /// <param name="field">the field configuration to process</param>
        protected override void HandleField(FieldConfig field)
        {
            base.HandleField(field);

            // default the JSON name to the field name
            if (string.IsNullOrEmpty(field.JsonName))
                field.JsonName = field.Name;

            // validate the JSON type if set
            if (!string.IsNullOrEmpty(field.JsonType))
            {
                var type = field.JsonType;
                if (type.EndsWith("[]", StringComparison.Ordinal))
                {
                    type = type.Substring(0, type.Length - 2);
                    field.IsJsonArray = true;
                }
                else if (field.IsRepeating)
                {
                    throw new BeanIOConfigurationException($"Invalid jsonType '{field.JsonType}', expected array");
                }

                switch (type.ToLowerInvariant())
                {
                    case "number":
                        type = "Integer";
                        break;
                }

                JTokenType tokenType;
                if (!Enum.TryParse(type, true, out tokenType))
                    tokenType = JTokenType.Undefined;
                switch (tokenType)
                {
                    case JTokenType.String:
                    case JTokenType.Integer:
                    case JTokenType.Boolean:
                    case JTokenType.Float:
                        break;
                    default:
                        throw new BeanIOConfigurationException($"Invalid jsonType '{field.JsonType}'");
                }

                field.JsonType = type;
            }
            else if (field.IsRepeating)
            {
                field.IsJsonArray = true;
            }
        }
    }
}
