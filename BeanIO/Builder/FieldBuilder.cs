using BeanIO.Internal.Config;
using BeanIO.Internal.Util;
using BeanIO.Types;

namespace BeanIO.Builder
{
    public class FieldBuilder : PropertyBuilderSupport<FieldBuilder, FieldConfig>
    {
        private FieldConfig _config;

        public FieldBuilder(string name)
        {
            _config = new FieldConfig()
                {
                    Name = name,
                    IsBound = true,
                };
        }

        /// <summary>
        /// Gets this.
        /// </summary>
        protected override FieldBuilder Me
        {
            get { return this; }
        }

        /// <summary>
        /// Gets the configuration settings.
        /// </summary>
        protected override FieldConfig Config
        {
            get { return _config; }
        }

        /// <summary>
        /// Indicates this field is used to identify the record.
        /// </summary>
        /// <returns>The value of <see cref="Me"/></returns>
        public FieldBuilder Rid()
        {
            Config.IsIdentifier = true;
            return Me;
        }

        /// <summary>
        /// Sets the position of the field.
        /// </summary>
        /// <param name="position">the position</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public FieldBuilder At(int position)
        {
            Config.Position = position;
            return Me;
        }

        /// <summary>
        /// Sets the maximum position of this field if it repeats an
        /// indeterminate number of times
        /// </summary>
        /// <param name="until">the maximum position</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public FieldBuilder Until(int until)
        {
            Config.Until = until;
            return Me;
        }

        /// <summary>
        /// Indicates the field text should be trimmed before validation and type conversion.
        /// </summary>
        /// <returns>The value of <see cref="Me"/></returns>
        public FieldBuilder Trim()
        {
            Config.IsTrim = true;
            return Me;
        }

        /// <summary>
        /// Indicates this field is required and must contain at least one character.
        /// </summary>
        /// <returns>The value of <see cref="Me"/></returns>
        public FieldBuilder Required()
        {
            Config.IsRequired = true;
            return Me;
        }

        /// <summary>
        /// Indicates the number of occurrences of this field is governed by another field.
        /// </summary>
        /// <param name="reference">the name of the field that governs the occurrences of this field</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public FieldBuilder OccursRef(string reference)
        {
            Config.OccursRef = reference;
            return Me;
        }

        /// <summary>
        /// Sets the minimum expected length of the field text.
        /// </summary>
        /// <param name="n">The minimum expected length</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public FieldBuilder MinLength(int n)
        {
            Config.MinLength = n;
            return Me;
        }

        /// <summary>
        /// Sets the maximum expected length of the field text.
        /// </summary>
        /// <param name="n">The maximum expected length</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public FieldBuilder MaxLength(int n)
        {
            Config.MaxLength = n;
            return Me;
        }

        /// <summary>
        /// Sets the regular expression the field text must match.
        /// </summary>
        /// <param name="pattern">The regular expression pattern</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public FieldBuilder RegEx(string pattern)
        {
            Config.RegEx = pattern;
            return Me;
        }

        /// <summary>
        /// Sets the literal text the field text must match.
        /// </summary>
        /// <param name="literal">The literal text</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public FieldBuilder Literal(string literal)
        {
            Config.Literal = literal;
            return Me;
        }

        /// <summary>
        /// Sets the default value of this field.
        /// </summary>
        /// <param name="defaultValue">The default value</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public FieldBuilder DefaultValue(string defaultValue)
        {
            Config.Default = defaultValue;
            return Me;
        }

        /// <summary>
        /// Sets the pattern used to format this field by the type handler.
        /// </summary>
        /// <param name="format">the pattern</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public FieldBuilder Format(string format)
        {
            Config.Format = format;
            return Me;
        }

        /// <summary>
        /// Indicates this field is not bound to a property of the class assigned
        /// to its parent record or segment.
        /// </summary>
        /// <returns>The value of <see cref="Me"/></returns>
        public FieldBuilder Ignore()
        {
            Config.IsBound = false;
            return Me;
        }

        /// <summary>
        /// Sets the padded length of this field.
        /// </summary>
        /// <param name="length">The length</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public FieldBuilder Length(int length)
        {
            Config.Length = length;
            return Me;
        }

        /// <summary>
        /// Sets the character used to pad this field.
        /// </summary>
        /// <remarks>
        /// Defaults to a space.
        /// </remarks>
        /// <param name="ch">the padding character</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public FieldBuilder Padding(char ch)
        {
            Config.Padding = ch;
            return Me;
        }

        /// <summary>
        /// Indicates this field should not be unpadded during unmarshalling.
        /// </summary>
        /// <returns>The value of <see cref="Me"/></returns>
        public FieldBuilder KeepPadding()
        {
            Config.KeepPadding = true;
            return Me;
        }

        /// <summary>
        /// Indicates the padding length should not be enforced for this field.
        /// </summary>
        /// <remarks>
        /// Only applies to fixed length formatted streams.
        /// </remarks>
        /// <returns>The value of <see cref="Me"/></returns>
        public FieldBuilder LenientPadding()
        {
            Config.IsLenientPadding = true;
            return Me;
        }

        /// <summary>
        /// Sets the alignment or justification of this field if padded.
        /// </summary>
        /// <param name="align">the alignment</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public FieldBuilder Align(Align align)
        {
            Config.Justify = align;
            return Me;
        }

        /// <summary>
        /// Indicates this field is nillable for XML formatted streams.
        /// </summary>
        /// <returns>The value of <see cref="Me"/></returns>
        public FieldBuilder Nillable()
        {
            Config.IsNillable = true;
            return Me;
        }

        /// <summary>
        /// Sets the type handler used for parsing and formatting field text.
        /// </summary>
        /// <param name="name">The type handler type name</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public FieldBuilder TypeHandler(string name)
        {
            Config.TypeHandler = name;
            return Me;
        }

        /// <summary>
        /// Sets the type handler used for parsing and formatting field text.
        /// </summary>
        /// <typeparam name="THandler">the type handler class</typeparam>
        /// <returns>The value of <see cref="Me"/></returns>
        public FieldBuilder TypeHandler<THandler>() where THandler : ITypeHandler, new()
        {
            Config.TypeHandler = typeof(THandler).GetFullName();
            return Me;
        }

        /// <summary>
        /// Sets the type handler used for parsing and formatting field text.
        /// </summary>
        /// <param name="handler">The <see cref="ITypeHandler"/>.</param>
        /// <returns>The value of <see cref="Me"/></returns>
        public FieldBuilder TypeHandler(ITypeHandler handler)
        {
            Config.TypeHandlerInstance = handler;
            return Me;
        }

        /// <summary>
        /// Builds this field.
        /// </summary>
        /// <returns>The field configuration</returns>
        public FieldConfig Build()
        {
            return Config;
        }

        /// <summary>
        /// Sets the configuration settings
        /// </summary>
        /// <param name="config">The configuration settings</param>
        protected void SetConfig(FieldConfig config)
        {
            _config = config;
        }
    }
}
