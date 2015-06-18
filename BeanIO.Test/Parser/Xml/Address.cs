namespace BeanIO.Parser.Xml
{
    public class Address
    {
        public string City { get; set; }

        public string State { get; set; }

        public string Zip { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}]", City, State, Zip);
        }
    }
}
