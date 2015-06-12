using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using BeanIO.Config;
using BeanIO.Types;
using BeanIO.Types.Xml;

using JetBrains.Annotations;

namespace BeanIO.Internal.Util
{
    /// <summary>
    /// A factory class used to get a <see cref="ITypeHandler"/> for parsing field text
    /// into field objects, and for formatting field objects into field text.
    /// </summary>
    /// <remarks>
    /// <para>A <see cref="ITypeHandler"/> is registered and retrieved by class, type alias, or name.  If a stream
    /// format is specified when registering a type handler by class or type alias, the type handler
    /// will only be returned when the same format is queried for.
    /// In most cases, registering a type handler by type alias has the same effect as registering the
    /// type handler using the target class associated with the alias.</para>
    /// <para>If a registered type handler implements the <see cref="IConfigurableTypeHandler"/> interface,
    /// handler properties can be overridden using a <see cref="IDictionary{TKey,TValue}"/> object.  When the type handler
    /// is retrieved, the factory calls <see cref="IConfigurableTypeHandler.Configure"/> to
    /// allow the type handler to return a customized version of itself.</para>
    /// <para>By default, a <see cref="TypeHandlerFactory"/> holds a reference to a parent
    /// factory.  If a factory cannot find a type handler, its parent will be checked
    /// recursively until there is no parent left to check.</para>
    /// </remarks>
    internal class TypeHandlerFactory
    {
        private const string NameKey = "name:";

        private const string TypeKey = "type:";

        private static readonly TypeHandlerFactory _defaultFactory;

        private readonly Dictionary<string, Func<ITypeHandler>> _handlerMap = new Dictionary<string, Func<ITypeHandler>>(StringComparer.OrdinalIgnoreCase);

        private readonly TypeHandlerFactory _parent;

