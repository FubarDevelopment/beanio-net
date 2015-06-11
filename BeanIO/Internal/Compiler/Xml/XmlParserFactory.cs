using System;
using System.Xml;

using BeanIO.Internal.Config;
using BeanIO.Internal.Parser;
using BeanIO.Internal.Parser.Format;
using BeanIO.Internal.Parser.Format.Xml;
using BeanIO.Stream;
using BeanIO.Stream.Xml;

namespace BeanIO.Internal.Compiler.Xml
{
    /// <summary>
    /// A <see cref="IParserFactory"/> for the XML stream format.
    /// </summary>
    public class XmlParserFactory : ParserFactorySupport
    {
        /// <summary>
        /// the current depth of the parser tree
        /// </summary>
        private int _groupDepth;

        private int _maxGroupDepth;

        /// <summary>
        /// Creates a new stream parser from a given stream configuration
        /// </summary>
        /// <param name="config">the stream configuration</param>
        /// <returns>the created <see cref="Parser.Stream"/></returns>
        public override Parser.Stream CreateStream(StreamConfig config)
        {
            var stream = base.CreateStream(config);
            var xmlStreamFormat = (XmlStreamFormat)stream.Format;
            xmlStreamFormat.Layout = stream.Layout;
            xmlStreamFormat.GroupDepth = _maxGroupDepth;
            xmlStreamFormat.NameConversionMode = config.NameConversionMode;
            return stream;
        }

        /// <summary>
        /// Creates a stream configuration pre-processor
        /// </summary>
        /// <remarks>May be overridden to return a format specific version</remarks>
        /// <param name="config">the stream configuration to pre-process</param>
        /// <returns>the new <see cref="Preprocessor"/></returns>
        protected override Preprocessor CreatePreprocessor(StreamConfig config)
        {
            return new XmlPreprocessor(config);
        }

        /// <summary>
        /// Creates the default <see cref="IRecordParserFactory"/>.
        /// </summary>
        /// <returns>
        /// The new <see cref="IRecordParserFactory"/>
        /// </returns>
        protected override IRecordParserFactory CreateDefaultRecordParserFactory()
        {
            return new XmlRecordParserFactory();
        }

        protected override IStreamFormat CreateStreamFormat(StreamConfig config)
        {
            var format = new XmlStreamFormat()
                {
                    Name = config.Name,
                    RecordParserFactory = CreateRecordParserFactory(config),
                };
            return format;
        }

        protected override IRecordFormat CreateRecordFormat(RecordConfig config)
        {
            return null;
        }

        protected override IFieldFormat CreateFieldFormat(FieldConfig config, Type type)
        {
            XmlFieldFormat format;
            if (config.XmlType == XmlNodeType.Element)
            {
                var element = new XmlElementField();
                element.SetLocalName(config.XmlName);
                element.SetNillable(config.IsNillable);
                element.SetNamespace(config.XmlNamespace);
                element.SetNamespaceAware(config.IsXmlNamespaceAware);
                element.SetPrefix(config.XmlPrefix);
                element.SetRepeating(config.IsRepeating);
                format = element;
            }
            else if (config.XmlType == XmlNodeType.Attribute)
            {
                var attribute = new XmlAttributeField();
                attribute.SetLocalName(config.XmlName);
                attribute.SetNamespace(config.XmlNamespace);
                attribute.SetNamespaceAware(config.IsXmlNamespaceAware);
                attribute.SetPrefix(config.XmlPrefix);
                format = attribute;
            }
            else if (config.XmlType == XmlNodeType.Text)
            {
                var text = new XmlTextField();
                format = text;
            }
            else
            {
                throw new InvalidOperationException(string.Format("Invalid xml type: {0}", config.XmlType));
            }

            format.Name = config.Name;
            format.IsLazy = config.MinOccurs.GetValueOrDefault() == 0;

            if (config.Length != null)
            {
                var padding = new FieldPadding
                    {
                        Length = config.Length.Value,
                        Justify = config.Justify,
                        IsOptional = !config.IsRequired,
                        PropertyType = type,
                    };
                padding.Filler = config.Padding ?? padding.Filler;
                padding.Init();
                format.Padding = padding;
            }

            return format;
        }

