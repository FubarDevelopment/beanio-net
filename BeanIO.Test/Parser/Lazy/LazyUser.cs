using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BeanIO.Parser.Lazy
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class LazyUser
    {
        public string name;

        public Account account;

        public List<Account> accounts;
    }
}
