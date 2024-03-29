// <copyright file="Field.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using BeanIO.Internal.Util;
using BeanIO.Types;

namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// A parser for marshalling and unmarshalling a single field in a record.
    /// </summary>
    /// <remarks>
    /// A field is usually, but optionally, bound to a simple property value.
    /// </remarks>
    internal class Field : ParserComponent, IProperty
    {
        private readonly ParserLocal<object?> _value = new ParserLocal<object?>(Value.Missing);

        private Regex? _regex;
        private IFieldFormat? _format;

        public Field()
            : base(0)
        {
            ErrorIfNullPrimitive = Settings.Instance.GetBoolean(Settings.ERROR_IF_NULL_PRIMITIVE);
            UseDefaultIfMissing = Settings.Instance.GetBoolean(Settings.USE_DEFAULT_IF_MISSING);
            ParseDefault = Settings.Instance.GetBoolean(Settings.DEFAULT_PARSING_ENABLED);
            MarshalDefault = Settings.Instance.GetBoolean(Settings.DEFAULT_MARSHALLING_ENABLED);
            ValidateOnMarshal = Settings.Instance.GetBoolean(Settings.VALIDATE_ON_MARSHAL);
            MaxLength = int.MaxValue;
        }

        /// <summary>
        /// Gets the size of a single occurrence of this element, which is used to offset
        /// field positions for repeating segments and fields.
        /// </summary>
        /// <remarks>
        /// The concept of size is dependent on the stream format.  The size of an element in a fixed
        /// length stream format is determined by the length of the element in characters, while other
        /// stream formats calculate size based on the number of fields.  Some stream formats,
        /// such as XML, may ignore size settings.
        /// </remarks>
        public override int? Size => Format.Size;

        /// <summary>
        /// Gets the <see cref="IProperty"/> implementation type.
        /// </summary>
        public PropertyType Type => Parser.PropertyType.Simple;

        /// <summary>
        /// Gets or sets the property accessor.
        /// </summary>
        public IPropertyAccessor? Accessor { get; set; }

        /// <summary>
        /// Gets or sets the bean property type.
        /// </summary>
        public Type? PropertyType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this parser or any descendant of this parser is used to identify
        /// a record during unmarshalling.
        /// </summary>
        public override bool IsIdentifier { get; set; }

        /// <summary>
        /// Gets a value indicating whether this node must exist during unmarshalling.
        /// </summary>
        public override bool IsOptional => Format.IsLazy;

        public IFieldFormat Format
        {
            get => _format ?? throw new InvalidOperationException("Format property not initialized");
            set => _format = value;
        }

        public bool IsBound { get; set; }

        public string? Regex
        {
            get { return _regex?.ToString(); }
            set { _regex = value == null ? null : new Regex(value); }
        }

        public string? Literal { get; set; }

        public bool IsTrim { get; set; }

        public bool IsRequired { get; set; }

        public bool IsLazy { get; set; }

        public int MinLength { get; set; }

        public int? MaxLength { get; set; }

        public object? DefaultValue { get; set; }

        public ITypeHandler? Handler { get; set; }

        public bool ErrorIfNullPrimitive { get; set; }

        public bool UseDefaultIfMissing { get; set; }

        public bool ParseDefault { get; set; }

        public bool MarshalDefault { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the field(s) should be validated during marshalling.
        /// </summary>
        public bool ValidateOnMarshal { get; set; }

        protected Regex? RegexPattern => _regex;

        /// <summary>
        /// Returns whether this parser and its children match a record being unmarshalled.
        /// </summary>
        /// <param name="context">The <see cref="UnmarshallingContext"/>.</param>
        /// <returns>true if matched, false otherwise.</returns>
        public override bool Matches(UnmarshallingContext context)
        {
            if (!IsIdentifier)
                return true;
            return IsMatch(Format.Extract(context, false));
        }

        /// <summary>
        /// Unmarshals a record.
        /// </summary>
        /// <param name="context">The <see cref="UnmarshallingContext"/>.</param>
        /// <returns>true if this component was present in the unmarshalled record, or false otherwise.</returns>
        public override bool Unmarshal(UnmarshallingContext context)
        {
            var text = Format.Extract(context, true);
            if (text == null)
            {
                object value = Value.Missing;
                if (UseDefaultIfMissing && DefaultValue != null && ParseDefault)
                    value = DefaultValue;
                SetValue(context, value);
                return false;
            }

            if (!ParseDefault)
            {
                var defaultValueAsString = (string?)DefaultValue;
                if (defaultValueAsString != null && string.Equals(text, defaultValueAsString))
                {
                    text = null;

                    // collections are not further validated
                    if (IsRequired)
                    {
                        context.AddFieldError(Name, null, "required");
                        text = Value.Invalid;
                    }

                    _value.Set(context, text);

                    return true;
                }
            }

            if (ReferenceEquals(text, Value.Invalid))
                _value.Set(context, text);
            else
                _value.Set(context, ParseValue(context, text));

            return true;
        }

        /// <summary>
        /// Marshals a record.
        /// </summary>
        /// <param name="context">The <see cref="MarshallingContext"/>.</param>
        /// <returns>whether a value was marshalled.</returns>
        public override bool Marshal(MarshallingContext context)
        {
            string? text;

            if (Literal != null)
            {
                text = Literal;
            }
            else
            {
                var value = GetValue(context);

                // the default value may be used to override null property values
                // if enabled (since 1.2.2)
                if (MarshalDefault && ReferenceEquals(value, Value.Missing))
                {
                    value = DefaultValue;
                    SetValue(context, DefaultValue);
                }

                if (ReferenceEquals(value, Value.Missing))
                {
                    value = null;
                    SetValue(context, null);
                }

                // allow the format to bypass type conversion
                if (Format.InsertValue(context, value))
                    return true;

                var stringValue = value as string;
                if (ParseDefault || stringValue == null)
                {
                    text = FormatValue(value);
                }
                else
                {
                    text = stringValue;
                }
            }

            if (ValidateOnMarshal)
            {
                if (ReferenceEquals(text, Value.Nil))
                {
                    if (!Format.IsNillable)
                    {
                        throw new InvalidBeanException($"Invalid field '{Name}', the value is not nillable");
                    }

                    if (IsRequired)
                    {
                        throw new InvalidBeanException($"Invalid field '{Name}', a value is required");
                    }
                }
                else if (text == null)
                {
                    if (IsRequired)
                    {
                        throw new InvalidBeanException($"Invalid field '{Name}', a value is required");
                    }
                }
                else
                {
                    // validate minimum length
                    if (text.Length < MinLength)
                    {
                        throw new InvalidBeanException($"Invalid field '{Name}', '{text}' does not meet minimum length of {MinLength}");
                    }

                    // validate maximum length
                    if (text.Length > MaxLength)
                    {
                        throw new InvalidBeanException($"Invalid field '{Name}', '{text}' exceeds maximum length of {MaxLength}");
                    }

                    // validate the regular expression
                    if (RegexPattern != null && !RegexPattern.IsMatch(text))
                    {
                        throw new InvalidBeanException($"Invalid field '{Name}', '{text}' does not match pattern '{Regex}'");
                    }
                }
            }

            Format.InsertField(context, text);
            return true;
        }

        /// <summary>
        /// Returns whether this parser or any of its descendant have content for marshalling.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/>.</param>
        /// <returns>true if there is content for marshalling, false otherwise.</returns>
        public override bool HasContent(ParsingContext context)
        {
            if (!IsBound)
                return true;
            return !ReferenceEquals(GetValue(context), Value.Missing);
        }

        /// <summary>
        /// Clears the current property value.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/>.</param>
        public override void ClearValue(ParsingContext context)
        {
            _value.Set(context, Value.Missing);
        }

        /// <summary>
        /// Creates the property value and returns it.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/>.</param>
        /// <returns>the property value.</returns>
        public object? CreateValue(ParsingContext context)
        {
            return GetValue(context);
        }

        /// <summary>
        /// Sets the property value for marshaling.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/>.</param>
        /// <param name="value">the property value.</param>
        public override void SetValue(ParsingContext context, object? value)
        {
            _value.Set(context, value ?? Value.Missing);
        }

        /// <summary>
        /// Returns the unmarshalled property value.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/>.</param>
        /// <returns>the property value.</returns>
        public override object? GetValue(ParsingContext context)
        {
            return _value.Get(context);
        }

        /// <summary>
        /// Returns whether a node is a supported child of this node.
        /// </summary>
        /// <remarks>
        /// Called by <see cref="TreeNode{T}.Add"/>.
        /// </remarks>
        /// <param name="child">the node to test.</param>
        /// <returns>true if the child is allowed.</returns>
        public override bool IsSupportedChild(Component child)
        {
            return false;
        }

        /// <summary>
        /// Called by a stream to register variables stored in the parsing context.
        /// </summary>
        /// <remarks>
        /// This method should be overridden by subclasses that need to register
        /// one or more parser context variables.
        /// </remarks>
        /// <param name="locals">set of local variables.</param>
        public override void RegisterLocals(ISet<IParserLocal> locals)
        {
            if (locals.Add(_value))
                base.RegisterLocals(locals);
        }

        /// <inheritdoc />
        public bool Defines(object? value)
        {
            if (value == null)
                return false;
            if (!IsIdentifier)
                return true;

            if (PropertyType != null && !value.GetType().IsInstanceOf(PropertyType))
                return false;

            return IsMatch(FormatValue(value));
        }

        protected virtual bool IsMatch(string? text)
        {
            if (text == null || ReferenceEquals(text, Value.Invalid) || ReferenceEquals(text, Value.Nil))
                return false;
            if (Literal != null && !string.Equals(Literal, text, StringComparison.Ordinal))
                return false;
            if (RegexPattern != null && !RegexPattern.IsMatch(text))
                return false;
            return true;
        }

        protected virtual string? FormatValue(object? value)
        {
            string? text;

            if (Handler != null)
            {
                try
                {
                    text = Handler.Format(value);
                    if (ReferenceEquals(text, Value.Nil))
                    {
                        if (!Format.IsNillable)
                            text = null;
                    }
                }
                catch (Exception ex)
                {
                    throw new BeanWriterException($"Type conversion failed for field '{Name}' while formatting value '{value}'", ex);
                }
            }
            else
            {
                text = value?.ToString();
            }

            return text;
        }

        protected virtual object? ParseValue(UnmarshallingContext context, string? fieldText)
        {
            var valid = true;
            var text = fieldText;

            if (ReferenceEquals(text, Value.Nil))
            {
                // validate field is nillable
                if (!Format.IsNillable)
                {
                    context.AddFieldError(Name, null, "nillable");
                    return Value.Invalid;
                }

                // collections are not further validated
                if (IsRequired)
                {
                    context.AddFieldError(Name, null, "required");
                    return Value.Invalid;
                }

                // return the default value if set
                return DefaultValue;
            }

            // repeating fields are always optional
            if (text == null)
            {
                if (!Format.IsLazy)
                {
                    context.AddFieldError(Name, null, "minOccurs", 1);
                    return Value.Invalid;
                }
            }
            else
            {
                // trim before validation if configured
                if (IsTrim)
                    text = text.Trim();
                if (IsLazy && text.Length == 0)
                    text = null;
            }

            // check if field exists
            if (string.IsNullOrEmpty(text))
            {
                // validation for required fields
                if (IsRequired)
                {
                    context.AddFieldError(Name, fieldText, "required");
                    valid = false;
                }
                else if (DefaultValue != null)
                {
                    // return the default value if set
                    return DefaultValue;
                }
            }
            else
            {
                // validate constant fields
                if (Literal != null && Literal != text)
                {
                    context.AddFieldError(Name, fieldText, "literal", Literal);
                    valid = false;
                }

                // validate minimum length
                if (text!.Length < MinLength)
                {
                    context.AddFieldError(Name, fieldText, "minLength", MinLength, MaxLength);
                    valid = false;
                }

                // validate maximum length
                if (MaxLength != null && text.Length > MaxLength)
                {
                    context.AddFieldError(Name, fieldText, "maxLength", MinLength, MaxLength);
                    valid = false;
                }

                // validate the regular expression
                if (RegexPattern is { } regex && !regex.IsMatch(text))
                {
                    context.AddFieldError(Name, fieldText, "regex", Regex);
                    valid = false;
                }
            }

            // type conversion is skipped if the text does not pass other validations
            if (!valid)
                return Value.Invalid;

            // perform type conversion and return the result
            try
            {
                var value = (Handler == null) ? text : Handler.Parse(text);
                if (value == null && ErrorIfNullPrimitive && PropertyType != null && PropertyType.IsPrimitive)
                {
                    context.AddFieldError(Name, fieldText, "type", "Primitive property values cannot be null");
                    return Value.Invalid;
                }

                return value;
            }
            catch (TypeConversionException ex)
            {
                context.AddFieldError(Name, fieldText, "type", ex.Message);
                return Value.Invalid;
            }
            catch (FormatException ex)
            {
                context.AddFieldError(Name, fieldText, "type", ex.Message);
                return Value.Invalid;
            }
            catch (Exception ex)
            {
                throw new BeanReaderException($"Type conversion failed for field '{Name}' while parsing text '{fieldText}'", ex, context.GetRecordContexts().ToArray());
            }
        }
    }
}
