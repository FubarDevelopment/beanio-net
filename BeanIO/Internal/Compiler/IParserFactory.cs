using BeanIO.Internal.Config;
using BeanIO.Internal.Util;

namespace BeanIO.Internal.Compiler
{
    /// <summary>
    /// A <see cref="IParserFactory"/> is used to convert a stream configuration (i.e. <see cref="StreamConfig"/>)
    /// into a stream parser (i.e. <see cref="Parser.Stream"/>).
    /// </summary>
    public interface IParserFactory
    {
        /// <summary>
        /// Gets or sets the type handler factory to use for resolving type handlers
        /// </summary>
        TypeHandlerFactory TypeHandlerFactory { get; set; }

        /// <summary>
        /// Creates a new stream parser from a given stream configuration
        /// </summary>
        /// <param name="config">the stream configuration</param>
        /// <returns>the created <see cref="Parser.Stream"/></returns>
        Parser.Stream CreateStream(StreamConfig config);
    }
}
