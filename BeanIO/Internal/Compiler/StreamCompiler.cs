using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

using BeanIO.Config;
using BeanIO.Internal.Config;
using BeanIO.Internal.Config.Xml;
using BeanIO.Internal.Util;
using BeanIO.Types;

using JetBrains.Annotations;

namespace BeanIO.Internal.Compiler
{
    /// <summary>
    /// Compiles a mapping file read from an <see cref="System.IO.Stream"/> into a collection of
    /// <see cref="Parser.Stream"/> parsers.
    /// </summary>
    internal class StreamCompiler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamCompiler"/> class.
        /// </summary>
        public StreamCompiler()
        {
            DefaultConfigurationLoader = new XmlConfigurationLoader();
        }

        /// <summary>
        /// Gets the default mapping configuration loader
        /// </summary>
        public IConfigurationLoader DefaultConfigurationLoader { get; private set; }

        /// <summary>
        /// Gets or sets the mapping configuration loader
        /// </summary>
        public IConfigurationLoader ConfigurationLoader { get; set; }

        /// <summary>
        /// Creates a new Stream from its configuration
        /// </summary>
        /// <param name="config">the <see cref="StreamConfig"/></param>
        /// <returns>the built <see cref="Parser.Stream"/> definition</returns>
        public Parser.Stream Build(StreamConfig config)
        {
            var typeHandlerFactory = CreateTypeHandlerFactory(TypeHandlerFactory.Default, config.Handlers);
            var factory = CreateParserFactory(config.Format);
            factory.TypeHandlerFactory = typeHandlerFactory;
            return factory.CreateStream(config);
        }

        /// <summary>
        /// Loads a mapping file
        /// </summary>
        /// <param name="input">the <see cref="System.IO.Stream"/> to load the mapping file from</param>
        /// <param name="properties">the <see cref="Properties"/></param>
        /// <returns>the <see cref="Parser.Stream"/> parsers configured in the loaded mapping file</returns>
        public IReadOnlyCollection<Parser.Stream> LoadMapping(System.IO.Stream input, Properties properties)
        {
            var loader = ConfigurationLoader ?? DefaultConfigurationLoader;

            var configurations = loader.LoadConfiguration(input, properties);
            if (configurations.Count == 0)
                return ObjectUtils.Empty<Parser.Stream>();

            // check for duplicate stream names...
            {
                var set = new HashSet<string>();
                foreach (var streamConfig in configurations.SelectMany(x => x.StreamConfigurations))
                {
                    if (!set.Add(streamConfig.Name))
                        throw new BeanIOConfigurationException(string.Format("Duplicate stream name '{0}'", streamConfig.Name));
                }
            }

            if (configurations.Count == 1)
                return CreateStreamDefinitions(configurations.Single()).ToList();

            var list = configurations.SelectMany(CreateStreamDefinitions).ToList();
            return list;
        }

        /// <summary>
        /// Creates stream definitions from a BeanIO stream mapping configuration
        /// </summary>
        /// <param name="config">the BeanIO stream mapping configuration</param>
        /// <returns>the collection of stream definitions</returns>
        protected IEnumerable<Parser.Stream> CreateStreamDefinitions([NotNull] BeanIOConfig config)
        {
            if (config == null)
                throw new ArgumentNullException("config");
            Contract.EndContractBlock();

            var parent = CreateTypeHandlerFactory(TypeHandlerFactory.Default, config.TypeHandlerConfigurations);
            var streamDefinitions = new List<Parser.Stream>(config.StreamConfigurations.Count);

            foreach (var streamConfiguration in config.StreamConfigurations)
            {
                var typeHandlerFactory = CreateTypeHandlerFactory(parent, streamConfiguration.Handlers);

                var factory = CreateParserFactory(streamConfiguration.Format);
                factory.TypeHandlerFactory = typeHandlerFactory;

                try
                {
                    streamDefinitions.Add(factory.CreateStream(streamConfiguration));
                }
                catch (BeanIOConfigurationException ex)
                {
                    // TODO: Use C# 6 exception filter
                    if (config.Source != null)
                        throw new BeanIOConfigurationException(string.Format("Invalid mapping file '{0}': {1}", config.Source, ex.Message), ex);
                    throw;
                }
            }

            return streamDefinitions;
        }

        /// <summary>
        /// Instantiates the factory implementation to create the stream definition
        /// </summary>
        /// <param name="format">the stream format</param>
        /// <returns>the stream definition factory</returns>
        protected IParserFactory CreateParserFactory(string format)
        {
            var propertyName = string.Format("org.beanio.{0}.streamDefinitionFactory", format);
            var typeName = Settings.Instance[propertyName];

            if (typeName == null)
                throw new BeanIOConfigurationException(string.Format("A stream definition factory is not configured for format '{0}'", format));

            var factory = BeanUtil.CreateBean(typeName) as IParserFactory;
            if (factory == null)
                throw new BeanIOConfigurationException(string.Format("Configured stream definition factory '{0}' does not implement '{1}'", typeName, typeof(IParserFactory)));

            return factory;
        }

        /// <summary>
        /// Creates a type handler factory for a list of configured type handlers
        /// </summary>
        /// <param name="parent">the parent <see cref="TypeHandlerFactory"/></param>
        /// <param name="typeHandlerConfigurations">the list of type handler configurations</param>
        /// <returns>the new <see cref="TypeHandlerFactory"/>, or <code>parent</code> if the configuration list was empty</returns>
        private TypeHandlerFactory CreateTypeHandlerFactory(TypeHandlerFactory parent, IReadOnlyCollection<TypeHandlerConfig> typeHandlerConfigurations)
        {
            if (typeHandlerConfigurations == null || typeHandlerConfigurations.Count == 0)
                return parent;

            var factory = new TypeHandlerFactory(parent);

            foreach (var handlerConfig in typeHandlerConfigurations)
            {
                if (handlerConfig.Name == null && handlerConfig.Type == null)
                    throw new BeanIOConfigurationException("Type handler must specify either 'type' or 'name'");

                var createFunc = handlerConfig.Create;
                if (createFunc == null)
                {
                    object bean;
                    try
                    {
                        bean = BeanUtil.CreateBean(handlerConfig.ClassName, handlerConfig.Properties);
                    }
                    catch (BeanIOConfigurationException ex)
                    {
                        if (handlerConfig.Name != null)
                            throw new BeanIOConfigurationException(string.Format("Failed to create type handler named '{0}'", handlerConfig.Name), ex);
                        throw new BeanIOConfigurationException(string.Format("Failed to create type handler for type '{0}'", handlerConfig.Type), ex);
                    }

                    // validate the configured class is assignable to the target class
                    if (!(bean is ITypeHandler))
                        throw new BeanIOConfigurationException(string.Format("Type handler class '{0}' does not implement TypeHandler interface", handlerConfig.ClassName));

                    var funcHandlerConfig = handlerConfig;
                    createFunc = () => (ITypeHandler)BeanUtil.CreateBean(funcHandlerConfig.ClassName, funcHandlerConfig.Properties);
                }

                if (handlerConfig.Name != null)
                    factory.RegisterHandler(handlerConfig.Name, createFunc);

                if (handlerConfig.Type != null)
                {
                    try
                    {
                        // type handlers configured for java types may be registered for a specific stream format
                        factory.RegisterHandlerFor(handlerConfig.Type, createFunc, handlerConfig.Format);
                    }
                    catch (Exception ex)
                    {
                        throw new BeanIOConfigurationException("Invalid type handler configuration", ex);
                    }
                }
            }

            return factory;
        }
    }
}
