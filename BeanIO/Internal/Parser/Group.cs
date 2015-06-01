using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BeanIO.Internal.Util;

namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// A Group holds child nodes including records and other groups
    /// </summary>
    /// <remarks>
    /// This class is the dynamic counterpart to the <see cref="GroupDefinition"/> and
    /// holds the current state of a group node during stream processing.
    /// </remarks>
    public class Group : ParserComponent, ISelector
    {
        /// <summary>
        /// map key used to store the state of the 'lastMatchedChild' attribute
        /// </summary>
        private const string LAST_MATCHED_KEY = "lastMatched";

        private readonly ParserLocal<int> _count = new ParserLocal<int>(0);

        private readonly ParserLocal<ISelector> _lastMatched = new ParserLocal<ISelector>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Group"/> class.
        /// </summary>
        public Group()
            : base(5)
        {
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
        public override int? Size
        {
            get { return null; }
        }

        /// <summary>
        /// Gets a value indicating whether this parser or any descendant of this parser is used to identify
        /// a record during unmarshalling.
        /// </summary>
        public override bool IsIdentifier
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether this node must exist during unmarshalling.
        /// </summary>
        public override bool IsOptional
        {
            get { return MinOccurs == 0; }
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
        /// Gets or sets the <see cref="IProperty"/> mapped to this component, or null if there is no property mapping.
        /// </summary>
        public IProperty Property { get; set; }

        /// <summary>
        /// Gets a value indicating whether this component is a record group.
        /// </summary>
        public bool IsRecordGroup
        {
            get { return true; }
        }

        /// <summary>
        /// Returns whether this parser and its children match a record being unmarshalled.
        /// </summary>
        /// <param name="context">The <see cref="UnmarshallingContext"/></param>
        /// <returns>true if matched, false otherwise</returns>
        public override bool Matches(UnmarshallingContext context)
        {
            return false;
        }

        /// <summary>
        /// Unmarshals a record
        /// </summary>
        /// <param name="context">The <see cref="UnmarshallingContext"/></param>
        /// <returns>true if this component was present in the unmarshalled record, or false otherwise</returns>
        public override bool Unmarshal(UnmarshallingContext context)
        {
            // this method is only invoked when this group is configured to
            // unmarshal a bean object that spans multiple records
            try
            {
                var child = _lastMatched.Get(context);
                child.Unmarshal(context);

                // read the next record
                while (true)
                {
                    context.NextRecord();

                    if (context.IsEof)
                    {
                        var unsatisfied = Close(context);
                        if (unsatisfied != null)
                            throw context.NewUnsatisfiedRecordException(unsatisfied.Name);
                        break;
                    }

                    child = MatchCurrent(context);
                    if (child == null)
                    {
                        Reset(context);
                        break;
                    }

                    try
                    {
                        child.Unmarshal(context);
                    }
                    catch (AbortRecordUnmarshalligException)
                    {
                        // Ignore aborts
                    }
                }

                if (Property != null)
                    Property.CreateValue(context);

                return true;
            }
            catch (UnsatisfiedNodeException ex)
            {
                throw context.NewUnsatisfiedRecordException(ex.Node.Name);
            }
        }

        /// <summary>
        /// Marshals a record
        /// </summary>
        /// <param name="context">The <see cref="MarshallingContext"/></param>
        /// <returns>whether a value was marshalled</returns>
        public override bool Marshal(MarshallingContext context)
        {
            // this method is only invoked when this group is configured to
            // marshal a bean object that spans multiple records
            return Children.Cast<IParser>().Aggregate(false, (current, child) => child.Marshal(context) || current);
        }

        /// <summary>
        /// Returns whether this parser or any of its descendant have content for marshalling.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <returns>true if there is content for marshalling, false otherwise</returns>
        public override bool HasContent(ParsingContext context)
        {
            if (Property != null)
                return !ReferenceEquals(Property.GetValue(context), Value.Missing);
            return Children.Cast<IParser>().Any(child => child.HasContent(context));
        }

        /// <summary>
        /// Clears the current property value.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        public override void ClearValue(ParsingContext context)
        {
            if (Property != null)
                Property.ClearValue(context);
        }

        /// <summary>
        /// Sets the property value for marshaling.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <param name="value">the property value</param>
        public override void SetValue(ParsingContext context, object value)
        {
            Property.SetValue(context, value);
        }

        /// <summary>
        /// Returns the unmarshalled property value.
        /// </summary>
        /// <param name="context">The <see cref="ParsingContext"/></param>
        /// <returns>the property value</returns>
        public override object GetValue(ParsingContext context)
        {
            return Property.GetValue(context);
        }

        /// <summary>
        /// Returns the number of times this component was matched within the current
        /// iteration of its parent component.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <returns>the match count</returns>
        public int GetCount(ParsingContext context)
        {
            return _count.Get(context);
        }

        /// <summary>
        /// Sets the number of times this component was matched within the current
        /// iteration of its parent component.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <param name="value">the new count</param>
        public void SetCount(ParsingContext context, int value)
        {
            _count.Set(context, value);
        }

        /// <summary>
        /// Returns a value indicating whether this component has reached its maximum occurrences
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <returns>true if maximum occurrences has been reached</returns>
        public bool IsMaxOccursReached(ParsingContext context)
        {
            return _lastMatched.Get(context) == null && MaxOccurs != null && GetCount(context) >= MaxOccurs;
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
        public ISelector MatchNext(MarshallingContext context)
        {
            try
            {
                if (Property == null)
                    return InternalMatchNext(context);

                var componentName = context.ComponentName;
                if (componentName != null && Name != componentName)
                    return null;

                var value = context.Bean;
                if (Property.Defines(value))
                {
                    Property.SetValue(context, value);
                    return this;
                }

                return null;
            }
            catch (UnsatisfiedNodeException ex)
            {
                throw new BeanWriterException(string.Format("Bean identification failed: expected record type '{0}'", ex.Node.Name), ex);
            }
        }

        /// <summary>
        /// Finds a parser for unmarshalling a record based on the current state of the stream.
        /// </summary>
        /// <param name="context">the <see cref="UnmarshallingContext"/></param>
        /// <returns>the matched <see cref="ISelector"/> for unmarshalling the record</returns>
        public ISelector MatchNext(UnmarshallingContext context)
        {
            try
            {
                return InternalMatchNext(context);
            }
            catch (UnsatisfiedNodeException ex)
            {
                throw context.NewUnsatisfiedRecordException(ex.Node.Name);
            }
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
        public ISelector MatchAny(UnmarshallingContext context)
        {
            return Children.Cast<ISelector>().Select(x => x.MatchAny(context)).FirstOrDefault(x => x != null);
        }

        /// <summary>
        /// Skips a record or group of records.
        /// </summary>
        /// <param name="context">the <see cref="UnmarshallingContext"/></param>
        public void Skip(UnmarshallingContext context)
        {
            // this method is only invoked when this group is configured to
            // unmarshal a bean object that spans multiple records
            try
            {
                var child = _lastMatched.Get(context);
                child.Skip(context);

                // read the next record
                while (true)
                {
                    context.NextRecord();
                    if (context.IsEof)
                    {
                        var unsatisfied = Close(context);
                        if (unsatisfied != null)
                            throw context.NewUnsatisfiedRecordException(unsatisfied.Name);
                        break;
                    }

                    // find the child unmarshaller for the record...
                    child = MatchCurrent(context);
                    if (child == null)
                    {
                        Reset(context);
                        break;
                    }

                    child.Skip(context);
                }
            }
            catch (UnsatisfiedNodeException ex)
            {
                throw context.NewUnsatisfiedRecordException(ex.Node.Name);
            }
        }

        /// <summary>
        /// Checks for any unsatisfied components before the stream is closed.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <returns>the first unsatisfied node</returns>
        public ISelector Close(ParsingContext context)
        {
            var lastMatch = _lastMatched.Get(context);

            if (lastMatch == null && MinOccurs == 0)
                return null;

            var pos = lastMatch == null ? 1 : lastMatch.Order;

            var unsatisfied = FindUnsatisfiedChild(context, pos);
            if (unsatisfied != null)
                return unsatisfied;

            if (GetCount(context) < MinOccurs)
            {
                // try to find a specific record before reporting any record from this group
                if (pos > 1)
                {
                    Reset(context);
                    unsatisfied = FindUnsatisfiedChild(context, 1);
                    if (unsatisfied != null)
                        return unsatisfied;
                }

                return this;
            }

            return null;
        }

        /// <summary>
        /// Resets the component count of this Selector's children.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        public void Reset(ParsingContext context)
        {
            _lastMatched.Set(context, null);
            foreach (var node in Children.Cast<ISelector>())
            {
                node.SetCount(context, 0);
                node.Reset(context);
            }
        }

        /// <summary>
        /// Updates a <see cref="IDictionary{TKey,TValue}"/> with the current state of the Writer to allow for restoration at a later time.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <param name="ns">a <see cref="string"/> to prefix all state keys with</param>
        /// <param name="state">the <see cref="IDictionary{TKey,TValue}"/> to update with the latest state</param>
        public void UpdateState(ParsingContext context, string ns, IDictionary<string, object> state)
        {
            state[GetKey(ns, COUNT_KEY)] = _count.Get(context);

            var lastMatchedChildName = string.Empty;
            var lastMatch = _lastMatched.Get(context);
            if (lastMatch != null)
                lastMatchedChildName = lastMatch.Name;

            state[GetKey(ns, LAST_MATCHED_KEY)] = lastMatchedChildName;

            // allow children to update their state
            foreach (var node in this.Cast<ISelector>())
                node.UpdateState(context, ns, state);
        }

        /// <summary>
        /// Restores a <see cref="IDictionary{TKey,TValue}"/> of previously stored state information
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <param name="ns">a <see cref="string"/> to prefix all state keys with</param>
        /// <param name="state">the <see cref="IDictionary{TKey,TValue}"/> containing the state to restore</param>
        public void RestoreState(ParsingContext context, string ns, IDictionary<string, object> state)
        {
            var key = GetKey(ns, COUNT_KEY);
            var n = (int?)state.Get(key);
            if (n == null)
                throw new InvalidOperationException(string.Format("Missing state information for key '{0}'", key));
            _count.Set(context, n.Value);

            // determine the last matched child
            key = GetKey(ns, LAST_MATCHED_KEY);
            var lastMatchedChildName = (string)state.Get(key);
            if (lastMatchedChildName == null)
                throw new InvalidOperationException(string.Format("Missing state information for key '{0}'", key));

            if (lastMatchedChildName.Length == 0)
            {
                _lastMatched.Set(context, null);
                lastMatchedChildName = null;
            }

            foreach (var child in Children.Cast<ISelector>())
            {
                if (lastMatchedChildName != null && lastMatchedChildName == child.Name)
                    _lastMatched.Set(context, child);
                child.RestoreState(context, ns, state);
            }
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
            if (Property != null)
                ((Component)Property).RegisterLocals(locals);

            if (locals.Add(_lastMatched))
            {
                locals.Add(_count);
                base.RegisterLocals(locals);
            }
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
        /// Called by <see cref="TreeNode{T}.ToString"/> to append node parameters to the output
        /// </summary>
        /// <param name="s">The output to append</param>
        protected override void ToParamString(StringBuilder s)
        {
            base.ToParamString(s);
            s
                .AppendFormat(", order={0}", Order)
                .AppendFormat(", occurs={0}", DebugUtil.FormatRange(MinOccurs, MaxOccurs));
            if (Property != null)
                s.AppendFormat(", property={0}", Property);
        }

        private ISelector FindUnsatisfiedChild(ParsingContext context, int from)
        {
            // find any unsatisfied child
            foreach (var node in Children.Cast<ISelector>())
            {
                if (node.Order < from)
                    continue;

                var unsatisfied = node.Close(context);
                if (unsatisfied != null)
                    return unsatisfied;
            }

            return null;
        }

        private ISelector InternalMatchNext(ParsingContext context)
        {
            /*
             * A matching record is searched for in 3 stages:
             * 1.  First, we give the last matching node an opportunity to match the next
             *     record if it hasn't reached it's max occurs.
             * 2.  Second, we search for another matching node at the same position/order
             *     or increment the position until we find a matching node or a min occurs
             *     is not met.
             * 3.  Finally, if all nodes in this group have been satisfied and this group
             *     hasn't reached its max occurs, we search nodes from the beginning again
             *     and increment the group count if a node matches.
             *
             * If no match is found, there SHOULD be no changes to the state of this node.
             */

            var match = MatchCurrent(context);
            if (match == null && (MaxOccurs == null || MaxOccurs > 1))
                match = MatchAgain(context);
            if (match != null)
                return Property != null ? this : match;
            return null;
        }

        private ISelector MatchAgain(ParsingContext context)
        {
            ISelector unsatisfied = null;

            if (_lastMatched.Get(context) != null)
            {
                // no need to check if the max occurs was already reached
                if (MaxOccurs != null && GetCount(context) >= MaxOccurs)
                    return null;

                // if there was no unsatisfied node and we haven't reached the max occurs,
                // try to find a match from the beginning again so that the parent can
                // skip this node
                var position = 1;
                foreach (var node in Children.Cast<ISelector>())
                {
                    if (node.Order > position)
                    {
                        if (unsatisfied != null)
                            return null;
                        position = node.Order;
                    }

                    if (node.MinOccurs > 0)
                    {
                        // when marshalling, allow records to be skipped that aren't bound to a property
                        if (context.Mode != ParsingMode.Marshalling || node.Property != null)
                            unsatisfied = node;
                    }

                    ISelector match = MatchNext(context, node);
                    if (match != null)
                    {
                        // this is different than reset() because we reset every node
                        // except the one that matched...
                        foreach (var sel in Children.Cast<ISelector>())
                        {
                            if (ReferenceEquals(sel, node))
                                continue;
                            sel.SetCount(context, 0);
                            sel.Reset(context);
                        }

                        _count.Set(context, _count.Get(context) + 1);
                        node.SetCount(context, 1);
                        _lastMatched.Set(context, node);

                        return match;
                    }
                }
            }

            return null;
        }

        private ISelector MatchCurrent(ParsingContext context)
        {
            ISelector match;
            ISelector unsatisfied = null;
            ISelector lastMatch = _lastMatched.Get(context);

            // check the last matching node - do not check records where the max occurs
            // has already been reached
            if (lastMatch != null && !lastMatch.IsMaxOccursReached(context))
            {
                match = MatchNext(context, lastMatch);
                if (match != null)
                    return match;
            }

            // set the current position to the order of the last matched node (or default to 1)
            var position = (lastMatch == null) ? 1 : lastMatch.Order;

            // iterate over each child
            foreach (var node in Children.Cast<ISelector>())
            {
                // skip the last node which was already checked
                if (node == lastMatch)
                    continue;

                // skip nodes where their order is less than the current position
                if (node.Order < position)
                    continue;

                // skip nodes where max occurs has already been met
                if (node.IsMaxOccursReached(context))
                    continue;

                // if no node matched at the current position, increment the position and test the next node
                if (node.Order > position)
                {
                    // before increasing the position, we must validate that all
                    // min occurs have been met at the previous position
                    if (unsatisfied != null)
                    {
                        if (lastMatch != null)
                            throw new UnsatisfiedNodeException(unsatisfied);
                        return null;
                    }

                    position = node.Order;
                }

                // if the min occurs has not been met for the next node, set the unsatisfied flag so we
                // can throw an exception before incrementing the position again
                if (node.GetCount(context) < node.MinOccurs)
                {
                    // when marshalling, allow records to be skipped that aren't bound to a property
                    if (context.Mode != ParsingMode.Marshalling || node.Property != null)
                    {
                        unsatisfied = node;
                    }
                }

                // search the child node for a match
                match = MatchNext(context, node);
                if (match != null)
                {
                    // the group count is incremented only when first invoked
                    if (lastMatch == null)
                    {
                        _count.Set(context, _count.Get(context) + 1);
                    }
                    else
                    {
                        // reset the last group when a new record or group is found
                        // at the same level (this has no effect for a record)
                        lastMatch.Reset(context);
                    }

                    _lastMatched.Set(context, node);
                    return match;
                }
            }

            // if last was not null, we continued checking for matches at the current position, now
            // we'll check for matches at the beginning (assuming there is no unsatisfied node)
            if (lastMatch != null && unsatisfied != null)
                throw new UnsatisfiedNodeException(unsatisfied);

            return null;
        }

        private ISelector MatchNext(ParsingContext context, ISelector child)
        {
            switch (context.Mode)
            {
                case ParsingMode.Marshalling:
                    return child.MatchNext((MarshallingContext)context);
                case ParsingMode.Unmarshalling:
                    return child.MatchNext((UnmarshallingContext)context);
                default:
                    throw new InvalidOperationException(string.Format("Invalid mode: {0}", context.Mode));
            }
        }

        private string GetKey(string ns, string name)
        {
            return string.Format("{0}.{1}.{2}", ns, Name, name);
        }

        private class UnsatisfiedNodeException : BeanIOException
        {
            public UnsatisfiedNodeException(ISelector node)
            {
                Node = node;
            }

            public ISelector Node { get; private set; }
        }
    }
}
