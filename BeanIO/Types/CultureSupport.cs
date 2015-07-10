using System;
using System.Globalization;

namespace BeanIO.Types
{
    /// <summary>
    /// Support for culture specific types
    /// </summary>
    public abstract class CultureSupport
    {
        private static readonly CultureInfo _cultureEnUs = new CultureInfo("en-US");

        private CultureInfo _culture = _cultureEnUs;

        /// <summary>
        /// Gets or sets the culture
        /// </summary>
        public CultureInfo Culture
        {
            get { return _culture; }
            set { _culture = value; }
        }

        /// <summary>
        /// Gets or sets the culture using a locale name.
        /// </summary>
        public string Locale
        {
            get { return _culture.Name; }
            set { _culture = new CultureInfo(value); }
        }
    }
}
