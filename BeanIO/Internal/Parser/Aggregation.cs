using System;
using System.Collections.Generic;
using System.Text;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// Base class for parser components capable of aggregating descendant properties
    /// </summary>
    public abstract class Aggregation : DelegatingParser, IProperty, IIteration
    {
        private readonly ParserLocal<int?> _index = new ParserLocal<int?>();

        /// <summary>
        /// Gets a value indicating whether this aggregation is a property of its parent bean object.
        /// </summary>
        public abstract bool IsProperty { get; }

        /// <summary>
        /// Gets or sets the property that dictates the number of occurrences or null if its not dynamic
        /// </summary>
        public Field Occurs { get; set; }

        /// <summary>
        /// Gets or sets the minimum occurrences
        /// </summary>
        public int MinOccurs { get; set; }

        /// <summary>
        /// Gets or sets the maximum occurrences
        /// </summary>
        public int? MaxOccurs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether null should be returned for an empty collection
        /// </summary>
        public bool IsLazy { get; set; }

        /// <summary>
        /// Gets the <see cref="IProperty"/> implementation type
        /// </summary>
        public abstract PropertyType Type { get; }

        /// <summary>
        /// Gets a value indicating whether this parser or any descendant of this parser is used to identify
        /// a record during unmarshalling.
        /// </summary>
        /// <returns>false; iterations cannot be used to identify records</returns>
        public override bool IsIdentifier
        {
            get
            {
                return false;
            }
            set
            {
                if (value)
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Gets or sets the property accessor
        /// </summary>
        public IPropertyAccessor Accessor { get; set; }

        /// <summary>
        /// Gets or sets the bean property type
        /// </summary>
        public Type PropertyType { get; set; }

        /// <summary>
        /// Gets the size of the components that make up a single iteration.
        /// </summary>
        public abstract int IterationSize { get; }

        /// <summary>
        /// Gets a value indicating whether the iteration size is variable based on another field in the record.
        /// </summary>
        public bool IsDynamicIteration
        {
            get { return Occurs != null; }
        }

        /// <summary>
        /// Gets a value indicating whether this node must exist during unmarshalling.
        /// </summary>
        public override bool IsOptional
        {
            get { return MinOccurs == 0; }
        }

        /// <summary>
        /// Returns the length of aggregation
        /// </summary>
        /// <param name="value">the aggregation value</param>
        /// <returns>the length</returns>
        public abstract int Length(object value);

        /// <summary>
        /// Creates the property value and returns it
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <returns>the property value</returns>
        public abstract object CreateValue(ParsingContext context);

        public abstract bool Defines(object value);

        /// <summary>
        /// Returns the index of the current iteration relative to its parent.
        /// </summary>
        /// <param name="context">The context of this iteration</param>
        /// <returns>the index of the current iteration</returns>
        public int GetIterationIndex(ParsingContext context)
        {
            return _index.Get(context).GetValueOrDefault();
        }

        /// <summary>
        /// Called by a stream to register variables stored in the parsing context.
        /// </summary>
        /// <remarks>
        /// This method should be overridden by subclasses that need to register
        /// one or more parser context variables.
        /// </remarks>
        /// <param name="locals">set of local variables</param>
        public override void RegisterLocals(ISet<IParserLocal> locals)
        {
            if (locals.Add(_index))
                base.RegisterLocals(locals);
        }

        /// <summary>
        /// Marshals a record
        /// </summary>
        /// <param name="context">The <see cref="MarshallingContext"/></param>
        /// <returns>whether a value was marshalled</returns>
        public override bool Marshal(MarshallingContext context)
        {
            var min = MinOccurs;
            var max = MaxOccurs;

            // handle dynamic occurrences
            if (Occurs != null)
            {
                max = min = (int)Convert.ToDecimal(Occurs.GetValue(context));
                SetIterationIndex(context, -1);
            }

            return Marshal(context, Parser, min, max);
        }

        /// <summary>
        /// Unmarshals a record
        /// </summary>
        /// <param name="context">The <see cref="UnmarshallingContext"/></param>
        /// <returns>true if this component was present in the unmarshalled record, or false otherwise</returns>
        public override bool Unmarshal(UnmarshallingContext context)
        {
            var min = MinOccurs;
            var max = MaxOccurs;

            // handle dynamic occurrences
            if (Occurs != null)
            {
                var n = Occurs.GetValue(context);
                if (ReferenceEquals(n, Value.Invalid))
                    throw new AbortRecordUnmarshalligException("Invalid occurrences");
                if (ReferenceEquals(n, Value.Missing))
                    n = 0;
                var occursVal = (int)Convert.ToDecimal(n);
                if (occursVal < min)
                {
                    context.AddFieldError(Name, null, "minOccurs", min, max);

                    // this prevents a duplicate exception being thrown by a parent segment:
                    if (occursVal == 0)
                        return true;
                }
                else if (max != null && occursVal > max)
                {
                    context.AddFieldError(Name, null, "maxOccurs", min, max);
                }

                max = min = occursVal;
                SetIterationIndex(context, -1);
            }

            return Unmarshal(context, Parser, min, max);
        }

        /// <summary>
        /// Sets the property value for marshaling.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <param name="value">the property value</param>
        public override void SetValue(ParsingContext context, object value)
        {
            if (Occurs != null && !Occurs.IsBound)
            {
                Occurs.SetValue(context, Length(value));
            }
        }

        protected abstract bool Marshal(MarshallingContext context, IParser parser, int minOccurs, int? maxOccurs);

        protected abstract bool Unmarshal(UnmarshallingContext context, IParser parser, int minOccurs, int? maxOccurs);

        protected void SetIterationIndex(ParsingContext context, int index)
        {
            _index.Set(context, index);
        }

        /// <summary>
        /// Called by <see cref="TreeNode{T}.ToString"/> to append node parameters to the output
        /// </summary>
        /// <param name="s">The output to append</param>
        protected override void ToParamString(StringBuilder s)
        {
            base.ToParamString(s);
            if (Occurs != null)
                s.AppendFormat("occursRef=${0}", Occurs.Name);
            s.AppendFormat(", occurs={0}", DebugUtil.FormatRange(MinOccurs, MaxOccurs))
             .AppendFormat(", {0}", DebugUtil.FormatOption("lazy", IsLazy));
        }
    }
}
