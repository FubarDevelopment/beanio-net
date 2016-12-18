// <copyright file="FixedLengthPreprocessor.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BeanIO.Config;
using BeanIO.Internal.Compiler.Flat;
using BeanIO.Internal.Config;

namespace BeanIO.Internal.Compiler.FixedLength
{
    /// <summary>
    /// Configuration <see cref="Preprocessor"/> for a fixed length stream format.
    /// </summary>
    internal class FixedLengthPreprocessor : FlatPreprocessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FixedLengthPreprocessor"/> class.
        /// </summary>
        /// <param name="settings">The configuration settings</param>
        /// <param name="stream">the stream configuration to pre-process</param>
        public FixedLengthPreprocessor(ISettings settings, StreamConfig stream)
            : base(settings, stream)
        {
        }

        /// <summary>
        /// Gets a value indicating whether the stream format is fixed length.
        /// </summary>
        protected override bool IsFixedLength => true;

        /// <summary>
        /// Returns the size of a field.
        /// </summary>
        /// <remarks>null = unbounded</remarks>
        /// <param name="field">the field to get the size from</param>
        /// <returns>the field size</returns>
        protected override int? GetSize(FieldConfig field)
        {
            return field.Length;
        }
    }
}
