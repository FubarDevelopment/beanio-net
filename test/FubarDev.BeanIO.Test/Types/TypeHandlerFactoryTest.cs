// <copyright file="TypeHandlerFactoryTest.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;

using BeanIO.Config;
using BeanIO.Internal.Util;

using NodaTime;

using Xunit;

namespace BeanIO.Types
{
    public class TypeHandlerFactoryTest
    {
        [Fact]
        public void TestGetHandler()
        {
            var factory = new TypeHandlerFactory();
            Assert.Null(factory.GetTypeHandlerFor(GetType()));
            Assert.Null(factory.GetTypeHandlerFor("invalid_alias"));
            Assert.Null(factory.GetTypeHandler("invalid_name"));
        }

        [Fact]
        public void TestFormatNotSupported()
        {
            var props = new Properties(
                new Dictionary<string, string?>()
                {
                    { "format", "yyyy-MM-dd" }
                });
            var factory = new TypeHandlerFactory();
            Assert.Throws<BeanIOConfigurationException>(() => factory.GetTypeHandlerFor(typeof(char), null, props));
        }

        [Fact]
        public void TestRegisterWithNullName()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new TypeHandlerFactory().RegisterHandler(null!, () => new IntegerTypeHandler()));
        }

        [Fact]
        public void TestRegisterWithNullHandler()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new TypeHandlerFactory().RegisterHandler("name", null!));
        }

        [Fact]
        public void TestGetHandlerWithNullName()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new TypeHandlerFactory().GetTypeHandler(null!));
        }

        [Fact]
        public void TestRegisterWithNullType()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new TypeHandlerFactory().RegisterHandlerFor((string)null!, () => new IntegerTypeHandler()));
        }

        [Fact]
        public void TestRegisterWithInvalidType()
        {
            Assert.Throws<ArgumentException>(() => new TypeHandlerFactory().RegisterHandlerFor("invalid_type_alias", () => new IntegerTypeHandler()));
        }

        [Fact]
        public void TestGetHandlerWithNullType()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new TypeHandlerFactory().GetTypeHandlerFor((string)null!));
        }

        [Fact]
        public void TestRegisterWithNullClass()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new TypeHandlerFactory().RegisterHandlerFor((Type)null!, () => new IntegerTypeHandler()));
        }

        [Fact]
        public void TestRegisterHandlerForWrongClass()
        {
            Assert.Throws<ArgumentException>(() => new TypeHandlerFactory().RegisterHandlerFor(typeof(int), () => new NumberTypeHandler(typeof(byte))));
        }

        [Fact]
        public void TestGetHandlerWithNullClass()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new TypeHandlerFactory().GetTypeHandlerFor((Type)null!));
        }

        [Fact]
        public void TestDateTypeHandlers()
        {
            var factory = new TypeHandlerFactory();

            var dtoNow = DateTimeOffset.Now;
            var zdtNow = ZonedDateTime.FromDateTimeOffset(dtoNow);
            var dtNow = dtoNow.DateTime;
            var date = zdtNow.Date;
            var time = zdtNow.TimeOfDay;

            var dateHandler = Assert.IsType<DateTypeHandler>(factory.GetTypeHandlerFor("date"));
            Assert.Equal(dtoNow.ToString(dateHandler.Culture.DateTimeFormat.ShortDatePattern, dateHandler.Culture), dateHandler.Format(date));
            var timeHandler = Assert.IsType<TimeTypeHandler>(factory.GetTypeHandlerFor("time"));
            Assert.Equal(dtoNow.ToString(timeHandler.Culture.DateTimeFormat.LongTimePattern, timeHandler.Culture), timeHandler.Format(time));
            var dateTimeHandler = Assert.IsType<DateTimeTypeHandler>(factory.GetTypeHandlerFor("dt"));
            Assert.Equal(
                dtoNow.ToString(
                    dateTimeHandler.Culture.DateTimeFormat.ShortDatePattern + " " + dateTimeHandler.Culture.DateTimeFormat.LongTimePattern,
                    dateTimeHandler.Culture),
                dateTimeHandler.Format(dtNow));

            dateHandler = new DateTypeHandler("MMddyyyy");
            dateTimeHandler = new DateTimeTypeHandler("MMddyyyy HH:mm");
            timeHandler = new TimeTypeHandler("HH:mm");

            factory.RegisterHandlerFor("datetime", () => dateTimeHandler);
            factory.RegisterHandlerFor("date", () => dateHandler);
            factory.RegisterHandlerFor("time", () => timeHandler);

            Assert.Same(dateTimeHandler, factory.GetTypeHandlerFor(typeof(DateTime).GetAssemblyQualifiedName()));
            Assert.Same(dateTimeHandler, factory.GetTypeHandlerFor(typeof(DateTime)));
            Assert.Same(dateHandler, factory.GetTypeHandlerFor("date"));
            Assert.Same(timeHandler, factory.GetTypeHandlerFor("time"));

            var props = new Properties(
                new Dictionary<string, string?>()
                {
                    { "format", "yyyy-MM-dd" }
                });
            var th = (DateTypeHandler?)factory.GetTypeHandlerFor("date", null, props);
            Assert.NotNull(th);
            Assert.Equal("yyyy-MM-dd", th.Pattern);
        }
    }
}
