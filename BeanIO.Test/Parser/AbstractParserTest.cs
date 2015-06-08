using System.IO;
using System.Text;

using BeanIO.Builder;

namespace BeanIO.Parser
{
    public abstract class AbstractParserTest
    {
        protected IBeanReader CreateReader(StreamFactory factory, string input, string name = "s")
        {
            return factory.CreateReader(name, new StringReader(input));
        }

        protected StreamFactory CreateFactory(string xml = null)
        {
            StreamFactory factory = StreamFactory.NewInstance();
            if (xml != null)
            {
                xml = "<beanio xmlns=\"http://www.beanio.org/2012/03\">\n" + xml + "\n</beanio>";
                factory.Load(new MemoryStream(Encoding.UTF8.GetBytes(xml)));
            }
            return factory;
        }

        protected StreamFactory CreateFactory(StreamBuilder builder)
        {
            StreamFactory factory = StreamFactory.NewInstance();
            if (builder != null)
            {
                factory.Define(builder);
            }
            return factory;
        }
    }
}
