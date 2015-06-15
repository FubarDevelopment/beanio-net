using System.Collections.Generic;

namespace BeanIO.Parser.Lazy
{
    public class Account
    {
        public int? Number { get; set; }

        public string Text { get; set; }

        public List<Transaction> Transactions { get; set; }
    }
}
