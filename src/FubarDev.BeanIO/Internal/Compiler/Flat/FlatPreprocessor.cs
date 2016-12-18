// <copyright file="FlatPreprocessor.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using BeanIO.Config;
using BeanIO.Internal.Config;
using BeanIO.Internal.Util;

namespace BeanIO.Internal.Compiler.Flat
{
    internal class FlatPreprocessor : Preprocessor
    {
        /// <summary>
        /// the list of components at the end of the record following the unbounded component
        /// </summary>
        private readonly List<PropertyConfig> _endComponents = new List<PropertyConfig>();

        /// <summary>
        /// stack of non-record segments
        /// </summary>
        private readonly Stack<SegmentConfig> _segmentStack = new Stack<SegmentConfig>();

        /// <summary>
        /// list of field components belonging to a record, used for validating dynamic occurrences
        /// </summary>
        private readonly List<FieldConfig> _fieldComponents = new List<FieldConfig>();

        /// <summary>
        /// the current default field position
        /// </summary>
        private int? _defaultPosition;

        /// <summary>
        /// position must be set for all fields or for no fields, this attribute
        /// is set when the first field is processed and all other fields must adhere to it
        /// </summary>
        private bool? _positionRequired;

        /// <summary>
        /// the field or segment that requires a maximum position set on it to test for more occurrences
        /// of an indefinitely repeating field or segment
        /// </summary>
        private PropertyConfig _unboundedComponent;

        /// <summary>
        /// the component that immediately follows the unbounded component
        /// </summary>
        private PropertyConfig _unboundedComponentFollower;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlatPreprocessor"/> class.
        /// </summary>
        /// <param name="settings">The configuration settings</param>
        /// <param name="stream">The stream configuration</param>
        public FlatPreprocessor(ISettings settings, StreamConfig stream)
            : base(settings, stream)
        {
        }

        /// <summary>
        /// Gets a value indicating whether the stream format is fixed length.
        /// </summary>
        protected virtual bool IsFixedLength => false;

        private int SegmentOffset
        {
            get { return _segmentStack.Where(x => x.Position != null).Sum(x => x.Position) ?? 0; }
        }

        /// <summary>
        /// Initializes a record configuration before its children have been processed
        /// </summary>
        /// <param name="record">the record configuration to process</param>
        protected override void InitializeRecord(RecordConfig record)
        {
            base.InitializeRecord(record);

            _defaultPosition = 0;
            _positionRequired = null;
            _unboundedComponent = null;
            _unboundedComponentFollower = null;
            _endComponents.Clear();
            _fieldComponents.Clear();
        }

        /// <summary>
        /// Finalizes a record configuration after its children have been processed
        /// </summary>
        /// <param name="record">the record configuration to finalize</param>
        protected override void FinalizeRecord(RecordConfig record)
        {
            base.FinalizeRecord(record);

            var minSet = record.MinLength != null;
            if (Stream.IsStrict)
            {
                if (record.MinLength == null)
                    record.MinLength = record.MinSize;
                if (record.MaxLength == null)
                    record.MaxLength = record.MaxSize;
            }
            else
            {
                if (record.MinLength == null)
                    record.MinLength = 0;
                if (record.MaxLength == null)
                    record.MaxLength = int.MaxValue;
            }

            // validate maximum record length is not less than the minimum record length
            if (record.MaxLength != null && record.MaxLength < record.MinLength)
            {
                if (minSet)
                    throw new BeanIOConfigurationException("Maximum record length cannot be less than minimum record length");
                throw new BeanIOConfigurationException($"Maximum record length must be at least {record.MinLength}");
            }

            // if there is an unbounded component in the middle of the record, we need to
            // set the end position on it
            if (_unboundedComponent != null && _unboundedComponentFollower != null)
            {
                SetEndPosition(_unboundedComponent, _unboundedComponentFollower.Position);
            }
        }

        /// <summary>
        /// Initializes a segment configuration before its children have been processed
        /// </summary>
        /// <param name="segment">the segment configuration to process</param>
        protected override void InitializeSegment(SegmentConfig segment)
        {
            if (segment.ComponentType == ComponentType.Segment)
                _segmentStack.Push(segment);

            base.InitializeSegment(segment);

            if (segment.OccursRef != null)
            {
                if (!segment.IsCollection)
                    throw new BeanIOConfigurationException("Collection required when 'occursRef' is set");
                segment.MinOccursRef = segment.MinOccurs;
                segment.MaxOccursRef = segment.MaxOccurs;
                segment.MinOccurs = 1;
                segment.MaxOccurs = 1;
            }
        }

