namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// The property type
    /// </summary>
    internal enum PropertyType
    {
        /// <summary>
        /// The simple property type that cannot hold other properties
        /// </summary>
        Simple,

        /// <summary>
        /// The bean object property type with simple properties and other bean objects for attributes
        /// </summary>
        Complex,

        /// <summary>
        /// The collection property type used to create a collection of other properties
        /// </summary>
        Collection,

        /// <summary>
        /// The map property type
        /// </summary>
        Map,

        /// <summary>
        /// The array property type
        /// </summary>
        AggregationArray,

        /// <summary>
        /// The collection property type used to aggregate multiple occurrences of a single property
        /// </summary>
        AggregationCollection,

        /// <summary>
        /// The map property type used to aggregate multiple occurrences of key/value pairs
        /// </summary>
        AggregationMap,
    }
}
