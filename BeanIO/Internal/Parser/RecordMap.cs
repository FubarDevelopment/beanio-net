using System.Collections;
using System.Text;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser
{
    internal class RecordMap : RecordAggregation
    {
        /// <summary>
        /// Gets the <see cref="IProperty"/> implementation type
        /// </summary>
        public override PropertyType Type
        {
            get { return Internal.Parser.PropertyType.AggregationMap; }
        }

        /// <summary>
        /// Gets or sets the child property used for the key
        /// </summary>
        public IProperty Key { get; set; }

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
                var keyValue = Key.GetValue(context);

                if (!IsLazy || StringUtil.HasValue(keyValue) || StringUtil.HasValue(aggregatedValue))
                {
                    var aggregation = _value.Get(context);
                    if (aggregation == null || ReferenceEquals(aggregation, Value.Missing))
                    {
                        aggregation = CreateAggregationType();
                        _value.Set(context, aggregation);
                    }

                    var map = (IDictionary)aggregation;
                    map[keyValue] = aggregatedValue;
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

            var map = GetMap(context);
            if (map == null && minOccurs == 0)
                return false;

            var parser = Parser;
            var maxOccurs = MaxOccurs;
            var index = 0;

            if (map != null)
            {
                foreach (var mapKey in map.Keys)
                {
                    if (maxOccurs != null && index >= maxOccurs)
                        return true;

                    var mapValue = map[mapKey];
                    Key.SetValue(context, mapKey);
                    parser.SetValue(context, mapValue);
                    parser.Marshal(context);
                    ++index;
                }
            }

            if (index < minOccurs)
            {
                Key.SetValue(context, null);
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
            if (value != null && ((IDictionary)value).Count == 0)
                value = null;
            base.SetValue(context, value);
        }

        /// <summary>
        /// Returns whether this parser or any of its descendant have content for marshalling.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <returns>true if there is content for marshalling, false otherwise</returns>
        public override bool HasContent(ParsingContext context)
        {
            var map = GetMap(context);
            return map != null && map.Count != 0;
        }

        public override bool Defines(object value)
        {
            return value != null && value.GetType().IsMap();
        }

        /// <summary>
        /// Called by <see cref="TreeNode{T}.ToString"/> to append node parameters to the output
        /// </summary>
        /// <param name="s">The output to append</param>
        protected override void ToParamString(StringBuilder s)
        {
            base.ToParamString(s);
            if (Key != null)
                s.AppendFormat(", key=${0}", Key.Name);
        }

        protected virtual IDictionary GetMap(ParsingContext context)
        {
            return (IDictionary)GetValue(context);
        }
    }
}
