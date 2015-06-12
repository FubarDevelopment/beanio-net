using System.Collections.Generic;

namespace BeanIO.Internal.Util
{
    /// <summary>
    /// <see cref="IStatefulWriter"/> can be implemented by writers that maintain state, so
    /// that in case of an error, the last updated state of the writer can be restored and
    /// writing can resume.
    /// </summary>
    internal interface IStatefulWriter
    {
        /// <summary>
        /// Updates a <see cref="IDictionary{TKey,TValue}"/> with the current state of the Writer to allow for
        /// restoration at a later time
        /// </summary>
        /// <param name="ns">a string to prefix all state keys with</param>
        /// <param name="state">the <see cref="IDictionary{TKey,TValue}"/> to update with the latest state</param>
        void UpdateState(string ns, IDictionary<string, object> state);

        /// <summary>
        /// Restores a <see cref="IReadOnlyDictionary{TKey,TValue}"/> of previously stored state information
        /// </summary>
        /// <param name="ns">a string to prefix all state keys with</param>
        /// <param name="state">the <see cref="IReadOnlyDictionary{TKey,TValue}"/> containing the state to restore</param>
        void RestoreState(string ns, IReadOnlyDictionary<string, object> state);
    }
}
