using System.Globalization;

namespace BeanIO.Types
{
    public abstract class CultureSupport
    {
        private CultureInfo _culture = CultureInfo.CurrentCulture;

        public CultureInfo Culture
        {
            get { return _culture; }
            set { _culture = value; }
        }
    }
}
