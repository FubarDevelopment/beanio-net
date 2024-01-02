// <copyright file="IMarshaller.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace BeanIO
{
    /// <summary>
    /// Interface for marshalling bean objects.
    /// </summary>
    /// <remarks>
    /// <para>A <see cref="IMarshaller"/> can be used to marshal a bean object bound to
    /// a record in a mapping file.  Marshalling bean objects that span multiple
    /// records is not supported and will cause a <see cref="BeanWriterException"/>.</para>
    /// <para>Depending on the stream format, a bean object can be marshalled to one or more
    /// formats.  All stream formats support marshalling to a <c>String</c> value,
    /// as shown in the following example:</para>
    /// <c>
    /// marshaller.Marshal(bean).ToString();
    /// </c>
    /// <para>A <see cref="IMarshaller"/> instance is stateful. If a BeanIO mapping file declares
    /// record ordering and expected occurrences, a <see cref="BeanWriterException"/> may be thrown for
    /// bean objects written out of sequence or that have exceeded a record's maximum occurrences.</para>
    /// <para>There is some performance benefit for reusing the same <see cref="IMarshaller"/> instance,
    /// but a <see cref="IMarshaller"/> is not thread safe and should not be used to concurrently
    /// marshal multiple bean objects.</para>
    /// </remarks>
    public interface IMarshaller
    {
        /// <summary>
        /// Marshals a single bean object.
        /// </summary>
        /// <param name="bean">The bean object to marshal.</param>
        /// <returns>This <see cref="IMarshaller"/>.</returns>
        /// <exception cref="BeanWriterException">if a record is not matched for the given bean object,
        /// or in some other rare (but fatal) conditions</exception>
        /// <exception cref="InvalidBeanException">if BeanIO is configured to validate fields during marshalling,
        /// and a field does not meet the configured validation rules</exception>
        IMarshaller Marshal(object bean);

        /// <summary>
        /// Marshals a single bean object.
        /// </summary>
        /// <param name="recordName">The name of the record to marshal.</param>
        /// <param name="bean">The bean object to marshal.</param>
        /// <returns>This <see cref="IMarshaller"/>.</returns>
        /// <exception cref="BeanWriterException">if a record is not matched for the given bean object,
        /// or in some other rare (but fatal) conditions</exception>
        /// <exception cref="InvalidBeanException">if BeanIO is configured to validate fields during marshalling,
        /// and a field does not meet the configured validation rules</exception>
        IMarshaller Marshal(string recordName, object? bean);

        /// <summary>
        /// Returns the most recent marshalled bean object as an array for CSV
        /// and delimited formatted streams.
        /// </summary>
        /// <returns>The <see cref="string"/> array of fields.</returns>
        string[] AsArray();

        /// <summary>
        /// Returns the most recent marshalled bean object as an <see cref="IList{T}"/> for CSV
        /// and delimited formatted streams.
        /// </summary>
        /// <returns>The <see cref="string"/> list of fields.</returns>
        IList<string> AsList();

        /// <summary>
        /// Returns the most recent marshalled bean object as an <see cref="XDocument"/> for XML
        /// formatted streams.
        /// </summary>
        /// <returns>The <see cref="XDocument"/>.</returns>
        XDocument AsDocument();
    }
}
