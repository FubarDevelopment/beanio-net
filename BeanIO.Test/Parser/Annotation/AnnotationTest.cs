using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

using BeanIO.Builder;
using BeanIO.Stream;

using Xunit;

namespace BeanIO.Parser.Annotation
{
    public class AnnotationTest : ParserTest
    {
        [Fact]
        public void TestAnnotatedSegments()
        {
            var factory = StreamFactory.NewInstance();

            factory.Define(
                new StreamBuilder("s1")
                    .Format("csv")
                    .AddRecord(typeof(AnnotatedRoom)));

            factory.Define(
                new StreamBuilder("s1-xml")
                    .Format("xml")
                    .NameConversionMode(ElementNameConversionMode.Decapitalize | ElementNameConversionMode.RemoveUnderscore)
                    .AddRecord(typeof(AnnotatedRoom)));

            var u = new[]
                {
                    factory.CreateUnmarshaller("s1"),
                    factory.CreateUnmarshaller("s1-xml")
                };

            var m = new[]
                {
                    factory.CreateMarshaller("s1"),
                    factory.CreateMarshaller("s1-xml")
                };

            var input = new[]
                {
                    // CSV input:
                    "2,60,CFL,40,IC,Bath,hardwood,10,20",

                    // XML input:
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                    "<s1-xml>" +
                    "<annotatedRoom>" +
                    "<light quantity=\"2\"><bulbs><watts>60</watts><style>CFL</style></bulbs><bulbs><watts>40</watts><style>IC</style></bulbs></light>" +
                    "<name>Bath</name>" +
                    "<flooring><floor>hardwood</floor><width>10</width><height>20</height></flooring>" +
                    "</annotatedRoom></s1-xml>"
                };

            for (var i = 0; i < input.Length; i++)
            {
                var room = (AnnotatedRoom)u[i].Unmarshal(input[i]);

                Assert.Equal("Bath", room.Name);

                var flooring = room.GetFlooring();
                Assert.NotNull(flooring);
                Assert.Equal(10, flooring.Width);
                Assert.Equal(20, flooring.Height);

                var fixture = room.GetLightFixture();
                Assert.NotNull(fixture);
                Assert.Equal(2, fixture.Quantity);

                var bulbs = Assert.IsType<List<AnnotatedBulb>>(fixture.Bulbs);

                Assert.Collection(
                    bulbs,
                    bulb =>
                        {
                            Assert.Equal(60, bulb.Watts);
                            Assert.Equal("CFL", bulb.Style);
                        },
                    bulb =>
                        {
                            Assert.Equal(40, bulb.Watts);
                            Assert.Equal("IC", bulb.Style);
                        });

                var marshalled = m[i].Marshal(room).ToString();
                Assert.Equal(input[i], marshalled);
            }
        }

        [Fact]
        public void TestAnnotatedFields()
        {
            var factory = StreamFactory.NewInstance();

            factory.Define(
                new StreamBuilder("s1")
                    .Format("csv")
                    .AddRecord(typeof(AnnotatedUser)));

            factory.Define(
                new StreamBuilder("s1-xml")
                    .Format("xml")
                    .XmlType(XmlNodeType.None)
                    .NameConversionMode(ElementNameConversionMode.Decapitalize | ElementNameConversionMode.RemoveUnderscore)
                    .AddRecord(typeof(AnnotatedUser)));

            var u = new[]
                {
                    factory.CreateUnmarshaller("s1"),
                    factory.CreateUnmarshaller("s1-xml")
                };

            var m = new[]
                {
                    factory.CreateMarshaller("s1"),
                    factory.CreateMarshaller("s1-xml")
                };

            var input = new[]
                {
                    // CSV input:
                    "USER,joe,smith,left,right,1970-01-01,0028,A,B,1,END",

                    // XML input:
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                    "<user xmlns=\"http://org.beanio.test\" type=\"USER\">" +
                    "<firstName>joe</firstName>" +
                    "<lastName>smith</lastName>" +
                    "<hands>left</hands>" +
                    "<hands>right</hands>" +
                    "<birthDate>1970-01-01</birthDate>" +
                    "<age>0028</age>" +
                    "<letters>A</letters>" +
                    "<letters>B</letters>" +
                    "<numbers>1</numbers>" +
                    "<end>END</end>" +
                    "</user>"
                };

            for (var i = 0; i < input.Length; i++)
            {
                var user = (AnnotatedUser)u[i].Unmarshal(input[i]);
                Assert.Equal("joe", user.FirstName);
                Assert.Equal("smith", user.GetSurname());
                Assert.Equal("left", user.Hands[0]);
                Assert.Equal("right", user.Hands[1]);
                Assert.Equal(new DateTime(1970, 1, 1), user.BirthDate);
                Assert.Equal(28, user.Age);
                Assert.Equal(2, user.Letters.Count);
                Assert.Equal('A', user.Letters[0]);
                Assert.Equal('B', user.Letters[1]);
                Assert.Equal(1, user.Numbers.Count);
                var numbers = Assert.IsType<ArrayList>(user.Numbers);
                Assert.Equal(1, numbers[0]);
                Assert.Equal("END", user.End);

                var marshalled = m[i].Marshal(user).ToString();
                Assert.Equal(input[i], marshalled);
            }
        }
    }
}
