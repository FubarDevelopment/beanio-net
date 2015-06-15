namespace BeanIO.Parser.Constructor
{
    public class Color
    {
        private readonly string _name;

        private readonly int _r;

        private readonly int _g;

        private readonly int _b;

        public Color()
            : this("black", 0, 0, 0)
        {
        }

        public Color(string name, int r, int g, int b)
        {
            _name = name;
            _r = r;
            _g = g;
            _b = b;
        }

        public string Name
        {
            get { return _name; }
        }

        public int R
        {
            get { return _r; }
        }

        public int G
        {
            get { return _g; }
        }

        public int B
        {
            get { return _b; }
        }
    }
}
