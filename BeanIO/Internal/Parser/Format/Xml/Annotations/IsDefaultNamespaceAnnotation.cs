namespace BeanIO.Internal.Parser.Format.Xml.Annotations
{
    internal class IsDefaultNamespaceAnnotation
    {
        public IsDefaultNamespaceAnnotation(bool value)
        {
            IsDefaultNamespace = value;
        }

        public bool IsDefaultNamespace { get; private set; }
    }
}
