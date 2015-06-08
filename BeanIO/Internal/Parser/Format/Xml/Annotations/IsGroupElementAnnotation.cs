namespace BeanIO.Internal.Parser.Format.Xml.Annotations
{
    internal class IsGroupElementAnnotation
    {
        public IsGroupElementAnnotation(bool value)
        {
            IsGroupElement = value;
        }

        public bool IsGroupElement { get; private set; }
    }
}
