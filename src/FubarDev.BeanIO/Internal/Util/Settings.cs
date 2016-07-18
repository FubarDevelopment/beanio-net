// <copyright file="Settings.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using BeanIO.Config;
using BeanIO.Config.SchemeHandlers;

using NodaTime;

namespace BeanIO.Internal.Util
{
    /// <summary>
    /// <see cref="Settings"/> is used to load and store BeanIO configuration settings.
    /// </summary>
    internal class Settings
    {
        /// <summary>
        /// This property is set to the fully qualified class name of the default stream factory implementation
        /// </summary>
        public static readonly string STREAM_FACTORY_CLASS = "org.beanio.streamFactory";

        /// <summary>
        /// The default locale used by type handlers
        /// </summary>
        public static readonly string DEFAULT_LOCALE = "org.beanio.defaultTypeHandlerLocale";

        /// <summary>
        /// The default date format pattern for fields assigned type alias <see cref="DateTime"/>
        /// </summary>
        public static readonly string DEFAULT_DATE_FORMAT = "org.beanio.defaultDateFormat";

        /// <summary>
        /// The default date format pattern for fields assigned type alias <see cref="DateTime"/> or of type <see cref="LocalDate"/>
        /// </summary>
        public static readonly string DEFAULT_DATETIME_FORMAT = "org.beanio.defaultDateTimeFormat";

        /// <summary>
        /// The default date format pattern for fields assigned type alias <see cref="LocalTime"/>
        /// </summary>
        public static readonly string DEFAULT_TIME_FORMAT = "org.beanio.defaultTimeFormat";

        /// <summary>
        /// Whether property values support the following escape sequences
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item><code>\\</code> - Backslash</item>
        /// <item><code>\n</code> - Line Feed</item>
        /// <item><code>\r</code> - Carriage Return</item>
        /// <item><code>\t</code> - Tab</item>
        /// <item><code>\f</code> - Form Feed</item>
        /// <item><code>\0</code> - Null</item>
        /// </list>
        /// <para>A backslash preceding any other character is ignored.</para>
        /// <para>Set to <code>false</code> to disable.</para>
        /// </remarks>
        public static readonly string PROPERTY_ESCAPING_ENABLED = "org.beanio.propertyEscapingEnabled";

        /// <summary>
        /// Whether the null character can be escaped using <code>\0</code> when property escaping is enabled.
        /// </summary>
        public static readonly string NULL_ESCAPING_ENABLED = "org.beanio.propertyEscapingEnabled";

        /// <summary>
        /// Whether property substitution is enabled for mapping files
        /// </summary>
        public static readonly string PROPERTY_SUBSTITUTION_ENABLED = "org.beanio.propertySubstitutionEnabled";

        /// <summary>
        /// The default XML type for a field definition, set to <code>element</code> or <code>attribute</code>.
        /// </summary>
        public static readonly string DEFAULT_XML_TYPE = "org.beanio.xml.defaultXmlType";

        /// <summary>
        /// The default namespace prefix for 'http://www.w3.org/2001/XMLSchema-instance'
        /// </summary>
        public static readonly string DEFAULT_XSI_NAMESPACE_PREFIX = "org.beanio.xml.xsiNamespacePrefix";

        /// <summary>
        /// Used for Spring Batch integration.  Set to 'true' to have the XmlWriter only update the execution context (state)
        /// with changes since the last update.  At the time of writing, it's not known whether Spring Batch will create a new
        /// ExecutionContext every time state is updated, or if the current context is used.  Disabled by default until proven
        /// the optimization will not impact state updates.
        /// </summary>
        public static readonly string XML_WRITER_UPDATE_STATE_USING_DELTA = "org.beanio.stream.xml.XmlWriter.deltaEnabled";

        /// <summary>
        /// Whether a configured field default is marshalled for null property values. The default configuration
        /// sets this property to <code>true</code>.
        /// </summary>
        public static readonly string DEFAULT_MARSHALLING_ENABLED = "org.beanio.marshalDefaultEnabled";

        /// <summary>
        /// Whether a configured field default is unmarshalled on initialization. The default configuration
        /// sets this property to <code>true</code>.
        /// </summary>
        /// <remarks>
        /// This is useful when the default value cannot be represented using the underlying type.
        /// </remarks>
        public static readonly string DEFAULT_PARSING_ENABLED = "org.beanio.parseDefaultEnabled";

        /// <summary>
        /// The default minOccurs setting for a group.
        /// </summary>
        public static readonly string DEFAULT_GROUP_MIN_OCCURS = "org.beanio.group.minOccurs";

        /// <summary>
        /// The default minOccurs setting for a record.
        /// </summary>
        public static readonly string DEFAULT_RECORD_MIN_OCCURS = "org.beanio.record.minOccurs";

        /// <summary>
        /// The default minOccurs setting for a field (after appending the stream format)
        /// </summary>
        public static readonly string DEFAULT_FIELD_MIN_OCCURS = "org.beanio.field.minOccurs";

        /// <summary>
        /// The method of property access to use, <code>reflection</code> (default) or <code>asm</code> is supported
        /// </summary>
        public static readonly string PROPERTY_ACCESSOR_METHOD = "org.beanio.propertyAccessorFactory";

        /// <summary>
        /// Whether version 2.0.0 style unmarshalling should be supported which instantiates bean objects
        /// for missing fields and records during unmarshalling. This behavior is not recommended.
        /// </summary>
        public static readonly string CREATE_MISSING_BEANS = "org.beanio.createMissingBeans";

