using System.IO;

using BeanIO.Stream;
using BeanIO.Stream.FixedLength;

namespace BeanIO.Internal.Parser.Format.FixedLength
{
    internal class FixedLengthStreamFormat : StreamFormatSupport
    {
        /// <summary>
        /// Creates a new unmarshalling context
        /// </summary>
        /// <returns>the new <see cref="UnmarshallingContext"/></returns>
        public override UnmarshallingContext CreateUnmarshallingContext()
        {
            return new FixedLengthUnmarshallingContext();
        }

        /// <summary>
        /// Creates a new marshalling context
        /// </summary>
        /// <param name="streaming">true if marshalling to a stream</param>
        /// <returns>the new <see cref="MarshallingContext"/></returns>
        public override MarshallingContext CreateMarshallingContext(bool streaming)
        {
            return new FixedLengthMarshallingContext();
        }
    }
}
