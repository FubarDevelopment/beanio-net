// <copyright file="PropertyComponent.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Text;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser
{
    internal abstract class PropertyComponent : Component, IProperty
    {
        protected static readonly bool CreateMissingBeans = Settings.Instance.GetBoolean(Settings.CREATE_MISSING_BEANS);

        /// <summary>
        /// Gets or sets a value indicating whether this property should always be instantiated when
        /// <see cref="CreateValue"/> is invoked.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this bean can still identify a record if null.
        /// </summary>
        public bool IsMatchNull { get; set; }

        /// <summary>
        /// Gets the <see cref="IProperty"/> implementation type
        /// </summary>
        public abstract PropertyType Type { get; }

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
        /// Clears the property value.
        /// </summary>
        /// <remarks>
        /// A subsequent call to <see cref="IProperty.GetValue"/> should return null, or <see cref="F:Value.Missing"/> for lazy property values.
        /// </remarks>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        public abstract void ClearValue(ParsingContext context);

        /// <summary>
        /// Creates the property value and returns it
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <returns>the property value</returns>
        public abstract object CreateValue(ParsingContext context);

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
        public abstract object GetValue(ParsingContext context);

        /// <summary>
        /// Sets the property value (before marshalling).
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <param name="value">the property value</param>
        public abstract void SetValue(ParsingContext context, object value);

        public abstract bool Defines(object value);

        /// <summary>
        /// Returns whether a node is a supported child of this node.
        /// </summary>
        /// <remarks>
        /// Called by <see cref="TreeNode{T}.Add"/>.
        /// </remarks>
        /// <param name="child">the node to test</param>
        /// <returns>true if the child is allowed</returns>
        public override bool IsSupportedChild(Component child)
        {
            return child is IProperty;
        }

        /// <summary>
        /// Called by <see cref="TreeNode{T}.ToString"/> to append node parameters to the output
        /// </summary>
        /// <param name="s">The output to append</param>
        protected override void ToParamString(StringBuilder s)
        {
            base.ToParamString(s);
            if (PropertyType != null)
                s.AppendFormat(", type={0}", Type);
            s.AppendFormat(", {0}", DebugUtil.FormatOption("rid", IsIdentifier))
             .AppendFormat(", {0}", DebugUtil.FormatOption("required", IsRequired));
        }
    }
}