        protected override void InitializeGroupMain(GroupConfig config, IProperty property)
        {
            if (config.XmlType != null && config.XmlType != XmlNodeType.None)
            {
                var wrapper = new XmlSelectorWrapper()
                    {
                        Name = config.Name,
                        LocalName = config.XmlName,
                        Namespace = config.XmlNamespace,
                        IsNamespaceAware = config.IsXmlNamespaceAware,
                        Prefix = config.XmlPrefix,
                        IsGroup = true,
                        Depth = _groupDepth++,
                    };
                PushParser(wrapper);
                _maxGroupDepth = Math.Max(_groupDepth, _maxGroupDepth);
            }
            base.InitializeGroupMain(config, property);
        }

        protected override IProperty FinalizeGroupMain(GroupConfig config)
        {
            var property = base.FinalizeGroupMain(config);
            if (config.XmlType != null && config.XmlType != XmlNodeType.None)
            {
                PopParser();
                --_groupDepth;
            }
            return property;
        }

        protected override void InitializeRecordMain(RecordConfig config, IProperty property)
        {
            // a record is always mapped to an XML element
            var wrapper = new XmlSelectorWrapper()
                {
                    Name = config.Name,
                    LocalName = config.XmlName,
                    Namespace = config.XmlNamespace,
                    IsNamespaceAware = config.IsXmlNamespaceAware,
                    Prefix = config.XmlPrefix,
                    IsGroup = false,
                    Depth = _groupDepth,
                };

            PushParser(wrapper);

            base.InitializeRecordMain(config, property);
        }

        protected override IProperty FinalizeRecordMain(RecordConfig config)
        {
            var property = base.FinalizeRecordMain(config);
            PopParser();
            return property;
        }

        /// <summary>
        /// Invoked by <see cref="ParserFactorySupport.FinalizeRecord(BeanIO.Internal.Config.RecordConfig)"/> to allow subclasses to perform
        /// further finalization of the created <see cref="Record"/>.
        /// </summary>
        /// <param name="config">the record configuration</param>
        /// <param name="record">the <see cref="Record"/> being finalized</param>
        protected override void FinalizeRecord(RecordConfig config, Record record)
        {
            base.FinalizeRecord(config, record);
            record.IsExistencePredetermined = true;
        }

        protected override bool IsSegmentRequired(SegmentConfig config)
        {
            if (config.IsConstant)
                return false;
            if (config.Type != null)
                return true;
            if (config.XmlType != XmlNodeType.Element)
                return false;
            if (config.Children.Count > 1)
                return true;
            return false;
        }

        /// <summary>
        /// Called by <see cref="ParserFactorySupport.InitializeSegment"/> to initialize the segment.
        /// </summary>
        /// <param name="config">the segment configuration</param>
        /// <param name="property">the property bound to the segment, or null if no property was bound</param>
        protected override void InitializeSegmentMain(SegmentConfig config, IProperty property)
        {
            if (IsWrappingRequired(config))
            {
                var wrapper = new XmlWrapper
                    {
                        Name = config.Name,
                        LocalName = config.XmlName,
                        Namespace = config.XmlNamespace,
                        IsNamespaceAware = config.IsXmlNamespaceAware,
                        Prefix = config.XmlPrefix,
                        IsNillable = config.IsNillable,
                        IsRepeating = config.IsRepeating,
                        IsLazy = config.MinOccurs.GetValueOrDefault() == 0,
                    };
                PushParser(wrapper);
            }
            base.InitializeSegmentMain(config, property);
        }

        /// <summary>
        /// Called by <see cref="ParserFactorySupport.FinalizeSegment(BeanIO.Internal.Config.SegmentConfig)"/> to finalize the segment component.
        /// </summary>
        /// <param name="config">the segment configuration</param>
        /// <returns>the target property</returns>
        protected override IProperty FinalizeSegmentMain(SegmentConfig config)
        {
            var property = base.FinalizeSegmentMain(config);
            if (IsWrappingRequired(config))
            {
                // pop the wrapper
                PopParser();
            }
            return property;
        }

        /// <summary>
        /// Invoked by <see cref="ParserFactorySupport.FinalizeSegmentMain"/> to allow subclasses to perform
        /// further finalization of the created <see cref="Segment"/>.
        /// </summary>
        /// <param name="config">the segment configuration</param>
        /// <param name="segment">the new <see cref="Segment"/></param>
        protected override void FinalizeSegment(SegmentConfig config, Segment segment)
        {
            base.FinalizeSegment(config, segment);

            // if the segment is wrapped, laziness is checked by the wrapper
            if (config.XmlType == XmlNodeType.Element)
                segment.SetOptional(false);

            segment.IsExistencePredetermined = true;
        }

        private bool IsWrappingRequired(SegmentConfig config)
        {
            return config.XmlType == XmlNodeType.Element && !config.IsConstant;
        }
    }
}
