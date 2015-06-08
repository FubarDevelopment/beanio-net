using System;
using System.Collections.Generic;
using System.Xml;

namespace BeanIO.Annotation
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class SegmentAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentAttribute" /> class.
        /// </summary>
        public SegmentAttribute()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentAttribute" /> class.
        /// </summary>
        /// <param name="name">The segment name.</param>
        public SegmentAttribute(string name)
        {
            Name = name;
            At = Until = Ordinal = int.MinValue;
            MinOccurs = MaxOccurs = int.MinValue;
            XmlType = XmlNodeType.None;
        }

        /// <summary>
        /// Gets the segment name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the absolute position of the segment.
        /// </summary>
        public int At { get; set; }

        /// <summary>
        /// Gets or sets the maximum position of a segment that repeats for an indeterminate number of times.
        /// </summary>
        public int Until { get; set; }

        /// <summary>
        /// Gets or sets the relative position of the segment.
        /// </summary>
        public int Ordinal { get; set; }

        /// <summary>
        /// Gets or sets the getter method.
        /// </summary>
        public string Getter { get; set; }

        /// <summary>
        /// Gets or sets the setter method.
        /// </summary>
        public string Setter { get; set; }

        /// <summary>
        /// Gets or sets the class bound to this segment, if one cannot be derived from the annotated field or method.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the class bound to this segment should be instantiated
        /// if all child fields are null or empty strings.
        /// </summary>
        /// <remarks>
        /// Also causes empty collections to returned as null.
        /// </remarks>
        public bool IsLazy { get; set; }

        /// <summary>
        /// Gets or sets the collection class bound to this segment, if one cannot be derived
        /// from the annotated field or method.
        /// </summary>
        public Type CollectionType { get; set; }

        /// <summary>
        /// Gets or sets the minimum occurrences.
        /// </summary>
        public int MinOccurs { get; set; }

        /// <summary>
        /// Gets or sets the maximum occurrences.
        /// </summary>
        public int MaxOccurs { get; set; }

        /// <summary>
        /// Gets or sets the name of a preceding field that governs the number of occurrences of this segment.
        /// </summary>
        /// <remarks>
        /// Does not apply to XML formatted streams.
        /// </remarks>
        public string OccursRef { get; set; }

        /// <summary>
        /// Gets or sets the name of a child component to use for the key value if
        /// this segment is bound to a <see cref="Dictionary{TKey,TValue}"/>.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the name of a child component to use for the value of this segment in lieu of a type.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the XML type of this segment.
        /// </summary>
        public XmlNodeType XmlType { get; set; }

        /// <summary>
        /// Gets or sets the XML attribute or element name.
        /// </summary>
        public string XmlName { get; set; }

        /// <summary>
        /// Gets or sets the XML namespace prefix of this segment.
        /// </summary>
        public string XmlPrefix { get; set; }

        /// <summary>
        /// Gets or sets the XML namespace URI of this segment.
        /// </summary>
        public string XmlNamespace { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the element is nillable.
        /// </summary>
        public bool IsNullable { get; set; }
    }
}
