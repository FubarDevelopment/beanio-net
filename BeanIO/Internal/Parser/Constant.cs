using System;
using System.Collections.Generic;

namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// A simple property implementation that stores a constant value.
    /// </summary>
    public class Constant : Component, IProperty
    {
        /// <summary>
        /// Gets the <see cref="IProperty"/> implementation type
        /// </summary>
        public PropertyType Type
        {
            get { return Parser.PropertyType.Simple; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this property or any of its descendants are used to identify a bean object.
        /// </summary>
        public bool IsIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the property accessor
        /// </summary>
        public IPropertyAccessor Accessor { get; set; }

        /// <summary>
        /// Gets or sets the bean property type
        /// </summary>
        public Type PropertyType { get; set; }

        /// <summary>
        /// Gets or sets the constant value
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Clears the property value.
        /// </summary>
        /// <remarks>
        /// A subsequent call to <see cref="IProperty.GetValue"/> should return null, or <see cref="F:Value.Missing"/> for lazy property values.
        /// </remarks>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        public void ClearValue(ParsingContext context)
        {
        }

        /// <summary>
        /// Creates the property value and returns it
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <returns>the property value</returns>
        public object CreateValue(ParsingContext context)
        {
            return GetValue(context);
        }

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
        public object GetValue(ParsingContext context)
        {
            return Value;
        }

        /// <summary>
        /// Sets the property value (before marshalling).
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <param name="value">the property value</param>
        public void SetValue(ParsingContext context, object value)
        {
            Value = value;
        }

        public bool Defines(object value)
        {
            if (ReferenceEquals(Value, value))
                return true;
            return Value.Equals(value);
        }
    }
}