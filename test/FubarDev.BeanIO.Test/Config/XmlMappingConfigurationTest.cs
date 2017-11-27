// <copyright file="XmlMappingConfigurationTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Reflection;

using BeanIO.Parser;

using Xunit;

namespace BeanIO.Config
{
    public class XmlMappingConfigurationTest : ParserTest
    {
        [Fact]
        public void TestTemplateImport()
        {
            var factory = StreamFactory.NewInstance();
            using (var stream = typeof(ParserTest).GetTypeInfo().Assembly.GetManifestResourceStream("BeanIO.Config.ab.xml"))
            {
                factory.Load(stream);
            }
        }

        [Fact]
        public void TestImport()
        {
            var factory = StreamFactory.NewInstance();
            using (var stream = typeof(ParserTest).GetTypeInfo().Assembly.GetManifestResourceStream("BeanIO.Config.import.xml"))
            {
                factory.Load(stream);
            }
        }

        [Fact(DisplayName = "Resource not found")]
        public void TestInvalidImportResourceNotFound()
        {
            LoadInvalidMappingFile("invalidImport_ResourceNotFound.xml", "Resource 'resource:BeanIO.Config.doesnotexist.xml, FubarDev.BeanIO.Test' not found in classpath for import");
        }

        [Fact(DisplayName = "Invalid resource")]
        public void TestInvalidImportInvalidResource()
        {
            LoadInvalidMappingFile("invalidImport_InvalidResource.xml", "No scheme specified for resource 'BeanIO.Config.imported.xml'");
        }

        [Fact]
        public void TestInvalidGroupXmlType()
        {
            LoadInvalidMappingFile("invalidStreamXmlType.xml", "Invalid xmlType 'Text'");
        }

        [Fact]
        public void TestInvalidBeanXmlType()
        {
            LoadInvalidMappingFile("invalidBeanXmlType.xml", "Invalid xmlType 'Attribute'");
        }

        [Fact]
        public void TestInvalidFieldXmlType()
        {
            LoadInvalidMappingFile("invalidFieldXmlType.xml", "Invalid xmlType 'invalid'");
        }

        [Fact]
        public void TestPrefixWithNoNamespace()
        {
            LoadInvalidMappingFile("prefixWithNoNamespace.xml", "Missing namespace for configured XML prefix");
        }

        [Fact]
        public void TestFieldNamespaceForText()
        {
            LoadInvalidMappingFile("fieldNamespaceForText.xml", "XML namespace is not applicable for xmlType 'Text'");
        }

        [Fact]
        public void TestNillableAttributeField()
        {
            LoadInvalidMappingFile("nillableAttributeField.xml", "xmlType 'Attribute' is not nillable");
        }

        [Fact]
        public void TestCollectionAttributeField()
        {
            LoadInvalidMappingFile("collectionAttributeField.xml", "Repeating fields must have xmlType 'element'");
        }

        [Fact]
        public void TestInvalidStreamMode()
        {
            LoadInvalidMappingFile("invalidStreamMode.xml", "Invalid mode 'xxx'");
        }

        [Fact]
        public void TestInvalidBeanClass()
        {
            LoadInvalidMappingFile("invalidBeanClass.xml", "Class must be concrete unless stream mode is set to 'Write'");
        }

        [Fact]
        public void TestNoReadableMethod()
        {
            var errorMessage = string.Format(
                "No readable access for property or field 'value' in class '{0}'",
                typeof(IInterfaceBean).GetTypeInfo().AssemblyQualifiedName);
            LoadInvalidMappingFile("noReadableMethod.xml", errorMessage);
        }

        [Fact]
        public void TestNoBeanProperty()
        {
            var errorMessage = string.Format(
                "Neither property or field found with name 'birthDate' for type '{0}'",
                typeof(ConcreteBean).GetTypeInfo().AssemblyQualifiedName);
            LoadInvalidMappingFile("noBeanProperty.xml", errorMessage);
        }

        // ReSharper disable once UnusedParameter.Local
        private void LoadInvalidMappingFile(string name, string errorMessage)
        {
            var factory = StreamFactory.NewInstance();
            var asm = typeof(ParserTest).GetTypeInfo().Assembly;
            var stream = asm.GetManifestResourceStream($"BeanIO.Config.{name}");
            Assert.NotNull(stream);
            var ex = Assert.Throws<BeanIOConfigurationException>(() => factory.Load(stream));
            var innermostException = ex.GetBaseException();
            Assert.Equal(errorMessage, innermostException.Message);
        }
    }
}
