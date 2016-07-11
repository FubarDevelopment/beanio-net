// <copyright file="ResourceBundleMessageFactory.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Resources;

namespace BeanIO.Internal.Parser.Message
{
    internal class ResourceBundleMessageFactory : IMessageFactory
    {
        /// <summary>
        /// Key prefix for labels
        /// </summary>
        private const string LABEL_MESSAGE_PREFIX = "label";

        /// <summary>
        /// Key prefix for fields
        /// </summary>
        private const string FIELD_ERROR_MESSAGE_PREFIX = "fielderror";

        /// <summary>
        /// Key prefix for records
        /// </summary>
        private const string RECORD_ERROR_MESSAGE_PREFIX = "recorderror";

        /// <summary>
        /// used to flag cache misses
        /// </summary>
        private static readonly string NOT_FOUND = new string(' ', 1);

        /// <summary>
        /// cache messages from resource bundles
        /// </summary>
        private readonly ConcurrentDictionary<string, string> _messageCache = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Gets or sets the configured resource bundle for messages
        /// </summary>
        public ResourceManager ResourceBundle { get; set; }

        /// <summary>
        /// Gets or sets the default resource bundle for messages based on the stream format
        /// </summary>
        public ResourceManager DefaultResourceBundle { get; set; }

        /// <summary>
        /// Returns the localized label for a record.
        /// </summary>
        /// <param name="recordName">the name of the record</param>
        /// <returns>the record label, or null if no label was found</returns>
        public virtual string GetRecordLabel(string recordName)
        {
            return GetMessage(
                new[]
                    {
                        string.Format("{0}.{1}", LABEL_MESSAGE_PREFIX, recordName)
                    },
                false);
        }

        /// <summary>
        /// Returns a record level error message.
        /// </summary>
        /// <param name="recordName">the name of the record</param>
        /// <param name="rule">the name of the validation rule</param>
        /// <returns>the error message, or null if no message was configured</returns>
        public virtual string GetRecordErrorMessage(string recordName, string rule)
        {
            return GetMessage(
                new[]
                    {
                        string.Format("{0}.{1}.{2}", RECORD_ERROR_MESSAGE_PREFIX, recordName, rule),
                        string.Format("{0}.{1}", RECORD_ERROR_MESSAGE_PREFIX, rule)
                    },
                true);
        }

        /// <summary>
        /// Returns the localized label for a field.
        /// </summary>
        /// <param name="recordName">the name of the record the field belongs to</param>
        /// <param name="fieldName">the name of the field</param>
        /// <returns>the field label, or null if no label was found</returns>
        public virtual string GetFieldLabel(string recordName, string fieldName)
        {
            return GetMessage(
                new[]
                    {
                        string.Format("{0}.{1}.{2}", LABEL_MESSAGE_PREFIX, recordName, fieldName)
                    },
                false);
        }

        /// <summary>
        /// Returns a field level error message.
        /// </summary>
        /// <param name="recordName">the name of the record</param>
        /// <param name="fieldName">the name of the field</param>
        /// <param name="rule">the name of the validation rule</param>
        /// <returns>the error message, or null if no message was configured</returns>
        public virtual string GetFieldErrorMessage(string recordName, string fieldName, string rule)
        {
            return GetMessage(
                new[]
                    {
                        $"{FIELD_ERROR_MESSAGE_PREFIX}.{recordName}.{fieldName}.{rule}",
                        $"{FIELD_ERROR_MESSAGE_PREFIX}.{recordName}.{rule}",
                        $"{FIELD_ERROR_MESSAGE_PREFIX}.{rule}"
                    },
                true);
        }

        protected virtual string GetMessage(IReadOnlyList<string> keys, bool returnKeyWhenNotFound)
        {
            var key = keys[0];
            var message = _messageCache.GetOrAdd(
                key,
                k =>
                {
                    string msg = null;
                    if (ResourceBundle != null)
                        msg = keys.Select(x => GetMessage(ResourceBundle, x)).FirstOrDefault(x => x != null);
                    if (msg == null && DefaultResourceBundle != null)
                        msg = keys.Select(x => GetMessage(DefaultResourceBundle, x)).FirstOrDefault(x => x != null);
                    return msg ?? NOT_FOUND;
                });
            if (ReferenceEquals(message, NOT_FOUND))
                return returnKeyWhenNotFound ? key : null;
            return message;
        }

        /// <summary>
        /// Returns a message from a resource bundle
        /// </summary>
        /// <param name="bundle">the resource bundle to check</param>
        /// <param name="key">the resource bundle key for the message</param>
        /// <returns>the message or <code>null</code> if not found</returns>
        private string GetMessage(ResourceManager bundle, string key)
        {
            // returns null if the key couldn't be found
            return bundle.GetString(key);
        }
    }
}
