using System;
using System.Reflection;

using BeanIO.Internal.Parser;

namespace BeanIO.Internal.Compiler
{
    /// <summary>
    /// Factory interface for creating <see cref="IPropertyAccessor"/> implementations.
    /// </summary>
    public interface IPropertyAccessorFactory
    {
        /// <summary>
        /// Creates a new <see cref="IPropertyAccessor"/>
        /// </summary>
        /// <param name="parent">the parent bean object type</param>
        /// <param name="property">the property to access</param>
        /// <param name="carg">the constructor argument index</param>
        /// <returns>the new <see cref="IPropertyAccessor"/></returns>
        IPropertyAccessor CreatePropertyAccessor(Type parent, PropertyInfo property, int carg);

        /// <summary>
        /// Creates a new <see cref="IPropertyAccessor"/>
        /// </summary>
        /// <param name="parent">the parent bean object type</param>
        /// <param name="field">the field to access</param>
        /// <param name="carg">the constructor argument index</param>
        /// <returns>the new <see cref="IPropertyAccessor"/></returns>
        IPropertyAccessor CreatePropertyAccessor(Type parent, FieldInfo field, int carg);
    }
}
