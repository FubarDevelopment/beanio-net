using System;

namespace BeanIO.Stream
{
    [Flags]
    public enum ElementNameConversionMode
    {
        Unchanged = 0,

        Decapitalize = 1,
        Capitalize = 2,
        AllLower = 3,
        AllUpper = 4,

        CasingMask = 15,

        RemoveUnderscore = 16,
    }
}
