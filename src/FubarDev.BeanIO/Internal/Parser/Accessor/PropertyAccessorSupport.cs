// <copyright file="PropertyAccessorSupport.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BeanIO.Internal.Parser.Accessor
{
    internal abstract class PropertyAccessorSupport : IPropertyAccessor
    {
        private int? _constructorArgumentIndex;

        /// <summary>
        /// Gets a value indicating whether this property is a constructor argument.
        /// </summary>
        public bool IsConstructorArgument => ConstructorArgumentIndex != null;

        /// <summary>
        /// Gets or sets the constructor argument index, or null if this property is
        /// not a constructor argument.
        /// </summary>
        public int? ConstructorArgumentIndex
        {
            get => _constructorArgumentIndex;
            set => _constructorArgumentIndex = (value != null && value == -1) ? null : value;
        }

        /// <summary>
        /// Returns the property value from a bean object.
        /// </summary>
        /// <param name="bean">the bean object to get the property from.</param>
        /// <returns>the property value.</returns>
        public abstract object? GetValue(object bean);

        /// <summary>
        /// Sets the property value on a bean object.
        /// </summary>
        /// <param name="bean">the bean object to set the property.</param>
        /// <param name="value">the property value.</param>
        public abstract void SetValue(object bean, object? value);
    }
}