        static TypeHandlerFactory()
        {
            _defaultFactory = new TypeHandlerFactory(null);

            _defaultFactory.RegisterHandlerFor(typeof(char), () => new CharacterTypeHandler());
            _defaultFactory.RegisterHandlerFor(typeof(string), () => new StringTypeHandler());

            _defaultFactory.RegisterHandlerFor(typeof(sbyte), () => new NumberTypeHandler(typeof(sbyte)));
            _defaultFactory.RegisterHandlerFor(typeof(short), () => new NumberTypeHandler(typeof(short)));
            _defaultFactory.RegisterHandlerFor(typeof(int), () => new NumberTypeHandler(typeof(int)));
            _defaultFactory.RegisterHandlerFor(typeof(long), () => new NumberTypeHandler(typeof(long)));

            _defaultFactory.RegisterHandlerFor(typeof(byte), () => new NumberTypeHandler(typeof(byte)));
            _defaultFactory.RegisterHandlerFor(typeof(ushort), () => new NumberTypeHandler(typeof(ushort)));
            _defaultFactory.RegisterHandlerFor(typeof(uint), () => new NumberTypeHandler(typeof(uint)));
            _defaultFactory.RegisterHandlerFor(typeof(ulong), () => new NumberTypeHandler(typeof(ulong)));

            _defaultFactory.RegisterHandlerFor(typeof(float), () => new NumberTypeHandler(typeof(float)));
            _defaultFactory.RegisterHandlerFor(typeof(double), () => new NumberTypeHandler(typeof(double)));

            _defaultFactory.RegisterHandlerFor(typeof(decimal), () => new NumberTypeHandler(typeof(decimal)));

            _defaultFactory.RegisterHandlerFor(typeof(bool), () => new BooleanTypeHandler());
            _defaultFactory.RegisterHandlerFor(typeof(Guid), () => new GuidTypeHandler());
            _defaultFactory.RegisterHandlerFor(typeof(Uri), () => new UrlTypeHandler());

            _defaultFactory.RegisterHandlerFor("datetime", () => new DateTimeTypeHandler());
            _defaultFactory.RegisterHandlerFor("datetimeoffset", () => new DateTimeOffsetHandler());
            _defaultFactory.RegisterHandlerFor("date", () => new DateTypeHandler());
            _defaultFactory.RegisterHandlerFor("time", () => new TimeTypeHandler());

            _defaultFactory.RegisterHandlerFor(typeof(bool), () => new XmlBooleanTypeHandler(), "xml");

            _defaultFactory.RegisterHandlerFor(
                "datetime",
                () => new XmlConvertTypeHandler(
                            typeof(DateTime),
                            v => XmlConvert.ToString((DateTime)v),
                            t => XmlConvert.ToDateTimeOffset(t).DateTime),
                "xml");

            _defaultFactory.RegisterHandlerFor(
                "datetimeoffset",
                () => new XmlConvertTypeHandler(
                            typeof(DateTimeOffset),
                            v => XmlConvert.ToString((DateTimeOffset)v),
                            t => XmlConvert.ToDateTimeOffset(t)),
                "xml");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeHandlerFactory"/> class.
        /// </summary>
        public TypeHandlerFactory()
            : this(Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeHandlerFactory"/> class.
        /// </summary>
        /// <param name="parent">The parent <see cref="TypeHandlerFactory"/></param>
        public TypeHandlerFactory(TypeHandlerFactory parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// Gets the default <see cref="TypeHandlerFactory"/>
        /// </summary>
        public static TypeHandlerFactory Default
        {
            get { return _defaultFactory; }
        }

        /// <summary>
        /// Returns a named type handler, or <code>null</code> if there is no type handler configured
        /// for the given name in this factory or any of its ancestors.
        /// </summary>
        /// <param name="name">the name of type handler was registered under</param>
        /// <returns>the type handler, or <code>null</code> if there is no configured type handler
        /// registered for the name</returns>
        public ITypeHandler GetTypeHandler([NotNull] string name)
        {
            return GetTypeHandler(name, null);
        }

        /// <summary>
        /// Returns a named type handler, or <code>null</code> if there is no type handler configured
        /// for the given name in this factory or any of its ancestors.
        /// </summary>
        /// <param name="name">the name of type handler was registered under</param>
        /// <param name="properties">the custom properties for configuring the type handler</param>
        /// <returns>the type handler, or <code>null</code> if there is no configured type handler
        /// registered for the name</returns>
        public ITypeHandler GetTypeHandler([NotNull] string name, Properties properties)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            return GetHandler(NameKey + name, null, properties);
        }

        /// <summary>
        /// Returns the type handler for the given type, or <code>null</code> if there is no type
        /// handler configured for the type in this factory or any of its ancestors.
        /// </summary>
        /// <param name="typeName">the class name or type alias</param>
        /// <returns>the type handler, or <code>null</code> if there is no configured type handler
        /// registered for the type</returns>
        public ITypeHandler GetTypeHandlerFor([NotNull] string typeName)
        {
            return GetTypeHandlerFor(typeName, null, null);
        }

        /// <summary>
        /// Returns the type handler for the given type, or <code>null</code> if there is no type
        /// handler configured for the type in this factory or any of its ancestors.
        /// </summary>
        /// <param name="typeName">the class name or type alias</param>
        /// <param name="format">the stream format, or if null, format specific handlers will not be returned</param>
        /// <returns>the type handler, or <code>null</code> if there is no configured type handler
        /// registered for the type</returns>
        public ITypeHandler GetTypeHandlerFor([NotNull] string typeName, string format)
        {
            return GetTypeHandlerFor(typeName, format, null);
        }

        /// <summary>
        /// Returns the type handler for the given type, or <code>null</code> if there is no type
        /// handler configured for the type in this factory or any of its ancestors.
        /// </summary>
        /// <param name="typeName">the class name or type alias</param>
        /// <param name="format">the stream format, or if null, format specific handlers will not be returned</param>
        /// <param name="properties">the custom properties for configuring the type handler</param>
        /// <returns>the type handler, or <code>null</code> if there is no configured type handler
        /// registered for the type</returns>
        public ITypeHandler GetTypeHandlerFor([NotNull] string typeName, string format, Properties properties)
        {
            if (typeName == null)
                throw new ArgumentNullException("typeName");

            var type = typeName.ToType();
            if (type == null)
                return null;
            return GetTypeHandlerFor(type, format, properties);
        }

        /// <summary>
        /// Returns a type handler for a class, or <code>null</code> if there is no type
        /// handler configured for the class in this factory or any of its ancestors
        /// </summary>
        /// <param name="type">the target class to find a type handler for</param>
        /// <returns>the type handler, or null if the class is not supported</returns>
        public ITypeHandler GetTypeHandlerFor([NotNull] Type type)
        {
            return GetTypeHandlerFor(type, null, null);
        }

        /// <summary>
        /// Returns a type handler for a class, or <code>null</code> if there is no type
        /// handler configured for the class in this factory or any of its ancestors
        /// </summary>
        /// <param name="type">the target class to find a type handler for</param>
        /// <param name="format">the stream format, or if null, format specific handlers will not be returned</param>
        /// <returns>the type handler, or null if the class is not supported</returns>
        public ITypeHandler GetTypeHandlerFor([NotNull] Type type, [NotNull] string format)
        {
            return GetTypeHandlerFor(type, format, null);
        }

        /// <summary>
        /// Returns a type handler for a class, or <code>null</code> if there is no type
        /// handler configured for the class in this factory or any of its ancestors
        /// </summary>
        /// <param name="type">the target class to find a type handler for</param>
        /// <param name="format">the stream format, or if null, format specific handlers will not be returned</param>
        /// <param name="properties">the custom properties for configuring the type handler</param>
        /// <returns>the type handler, or null if the class is not supported</returns>
        public ITypeHandler GetTypeHandlerFor([NotNull] Type type, string format, Properties properties)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            // Ensure that we "unbox" a nullable type
            type = Nullable.GetUnderlyingType(type) ?? type;

            var handler = GetHandler(TypeKey + type.FullName, format, properties);
            if (handler == null && typeof(Enum).IsAssignableFrom(type))
                return GetEnumHandler(type, properties);

            return handler;
        }

        /// <summary>
        /// Registers a type handler in this factory
        /// </summary>
        /// <param name="name">the name to register the type handler under</param>
        /// <param name="createHandler">the type handler creation function to register</param>
        public void RegisterHandler([NotNull] string name, [NotNull] Func<ITypeHandler> createHandler)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (createHandler == null)
                throw new ArgumentNullException("createHandler");
            _handlerMap[NameKey + name] = createHandler;
        }

