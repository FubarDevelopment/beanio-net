﻿using System;
using System.IO;

using JetBrains.Annotations;

using Newtonsoft.Json.Linq;

using NJ = Newtonsoft.Json;

namespace BeanIO.Stream.Json
{
    public class JsonRecordMarshaller : IRecordMarshaller
    {
        private readonly JsonParserConfiguration _config;

        private StringWriter _out;

        private NJ.JsonTextWriter _writer;

        public JsonRecordMarshaller()
            : this(new JsonParserConfiguration())
        {
        }

        public JsonRecordMarshaller([CanBeNull] JsonParserConfiguration config)
        {
            _config = config ?? new JsonParserConfiguration();
            Init();
        }

        /// <summary>
        /// Marshals a single record object to a <code>String</code>.
        /// </summary>
        /// <param name="record">Record the record object to marshal</param>
        /// <returns>The marshalled record text</returns>
        public string Marshal(object record)
        {
            try
            {
                var obj = (JObject)record;
                if (obj == null)
                    return null;
                obj.WriteTo(_writer);
                _writer.Close();
                return _out.ToString();
            }
            finally
            {
                Init();
            }
        }

        private void Init()
        {
            _out = new StringWriter()
                {
                    NewLine = _config.LineSeparator ?? Environment.NewLine,
                };
            _writer = new NJ.JsonTextWriter(_out)
            {
                CloseOutput = false,
                IndentChar = ' ',
                Indentation = _config.Indentation,
                Formatting = _config.Pretty ? NJ.Formatting.Indented : NJ.Formatting.None,
            };
        }
    }
}