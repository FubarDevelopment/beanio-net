using System;
using System.Reflection;

using BeanIO.Internal.Parser;
using BeanIO.Internal.Parser.Accessor;

namespace BeanIO.Internal.Compiler.Accessor
{
    /// <summary>
    /// <see cref="IPropertyAccessorFactory"/> implementations based on .NET reflection
    /// </summary>
    public class ReflectionAccessorFactory : IPropertyAccessorFactory
    {
        /// <summary>
        /// Creates a new <see cref="IPropertyAccessor"/>
        /// </summary>
        /// <param name="parent">the parent bean object type</param>
        /// <param name="property">the property to access</param>
        /// <param name="carg">the constructor argument index</param>
        /// <returns>the new <see cref="IPropertyAccessor"/></returns>
        public IPropertyAccessor CreatePropertyAccessor(Type parent, PropertyInfo property, int carg)
        {
            var accessor = new PropertyReflectionAccessor(property, null, null, carg);
            return accessor;
        }

        /// <summary>
        /// Creates a new <see cref="IPropertyAccessor"/>
        /// </summary>
        /// <param name="parent">the parent bean object type</param>
        /// <param name="field">the field to access</param>
        /// <param name="carg">the constructor argument index</param>
        /// <returns>the new <see cref="IPropertyAccessor"/></returns>
        public IPropertyAccessor CreatePropertyAccessor(Type parent, FieldInfo field, int carg)
        {
            var accessor = new FieldReflectionAccessor(field, carg);
            return accessor;
        }
    }
}
