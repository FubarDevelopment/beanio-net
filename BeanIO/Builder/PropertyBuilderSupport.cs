// <copyright file="PropertyBuilderSupport.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Xml;

using BeanIO.Internal.Config;
using BeanIO.Internal.Util;

namespace BeanIO.Builder
{
    /// <summary>
    /// Support for property configuration builders
    /// </summary>
    /// <remarks>Methods may throw a <see cref="BeanIOConfigurationException" /> if an
    /// invalid setting is configured.</remarks>
    /// <typeparam name="T">The builder subclass</typeparam>
    /// <typeparam name="TConfig">The component configuration subclass</typeparam>
    public abstract class PropertyBuilderSupport<T, TConfig>
        where T : PropertyBuilderSupport<T, TConfig>
        where TConfig : PropertyConfig
    {
        /// <summary>
        /// Gets this.
        /// </summary>
        protected abstract T Me { get; }

        /// <summary>
        /// Gets the configuration settings.
        /// </summary>
        protected abstract TConfig Config { get; }

        /// <summary>
        /// Sets the minimum occurrences of this component.
        /// </summary>
        /// <param name="min">The minimum occurrences</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public T MinOccurs(int min)
        {
            Config.MinOccurs = min;
            return Me;
        }

        /// <summary>
        /// Sets the maximum occurrences of this component.
        /// </summary>
        /// <param name="max">The maximum occurrences</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public T MaxOccurs(int max)
        {
            Config.MaxOccurs = max < 0 ? int.MaxValue : max;
            return Me;
        }

        /// <summary>
        /// Sets the exact occurrences of this component.
        /// </summary>
        /// <param name="n">The number of occurrences</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public T Occurs(int n)
        {
            return Occurs(n, n);
        }

        /// <summary>
        /// Sets the minimum and maximum occurrences of this component.
        /// </summary>
        /// <param name="min">The minimum occurrences</param>
        /// <param name="max">The maximum occurrences</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public T Occurs(int min, int max)
        {
            return MinOccurs(min)
                .MaxOccurs(max);
        }

        /// <summary>
        /// Sets the class bound to this component.
        /// </summary>
        /// <param name="type">Type name</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public virtual T Type(Type type)
        {
            Config.Type = type.GetAssemblyQualifiedName();
            return Me;
        }

        /// <summary>
        /// Sets the collection type bound to this component.
        /// </summary>
        /// <param name="type">Collection or map type name</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public virtual T Collection(Type type)
        {
            Config.Collection = type.GetAssemblyQualifiedName();
            return Me;
        }

        /// <summary>
        /// Sets the getter method for getting this component from its parent.
        /// </summary>
        /// <param name="getter">The getter method name</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public T Getter(string getter)
        {
            Config.Getter = getter;
            return Me;
        }

        /// <summary>
        /// Sets the setter method for setting this component on its parent.
        /// </summary>
        /// <param name="setter">he setter method name</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public T Setter(string setter)
        {
            Config.Setter = setter;
            return Me;
        }

        /// <summary>
        /// Indicates this component should not be instantiated if this component
        /// or all of its children are null or the empty String.
        /// </summary>
        /// <returns>The value of <see cref="Me"/></returns>
        public T Lazy()
        {
            Config.IsLazy = true;
            return Me;
        }

        /// <summary>
        /// Sets the default validation mode for fields during marshalling.
        /// </summary>
        /// <returns>The value of <see cref="Me"/></returns>
        public T Validate()
        {
            return Validate(true);
        }

        /// <summary>
        /// Sets the default validation mode for fields during marshalling.
        /// </summary>
        /// <param name="validate">true to enable field validation during marshalling</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public T Validate(bool validate)
        {
            Config.ValidateOnMarshal = validate;
            return Me;
        }

        /// <summary>
        /// Sets the XML type of this component.
        /// </summary>
        /// <param name="xmlType">The <see cref="XmlNodeType"/>.</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public T XmlType(XmlNodeType xmlType)
        {
            Config.XmlType = xmlType;
            return Me;
        }

        /// <summary>
        /// Sets the XML namespace prefix.
        /// </summary>
        /// <param name="xmlPrefix">The prefix</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public T XmlPrefix(string xmlPrefix)
        {
            Config.XmlPrefix = xmlPrefix;
            return Me;
        }

        /// <summary>
        /// Sets the XML element or attribute name.
        /// </summary>
        /// <param name="xmlName">The name</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public T XmlName(string xmlName)
        {
            Config.XmlName = xmlName;
            return Me;
        }

        /// <summary>
        /// Sets the XML namespace.
        /// </summary>
        /// <param name="xmlNamespace">The namespace</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public T XmlNamespace(string xmlNamespace)
        {
            Config.XmlNamespace = xmlNamespace;
            return Me;
        }
    }
}
