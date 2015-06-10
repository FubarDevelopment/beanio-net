using System.Collections.Generic;

namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// A <see cref="ISelector"/> is used to match a <see cref="Group"/> or <see cref="Record"/> for
    /// marshalling and unmarshalling.
    /// </summary>
    public interface ISelector : IParser
    {
        /// <summary>
        /// Gets the minimum number of occurrences of this component (within the context of its parent).
        /// </summary>
        int MinOccurs { get; }

        /// <summary>
        /// Gets the maximum number of occurrences of this component (within the context of its parent).
        /// </summary>
        int? MaxOccurs { get; }

        /// <summary>
        /// Gets the order of this component (within the context of its parent).
        /// </summary>
        int Order { get; }

        /// <summary>
        /// Gets the <see cref="IProperty"/> mapped to this component, or null if there is no property mapping.
        /// </summary>
        IProperty Property { get; }

        /// <summary>
        /// Gets a value indicating whether this component is a record group.
        /// </summary>
        bool IsRecordGroup { get; }

        /// <summary>
        /// Returns the number of times this component was matched within the current
        /// iteration of its parent component.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <returns>the match count</returns>
        int GetCount(ParsingContext context);

        /// <summary>
        /// Sets the number of times this component was matched within the current
        /// iteration of its parent component.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <param name="value">the new count</param>
        void SetCount(ParsingContext context, int value);

        /// <summary>
        /// Returns a value indicating whether this component has reached its maximum occurrences
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <returns>true if maximum occurrences has been reached</returns>
        bool IsMaxOccursReached(ParsingContext context);

        /// <summary>
        /// Finds a parser for marshalling a bean object
        /// </summary>
        /// <remarks>
        /// If matched by this Selector, the method
        /// should set the bean object on the property tree and return itself.
        /// </remarks>
        /// <param name="context">the <see cref="MarshallingContext"/></param>
        /// <returns>the matched <see cref="ISelector"/> for marshalling the bean object</returns>
        ISelector MatchNext(MarshallingContext context);

        /// <summary>
        /// Finds a parser for unmarshalling a record based on the current state of the stream.
        /// </summary>
        /// <param name="context">the <see cref="UnmarshallingContext"/></param>
        /// <returns>the matched <see cref="ISelector"/> for unmarshalling the record</returns>
        ISelector MatchNext(UnmarshallingContext context);

        /// <summary>
        /// Finds a parser that matches the input record
        /// </summary>
        /// <remarks>
        /// This method is invoked when <see cref="MatchNext(UnmarshallingContext)"/> returns
        /// null, in order to differentiate between unexpected and unidentified record types.
        /// </remarks>
        /// <param name="context">the <see cref="UnmarshallingContext"/></param>
        /// <returns>the matched <see cref="ISelector"/></returns>
        ISelector MatchAny(UnmarshallingContext context);

        /// <summary>
        /// Skips a record or group of records.
        /// </summary>
        /// <param name="context">the <see cref="UnmarshallingContext"/></param>
        void Skip(UnmarshallingContext context);

        /// <summary>
        /// Checks for any unsatisfied components before the stream is closed.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <returns>the first unsatisfied node</returns>
        ISelector Close(ParsingContext context);

        /// <summary>
        /// Resets the component count of this Selector's children.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        void Reset(ParsingContext context);

        /// <summary>
        /// Updates a <see cref="IDictionary{TKey,TValue}"/> with the current state of the Writer to allow for restoration at a later time.
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <param name="ns">a <see cref="string"/> to prefix all state keys with</param>
        /// <param name="state">the <see cref="IDictionary{TKey,TValue}"/> to update with the latest state</param>
        void UpdateState(ParsingContext context, string ns, IDictionary<string, object> state);

        /// <summary>
        /// Restores a <see cref="IDictionary{TKey,TValue}"/> of previously stored state information
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/></param>
        /// <param name="ns">a <see cref="string"/> to prefix all state keys with</param>
        /// <param name="state">the <see cref="IDictionary{TKey,TValue}"/> containing the state to restore</param>
        void RestoreState(ParsingContext context, string ns, IReadOnlyDictionary<string, object> state);
    }
}
