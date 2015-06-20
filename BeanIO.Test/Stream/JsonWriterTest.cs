using BeanIO.Stream.Json;

using Newtonsoft.Json.Linq;

using Xunit;

namespace BeanIO.Stream
{
    public class JsonWriterTest
    {
        [Fact]
        public void TestWriteObject()
        {
            var numberList = new JArray(20.45, 20);
            var list = new JArray(true, false, NewObject(), numberList);
            var map = new JObject(
                new JProperty("null", null),
                new JProperty("field", "value"),
                new JProperty("list", list),
                new JProperty("object", NewObject()));

            var m = new JsonRecordMarshaller();
            Assert.Equal("{\"null\":null,\"field\":\"value\",\"list\":[true,false,{\"field\":\"value\"},[20.45,20]],\"object\":{\"field\":\"value\"}}", m.Marshal(map));

            var objectList = new JArray(NewObject(), NewObject());
            map["objectList"] = objectList;

            m = new JsonRecordMarshaller(new JsonParserConfiguration()
                {
                    Pretty = true,
                    LineSeparator = "\n",
                });

            var expected =
                "{\n" +
                "  \"null\": null,\n" +
                "  \"field\": \"value\",\n" +
                "  \"list\": [\n" +
                "    true,\n" +
                "    false,\n" +
                "    {\n" +
                "      \"field\": \"value\"\n" +
                "    },\n" +
                "    [\n" +
                "      20.45,\n" +
                "      20\n" +
                "    ]\n" +
                "  ],\n" +
                "  \"object\": {\n" +
                "    \"field\": \"value\"\n" +
                "  },\n" +
                "  \"objectList\": [\n" +
                "    {\n" +
                "      \"field\": \"value\"\n" +
                "    },\n" +
                "    {\n" +
                "      \"field\": \"value\"\n" +
                "    }\n" +
                "  ]\n" +
                "}";

            Assert.Equal(expected, m.Marshal(map));
        }

        private JObject NewObject()
        {
            return new JObject(new JProperty("field", "value"));
        }
    }
}
