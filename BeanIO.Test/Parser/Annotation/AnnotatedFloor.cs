using BeanIO.Annotation;

namespace BeanIO.Parser.Annotation
{
    [UnboundField("floor", At = 0, Literal = "hardwood")]
    public class AnnotatedFloor
    {
        [Field(At = 1)]
        public int Width { get; set; }

        [Field(At = 2)]
        public int Height { get; set; }
    }
}
