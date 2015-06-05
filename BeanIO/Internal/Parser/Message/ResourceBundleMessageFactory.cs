using System;
using System.Collections.Concurrent;
using System.Resources;

namespace BeanIO.Internal.Parser.Message
{
    public class ResourceBundleMessageFactory : IMessageFactory
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
        /// configured resource bundle for messages
        /// </summary>
        private ResourceManager _resourceBundle;

        /// <summary>
        /// default resource bundle for messages based on the stream format
        /// </summary>
        private ResourceManager _defaultResourceBundle;

        /// <summary>
        /// Returns the localized label for a record.
        /// </summary>
        /// <param name="recordName">the name of the record</param>
        /// <returns>the record label, or null if no label was found</returns>
        public string GetRecordLabel(string recordName)
        {
            return GetLabel(string.Format("{0}.{1}", LABEL_MESSAGE_PREFIX, recordName));
        }

        /// <summary>
        /// Returns a record level error message.
        /// </summary>
        /// <param name="recordName">the name of the record</param>
        /// <param name="rule">the name of the validation rule</param>
        /// <returns>the error message, or null if no message was configured</returns>
        public string GetRecordErrorMessage(string recordName, string rule)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Returns the localized label for a field.
        /// </summary>
        /// <param name="recordName">the name of the record the field belongs to</param>
        /// <param name="fieldName">the name of the field</param>
        /// <returns>the field label, or null if no label was found</returns>
        public string GetFieldLabel(string recordName, string fieldName)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Returns a field level error message.
        /// </summary>
        /// <param name="recordName">the name of the record</param>
        /// <param name="fieldName">the name of the field</param>
        /// <param name="rule">the name of the validation rule</param>
        /// <returns>the error message, or null if no message was configured</returns>
        public string GetFieldErrorMessage(string recordName, string fieldName, string rule)
        {
            throw new System.NotImplementedException();
        }
    }
}
