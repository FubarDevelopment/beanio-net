// <copyright file="StreamFactoryTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

using Xunit;

namespace BeanIO
{
    public class StreamFactoryTest
    {
        [Fact]
        public void TestLoadMappingFile()
        {
            var asm = typeof(StreamFactoryTest).Assembly;
            using var mappingStream = asm.GetManifestResourceStream("BeanIO.mapping.xml");
            Assert.NotNull(mappingStream);
            var factory = StreamFactory.NewInstance();
            factory.Load(mappingStream);
        }
    }
}
