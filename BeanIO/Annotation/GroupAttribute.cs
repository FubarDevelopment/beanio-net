using System;
using System.Xml;

namespace BeanIO.Annotation
{
    /// <summary>
    /// Group annotation for classes, and for fields and methods in a class annotated by a parent Group.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class GroupAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupAttribute" /> class.
        /// </summary>
        /// <param name="name">The group name.</param>
        public GroupAttribute(string name)
        {
            Name = name;
            MinOccurs = MaxOccurs = int.MinValue;
            Order = int.MinValue;
            XmlType = XmlNodeType.None;
        }

        /// <summary>
        /// Gets the group name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the group type, if it cannot be determined from the annotated field or method declaration.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets the collection type for repeating group bound to a parent group class, if
        /// it cannot be determined from the annotated field or method declaration.
        /// </summary>
        public Type CollectionType { get; set; }

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
