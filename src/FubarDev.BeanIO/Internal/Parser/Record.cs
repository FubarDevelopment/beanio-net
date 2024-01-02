// <copyright file="Record.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser
{
    internal class Record : Segment, ISelector
    {
        private readonly ParserLocal<int> _count = new ParserLocal<int>(0);

        /// <summary>
        /// Initializes a new instance of the <see cref="Record"/> class.
        /// </summary>
        public Record()
        {
            Order = 1;
            MaxOccurs = int.MaxValue;
        }

        /// <summary>
        /// Gets or sets the minimum number of occurrences of this component (within the context of its parent).
        /// </summary>
        public int MinOccurs { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of occurrences of this component (within the context of its parent).
        /// </summary>
        public int? MaxOccurs { get; set; }

        /// <summary>
        /// Gets or sets the order of this component (within the context of its parent).
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the record format.
        /// </summary>
        public IRecordFormat? RecordFormat { get; set; }

        /// <summary>
        /// Gets a value indicating whether this component is a record group.
        /// </summary>
        public bool IsRecordGroup => false;

        /// <summary>
        /// Returns the number of times this component was matched within the current
        /// iteration of its parent component.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/>.</param>
        /// <returns>the match count.</returns>
        public int GetCount(ParsingContext context)
        {
            return _count.Get(context);
        }

        /// <summary>
        /// Sets the number of times this component was matched within the current
        /// iteration of its parent component.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/>.</param>
        /// <param name="value">the new count.</param>
        public void SetCount(ParsingContext context, int value)
        {
            _count.Set(context, value);
        }

        /// <summary>
        /// Returns a value indicating whether this component has reached its maximum occurrences.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/>.</param>
        /// <returns>true if maximum occurrences has been reached.</returns>
        public bool IsMaxOccursReached(ParsingContext context)
        {
            return MaxOccurs != null && GetCount(context) >= MaxOccurs;
        }

        /// <summary>
        /// Finds a parser for marshalling a bean object.
        /// </summary>
        /// <remarks>
        /// If matched by this Selector, the method
        /// should set the bean object on the property tree and return itself.
        /// </remarks>
        /// <param name="context">the <see cref="MarshallingContext"/>.</param>
        /// <returns>the matched <see cref="ISelector"/> for marshalling the bean object.</returns>
        public ISelector? MatchNext(MarshallingContext context)
        {
            var property = Property;
            if (property != null)
            {
                var componentName = context.ComponentName;
                if (componentName != null && Name != componentName)
                    return null;

                var value = context.Bean;
                if (property.Defines(value))
                {
                    SetCount(context, GetCount(context) + 1);
                    property.SetValue(context, value);
                    return this;
                }
            }
            else if (context.Bean == null)
            {
                var componentName = context.ComponentName;
                if (componentName != null && Name == componentName)
                {
                    SetCount(context, GetCount(context) + 1);
                    return this;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds a parser for unmarshalling a record based on the current state of the stream.
        /// </summary>
        /// <param name="context">the <see cref="UnmarshallingContext"/>.</param>
        /// <returns>the matched <see cref="ISelector"/> for unmarshalling the record.</returns>
        public ISelector? MatchNext(UnmarshallingContext context)
        {
            if (Matches(context))
            {
                SetCount(context, GetCount(context) + 1);
                return this;
            }

            return null;
        }

        /// <summary>
        /// Finds a parser that matches the input record.
        /// </summary>
        /// <remarks>
        /// This method is invoked when <see cref="ISelector.MatchNext(BeanIO.Internal.Parser.UnmarshallingContext)"/> returns
        /// null, in order to differentiate between unexpected and unidentified record types.
        /// </remarks>
        /// <param name="context">the <see cref="UnmarshallingContext"/>.</param>
        /// <returns>the matched <see cref="ISelector"/>.</returns>
        public ISelector? MatchAny(UnmarshallingContext context)
        {
            return Matches(context) ? this : null;
        }

        /// <summary>
        /// Skips a record or group of records.
        /// </summary>
        /// <param name="context">the <see cref="UnmarshallingContext"/>.</param>
        public void Skip(UnmarshallingContext context)
        {
            context.RecordSkipped();
        }

        /// <summary>
        /// Checks for any unsatisfied components before the stream is closed.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/>.</param>
        /// <returns>the first unsatisfied node.</returns>
        public ISelector? Close(ParsingContext context)
        {
            return GetCount(context) < MinOccurs ? this : null;
        }

        /// <summary>
        /// Resets the component count of this Selector's children.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/>.</param>
        public void Reset(ParsingContext context)
        {
        }

        /// <summary>
        /// Updates a <see cref="IDictionary{TKey,TValue}"/> with the current state of the Writer to allow for restoration at a later time.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/>.</param>
        /// <param name="ns">a <see cref="string"/> to prefix all state keys with.</param>
        /// <param name="state">the <see cref="IDictionary{TKey,TValue}"/> to update with the latest state.</param>
        public void UpdateState(ParsingContext context, string ns, IDictionary<string, object?> state)
        {
            state[GetKey(ns, DefaultSelectorStateKeys.CountKey)] = _count.Get(context);
        }

        /// <summary>
        /// Restores a <see cref="IDictionary{TKey,TValue}"/> of previously stored state information.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/>.</param>
        /// <param name="ns">a <see cref="string"/> to prefix all state keys with.</param>
        /// <param name="state">the <see cref="IDictionary{TKey,TValue}"/> containing the state to restore.</param>
        public void RestoreState(ParsingContext context, string ns, IReadOnlyDictionary<string, object?> state)
        {
            var key = GetKey(ns, DefaultSelectorStateKeys.CountKey);
            if (!state.TryGetValue(key, out var n))
                throw new InvalidOperationException($"Missing state information for key '{key}'");
            _count.Set(context, (int?)n ?? 0);
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
            if (locals.Add(_count))
                base.RegisterLocals(locals);
        }

        /// <summary>
        /// Marshals a record.
        /// </summary>
        /// <param name="context">The <see cref="MarshallingContext"/>.</param>
        /// <returns>whether a value was marshalled.</returns>
        public override bool Marshal(MarshallingContext context)
        {
            try
            {
                var marshalled = base.Marshal(context);
                if (marshalled)
                    context.WriteRecord();
                return marshalled;
            }
            finally
            {
                context.Clear();
            }
        }

        /// <summary>
        /// Unmarshals a record.
        /// </summary>
        /// <param name="context">The <see cref="UnmarshallingContext"/>.</param>
        /// <returns>true if this component was present in the unmarshalled record, or false otherwise.</returns>
        public override bool Unmarshal(UnmarshallingContext context)
        {
            try
            {
                // update the record context before unmarshalling
                context.RecordStarted(Name);

                if (RecordFormat != null)
                {
                    RecordFormat.Validate(context);
                    if (context.HasRecordErrors)
                        return true;
                }

                // invoke segment unmarshalling
                base.Unmarshal(context);

                return true;
            }
            finally
            {
                context.RecordCompleted();
            }
        }

        /// <summary>
        /// Returns whether this parser and its children match a record being unmarshalled.
        /// </summary>
        /// <param name="context">The <see cref="UnmarshallingContext"/>.</param>
        /// <returns>true if matched, false otherwise.</returns>
        public override bool Matches(UnmarshallingContext context)
        {
            if (RecordFormat != null && !RecordFormat.Matches(context))
                return false;
            return base.Matches(context);
        }

        /// <summary>
        /// Called by <see cref="TreeNode{T}.ToString"/> to append node parameters to the output.
        /// </summary>
        /// <param name="s">The output to append.</param>
        protected override void ToParamString(StringBuilder s)
        {
            s
                .AppendFormat(", order={0}", Order)
                .AppendFormat(", occurs={0}", DebugUtil.FormatRange(MinOccurs, MaxOccurs));
            base.ToParamString(s);
            s.AppendFormat(", format={0}", RecordFormat);
        }

        /// <summary>
        /// Returns a <see cref="IDictionary{TKey,TValue}"/> key for accessing state information for this <see cref="ISelector"/>.
        /// </summary>
        /// <param name="ns">the assigned namespace for the key.</param>
        /// <param name="name">the state information to access.</param>
        /// <returns>the fully qualified key.</returns>
        protected string GetKey(string ns, string name)
        {
            return $"{ns}.{Name}.{name}";
        }
    }
}
