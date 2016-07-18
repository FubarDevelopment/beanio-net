// <copyright file="Preprocessor.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using BeanIO.Internal.Config;
using BeanIO.Internal.Util;

namespace BeanIO.Internal.Compiler
{
    /// <summary>
    /// A Pre-processer is responsible for validating a stream configuration, setting
    /// default configuration values, and populating any calculated values before the
    /// <see cref="ParserFactorySupport"/> compiles the configuration into parser components.
    /// </summary>
    internal class Preprocessor : ProcessorSupport
    {
        private static readonly Settings _settings = Settings.Instance;

        private static readonly bool SORT_XML_COMPONENTS = Settings.Instance.GetBoolean(Settings.SORT_XML_COMPONENTS_BY_POSITION);

        private readonly StreamConfig _stream;

        private bool _recordIgnored;

        public Preprocessor(StreamConfig stream)
        {
            _stream = stream;
        }

        /// <summary>
        /// Gets the stream configuration
        /// </summary>
        protected StreamConfig Stream => _stream;

        protected PropertyConfig PropertyRoot { get; set; }

        protected override void InitializeStream(StreamConfig stream)
        {
            if (stream.MinOccurs == null)
                stream.MinOccurs = 0;
            if (stream.MaxOccurs == null)
                stream.MaxOccurs = 1;
            if (stream.MaxOccurs <= 0)
                throw new BeanIOConfigurationException("Maximum occurrences must be greater than 0");
            InitializeGroup(stream);
        }

        /// <summary>
        /// Finalizes a stream configuration after its children have been processed
        /// </summary>
        /// <param name="stream">the stream configuration to finalize</param>
        protected override void FinalizeStream(StreamConfig stream)
        {
            FinalizeGroup(stream);

            bool sortingRequired = SORT_XML_COMPONENTS || !string.Equals("xml", stream.Format, StringComparison.Ordinal);
            if (sortingRequired)
                stream.Sort(new ComponentConfigComparer());
        }

        /// <summary>
        /// Initializes a group configuration before its children have been processed
        /// </summary>
        /// <param name="group">the group configuration to process</param>
        protected override void InitializeGroup(GroupConfig group)
        {
            if (group.MinOccurs == null)
                group.MinOccurs = _settings.GetInt(Settings.DEFAULT_GROUP_MIN_OCCURS, 0);
            if (group.MaxOccurs == null)
                group.MaxOccurs = int.MaxValue;
            if (group.MaxOccurs <= 0)
                throw new BeanIOConfigurationException("Maximum occurrences must be greater than 0");
            if (group.MaxOccurs < group.MinOccurs)
                throw new BeanIOConfigurationException("Maximum occurences cannot be less than mininum occurences");

            // validate both 'class' and 'target' aren't set
            if (group.Type != null && group.Target != null)
                throw new BeanIOConfigurationException("Cannot set both 'class' and 'value'");

            if (PropertyRoot != null)
            {
                group.IsBound = true;
                if (group.Collection != null && group.Type == null)
                    throw new BeanIOConfigurationException("Class required if collection is set");

                if (group.Type != null
                    && (group.MaxOccurs == null || group.MaxOccurs > 1)
                    && group.Collection == null)
                {
                    throw new BeanIOConfigurationException("Collection required when maxOccurs is greater than 1 and class is set");
                }

                if (group.IsRepeating && group.Collection == null)
                    group.IsBound = false;
            }

            if (PropertyRoot == null && (group.Type != null || group.Target != null))
                PropertyRoot = group;

            if (Parent != null && group.ValidateOnMarshal == null)
                group.ValidateOnMarshal = Parent.ValidateOnMarshal;
        }

        /// <summary>
        /// Finalizes a group configuration after its children have been processed
        /// </summary>
        /// <param name="group">the group configuration to finalize</param>
        protected override void FinalizeGroup(GroupConfig group)
        {
            // order must be set for all group children, or for none of them
            // if order is specified...
            //   -validate group children are in ascending order
            // otherwise if order is not specified...
            //   -if strict, all children have current order incremented
            //   -if not, all children have order set to 1
            var lastOrder = 0;
            bool? orderSet = null;
            foreach (var child in group.Children.Cast<ISelectorConfig>())
            {
                string typeDescription = child.ComponentType == ComponentType.Record ? "record" : "group";

                if (child.Order != null && child.Order < 0)
                    throw new BeanIOConfigurationException("Order must be 1 or greater");
                if (orderSet == null)
                {
                    orderSet = child.Order != null;
                }
                else if (orderSet.Value ^ (child.Order != null))
                {
                    throw new BeanIOConfigurationException("Order must be set all children at a group level, or none at all");
                }

                if (orderSet.Value)
                {
                    if (child.Order < lastOrder)
                        throw new BeanIOConfigurationException($"'{child.Name}' {typeDescription} configuration is out of order");
                    lastOrder = child.Order.Value;
                }
                else
                {
                    if (_stream.IsStrict)
                    {
                        child.Order = ++lastOrder;
                    }
                    else
                    {
                        child.Order = 1;
                    }
                }
            }

            if (PropertyRoot == group)
                PropertyRoot = null;
        }

