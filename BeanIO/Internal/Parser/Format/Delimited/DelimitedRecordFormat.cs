namespace BeanIO.Internal.Parser.Format.Delimited
{
    /// <summary>
    /// A <see cref="IRecordFormat"/> for delimited records.
    /// </summary>
    /// <remarks>
    /// A delimited record may be configured to validate a record field count by
    /// setting a minimum and maximum length.
    /// </remarks>
    public class DelimitedRecordFormat : IRecordFormat
    {
        /// <summary>
        /// Gets or sets the minimum number of fields in the record
        /// </summary>
        public int MinLength { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of fields in the record
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the minimum record length for identifying a record
        /// </summary>
        public int MinMatchLength { get; set; }

        /// <summary>
        /// Gets or sets the maximum record length for identifying a record
        /// </summary>
        public int? MaxMatchLength { get; set; }

        /// <summary>
        /// Returns whether the record meets configured matching criteria during unmarshalling.
        /// </summary>
        /// <param name="context">the <see cref="UnmarshallingContext"/></param>
        /// <returns>true if the record meets all matching criteria, false otherwise</returns>
        public bool Matches(UnmarshallingContext context)
        {
            var length = ((DelimitedUnmarshallingContext)context).FieldCount;
            return length >= MinMatchLength && (MaxMatchLength == null || length <= MaxMatchLength);
        }

        /// <summary>
        /// Validates a record during unmarshalling
        /// </summary>
        /// <param name="context">the <see cref="UnmarshallingContext"/></param>
        public void Validate(UnmarshallingContext context)
        {
            var length = ((DelimitedUnmarshallingContext)context).FieldCount;
            if (length < MinLength)
                context.AddRecordError("minLength", MinLength, MaxLength ?? int.MaxValue);
            if (MaxLength != null && length > MaxLength)
                context.AddRecordError("maxLength", MinLength, MaxLength);
        }
    }
}
