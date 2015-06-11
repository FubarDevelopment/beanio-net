using System;
using System.ComponentModel;

using BeanIO.Internal.Config;
using BeanIO.Internal.Util;
using BeanIO.Stream;
using BeanIO.Types;

using JetBrains.Annotations;

namespace BeanIO.Builder
{
    /// <summary>
    /// Builds a new stream configuration.
    /// </summary>
    public class StreamBuilder : GroupBuilderSupport<StreamBuilder, StreamConfig>
    {
        private StreamConfig _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamBuilder"/> class.
        /// </summary>
        /// <param name="name">The stream name</param>
        public StreamBuilder(string name)
        {
            _config = new StreamConfig()
                {
                    Name = name,
                };
        }

        /// <summary>
        /// Gets this.
        /// </summary>
        protected override StreamBuilder Me
        {
            get { return this; }
        }

        /// <summary>
        /// Gets the configuration settings.
        /// </summary>
        protected override StreamConfig Config
        {
            get { return _config; }
        }

        /// <summary>
        /// Sets the stream format.
        /// </summary>
        /// <param name="format">the format (e.g. csv, delimited, fixedlength, xml)</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public StreamBuilder Format(string format)
        {
            Config.Format = format;
            return Me;
        }

        /// <summary>
        /// Sets the parser for this stream.
        /// </summary>
        /// <param name="parser">the stream parser factory</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public StreamBuilder Parser(IRecordParserFactory parser)
        {
            var bc = new BeanConfig<IRecordParserFactory>()
                {
                    CreateFunc = () => parser,
                };
            Config.ParserFactory = bc;
            return Me;
        }

        /// <summary>
        /// Sets the parser for this stream.
        /// </summary>
        /// <param name="parser">the <see cref="IParserBuilder"/></param>
        /// <returns>The value of <see cref="Me"/></returns>
        public StreamBuilder Parser(IParserBuilder parser)
        {
            Config.ParserFactory = parser.Build();
            return Me;
        }

        /// <summary>
        /// Adds a type handler
        /// </summary>
        /// <param name="name">the name of the type handler</param>
        /// <param name="createFunc">the type handler creation function</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public StreamBuilder AddTypeHandler([CanBeNull] string name, [NotNull] Func<ITypeHandler> createFunc)
        {
            var thc = new TypeHandlerConfig()
            {
                Name = name ?? createFunc().TargetType.GetAssemblyQualifiedName(),
                CreateFunc = createFunc,
            };
            Config.AddHandler(thc);
            return Me;
        }

        /// <summary>
        /// Adds a type handler
        /// </summary>
        /// <param name="createFunc">the type handler creation function</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public StreamBuilder AddTypeHandler(Func<ITypeHandler> createFunc)
        {
            return AddTypeHandler(null, createFunc);
        }

        /// <summary>
        /// Indicates this stream configuration is only used for unmarshalling.
        /// </summary>
        /// <returns>The value of <see cref="Me"/></returns>
        public StreamBuilder ReadOnly()
        {
            Config.Mode = AccessMode.Read;
            return Me;
        }

        /// <summary>
        /// Indicates this stream configuration is only used for marshalling.
        /// </summary>
        /// <returns>The value of <see cref="Me"/></returns>
        public StreamBuilder WriteOnly()
        {
            Config.Mode = AccessMode.Write;
            return Me;
        }

        /// <summary>
        /// Indicates this stream should be strictly validated.
        /// </summary>
        /// <returns>The value of <see cref="Me"/></returns>
        public StreamBuilder Strict()
        {
            Config.IsStrict = true;
            return Me;
        }

        /// <summary>
        /// Sets the streams name conversion mode.
        /// </summary>
        /// <param name="conversionMode">The conversion mode to set</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public StreamBuilder NameConversionMode(ElementNameConversionMode conversionMode)
        {
            Config.NameConversionMode = conversionMode;
            return Me;
        }

        /// <summary>
        /// Indicates unidentified records should be ignored during unmarshalling.
        /// </summary>
        /// <returns>The value of <see cref="Me"/></returns>
        public StreamBuilder IgnoreUnidentifiedRecords()
        {
            Config.IgnoreUnidentifiedRecords = true;
            return Me;
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="type">Type name</param>
        /// <returns>The value of <see cref="PropertyBuilderSupport{T,TConfig}.Me"/></returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override StreamBuilder Type(Type type)
        {
            throw new BeanIOConfigurationException("Type not supported by StreamBuilder");
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="type">Collection or map type name</param>
        /// <returns>The value of <see cref="PropertyBuilderSupport{T,TConfig}.Me"/></returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override StreamBuilder Collection(Type type)
        {
            throw new BeanIOConfigurationException("Collection not supported by StreamBuilder");
        }

        /// <summary>
        /// Builds the stream configuration.
        /// </summary>
        /// <returns>The stream configuration</returns>
        public StreamConfig Build()
        {
            return Config;
        }

        /// <summary>
        /// Sets the configuration settings
        /// </summary>
        /// <param name="config">The configuration settings</param>
        protected void SetConfig(StreamConfig config)
        {
            _config = config;
        }
    }
}