        /// <summary>
        /// Registers a type handler in this factory by class type for all stream formats
        /// </summary>
        /// <param name="name">the fully qualified class name or type alias to register the type handler for</param>
        /// <param name="createHandler">the type handler creation function to register</param>
        public void RegisterHandlerFor([NotNull] string name, [NotNull] Func<ITypeHandler> createHandler)
        {
            RegisterHandlerFor(name, createHandler, null);
        }

        /// <summary>
        /// Registers a type handler in this factory by class type for a specific stream format
        /// </summary>
        /// <param name="name">the fully qualified class name or type alias to register the type handler for</param>
        /// <param name="createHandler">the type handler creation function to register</param>
        /// <param name="format">the stream format to register the type handler for, or if null the type handler may be returned for any format</param>
        public void RegisterHandlerFor([NotNull] string name, [NotNull] Func<ITypeHandler> createHandler, string format)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            var type = name.ToType();
            if (type == null)
                throw new ArgumentException(string.Format("Invalid type or type alias '{0}'", name), "name");

            RegisterHandlerFor(format, type.FullName, type, createHandler);
        }

        /// <summary>
        /// Registers a type handler in this factory for any stream format.
        /// </summary>
        /// <param name="type">the target class to register the type handler for</param>
        /// <param name="createHandler">the type handler creation function to register</param>
        public void RegisterHandlerFor([NotNull] Type type, [NotNull] Func<ITypeHandler> createHandler)
        {
            RegisterHandlerFor(type, createHandler, null);
        }

        /// <summary>
        /// Registers a type handler in this factory for a specific stream format
        /// </summary>
        /// <param name="type">the target class to register the type handler for</param>
        /// <param name="createHandler">the type handler creation function to register</param>
        /// <param name="format">the stream format to register the type handler for, or if null the type handler may be returned for any format</param>
        public void RegisterHandlerFor([NotNull] Type type, [NotNull] Func<ITypeHandler> createHandler, string format)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            RegisterHandlerFor(format, type.FullName, type, createHandler);
        }

        private void RegisterHandlerFor([CanBeNull] string format, [NotNull] string typeName, [NotNull] Type expectedClass, [NotNull] Func<ITypeHandler> createHandler)
        {
            var testInstance = createHandler();
            if (!expectedClass.IsAssignableFrom(testInstance.TargetType))
                throw new ArgumentException(string.Format("Type handler of type '{0}' is not assignable from configured type '{1}'", testInstance.TargetType, expectedClass));

            if (format != null)
            {
                _handlerMap[string.Concat(format, ".", TypeKey, typeName)] = createHandler;
            }
            else
            {
                _handlerMap[string.Concat(TypeKey, typeName)] = createHandler;
            }
        }

        private ITypeHandler GetEnumHandler([NotNull] Type enumType, Properties properties)
        {
            var handler = new EnumTypeHandler(enumType);
            if (properties != null)
                handler.Configure(properties);
            return handler;
        }

        private ITypeHandler GetHandler([NotNull] string key, string format, Properties properties)
        {
            var factory = this;
            while (factory != null)
            {
                Func<ITypeHandler> createHandler;
                if (format != null && factory._handlerMap.TryGetValue(format + "." + key, out createHandler))
                    return GetHandler(createHandler, properties);

                if (factory._handlerMap.TryGetValue(key, out createHandler))
                    return GetHandler(createHandler, properties);

                factory = factory._parent;
            }

            return null;
        }

        private ITypeHandler GetHandler([NotNull] Func<ITypeHandler> createHandler, Properties properties)
        {
            var handler = createHandler();
            if (properties != null && properties.Count != 0)
            {
                var configurableHandler = handler as IConfigurableTypeHandler;
                if (configurableHandler == null)
                    throw new BeanIOConfigurationException(string.Format("'{0}' setting not supported by type handler with target type '{1}'", properties.First().Key, handler.TargetType));
                configurableHandler.Configure(properties);
            }

            return handler;
        }
    }
}
