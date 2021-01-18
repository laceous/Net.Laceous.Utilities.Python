namespace Net.Laceous.Utilities.Python.Extensions
{
    internal static class IntExtensions
    {
        internal static bool IsUnifiedIdeograph(this int i)
        {
            // https://github.com/python/cpython/blob/master/Modules/unicodedata.c
            return
                (0x3400  <= i && i <= 0x4DBF)  || // CJK Ideograph Extension A
                (0x4E00  <= i && i <= 0x9FFC)  || // CJK Ideograph
                (0x20000 <= i && i <= 0x2A6DD) || // CJK Ideograph Extension B
                (0x2A700 <= i && i <= 0x2B734) || // CJK Ideograph Extension C
                (0x2B740 <= i && i <= 0x2B81D) || // CJK Ideograph Extension D
                (0x2B820 <= i && i <= 0x2CEA1) || // CJK Ideograph Extension E
                (0x2CEB0 <= i && i <= 0x2EBE0) || // CJK Ideograph Extension F
                (0x30000 <= i && i <= 0x3134A);   // CJK Ideograph Extension G
        }

        internal static bool IsTangutIdeograph(this int i)
        {
            return
                (0x17000 <= i && i <= 0x187F7) || // Tangut Ideograph
                (0x18D00 <= i && i <= 0x18D08);   // Tangut Ideograph Supplement
        }
    }
}
