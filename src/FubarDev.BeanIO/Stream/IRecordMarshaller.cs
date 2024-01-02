// <copyright file="IRecordMarshaller.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BeanIO.Stream
{
    /// <summary>
    /// Interface for marshalling a single record object.
    /// </summary>
    /// <remarks>
    /// The class used to represent a <i>record</i> is specific to the
    /// format of a record.  For example, a delimited record marshaller may use
    /// <c>String[]</c>.
    /// </remarks>
    public interface IRecordMarshaller
    {
        /// <summary>
        /// Marshals a single record object to a <c>String</c>.
        /// </summary>
        /// <param name="record">Record the record object to marshal.</param>
        /// <returns>The marshalled record text.</returns>
        string Marshal(object record);
    }
}
