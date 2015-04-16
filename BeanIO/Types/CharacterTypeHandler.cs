using System;

namespace BeanIO.Types
{
    public class CharacterTypeHandler : ITypeHandler
    {
        public Type TargetType
        {
            get { return typeof(char); }
        }

        public object Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            if (text.Length != 1)
                throw new FormatException(string.Format("Invalid value '{0}' (too long)", text));

            return text[0];
        }

        public string Format(object value)
        {
            if (value == null)
                return null;
            return value.ToString();
        }
    }
}
