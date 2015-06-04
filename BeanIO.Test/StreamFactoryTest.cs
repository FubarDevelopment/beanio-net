using System;

using Xunit;

namespace BeanIO
{
    public class StreamFactoryTest
    {
        [Fact]
        public void TestLoadMappingFile()
        {
            using (var mappingStream = typeof(StreamFactoryTest).Assembly.GetManifestResourceStream("BeanIO.mapping.xml"))
            {
                var factory = StreamFactory.NewInstance();
                factory.Load(mappingStream);
            }
        }
    }
}
