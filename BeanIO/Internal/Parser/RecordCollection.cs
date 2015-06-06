using System.Collections;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// A <see cref="IParser"/> tree component for parsing a collection of bean objects, where
    /// a bean object is mapped to a <see cref="Record"/> or <see cref="Group"/>.
    /// </summary>
    /// <remarks>
    /// A <see cref="RecordCollection"/> supports a single <see cref="Record"/> or <see cref="Group"/> child.
    /// </remarks>
    public class RecordCollection : RecordAggregation
    {
        /// <summary>
        /// Gets the <see cref="IProperty"/> implementation type
        /// </summary>
        public override PropertyType Type
        {
            get { return Internal.Parser.PropertyType.AggregationCollection; }
        }

        /// <summary>
        /// Unmarshals a record
        /// </summary>
        /// <param name="context">The <see cref="UnmarshallingContext"/></param>
        /// <returns>true if this component was present in the unmarshalled record, or false otherwise</returns>
        public override bool Unmarshal(UnmarshallingContext context)
        {
            // allow the delegate to unmarshal itself
            var result = base.Unmarshal(context);

            var aggregatedValue = Selector.GetValue(context);
            if (!ReferenceEquals(aggregatedValue, Value.Invalid))
            {
                if (!IsLazy || StringUtil.HasValue(aggregatedValue))
                {
                    var aggregation = _value.Get(context);
                    if (aggregation == null || ReferenceEquals(aggregation, Value.Missing))
                    {
                        aggregation = CreateAggregationType();
                        _value.Set(context, aggregation);
                    }

                    var collection = (IList)aggregation;
                    collection.Add(aggregatedValue);
                }
            }

            Parser.ClearValue(context);

            return result;
        }

        /// <summary>
        /// Marshals a record
        /// </summary>
        /// <param name="context">The <see cref="MarshallingContext"/></param>
        /// <returns>whether a value was marshalled</returns>
        public override bool Marshal(MarshallingContext context)
        {
            var minOccurs = MinOccurs;

            var collection = GetCollection(context);
            if (collection == null && minOccurs == 0)
                return false;

            var parser = Parser;
            var maxOccurs = MaxOccurs;
            var index = 0;

            if (collection != null)
            {
                foreach (var value in collection)
                {
                    if (maxOccurs != null && index >= maxOccurs)
                        return true;
                    parser.SetValue(context, value);
                    parser.Marshal(context);
                    ++index;
                }
            }

            if (index < MinOccurs)
            {
                parser.SetValue(context, null);
                while (index < minOccurs)
                {
                    parser.Marshal(context);
                    ++index;
                }
            }

            return true;
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
            if (value != null && ((ICollection)value).Count == 0)
                value = null;
            base.SetValue(context, value);
        }

        /// <summary>
        /// Returns the collection value being parsed
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <returns>the <see cref="IList"/></returns>
        protected virtual ICollection GetCollection(ParsingContext context)
        {
            return (ICollection)GetValue(context);
        }

        /// <summary>
        /// Returns whether this parser or any of its descendant have content for marshalling.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <returns>true if there is content for marshalling, false otherwise</returns>
        public override bool HasContent(ParsingContext context)
        {
            var collection = GetCollection(context);
            return collection != null && collection.Count != 0;
        }
    }
}
