using System;
using System.Globalization;

namespace BeanIO.Types
{
    public class BooleanIntegerTypeHandler : CultureSupport, ITypeHandler
    {
        public BooleanIntegerTypeHandler()
        {
            TrueValue = 1;
            FalseValue = 0;
        }

        public int TrueValue { get; set; }

        public int? FalseValue { get; set; }

        public int? NullValue { get; set; }

        public Type TargetType
        {
            get { return typeof(bool); }
        }

        public virtual object Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            const NumberStyles styles = NumberStyles.Integer;
            int intValue;
            if (!int.TryParse(text, styles, Culture, out intValue))
                throw new FormatException(string.Format("Number value '{0}' doesn't match the number styles {1}", text, styles));

            if (intValue == TrueValue)
                return true;
            if (FalseValue.HasValue && FalseValue == intValue)
                return false;

            throw new FormatException(string.Format("Invalid value '{0}' for type '{1}'", text, TargetType.Name));
        }

        public virtual string Format(object value)
        {
            if (value == null)
                return string.Format(Culture, "{0}", NullValue);
            var boolValue = (bool)value;
            return string.Format(Culture, "{0}", boolValue ? TrueValue : FalseValue);
        }
    }
}
