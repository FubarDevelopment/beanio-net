namespace BeanIO.Internal.Parser.Format.Json
{
    /// <summary>
    /// A <see cref="StreamFormatSupport"/> implementation for the JSON stream format.
    /// </summary>
    internal class JsonStreamFormat : StreamFormatSupport
    {
        /// <summary>
        /// Gets or sets the maximum depth of the all <see cref="JsonWrapper"/> components in the parser tree layout.
        /// </summary>
        public int MaxDepth { get; set; }

        /// <summary>
        /// Creates a new unmarshalling context
        /// </summary>
        /// <returns>the new <see cref="UnmarshallingContext"/></returns>
        public override UnmarshallingContext CreateUnmarshallingContext()
        {
            return new JsonUnmarshallingContext(MaxDepth);
        }

        /// <summary>
        /// Creates a new marshalling context
        /// </summary>
        /// <param name="streaming">true if marshalling to a stream</param>
        /// <returns>the new <see cref="MarshallingContext"/></returns>
        public override MarshallingContext CreateMarshallingContext(bool streaming)
        {
            return new JsonMarshallingContext(MaxDepth);
        }
    }
}
