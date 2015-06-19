using System;

using BeanIO.Stream;

using Newtonsoft.Json.Linq;

namespace BeanIO.Internal.Parser.Format.Json
{
    /// <summary>
    /// A <see cref="MarshallingContext"/> for JSON formatted streams.
    /// </summary>
    internal class JsonMarshallingContext : MarshallingContext
    {
        private readonly object[] _valueStack;

        private readonly JTokenType[] _typeStack;

        private int _depth = -1;

        public JsonMarshallingContext(int maxDepth)
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

        private JTokenType JsonType
        {
            get { return _typeStack[_depth]; }
        }

        /// <summary>
        /// Clear is invoked after each bean object (record or group) is marshalled
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            _depth = -1;
        }

        public void Push(IJsonNode type)
        {
            JToken value;
            if (type.JsonType == JTokenType.Object)
            {
                value = new JObject();
            }
            else
            {
                // array
                value = new JArray();
            }

            Put(type, value);

            _depth++;
            _valueStack[_depth] = value;
            _typeStack[_depth] = type.JsonType;
        }

        public void Pop()
        {
            --_depth;
        }

        public void Put(IJsonNode type, JToken value)
        {
            if (_depth < 0)
            {
                _depth = 0;
                _valueStack[_depth] = new JObject();
                _typeStack[_depth] = JTokenType.Object;
            }

            if (type.IsJsonArray)
            {
                JArray list;
                if (JsonType == JTokenType.Array)
                {
                    var index = type.JsonArrayIndex;
                    if (index < JsonArray.Count)
                    {
                        list = (JArray)JsonArray[index];
                    }
                    else
                    {
                        if (index > JsonArray.Count)
                            throw new InvalidOperationException();
                        list = new JArray();
                        JsonArray.Add(list);
                    }
                }
                else
                {
                    // object
                    JToken token;
                    if (!JsonObject.TryGetValue(type.JsonName, out token))
                    {
                        list = new JArray();
                        JsonObject[type.JsonName] = list;
                    }
                    else
                    {
                        list = (JArray)token;
                    }
                }
                list.Add(value);
            }
            else
            {
                switch (JsonType)
                {
                    case JTokenType.Array:
                        JsonArray.Add(value);
                        break;
                    case JTokenType.Object:
                        JsonObject[type.JsonType] = value;
                        break;
                }
            }
        }

        /// <summary>
        /// Creates the record object to pass to the <see cref="IRecordWriter"/>
        /// when <see cref="MarshallingContext.WriteRecord"/> is called.
        /// </summary>
        /// <returns>
        /// The newly created record object.
        /// </returns>
        protected override object ToRecordObject()
        {
            return _depth < 0 ? null : _valueStack[0];
        }
    }
}
