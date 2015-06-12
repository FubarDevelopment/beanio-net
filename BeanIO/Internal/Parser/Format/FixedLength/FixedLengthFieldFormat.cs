using BeanIO.Internal.Parser.Format.Flat;

namespace BeanIO.Internal.Parser.Format.FixedLength
{
    internal class FixedLengthFieldFormat : FlatFieldFormatSupport
    {
        /// <summary>
        /// Gets or sets a value indicating whether to keep field padding during unmarshalling
        /// </summary>
        public bool KeepPadding { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the padding length is enforced
        /// </summary>
        public bool IsLenientPadding { get; set; }

        /// <summary>
        /// Gets the size of the field
        /// </summary>
        /// <remarks>
        /// Fixed length formats should return the field length, while other formats should simply return 1.
        /// </remarks>
        public override int Size
        {
            get
            {
                return Padding.Length;
            }
        }

        /// <summary>
        /// Extracts the field text from a record.
        /// </summary>
        /// <remarks>
        /// <para>May return <see cref="F:Value.Invalid"/> if the field is invalid, or <see cref="F:Value.Nil"/>
        /// if the field is explicitly set to nil or null such as in an XML or JSON formatted
        /// stream.</para>
        /// <para>Implementations should also remove any field padding before returning the text.</para>
        /// </remarks>
        /// <param name="context">the <see cref="UnmarshallingContext"/> holding the record</param>
        /// <param name="reportErrors">report the errors?</param>
        /// <returns>the field text or null if the field was not present in the record</returns>
        public override string Extract(UnmarshallingContext context, bool reportErrors)
        {
            var text = ExtractFieldText(context, reportErrors);
            if (text == null)
                return null;

            var padding = Padding;
            if (padding.Length >= 0 && text.Length != padding.Length && !IsLenientPadding)
            {
                if (reportErrors)
                    context.AddFieldError(Name, text, "length", padding.Length);
                return Value.Invalid;
            }

            if (KeepPadding)
            {
                // return empty string for required fields to trigger the field validation
                if (!padding.IsOptional)
                {
                    var s = padding.Unpad(text);
                    if (string.IsNullOrEmpty(s))
                        return s;
                }

                return text;
            }

            return padding.Unpad(text);
        }

        /// <summary>
        /// Inserts field text into a record
        /// </summary>
        /// <param name="context">the <see cref="MarshallingContext"/> holding the record</param>
        /// <param name="text">the field text to insert into the record</param>
        /// <param name="commit">true to commit the current record, or false
        /// if the field is optional and should not extend the record
        /// unless a subsequent field is later appended to the record</param>
        protected override void InsertFieldText(MarshallingContext context, string text, bool commit)
        {
            var ctx = (FixedLengthMarshallingContext)context;
            ctx.SetFieldText(Position, text, commit);
        }

        /// <summary>
        /// Extracts the field text from a record.
        /// </summary>
        /// <param name="context">the <see cref="UnmarshallingContext"/> holding the record</param>
        /// <param name="reporting">report the errors?</param>
        /// <returns>the field text or null if the field was not present in the record</returns>
        protected override string ExtractFieldText(UnmarshallingContext context, bool reporting)
        {
            var ctx = (FixedLengthUnmarshallingContext)context;
            return ctx.GetFieldText(Name, Position, Size, Until);
        }
    }
}
