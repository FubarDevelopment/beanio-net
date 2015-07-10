using System;
using System.IO;

using JetBrains.Annotations;

using Newtonsoft.Json.Linq;

using NJ = Newtonsoft.Json;

namespace BeanIO.Stream.Json
{
    /// <summary>
    /// A <see cref="IRecordWriter"/> implementation for writing JSON formatted record.
    /// </summary>
    public class JsonWriter : IRecordWriter
    {
        private readonly NJ.JsonTextWriter _writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonWriter"/> class.
        /// </summary>
        /// <param name="writer">the <see cref="TextWriter"/> to write to</param>
        public JsonWriter(TextWriter writer)
            : this(writer, new JsonParserConfiguration())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonWriter"/> class.
        /// </summary>
        /// <param name="writer">the <see cref="TextWriter"/> to write to</param>
        /// <param name="config">the configuration</param>
        public JsonWriter(TextWriter writer, [CanBeNull] JsonParserConfiguration config)
        {
            var cfg = config ?? new JsonParserConfiguration();
            LineSeparator = writer.NewLine = cfg.LineSeparator ?? Environment.NewLine;
            _writer = new NJ.JsonTextWriter(writer)
                {
                    CloseOutput = true,
                    IndentChar = ' ',
                    Indentation = cfg.Indentation,
                    Formatting = cfg.Pretty ? NJ.Formatting.Indented : NJ.Formatting.None,
                };
        }

        /// <summary>
        /// Gets the line separator used while indenting the JSON data.
        /// </summary>
        protected string LineSeparator { get; private set; }

        /// <summary>
        /// Gets the <see cref="NJ.JsonTextWriter"/>
        /// </summary>
        protected NJ.JsonTextWriter Writer
        {
            get { return _writer; }
        }

        /// <summary>
        /// Writes a record object to this output stream.
        /// </summary>
        /// <param name="record">Record the record object to write</param>
        public void Write(object record)
        {
            var obj = (JObject)record;
            obj.WriteTo(_writer);
            _writer.WriteWhitespace(LineSeparator);
        }

        /// <summary>
        /// Flushes the output stream.
        /// </summary>
        public void Flush()
        {
            _writer.Flush();
        }

        /// <summary>
        /// Closes the output stream.
        /// </summary>
        public void Close()
        {
            _writer.Close();
        }
    }
}
