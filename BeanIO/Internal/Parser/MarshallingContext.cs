using System.Collections.Generic;
using System.Xml.Linq;

using BeanIO.Stream;

namespace BeanIO.Internal.Parser
{
    public abstract class MarshallingContext : ParsingContext
    {
        /// <summary>
        /// Gets or sets the bean object to marshal.
        /// </summary>
        public object Bean { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IRecordWriter"/> to write to.
        /// </summary>
        public IRecordWriter RecordWriter { get; set; }

        /// <summary>
        /// Gets or sets the component name of the record or group to marshal.
        /// </summary>
        /// <remarks>
        /// May be null if not specified.
        /// </remarks>
        public string ComponentName { get; set; }

        /// <summary>
        /// Gets the parsing mode.
        /// </summary>
        public override ParsingMode Mode
        {
            get { return ParsingMode.Marshalling; }
        }

        /// <summary>
        /// Gets the record object to pass to the <see cref="IRecordWriter"/>
        /// when <see cref="MarshallingContext.WriteRecord"/> is called.
        /// </summary>
        protected abstract object RecordObject { get; }

        /// <summary>
        /// Clear is invoked after each bean object (record or group) is marshalled
        /// </summary>
        public void Clear()
        {
            ClearOffset();
            Bean = null;
            ComponentName = null;
        }

        /// <summary>
        /// Writes the current record object to the record writer.
        /// </summary>
        /// <remarks>
        /// This method uses the <see cref="RecordObject"/>.
        /// </remarks>
        public void WriteRecord()
        {
            RecordWriter.Write(RecordObject);
            Clear();
        }

        /// <summary>
        /// Converts a record object to a <see cref="string"/>[].
        /// </summary>
        /// <param name="record">the record object to convert</param>
        /// <returns>the <see cref="string"/> array result, or null if not supported</returns>
        public virtual string[] ToArray(object record)
        {
            return null;
        }

        /// <summary>
        /// Converts a record object to a <see cref="IList{T}"/> with T=<see cref="string"/>.
        /// </summary>
        /// <param name="record">the record object to convert</param>
        /// <returns>the <see cref="IList{T}"/> result, or null if not supported</returns>
        public virtual IList<string> ToList(object record)
        {
            return null;
        }

        /// <summary>
        /// Converts a record object to a <see cref="XDocument"/>.
        /// </summary>
        /// <param name="record">the record object to convert</param>
        /// <returns>the <see cref="XDocument"/> result, or null if not supported</returns>
        public virtual XDocument ToXDocument(object record)
        {
            return null;
        }
    }
}
