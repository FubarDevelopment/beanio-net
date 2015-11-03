using JetBrains.Annotations;

namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// A <see cref="IParser"/> is used marshal and unmarshal record components.
    /// </summary>
    internal interface IParser
    {
        /// <summary>
        /// Gets the name of this parser component
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the size of a single occurrence of this element, which is used to offset
        /// field positions for repeating segments and fields.
        /// </summary>
        /// <remarks>
        /// The concept of size is dependent on the stream format.  The size of an element in a fixed
        /// length stream format is determined by the length of the element in characters, while other
        /// stream formats calculate size based on the number of fields.  Some stream formats,
        /// such as XML, may ignore size settings.
        /// </remarks>
        int? Size { get; }

        /// <summary>
        /// Gets a value indicating whether this parser or any descendant of this parser is used to identify
        /// a record during unmarshalling.
        /// </summary>
        bool IsIdentifier { get; }

        /// <summary>
        /// Gets a value indicating whether this node must exist during unmarshalling.
        /// </summary>
        bool IsOptional { get; }

        /// <summary>
        /// Returns whether this parser and its children match a record being unmarshalled.
        /// </summary>
        /// <param name="context">The <see cref="UnmarshallingContext"/></param>
        /// <returns>true if matched, false otherwise</returns>
        bool Matches(UnmarshallingContext context);

        /// <summary>
        /// Unmarshals a record
        /// </summary>
        /// <param name="context">The <see cref="UnmarshallingContext"/></param>
        /// <returns>true if this component was present in the unmarshalled record, or false otherwise</returns>
        bool Unmarshal(UnmarshallingContext context);

        /// <summary>
        /// Marshals a record
        /// </summary>
        /// <param name="context">The <see cref="MarshallingContext"/></param>
        /// <returns>whether a value was marshalled</returns>
        bool Marshal(MarshallingContext context);

        /// <summary>
        /// Returns whether this parser or any of its descendant have content for marshalling.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <returns>true if there is content for marshalling, false otherwise</returns>
        bool HasContent(ParsingContext context);

        /// <summary>
        /// Clears the current property value.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        void ClearValue(ParsingContext context);

        /// <summary>
        /// Sets the property value for marshaling.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <param name="value">the property value</param>
        void SetValue(ParsingContext context, [CanBeNull] object value);

        /// <summary>
        /// Returns the unmarshalled property value.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <returns>the property value</returns>
        object GetValue(ParsingContext context);
    }
}
