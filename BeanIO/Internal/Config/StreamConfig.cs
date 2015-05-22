using System.Collections.Generic;

using BeanIO.Builder;
using BeanIO.Stream;

namespace BeanIO.Internal.Config
{
    public class StreamConfig : GroupConfig
    {
        private readonly List<TypeHandlerConfig> _typeHandlerConfigs = new List<TypeHandlerConfig>();

        public StreamConfig()
        {
            MinOccurs = 0;
            MaxOccurs = 1;
            Order = 1;
        }

        /// <summary>
        /// Gets the component type
        /// </summary>
        /// <returns>
        /// One of <see cref="F:ComponentType.Group"/>,
        /// <see cref="F:ComponentType.Record"/>, <see cref="F:ComponentType.Segment"/>
        /// <see cref="F:ComponentType.Field"/>, <see cref="F:ComponentType.Constant"/>,
        /// <see cref="F:ComponentType.Wrapper"/>, or <see cref="F:ComponentType.Stream"/>
        /// </returns>
        public override ComponentType ComponentType
        {
            get { return ComponentType.Stream; }
        }

        /// <summary>
        /// Gets or sets the format of this stream.
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the allowed mode(s) of operation for this stream.
        /// </summary>
        public AccessMode? Mode { get; set; }

        /// <summary>
        /// Gets a list of customized type handlers configured for this stream.
        /// </summary>
        public IReadOnlyList<TypeHandlerConfig> Handlers
        {
            get { return _typeHandlerConfigs; }
        }

        /// <summary>
        /// Gets or sets the record parser factory configuration bean.
        /// </summary>
        public BeanConfig<IRecordParserFactory> ParserFactory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether BeanIO should calculate and enforce strict record ordering
        /// (based on the order records are declared in the mapping file) and record length
        /// (based on configured field occurrences).
        /// </summary>
        public bool IsStrict { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore unidentified records.
        /// </summary>
        public bool IgnoreUnidentifiedRecords { get; set; }

        /// <summary>
        /// Adds a custom type handler to this stream.
        /// </summary>
        /// <param name="handler">the type handler to add</param>
        public void AddHandler(TypeHandlerConfig handler)
        {
            _typeHandlerConfigs.Add(handler);
        }
    }
}
