// <copyright file="XmlParserTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Xml.Linq;

using Xunit;

namespace BeanIO.Parser.Xml
{
    /// <summary>
    /// Base class for XML parser XUnit test cases.
    /// </summary>
    public class XmlParserTest : ParserTest
    {
        /// <summary>
        /// Compares expected and actual XML documents using the <see cref="XNodeEqualityComparer"/>
        /// </summary>
        /// <param name="expected">the expected XML document</param>
        /// <param name="actual">the actual XML document</param>
        public void AssertXmlEquals(string expected, string actual)
        {
            var d1 = XDocument.Parse(expected, LoadOptions.SetLineInfo);
            var d2 = XDocument.Parse(actual, LoadOptions.SetLineInfo);
            var comparer = new XNodeEqualityComparer();
            Assert.True(comparer.Equals(d1, d2));
        }
    }
}
