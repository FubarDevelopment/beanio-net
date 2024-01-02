// <copyright file="MethodReflectionAccessor.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Reflection;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser.Accessor
{
    internal class MethodReflectionAccessor : PropertyAccessorSupport
    {
        private readonly MethodInfo _getter;
        private readonly MethodInfo _setter;

        public MethodReflectionAccessor(MethodInfo getter, MethodInfo setter, int? constructorArgumentIndex)
        {
            ConstructorArgumentIndex = constructorArgumentIndex;
            _getter = getter;
            _setter = setter;
        }

        /// <summary>
        /// Returns the property value from a bean object.
        /// </summary>
        /// <param name="bean">the bean object to get the property from.</param>
        /// <returns>the property value.</returns>
        public override object? GetValue(object bean)
        {
            if (_getter == null)
                throw new BeanIOException($"There is no getter defined on bean class '{bean.GetType().GetAssemblyQualifiedName()}'");

            try
            {
                return _getter.Invoke(bean, null);
            }
            catch (Exception ex)
            {
                throw new BeanIOException(
                    $"Failed to invoke method '{_getter.Name}' on bean class '{bean.GetType().GetAssemblyQualifiedName()}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Sets the property value on a bean object.
        /// </summary>
        /// <param name="bean">the bean object to set the property.</param>
        /// <param name="value">the property value.</param>
        public override void SetValue(object bean, object? value)
        {
            if (_setter == null)
                throw new BeanIOException($"There is no setter defined on bean class '{bean.GetType().GetAssemblyQualifiedName()}'");

            try
            {
                _setter.Invoke(bean, new[] { value });
            }
            catch (Exception ex)
            {
                throw new BeanIOException(
                    $"Failed to invoke method '{_setter.Name}' on bean class '{bean.GetType().GetAssemblyQualifiedName()}': {ex.Message}", ex);
            }
        }
    }
}
