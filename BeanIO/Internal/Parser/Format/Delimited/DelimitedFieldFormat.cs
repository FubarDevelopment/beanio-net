using BeanIO.Internal.Parser.Format.Flat;

namespace BeanIO.Internal.Parser.Format.Delimited
{
    /// <summary>
    /// A <see cref="IFieldFormat"/> implementation for a field in a delimited stream.
    /// </summary>
    internal class DelimitedFieldFormat : FlatFieldFormatSupport
    {
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
            ((DelimitedMarshallingContext)context).SetField(Position, text, commit);
        }

        /// <summary>
        /// Extracts the field text from a record.
        /// </summary>
        /// <param name="context">the <see cref="UnmarshallingContext"/> holding the record</param>
        /// <param name="reporting">report the errors?</param>
        /// <returns>the field text or null if the field was not present in the record</returns>
        protected override string ExtractFieldText(UnmarshallingContext context, bool reporting)
        {
            return ((DelimitedUnmarshallingContext)context).GetFieldText(Name, Position, Until);
        }
    }
}
