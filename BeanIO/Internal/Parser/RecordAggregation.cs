using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser
{
    internal abstract class RecordAggregation : DelegatingParser, ISelector, IProperty
    {
        /// <summary>
        /// The property value
        /// </summary>
        private readonly ParserLocal<object> _value = new ParserLocal<object>(Value.Missing);

        /// <summary>
        /// Gets or sets the bean property type
        /// </summary>
        public virtual Type PropertyType { get; set; }

        /// <summary>
        /// Gets the minimum number of occurrences of this component (within the context of its parent).
        /// </summary>
        public virtual int MinOccurs
        {
            get { return Selector.MinOccurs; }
        }

        /// <summary>
        /// Gets the maximum number of occurrences of this component (within the context of its parent).
        /// </summary>
        public virtual int? MaxOccurs
        {
            get { return Selector.MaxOccurs; }
        }

        /// <summary>
        /// Gets the order of this component (within the context of its parent).
        /// </summary>
        public virtual int Order
        {
            get { return Selector.Order; }
        }

        /// <summary>
        /// Gets the <see cref="IProperty"/> mapped to this component, or null if there is no property mapping.
        /// </summary>
        public virtual IProperty Property
        {
            get
            {
                // for now, a collection cannot be a property root so its safe to return null here
                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this component is a record group.
        /// </summary>
        public virtual bool IsRecordGroup
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the <see cref="IProperty"/> implementation type
        /// </summary>
        public abstract PropertyType Type { get; }

        /// <summary>
        /// Gets or sets the property accessor
        /// </summary>
        public virtual IPropertyAccessor Accessor { get; set; }

        /// <summary>
        /// Gets the child selector
        /// </summary>
        public virtual ISelector Selector
        {
            get { return (ISelector)Children.First(); }
        }

        public virtual object NullValue
        {
            get { return CreateAggregationType(); }
        }

        public virtual bool IsLazy { get; set; }

        /// <summary>
        /// Gets a value indicating whether this parser or any descendant of this parser is used to identify
        /// a record during unmarshalling.
        /// </summary>
        public override bool IsIdentifier
        {
            get
            {
                // a collection cannot be used to identify a bean
                return false;
            }
            set
            {
                // a collection cannot be used to identify a bean
            }
        }

        /// <summary>
        /// Gets the property value
        /// </summary>
        protected ParserLocal<object> PropertyValue
        {
            get { return _value; }
        }

        /// <summary>
        /// Returns the number of times this component was matched within the current
        /// iteration of its parent component.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <returns>the match count</returns>
        public virtual int GetCount(ParsingContext context)
        {
            return Selector.GetCount(context);
        }

        /// <summary>
        /// Sets the number of times this component was matched within the current
        /// iteration of its parent component.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <param name="value">the new count</param>
        public virtual void SetCount(ParsingContext context, int value)
        {
            Selector.SetCount(context, value);
        }

        /// <summary>
        /// Returns a value indicating whether this component has reached its maximum occurrences
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <returns>true if maximum occurrences has been reached</returns>
        public virtual bool IsMaxOccursReached(ParsingContext context)
        {
            return Selector.IsMaxOccursReached(context);
        }

        /// <summary>
        /// Finds a parser for marshalling a bean object
        /// </summary>
        /// <remarks>
        /// If matched by this Selector, the method
        /// should set the bean object on the property tree and return itself.
        /// </remarks>
        /// <param name="context">the <see cref="MarshallingContext"/></param>
        /// <returns>the matched <see cref="ISelector"/> for marshalling the bean object</returns>
        public virtual ISelector MatchNext(MarshallingContext context)
        {
            return Selector.MatchNext(context);
        }

        /// <summary>
        /// Finds a parser for unmarshalling a record based on the current state of the stream.
        /// </summary>
        /// <param name="context">the <see cref="UnmarshallingContext"/></param>
        /// <returns>the matched <see cref="ISelector"/> for unmarshalling the record</returns>
        public virtual ISelector MatchNext(UnmarshallingContext context)
        {
            if (Selector.MatchNext(context) != null)
                return this;
            return null;
        }

        /// <summary>
        /// Finds a parser that matches the input record
        /// </summary>
        /// <remarks>
        /// This method is invoked when <see cref="ISelector.MatchNext(BeanIO.Internal.Parser.UnmarshallingContext)"/> returns
        /// null, in order to differentiate between unexpected and unidentified record types.
        /// </remarks>
        /// <param name="context">the <see cref="UnmarshallingContext"/></param>
        /// <returns>the matched <see cref="ISelector"/></returns>
        public virtual ISelector MatchAny(UnmarshallingContext context)
        {
            return Selector.MatchAny(context);
        }

        /// <summary>
        /// Skips a record or group of records.
        /// </summary>
        /// <param name="context">the <see cref="UnmarshallingContext"/></param>
        public virtual void Skip(UnmarshallingContext context)
        {
            Selector.Skip(context);
        }

        /// <summary>
        /// Checks for any unsatisfied components before the stream is closed.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <returns>the first unsatisfied node</returns>
        public virtual ISelector Close(ParsingContext context)
        {
            return Selector.Close(context);
        }

        /// <summary>
        /// Resets the component count of this Selector's children.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        public virtual void Reset(ParsingContext context)
        {
            Selector.Reset(context);
        }

        /// <summary>
        /// Updates a <see cref="IDictionary{TKey,TValue}"/> with the current state of the Writer to allow for restoration at a later time.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <param name="ns">a <see cref="string"/> to prefix all state keys with</param>
        /// <param name="state">the <see cref="IDictionary{TKey,TValue}"/> to update with the latest state</param>
        public virtual void UpdateState(ParsingContext context, string ns, IDictionary<string, object> state)
        {
            Selector.UpdateState(context, ns, state);
        }

        /// <summary>
        /// Restores a <see cref="IDictionary{TKey,TValue}"/> of previously stored state information
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <param name="ns">a <see cref="string"/> to prefix all state keys with</param>
        /// <param name="state">the <see cref="IDictionary{TKey,TValue}"/> containing the state to restore</param>
        public virtual void RestoreState(ParsingContext context, string ns, IReadOnlyDictionary<string, object> state)
        {
            Selector.RestoreState(context, ns, state);
        }

        /// <summary>
        /// Creates the property value and returns it
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <returns>the property value</returns>
        public virtual object CreateValue(ParsingContext context)
        {
            if (ReferenceEquals(_value.Get(context), Value.Missing))
                _value.Set(context, CreateAggregationType());
            return GetValue(context);
        }

        /// <summary>
        /// Clears the current property value.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        public override void ClearValue(ParsingContext context)
        {
            _value.Set(context, Value.Missing);
        }

        /// <summary>
        /// Returns the unmarshalled property value.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <returns>the property value</returns>
        public override object GetValue(ParsingContext context)
        {
            return _value.Get(context);
        }

        /// <summary>
        /// Sets the property value for marshaling.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <param name="value">the property value</param>
        public override void SetValue(ParsingContext context, object value)
        {
            _value.Set(context, value);
        }

        public virtual bool Defines(object value)
        {
            throw new InvalidOperationException("RecordAggregation cannot identify a bean");
        }

        /// <summary>
        /// Returns whether a node is a supported child of this node.
        /// </summary>
        /// <remarks>
        /// Called by <see cref="TreeNode{T}.Add"/>.
        /// </remarks>
        /// <param name="child">the node to test</param>
        /// <returns>true if the child is allowed</returns>
        public override bool IsSupportedChild(Component child)
        {
            return child is ISelector;
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
            if (locals.Add(_value))
                base.RegisterLocals(locals);
        }

        protected virtual object CreateAggregationType()
        {
            return PropertyType.NewInstance();
        }

        /// <summary>
        /// Called by <see cref="TreeNode{T}.ToString"/> to append node parameters to the output
        /// </summary>
        /// <param name="s">The output to append</param>
        protected override void ToParamString(StringBuilder s)
        {
            base.ToParamString(s);
            if (PropertyType != null)
                s.AppendFormat(", type={0}", PropertyType);
            s.AppendFormat(", {0}", DebugUtil.FormatOption("lazy", IsLazy));
        }
    }
}
