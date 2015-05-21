using System;
using System.Xml;

using BeanIO.Builder;
using BeanIO.Types;

namespace BeanIO.Annotation
{
    /// <summary>
    /// Field annotation applied to fields, properties, methods or constructor parameters.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property)]
    public class FieldAttribute : Attribute
    {
        public FieldAttribute(string name)
        {
            Name = name;
            At = int.MinValue;
            Until = int.MinValue;
            Ordinal = int.MinValue;
            Length = int.MinValue;
            Padding = int.MinValue;
            MinLength = MaxLength = int.MinValue;
            MinOccurs = MaxOccurs = int.MinValue;
            XmlType = XmlNodeType.None;
        }

        /// <summary>
        /// Gets the field name.
        /// </summary>
        [Field("Test", Padding = 'a')]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the absolute position of the field.
        /// </summary>
        public int At { get; set; }

        /// <summary>
        /// Gets or sets the maximum position of a field that repeats for an indeterminate number of times.
        /// </summary>
        public int Until { get; set; }

        /// <summary>
        /// Gets or sets the relative position of the field.
        /// </summary>
        public int Ordinal { get; set; }

        /// <summary>
        /// Gets or sets the padded length of the field.
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Gets or sets the character used to pad the field.
        /// </summary>
        public int Padding { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to keep the field padding during unmarshalling.
        /// </summary>
        /// <remarks>
        /// Only applies to fixed length formatted streams.
        /// </remarks>
        public bool KeepPadding { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enforce the padding length during unmarshalling.
        /// </summary>
        /// <remarks>
        /// Only applies to fixed length formatted streams.
        /// </remarks>
        public bool LenientPadding { get; set; }

        /// <summary>
        /// Gets or sets the alignment of a padded field.
        /// </summary>
        public Align Align { get; set; }

        /// <summary>
        /// Gets or sets the getter method.
        /// </summary>
        public string Getter { get; set; }

        /// <summary>
        /// Gets or sets the setter method.
        /// </summary>
        public string Setter { get; set; }

        /// <summary>
        /// Gets or sets the field type, if it can not be detected from the method or field declaration.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ITypeHandler" /> implementation class for this field.
        /// </summary>
        public Type HandlerType { get; set; }

        /// <summary>
        /// Gets or sets the name of a registered <see cref="ITypeHandler" />.
        /// </summary>
        public string HandlerName { get; set; }

        /// <summary>
        /// Gets or sets the format passed to the <see cref="ITypeHandler" />.
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to trim the field text before validation and type handling.
        /// </summary>
        public bool Trim { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the field is used to identify the record.
        /// </summary>
        public bool IsRecordIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the regular expression for validating and/or matching field text.
        /// </summary>
        public string RegEx { get; set; }

        /// <summary>
        /// Gets or sets the literal text for validating or matching field text.
        /// </summary>
        public string Literal { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this field text is required.
        /// </summary>
        /// <remarks>
        /// true if field text must be at least one character (after trimming if enabled), or false otherwise.
        /// </remarks>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the default value for this field.
        /// </summary>
        /// <remarks>
        /// The value is parsed into a Java object using the assigned type handler.
        /// </remarks>
        public string DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the minimum length of the field text (after trimming if enabled).
        /// </summary>
        public int MinLength { get; set; }

        /// <summary>
        /// Gets or sets the maximum length of the field text (after trimming if enabled).
        /// </summary>
        public int MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the collection type for repeating fields, if it cannot be detected from the field or method declaration.
        /// </summary>
        public Type CollectionType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an empty string should be converted to null, or null
        /// returned for an empty collection.
        /// </summary>
        public bool IsLazy { get; set; }

        /// <summary>
        /// Gets or sets the minimum occurrences of the field.
        /// </summary>
        public int MinOccurs { get; set; }

        /// <summary>
        /// Gets or sets the maximum occurrences of the field if it repeats.
        /// </summary>
        public int MaxOccurs { get; set; }

        /// <summary>
        /// Gets or sets the name of a preceding field that governs the number of occurrences of this field.
        /// </summary>
        /// <remarks>
        /// Does not apply to XML formatted streams.
        /// </remarks>
        public string OccursRef { get; set; }

        /// <summary>
        /// Gets or sets the XML type of this field.
        /// </summary>
        public XmlNodeType XmlType { get; set; }

        /// <summary>
        /// Gets or sets the XML attribute or element name.
        /// </summary>
        public string XmlName { get; set; }

        /// <summary>
        /// Gets or sets the XML namespace prefix of this field.
        /// </summary>
        public string XmlPrefix { get; set; }

        /// <summary>
        /// Gets or sets the XML namespace URI of this field.
        /// </summary>
        public string XmlNamespace { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the element is nillable.
        /// </summary>
        public bool IsNullable { get; set; }
    }
}
