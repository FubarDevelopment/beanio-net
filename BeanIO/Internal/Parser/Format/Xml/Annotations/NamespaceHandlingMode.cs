namespace BeanIO.Internal.Parser.Format.Xml.Annotations
{
    /// <summary>
    /// Various modes of namespace handling
    /// </summary>
    internal enum NamespaceHandlingMode
    {
        /// <summary>
        /// Use the namespace as-is
        /// </summary>
        UseNamespace,

        /// <summary>
        /// Ignore the namespace
        /// </summary>
        IgnoreNamespace,

        /// <summary>
        /// Use the default namespace
        /// </summary>
        DefaultNamespace,

        /// <summary>
        /// Apply namespace without prefix
        /// </summary>
        NoPrefix,
    }
}
