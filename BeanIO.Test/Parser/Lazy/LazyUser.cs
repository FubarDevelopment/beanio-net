using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BeanIO.Parser.Lazy
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Java beanio compatibility")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCopPlus.StyleCopPlusRules", "SP0100:AdvancedNamingRules", Justification = "Reviewed. Suppression is OK here.")]
    public class LazyUser
    {
        public string name;

        public Account account;

        public List<Account> accounts;
    }
}
