using System;

using NodaTime;

namespace BeanIO.Types
{
    public class TimeTypeHandler : DateTimeTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeTypeHandler"/> class.
        /// </summary>
        public TimeTypeHandler()
            : base(culture => culture.DateTimeFormat.LongTimePattern)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeTypeHandler"/> class.
        /// </summary>
        /// <param name="pattern">The pattern to use</param>
        public TimeTypeHandler(string pattern)
            : base(culture => culture.DateTimeFormat.LongTimePattern, pattern)
        {
        }

        /// <summary>
        /// Gets the class type supported by this handler.
        /// </summary>
        public override Type TargetType
        {
            get { return typeof(LocalTime); }
        }

        /// <summary>
        /// Parses field text into an object.
        /// </summary>
        /// <param name="text">The field text to parse, which may be null if the field was not passed in the record</param>
        /// <returns>The parsed object</returns>
        public override object Parse(string text)
        {
            var dt = ParseDate(text);
            if (dt == null)
                return null;
            return dt.Value.TimeOfDay;
        }

        /// <summary>
        /// Formats an object into field text.
        /// </summary>
        /// <param name="value">The value to format, which may be null</param>
        /// <returns>The formatted field text, or <code>null</code> to indicate the value is not present</returns>
        public override string Format(object value)
        {
            if (value == null)
                return null;
            var t = (LocalTime)value;
            return FormatDate(new LocalDate().At(t));
        }
    }
}
