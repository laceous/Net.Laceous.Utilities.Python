using Net.Laceous.Utilities.Python.Extensions;
using System;
using System.Globalization;
using System.Unicode;

namespace Net.Laceous.Utilities.Python
{
    // make this internal because chars are encapsulated in strings in python
    internal static class CharUtils
    {
        internal static string Escape(char c, CharEscapeOptions escapeOptions = null)
        {
            if (escapeOptions == null)
            {
                escapeOptions = new CharEscapeOptions();
            }

            if (escapeOptions.UseShortEscape)
            {
                switch (c)
                {
                    case '\\':
                        return "\\\\";
                    case '\'':
                        return "\\\'";
                    case '\"':
                        return "\\\"";
                    case '\a':
                        return "\\a";
                    case '\b':
                        return "\\b";
                    case '\f':
                        return "\\f";
                    case '\n':
                        return "\\n";
                    case '\r':
                        return "\\r";
                    case '\t':
                        return "\\t";
                    case '\v':
                        return "\\v";
                    case '\0':
                        return "\\0"; // this is an octal escape in Python, add it here for parity with other languages
                }
            }

            string xu = null;
            string suffix = null;
            switch (escapeOptions.EscapeLetter)
            {
                case CharEscapeLetter.None1:
                    if (c <= 0x1FF) // 777 (max 3 char octal), 511 (decimal)
                    {
                        xu = "";
                        suffix = Convert.ToString((int)c, 8); // convert to octal
                    }
                    break;
                case CharEscapeLetter.None2:
                    if (c <= 0x1FF)
                    {
                        xu = "";
                        suffix = Convert.ToString((int)c, 8).PadLeft(2, '0');
                    }
                    break;
                case CharEscapeLetter.None3:
                    if (c <= 0x1FF)
                    {
                        xu = "";
                        suffix = Convert.ToString((int)c, 8).PadLeft(3, '0');
                    }
                    break;
                case CharEscapeLetter.LowerCaseX2:
                    if (c <= 0xFF) // 255 (decimal)
                    {
                        xu = "x";
                        suffix = ((int)c).ToString(escapeOptions.UseLowerCaseHex ? "x2" : "X2");
                    }
                    break;
                case CharEscapeLetter.LowerCaseU4:
                    xu = "u";
                    suffix = ((int)c).ToString(escapeOptions.UseLowerCaseHex ? "x4" : "X4");
                    break;
                case CharEscapeLetter.UpperCaseU8:
                    xu = "U";
                    suffix = ((int)c).ToString(escapeOptions.UseLowerCaseHex ? "x8" : "X8");
                    break;
                case CharEscapeLetter.UpperCaseN1:
                    string name = GetNameFromCodePoint((int)c);
                    if (!string.IsNullOrEmpty(name)) // StringComparison.Ordinal
                    {
                        xu = "N";
                        suffix = "{" + CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name.ToLowerInvariant()) + "}";
                    }
                    break;
                default:
                    throw new ArgumentException(string.Format("{0} is not a valid {1}.", escapeOptions.EscapeLetter, nameof(escapeOptions.EscapeLetter)), nameof(escapeOptions));
            }

            if (xu == null || suffix == null)
            {
                switch (escapeOptions.EscapeLetterFallback)
                {
                    case CharEscapeLetter.LowerCaseU4:
                        xu = "u";
                        suffix = ((int)c).ToString(escapeOptions.UseLowerCaseHex ? "x4" : "X4");
                        break;
                    case CharEscapeLetter.UpperCaseU8:
                        xu = "U";
                        suffix = ((int)c).ToString(escapeOptions.UseLowerCaseHex ? "x8" : "X8");
                        break;
                    default:
                        throw new ArgumentException(string.Format("{0} is not a valid {1}.", escapeOptions.EscapeLetterFallback, nameof(escapeOptions.EscapeLetterFallback)), nameof(escapeOptions));
                }
            }

            return "\\" + xu + suffix;
        }

        private static string GetNameFromCodePoint(int codePoint)
        {
            // compare the following for any discrepancies:
            //  * https://github.com/GoldenCrystal/NetUnicodeInfo
            //  * https://github.com/python/cpython/blob/master/Modules/unicodedata.c
            //  * http://www.unicode.org/Public/UCD/latest/ (pay attention to version - currently 13.0.0)
            // python's and NetUnicodeInfo's hangul rules appear to line up
            if (codePoint.IsUnifiedIdeograph())
            {
                // python explicitly defines these
                // NetUnicodeInfo returns these differently (Extension A vs B, etc)
                return "CJK UNIFIED IDEOGRAPH-" + codePoint.ToString("X");
            }
            else
            {
                UnicodeCharInfo charInfo = UnicodeInfo.GetCharInfo(codePoint);
                switch (charInfo.Category)
                {
                    // these categories don't technically have names
                    // NetUnicodeInfo returns/generates a name, however, while python does not
                    // aliases could still come into play here
                    case UnicodeCategory.Control:          // Cc
                    case UnicodeCategory.PrivateUse:       // Co
                    case UnicodeCategory.Surrogate:        // Cs
                    case UnicodeCategory.OtherNotAssigned: // Cn
                        return charInfo.NameAliases.Count > 0 ? charInfo.NameAliases[0].Name : null;
                }
                if (codePoint.IsTangutIdeograph())
                {
                    // these don't technically have names either (found by looking at the ucd data)
                    return charInfo.NameAliases.Count > 0 ? charInfo.NameAliases[0].Name : null;
                }
                // python 3.3 added support for aliases, fall back to that if there's no name
                return !string.IsNullOrEmpty(charInfo.Name) ? charInfo.Name : charInfo.NameAliases.Count > 0 ? charInfo.NameAliases[0].Name : null;
            }
        }

        internal static string EscapeSurrogatePair(char highSurrogate, char lowSurrogate, CharEscapeOptions escapeOptions = null)
        {
            if (escapeOptions == null)
            {
                escapeOptions = new CharEscapeOptions();
            }

            int codePoint = char.ConvertToUtf32(highSurrogate, lowSurrogate);

            string xu = null;
            string suffix = null;
            switch (escapeOptions.SurrogatePairEscapeLetter)
            {
                case CharEscapeLetter.UpperCaseU8:
                    xu = "U";
                    suffix = codePoint.ToString(escapeOptions.UseLowerCaseHex ? "x8" : "X8");
                    break;
                case CharEscapeLetter.UpperCaseN1:
                    string name = GetNameFromCodePoint(codePoint);
                    if (!string.IsNullOrEmpty(name))
                    {
                        xu = "N";
                        suffix = "{" + CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name.ToLowerInvariant()) + "}";
                    }
                    break;
                default:
                    throw new ArgumentException(string.Format("{0} is not a valid {1}.", escapeOptions.SurrogatePairEscapeLetter, nameof(escapeOptions.SurrogatePairEscapeLetter)), nameof(escapeOptions));
            }

            if (xu == null || suffix == null)
            {
                switch (escapeOptions.SurrogatePairEscapeLetterFallback)
                {
                    case CharEscapeLetter.UpperCaseU8:
                        xu = "U";
                        suffix = codePoint.ToString(escapeOptions.UseLowerCaseHex ? "x8" : "X8");
                        break;
                    default:
                        throw new ArgumentException(string.Format("{0} is not a valid {1}.", escapeOptions.SurrogatePairEscapeLetterFallback, nameof(escapeOptions.SurrogatePairEscapeLetterFallback)), nameof(escapeOptions));
                }
            }

            return "\\" + xu + suffix;
        }
    }
}
