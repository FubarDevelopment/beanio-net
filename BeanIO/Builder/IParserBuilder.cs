using BeanIO.Internal.Config;
using BeanIO.Stream;

namespace BeanIO.Builder
{
    public interface IParserBuilder
    {
        BeanConfig<IRecordParserFactory> Build();
    }
}
