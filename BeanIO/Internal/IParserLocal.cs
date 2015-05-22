using BeanIO.Internal.Parser;

namespace BeanIO.Internal
{
    public interface IParserLocal
    {
        void Init(int index, ParsingContext context);
    }
}
