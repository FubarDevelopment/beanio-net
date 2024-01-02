// <copyright file="DelimitedStreamFormat.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BeanIO.Stream;

namespace BeanIO.Internal.Parser.Format.Delimited
{
    /// <summary>
    /// A <see cref="StreamFormatSupport"/> implementation for the delimited stream format.
    /// </summary>
    internal class DelimitedStreamFormat : StreamFormatSupport
    {
        /// <inheritdoc />
        public override required IRecordParserFactory RecordParserFactory { get; set; }

        /// <inheritdoc />
        public override UnmarshallingContext CreateUnmarshallingContext(IMessageFactory messageFactory)
        {
            return new DelimitedUnmarshallingContext()
            {
                MessageFactory = messageFactory,
            };
        }

        /// <summary>
        /// Creates a new marshalling context.
        /// </summary>
        /// <param name="streaming">true if marshalling to a stream.</param>
        /// <returns>the new <see cref="MarshallingContext"/>.</returns>
        public override MarshallingContext CreateMarshallingContext(bool streaming)
        {
            return new DelimitedMarshallingContext();
        }
    }
}