        /// <summary>
        /// Finalizes a segment configuration after its children have been processed
        /// </summary>
        /// <param name="segment">the segment configuration to finalize</param>
        [SuppressMessage("StyleCopPlus.StyleCopPlusRules", "SP2101:MethodMustNotContainMoreLinesThan", Justification = "Reviewed. Suppression is OK here.")]
        protected override void FinalizeSegment(SegmentConfig segment)
        {
            base.FinalizeSegment(segment);

            PropertyConfig first = null;
            PropertyConfig last = null;
            int position = 0;
            int minSize = 0;
            int maxSize = -1;

            // by default, a segment is not constant
            segment.IsConstant = false;

            var isRecord = segment.ComponentType == ComponentType.Record;
            var isVariableSized =
                segment.MaxOccurs == null || segment.MaxOccurs == int.MaxValue ||
                (segment.IsRepeating && segment.MinOccurs != segment.MaxOccurs);

            if (isVariableSized && !isRecord && (_defaultPosition == null || _defaultPosition == int.MaxValue))
            {
                throw new BeanIOConfigurationException("A segment of indeterminate size may not follow another component of indeterminate size");
            }

            // calculate the maximum size and position of the segment
            foreach (var config in segment.PropertyList)
            {
                if (config.ComponentType == ComponentType.Constant)
                    continue;
                if (config.ComponentType == ComponentType.Segment && ((SegmentConfig)config).IsConstant)
                    continue;

                if (!isRecord && segment.IsRepeating && config.MinOccurs.GetValueOrDefault() == 0)
                    throw new BeanIOConfigurationException("A repeating segment may not contain components where minOccurs=0");

                if (config.MaxSize == int.MaxValue)
                {
                    maxSize = int.MaxValue;
                }

                var n = config.Position;
                if (first == null || ComparePosition(n, first.Position) < 0)
                {
                    first = config;
                }

                if (last == null || ComparePosition(n, last.Position) > 0)
                {
                    last = config;
                }
            }

            if (last == null)
            {
                if (segment.ComponentType == ComponentType.Record)
                {
                    maxSize = int.MaxValue;
                }
                else
                {
                    segment.IsConstant = true;
                    maxSize = 0;
                }
            }
            else if (maxSize < 0)
            {
                position = first.Position.Value;
                if (last.Position.GetValueOrDefault() < 0 && first.Position.GetValueOrDefault() >= 0)
                {
                    if (!isRecord && segment.IsRepeating)
                    {
                        throw new BeanIOConfigurationException("A repeating segment may not contain components of indeterminate size");
                    }

                    maxSize = int.MaxValue;
                }
                else if (last.MaxOccurs == null || last.MaxOccurs == int.MaxValue)
                {
                    maxSize = int.MaxValue;
                }
                else
                {
                    maxSize = Math.Abs(last.Position.GetValueOrDefault() - first.Position.GetValueOrDefault())
                              + (last.MaxSize * last.MaxOccurs.GetValueOrDefault());
                }
            }

            // calculate the minimum size of the segment
            if (last != null)
            {
                first = null;
                last = null;

                foreach (var config in segment.PropertyList)
                {
                    if (config.ComponentType == ComponentType.Constant)
                        continue;

                    minSize += config.MinSize * config.MinOccurs.Value;

                    var n = config.Position;
                    if (first == null || ComparePosition(n, first.Position) < 0)
                    {
                        first = config;
                    }

                    if (config.MinOccurs.GetValueOrDefault() > 0)
                    {
                        if (last == null || ComparePosition(n, last.Position) > 0)
                        {
                            last = config;
                        }
                    }
                }

                if (last == null)
                {
                    last = first;
                }

                if (first.Position.GetValueOrDefault() >= 0 && last.Position.GetValueOrDefault() < 0)
                {
                    // go with counted min size
                }
                else
                {
                    minSize = Math.Abs(last.Position.GetValueOrDefault() - first.Position.GetValueOrDefault())
                              + (last.MaxSize * last.MinOccurs.GetValueOrDefault());
                }
            }

            segment.Position = position;
            segment.MaxSize = maxSize;
            segment.MinSize = minSize;

            // calculate the next position
            if (!isRecord && _positionRequired == false)
            {
                if (_defaultPosition == null || _defaultPosition == int.MaxValue)
                {
                    // if the unbound component is a descendant of this segment, it should
                    // not affect the next default position
                    if (!segment.IsDescendant(_unboundedComponent))
                    {
                        var offset = 0 - (maxSize * (segment.MaxOccurs - 1));
                        foreach (var c in _endComponents)
                        {
                            c.Position = c.Position + offset;
                        }

                        segment.Position = offset + segment.Position;
                        _endComponents.Add(segment);

                        if (_unboundedComponentFollower == null)
                        {
                            _unboundedComponentFollower = segment;
                        }
                    }
                }
                else if (
                    (segment.IsRepeating && segment.MinOccurs != segment.MaxOccurs) ||
                    segment.MaxOccurs.GetValueOrDefault(int.MaxValue) == int.MaxValue ||
                    segment.MaxSize == int.MaxValue)
                {
                    if (_unboundedComponent == null)
                    {
                        _unboundedComponent = segment;
                    }

                    _defaultPosition = int.MaxValue;
                }
                else
                {
                    _defaultPosition = segment.Position + (segment.MaxSize * segment.MaxOccurs);
                }
            }

            // determine the default existence of the segment
            var defaultExistence = true;
            foreach (PropertyConfig child in segment.PropertyList.Where(x => x.ComponentType != ComponentType.Constant))
            {
                if (child.ComponentType == ComponentType.Segment)
                {
                    if (((SegmentConfig)child).IsDefaultExistence)
                    {
                        continue;
                    }
                }

                defaultExistence = false;
            }

            segment.IsDefaultExistence = defaultExistence;

            if (segment.IsDefaultExistence && segment.MinOccurs != segment.MaxOccurs)
            {
                throw new BeanIOConfigurationException("Repeating segments without any child field component must have minOccurs=maxOccurs");
            }

            HandleOccursRef(segment);

            if (segment.ComponentType == ComponentType.Segment)
            {
                _segmentStack.Pop();
            }
        }