        /// <summary>
        /// Initializes a record configuration before its children have been processed
        /// </summary>
        /// <param name="record">the record configuration to process</param>
        protected override void InitializeRecord(RecordConfig record)
        {
            // a record is ignored if a 'class' was not set and the property root is null
            // or the record repeats
            _recordIgnored = false;
            if (record.Type == null && record.Target == null)
            {
                if (PropertyRoot == null || record.IsRepeating)
                    _recordIgnored = true;
            }

            // assign default min and max occurs
            if (record.MinOccurs == null)
                record.MinOccurs = _settings.GetInt(Settings.DEFAULT_RECORD_MIN_OCCURS, 0);
            if (record.MaxOccurs == null)
                record.MaxOccurs = int.MaxValue;
            if (record.MaxOccurs <= 0)
                throw new BeanIOConfigurationException("Maximum occurrences must be greater than 0");

            if (PropertyRoot == null)
            {
                PropertyRoot = record;
                if (record.IsLazy)
                    throw new BeanIOConfigurationException("Lazy cannot be true for unbound records");
            }

            InitializeSegment(record);
        }

        /// <summary>
        /// Finalizes a record configuration after its children have been processed
        /// </summary>
        /// <param name="record">the record configuration to finalize</param>
        protected override void FinalizeRecord(RecordConfig record)
        {
            FinalizeSegment(record);

            if (PropertyRoot == record)
                PropertyRoot = null;
        }

        /// <summary>
        /// Initializes a segment configuration before its children have been processed
        /// </summary>
        /// <param name="segment">the segment configuration to process</param>
        protected override void InitializeSegment(SegmentConfig segment)
        {
            if (segment.Name == null)
                throw new BeanIOConfigurationException("name must be set");
            if (segment.Label == null)
                segment.Label = segment.Name;

            // validate both 'class' and 'target' aren't set
            if (segment.Type != null && segment.Target != null)
                throw new BeanIOConfigurationException("Cannot set both 'class' and 'value'");

            // set default occurrences and validate
            if (segment.MinOccurs == null)
                segment.MinOccurs = segment.OccursRef != null ? 0 : 1;
            if (segment.MaxOccurs == null)
                segment.MaxOccurs = segment.OccursRef != null ? int.MaxValue : 1;
            if (segment.MaxOccurs <= 0)
                throw new BeanIOConfigurationException("Maximum occurrences must be greater than 0");
            if (segment.MaxOccurs < segment.MinOccurs)
                throw new BeanIOConfigurationException("Maximum occurrences cannot be less than minimum occurrences");

            if (segment.Key != null && segment.Collection == null)
                throw new BeanIOConfigurationException("Unexpected key value when collection not set");
            if (segment.Collection != null && segment.Type == null && segment.Target == null)
                throw new BeanIOConfigurationException("Class or value required if collection is set");

            if (Parent != null && segment.ValidateOnMarshal == null)
                segment.ValidateOnMarshal = Parent.ValidateOnMarshal;

            if (PropertyRoot == null || PropertyRoot != segment)
            {
                segment.IsBound = true;
                if ((segment.MaxOccurs == null || segment.MaxOccurs > 1) && segment.Collection == null)
                {
                    if (segment.Type != null || segment.Target != null)
                        throw new BeanIOConfigurationException("Collection required when maxOccurs is greater than 1 and class or value is set");
                }

                if (segment.ComponentType == ComponentType.Record
                    && segment.IsRepeating
                    && segment.Type == null
                    && segment.Target == null)
                {
                    segment.IsBound = false;
                }
            }
            else if (segment.Collection != null)
            {
                throw new BeanIOConfigurationException("Collection cannot be set on unbound record or segment.");
            }
        }

