namespace Net.Laceous.Utilities.Python.Extensions
{
    internal static class CharExtensions
    {
        internal static bool IsHex(this char c)
        {
            return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
        }

        internal static bool IsOctal(this char c)
        {
            return c >= '0' && c <= '7';
        }

        internal static bool IsZero(this char c)
        {
            return c == '0';
        }

        internal static bool IsBackslash(this char c)
        {
            return c == '\\';
        }

        internal static bool IsDoubleQuote(this char c)
        {
            return c == '\"';
        }

        internal static bool IsSingleQuote(this char c)
        {
            return c == '\'';
        }

        internal static bool IsLeftBrace(this char c)
        {
            return c == '{';
        }

        internal static bool IsRightBrace(this char c)
        {
            return c == '}';
        }

        internal static bool IsCarriageReturn(this char c)
        {
            return c == '\r';
        }

        internal static bool IsLineFeed(this char c)
        {
            return c == '\n';
        }

        internal static bool IsPrintAscii(this char c)
        {
            return c >= 32 && c <= 126; // space - tilde (~)
        }
    }
}
