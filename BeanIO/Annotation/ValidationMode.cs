namespace BeanIO.Annotation
{
    /// <summary>
    /// The default validation mode when marshalling fields.
    /// </summary>
    public enum ValidationMode
    {
        /// <summary>
        /// Copy the configuration from the parent element (or Settings)
        /// </summary>
        SameAsParent,

        /// <summary>
        /// Validate on marshal
        /// </summary>
        ValidateOnMarshal,

        /// <summary>
        /// Don't validate on marshal
        /// </summary>
        NoValidateOnMarshal,
    }
}
