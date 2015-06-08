using System;
using System.IO;

using Xunit;

namespace BeanIO.Parser
{
    /// <summary>
    /// Base class for test classes that test the parsing framework.
    /// </summary>
    public class ParserTest
    {
        private static readonly string _lineSeparator = Environment.NewLine;

        protected static string LineSeparator
        {
            get { return _lineSeparator; }
        }

        /// <summary>
        /// Loads the contents of a resource into a String.
        /// </summary>
        /// <param name="resourceName">the name of the resource to load</param>
        /// <returns>the resource contents</returns>
        public virtual string Load(string resourceName)
        {
            using (var resStream = typeof(ParserTest).Assembly.GetManifestResourceStream(resourceName))
            {
                var reader = new StreamReader(resStream);
                return reader.ReadToEnd();
            }
        }

        protected virtual StreamFactory NewStreamFactory(string resourceName)
        {
            var factory = StreamFactory.NewInstance();
            LoadMappingFile(factory, resourceName);
            return factory;
        }

        protected virtual void LoadMappingFile(StreamFactory factory, string resourceName)
        {
            using (var resStream = typeof(ParserTest).Assembly.GetManifestResourceStream(resourceName))
            {
                factory.Load(resStream);
            }
        }

        protected virtual void AssertRecordError(IBeanReader reader, int lineNumber, string recordName, string message)
        {
            try
            {
                reader.Read();
                throw new Exception("Record expected to fail validation");
            }
            catch (InvalidRecordException ex)
            {
                Assert.Equal(recordName, reader.RecordName);
                Assert.Equal(lineNumber, reader.LineNumber);

                var ctx = ex.RecordContext;
                Assert.Equal(recordName, ctx.RecordName);
                Assert.Equal(lineNumber, ctx.LineNumber);

                foreach (var s in ctx.RecordErrors)
                {
                    Assert.Equal(message, s);
                }
            }
        }

        protected virtual void AssertFieldError(IBeanReader reader, int lineNumber, string recordName, string fieldName, string fieldText, string message)
        {
            AssertFieldError(reader, lineNumber, recordName, fieldName, 0, fieldText, message);
        }

        protected virtual void AssertFieldError(IBeanReader reader, int lineNumber, string recordName, string fieldName, int fieldIndex, string fieldText, string message)
        {
            try
            {
                reader.Read();
                throw new Exception("Record expected to fail validation");
            }
            catch (InvalidRecordException ex)
            {
                Assert.Equal(recordName, reader.RecordName);
                Assert.Equal(lineNumber, reader.LineNumber);

                var ctx = ex.RecordContext;
                Assert.Equal(recordName, ctx.RecordName);
                Assert.Equal(lineNumber, ctx.LineNumber);
                Assert.Equal(fieldText, ctx.GetFieldText(fieldName, fieldIndex));

                foreach (var s in ctx.RecordErrors)
                {
                    Assert.Equal(message, s);
                }
            }
        }
    }
}
