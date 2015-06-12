namespace BeanIO.Internal.Config
{
    /// <summary>
    /// This interface is implemented by configuration components used to select
    /// a record for marshalling or unmarshalling, namely <see cref="RecordConfig"/> and
    /// <see cref="GroupConfig"/> components.
    /// </summary>
    internal interface ISelectorConfig
    {
        /// <summary>
        /// Gets the component type of this selector.
        /// </summary>
        /// <returns>either <see cref="F:ComponentType.Record"/> or <see cref="F:ComponentType.Group"/></returns>
        ComponentType ComponentType { get; }

        /// <summary>
        /// Gets the name of this component.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the minimum occurrences of this component.
        /// </summary>
        int? MinOccurs { get; }

        /// <summary>
        /// Gets the maximum occurrences of this component.
        /// </summary>
        int? MaxOccurs { get; }

        /// <summary>
        /// Gets or sets the order of this component within the context of its parent group.
        /// </summary>
        /// <remarks>
        /// Records and groups assigned the same order number may appear in any order.
        /// </remarks>
        /// <returns>the component order (starting at 1)</returns>
        int? Order { get; set; }
    }
}
