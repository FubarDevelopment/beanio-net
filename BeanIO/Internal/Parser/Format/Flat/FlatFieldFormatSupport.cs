using System;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser.Format.Flat
{
    public abstract class FlatFieldFormatSupport : IFlatFieldFormat
    {
        /// <summary>
        /// Gets the size of the field
        /// </summary>
        /// <remarks>
        /// Fixed length formats should return the field length, while other formats should simply return 1.
        /// </remarks>
        public virtual int Size
        {
            get { return 1; }
        }

        /// <summary>
        /// Gets or sets the field name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the characters from the end of the record
        /// </summary>
        public int Until { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="FieldPadding"/>
        /// </summary>
        public FieldPadding Padding { get; set; }

        /// <summary>
        /// Gets a value indicating whether this field is nillable
        /// </summary>
        public virtual bool IsNillable
        {
            get { return false; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this field is optionally present in the record.
        /// </summary>
        /// <remarks>
        /// TODO: rename isLazy to something better??
        /// </remarks>
        public virtual bool IsLazy { get; set; }

        /// <summary>
        /// Gets or sets the field position
        /// </summary>
        /// <remarks>
        /// <para>In a delimited/CSV stream format, the position is the index of the field in the
        /// record starting at 0. For example, the position of field2 in the following
        /// comma delimited record is 1:</para>
        /// <example>field1,field2,field3</example>
        /// <para>In a fixed length stream format, the position is the index of the first character
        /// of the field in the record, also starting at 0.  For example, the position of field2
        /// in the following record is 6:</para>
        /// <example>field1field2field3</example>
        /// </remarks>
        public virtual int Position { get; set; }

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
        public virtual string Extract(UnmarshallingContext context, bool reportErrors)
        {
            var text = ExtractFieldText(context, reportErrors);

            if (Padding != null)
            {
                if (text == null)
                    return null;

                if (text == string.Empty)
                {
                    // this will either cause a required validation error or map
                    // to a null value depending on the value of 'required'
                    return string.Empty;
                }

                if (text.Length != Padding.Length)
                {
                    if (reportErrors)
                        context.AddFieldError(Name, text, "length", Padding.Length);
                    return Value.Invalid;
                }

                return Padding.Unpad(text);
            }

            return text;
        }

        /// <summary>
        /// Inserts a value into a record
        /// </summary>
        /// <remarks>
        /// <para>This method is called before type conversion.</para>
        /// <para>If the method returns false, type conversion is invoked and <see cref="IFieldFormat.InsertField"/>
        /// is called. If the method returns true, <see cref="IFieldFormat.InsertField"/>
        /// is not invoked.</para>
        /// </remarks>
        /// <param name="context">the <see cref="MarshallingContext"/></param>
        /// <param name="value">the value to insert into the record</param>
        /// <returns>true if type conversion is required and <see cref="IFieldFormat.InsertField"/> must be invoked, false otherwise</returns>
        public virtual bool InsertValue(MarshallingContext context, object value)
        {
            return false;
        }

        /// <summary>
        /// Inserts field text into a record
        /// </summary>
        /// <param name="context">the <see cref="MarshallingContext"/> holding the record</param>
        /// <param name="text">the field text to insert into the record</param>
        public virtual void InsertField(MarshallingContext context, string text)
        {
            var commit = text != null || !IsLazy;

            if (Padding != null)
            {
                text = Padding.Pad(text);
            }
            else if (text == null)
            {
                text = string.Empty;
            }

            InsertFieldText(context, text, commit);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>a string that represents the current object</returns>
        public override string ToString()
        {
            return string.Format(
                "{0}[at={1}{2}, {3}, {4}]",
                this,
                Position,
                Until != 0 ? string.Format(", until={0}", Until) : string.Empty,
                DebugUtil.FormatOption("optional", IsLazy),
                Padding.FormatPadding());
        }

        /// <summary>
        /// Inserts field text into a record
        /// </summary>
        /// <param name="context">the <see cref="MarshallingContext"/> holding the record</param>
        /// <param name="text">the field text to insert into the record</param>
        /// <param name="commit">true to commit the current record, or false
        /// if the field is optional and should not extend the record
        /// unless a subsequent field is later appended to the record</param>
        protected abstract void InsertFieldText(MarshallingContext context, string text, bool commit);

        /// <summary>
        /// Extracts the field text from a record.
        /// </summary>
        /// <param name="context">the <see cref="UnmarshallingContext"/> holding the record</param>
        /// <param name="reporting">report the errors?</param>
        /// <returns>the field text or null if the field was not present in the record</returns>
        protected abstract string ExtractFieldText(UnmarshallingContext context, bool reporting);
    }
}
