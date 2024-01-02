// <copyright file="DelimitedRecordParser.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeanIO.Stream.Delimited
{
    /// <summary>
    /// A combined <see cref="IRecordMarshaller"/> and <see cref="IRecordUnmarshaller"/> implementation
    /// for delimited formatted records.
    /// </summary>
    public class DelimitedRecordParser : IRecordMarshaller, IRecordUnmarshaller
    {
        private readonly char _delim;

        private readonly char? _escape;

        private readonly IList<string> _fieldList = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DelimitedRecordParser"/> class.
        /// </summary>
        public DelimitedRecordParser()
            : this(new DelimitedParserConfiguration())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelimitedRecordParser"/> class.
        /// </summary>
        /// <param name="config">the <see cref="DelimitedParserConfiguration"/>.</param>
        public DelimitedRecordParser(DelimitedParserConfiguration config)
        {
            _delim = config.Delimiter;
            _escape = config.Escape;
            if (_escape != null && _delim == _escape)
                throw new BeanIOConfigurationException("The field delimiter canot match the escape character");
        }

        /// <summary>
        /// Marshals a single record object to a <c>String</c>.
        /// </summary>
        /// <param name="record">Record the record object to marshal.</param>
        /// <returns>The marshalled record text.</returns>
        public string Marshal(object record)
        {
            return Marshal((string[])record);
        }

        /// <summary>
        /// Unmarshals a single record.
        /// </summary>
        /// <param name="text">The record text to unmarshal.</param>
        /// <returns>The unmarshalled record object.</returns>
        public object Unmarshal(string text)
        {
            _fieldList.Clear();

            var escaped = false;
            var field = new StringBuilder();

            foreach (var c in text.ToCharArray())
            {
                if (escaped)
                {
                    escaped = false;

                    if ((_escape != null && c == _escape) || c == _delim)
                    {
                        field.Append(c);
                        continue;
                    }

                    field.Append(_escape);
                }

                if (_escape != null && c == _escape)
                {
                    escaped = true;
                }
                else if (c == _delim)
                {
                    _fieldList.Add(field.ToString());
                    field = new StringBuilder();
                }
                else
                {
                    field.Append(c);
                }
            }

            if (escaped)
                field.Append(_escape);

            _fieldList.Add(field.ToString());

            var record = _fieldList.ToArray();
            return record;
        }

        /// <summary>
        /// Marshals a single record object to a <c>String</c>.
        /// </summary>
        /// <param name="record">Record the record object to marshal.</param>
        /// <returns>The marshalled record text.</returns>
        public string Marshal(string[] record)
        {
            var text = new StringBuilder();

            if (_escape != null)
            {
                var pos = 0;
                foreach (var field in record)
                {
                    if (pos++ > 0)
                        text.Append(_delim);

                    var cs = field.ToCharArray();
                    for (int i = 0, j = cs.Length; i < j; ++i)
                    {
                        var c = cs[i];
                        if (c == _delim || _escape == c)
                            text.Append(_escape);
                        text.Append(c);
                    }
                }
            }
            else
            {
                var pos = 0;
                foreach (var field in record)
                {
                    if (pos++ > 0)
                        text.Append(_delim);
                    text.Append(field);
                }
            }

            return text.ToString();
        }
    }
}
