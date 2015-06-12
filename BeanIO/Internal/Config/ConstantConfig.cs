namespace BeanIO.Internal.Config
{
    /// <summary>
    /// A constant component is used to add a property value to a bean object that is
    /// not bound to any field in a stream.
    /// </summary>
    /// <remarks>
    /// During marshalling, constants can be used to identify the record mapping for
    /// a bean object if <code>identifier</code> is set to true.
    /// </remarks>
    internal class ConstantConfig : SimplePropertyConfig
    {
        /// <summary>
        /// Gets the component type
        /// </summary>
        /// <returns>
        /// One of <see cref="F:ComponentType.Group"/>,
        /// <see cref="F:ComponentType.Record"/>, <see cref="F:ComponentType.Segment"/>
        /// <see cref="F:ComponentType.Field"/>, <see cref="F:ComponentType.Constant"/>,
        /// <see cref="F:ComponentType.Wrapper"/>, or <see cref="F:ComponentType.Stream"/>
        /// </returns>
        public override ComponentType ComponentType
        {
            get { return ComponentType.Constant; }
        }

        /// <summary>
        /// Gets or sets the textual representation of this fixed property value
        /// </summary>
        public string Value { get; set; }
    }
}
