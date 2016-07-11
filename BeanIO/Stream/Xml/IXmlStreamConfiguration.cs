// <copyright file="IXmlStreamConfiguration.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Xml.Linq;

namespace BeanIO.Stream.Xml
{
    /// <summary>
    /// This interface provides access to the XMl stream definition for XML
    /// <see cref="IRecordParserFactory"/> classes that implement <see cref="IXmlStreamConfigurationAware"/>.
    /// </summary>
    public interface IXmlStreamConfiguration
    {
        /// <summary>
        /// Gets the base document object model that defines the group structure
        /// of the XML read from an input stream.
        /// </summary>
        /// <remarks>
        /// The returned DOM object should only be used to parse a single stream.
        /// </remarks>
        XDocument CreateDocument();
    }
}
