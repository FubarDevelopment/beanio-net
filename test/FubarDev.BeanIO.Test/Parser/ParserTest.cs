// <copyright file="ParserTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

using Xunit;

namespace BeanIO.Parser
{
    /// <summary>
    /// Base class for test classes that test the parsing framework.
    /// </summary>
    public class ParserTest
    {
        protected static string LineSeparator { get; } = Environment.NewLine;

        public System.IO.Stream LoadStream(string fileName)
        {
            return LoadStreamInternal(fileName, 2);
        }

        public TextReader LoadReader(string fileName)
        {
            return new StreamReader(LoadStreamInternal(fileName, 2));
        }

        /// <summary>
        /// Loads the contents of a resource into a String.
        /// </summary>
        /// <param name="resourceName">the name of the resource to load</param>
        /// <returns>the resource contents</returns>
        public virtual string Load(string resourceName)
        {
            using (var resStream = LoadStreamInternal(resourceName, 2))
            {
                var reader = new StreamReader(resStream);
                return reader.ReadToEnd();
            }
        }

        protected virtual StreamFactory NewStreamFactory(string resourceName)
        {
            var factory = StreamFactory.NewInstance();
            using (var resStream = LoadStreamInternal(resourceName, 2))
            {
                factory.Load(resStream);
            }

            return factory;
        }

        protected virtual void LoadMappingFile(StreamFactory factory, string resourceName)
        {
            using (var resStream = LoadStreamInternal(resourceName, 2))
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

        private System.IO.Stream LoadStreamInternal(string fileName, int levels)
        {
            var asm = typeof(AbstractParserTest).GetTypeInfo().Assembly;
            var resStream = asm.GetManifestResourceStream(fileName);

            if (resStream == null)
            {
                var resName = $"{GetType().Namespace}.{fileName}";
                resStream = asm.GetManifestResourceStream(resName);
            }

            if (resStream == null)
                throw new ArgumentOutOfRangeException("fileName");
            return resStream;
        }
    }
}
