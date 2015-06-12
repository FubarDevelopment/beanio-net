using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;

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
            var birthDateHandler = new DateTimeTypeHandler();
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

        [Fact]
        public void TestGroupBuilder()
        {
            var b = new GroupBuilder("group");
            var c = b.Build();
            Assert.Equal("group", c.Name);
            Assert.Null(c.Order);
            Assert.Null(c.MinOccurs);
            Assert.Null(c.MaxOccurs);

            b.Order(1)
             .AddRecord(new RecordBuilder("record"))
             .AddGroup(new GroupBuilder("subgroup"));
            c = b.Build();
            Assert.Equal(1, c.Order);
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
        }

        [Fact]
        public void TestRecordBuilder()
        {
            var b = new RecordBuilder("record");
            var c = b.Build();
            Assert.Equal("record", c.Name);
            Assert.Null(c.Order);
            Assert.Null(c.MinLength);
            Assert.Null(c.MaxLength);
            Assert.Null(c.MinOccurs);
            Assert.Null(c.MaxOccurs);
            Assert.Null(c.MinMatchLength);
            Assert.Null(c.MaxMatchLength);

            b.Order(2)
             .MinLength(1)
             .MaxLength(-1)
             .RidLength(5, 10)
             .AddField(new FieldBuilder("field"))
             .AddSegment(new SegmentBuilder("segment"));
            c = b.Build();
            Assert.Equal(2, c.Order);
            Assert.Equal(1, c.MinLength);
            Assert.Null(c.MaxLength);
            Assert.Equal(5, c.MinMatchLength);
            Assert.Equal(10, c.MaxMatchLength);
            Assert.Collection(
                c.Children,
                child =>
                    {
                        Assert.Equal("field", child.Name);
                        Assert.IsType(typeof(FieldConfig), child);
                    },
                child =>
                    {
                        Assert.Equal("segment", child.Name);
                        Assert.IsType(typeof(SegmentConfig), child);
                    });

            b = new RecordBuilder("record")
                .Length(5)
                .RidLength(10);
            c = b.Build();
            Assert.Equal(5, c.MinLength);
            Assert.Equal(5, c.MaxLength);
            Assert.Equal(10, c.MinMatchLength);
            Assert.Equal(10, c.MaxMatchLength);
        }

        [Fact]
        public void TestSegmentBuilder()
        {
            var b = new SegmentBuilder("segment");
            var c = b.Build();
            Assert.Equal("segment", c.Name);
            Assert.Null(c.Key);
            Assert.Null(c.Target);
            Assert.Null(c.OccursRef);
            Assert.False(c.IsNillable);
            Assert.Null(c.MinOccurs);
            Assert.Null(c.MaxOccurs);

            b.OccursRef("other")
             .Key("key")
             .Value("value")
             .IsNillable()
             .Occurs(5)
             .AddField(new FieldBuilder("field"))
             .AddSegment(new SegmentBuilder("segment"));
            c = b.Build();
            Assert.Equal("other", c.OccursRef);
            Assert.Equal("key", c.Key);
            Assert.Equal("value", c.Target);
            Assert.True(c.IsNillable);
            Assert.Equal(5, c.MinOccurs);
            Assert.Equal(5, c.MaxOccurs);
            Assert.Collection(
                c.Children,
                child =>
                {
                    Assert.Equal("field", child.Name);
                    Assert.IsType(typeof(FieldConfig), child);
                },
                child =>
                {
                    Assert.Equal("segment", child.Name);
                    Assert.IsType(typeof(SegmentConfig), child);
                });
        }

        [Fact]
        public void TestFieldBuilder()
        {
            var b = new FieldBuilder("field");
            var c = b.Build();
            Assert.Equal("field", c.Name);
            Assert.Null(c.Type);
            Assert.Null(c.Collection);
            Assert.Null(c.Getter);
            Assert.Null(c.Setter);
            Assert.True(c.IsBound);
            Assert.Null(c.Position);
            Assert.Null(c.Until);
            Assert.False(c.IsIdentifier);
            Assert.False(c.IsTrim);
            Assert.False(c.IsRequired);
            Assert.False(c.IsLazy);
            Assert.Null(c.OccursRef);
            Assert.Null(c.MinOccurs);
            Assert.Null(c.MaxOccurs);
            Assert.Null(c.MinLength);
            Assert.Null(c.MaxLength);
            Assert.Null(c.Length);
            Assert.Null(c.Literal);
            Assert.Null(c.Default);
            Assert.Null(c.Format);
            Assert.Equal(Align.Left, c.Justify);
            Assert.Null(c.Padding);
            Assert.False(c.KeepPadding);
            Assert.False(c.IsLenientPadding);
            Assert.Null(c.RegEx);
            Assert.False(c.IsNillable);
            Assert.Null(c.XmlType);
            Assert.Null(c.XmlName);
            Assert.Null(c.XmlPrefix);
            Assert.Null(c.XmlNamespace);
            Assert.Null(c.TypeHandler);

            b.Type(typeof(int))
             .Collection(typeof(List<object>))
             .Getter("GetField")
             .Setter("SetField")
             .Ignore()
             .At(3)
             .Until(-2)
             .Rid()
             .Trim()
             .Required()
             .Lazy()
             .OccursRef("other")
             .Occurs(0, -1)
             .Length(10)
             .MinLength(0)
             .MaxLength(-1)
             .Literal("literal")
             .DefaultValue("default")
             .Format("format")
             .Align(Align.Right)
             .Padding('X')
             .KeepPadding()
             .LenientPadding()
             .RegEx(".*")
             .Nillable()
             .XmlType(XmlNodeType.Attribute)
             .XmlName("xmlName")
             .XmlPrefix("prefix")
             .XmlNamespace("namespace")
             .TypeHandler("typeHandler");
            c = b.Build();
            Assert.Equal(typeof(int).GetTypeInfo().AssemblyQualifiedName, c.Type);
            Assert.Equal(typeof(List<>).GetTypeInfo().AssemblyQualifiedName, c.Collection);
            Assert.Equal("GetField", c.Getter);
            Assert.Equal("SetField", c.Setter);
            Assert.False(c.IsBound);
            Assert.Equal(3, c.Position);
            Assert.Equal(-2, c.Until);
            Assert.True(c.IsIdentifier);
            Assert.True(c.IsTrim);
            Assert.True(c.IsRequired);
            Assert.True(c.IsLazy);
            Assert.Equal("other", c.OccursRef);
            Assert.Equal(0, c.MinOccurs);
            Assert.Null(c.MaxOccurs);
            Assert.Equal(0, c.MinLength);
            Assert.Null(c.MaxLength);
            Assert.Equal(10, c.Length);
            Assert.Equal("literal", c.Literal);
            Assert.Equal("default", c.Default);
            Assert.Equal("format", c.Format);
            Assert.Equal(Align.Right, c.Justify);
            Assert.Equal('X', c.Padding);
            Assert.True(c.KeepPadding);
            Assert.True(c.IsLenientPadding);
            Assert.Equal(".*", c.RegEx);
            Assert.True(c.IsNillable);
            Assert.Equal(XmlNodeType.Attribute, c.XmlType);
            Assert.Equal("xmlName", c.XmlName);
            Assert.Equal("prefix", c.XmlPrefix);
            Assert.Equal("namespace", c.XmlNamespace);
            Assert.Equal("typeHandler", c.TypeHandler);

            b = new FieldBuilder("field")
                .TypeHandler<StringTypeHandler>();
            c = b.Build();
            Assert.Equal(typeof(StringTypeHandler).GetTypeInfo().AssemblyQualifiedName, c.TypeHandler);

            var th = new StringTypeHandler();
            b = new FieldBuilder("field")
                .TypeHandler(th);
            c = b.Build();
            Assert.Same(th, c.TypeHandlerInstance);
        }
    }
}
