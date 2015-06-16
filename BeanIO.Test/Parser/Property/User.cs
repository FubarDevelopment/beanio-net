namespace BeanIO.Parser.Property
{
    public class User
    {
        private int _type;

        public string Name { get; set; }

        public new int GetType()
        {
            return _type;
        }

        public void SetType(int type)
        {
            _type = type;
        }
    }
}
