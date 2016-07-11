// <copyright file="StreamFactoryTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Reflection;

using Xunit;

namespace BeanIO
{
    public class StreamFactoryTest
    {
        [Fact]
        public void TestLoadMappingFile()
        {
            using (var mappingStream = typeof(StreamFactoryTest).GetTypeInfo().Assembly.GetManifestResourceStream("BeanIO.Test.mapping.xml"))
            {
                var factory = StreamFactory.NewInstance();
                factory.Load(mappingStream);
            }
        }
    }
}
