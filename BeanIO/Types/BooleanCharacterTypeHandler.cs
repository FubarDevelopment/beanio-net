using System;

namespace BeanIO.Types
{
    public class BooleanCharacterTypeHandler : ITypeHandler
    {
        public BooleanCharacterTypeHandler()
        {
            TrueValue = 'Y';
            FalseValue = 'N';
        }

        public char TrueValue { get; set; }

        public char? FalseValue { get; set; }

        public char? NullValue { get; set; }

        public Type TargetType
        {
            get { return typeof(bool); }
        }

        public virtual object Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            if (text.Length != 1)
                throw new FormatException(string.Format("Invalid value '{0}' (too long)", text));

            var ch = text[0];
            if (ch == TrueValue)
                return true;
            if (FalseValue.HasValue && FalseValue == ch)
                return false;

            throw new FormatException(string.Format("Invalid value '{0}' for type '{1}'", text, TargetType.Name));
        }

        public virtual string Format(object value)
        {
            if (value == null)
                return string.Format("{0}", NullValue);
            var boolValue = (bool)value;
            return string.Format("{0}", boolValue ? TrueValue : FalseValue);
        }
    }
}
