using BeanIO.Annotation;

namespace BeanIO.Parser.Annotation
{
    public abstract class AnnotatedUserSupport
    {
        [Field(At = 1, IsRequired = true)]
        public string FirstName { get; set; }

        [Field(At = 2, Getter = "GetSurname", Setter = "SetSurname")]
        public string LastName { get; set; }

        public string GetSurname()
        {
            return LastName;
        }

        public void SetSurname(string lastName)
        {
            LastName = lastName;
        }
    }
}
