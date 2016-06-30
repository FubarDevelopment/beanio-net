using System.Collections.Generic;

using BeanIO.Beans;

namespace BeanIO.Parser.Collection
{
    /// <summary>
    /// Test bean used by <see cref="CollectionFieldParserTest"/>.
    /// </summary>
    public class CollectionBean
    {
        public List<string> List { get; set; }

        public ISet<char?> Set { get; set; }

        public int[] Array { get; set; }

        public List<Person> ObjectList { get; set; }
    }
}
