using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using JetBrains.Annotations;

using Xunit;

namespace BeanIO.Parser.Validation
{
    public class FieldValidationTest : ParserTest
    {
        [Fact]
        public void TestFieldValidation()
        {
            var factory = NewStreamFactory("BeanIO.Parser.Validation.validation.xml");
            var reader = factory.CreateReader("v1", LoadStream("v1.txt"));
            try
            {
                var info = new ValidationInfo(reader);
                TestValid(info, "regex", "12345");
                TestInvalid(info, "regex", "abc", "regex('\\d+') at line 2");
                TestValid(info, "minLength", "ab");
                TestInvalid(info, "minLength", "a", "minLength(2) at line 4");
                TestValid(info, "maxLength", "abcde");
                TestInvalid(info, "maxLength", "abcdef", "maxLength(5) at line 6");
                TestValid(info, "requiredWithTrim", "value");
                TestInvalid(info, "requiredWithTrim", "     ", "required at line 8");
                TestValid(info, "typeHandler", new DateTime(1970, 1, 1));

                // TODO: Fix the following test part
                info.Reader.Skip(1);
                info.LineNumber = info.LineNumber + 1;
                ////TestInvalid(info, "typeHandler", "010170a", "type at line 10");

                TestValid(info, "requiredWithoutTrim", " ");
                TestInvalid(info, "requiredWithoutTrim", string.Empty, "required at line 12");
                TestValid(info, "literal", "value");
                TestInvalid(info, "literal", "other", "Invalid Literal Field at line 14 on Literal Record, expected 'value'");
            }
            finally
            {
                reader.Close();
            }
        }

        private static TextReader LoadStream(string fileName)
        {
            var resourceName = string.Format("BeanIO.Parser.Validation.{0}", fileName);
            var asm = typeof(FieldValidationTest).Assembly;
            var resStream = asm.GetManifestResourceStream(resourceName);
            if (resStream == null)
                throw new ArgumentOutOfRangeException("fileName");
            return new StreamReader(resStream);
        }

        private void TestValid(ValidationInfo info, [UsedImplicitly] string recordName, object expected)
        {
            info.LineNumber = info.LineNumber + 1;
            var record = Assert.IsType<Dictionary<string, object>>(info.Reader.Read());
            Assert.Equal(expected, record["field"]);
            Assert.Equal(recordName, info.Reader.RecordName);
            Assert.Equal(info.LineNumber, info.Reader.LineNumber);
        }

        private void TestInvalid(ValidationInfo info, [UsedImplicitly] string recordName, string fieldText, string message)
        {
            info.LineNumber = info.LineNumber + 1;
            var ex = Assert.ThrowsAny<InvalidRecordException>(() => info.Reader.Read());
            Assert.Equal(recordName, info.Reader.RecordName);
            Assert.Equal(info.LineNumber, info.Reader.LineNumber);

            var ctx = ex.RecordContext;
            Assert.Equal(recordName, ctx.RecordName);
            Assert.Equal(info.LineNumber, ctx.LineNumber);
            Assert.Equal(fieldText, ctx.GetFieldText("field"));
            if (!string.IsNullOrEmpty(message))
            {
                var fieldError = ctx.GetFieldErrors("field").First();
                Assert.Equal(message, fieldError);
            }
        }

        private class ValidationInfo
        {
            public ValidationInfo(IBeanReader reader)
            {
                Reader = reader;
            }

            public IBeanReader Reader { get; private set; }

            public int LineNumber { get; set; }
        }
    }
}
