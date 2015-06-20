using System;

using Newtonsoft.Json.Linq;

namespace BeanIO.Internal.Parser.Format.Json
{
    /// <summary>
    /// A <see cref="IFieldFormat"/> implementation for a field in a JSON formatted record.
    /// </summary>
    internal class JsonFieldFormat : IFieldFormat, IJsonNode
    {
        /// <summary>
        /// Gets the size of the field
        /// </summary>
        /// <remarks>
        /// Fixed length formats should return the field length, while other formats should simply return 1.
        /// </remarks>
        public virtual int Size
        {
            get { return 1; }
        }

        /// <summary>
        /// Gets or sets the field name.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the JSON field name.
        /// </summary>
        public virtual string JsonName { get; set; }

        /// <summary>
        /// Gets or sets the type of node.
        /// </summary>
        /// <remarks>
        /// If <see cref="IJsonNode.IsJsonArray"/> is true, this method returns the component type of the array.
        /// </remarks>
        public virtual JTokenType JsonType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this node is a JSON array.
        /// </summary>
        public virtual bool IsJsonArray { get; set; }

        /// <summary>
        /// Gets or sets the index of this node in its parent array, or -1 if not applicable
        /// (i.e. its parent is an object).
        /// </summary>
        /// <remarks>
        /// Set to the index of this field in a JSON array, or -1 if the field itself repeats
        /// </remarks>
        public virtual int JsonArrayIndex { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether this node may be explicitly set to <code>null</code>.
        /// </summary>
        public virtual bool IsNillable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this field is optionally present in the record.
        /// </summary>
        /// <remarks>
        /// TODO: rename isLazy to something better??
        /// </remarks>
        public virtual bool IsLazy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether type conversion can be bypassed and the value directly set into the map
        /// </summary>
        public virtual bool BypassTypeHandler { get; set; }

        /// <summary>
        /// Gets or sets the field padding
        /// </summary>
        /// <remarks>
        /// null if the field text is not padded
        /// </remarks>
        public virtual FieldPadding Padding { get; set; }

        /// <summary>
        /// Extracts the field text from a record.
        /// </summary>
        /// <remarks>
        /// <para>May return <see cref="F:Value.Invalid"/> if the field is invalid, or <see cref="F:Value.Nil"/>
        /// if the field is explicitly set to nil or null such as in an XML or JSON formatted
        /// stream.</para>
        /// <para>Implementations should also remove any field padding before returning the text.</para>
        /// </remarks>
        /// <param name="context">the <see cref="UnmarshallingContext"/> holding the record</param>
        /// <param name="reportErrors">report the errors?</param>
        /// <returns>the field text or null if the field was not present in the record</returns>
        public virtual string Extract(UnmarshallingContext context, bool reportErrors)
        {
            var ctx = (JsonUnmarshallingContext)context;

            var value = ctx.GetValue(this);

            // nothing more to do with null or missing values
            if (value == null || ReferenceEquals(value, Value.Nil))
            {
                ctx.SetFieldText(Name, null);
                return (string)value;
            }

            // extract the field from a list if repeating
            if (IsJsonArray)
            {
                var index = -1; // jsonArrayIndex; // TODO is this needed?
                if (index < 0)
                    index = ctx.GetRelativeFieldIndex();

                try
                {
                    var array = (JArray)value;
                    if (index >= array.Count)
                        return null;
                    value = array[index];
                }
                catch (InvalidCastException)
                {
                    // if index is greater than zero, we're trying to get next value
                    // which doesn't exist if the value isn't a list so return null
                    // instead of repetitively reporting the same field error
                    if (index > 0 && JsonArrayIndex < 0)
                        return null;

                    var tempFieldText = value.ToString();
                    ctx.SetFieldText(Name, tempFieldText);

                    if (reportErrors)
                    {
                        context.AddFieldError(Name, tempFieldText, "jsontype", this.GetTypeDescription());
                    }

                    return Value.Invalid;
                }
            }

            // TODO validate JSON type (how should this be configured...?)

            // convert to field text
            var fieldText = value.ToString();
            ctx.SetFieldText(Name, fieldText);

            // handle padded fields
            if (Padding != null)
            {
                int length = fieldText.Length;
                if (length == 0)
                {
                    // this will either cause a required validation error or map
                    // to a null value depending on the value of 'required'
                    return string.Empty;
                }

                if (length != Padding.Length)
                {
                    if (reportErrors)
                    {
                        context.AddFieldError(Name, fieldText, "length", Padding.Length);
                    }
                    fieldText = Value.Invalid;
                }
                else
                {
                    fieldText = Padding.Unpad(fieldText);
                }
            }

            return fieldText;
        }

        /// <summary>
        /// Inserts a value into a record
        /// </summary>
        /// <remarks>
        /// <para>This method is called before type conversion.</para>
        /// <para>If the method returns false, type conversion is invoked and <see cref="IFieldFormat.InsertField"/>
        /// is called. If the method returns true, <see cref="IFieldFormat.InsertField"/>
        /// is not invoked.</para>
        /// </remarks>
        /// <param name="context">the <see cref="MarshallingContext"/></param>
        /// <param name="value">the value to insert into the record</param>
        /// <returns>true if type conversion is required and <see cref="IFieldFormat.InsertField"/> must be invoked, false otherwise</returns>
        public virtual bool InsertValue(MarshallingContext context, object value)
        {
            if (!BypassTypeHandler)
                return false;

            var ctx = (JsonMarshallingContext)context;

            if (ReferenceEquals(value, Value.Nil))
            {
                ctx.Put(this, null);
            }
            else if (value == null && IsLazy)
            {
                // do nothing
            }
            else
            {
                ctx.Put(this, (value as JToken) ?? new JValue(value));
            }

            return true;
        }

        /// <summary>
        /// Inserts field text into a record
        /// </summary>
        /// <param name="context">the <see cref="MarshallingContext"/> holding the record</param>
        /// <param name="text">the field text to insert into the record</param>
        public virtual void InsertField(MarshallingContext context, string text)
        {
            var ctx = (JsonMarshallingContext)context;

            if (text == Value.Nil)
            {
                ctx.Put(this, null);
                return;
            }

            if (text == null && IsLazy)
            {
                return;
            }

            JToken value;

            if (text != null)
            {
                // convert text to JSON type
                switch (JsonType)
                {
                    case JTokenType.String:
                        value = new JValue(text);
                        break;
                    default:
                        try
                        {
                            value = JToken.Parse(text);
                        }
                        catch (Exception ex)
                        {
                            throw new BeanWriterException("Cannot parse '" + text + "' into a JSON number", ex);
                        }
                        break;
                }
            }
            else
            {
                value = null;
            }

            ctx.Put(this, value);
        }

        public override string ToString()
        {
            return string.Format(
                "{0}[name={1}, jsonName={2}, jsonType={3}{4}, jsonArrayIndex={5}, bypass={6}]",
                GetType().Name,
                Name,
                JsonName,
                JsonType,
                IsJsonArray ? "[]" : string.Empty,
                JsonArrayIndex,
                BypassTypeHandler);
        }
    }
}
