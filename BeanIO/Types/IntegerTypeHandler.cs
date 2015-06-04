using System;

namespace BeanIO.Types
{
    /// <summary>
    /// A type handler implementation for the <see cref="System.Int32"/> class.
    /// </summary>
    public class IntegerTypeHandler : NumberTypeHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntegerTypeHandler"/> class.
        /// </summary>
        public IntegerTypeHandler()
            : base(typeof(int))
        {
        }
    }
}
