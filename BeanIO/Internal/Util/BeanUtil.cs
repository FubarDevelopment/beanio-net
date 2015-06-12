using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using BeanIO.Config;
using BeanIO.Types;

using JetBrains.Annotations;

namespace BeanIO.Internal.Util
{
    /// <summary>
    /// Utility class for instantiating configurable bean classes
    /// </summary>
    internal static class BeanUtil
    {
        private static readonly bool NULL_ESCAPING_ENABLED = Settings.Instance.GetBoolean(Settings.NULL_ESCAPING_ENABLED);

        private static readonly TypeHandlerFactory _typeHandlerFactory = new TypeHandlerFactory();

        static BeanUtil()
        {
            _typeHandlerFactory.RegisterHandlerFor(typeof(string[]), () => new StringArrayTypeHandler());
            if (Settings.Instance.GetBoolean(Settings.PROPERTY_ESCAPING_ENABLED))
            {
                _typeHandlerFactory.RegisterHandlerFor(typeof(string), () => new EscapedStringTypeHandler());
                _typeHandlerFactory.RegisterHandlerFor(typeof(char), () => new EscapedCharacterTypeHandler());
            }
        }

        public static object CreateBean([NotNull] string className, Properties properties)
        {
            var bean = CreateBean(className);
            Configure(bean, properties);
            return bean;
        }

        public static object CreateBean([NotNull] string className)
        {
            if (className == null)
                throw new ArgumentNullException("className");

            Type type;
            try
            {
                // load the class
                type = Type.GetType(className, true);
            }
            catch (Exception ex)
            {
                throw new BeanIOConfigurationException(string.Format("Class not found '{0}'", className), ex);
            }

            try
            {
                // instantiate an instance of the class
                return type.NewInstance();
            }
            catch (Exception ex)
            {
                throw new BeanIOConfigurationException(string.Format("Could not instantiate class '{0}'", type), ex);
            }
        }

        public static void Configure(object bean, Properties properties)
        {
            // if no properties, we're done...
            if (properties == null || properties.Count == 0)
                return;

            var type = bean.GetType();
            var typeInfo = type.GetTypeInfo();

            foreach (var property in properties)
            {
                var name = property.Key;

                Action<object> setAction;
                Type valueType;

                var prop = typeInfo.GetDeclaredProperty(name);
                if (prop != null && prop.SetMethod != null)
                {
                    setAction = v => prop.SetValue(bean, v);
                    valueType = prop.PropertyType;
                }
                else
                {
                    var field = typeInfo.GetDeclaredField(name);
                    if (field != null)
                    {
                        setAction = v => field.SetValue(bean, v);
                        valueType = field.FieldType;
                    }
                    else
                    {
                        throw new BeanIOConfigurationException(string.Format("Property '{0}' not found on class '{1}'", name, type));
                    }
                }

                var handler = _typeHandlerFactory.GetTypeHandlerFor(valueType);
                if (handler == null)
                    throw new BeanIOConfigurationException(string.Format("Property type '{0}' not supported for property '{1}' on class '{2}'", valueType, name, type));

                try
                {
                    var value = handler.Parse(property.Value);
                    if (value != null)
                        setAction(value);
                }
                catch (FormatException ex)
                {
                    throw new BeanIOConfigurationException(string.Format("Type conversion failed for property '{0}' on class '{1}': {2}", name, type, ex.Message), ex);
                }
                catch (Exception ex)
                {
                    throw new BeanIOConfigurationException(string.Format("Failed to set property '{0}' on class '{1}': {2}", name, type, ex.Message), ex);
                }
            }
        }

        private class EscapedCharacterTypeHandler : ITypeHandler
        {
            /// <summary>
            /// Gets the class type supported by this handler.
            /// </summary>
            public Type TargetType
            {
                get { return typeof(char); }
            }

            /// <summary>
            /// Parses field text into an object.
            /// </summary>
            /// <param name="text">The field text to parse, which may be null if the field was not passed in the record</param>
            /// <returns>The parsed object</returns>
            public object Parse(string text)
            {
                if (string.IsNullOrEmpty(text))
                    return null;

                var ch = text[0];
                if (text.Length == 1 || ch != '\\')
                    return ch;

