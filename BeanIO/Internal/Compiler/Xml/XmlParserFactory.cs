using System;

using BeanIO.Internal.Config;
using BeanIO.Internal.Parser;
using BeanIO.Internal.Parser.Format.Xml;
using BeanIO.Stream;

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
            ((XmlStreamFormat)stream.Format).Layout = stream.Layout;
            ((XmlStreamFormat)stream.Format).GroupDepth = _maxGroupDepth;
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
            throw new NotImplementedException();
        }

        protected override IStreamFormat CreateStreamFormat(StreamConfig config)
        {
            throw new NotImplementedException();
        }

        protected override IRecordFormat CreateRecordFormat(RecordConfig config)
        {
            throw new NotImplementedException();
        }

        protected override IFieldFormat CreateFieldFormat(FieldConfig config, Type type)
        {
        }
    }
}
