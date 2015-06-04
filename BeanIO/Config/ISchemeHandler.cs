using System;

namespace BeanIO.Config
{
    /// <summary>
    /// Interface to handle loading mappings
    /// </summary>
    public interface ISchemeHandler
    {
        /// <summary>
        /// Gets the scheme this handler supports (e.g. file)
        /// </summary>
        string Scheme { get; }

        /// <summary>
        /// This functions opens a stream for the given <paramref name="resource"/> <see cref="Uri"/>
        /// </summary>
        /// <param name="resource">The resource to load the mapping from</param>
        /// <returns>the stream to read the mapping from</returns>
        System.IO.Stream Open(Uri resource);
    }
}
