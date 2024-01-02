// <copyright file="XmlFieldFormat.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Text;
using System.Xml;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser.Format.Xml
{
    /// <summary>
    /// Base class for XML <see cref="IFieldFormat"/> implementations.
    /// </summary>
    internal abstract class XmlFieldFormat : IFieldFormat, IXmlNode
    {
        /// <summary>
        /// Gets the size of the field.
        /// </summary>
        /// <remarks>
        /// Fixed length formats should return the field length, while other formats should simply return 1.
        /// </remarks>
        public virtual int Size => 1;

        /// <summary>
        /// Gets the XML node type.
        /// </summary>
        public abstract XmlNodeType Type { get; }

        /// <summary>
        /// Gets the XML local name for this node.
        /// </summary>
        public abstract string? LocalName { get; }

        /// <summary>
        /// Gets the namespace of this node.  If there is no namespace for this
        /// node, or this node is not namespace aware, <see langword="null" /> is returned.
        /// </summary>
        public abstract string? Namespace { get; }

        /// <summary>
        /// Gets a value indicating whether a namespace was configured for this node,
        /// and is therefore used to unmarshal and marshal the node.
        /// </summary>
        public abstract bool IsNamespaceAware { get; }

        /// <summary>
        /// Gets the namespace prefix for marshaling this node, or <see langword="null" />
        /// if the namespace should override the default namespace.
        /// </summary>
        public abstract string? Prefix { get; }

        /// <summary>
        /// Gets a value indicating whether this node may repeat in the context of its immediate parent.
        /// </summary>
        public abstract bool IsRepeating { get; }

        /// <summary>
        /// Gets a value indicating whether this field is nillable.
        /// </summary>
        public abstract bool IsNillable { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this field is optionally present in the record.
        /// </summary>
        /// <remarks>
        /// TODO: Maybe rename isLazy to something better.
        /// </remarks>
        public virtual bool IsLazy { get; set; }

        /// <summary>
        /// Gets or sets the field padding.
        /// </summary>
        /// <remarks>
        /// null if the field text is not padded.
        /// </remarks>
        public virtual FieldPadding? Padding { get; set; }

        /// <summary>
        /// Gets or sets the field name.
        /// </summary>
        public virtual required string Name { get; set; }

        /// <summary>
        /// Extracts the field text from a record.
        /// </summary>
        /// <remarks>
        /// <para>May return <see cref="F:Value.Invalid"/> if the field is invalid, or <see cref="F:Value.Nil"/>
        /// if the field is explicitly set to nil or null such as in an XML formatted
        /// stream.</para>
        /// <para>Implementations should also remove any field padding before returning the text.</para>
        /// </remarks>
        /// <param name="context">the <see cref="UnmarshallingContext"/> holding the record.</param>
        /// <param name="reportErrors">report the errors?.</param>
        /// <returns>the field text or null if the field was not present in the record.</returns>
        public virtual string? Extract(UnmarshallingContext context, bool reportErrors)
        {
            var ctx = (XmlUnmarshallingContext)context;

            var fieldText = ExtractText(ctx);
            ctx.SetFieldText(Name, ReferenceEquals(fieldText, Value.Nil) ? null : fieldText);

            if (Padding != null && fieldText != null)
            {
                int length = fieldText.Length;
                if (length == 0)
                {
                    // this will either cause a required validation error or map
                    // to a null value depending on the value of 'required'
                    return string.Empty;
                }

                if (length != Padding.Length)
                {
                    if (reportErrors)
                    {
                        context.AddFieldError(Name, fieldText, "length", Padding.Length);
                    }

                    fieldText = Value.Invalid;
                }
                else
                {
                    fieldText = Padding.Unpad(fieldText);
                }
            }

            return fieldText;
        }

        /// <summary>
        /// Inserts a value into a record.
        /// </summary>
        /// <remarks>
        /// <para>This method is called before type conversion.</para>
        /// <para>If the method returns false, type conversion is invoked and <see cref="IFieldFormat.InsertField"/>
        /// is called. If the method returns true, <see cref="IFieldFormat.InsertField"/>
        /// is not invoked.</para>
        /// </remarks>
        /// <param name="context">the <see cref="MarshallingContext"/>.</param>
        /// <param name="value">the value to insert into the record.</param>
        /// <returns>true if type conversion is required and <see cref="IFieldFormat.InsertField"/> must be invoked, false otherwise.</returns>
        public virtual bool InsertValue(MarshallingContext context, object? value)
        {
            return false;
        }

        /// <summary>
        /// Inserts field text into a record.
        /// </summary>
        /// <param name="context">the <see cref="MarshallingContext"/> holding the record.</param>
        /// <param name="text">the field text to insert into the record.</param>
        public virtual void InsertField(MarshallingContext context, string? text)
        {
            var ctx = (XmlMarshallingContext)context;

            if (Padding != null && text != null)
                text = Padding.Pad(text);

            InsertText(ctx, text);
        }

        public override string ToString()
        {
            var s = new StringBuilder();
            s.AppendFormat("{0}[", GetType());
            ToParamString(s);
            s.Append("]");
            return s.ToString();
        }

        /// <summary>
        /// Inserts a field into the record during marshalling.
        /// </summary>
        /// <param name="context">the <see cref="XmlMarshallingContext"/> holding the record.</param>
        /// <param name="text">the field text to insert.</param>
        public abstract void InsertText(XmlMarshallingContext context, string? text);

        /// <summary>
        /// Extracts a field from a record during unmarshalling.
        /// </summary>
        /// <param name="context">the <see cref="XmlUnmarshallingContext"/> holding the record.</param>
        /// <returns>the extracted field text.</returns>
        protected abstract string? ExtractText(XmlUnmarshallingContext context);

        /// <summary>
        /// Called by <see cref="ToString"/> to append attributes of this field.
        /// </summary>
        /// <param name="s">the string builder to add the parameters to.</param>
        protected virtual void ToParamString(StringBuilder s)
        {
            s.Append(DebugUtil.FormatOption("optional", IsLazy));
        }
    }
}
