using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// A component used to aggregate <see cref="IProperty"/>'s into a bean object, which
    /// may also be a property of a parent bean object itself.
    /// </summary>
    internal class Bean : PropertyComponent
    {
        /// <summary>
        /// the constructor for creating this bean object (if null, the default constructor is used)
        /// </summary>
        private readonly ParserLocal<object> _bean;

        /// <summary>
        /// used to temporarily hold constructor argument values when a constructor is specified
        /// </summary>
        private readonly ParserLocal<object[]> _constructorArgs;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bean"/> class.
        /// </summary>
        public Bean()
        {
            _bean = new BeanParserLocal(this);
            _constructorArgs = new ConstructorArgsParserLocal(this);
        }

        /// <summary>
        /// Gets or sets the <see cref="ConstructorInfo"/> used to instantiate this
        /// bean object, or null if the default constructor is used.
        /// </summary>
        public ConstructorInfo Constructor { get; set; }

        public bool IsLazy { get; set; }

        /// <summary>
        /// Gets the <see cref="IProperty"/> implementation type
        /// </summary>
        public override PropertyType Type
        {
            get { return IsMap ? Parser.PropertyType.Map : Parser.PropertyType.Complex; }
        }

        /// <summary>
        /// Gets a value indicating whether the bean object implements <see cref="IDictionary"/>
        /// </summary>
        protected virtual bool IsMap
        {
            get { return PropertyType.IsMap(); }
        }

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
            var hasProperties = false;

            // populate constructor arguments first
            if (Constructor != null)
            {
                // lazily create...
                var create = false;

                var cargs = _constructorArgs.Get(context);
                foreach (var property in Children.Cast<IProperty>())
                {
                    var accessor = property.Accessor;
                    if (accessor == null)
                        throw new InvalidOperationException(string.Format("Accessor not set for property value '{0}'", property.Name));
                    if (!accessor.IsConstructorArgument)
                        continue;

                    var value = property.GetValue(context);
                    if (ReferenceEquals(value, Value.Invalid))
                        return Value.Invalid;

                    if (ReferenceEquals(value, Value.Missing))
                    {
                        value = CreateMissingBeans ? property.CreateValue(context) : null;
                    }
                    else
                    {
                        hasProperties = true;
                        create = create || !IsLazy || StringUtil.HasValue(value);
                    }

                    Debug.Assert(accessor.ConstructorArgumentIndex != null, "accessor.ConstructorArgumentIndex != null");
                    cargs[accessor.ConstructorArgumentIndex.Value] = value;
                }

                if (create)
                    b = NewInstance(context);
            }

            foreach (var child in Children)
            {
                var property = (IProperty)child;
                if (property.Accessor.IsConstructorArgument)
                    continue;
                var value = property.GetValue(context);
                if (CreateMissingBeans && ReferenceEquals(value, Value.Missing))
                    value = property.CreateValue(context);

                if (ReferenceEquals(value, Value.Invalid))
                {
                    _bean.Set(context, b);
                    return Value.Invalid;
                }

                // explicitly null values must still be set on the bean...
                if (!ReferenceEquals(value, Value.Missing))
                {
                    hasProperties = true;
                    if (b == null)
                    {
                        if (IsLazy)
                        {
                            if (!StringUtil.HasValue(value))
                                continue;

                            b = NewInstance(context);
                            Backfill(context, b, child);
                        }
                        else
                        {
                            b = NewInstance(context);
                        }
                    }

                    try
                    {
                        if (value != null)
                            property.Accessor.SetValue(b, value);
                    }
                    catch (Exception ex)
                    {
                        throw new BeanIOException(string.Format("Failed to set property '{0}' on bean '{1}'", property.Name, Name), ex);
                    }
                }
            }

            if (b == null)
            {
                if (IsRequired || CreateMissingBeans)
                {
                    b = NewInstance(context);
                }
                else if (hasProperties)
                {
                    b = null;
                }
                else
                {
                    b = Value.Missing;
                }
            }

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

            var b = value;

            foreach (var property in Children.Cast<IProperty>())
            {
                var propertyValue = property.Accessor.GetValue(b);
                property.SetValue(context, propertyValue);
            }

            _bean.Set(context, b);
        }

        public override bool Defines(object bean)
        {
            if (PropertyType == null)
                return false;

            if (bean == null)
            {
                // allow beans that are not top level to still match if minOccurs=0
                return IsMatchNull;
            }

            if (!PropertyType.IsAssignableFrom(bean.GetType()))
                return false;

            // 'identifier' indicates the value of a child component must match
            if (!IsIdentifier)
                return true;

            // check identifying properties
            foreach (var property in Children.Cast<IProperty>().Where(x => !x.IsIdentifier))
            {
                var value = property.Accessor.GetValue(bean);
                if (!property.Defines(value))
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
            if (!locals.Add(_bean))
                return;
            locals.Add(_constructorArgs);
            base.RegisterLocals(locals);
        }

        /// <summary>
        /// Creates a new instance of this bean object
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <returns>the new bean object</returns>
        protected virtual object NewInstance(ParsingContext context)
        {
            var beanClass = PropertyType;
            if (beanClass == null)
                return null;

            try
            {
                if (Constructor == null)
                    return beanClass.NewInstance();
                return Constructor.Invoke(_constructorArgs.Get(context));
            }
            catch (Exception ex)
            {
                throw new BeanReaderException(string.Format("Failed to instantiate class '{0}'", beanClass.GetAssemblyQualifiedName()), ex);
            }
        }

        /// <summary>
        /// Backfill bean properties up to the component <code>stop</code>.
        /// </summary>
        /// <param name="context">the parsing context</param>
        /// <param name="bean">the bean object</param>
        /// <param name="stop">the component to stop at</param>
        private void Backfill(ParsingContext context, object bean, Component stop)
        {
            foreach (var property in Children.TakeWhile(x => !ReferenceEquals(x, stop)).Cast<IProperty>().Where(x => !x.Accessor.IsConstructorArgument))
            {
                var value = property.GetValue(context);
                if (ReferenceEquals(value, Value.Missing))
                    continue;

                try
                {
                    property.Accessor.SetValue(bean, value);
                }
                catch (Exception ex)
                {
                    throw new BeanIOException(string.Format("Failed to set property '{0}' on bean '{1}'", property.Name, Name), ex);
                }
            }
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

        private class ConstructorArgsParserLocal : ParserLocal<object[]>
        {
            private readonly Bean _owner;

            public ConstructorArgsParserLocal(Bean owner)
            {
                _owner = owner;
            }

            /// <summary>
            /// Called when initialized to return a default value.  If not overridden,
            /// it returns the default value passed via the constructor.
            /// </summary>
            /// <returns>the default value</returns>
            protected override object[] CreateDefaultValue()
            {
                return _owner.Constructor != null ? new object[_owner.Constructor.GetParameters().Length] : null;
            }
        }
    }
}
