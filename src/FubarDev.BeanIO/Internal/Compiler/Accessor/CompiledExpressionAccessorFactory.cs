// <copyright file="CompiledExpressionAccessorFactory.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Linq.Expressions;

using BeanIO.Internal.Parser;
using BeanIO.Internal.Parser.Accessor;
using BeanIO.Internal.Util;

namespace BeanIO.Internal.Compiler.Accessor
{
    /// <summary>
    /// <see cref="IPropertyAccessorFactory"/> implementations based on compiled expression trees
    /// </summary>
    internal class CompiledExpressionAccessorFactory : IPropertyAccessorFactory
    {
        private delegate void SetValueDelegate(object bean, object value);

        private delegate object GetValueDelegate(object bean);

        /// <inheritdoc />
        public IPropertyAccessor CreatePropertyAccessor(Type parentType, PropertyDescriptor property, int? carg)
        {
            var getDelegate = CreateGetter(property, parentType);
            var setDelegate = CreateSetter(property, parentType);

            return new CompiledExpressionAccessor(property.Name, getDelegate, setDelegate, carg);
        }

        private static GetValueDelegate CreateGetter(PropertyDescriptor info, Type parenType)
        {
            if (!info.HasGetter)
                return null;

            var beanParameter = Expression.Parameter(typeof(object), "bean");
            var beanExpression = Expression.Convert(beanParameter, parenType);
            Expression getExpression;

            if (info.GetMethodInfo != null)
            {
                getExpression = Expression.Call(beanExpression, info.GetMethodInfo);
            }
            else if (info.PropertyInfo != null)
            {
                getExpression = Expression.Property(beanExpression, info.PropertyInfo);
            }
            else if (info.FieldInfo != null)
            {
                getExpression = Expression.Field(beanExpression, info.FieldInfo);
            }
            else
            {
                throw new NotSupportedException("Unsupported property descriptor");
            }

            var getValueLambda = Expression.Lambda<GetValueDelegate>(Expression.Convert(getExpression, typeof(object)), beanParameter);
            return getValueLambda.Compile();
        }

        private static SetValueDelegate CreateSetter(PropertyDescriptor info, Type parenType)
        {
            if (!info.HasSetter)
                return null;

            var beanParameter = Expression.Parameter(typeof(object), "bean");
            var beanExpression = Expression.Convert(beanParameter, parenType);
            var valueParameter = Expression.Parameter(typeof(object), "value");
            var valueExpression = Expression.Convert(valueParameter, info.PropertyType);
            Expression setExpression;

            if (info.SetMethodInfo != null)
            {
                setExpression = Expression.Call(beanExpression, info.SetMethodInfo, valueExpression);
            }
            else if (info.PropertyInfo != null)
            {
                setExpression = Expression.Assign(Expression.Property(beanExpression, info.PropertyInfo), valueExpression);
            }
            else if (info.FieldInfo != null)
            {
                setExpression = Expression.Assign(Expression.Field(beanExpression, info.FieldInfo), valueExpression);
            }
            else
            {
                throw new NotSupportedException("Unsupported property descriptor");
            }

            var setValueLambda = Expression.Lambda<SetValueDelegate>(setExpression, beanParameter, valueParameter);
            return setValueLambda.Compile();
        }

        private class CompiledExpressionAccessor : PropertyAccessorSupport
        {
            private readonly string _propertyName;
            private readonly GetValueDelegate _getValueDelegate;
            private readonly SetValueDelegate _setValueDelegate;

            public CompiledExpressionAccessor(string propertyName, GetValueDelegate getValueDelegate, SetValueDelegate setValueDelegate, int? constructorArgumentIndex)
            {
                _propertyName = propertyName;
                _getValueDelegate = getValueDelegate;
                _setValueDelegate = setValueDelegate;
                ConstructorArgumentIndex = constructorArgumentIndex;
            }

            public override object GetValue(object bean)
            {
                try
                {
                    return _getValueDelegate(bean);
                }
                catch (Exception ex)
                {
                    throw new BeanIOException(
                        string.Format(
                            "Failed to get value for property or field '{2}' on bean class '{0}': {1}",
                            bean.GetType().GetAssemblyQualifiedName(),
                            ex.Message,
                            _propertyName),
                        ex);
                }
            }

            public override void SetValue(object bean, object value)
            {
                try
                {
                    _setValueDelegate(bean, value);
                }
                catch (Exception ex)
                {
                    throw new BeanIOException(
                        string.Format(
                            "Failed to set value for property or field '{2}' on bean class '{0}': {1}",
                            bean.GetType().GetAssemblyQualifiedName(),
                            ex.Message,
                            _propertyName),
                        ex);
                }
            }
        }
    }
}