                ch = text[1];
                switch (ch)
                {
                    case '\\':
                        return '\\';
                    case 'n':
                        return '\n';
                    case 'r':
                        return '\r';
                    case 't':
                        return '\t';
                    case 'f':
                        return '\f';
                    case '0':
                        if (NULL_ESCAPING_ENABLED)
                            return '\0';
                        break;
                }

                throw new FormatException(string.Format("Invalid character '{0}'", ch));
            }

            /// <summary>
            /// Formats an object into field text.
            /// </summary>
            /// <param name="value">The value to format, which may be null</param>
            /// <returns>The formatted field text, or <code>null</code> to indicate the value is not present</returns>
            public string Format(object value)
            {
                throw new NotSupportedException();
            }
        }

        private class EscapedStringTypeHandler : ITypeHandler
        {
            /// <summary>
            /// Gets the class type supported by this handler.
            /// </summary>
            public Type TargetType
            {
                get { return typeof(string); }
            }

            /// <summary>
            /// Parses field text into an object.
            /// </summary>
            /// <param name="text">The field text to parse, which may be null if the field was not passed in the record</param>
            /// <returns>The parsed object</returns>
            public object Parse(string text)
            {
                if (text == null)
                    return null;

                var n = text.IndexOf('\\') + 1;
                if (n == 0)
                    return text;

                var start = 0;
                var value = new StringBuilder();
                var escaped = true;
                while (n < text.Length && n != -1)
                {
                    if (escaped)
                    {
                        value.Append(text.Substring(start, n - start - 1));
                        var c = text[n];
                        switch (c)
                        {
                            case 'n':
                                value.Append('\n');
                                break;
                            case 'r':
                                value.Append('\r');
                                break;
                            case 't':
                                value.Append('\t');
                                break;
                            case 'f':
                                value.Append('\f');
                                break;
                            case '0':
                                if (NULL_ESCAPING_ENABLED)
                                {
                                    value.Append('\0');
                                }
                                else
                                {
                                    value.Append(c);
                                }
                                break;

                            default:
                                value.Append(c);
                                break;
                        }
                        escaped = false;
                        start = n + 1;
                        n = text.IndexOf('\\', start);
                    }
                    else
                    {
                        escaped = true;
                        n = n + 1;
                    }
                }

                if (start < text.Length)
                    value.Append(text.Substring(start, text.Length - start - (escaped ? 1 : 0)));

                return value.ToString();
            }

            /// <summary>
            /// Formats an object into field text.
            /// </summary>
            /// <param name="value">The value to format, which may be null</param>
            /// <returns>The formatted field text, or <code>null</code> to indicate the value is not present</returns>
            public string Format(object value)
            {
                throw new NotSupportedException();
            }
        }

        private class StringArrayTypeHandler : ITypeHandler
        {
            /// <summary>
            /// Gets the class type supported by this handler.
            /// </summary>
            public Type TargetType
            {
                get { return typeof(string[]); }
            }

            /// <summary>
            /// Parses field text into an object.
            /// </summary>
            /// <param name="text">The field text to parse, which may be null if the field was not passed in the record</param>
            /// <returns>The parsed object</returns>
            public object Parse(string text)
            {
                if (string.IsNullOrEmpty(text))
                    return null;

                var pos = text.IndexOf(',');
                if (pos < 0)
                    return new[] { text };

                var escaped = false;
                var item = new StringBuilder();
                var list = new List<string>();

                var ca = text.ToCharArray();
                foreach (var c in ca)
                {
                    if (escaped)
                    {
                        item.Append(c);
                        escaped = false;
                    }
                    else if (c == '\\')
                    {
                        escaped = true;
                    }
                    else if (c == ',')
                    {
                        list.Add(item.ToString());
                        item = new StringBuilder();
                    }
                    else
                    {
                        item.Append(c);
                    }
                }

                list.Add(item.ToString());

                return list.ToArray();
            }

            /// <summary>
            /// Formats an object into field text.
            /// </summary>
            /// <param name="value">The value to format, which may be null</param>
            /// <returns>The formatted field text, or <code>null</code> to indicate the value is not present</returns>
            public string Format(object value)
            {
                throw new NotSupportedException();
            }
        }
    }
}
