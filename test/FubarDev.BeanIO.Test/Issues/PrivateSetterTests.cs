// <copyright file="PrivateSetterTests.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BeanIO.Beans;
using BeanIO.Builder;

using Xunit;

namespace BeanIO.Issues;

public class PrivateSetterTests
{
    [Fact]
    public void TestPrivateSetter()
    {
        var factory = StreamFactory.NewInstance();
        factory.Define(
            new StreamBuilder("s")
                .Format("fixedlength")
                .AddRecord(
                    new RecordBuilder("r", typeof(Bean))
                        .AddField(new FieldBuilder(nameof(Bean.NoOpSetter))
                            .Length(1)
                            .RegEx("[0-9]")
                            .Setter("SetNoOpSetter"))));
        var unmarshaller = factory.CreateUnmarshaller("s");
        var bean = Assert.IsType<Bean>(unmarshaller.Unmarshal("1"));
        Assert.Equal(1, bean.NoOpSetter);
    }
}