        /// <summary>
        /// Processes a field configuration
        /// </summary>
        /// <param name="field">the field configuration to process</param>
        protected override void HandleField(FieldConfig field)
        {
            base.HandleField(field);

            if (field.OccursRef != null)
            {
                if (!field.IsCollection)
                    throw new BeanIOConfigurationException("Collection required when 'occursRef' is set");
                field.MinOccursRef = field.MinOccurs;
                field.MaxOccursRef = field.MaxOccurs;
                field.MinOccurs = 1;
                field.MaxOccurs = 1;
            }

            // validate and configure padding
            if (IsFixedLength)
            {
                // if a literal is set and length is not
                if (field.Literal != null)
                {
                    if (field.Length == null)
                    {
                        field.Length = field.Literal.Length;
                    }
                    else if (field.Literal.Length > field.Length)
                    {
                        throw new BeanIOConfigurationException("literal size exceeds the field length");
                    }
                }
                else if (field.Length == null)
                {
                    throw new BeanIOConfigurationException("length required for fixed length fields");
                }
            }
            else
            {
                if (field.Length == -1)
                {
                    field.Length = null;
                }
            }

            // default the padding character to a single space
            if (field.Length != null)
            {
                if (field.Padding == null)
                {
                    field.Padding = ' ';
                }
            }

            // calculate the size of the field
            var size = GetSize(field);
            if (size == null || size == -1)
            {
                field.MinSize = 0;
                field.MaxSize = int.MaxValue;
            }
            else
            {
                field.MaxSize = size.Value;
                field.MinSize = size.Value;
            }

            // calculate the position of this field (size must be calculated first)
            if (_positionRequired == null)
            {
                _positionRequired = field.Position != null;
            }
            else if (_positionRequired.Value ^ (field.Position != null))
            {
                throw new BeanIOConfigurationException("position must be declared for all the fields " +
                    "in a record, or for none of them (in which case, all fields must be configured in the " +
                    "order they will appear in the stream)");
            }

            if (field.Position != null)
            {
                field.Position = field.Position + SegmentOffset;
            }

            if (field.Position == null)
            {
                CalculateDefaultPosition(field);
            }
            else if (field.Until != null)
            {
                if (!IsVariableSized(field))
                {
                    throw new BeanIOConfigurationException("until should not be specified for " +
                        "fields of determinate occurences and length");
                }

                if (field.Until >= 0)
                {
                    throw new BeanIOConfigurationException("until must be less than 0 (i.e. " +
                        "a position relative to the end of the record)");
                }
            }

            HandleOccursRef(field);

            _fieldComponents.Add(field);
        }

