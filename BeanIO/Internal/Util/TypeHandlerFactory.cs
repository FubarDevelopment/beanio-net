using System;
using System.Collections.Generic;

using BeanIO.Types;

using JetBrains.Annotations;

namespace BeanIO.Internal.Util
{
    public class TypeHandlerFactory
    {
        private static readonly TypeHandlerFactory _defaultFactory;

        private readonly Dictionary<string, ITypeHandler> _handlerMap = new Dictionary<string, ITypeHandler>(StringComparer.OrdinalIgnoreCase);

        private TypeHandlerFactory _parent;

        private const string NameKey = "name:";

        private const string TypeKey = "type:";

        static TypeHandlerFactory()
        {
            _defaultFactory = new TypeHandlerFactory();

            _defaultFactory.RegisterHandlerFor(typeof(char), new CharacterTypeHandler());
            _defaultFactory.RegisterHandlerFor(typeof(string), new StringTypeHandler());
            
            _defaultFactory.RegisterHandlerFor(typeof(sbyte), new NumberTypeHandler(typeof(sbyte)));
            _defaultFactory.RegisterHandlerFor(typeof(short), new NumberTypeHandler(typeof(short)));
            _defaultFactory.RegisterHandlerFor(typeof(int), new NumberTypeHandler(typeof(int)));
            _defaultFactory.RegisterHandlerFor(typeof(long), new NumberTypeHandler(typeof(long)));

            _defaultFactory.RegisterHandlerFor(typeof(byte), new NumberTypeHandler(typeof(byte)));
            _defaultFactory.RegisterHandlerFor(typeof(ushort), new NumberTypeHandler(typeof(ushort)));
            _defaultFactory.RegisterHandlerFor(typeof(uint), new NumberTypeHandler(typeof(uint)));
            _defaultFactory.RegisterHandlerFor(typeof(ulong), new NumberTypeHandler(typeof(ulong)));

            _defaultFactory.RegisterHandlerFor(typeof(float), new NumberTypeHandler(typeof(float)));
            _defaultFactory.RegisterHandlerFor(typeof(double), new NumberTypeHandler(typeof(double)));

            _defaultFactory.RegisterHandlerFor(typeof(decimal), new NumberTypeHandler(typeof(decimal)));

            _defaultFactory.RegisterHandlerFor(typeof(bool), new BooleanTypeHandler());
            _defaultFactory.RegisterHandlerFor(typeof(Guid), new GuidTypeHandler());
            _defaultFactory.RegisterHandlerFor(typeof(Uri), new UrlTypeHandler());
        }

        public void RegisterHandler([NotNull] string name, [NotNull] ITypeHandler handler)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (handler == null)
                throw new ArgumentNullException("handler");
            _handlerMap[NameKey + name] = handler;
        }

        public void RegisterHandlerFor([NotNull] string name, [NotNull] ITypeHandler handler)
        {
            RegisterHandlerFor(name, handler, null);
        }

        public void RegisterHandlerFor([NotNull] string name, [NotNull] ITypeHandler handler, string format)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            var type = name.ToType();
            if (type == null)
                throw new ArgumentException(string.Format("Invalid type or type alias '{0}'", name), "name");

            if (name.IsAliasOnly())
            {
                name = name.ToLowerInvariant();
                RegisterHandlerFor(format, name, type, handler);
            }
            else
            {
                RegisterHandlerFor(format, type.GetFullName(), type, handler);
            }
        }

        public void RegisterHandlerFor([NotNull] Type type, [NotNull] ITypeHandler handler)
        {
            RegisterHandlerFor(type, handler, null);
        }

        public void RegisterHandlerFor([NotNull] Type type, [NotNull] ITypeHandler handler, string format)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            RegisterHandlerFor(format, type.GetFullName(), type, handler);
        }

        private void RegisterHandlerFor([CanBeNull] string format, [NotNull] string typeName, [NotNull] Type expectedClass, [NotNull] ITypeHandler handler)
        {
            if (!expectedClass.IsAssignableFrom(handler.TargetType))
                throw new ArgumentException(string.Format("Type handler of type '{0}' is not assignable from configured type '{1}'", handler.TargetType, expectedClass));

            if (format != null)
            {
                _handlerMap[string.Concat(format, ".", TypeKey, typeName)] = handler;
            }
            else
            {
                _handlerMap[string.Concat(TypeKey, typeName)] = handler;
            }
        }
    }
}
