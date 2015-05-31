﻿namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// A <see cref="IMessageFactory"/> implementation is used to generate localized error
    /// messages for record and field level errors.
    /// </summary>
    public interface IMessageFactory
    {
        /// <summary>
        /// Returns the localized label for a record.
        /// </summary>
        /// <param name="recordName">the name of the record</param>
        /// <returns>the record label, or null if no label was found</returns>
        string GetRecordLabel(string recordName);

        /// <summary>
        /// Returns a record level error message.
        /// </summary>
        /// <param name="recordName">the name of the record</param>
        /// <param name="rule">the name of the validation rule</param>
        /// <returns>the error message, or null if no message was configured</returns>
        string GetRecordErrorMessage(string recordName, string rule);

        /// <summary>
        /// Returns the localized label for a field.
        /// </summary>
        /// <param name="recordName">the name of the record the field belongs to</param>
        /// <param name="fieldName">the name of the field</param>
        /// <returns>the field label, or null if no label was found</returns>
        string GetFieldLabel(string recordName, string fieldName);

        /// <summary>
        /// Returns a field level error message.
        /// </summary>
        /// <param name="recordName">the name of the record</param>
        /// <param name="fieldName">the name of the field</param>
        /// <param name="rule">the name of the validation rule</param>
        /// <returns>the error message, or null if no message was configured</returns>
        string GetFieldErrorMessage(string recordName, string fieldName, string rule);
    }
}