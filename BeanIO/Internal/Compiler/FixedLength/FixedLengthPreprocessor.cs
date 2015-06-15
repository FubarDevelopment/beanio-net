using BeanIO.Internal.Compiler.Flat;
using BeanIO.Internal.Config;

namespace BeanIO.Internal.Compiler.FixedLength
{
    /// <summary>
    /// Configuration <see cref="Preprocessor"/> for a fixed length stream format.
    /// </summary>
    internal class FixedLengthPreprocessor : FlatPreprocessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FixedLengthPreprocessor"/> class.
        /// </summary>
        /// <param name="stream">the stream configuration to pre-process</param>
        public FixedLengthPreprocessor(StreamConfig stream)
            : base(stream)
        {
        }

        /// <summary>
        /// Gets a value indicating whether the stream format is fixed length.
        /// </summary>
        protected override bool IsFixedLength
        {
            get { return true; }
        }

        /// <summary>
        /// Returns the size of a field.
        /// </summary>
        /// <remarks>null = unbounded</remarks>
        /// <param name="field">the field to get the size from</param>
        /// <returns>the field size</returns>
        protected override int? GetSize(FieldConfig field)
        {
            return field.Length;
        }
    }
}
