using System.Collections.Generic;
using System.Xml;

using BeanIO.Annotation;

namespace BeanIO.Parser.Annotation
{
    public class AnnotatedLight
    {
        [Field(At = 0, XmlType = XmlNodeType.Attribute)]
        public int Quantity { get; set; }

        [Segment(At = 1, CollectionType = typeof(List<AnnotatedBulb>), MinOccurs = 2, MaxOccurs = 2)]
        public IList<AnnotatedBulb> Bulbs { get; set; }
    }
}
