using System.Collections.Generic;

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
    }
}
