// <copyright file="TargetTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;

using BeanIO.Beans;

using Xunit;

namespace BeanIO.Parser.Target
{
    public class TargetTest : ParserTest
    {
        [Fact]
        public void TestTarget()
        {
            var factory = NewStreamFactory("FubarDev.BeanIO.Test.Parser.Target.target_mapping.xml");
            var unmarshaller = factory.CreateUnmarshaller("stream");
            var marshaller = factory.CreateMarshaller("stream");

            var list = Assert.IsType<List<string>>(unmarshaller.Unmarshal("N,kevin,kev,kevo"));
            Assert.Equal(new[] { "kev", "kevo" }, list);
            Assert.Equal("N,,kev,kevo", marshaller.Marshal(list).ToString());

            var age = Assert.IsType<int>(unmarshaller.Unmarshal("A,jen,28"));
            Assert.Equal(28, age);
            Assert.Equal("A,unknown,28", marshaller.Marshal(age).ToString());
        }

        [Fact]
        public void TestSegmentTarget()
        {
            var factory = NewStreamFactory("FubarDev.BeanIO.Test.Parser.Target.target_mapping.xml");
            var unmarshaller = factory.CreateUnmarshaller("t2");
            var marshaller = factory.CreateMarshaller("t2");

            var person = Assert.IsType<Person>(unmarshaller.Unmarshal("john,smith"));
            Assert.Equal("smith", person.LastName);

            Assert.Equal(",smith", marshaller.Marshal(person).ToString());
        }

        [Fact]
        public void TestTargetTargetNotFound()
        {
            Assert.Throws<BeanIOConfigurationException>(() => NewStreamFactory("FubarDev.BeanIO.Test.Parser.Target.targetNotFound.xml"));
        }

        [Fact]
        public void TestTargetRepeatingTarget()
        {
            Assert.Throws<BeanIOConfigurationException>(() => NewStreamFactory("FubarDev.BeanIO.Test.Parser.Target.targetRepeating.xml"));
        }

        [Fact]
        public void TestTargetIsNotProperty()
        {
            Assert.Throws<BeanIOConfigurationException>(() => NewStreamFactory("FubarDev.BeanIO.Test.Parser.Target.targetIsNotProperty.xml"));
        }

        [Fact]
        public void TestTargetTargetAndClassSet()
        {
            Assert.Throws<BeanIOConfigurationException>(() => NewStreamFactory("FubarDev.BeanIO.Test.Parser.Target.targetAndClass.xml"));
        }
    }
}
