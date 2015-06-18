using System.Collections.Generic;

using BeanIO.Internal.Util;

namespace BeanIO.Parser.Xml
{
    public class Person
    {
        public string Type { get; set; }

        public string Gender { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public List<string> Color { get; set; }

        public Address Address { get; set; }

        public List<Address> AddressList { get; set; }

        public int Age { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1} {2}: {3} {4}", Gender, FirstName, LastName, Color.ToDebug(), AddressList.ToDebug());
        }
    }
}
