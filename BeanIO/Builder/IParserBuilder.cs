using BeanIO.Internal.Config;
using BeanIO.Stream;

namespace BeanIO.Builder
{
    /// <summary>
    /// The basic parser builder interface
    /// </summary>
    public interface IParserBuilder
    {
        /// <summary>
        /// Builds the configuration about the record parser factory.
        /// </summary>
        /// <returns>The configuration for the record parser factory.</returns>
        BeanConfig<IRecordParserFactory> Build();
    }
}
