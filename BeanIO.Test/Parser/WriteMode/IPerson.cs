using NodaTime;

namespace BeanIO.Parser.WriteMode
{
    public interface IPerson
    {
        string FirstName { get; }

        string LastName { get; }

        int Age { get; }

        LocalDate BirthDate { get; }
    }
}
