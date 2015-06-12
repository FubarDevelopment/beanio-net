namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// A <see cref="IRecordFormat"/> provides format specific processing for a <see cref="Record"/> parser.
    /// </summary>
    internal interface IRecordFormat
    {
        /// <summary>
        /// Returns whether the record meets configured matching criteria during unmarshalling.
        /// </summary>
        /// <param name="context">the <see cref="UnmarshallingContext"/></param>
        /// <returns>true if the record meets all matching criteria, false otherwise</returns>
        bool Matches(UnmarshallingContext context);

        /// <summary>
        /// Validates a record during unmarshalling
        /// </summary>
        /// <param name="context">the <see cref="UnmarshallingContext"/></param>
        void Validate(UnmarshallingContext context);
    }
}
