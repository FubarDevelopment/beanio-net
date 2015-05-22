using System;

namespace BeanIO.Types
{
    public class DecimalTypeHandler : NumberTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecimalTypeHandler"/> class.
        /// </summary>
        public DecimalTypeHandler()
            : base(typeof(decimal))
        {
        }

        /// <summary>
        /// Parses a number from a <see cref="System.Decimal"/>.
        /// </summary>
        /// <param name="value">The <see cref="System.Decimal"/> to convert to a number</param>
        /// <returns>The parsed number</returns>
        protected override object CreateNumber(decimal value)
        {
            return value;
        }
    }
}