        /// <summary>
        /// Whether objects are lazily instantiated if Strings are empty, rather than just null.
        /// </summary>
        public static readonly string LAZY_IF_EMPTY = "org.beanio.lazyIfEmpty";

        /// <summary>
        /// Whether null field values should throw an exception if bound to a primitive
        /// </summary>
        public static readonly string ERROR_IF_NULL_PRIMITIVE = "org.beanio.errorIfNullPrimitive";

        /// <summary>
        /// Whether default field values apply to missing fields
        /// </summary>
        public static readonly string USE_DEFAULT_IF_MISSING = "org.beanio.useDefaultIfMissing";

        /// <summary>
        /// Whether to validate marshalled fields
        /// </summary>
        public static readonly string VALIDATE_ON_MARSHAL = "org.beanio.validateOnMarshal";

        /// <summary>
        /// Whether XML components should be sorted by position. Helpful for use with annotations
        /// where fields and methods may not be ordered by their position in the stream.
        /// </summary>
        public static readonly string SORT_XML_COMPONENTS_BY_POSITION = "org.beanio.xml.sorted";

        /// <summary>
        /// Whether non-public fields and methods may be made accessible.
        /// </summary>
        public static readonly string ALLOW_PROTECTED_PROPERTY_ACCESS = "org.beanio.allowProtectedAccess";

        private const string DEFAULT_CONFIGURATION_PATH = "BeanIO.Internal.Config.beanio.properties";

        private static readonly object _syncRoot = new object();

        private static Settings _instance;

        private readonly Properties _properties;

        private readonly Dictionary<string, ISchemeHandler> _schemeHandlers = new Dictionary<string, ISchemeHandler>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        /// <param name="properties">The properties to use for this settings object.</param>
        public Settings(Properties properties)
        {
            _properties = properties;
            Add(new ResourceSchemeHandler());
        }

        /// <summary>
        /// Gets or sets the global <see cref="Settings"/> instance.
        /// </summary>
        public static Settings Instance
        {
            get
            {
                lock (_syncRoot)
                    return _instance ?? (_instance = new Settings(CreateDefaultProvider().Read()));
            }
            set
            {
                lock (_syncRoot)
                    _instance = value;
            }
        }

        /// <summary>
        /// Gets all registered scheme handlers
        /// </summary>
        public IReadOnlyDictionary<string, ISchemeHandler> SchemeHandlers => _schemeHandlers;

        /// <summary>
        /// Returns a BeanIO configuration setting
        /// </summary>
        /// <param name="key">the name of the setting</param>
        /// <returns>the value of the setting, or null if the name is invalid</returns>
        public string this[string key]
        {
            get
            {
                string result;
                if (_properties.TryGetValue(key, out result))
                    return result;
                return null;
            }
        }

        /// <summary>
        /// Returns a BeanIO configuration setting
        /// </summary>
        /// <param name="key">the name of the setting</param>
        /// <returns>the value of the setting, or null if the name is invalid</returns>
        public string GetProperty(string key)
        {
            return this[key];
        }

        /// <summary>
        /// Returns the boolean value of a BeanIO configuration setting
        /// </summary>
        /// <param name="key">the property key</param>
        /// <returns>true if the property value is "1" or "true" (case insensitive),
        /// or false if the property is any other value</returns>
        public bool GetBoolean(string key)
        {
            var temp = this[key];
            if (string.IsNullOrEmpty(temp))
                return false;
            temp = temp.Trim();
            if (string.Equals("true", temp, StringComparison.OrdinalIgnoreCase))
                return true;
            return temp == "1";
        }

        /// <summary>
        /// Returns a BeanIO configuration setting as an integer
        /// </summary>
        /// <param name="key">the property key</param>
        /// <param name="defaultValue">the default value if the setting wasn't configured or invalid</param>
        /// <returns>the <code>int</code> property value or <paramref name="defaultValue"/></returns>
        public int GetInt(string key, int defaultValue)
        {
            var temp = this[key];
            if (string.IsNullOrWhiteSpace(temp))
                return defaultValue;
            int result;
            if (int.TryParse(temp, out result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// Adds a new <see cref="ISchemeHandler"/>
        /// </summary>
        /// <param name="handler">the handler to add</param>
        public void Add(ISchemeHandler handler)
        {
            _schemeHandlers[handler.Scheme] = handler;
        }

        public ISchemeHandler GetSchemeHandler(Uri url, bool throwIfMissing)
        {
            return GetSchemeHandler(url.Scheme, throwIfMissing);
        }

        public ISchemeHandler GetSchemeHandler(string scheme, bool throwIfMissing)
        {
            ISchemeHandler handler;
            if (!SchemeHandlers.TryGetValue(scheme, out handler))
            {
                if (!throwIfMissing)
                    return null;
                throw new BeanIOConfigurationException(
                    $"Scheme '{scheme}' must one of: {string.Join(", ", SchemeHandlers.Keys.Select(x => $"'{x}'"))}");
            }

            return handler;
        }

        private static IPropertiesProvider CreateDefaultProvider()
        {
            var defaultProvider = new PropertiesStreamProvider(typeof(Settings).GetTypeInfo().Assembly.GetManifestResourceStream(DEFAULT_CONFIGURATION_PATH));
            return defaultProvider;
        }
    }
}
