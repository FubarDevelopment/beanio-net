using System;
using System.Reflection;
using System.Text;

using BeanIO.Internal.Config;
using BeanIO.Stream;
using BeanIO.Stream.Csv;
using BeanIO.Stream.Delimited;
using BeanIO.Stream.FixedLength;
using BeanIO.Stream.Xml;
using BeanIO.Types;

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
            Assert.Equal(1, c.Order);
            Assert.Equal(0, c.MinOccurs);
            Assert.Equal(1, c.MaxOccurs);
            Assert.Null(c.Format);
            Assert.Null(c.Mode);
            Assert.False(c.IsStrict);
            Assert.False(c.IgnoreUnidentifiedRecords);
            Assert.Equal(ElementNameConversionMode.Unchanged, c.NameConversionMode);

            var csvParser = new CsvRecordParserFactory();
            var birthDateHandler = new DateTimeHandler();
            var stringTypeHandler = new StringTypeHandler();

            b.Format("csv")
             .ReadOnly()
             .ResourceBundle("bundle")
             .Strict()
             .IgnoreUnidentifiedRecords()
             .Parser(csvParser)
             .AddRecord(new RecordBuilder("record"))
             .AddGroup(new GroupBuilder("subgroup"))
             .AddTypeHandler("birthDate", () => birthDateHandler)
             .AddTypeHandler(typeof(string), () => stringTypeHandler);
            c = b.Build();
            Assert.Equal("csv", c.Format);
            Assert.Equal(AccessMode.Read, c.Mode);
            Assert.Equal("bundle", c.ResourceBundle);
            Assert.True(c.IsStrict);
            Assert.True(c.IgnoreUnidentifiedRecords);
            Assert.Same(csvParser, c.ParserFactory.Create());
            Assert.Collection(
                c.Children,
                child =>
                    {
                        Assert.Equal("record", child.Name);
                        Assert.IsType(typeof(RecordConfig), child);
                    },
                child =>
                    {
                        Assert.Equal("subgroup", child.Name);
                        Assert.IsType(typeof(GroupConfig), child);
                    });
            Assert.Collection(
                c.Handlers,
                handler =>
                    {
                        Assert.Equal("birthDate", handler.Name);
                        Assert.Same(birthDateHandler, handler.Create());
                    },
                handler =>
                    {
                        Assert.Equal(typeof(string).GetTypeInfo().AssemblyQualifiedName, handler.Name);
                        Assert.Same(stringTypeHandler, handler.Create());
                    });

            var xmlParser = new XmlParserBuilder();
            b = new StreamBuilder("stream", "fixedlength")
                .WriteOnly()
                .Parser(xmlParser);
            c = b.Build();
            Assert.Equal("fixedlength", c.Format);
            Assert.Equal(AccessMode.Write, c.Mode);
            Assert.IsType(typeof(XmlRecordParserFactory), c.ParserFactory.Create());
        }
    }
}
