using System;

using BeanIO.Internal.Compiler.Flat;
using BeanIO.Internal.Config;
using BeanIO.Internal.Parser;
using BeanIO.Internal.Parser.Format;
using BeanIO.Internal.Parser.Format.Delimited;
using BeanIO.Stream;
using BeanIO.Stream.Delimited;

namespace BeanIO.Internal.Compiler.Delimited
{
    public class DelimitedParserFactory : FlatParserFactory
    {
        /// <summary>
        /// Creates the default <see cref="IRecordParserFactory"/>.
        /// </summary>
        /// <returns>
        /// The new <see cref="IRecordParserFactory"/>
        /// </returns>
        protected override IRecordParserFactory CreateDefaultRecordParserFactory()
        {
            return new DelimitedRecordParserFactory();
        }

        protected override IStreamFormat CreateStreamFormat(StreamConfig config)
        {
            var format = new DelimitedStreamFormat
                {
                    Name = config.Name,
                    RecordParserFactory = CreateRecordParserFactory(config)
                };
            return format;
        }

        protected override IRecordFormat CreateRecordFormat(RecordConfig config)
        {
            var format = new DelimitedRecordFormat();

            if (config.MinLength != null)
                format.MinLength = config.MinLength ?? 0;
            if (config.MaxLength != null)
                format.MaxLength = config.MaxLength;
            if (config.MinMatchLength != null)
                format.MinMatchLength = config.MinMatchLength ?? 0;
            if (config.MaxMatchLength != null)
                format.MaxMatchLength = config.MaxMatchLength;

            return format;
        }

        protected override IFieldFormat CreateFieldFormat(FieldConfig config, Type type)
        {
            var format = new DelimitedFieldFormat()
                {
                    Name = config.Name,
                    Until = config.Until.GetValueOrDefault(),
                };

            if (config.Length != null)
            {
                var padding = new FieldPadding()
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

            // TODO why was isBound checked here?
            ////if (config.IsBound) {
            format.Position = config.Position.GetValueOrDefault();
            ////}

            format.IsLazy = config.MinOccurs.GetValueOrDefault() == 0;

            return format;
        }
    }
}