        /// <summary>
        /// Finalizes a segment configuration after its children have been processed
        /// </summary>
        /// <param name="segment">the segment configuration to finalize</param>
        protected override void FinalizeSegment(SegmentConfig segment)
        {
            if (segment.PropertyList.Any(x => x.IsIdentifier))
                segment.IsIdentifier = true;
        }

        /// <summary>
        /// Processes a field configuration
        /// </summary>
        /// <param name="field">the field configuration to process</param>
        protected override void HandleField(FieldConfig field)
        {
            // ignore fields that belong to ignored records
            if (_recordIgnored)
                field.IsBound = false;

            if (field.Name == null)
                throw new BeanIOConfigurationException("name is required");
            if (field.Label == null)
                field.Label = field.Name;

            // set and validate occurrences
            if (field.MinOccurs == null)
            {
                field.MinOccurs =
                    field.OccursRef != null
                        ? 0
                        : _settings.GetInt($"{Settings.DEFAULT_FIELD_MIN_OCCURS}.{_stream.Format}", 0);
            }

            if (field.MaxOccurs == null)
                field.MaxOccurs = field.OccursRef != null ? int.MaxValue : Math.Max(field.MinOccurs.Value, 1);
            if (field.MaxOccurs != null)
            {
                if (field.MaxOccurs <= 0)
                    throw new BeanIOConfigurationException("Maximum occurrences must be greater than 0");
                if (field.MaxOccurs < field.MinOccurs)
                    throw new BeanIOConfigurationException("Maximum occurrences cannot be less than minimum occurrences");
            }

            // set and validate min and max length
            if (field.MinLength == null)
                field.MinLength = 0;
            if (field.MaxLength == null)
                field.MaxLength = int.MaxValue;
            if (field.MaxLength < field.MinLength)
                throw new BeanIOConfigurationException("maxLength must be greater than or equal to minLength");
            if (field.Literal != null)
            {
                var literalLength = field.Literal.Length;
                if (literalLength < field.MinLength)
                    throw new BeanIOConfigurationException("literal text length is less than minLength");
                if (field.MaxLength != null && literalLength > field.MaxLength)
                    throw new BeanIOConfigurationException("literal text length is greater than maxLength");
            }

            if (field.IsRepeating && field.IsIdentifier)
                throw new BeanIOConfigurationException("repeating fields cannot be used as identifiers");

            if (field.IsBound && field.IsRepeating && field.Collection == null)
                throw new BeanIOConfigurationException("collection not set");

            if (Parent != null && field.ValidateOnMarshal == null)
                field.ValidateOnMarshal = Parent.ValidateOnMarshal;

            if (field.IsIdentifier)
                ValidateRecordIdentifyingCriteria(field);
        }

        /// <summary>
        /// Processes a constant configuration
        /// </summary>
        /// <param name="constant">the constant configuration to process</param>
        protected override void HandleConstant(ConstantConfig constant)
        {
            constant.IsBound = true;

            if (constant.Name == null)
                throw new BeanIOConfigurationException("Missing property name");
        }

        /// <summary>
        /// This method validates a record identifying field has a literal or regular expression
        /// configured for identifying a record.
        /// </summary>
        /// <param name="field">the record identifying field configuration to validate</param>
        protected virtual void ValidateRecordIdentifyingCriteria(FieldConfig field)
        {
            // validate regex or literal is configured for record identifying fields
            if (field.Literal == null && field.RegEx == null)
                throw new BeanIOConfigurationException("Literal or regex pattern required for identifying fields");
        }

        private class ComponentConfigComparer : IComparer<ComponentConfig>
        {
            public int Compare(ComponentConfig x, ComponentConfig y)
            {
                var p1 = GetPosition(x);
                var p2 = GetPosition(y);
                if (p1 == null)
                    return p2 == null ? 0 : 1;
                if (p2 == null)
                    return -1;
                return p1.Value.CompareTo(p2.Value);
            }

            private int? GetPosition(ComponentConfig config)
            {
                int? result = null;
                switch (config.ComponentType)
                {
                    case ComponentType.Field:
                    case ComponentType.Segment:
                        result = ((PropertyConfig)config).Position;
                        break;
                    case ComponentType.Record:
                        result = ((RecordConfig)config).Order;
                        break;
                    case ComponentType.Group:
                        result = ((GroupConfig)config).Order;
                        break;
                }

                if (result != null && result < 0)
                    result = int.MaxValue + result;
                return result;
            }
        }
    }
}
