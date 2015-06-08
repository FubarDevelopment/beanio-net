using System;
using System.Xml;

namespace BeanIO.Annotation
{
    /// <summary>
    /// Record annotation for classes, and for fields and methods in a class annotated by a parent Group.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class RecordAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecordAttribute" /> class.
        /// </summary>
        public RecordAttribute()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordAttribute" /> class.
        /// </summary>
        /// <param name="name">The record name.</param>
        public RecordAttribute(string name)
        {
            Name = name;
            MinRecordIdentificationLength = MaxRecordIdentificationLength = int.MinValue;
            MinLength = MaxLength = int.MinValue;
            MinOccurs = MaxOccurs = int.MinValue;
            Order = int.MinValue;
            XmlType = XmlNodeType.None;
        }

        /// <summary>
        /// Gets the record name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the record type, if it cannot be determined from the annotated field or method declaration.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets the collection type for repeating records bound to a group class, if
        /// it cannot be determined from the annotated field or method declaration.
        /// </summary>
        public Type CollectionType { get; set; }

        /// <summary>
        /// Gets or sets the minimum length of this record used to identify it.
        /// </summary>
        public int MinRecordIdentificationLength { get; set; }

        /// <summary>
        /// Gets or sets the maximum length of this record used to identify it.
        /// </summary>
        public int MaxRecordIdentificationLength { get; set; }

        /// <summary>
        /// Gets or sets the validated minimum length of the record.
        /// </summary>
        public int MinLength { get; set; }

        /// <summary>
        /// Gets or sets the validated maximum length of the record.
        /// </summary>
        public int MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the minimum occurrences of the record.
        /// </summary>
        public int MinOccurs { get; set; }

        /// <summary>
        /// Gets or sets the maximum occurrences of the record.
        /// </summary>
        public int MaxOccurs { get; set; }

        /// <summary>
        /// Gets or sets the order of this record within its parent group.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the name of child component to use for the value of this record in
        /// lieu of a type.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the XML type of this record.
        /// </summary>
        public XmlNodeType XmlType { get; set; }

        /// <summary>
        /// Gets or sets the XML attribute or element name.
        /// </summary>
        public string XmlName { get; set; }

        /// <summary>
        /// Gets or sets the XML namespace prefix of this record.
        /// </summary>
        public string XmlPrefix { get; set; }

        /// <summary>
        /// Gets or sets the XML namespace URI of this record.
        /// </summary>
        public string XmlNamespace { get; set; }
    }
}
