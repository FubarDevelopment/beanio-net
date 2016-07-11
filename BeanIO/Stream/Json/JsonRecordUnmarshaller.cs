// <copyright file="JsonRecordUnmarshaller.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.IO;

using Newtonsoft.Json.Linq;

using NJ = Newtonsoft.Json;

namespace BeanIO.Stream.Json
{
    /// <summary>
    /// A <see cref="IRecordUnmarshaller"/> implementation for JSON formatted records.
    /// </summary>
    public class JsonRecordUnmarshaller : IRecordUnmarshaller
    {
        /// <summary>
        /// Unmarshals a single record.
        /// </summary>
        /// <param name="text">The record text to unmarshal</param>
        /// <returns>The unmarshalled record object</returns>
        public object Unmarshal(string text)
        {
            if (text == null)
                return null;
            var reader = new NJ.JsonTextReader(new StringReader(text))
            {
                CloseInput = true,
            };

            try
            {
                while (reader.Read())
                {
                    if (reader.TokenType == NJ.JsonToken.StartObject)
                    {
                        var value = JToken.Load(reader);
                        return value;
                    }

                    throw new RecordIOException(string.Format("Unexpected token {0}", reader.TokenType));
                }
            }
            catch (NJ.JsonReaderException ex)
            {
                throw new RecordIOException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new RecordIOException(string.Format("{0} at line {1}, near position {2}", ex.Message, reader.LineNumber, reader.LinePosition), ex);
            }

            return null;
        }
    }
}
