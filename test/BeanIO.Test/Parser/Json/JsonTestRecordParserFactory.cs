// <copyright file="JsonTestRecordParserFactory.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BeanIO.Stream.Json;

namespace BeanIO.Parser.Json
{
    public class JsonTestRecordParserFactory : JsonRecordParserFactory
    {
        public JsonTestRecordParserFactory()
        {
            Pretty = true;
            Indentation = 2;
            LineSeparator = "\r\n";
        }
    }
}
