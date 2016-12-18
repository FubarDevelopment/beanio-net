// <copyright file="CollectionBean.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using BeanIO.Config;
using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser
{
    internal class CollectionBean : PropertyComponent
    {
        private readonly ParserLocal<object> _bean;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionBean"/> class.
        /// </summary>
        /// <param name="settings">The configuration settings</param>
        public CollectionBean(ISettings settings)
            : base(settings)
        {
            _bean = new BeanParserLocal(this);
        }

        /// <summary>
        /// Gets the <see cref="IProperty"/> implementation type
        /// </summary>
        public override PropertyType Type => Parser.PropertyType.Collection;

        /// <summary>
        /// Clears the property value.
        /// </summary>
        /// <remarks>
        /// A subsequent call to <see cref="IProperty.GetValue"/> should return null, or <see cref="F:Value.Missing"/> for lazy property values.
        /// </remarks>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        public override void ClearValue(ParsingContext context)
        {
            foreach (var child in Children.Cast<IProperty>())
                child.ClearValue(context);
            _bean.Set(context, IsRequired ? null : Value.Missing);
        }

        /// <summary>
        /// Creates the property value and returns it
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <returns>the property value</returns>
        public override object CreateValue(ParsingContext context)
        {
            object b = null;
            foreach (var child in Children)
            {
                var property = (IProperty)child;
                var value = property.GetValue(context);
                if (CreateMissingBeans && ReferenceEquals(value, Value.Missing))
                    value = property.CreateValue(context);

                if (ReferenceEquals(value, Value.Invalid))
                {
                    _bean.Set(context, b);
                    return Value.Invalid;
                }

                if (ReferenceEquals(value, Value.Missing))
                {
                    if (b == null)
                        continue;
                    value = null;
                }

                if (b == null)
                {
                    b = NewInstance();
                    Backfill((IList)b, child);
                }

                ((IList)b).Add(value);
            }

            if (b == null)
                b = IsRequired || CreateMissingBeans ? NewInstance() : Value.Missing;

            _bean.Set(context, b);

            return b;
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
        public override object GetValue(ParsingContext context)
        {
            return _bean.Get(context);
        }

        /// <summary>
        /// Sets the property value (before marshalling).
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <param name="value">the property value</param>
        public override void SetValue(ParsingContext context, object value)
        {
            if (value == null)
            {
                ClearValue(context);
                return;
            }

            _bean.Set(context, value);

            var iterFinished = false;
            var iter = ((IEnumerable)value).GetEnumerator();
            foreach (var child in Children.Cast<IProperty>())
            {
                object childValue = null;
                if (!iterFinished)
                {
                    if (iter.MoveNext())
                    {
                        childValue = iter.Current;
                    }
                    else
                    {
                        iterFinished = true;
                    }
                }

                child.SetValue(context, childValue);
            }
        }

        public override bool Defines(object value)
        {
            if (PropertyType == null)
                return false;
            if (value == null)
                return IsMatchNull;
            if (!PropertyType.IsAssignableFromThis(value.GetType()))
                return false;
            if (!IsIdentifier)
                return true;

            // check identifying properties
            var iterFinished = false;
            var iter = ((IEnumerable)value).GetEnumerator();
            foreach (var property in Children.Cast<IProperty>())
            {
                object childValue = null;
                if (!iterFinished)
                {
                    if (iter.MoveNext())
                    {
                        childValue = iter.Current;
                    }
                    else
                    {
                        iterFinished = true;
                    }
                }

                // if the child property is not used to identify records, no need to go further
                if (!property.IsIdentifier)
                    continue;

                if (!property.Defines(childValue))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Called by a stream to register variables stored in the parsing context.
        /// </summary>
        /// <remarks>
        /// This method should be overridden by subclasses that need to register
        /// one or more parser context variables.
        /// </remarks>
        /// <param name="locals">set of local variables</param>
        public override void RegisterLocals(ISet<IParserLocal> locals)
        {
            if (locals.Add(_bean))
                base.RegisterLocals(locals);
        }

        /// <summary>
        /// Creates a new instance of this bean object
        /// </summary>
        /// <returns>the new bean object</returns>
        protected virtual object NewInstance()
        {
            return PropertyType.Instantiate(null).NewInstance();
        }

        private void Backfill(IList collection, Component to)
        {
            var count = Children.TakeWhile(x => !ReferenceEquals(x, to)).Count();
            for (var i = 0; i != count; ++i)
                collection.Add(null);
        }

        private class BeanParserLocal : ParserLocal<object>
        {
            private readonly PropertyComponent _owner;

            public BeanParserLocal(PropertyComponent owner)
            {
                _owner = owner;
            }

            /// <summary>
            /// Called when initialized to return a default value.  If not overridden,
            /// it returns the default value passed via the constructor.
            /// </summary>
            /// <returns>the default value</returns>
            protected override object CreateDefaultValue()
            {
                return _owner.IsRequired ? null : Value.Missing;
            }
        }
    }
}
