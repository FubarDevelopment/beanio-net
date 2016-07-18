// <copyright file="MapParser.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections;
using System.Collections.Generic;
using System.Text;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// A <see cref="IParser"/> component for aggregating inline <see cref="IDictionary"/> objects.
    /// </summary>
    /// <example>
    /// <code>key1,field1,key2,field2</code>
    /// </example>
    internal class MapParser : Aggregation
    {
        /// <summary>
        /// the property value
        /// </summary>
        private readonly ParserLocal<object> _value = new ParserLocal<object>();

        /// <summary>
        /// Gets or sets the key property
        /// </summary>
        public IProperty KeyProperty { get; set; }

        /// <summary>
        /// Gets or sets the value property
        /// </summary>
        public IProperty ValueProperty { get; set; }

        /// <summary>
        /// Gets a value indicating whether this aggregation is a property of its parent bean object.
        /// </summary>
        public override bool IsProperty => PropertyType != null;

        /// <summary>
        /// Gets the <see cref="IProperty"/> implementation type
        /// </summary>
        public override PropertyType Type => Internal.Parser.PropertyType.AggregationMap;

        /// <summary>
        /// Gets the size of the components that make up a single iteration.
        /// </summary>
        public override int IterationSize => Size ?? 0;

        /// <summary>
        /// Returns the length of aggregation
        /// </summary>
        /// <param name="value">the aggregation value</param>
        /// <returns>the length</returns>
        public override int Length(object value)
        {
            var map = (IDictionary)value;
            return map?.Count ?? 0;
        }

        /// <summary>
        /// Creates the property value and returns it
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <returns>the property value</returns>
        public override object CreateValue(ParsingContext context)
        {
            var value = _value.Get(context);
            if (value == null)
            {
                value = CreateMap();
                _value.Set(context, value);
            }

            return GetValue(context);
        }

        /// <summary>
        /// Returns the unmarshalled property value.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <returns>the property value</returns>
        public override object GetValue(ParsingContext context)
        {
            var value = _value.Get(context);
            return value ?? Value.Missing;
        }

        /// <summary>
        /// Clears the current property value.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        public override void ClearValue(ParsingContext context)
        {
            _value.Set(context, null);
        }

        /// <summary>
        /// Sets the property value for marshaling.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <param name="value">the property value</param>
        public override void SetValue(ParsingContext context, object value)
        {
            // convert empty collections to null so that parent parsers
            // will consider this property missing during marshalling
            if (value != null && ((IDictionary)value).Count == 0)
                value = null;

            _value.Set(context, value);

            base.SetValue(context, value);
        }

        public override bool Defines(object value)
        {
            if (value == null || PropertyType == null)
                return false;

            if (value.GetType().IsMap())
            {
                // children of collections cannot be used to identify bean objects
                // so we can immediately return true here
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns whether this parser and its children match a record being unmarshalled.
        /// </summary>
        /// <param name="context">The <see cref="UnmarshallingContext"/></param>
        /// <returns>true if matched, false otherwise</returns>
        public override bool Matches(UnmarshallingContext context)
        {
            // matching repeating fields is not supported
            return true;
        }

        /// <summary>
        /// Called by a stream to register variables stored in the parsing context.
        /// </summary>
        /// <remarks>
        /// This method should be overridden by subclasses that need to register
        /// one or more parser context variables.
        /// </remarks>
        /// <param name="locals">set of local variables</param>
        public override void RegisterLocals(ISet<IParserLocal> locals)
        {
            ((Component)KeyProperty)?.RegisterLocals(locals);
            if (locals.Add(_value))
                base.RegisterLocals(locals);
        }

        /// <summary>
        /// Returns whether this parser or any of its descendant have content for marshalling.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <returns>true if there is content for marshalling, false otherwise</returns>
        public override bool HasContent(ParsingContext context)
        {
            var map = GetMap(context);
            return map != null && map.Count > 0;
        }

        protected override bool Marshal(MarshallingContext context, IParser parser, int minOccurs, int? maxOccurs)
        {
            context.PushIteration(this);
            try
            {
                var map = GetMap(context);
                if (map == null && minOccurs == 0)
                    return false;

                var i = 0;

                if (map != null)
                {
                    foreach (var mapKey in map.Keys)
                    {
                        if (maxOccurs != null && i >= maxOccurs)
                            return true;
                        var mapValue = map[mapKey];
                        SetIterationIndex(context, i);
                        KeyProperty.SetValue(context, mapKey);
                        parser.SetValue(context, mapValue);
                        parser.Marshal(context);
                        ++i;
                    }
                }

                if (i < minOccurs)
                {
                    KeyProperty.SetValue(context, null);
                    parser.SetValue(context, null);
                    while (i < minOccurs)
                    {
                        SetIterationIndex(context, i);
                        parser.Marshal(context);
                        ++i;
                    }
                }

                return true;
            }
            finally
            {
                context.PopIteration();
            }
        }

        protected override bool Unmarshal(UnmarshallingContext context, IParser parser, int minOccurs, int? maxOccurs)
        {
            var map = IsLazy ? null : CreateMap();

            var invalid = false;
            var count = 0;
            try
            {
                context.PushIteration(this);
                var index = 0;
                while (maxOccurs == null || index < maxOccurs)
                {
                    SetIterationIndex(context, index);

                    // unmarshal the field
                    var found = parser.Unmarshal(context);
                    if (!found)
                    {
                        parser.ClearValue(context);
                        break;
                    }

                    // collect the field value and add it to our buffered list
                    var fieldValue = parser.GetValue(context);
                    if (ReferenceEquals(fieldValue, Value.Invalid))
                    {
                        invalid = true;
                    }
                    else if (!ReferenceEquals(fieldValue, Value.Missing))
                    {
                        var mapKey = KeyProperty.GetValue(context);
                        if (!IsLazy || StringUtil.HasValue(mapKey) || StringUtil.HasValue(fieldValue))
                        {
                            if (map == null)
                                map = CreateMap();
                            map[mapKey] = fieldValue;
                        }
                    }

                    parser.ClearValue(context);
                    ++count;

                    index += 1;
                }
            }
            finally
            {
                context.PopIteration();
            }

            object value;

            // validate minimum occurrences have been met
            if (count < minOccurs)
            {
                context.AddFieldError(Name, null, "minOccurs", minOccurs, maxOccurs);
                value = Value.Invalid;
            }
            else if (invalid)
            {
                value = Value.Invalid;
            }
            else
            {
                value = map;
            }

            _value.Set(context, value);

            return count > 0;
        }

        protected virtual IDictionary GetMap(ParsingContext context)
        {
            var value = _value.Get(context);
            if (ReferenceEquals(value, Value.Invalid))
                return null;
            return (IDictionary)value;
        }

        protected virtual IDictionary CreateMap()
        {
            return (IDictionary)PropertyType.NewInstance();
        }

        /// <summary>
        /// Called by <see cref="TreeNode{T}.ToString"/> to append node parameters to the output
        /// </summary>
        /// <param name="s">The output to append</param>
        protected override void ToParamString(StringBuilder s)
        {
            base.ToParamString(s);
            if (KeyProperty != null)
                s.AppendFormat(", key=${0}", KeyProperty.Name);
            if (PropertyType != null)
                s.AppendFormat(", type={0}", PropertyType.GetAssemblyQualifiedName());
        }
    }
}
