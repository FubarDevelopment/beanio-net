// <copyright file="IRecordUnmarshaller.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BeanIO.Stream
{
    /// <summary>
    /// Interface for unmarshalling a single record.
    /// </summary>
    /// <returns>
    /// The class used to represent a <i>record</i> is specific to the
    /// format of a record.  For example, a delimited record marshaller may use
    /// <code>Stringp[]</code>.
    /// </returns>
    public interface IRecordUnmarshaller
    {
        /// <summary>
        /// Unmarshals a single record.
        /// </summary>
        /// <param name="text">The record text to unmarshal</param>
        /// <returns>The unmarshalled record object</returns>
        object Unmarshal(string text);
    }
}
