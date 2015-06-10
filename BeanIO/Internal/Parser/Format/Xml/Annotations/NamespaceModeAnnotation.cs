namespace BeanIO.Internal.Parser.Format.Xml.Annotations
{
    internal class NamespaceModeAnnotation
    {
        public NamespaceModeAnnotation(NamespaceHandlingMode handlingMode)
        {
            HandlingMode = handlingMode;
        }

        public NamespaceHandlingMode HandlingMode { get; private set; }
    }
}
