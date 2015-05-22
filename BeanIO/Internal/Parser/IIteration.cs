namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// Repeating components must implement <see cref="IIteration"/> to offset record positions
    /// during marshalling and unmarshalling.
    /// </summary>
    public interface IIteration
    {
        /// <summary>
        /// Gets the size of the components that make up a single iteration.
        /// </summary>
        int IterationSize { get; }

        /// <summary>
        /// Gets a value indicating whether the iteration size is variable based on another field in the record.
        /// </summary>
        bool IsDynamicIteration { get; }

        /// <summary>
        /// Returns the index of the current iteration relative to its parent.
        /// </summary>
        /// <param name="context">The context of this iteration</param>
        /// <returns>the index of the current iteration</returns>
        int GetIterationIndex(ParsingContext context);
    }
}
