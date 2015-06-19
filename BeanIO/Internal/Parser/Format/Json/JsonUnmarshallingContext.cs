using System;

using BeanIO.Stream;

using Newtonsoft.Json.Linq;

namespace BeanIO.Internal.Parser.Format.Json
{
    /// <summary>
    /// An <see cref="UnmarshallingContext"/> for JSON formatted streams.
    /// </summary>
    internal class JsonUnmarshallingContext : UnmarshallingContext
    {
        /// <summary>
        /// the current position in the record
        /// </summary>
        private readonly object[] _valueStack;

        private readonly JTokenType[] _typeStack;

        private int _depth;

        public JsonUnmarshallingContext(int maxDepth)
        {
            _valueStack = new object[maxDepth];
            _typeStack = new JTokenType[maxDepth];
        }

        private JObject JsonObject
        {
            get { return (JObject)_valueStack[_depth]; }
        }

        private JArray JsonArray
        {
            get { return (JArray)_valueStack[_depth]; }
        }

        /// <summary>
        /// Sets the value of the record returned from the <see cref="IRecordReader"/>
        /// </summary>
        /// <param name="value">
        /// the record value read by a <see cref="IRecordReader"/>
        /// </param>
        public override void SetRecordValue(object value)
        {
            _depth = 0;
            _valueStack[0] = value;
            _typeStack[0] = ((JToken)value).Type;
        }

        public object GetValue(IJsonNode node)
        {
            JToken value;
            switch (_typeStack[_depth])
            {
                case JTokenType.Object:
                    if (JsonObject.TryGetValue(node.JsonName, out value))
                    {
                        if (value == null)
                            return Value.Nil;
                    }
                    break;
                case JTokenType.Array:
                    {
                        var index = node.JsonArrayIndex;
                        if (index < 0)
                            index = GetRelativeFieldIndex();
                        var parent = JsonArray;
                        if (index >= parent.Count)
                        {
                            value = null;
                        }
                        else
                        {
                            value = parent[index];
                            if (value == null)
                                return Value.Nil;
                        }
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }
            return value;
        }

        public object Push(IJsonNode node, bool validate)
        {
            var value = GetValue(node);
            if (ReferenceEquals(value, Value.Nil))
            {
                if (validate && !node.IsNillable)
                {
                    AddFieldError(node.Name, null, "nillable");
                    return Value.Invalid;
                }
                return value;
            }

            if (value == null)
                return null;

            // validate type
            switch (node.JsonType)
            {
                case JTokenType.Array:
                    if (!(value is JArray))
                    {
                        if (validate)
                        {
                            AddFieldError(node.Name, null, "jsontype", JTokenType.Array);
                            return Value.Invalid;
                        }
                        return null;
                    }
                    break;
                case JTokenType.Object:
                    if (!(value is JObject))
                    {
                        if (validate)
                        {
                            AddFieldError(node.Name, null, "jsontype", JTokenType.Object);
                            return Value.Invalid;
                        }
                        return null;
                    }
                    break;
                default:
                    throw new InvalidOperationException(string.Format("Invalid json type: {0}", node.JsonType));
            }

            ++_depth;
            _valueStack[_depth] = value;
            _typeStack[_depth] = node.JsonType;

            return value;
        }

        public object Pop()
        {
            return _valueStack[_depth--];
        }
    }
}
