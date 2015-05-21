using BeanIO.Builder;

using JetBrains.Annotations;

namespace BeanIO.Internal.Config
{
    /// <summary>
    /// A field is used to define the smallest physical part of a stream.
    /// Fields are combined to form segments and records.
    /// </summary>
    /// <remarks>
    /// <para>Unless <see cref="FieldConfig.IsBound"/> is set to false, a field is bound to a property of
    /// its closest parent bean object.</para>
    /// <para>Position must be set for all fields in record, or for none of them.  If not
    /// set, position is determined based on the order in which the fields are added to
    /// the record.</para>
    /// </remarks>
    public class FieldConfig : SimplePropertyConfig
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
            get { return ComponentType.Field; }
        }

        /// <summary>
        /// Gets or sets the textual representation of the default value for
        /// this field when the field is not present or empty during unmarshalling.
        /// </summary>
        [CanBeNull]
        public string Default { get; set; }

        /// <summary>
        /// Gets or sets the minimum length of this field in characters, or null
        /// if a minimum length should not be enforced.
        /// </summary>
        public int? MinLength { get; set; }

        /// <summary>
        /// Gets or sets the maximum length of this field in characters.
        /// </summary>
        /// <returns>null if a maximum length will not be enforced</returns>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether field text should be trimmed before validation
        /// and type conversion.
        /// </summary>
        public bool IsTrim { get; set; }

        /// <summary>
        /// Gets or sets the static text for this field, or null if the field text is not static.
        /// </summary>
        /// <remarks>
        /// If set, unmarshalled field text must match the literal text, and likewise, the
        /// literal text is always marshalled for this field.
        /// </remarks>
        public string Literal { get; set; }

        /// <summary>
        /// Gets or sets the regular expression pattern for validating the field text
        /// during unmarshalling.
        /// </summary>
        /// <remarks>
        /// Field text is only validated using the regular expression after trimming
        /// (if enabled) and when its not the empty string.
        /// </remarks>
        public string RegEx { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this field is required when unmarshalled.
        /// </summary>
        /// <remarks>
        /// Required fields must have at least one character (after
        /// trimming, if enabled).  If this field is not required and no text
        /// is parsed from the input stream, no further validations are performed.
        /// </remarks>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the length of this field in characters.
        /// </summary>
        /// <remarks>
        /// Applies to fixed length and padded fields.
        /// May return -1 (aka 'unbounded'), for the last field in
        /// a fixed length record to indicate it is not padded and
        /// of variable length.
        /// </remarks>
        public int? Length { get; set; }

        /// <summary>
        /// Gets or sets the character used to pad this field.
        /// </summary>
        /// <remarks>
        /// Defaults to a space when padding is enabled.
        /// </remarks>
        public char? Padding { get; set; }

        /// <summary>
        /// Gets or sets the justification of this field.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="F:Align.Left"/>.
        /// Applies to fixed length and padded fields.
        /// </remarks>
        public Align Justify { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a fixed length
        /// field should keep its padding when unmarshalled.
        /// </summary>
        public bool KeepPadding { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether padding length
        /// is enforced for fixed length formatted streams.
        /// </summary>
        public bool IsLenientPadding { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this field is
        /// referenced by another component.
        /// </summary>
        public bool IsRef { get; set; }
    }
}
