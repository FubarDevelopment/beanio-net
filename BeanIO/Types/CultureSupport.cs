using System;
using System.Globalization;

namespace BeanIO.Types
{
    public abstract class CultureSupport
    {
        private static readonly CultureInfo _cultureEnUs = new CultureInfo("en-US");

        private CultureInfo _culture = _cultureEnUs;

        public CultureInfo Culture
        {
            get { return _culture; }
            set { _culture = value; }
        }

        public string Locale
        {
            get { return _culture.Name; }
            set { _culture = new CultureInfo(value); }
        }
    }
}
