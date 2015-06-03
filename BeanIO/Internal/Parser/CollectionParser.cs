using System;
using System.Collections;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser
{
    public class CollectionParser : Aggregation
    {
        /// <summary>
        /// the property value
        /// </summary>
        private readonly ParserLocal<object> _value = new ParserLocal<object>();

        /// <summary>
        /// Gets a value indicating whether this aggregation is a property of its parent bean object.
        /// </summary>
        public override bool IsProperty
        {
            get { return PropertyType != null; }
        }

        /// <summary>
        /// Gets the <see cref="IProperty"/> implementation type
        /// </summary>
        public override PropertyType Type
        {
            get { return Internal.Parser.PropertyType.AggregationCollection; }
        }

        /// <summary>
        /// Gets the size of the components that make up a single iteration.
        /// </summary>
        public override int IterationSize
        {
            get { return Size.GetValueOrDefault(); }
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
        /// Returns the length of aggregation
        /// </summary>
        /// <param name="value">the aggregation value</param>
        /// <returns></returns>
        public override int Length(object value)
        {
            var collection = (ICollection)value;
            return collection != null ? collection.Count : 0;
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
                value = CreateCollection();
                _value.Set(context, value);
            }

            return GetValue(context);
        }

        public override bool Defines(object value)
        {
            // TODO: implement for arrays....

            if (value == null || PropertyType == null)
                return false;

            if (value is ICollection)
            {
                // children of collections cannot be used to identify bean objects
                // so we can immediately return true here
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clears the current property value.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        public override void ClearValue(ParsingContext context)
        {
            _value.Set(context, null);
        }

        protected override bool Marshal(MarshallingContext context, IParser parser, int minOccurs, int? maxOccurs)
        {
            context.PushIteration(this);
            try
            {
                var collection = GetCollection(context);
                if (collection == null && minOccurs == 0)
                    return false;

                var i = 0;
                if (collection != null)
                {
                    foreach (var value in collection)
                    {
                        if (maxOccurs != null && i >= maxOccurs)
                            return true;

                        SetIterationIndex(context, i);
                        parser.SetValue(context, value);
                        parser.Marshal(context);
                        i += 1;
                    }
                }

                if (i < minOccurs)
                {
                    parser.SetValue(context, null);
                    while (i < minOccurs)
                    {
                        SetIterationIndex(context, i);
                        parser.Marshal(context);
                        i += 1;
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
            var collection = IsLazy ? null : CreateCollection();

            var invalid = false;
            var count = 0;
            try
            {
                context.PushIteration(this);

                for (int i = 0; (maxOccurs == null || i != maxOccurs); ++i)
                {
                    SetIterationIndex(context, i);

                    // unmarshal the field
                    var found = parser.Unmarshal(context);
                    if (!found)
                    {
                        parser.ClearValue(context);
                        break;
                    }

                    // collect the field value and add it to our buffered list
                    object fieldValue = parser.GetValue(context);
                    if (ReferenceEquals(fieldValue, Value.Invalid))
                    {
                        invalid = true;
                    }
                    else if (!ReferenceEquals(fieldValue, Value.Missing))
                    {
                        // the field value may still be missing if 'optional' is true on a child segment
                        if (!IsLazy || StringUtil.HasValue(fieldValue))
                        {
                            if (collection == null)
                                collection = CreateCollection();
                            collection.Add(fieldValue);
                        }
                    }

                    parser.ClearValue(context);
                    ++count;
                }
            }
            finally
            {
                context.PopIteration();
            }

            object value;

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
                value = collection;
            }

            _value.Set(context, value);

            return ReferenceEquals(value, Value.Invalid) || count > 0;
        }

        protected IList GetCollection(ParsingContext context)
        {
            var value = _value.Get(context);
            if (ReferenceEquals(value, Value.Invalid))
                return null;
            return (IList)value;
        }

        protected IList CreateCollection()
        {
            return (IList)ObjectUtils.NewInstance(PropertyType);
        }
    }
}
