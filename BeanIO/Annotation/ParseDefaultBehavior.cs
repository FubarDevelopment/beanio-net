namespace BeanIO.Annotation
{
    /// <summary>
    /// The behavior for parsing the default value during configuration
    /// </summary>
    public enum ParseDefaultBehavior
    {
        /// <summary>
        /// As configured in the <code>beanio.properties</code> (default: <code>Unmarshal</code>)
        /// </summary>
        Default,

        /// <summary>
        /// Unmarshal the default value during configuration
        /// </summary>
        Parse,

        /// <summary>
        /// Keep the default value as text
        /// </summary>
        DontParse,
    }
}
