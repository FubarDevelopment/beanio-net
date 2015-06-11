using System;
using System.Text;

using BeanIO.Stream.Csv;
using BeanIO.Stream.Delimited;
using BeanIO.Stream.FixedLength;
using BeanIO.Stream.Xml;

using Xunit;

namespace BeanIO.Builder
{
    public class BuilderTest
    {
        [Fact]
        public void TestXmlParserBuilder()
        {
            var b = new XmlParserBuilder();
            var p = (XmlRecordParserFactory)b.Build().Create();
            Assert.Null(p.Indentation);
            Assert.Null(p.LineSeparator);
            Assert.NotNull(p.Version);
            Assert.Equal(new Version(1, 0), p.Version);
            Assert.NotNull(p.Encoding);
            Assert.Equal("utf-8", p.Encoding.WebName);
            Assert.False(p.SuppressHeader);
            Assert.Null(p.NamespaceMap);

            b = new XmlParserBuilder()
                .Indent()
                .HeaderVersion(new Version(2, 0))
                .HeaderEncoding(Encoding.ASCII)
                .LineSeparator("\n")
                .AddNamespace("r", "rock")
                .AddNamespace("p", "paper");
            p = (XmlRecordParserFactory)b.Build().Create();
            Assert.Equal(2, p.Indentation);
            Assert.Equal(new Version(2, 0), p.Version);
            Assert.Equal(Encoding.ASCII, p.Encoding);
            Assert.Equal("\n", p.LineSeparator);
            Assert.Collection(
                p.NamespaceMap,
                item =>
                    {
                        Assert.Equal("rock", item.Key);
                        Assert.Equal("r", item.Value);
                    },
                item =>
                    {
                        Assert.Equal("paper", item.Key);
                        Assert.Equal("p", item.Value);
                    });
        }

        [Fact]
        public void TestDelimitedParserBuilder()
        {
            var b = new DelimitedParserBuilder();
            var p = (DelimitedRecordParserFactory)b.Build().Create();
            Assert.Equal('\t', p.Delimiter);
            Assert.Null(p.Escape);
            Assert.Null(p.LineContinuationCharacter);
            Assert.Null(p.Comments);
            Assert.Null(p.RecordTerminator);

            b = new DelimitedParserBuilder()
                .Delimiter(',')
                .EnableEscape('\\')
                .EnableLineContinuation('&')
                .EnableComments("#", "!")
                .RecordTerminator("\n");
            p = (DelimitedRecordParserFactory)b.Build().Create();
            Assert.Equal(',', p.Delimiter);
            Assert.Equal('\\', p.Escape);
            Assert.Equal('&', p.LineContinuationCharacter);
            Assert.Collection(
                p.Comments,
                item => Assert.Equal("#", item),
                item => Assert.Equal("!", item));
            Assert.Equal("\n", p.RecordTerminator);

            b = new DelimitedParserBuilder('|');
            p = (DelimitedRecordParserFactory)b.Build().Create();
            Assert.Equal('|', p.Delimiter);
        }

        [Fact]
        public void TestFixedLengthParserBuilder()
        {
            var b = new FixedLengthParserBuilder();
            var p = (FixedLengthRecordParserFactory)b.Build().Create();
            Assert.Null(p.LineContinuationCharacter);
            Assert.Null(p.Comments);
            Assert.Null(p.RecordTerminator);

            b = new FixedLengthParserBuilder()
                .EnableLineContinuation('\\')
                .EnableComments("#", "!")
                .RecordTerminator("\r\n");
            p = (FixedLengthRecordParserFactory)b.Build().Create();
            Assert.Equal('\\', p.LineContinuationCharacter);
            Assert.Collection(
                p.Comments,
                item => Assert.Equal("#", item),
                item => Assert.Equal("!", item));
            Assert.Equal("\r\n", p.RecordTerminator);
        }

        [Fact]
        public void TestCsvParserBuilder()
        {
            var b = new CsvParserBuilder();
            var p = (CsvRecordParserFactory)b.Build().Create();
            Assert.Equal(',', p.Delimiter);
            Assert.Equal('"', p.Quote);
            Assert.Equal('"', p.Escape);
            Assert.False(p.IsMultilineEnabled);
            Assert.False(p.AlwaysQuote);
            Assert.False(p.IsWhitespaceAllowed);
            Assert.False(p.UnquotedQuotesAllowed);
            Assert.Null(p.Comments);
            Assert.Null(p.RecordTerminator);

            b = new CsvParserBuilder()
                .Delimiter('|')
                .Quote('\'')
                .Escape('\\')
                .EnableMultiline()
                .AlwaysQuote()
                .AllowUnquotedQuotes()
                .AllowUnquotedWhitespace()
                .EnableComments("#", "!")
                .RecordTerminator("\r");
            p = (CsvRecordParserFactory)b.Build().Create();
            Assert.Equal('|', p.Delimiter);
            Assert.Equal('\'', p.Quote);
            Assert.Equal('\\', p.Escape);
            Assert.True(p.IsMultilineEnabled);
            Assert.True(p.AlwaysQuote);
            Assert.True(p.IsWhitespaceAllowed);
            Assert.True(p.UnquotedQuotesAllowed);
            Assert.Collection(
                p.Comments,
                item => Assert.Equal("#", item),
                item => Assert.Equal("!", item));
            Assert.Equal("\r", p.RecordTerminator);
        }

        [Fact]
        public void TestStreamBuilder()
        {
            var b = new StreamBuilder("stream");
            var c = b.Build();
            Assert.Equal("stream", c.Name);
            throw new NotImplementedException();
        }
    }
}
