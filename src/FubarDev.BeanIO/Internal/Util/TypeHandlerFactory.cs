// <copyright file="TypeHandlerFactory.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BeanIO.Config;
using BeanIO.Types;
using BeanIO.Types.Xml;

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

        private readonly TypeHandlerFactory? _parent;

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

            _defaultFactory.RegisterHandlerFor(typeof(float), () => new SingleTypeHandler());
            _defaultFactory.RegisterHandlerFor(typeof(double), () => new DoubleTypeHandler());

            _defaultFactory.RegisterHandlerFor(typeof(decimal), () => new NumberTypeHandler(typeof(decimal)));

            _defaultFactory.RegisterHandlerFor(typeof(bool), () => new BooleanTypeHandler());
            _defaultFactory.RegisterHandlerFor(typeof(Guid), () => new GuidTypeHandler());
            _defaultFactory.RegisterHandlerFor(typeof(Uri), () => new UrlTypeHandler());

            _defaultFactory.RegisterHandlerFor(typeof(Version), () => new VersionTypeHandler());
            _defaultFactory.RegisterHandlerFor(typeof(Encoding), () => new EncodingTypeHandler());

            _defaultFactory.RegisterHandlerFor("datetime", () => new DateTimeTypeHandler());
            _defaultFactory.RegisterHandlerFor("datetimeoffset", () => new DateTimeOffsetTypeHandler());
            _defaultFactory.RegisterHandlerFor("date", () => new DateTypeHandler());
            _defaultFactory.RegisterHandlerFor("time", () => new TimeTypeHandler());

            _defaultFactory.RegisterHandlerFor(typeof(bool), () => new XmlBooleanTypeHandler(), "xml");
            _defaultFactory.RegisterHandlerFor("datetime", () => new XmlDateTimeTypeHandler(), "xml");
            _defaultFactory.RegisterHandlerFor("datetimeoffset", () => new XmlDateTimeOffsetTypeHandler(), "xml");
            _defaultFactory.RegisterHandlerFor("date", () => new XmlDateTypeHandler(), "xml");
            _defaultFactory.RegisterHandlerFor("time", () => new XmlTimeTypeHandler(), "xml");
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
        /// <param name="parent">The parent <see cref="TypeHandlerFactory"/>.</param>
        public TypeHandlerFactory(TypeHandlerFactory? parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// Gets the default <see cref="TypeHandlerFactory"/>.
        /// </summary>
        public static TypeHandlerFactory Default => _defaultFactory;

        /// <summary>
        /// Returns a named type handler, or <see langword="null" /> if there is no type handler configured
        /// for the given name in this factory or any of its ancestors.
        /// </summary>
        /// <param name="name">the name of type handler was registered under.</param>
        /// <returns>the type handler, or <see langword="null" /> if there is no configured type handler
        /// registered for the name.</returns>
        public ITypeHandler? GetTypeHandler(string name)
        {
            return GetTypeHandler(name, null);
        }

        /// <summary>
        /// Returns a named type handler, or <see langword="null" /> if there is no type handler configured
        /// for the given name in this factory or any of its ancestors.
        /// </summary>
        /// <param name="name">the name of type handler was registered under.</param>
        /// <param name="properties">the custom properties for configuring the type handler.</param>
        /// <returns>the type handler, or <see langword="null" /> if there is no configured type handler
        /// registered for the name.</returns>
        public ITypeHandler? GetTypeHandler(string name, Properties? properties)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            return GetHandler(NameKey + name, null, properties);
        }

        /// <summary>
        /// Returns the type handler for the given type, or <see langword="null" /> if there is no type
        /// handler configured for the type in this factory or any of its ancestors.
        /// </summary>
        /// <param name="typeName">the class name or type alias.</param>
        /// <returns>the type handler, or <see langword="null" /> if there is no configured type handler
        /// registered for the type.</returns>
        public ITypeHandler? GetTypeHandlerFor(string typeName)
        {
            return GetTypeHandlerFor(typeName, null, null);
        }

        /// <summary>
        /// Returns the type handler for the given type, or <see langword="null" /> if there is no type
        /// handler configured for the type in this factory or any of its ancestors.
        /// </summary>
        /// <param name="typeName">the class name or type alias.</param>
        /// <param name="format">the stream format, or if null, format specific handlers will not be returned.</param>
        /// <returns>the type handler, or <see langword="null" /> if there is no configured type handler
        /// registered for the type.</returns>
        public ITypeHandler? GetTypeHandlerFor(string typeName, string? format)
        {
            return GetTypeHandlerFor(typeName, format, null);
        }

        /// <summary>
        /// Returns the type handler for the given type, or <see langword="null" /> if there is no type
        /// handler configured for the type in this factory or any of its ancestors.
        /// </summary>
        /// <param name="typeName">the class name or type alias.</param>
        /// <param name="format">the stream format, or if null, format specific handlers will not be returned.</param>
        /// <param name="properties">the custom properties for configuring the type handler.</param>
        /// <returns>the type handler, or <see langword="null" /> if there is no configured type handler
        /// registered for the type.</returns>
        public ITypeHandler? GetTypeHandlerFor(string typeName, string? format, Properties? properties)
        {
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            var type = typeName.ToType();
            if (type == null)
                return null;
            return GetTypeHandlerFor(type, format, properties);
        }

        /// <summary>
        /// Returns a type handler for a class, or <see langword="null" /> if there is no type
        /// handler configured for the class in this factory or any of its ancestors.
        /// </summary>
        /// <param name="type">the target class to find a type handler for.</param>
        /// <returns>the type handler, or null if the class is not supported.</returns>
        public ITypeHandler? GetTypeHandlerFor(Type type)
        {
            return GetTypeHandlerFor(type, null, null);
        }

        /// <summary>
        /// Returns a type handler for a class, or <see langword="null" /> if there is no type
        /// handler configured for the class in this factory or any of its ancestors.
        /// </summary>
        /// <param name="type">the target class to find a type handler for.</param>
        /// <param name="format">the stream format, or if null, format specific handlers will not be returned.</param>
        /// <returns>the type handler, or null if the class is not supported.</returns>
        public ITypeHandler? GetTypeHandlerFor(Type type, string format)
        {
            return GetTypeHandlerFor(type, format, null);
        }

        /// <summary>
        /// Returns a type handler for a class, or <see langword="null" /> if there is no type
        /// handler configured for the class in this factory or any of its ancestors.
        /// </summary>
        /// <param name="type">the target class to find a type handler for.</param>
        /// <param name="format">the stream format, or if null, format specific handlers will not be returned.</param>
        /// <param name="properties">the custom properties for configuring the type handler.</param>
        /// <returns>the type handler, or null if the class is not supported.</returns>
        public ITypeHandler? GetTypeHandlerFor(Type type, string? format, Properties? properties)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            // Ensure that we "unbox" a nullable type
            type = Nullable.GetUnderlyingType(type) ?? type;

            var handler = GetHandler(TypeKey + type.FullName, format, properties);
            if (handler == null && typeof(Enum).IsAssignableFromThis(type))
                return GetEnumHandler(type, properties);

            return handler;
        }

        /// <summary>
        /// Registers a type handler in this factory.
        /// </summary>
        /// <param name="name">the name to register the type handler under.</param>
        /// <param name="createHandler">the type handler creation function to register.</param>
        public void RegisterHandler(string name, Func<ITypeHandler> createHandler)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            _handlerMap[NameKey + name] = createHandler ?? throw new ArgumentNullException(nameof(createHandler));
        }

        /// <summary>
        /// Registers a type handler in this factory by class type for all stream formats.
        /// </summary>
        /// <param name="name">the fully qualified class name or type alias to register the type handler for.</param>
        /// <param name="createHandler">the type handler creation function to register.</param>
        public void RegisterHandlerFor(string name, Func<ITypeHandler> createHandler)
        {
            RegisterHandlerFor(name, createHandler, null);
        }

        /// <summary>
        /// Registers a type handler in this factory by class type for a specific stream format.
        /// </summary>
        /// <param name="name">the fully qualified class name or type alias to register the type handler for.</param>
        /// <param name="createHandler">the type handler creation function to register.</param>
        /// <param name="format">the stream format to register the type handler for, or if null the type handler may be returned for any format.</param>
        public void RegisterHandlerFor(string name, Func<ITypeHandler> createHandler, string? format)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var type = name.ToType();
            if (type == null)
                throw new ArgumentException($"Invalid type or type alias '{name}'", nameof(name));

            RegisterHandlerFor(
                format,
                type.FullName ?? throw new InvalidOperationException($"Type without FullName: {type}"),
                type,
                createHandler);
        }

        /// <summary>
        /// Registers a type handler in this factory for any stream format.
        /// </summary>
        /// <param name="type">the target class to register the type handler for.</param>
        /// <param name="createHandler">the type handler creation function to register.</param>
        public void RegisterHandlerFor(Type type, Func<ITypeHandler> createHandler)
        {
            RegisterHandlerFor(type, createHandler, null);
        }

        /// <summary>
        /// Registers a type handler in this factory for a specific stream format.
        /// </summary>
        /// <param name="type">the target class to register the type handler for.</param>
        /// <param name="createHandler">the type handler creation function to register.</param>
        /// <param name="format">the stream format to register the type handler for, or if null the type handler may be returned for any format.</param>
        public void RegisterHandlerFor(Type type, Func<ITypeHandler> createHandler, string? format)
        {
            if (type == null!)
                throw new ArgumentNullException(nameof(type));

            if (type.FullName == null)
                throw new ArgumentNullException(nameof(type));

            RegisterHandlerFor(format, type.FullName, type, createHandler);
        }

        private void RegisterHandlerFor(string? format, string typeName, Type expectedClass, Func<ITypeHandler> createHandler)
        {
            var testInstance = createHandler();
            if (!expectedClass.IsAssignableFromThis(testInstance.TargetType))
            {
                throw new ArgumentException(
                    $"Type handler of type '{testInstance.TargetType}' is not assignable from configured type '{expectedClass}'");
            }

            if (format != null)
            {
                _handlerMap[string.Concat(format, ".", TypeKey, typeName)] = createHandler;
            }
            else
            {
                _handlerMap[string.Concat(TypeKey, typeName)] = createHandler;
            }
        }

        private ITypeHandler GetEnumHandler(Type enumType, Properties? properties)
        {
            var handler = new EnumTypeHandler(enumType);
            if (properties != null)
                handler.Configure(properties);
            return handler;
        }

        private ITypeHandler? GetHandler(string key, string? format, Properties? properties)
        {
            var factory = this;
            while (factory != null)
            {
                if (format != null && factory._handlerMap.TryGetValue(format + "." + key, out var createHandler))
                    return GetHandler(createHandler, properties);

                if (factory._handlerMap.TryGetValue(key, out createHandler))
                    return GetHandler(createHandler, properties);

                factory = factory._parent;
            }

            return null;
        }

        private ITypeHandler GetHandler(Func<ITypeHandler> createHandler, Properties? properties)
        {
            var handler = createHandler();
            if (properties != null && properties.Count != 0)
            {
                var configurableHandler = handler as IConfigurableTypeHandler;
                if (configurableHandler == null)
                {
                    throw new BeanIOConfigurationException(
                        $"'{properties.First().Key}' setting not supported by type handler with target type '{handler.TargetType}'");
                }

                configurableHandler.Configure(properties);
            }

            return handler;
        }
    }
}
