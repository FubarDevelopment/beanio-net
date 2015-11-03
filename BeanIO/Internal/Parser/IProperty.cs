using System;

using JetBrains.Annotations;

namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// The <see cref="IProperty"/> interface is implemented by parser components capable
    /// of storing a property value.
    /// </summary>
    internal interface IProperty
    {
        /// <summary>
        /// Gets the <see cref="IProperty"/> implementation type
        /// </summary>
        PropertyType Type { get; }

        /// <summary>
        /// Gets the property name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this property or any of its descendants are used to identify a bean object.
        /// </summary>
        bool IsIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the property accessor
        /// </summary>
        IPropertyAccessor Accessor { get; set; }

        /// <summary>
        /// Gets or sets the bean property type
        /// </summary>
        Type PropertyType { get; set; }

        /// <summary>
        /// Clears the property value.
        /// </summary>
        /// <remarks>
        /// A subsequent call to <see cref="GetValue"/> should return null, or <see cref="F:Value.Missing"/> for lazy property values.
        /// </remarks>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        void ClearValue(ParsingContext context);

        /// <summary>
        /// Creates the property value and returns it
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <returns>the property value</returns>
        object CreateValue(ParsingContext context);

        /// <summary>
        /// Returns the value of this property
        /// </summary>
        /// <remarks>
        /// <para>When unmarshalling, this method should return <see cref="F:Value.Missing"/> if the field
        /// was not present in the stream.  Or if present, but has no value, null should be returned.</para>
        /// <para>When marshalling, this method should return <see cref="F:Value.Missing"/> for any optional
        /// segment bound to a bean object, or null if required. Null field properties should
        /// always return <see cref="F:Value.Missing"/>.</para>
        /// </remarks>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <returns>the property value,
        /// or <see cref="F:Value.Missing"/> if not present in the stream,
        /// or <see cref="F:Value.Invalid"/> if the field was invalid</returns>
        object GetValue(ParsingContext context);

        /// <summary>
        /// Sets the property value (before marshalling).
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <param name="value">the property value</param>
        void SetValue(ParsingContext context, [CanBeNull] object value);

        bool Defines(object value);
    }
}
