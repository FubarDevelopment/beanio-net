// <copyright file="IDebuggable.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.IO;

namespace BeanIO.Internal.Util
{
    /// <summary>
    /// Interface implemented by marshallers and unmarshallers for debugging
    /// BeanIO's compiled configuration.
    /// </summary>
    /// <remarks>
    /// The information displayed by these methods may be changed without notice.
    /// </remarks>
    internal interface IDebuggable
    {
        /// <summary>
        /// Prints the internal view of the stream configuration.
        /// </summary>
        /// <param name="writer">the <see cref="TextWriter"/> to write to.</param>
        void Debug(TextWriter writer);
    }
}
