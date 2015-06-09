namespace BeanIO.Internal.Parser.Format.Xml.Annotations
{
    internal class GroupCountAnnotation
    {
        public GroupCountAnnotation(int count)
        {
            Count = count;
        }

        public int Count { get; private set; }
    }
}