        /// <summary>
        /// Returns the size of a field.
        /// </summary>
        /// <remarks>null = unbounded</remarks>
        /// <param name="field">the field to get the size from</param>
        /// <returns>the field size</returns>
        protected virtual int? GetSize(FieldConfig field)
        {
            return IsFixedLength ? field.Length : 1;
        }

        /// <summary>
        /// compares two positions
        /// </summary>
        /// <param name="p1">position 1</param>
        /// <param name="p2">position 2</param>
        /// <returns>
        ///  1,  0 returns 1 (greater than)
        /// -1, -2 returns 1 (greater than)
        ///  5, -1 returns -1 (less than)
        /// </returns>
        private static int ComparePosition(int? p1, int? p2)
        {
            if (p1 > 0 && p2 < 0)
                return -1;

            if (p1 < 0 && p2 >= 0)
                return 1;

            return p1.GetValueOrDefault().CompareTo(p2.GetValueOrDefault());
        }

        private void SetEndPosition(ComponentConfig config, int? end)
        {
            switch (config.ComponentType)
            {
                case ComponentType.Segment:
                    foreach (var child in config.Children)
                        SetEndPosition(child, end);
                    break;
                case ComponentType.Field:
                    ((FieldConfig)config).Until = end;
                    break;
            }
        }

        private void HandleOccursRef(PropertyConfig config)
        {
            if (config.OccursRef == null)
                return;

            // search in reverse to find the most recent field in case multiple
            // fields share the same name (gc0080)
            FieldConfig occurs = _fieldComponents.FirstOrDefault(fc => string.Equals(fc.Name, config.OccursRef));
            if (occurs == null)
            {
                throw new BeanIOConfigurationException($"Referenced field '{config.OccursRef}' not found");
            }

            if (occurs.Collection != null)
            {
                throw new BeanIOConfigurationException($"Referenced field '{config.OccursRef}' may not repeat");
            }

            if (occurs.Position >= config.Position)
            {
                throw new BeanIOConfigurationException($"Referenced field '{config.OccursRef}' must precede this field");
            }

            // default occurs to an Integer if not set...
            if (occurs.Type == null && occurs.TypeHandler == null && occurs.TypeHandlerInstance == null)
            {
                occurs.Type = typeof(int).GetAssemblyQualifiedName();
            }

            if (occurs.IsRef && !occurs.IsBound)
            {
                throw new BeanIOConfigurationException($"Unbound field '{occurs.Name}' cannot be referenced more than once");
            }

            occurs.IsRef = true;
        }

        private bool IsVariableSized(FieldConfig config)
        {
            return config.MaxOccurs == null || config.MaxOccurs == int.MaxValue
                   || (IsFixedLength && config.MaxSize == int.MaxValue)
                   || (config.IsRepeating && config.MinOccurs != config.MaxOccurs);
        }

        /// <summary>
        /// Calculates and sets the default field position.
        /// </summary>
        /// <param name="config">the field configuration to calculate the position for</param>
        private void CalculateDefaultPosition(FieldConfig config)
        {
            var isVariableSized = IsVariableSized(config);

            if (_defaultPosition == null || _defaultPosition == int.MaxValue)
            {
                if (isVariableSized)
                {
                    string error = "Cannot determine field position, field is preceded by " +
                                   "another component with indeterminate occurrences";

                    if (IsFixedLength)
                    {
                        error += "or unbounded length";
                    }

                    throw new BeanIOConfigurationException(error);
                }

                int offset = (0 - (config.MaxSize * config.MaxOccurs)).GetValueOrDefault();
                foreach (PropertyConfig c in _endComponents)
                {
                    c.Position = c.Position + offset;
                }

                config.Position = offset;
                _endComponents.Add(config);

                if (_unboundedComponentFollower == null)
                {
                    _unboundedComponentFollower = config;
                }
            }
            else
            {
                config.Position = _defaultPosition;

                if (isVariableSized)
                {
                    _defaultPosition = null;
                    _unboundedComponent = config;
                }
                else
                {
                    _defaultPosition = config.Position + (config.MaxSize * config.MaxOccurs);
                }
            }
        }
    }
}
