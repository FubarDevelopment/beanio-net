namespace BeanIO.Internal.Parser.Format.Delimited
{
    /// <summary>
    /// A <see cref="StreamFormatSupport"/> implementation for the delimited stream format.
    /// </summary>
    public class DelimitedStreamFormat : StreamFormatSupport
    {
        /// <summary>
        /// Creates a new unmarshalling context
        /// </summary>
        /// <returns>the new <see cref="UnmarshallingContext"/></returns>
        public override UnmarshallingContext CreateUnmarshallingContext()
        {
            return new DelimitedUnmarshallingContext();
        }

        /// <summary>
        /// Creates a new marshalling context
        /// </summary>
        /// <param name="streaming">true if marshalling to a stream</param>
        /// <returns>the new <see cref="MarshallingContext"/></returns>
        public override MarshallingContext CreateMarshallingContext(bool streaming)
        {
            return new DelimitedMarshallingContext();
        }
    }
}
