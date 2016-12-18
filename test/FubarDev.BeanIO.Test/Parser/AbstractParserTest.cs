// <copyright file="AbstractParserTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

using BeanIO.Builder;

namespace BeanIO.Parser
{
    public abstract class AbstractParserTest
    {
        public TextReader LoadStream(string fileName)
        {
            var asm = typeof(AbstractParserTest).GetTypeInfo().Assembly;
            var resStream = asm.GetManifestResourceStream(fileName);
            if (resStream == null)
            {
                var testClassNamespace = this.GetType().Namespace.Replace("BeanIO.", "BeanIO.Test.");
                var resourceName = string.Format("{0}.{1}", testClassNamespace, fileName);
                resStream = asm.GetManifestResourceStream(resourceName);
            }

            if (resStream == System.IO.Stream.Null)
                throw new ArgumentOutOfRangeException(nameof(fileName));
            return new StreamReader(resStream);
        }

        protected IBeanReader CreateReader(StreamFactory factory, string input, string name = "s")
        {
            return factory.CreateReader(name, new StringReader(input));
        }

        protected IStreamFactory CreateFactory(string xml = null)
        {
            var factory = StreamFactory.NewInstance();
            if (xml != null)
            {
                xml = "<beanio xmlns=\"http://www.beanio.org/2015/06\">\n" + xml + "\n</beanio>";
                factory.Load(new MemoryStream(Encoding.UTF8.GetBytes(xml)));
            }

            return factory;
        }

        protected IStreamFactory CreateFactory(StreamBuilder builder)
        {
            var factory = StreamFactory.NewInstance();
            if (builder != null)
            {
                factory.Define(builder);
            }

            return factory;
        }
    }
}
