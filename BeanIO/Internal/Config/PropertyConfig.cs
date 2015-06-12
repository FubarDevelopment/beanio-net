using System.Collections.Generic;
using System.Xml;

using BeanIO.Internal.Util;

using Newtonsoft.Json.Linq;

namespace BeanIO.Internal.Config
{
    /// <summary>
    /// A base class for configuration components that can be bound to a property
    /// of a bean object.
    /// </summary>
    /// <remarks>
    /// <para>The following attributes are set during compilation, and are meant for internal use only:</para>
    /// <list type="bullet">
    ///     <item>minSize</item>
    ///     <item>maxSize</item>
    /// </list>
    /// </remarks>
    public abstract class PropertyConfig : ComponentConfig
    {
        /// <summary>
        /// Gets or sets the component name used for identification in error handling
        /// </summary>
        /// <remarks>
        /// Defaults to getName() if not set.
        /// </remarks>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the fully qualified class name or type alias of this property
        /// </summary>
        /// <remarks>
        /// By default, <code>null</code> is returned and the property value type
        /// is detected through bean introspection.
        /// </remarks>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the getter method for retrieving this property's
        /// value from its parent bean object during marshalling
        /// </summary>
        public string Getter { get; set; }

        /// <summary>
        /// Gets or sets the name of the setter method to use when setting this property's
        /// value on its parent bean object during unmarshalling.
        /// </summary>
        public string Setter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this property is bound to its parent bean object
        /// </summary>
        public bool IsBound { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the class assigned to this segment should only be instantiated
        /// if at least one child element is not null or the empty string, or if null should
        /// be returned for an empty collection.
        /// </summary>
        public bool IsLazy { get; set; }

        /// <summary>
        /// Gets or sets the position of this component
        /// </summary>
        /// <remarks>
        /// <para>A negative number is counted from the end of the record (e.g. -1 is
        /// the last field in the record).</para>
        /// <para>For delimited record formats, the position is the
        /// index (beginning at 0) of this component in the record.
        /// For fixed length record formats, the position is the index
        /// of the first character in the component.</para>
        /// <para>A negative number is counted from the end of the record.</para>
        /// </remarks>
        public int? Position { get; set; }

        /// <summary>
        /// Gets or sets the excluded maximum position of this component which may be
        /// specified for components that repeat indefinitely.
        /// </summary>
        /// <remarks>
        /// A negative number is counted from the end of the record.
        /// </remarks>
        public int? Until { get; set; }

        /// <summary>
        /// Gets or sets the collection type, or <code>null</code> if this component
        /// is not bound to a collection or array.
        /// </summary>
        public string Collection { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of times this component must appear in the stream.
        /// </summary>
        public int? MinOccurs { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of times this component may consecutively appear in a stream.
        /// </summary>
        /// <remarks>
        /// <para>If set to <code>null</code>, one occurrence is assumed.</para>
        /// <para>If set to any value greater than one, a collection type is expected.</para>
        /// <para>Must be greater than the minimum occurrences, or set to <code>-1</code> to indicate the limit is unbounded.</para>
        /// </remarks>
        public int? MaxOccurs { get; set; }

        /// <summary>
        /// Gets or sets the name of a field in the same record that indicates the number of occurrences for this component.
        /// </summary>
        public string OccursRef { get; set; }

        /// <summary>
        /// Gets or sets the minimum required value of the referenced occurs field.
        /// </summary>
        public int? MinOccursRef { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowed value of the referenced occurs field.
        /// </summary>
        public int? MaxOccursRef { get; set; }

        /// <summary>
        /// Gets or sets the XML node type of this component.
        /// </summary>
        public XmlNodeType? XmlType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this component is nillable.
        /// </summary>
        public bool IsNillable { get; set; }

        /// <summary>
        /// Gets or sets the JSON field name if different that the property name.
        /// </summary>
        /// <remarks>
        /// Ignored if its parent is a JSON array.
        /// </remarks>
        public string JsonName { get; set; }

        /// <summary>
        /// Gets or sets the JSON type.
        /// </summary>
        public JTokenType JsonType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the property is mapped to a JSON array.
        /// </summary>
        /// <remarks>
        /// Set internally by BeanIO.
        /// </remarks>
        public bool IsJsonArray { get; set; }

        /// <summary>
        /// Gets or sets the index of this property in its parent JSON array.
        /// </summary>
        public int? JsonArrayIndex { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this component is used to identify a record during
        /// unmarshalling or a bean during marshalling.
        /// </summary>
        /// <remarks>
        /// If this component is a record or segment, true is returned if any descendent is used for
        /// identification.
        /// </remarks>
        public bool IsIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the minimum size of this component (based on its field length
        /// or the field length of its descendants).
        /// </summary>
        public int MinSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum size of one occurrence of this component (based on its field length
        /// or the field length of its descendants).
        /// </summary>
        public int MaxSize { get; set; }

        /// <summary>
        /// Gets a value indicating whether this component is bound to a collection or array.
        /// </summary>
        public bool IsCollection
        {
            get { return Collection != null; }
        }

        /// <summary>
        /// Gets a value indicating whether this component repeats in a stream.
        /// </summary>
        /// <remarks>
        /// The component is assumed to repeat if bound to a collection or the maximum
        /// occurrences is greater than one.
        /// </remarks>
        public bool IsRepeating
        {
            get { return IsCollection || (MaxOccurs != null && MaxOccurs > 1); }
        }

        /// <summary>
        /// Gets the name of the property descendant to use for the Map key when
        /// <see cref="Collection"/> is set to <see cref="Dictionary{TKey,TValue}"/> (map).
        /// </summary>
        public virtual string Key
        {
            get { return null; }
        }

        /// <summary>
        /// Returns whether a node is a supported child of this node.
        /// </summary>
        /// <remarks>
        /// Called by <see cref="TreeNode{T}.Add"/>.
        /// </remarks>
        /// <param name="child">the node to test</param>
        /// <returns>true if the child is allowed</returns>
        public override bool IsSupportedChild(ComponentConfig child)
        {
            return false;
        }
    }
}
