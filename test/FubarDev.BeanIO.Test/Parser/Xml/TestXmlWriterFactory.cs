// <copyright file="TestXmlWriterFactory.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BeanIO.Stream.Xml;

namespace BeanIO.Parser.Xml
{
    public class TestXmlWriterFactory : XmlRecordParserFactory
    {
        public TestXmlWriterFactory()
        {
            SuppressHeader = true;
            LineSeparator = "\r\n";
            Indentation = 2;
        }
    }
}
