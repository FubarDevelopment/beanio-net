using BeanIO.Annotation;

namespace BeanIO.Parser.Annotation
{
    public class AnnotatedBulb
    {
        [Field(At = 0)]
        public int Watts { get; set; }

        [Field(At = 1)]
        public string Style { get; set; }
    }
}
