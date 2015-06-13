using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using BeanIO.Stream;

namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// Stores context information needed to unmarshal a bean object.
    /// </summary>
    /// <remarks>
    /// Subclasses must implement <see cref="SetRecordValue"/> which is called
    /// by <see cref="NextRecord"/> each time a new record is read from the <see cref="IRecordReader"/>.
    /// The object used to represent a record is dependent on the <see cref="IRecordReader"/>
    /// implementation for the stream format.</remarks>
    internal abstract class UnmarshallingContext : ParsingContext
    {
        /// <summary>
        /// a list of record contexts (for parsing record groups)
        /// </summary>
        private readonly List<ErrorContext> _recordList = new List<ErrorContext>();

        /// <summary>
        /// indicates the last record read from the reader has been processed
        /// </summary>
        private bool _isProcessed = true;

        /// <summary>
        /// the top level component name being unmarshalled
        /// </summary>
        private string _componentName;

        /// <summary>
        /// indicates the component being unmarshalled is a record group (otherwise just a record)
        /// </summary>
        private bool _isRecordGroup;

        /// <summary>
        /// the current record context
        /// </summary>
        private ErrorContext _recordContext = new ErrorContext();

        /// <summary>
        /// indicates the record context was queried and must therefore be recreated when the next record is read
        /// </summary>
        private bool _isDirty;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnmarshallingContext"/> class.
        /// </summary>
        protected UnmarshallingContext()
        {
            Culture = CultureInfo.CurrentCulture;
        }

        /// <summary>
        /// Gets the parsing mode.
        /// </summary>
        public override ParsingMode Mode
        {
            get { return ParsingMode.Unmarshalling; }
        }

        /// <summary>
        /// Gets or sets the <see cref="IRecordReader"/> to read from.
        /// </summary>
        public IRecordReader RecordReader { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IMessageFactory"/> for formatting error messages.
        /// </summary>
        public IMessageFactory MessageFactory { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CultureInfo"/> to format error messages in.
        /// </summary>
        public CultureInfo Culture { get; set; }

        /// <summary>
        /// Gets the number of record read for the last unmarshalled bean object.
        /// </summary>
        public int RecordCount { get; private set; }

        /// <summary>
        /// Gets the last line number read from the input stream.
        /// </summary>
        public int LineNumber { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the end of the stream was reached after
        /// <see cref="NextRecord"/> was called.
        /// </summary>
        public bool IsEof { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a field error was reported while parsing
        /// </summary>
        public bool HasFieldErrors
        {
            get { return _recordContext.HasFieldErrors; }
        }

        /// <summary>
        /// Gets a value indicating whether a record level error was reported while parsing
        /// </summary>
        public bool HasRecordErrors
        {
            get { return _recordContext.HasRecordErrors; }
        }

        public IEnumerable<IRecordContext> GetRecordContexts()
        {
            var contexts = new List<IRecordContext> { _recordContext };
            contexts.AddRange(_recordList);
            return contexts;
        }

        /// <summary>
        /// Sets the value of the record returned from the <see cref="IRecordReader"/>
        /// </summary>
        /// <param name="value">the record value read by a <see cref="IRecordReader"/></param>
        public abstract void SetRecordValue(object value);

        /// <summary>
        /// Converts a <see cref="string"/>[] to a record value.
        /// </summary>
        /// <param name="array">the <see cref="string"/>[] to convert</param>
        /// <returns>the record value, or null if not supported</returns>
        public virtual object ToRecordValue(string[] array)
        {
            return null;
        }

        /// <summary>
        /// Converts a <see cref="List{T}"/> to a record value.
        /// </summary>
        /// <param name="list">the <see cref="List{T}"/> to convert</param>
        /// <returns>the record value, or null if not supported</returns>
        public virtual object ToRecordValue(IList<string> list)
        {
            return null;
        }

        /// <summary>
        /// Converts a <see cref="XElement"/> to a record value.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> to convert</param>
        /// <returns>the record value, or null if not supported</returns>
        public virtual object ToRecordValue(XElement element)
        {
            return null;
        }

        /// <summary>
        /// Prepares this context for unmarshalling a record (or group of records that
        /// are combined to form a single bean object).
        /// </summary>
        /// <param name="componentName">the record or group name to be unmarshalled</param>
        /// <param name="isRecordGroup">true if the component is a group, false if it is a record</param>
        public virtual void Prepare(string componentName, bool isRecordGroup)
        {
            // clear any context state information before parsing the next record
            if (_isDirty)
            {
                // create a new context
                _recordContext = new ErrorContext();
            }
            else
            {
                _recordContext.Clear();
            }

            _recordList.Clear();

            RecordCount = 0;
            _isDirty = false;
            _componentName = componentName;
            _isRecordGroup = isRecordGroup;
        }

        /// <summary>
        /// This method must be invoked before a record is unmarshalled.
        /// </summary>
        /// <param name="recordName">the name of the record</param>
        public void RecordStarted(string recordName)
        {
            ++RecordCount;

            _recordContext.RecordName = recordName;
            _recordContext.LineNumber = LineNumber;
            _recordContext.RecordText = RecordReader.RecordText;
        }

        /// <summary>
        /// Either this method (or <see cref="RecordSkipped"/>) must be invoked after a
        /// record is unmarshalled, even if an error has occurred.
        /// </summary>
        public void RecordCompleted()
        {
            _isProcessed = true;

            // if unmarshalling a record group, add the last record context to the
            // record list and create a new one
            if (_isRecordGroup)
            {
                _recordList.Add(_recordContext);
                _recordContext = new ErrorContext();
            }
        }

        /// <summary>
        /// This method should be invoked when a record is skipped.
        /// </summary>
        public void RecordSkipped()
        {
            _isProcessed = true;
        }

        /// <summary>
        /// Reads the next record from the input stream and calls <see cref="SetRecordValue"/>.
        /// </summary>
        public void NextRecord()
        {
            if (!_isProcessed || IsEof)
                return;

            // clear the field offset for the next record
            ClearOffset();

            // reset the processed flag
            _isProcessed = false;

            // read the next record
            try
            {
                var recordValue = RecordReader.Read();
                if (recordValue == null)
                {
                    IsEof = true;
                    LineNumber++;
                }
                else
                {
                    // set the value of the record (which is implementation specific) on the record
                    SetRecordValue(recordValue);
                    LineNumber = RecordReader.RecordLineNumber;
                }
            }
            catch (RecordIOException e)
            {
                LineNumber = RecordReader.RecordLineNumber;
                throw NewMalformedRecordException(e);
            }
            catch (IOException e)
            {
                throw new BeanReaderIOException(string.Format("IOException caught reading from input stream: {0}", e.Message), e);
            }
        }

        /// <summary>
        /// Validates all unmarshalled records and throws an exception if any record is invalid.
        /// </summary>
        /// <remarks>
        /// This method must be invoked after unmarshalling is completed.
        /// If unmarshalling fails due to some other fatal exception, there is no need
        /// to call this method.
        /// </remarks>
        public void Validate()
        {
            // check for errors
            if (_isRecordGroup)
            {
                var hasErrors = _recordList.Any(x => x.HasErrors);
                if (hasErrors)
                {
                    var rca = _recordList.Cast<IRecordContext>().ToArray();
                    throw new InvalidRecordGroupException(
                        string.Format(
                            "Invalid '{0}' record group at line {1}",
                            _componentName,
                            rca[0].LineNumber),
                        _componentName,
                        rca);
                }
            }
            else if (_recordContext.HasErrors)
            {
                _isDirty = true;
                if (LineNumber > 0)
                    throw new InvalidRecordException(string.Format("Invalid '{0}' record at line {1}", _componentName, LineNumber), _recordContext);
                throw new InvalidRecordException(string.Format("Invalid '{0}' record", _componentName), _recordContext);
            }
        }

        /// <summary>
        /// Returns the record context for a record read for the last unmarshalled bean object.
        /// </summary>
        /// <param name="index">the index of the record</param>
        /// <returns>the <see cref="IRecordContext"/></returns>
        public IRecordContext GetRecordContext(int index)
        {
            if (_isRecordGroup)
                return _recordList[index];

            if (RecordCount > 0 && index == 0)
            {
                _isDirty = true;
                return _recordContext;
            }

            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// Sets the raw field text for a named field.
        /// </summary>
        /// <param name="fieldName">the name of the field</param>
        /// <param name="text">the raw field text</param>
        public void SetFieldText(string fieldName, string text)
        {
            _recordContext.SetFieldText(fieldName, text, IsRepeating);
        }

        /// <summary>
        /// Adds a field error to this record.
        /// </summary>
        /// <param name="fieldName">the name of the field in error</param>
        /// <param name="fieldText">the invalid field text</param>
        /// <param name="rule">the name of the failed validation rule</param>
        /// <param name="args">an optional list of parameters for formatting the error message</param>
        /// <returns>the formatted field error message</returns>
        public string AddFieldError(string fieldName, string fieldText, string rule, params object[] args)
        {
            var lineNumber = _recordContext.LineNumber;
            var recordName = _recordContext.RecordName;
            var recordLabel = MessageFactory.GetRecordLabel(recordName) ?? string.Format("'{0}'", recordName);
            var fieldLabel = MessageFactory.GetFieldLabel(recordName, fieldName) ?? string.Format("'{0}'", fieldName);

            var messageParams = new object[4 + args.Length];
            messageParams[0] = lineNumber;
            messageParams[1] = recordLabel;
            messageParams[2] = fieldLabel;
            messageParams[3] = fieldText;
            Array.Copy(args, 0, messageParams, 4, args.Length);

            var pattern = MessageFactory.GetFieldErrorMessage(recordName, fieldName, rule);
            var message = string.Format(Culture, pattern, messageParams);
            _recordContext.AddFieldError(fieldName, message);
            return message;
        }

        /// <summary>
        /// Adds a record level error to this record.
        /// </summary>
        /// <param name="rule">the name of the failed validation rule</param>
        /// <param name="args">an optional list of parameters for formatting the error message</param>
        /// <returns>the formatted record error message</returns>
        public string AddRecordError(string rule, params object[] args)
        {
            return AddRecordError(_recordContext, rule, args);
        }

        public BeanReaderException NewMalformedRecordException(RecordIOException cause)
        {
            return new MalformedRecordException(
                string.Format("Malformed record at line {0}: {1}", RecordReader.RecordLineNumber, cause.Message),
                RecordException(null, "malformed", cause.Message));
        }

        public BeanReaderException NewUnsatisfiedGroupException(string groupName)
        {
            if (IsEof)
                return new UnexpectedRecordException(
                    string.Format("End of stream reached, expected record from group '{0}'", groupName),
                    RecordException(groupName, "unsatisfied"));
            return new UnexpectedRecordException(
                string.Format("Expected record from group '{0}' at line {1}", groupName, RecordReader.RecordLineNumber),
                RecordException(groupName, "unsatisfied"));
        }

        public BeanReaderException NewUnsatisfiedRecordException(string recordName)
        {
            if (IsEof)
                return new UnexpectedRecordException(
                    string.Format("End of stream reached, expected record '{0}'", recordName),
                    RecordException(recordName, "unsatisfied"));
            return new UnexpectedRecordException(
                string.Format("Expected record '{0}' at line {1}", recordName, RecordReader.RecordLineNumber),
                RecordException(recordName, "unsatisfied"));
        }

        public BeanReaderException RecordUnexpectedException(string recordName)
        {
            return new UnexpectedRecordException(
                string.Format("Unexpected record '{0}' at line {1}", recordName, RecordReader.RecordLineNumber),
                RecordException(recordName, "unexpected"));
        }

        public BeanReaderException RecordUnidentifiedException()
        {
            return new UnidentifiedRecordException(
                string.Format("Unidentified record at line {0}", RecordReader.RecordLineNumber),
                RecordException(null, "unidentified"));
        }

        /// <summary>
        /// Adds a record level error to this record.
        /// </summary>
        /// <param name="errorContext">the error context to update</param>
        /// <param name="rule">the name of the failed validation rule</param>
        /// <param name="args">an optional list of parameters for formatting the error message</param>
        /// <returns>the formatted record error message</returns>
        protected string AddRecordError(ErrorContext errorContext, string rule, params object[] args)
        {
            var lineNumber = errorContext.LineNumber;
            var recordName = errorContext.RecordName;

            var recordLabel = (recordName != null ? MessageFactory.GetRecordLabel(recordName) : null)
                              ?? string.Format("'{0}'", recordName);

            var messageParams = new object[3 + args.Length];
            messageParams[0] = lineNumber;
            messageParams[1] = recordLabel;
            messageParams[2] = errorContext.RecordText;
            Array.Copy(args, 0, messageParams, 3, args.Length);

            var pattern = MessageFactory.GetRecordErrorMessage(recordName, rule);
            var message = string.Format(Culture, pattern, messageParams);
            _recordContext.AddRecordError(message);
            return message;
        }

        /// <summary>
        /// Handles a record level exception and returns a new <see cref="ErrorContext"/> for the exception.
        /// </summary>
        /// <param name="recordName">the name of the record that failed</param>
        /// <param name="rule">the record level rule that failed validation</param>
        /// <param name="args">message parameters for formatting the error message</param>
        /// <returns>the created <see cref="ErrorContext"/></returns>
        protected ErrorContext RecordException(string recordName, string rule, params object[] args)
        {
            _isProcessed = true;

            var ec = new ErrorContext
                {
                    RecordName = recordName,
                    LineNumber = LineNumber,
                    RecordText = RecordReader.RecordText
                };

            AddRecordError(ec, rule, args);

            return ec;
        }
    }
}
