// <copyright file="XmlMappingReader.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Xml;
using System.Xml.Linq;

namespace BeanIO.Internal.Config.Xml
{
    /// <summary>
    /// Reads a BeanIO XML mapping file into an XML document object model (DOM).
    /// </summary>
    internal class XmlMappingReader
    {
        /// <summary>
        /// Parses an XML BeanIO mapping file into a document object model (DOM).
        /// </summary>
        /// <param name="input"> input stream to read.</param>
        /// <returns>the resulting DOM.</returns>
        public virtual XDocument LoadDocument(System.IO.Stream input)
        {
            var readerSettings = new XmlReaderSettings()
                {
                    IgnoreComments = true,
                };
            var reader = XmlReader.Create(input, readerSettings);
            return XDocument.Load(reader, LoadOptions.SetLineInfo);
        }
    }
}
