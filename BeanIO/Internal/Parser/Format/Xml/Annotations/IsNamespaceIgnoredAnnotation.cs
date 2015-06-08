namespace BeanIO.Internal.Parser.Format.Xml.Annotations
{
    internal class IsNamespaceIgnoredAnnotation
    {
        public IsNamespaceIgnoredAnnotation(bool value)
        {
            IsNamespaceIgnored = value;
        }

        public bool IsNamespaceIgnored { get; private set; }
    }
}
