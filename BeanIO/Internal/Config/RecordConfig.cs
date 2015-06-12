namespace BeanIO.Internal.Config
{
    /// <summary>
    /// A record is a segment that is bound to a record in a stream.
    /// </summary>
    /// <remarks>
    /// The physical representation of a record is dependent on the type of stream.
    /// A typical example might be a line in a fixed length or delimited flat file.
    /// </remarks>
    public class RecordConfig : SegmentConfig, ISelectorConfig
    {
        /// <summary>
        /// Gets the component type
        /// </summary>
        /// <returns>
        /// One of <see cref="F:ComponentType.Group"/>,
        /// <see cref="F:ComponentType.Record"/>, <see cref="F:ComponentType.Segment"/>
        /// <see cref="F:ComponentType.Field"/>, <see cref="F:ComponentType.Constant"/>,
        /// <see cref="F:ComponentType.Wrapper"/>, or <see cref="F:ComponentType.Stream"/>
        /// </returns>
        public override ComponentType ComponentType
        {
            get { return ComponentType.Record; }
        }

        /// <summary>
        /// Gets or sets the minimum length of the record.
        /// </summary>
        /// <remarks>
        /// Depending on the type of stream, the length may refer
        /// to the number of fields or the number of characters.
        /// </remarks>
        public int? MinLength { get; set; }

        /// <summary>
        /// Gets or sets the maximum length of the record.
        /// </summary>
        /// <remarks>
        /// Depending on the type of stream, the length may refer
        /// to the number of fields or the number of characters.
        /// </remarks>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the minimum record length for identifying this record.
        /// </summary>
        public int? MinMatchLength { get; set; }

        /// <summary>
        /// Gets or sets the maximum record length for identifying this record.
        /// </summary>
        public int? MaxMatchLength { get; set; }

        /// <summary>
        /// Gets or sets the order of this component within the context of its parent group.
        /// </summary>
        /// <remarks>
        /// Records and groups assigned the same order number may appear in any order.
        /// </remarks>
        /// <returns>the component order (starting at 1)</returns>
        public int? Order { get; set; }
    }
}
