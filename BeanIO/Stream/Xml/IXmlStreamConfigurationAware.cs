namespace BeanIO.Stream.Xml
{
    /// <summary>
    /// This callback interface can be implemented by <see cref="IRecordReader"/> implementations
    /// for XML formatted streams that wish to obtain configuration information from the
    /// XML stream definition.
    /// </summary>
    public interface IXmlStreamConfigurationAware
    {
        /// <summary>
        /// This method is invoked by a XML stream definition when a <see cref="IRecordReader"/>
        /// implementation is registered.
        /// </summary>
        /// <param name="configuration">the XML stream configuration</param>
        void Configure(IXmlStreamConfiguration configuration);
    }
}
