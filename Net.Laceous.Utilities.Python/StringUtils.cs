using Net.Laceous.Utilities.Python.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Unicode;

namespace Net.Laceous.Utilities.Python
{
    public static class StringUtils
    {
        public static string Escape(string s, StringEscapeOptions stringEscapeOptions = null, CharEscapeOptions charEscapeOptions = null)
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }
            if (stringEscapeOptions == null)
            {
                stringEscapeOptions = new StringEscapeOptions();
            }
            if (charEscapeOptions == null)
            {
                charEscapeOptions = new CharEscapeOptions();
            }

            CharEscapeOptions charEscapeOptionsNone3 = new CharEscapeOptions(
                escapeLetter: CharEscapeLetter.None3,
                escapeLetterFallback: charEscapeOptions.EscapeLetterFallback,
                surrogatePairEscapeLetter: charEscapeOptions.SurrogatePairEscapeLetter,
                surrogatePairEscapeLetterFallback: charEscapeOptions.SurrogatePairEscapeLetterFallback,
                useLowerCaseHex: charEscapeOptions.UseLowerCaseHex,
                useShortEscape: charEscapeOptions.UseShortEscape
            );

            StringBuilder sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                if (stringEscapeOptions.EscapeSurrogatePairs && i + 1 < s.Length && char.IsSurrogatePair(s[i], s[i + 1]))
                {
                    sb.Append(CharUtils.EscapeSurrogatePair(s[i], s[++i], charEscapeOptions));
                }
                else
                {
                    switch (stringEscapeOptions.EscapeKind)
                    {
                        case StringEscapeKind.EscapeAll:
                            sb.Append(CharUtils.Escape(s[i], charEscapeOptions));
                            break;
                        case StringEscapeKind.EscapeNonAscii:
                            // go simple for now and escape all the quote chars
                            // that way you can copy the output and put it within any python string quote type: "s", 's', """s""", '''s'''
                            if (s[i].IsPrintAscii() && !s[i].IsBackslash() && !s[i].IsDoubleQuote() && !s[i].IsSingleQuote())
                            {
                                sb.Append(s[i]);
                            }
                            else
                            {
                                // workaround for variable length: \o, \oo, \ooo
                                if ((charEscapeOptions.EscapeLetter == CharEscapeLetter.None1 || charEscapeOptions.EscapeLetter == CharEscapeLetter.None2) && i + 1 < s.Length && s[i + 1].IsOctal())
                                {
                                    sb.Append(CharUtils.Escape(s[i], charEscapeOptionsNone3));
                                }
                                else
                                {
                                    sb.Append(CharUtils.Escape(s[i], charEscapeOptions));
                                }
                            }
                            break;
                        default:
                            throw new ArgumentException(string.Format("{0} is not a valid {1}.", stringEscapeOptions.EscapeKind, nameof(stringEscapeOptions.EscapeKind)), nameof(stringEscapeOptions));
                    }
                }
            }
            return sb.ToString();
        }

        public static string Unescape(string s, StringUnescapeOptions unescapeOptions)
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }
            if (unescapeOptions == null)
            {
                unescapeOptions = new StringUnescapeOptions();
            }

            if (s.IndexOfAny(new char[] { '\\', '\"', '\'', '\r', '\n' }) == -1)
            {
                return s;
            }
            else
            {
                StringBuilder sb = new StringBuilder(s.Length);
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i].IsBackslash())
                    {
                        if (i + 1 < s.Length)
                        {
                            switch (s[++i])
                            {
                                case '\\':
                                    sb.Append('\\');
                                    break;
                                case '\'':
                                    sb.Append('\'');
                                    break;
                                case '\"':
                                    sb.Append('\"');
                                    break;
                                case 'a':
                                    sb.Append('\a');
                                    break;
                                case 'b':
                                    sb.Append('\b');
                                    break;
                                case 'f':
                                    sb.Append('\f');
                                    break;
                                case 'n':
                                    sb.Append('\n');
                                    break;
                                case 'r':
                                    sb.Append('\r');
                                    break;
                                case 't':
                                    sb.Append('\t');
                                    break;
                                case 'v':
                                    sb.Append('\v');
                                    break;
                                case '0':
                                    sb.Append('\0');
                                    break;
                                case 'u':
                                    if (i + 4 < s.Length && s[i + 1].IsHex() && s[i + 2].IsHex() && s[i + 3].IsHex() && s[i + 4].IsHex())
                                    {
                                        sb.Append((char)int.Parse(new string(new char[] { s[++i], s[++i], s[++i], s[++i] }), NumberStyles.AllowHexSpecifier));
                                    }
                                    else
                                    {
                                        UnrecognizedEscapeOrAppend(sb, unescapeOptions.IsUnrecognizedEscapeVerbatim, nameof(s), new char[] { '\\', s[i] });
                                    }
                                    break;
                                case 'x':
                                    if (i + 2 < s.Length && s[i + 1].IsHex() && s[i + 2].IsHex())
                                    {
                                        sb.Append((char)int.Parse(new string(new char[] { s[++i], s[++i] }), NumberStyles.AllowHexSpecifier));
                                    }
                                    else
                                    {
                                        UnrecognizedEscapeOrAppend(sb, unescapeOptions.IsUnrecognizedEscapeVerbatim, nameof(s), new char[] { '\\', s[i] });
                                    }
                                    break;
                                case 'U':
                                    if (i + 8 < s.Length && s[i + 1].IsHex() && s[i + 2].IsHex() && s[i + 3].IsHex() && s[i + 4].IsHex() && s[i + 5].IsHex() && s[i + 6].IsHex() && s[i + 7].IsHex() && s[i + 8].IsHex())
                                    {
                                        if (s[i + 1].IsZero() && s[i + 2].IsZero() && s[i + 3].IsZero() && s[i + 4].IsZero())
                                        {
                                            i += 4;
                                            sb.Append((char)int.Parse(new string(new char[] { s[++i], s[++i], s[++i], s[++i] }), NumberStyles.AllowHexSpecifier));
                                        }
                                        else
                                        {
                                            string temp = ConvertFromUtf32(int.Parse(new string(new char[] { s[i + 1], s[i + 2], s[i + 3], s[i + 4], s[i + 5], s[i + 6], s[i + 7], s[i + 8] }), NumberStyles.AllowHexSpecifier), unescapeOptions.IsUnrecognizedEscapeVerbatim);
                                            if (temp == null)
                                            {
                                                sb.Append('\\');
                                                sb.Append(s[i]);
                                            }
                                            else
                                            {
                                                i += 8;
                                                sb.Append(temp);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        UnrecognizedEscapeOrAppend(sb, unescapeOptions.IsUnrecognizedEscapeVerbatim, nameof(s), new char[] { '\\', s[i] });
                                    }
                                    break;
                                case 'N':
                                    {
                                        // \N{Latin Capital Letter A} -> A
                                        string temp = FindStringFromBracedName(s, ref i, unescapeOptions.IsUnrecognizedEscapeVerbatim);
                                        if (temp == null)
                                        {
                                            UnrecognizedEscapeOrAppend(sb, unescapeOptions.IsUnrecognizedEscapeVerbatim, nameof(s), new char[] { '\\', s[i] });
                                        }
                                        else
                                        {
                                            sb.Append(temp);
                                        }
                                    }
                                    break;
                                default:
                                    // "" can support up to \777 unlike b"" which can only support up to \377 before rolling over
                                    if (i + 2 < s.Length && s[i].IsOctal() && s[i + 1].IsOctal() && s[i + 2].IsOctal())
                                    {
                                        sb.Append((char)Convert.ToInt32(new string(new char[] { s[i], s[++i], s[++i] }), 8));
                                    }
                                    else if (i + 1 < s.Length && s[i].IsOctal() && s[i + 1].IsOctal())
                                    {
                                        sb.Append((char)Convert.ToInt32(new string(new char[] { s[i], s[++i] }), 8));
                                    }
                                    else if (s[i].IsOctal())
                                    {
                                        sb.Append((char)Convert.ToInt32(new string(new char[] { s[i] }), 8));
                                    }
                                    else
                                    {
                                        // python treats this as verbatim
                                        sb.Append('\\');
                                        sb.Append(s[i]);
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            UnrecognizedEscapeOrAppend(sb, unescapeOptions.IsUnrecognizedEscapeVerbatim, nameof(s), new char[] { s[i] });
                        }
                    }
                    else if (s[i].IsDoubleQuote())
                    {
                        switch (unescapeOptions.QuoteKind)
                        {
                            case StringQuoteKind.DoubleQuote:
                                // " within ""
                                UnrecognizedEscapeOrAppend(sb, unescapeOptions.IsUnrecognizedEscapeVerbatim, nameof(s), new char[] { s[i] });
                                break;
                            case StringQuoteKind.TripleDoubleQuote:
                                if (i + 1 == s.Length)
                                {
                                    // can't have unescaped " as the last char in the string
                                    UnrecognizedEscapeOrAppend(sb, unescapeOptions.IsUnrecognizedEscapeVerbatim, nameof(s), new char[] { s[i] });
                                }
                                else if (i + 2 < s.Length && s[i + 1].IsDoubleQuote() && s[i + 2].IsDoubleQuote())
                                {
                                    // can't have unescaped triple quotes within triple quotes
                                    UnrecognizedEscapeOrAppend(sb, unescapeOptions.IsUnrecognizedEscapeVerbatim, nameof(s), new char[] { s[i] });
                                }
                                else
                                {
                                    sb.Append(s[i]);
                                }
                                break;
                            case StringQuoteKind.SingleQuote:
                            case StringQuoteKind.TripleSingleQuote:
                                sb.Append(s[i]);
                                break;
                            default:
                                throw new ArgumentException(string.Format("{0} is not a valid {1}.", unescapeOptions.QuoteKind, nameof(unescapeOptions.QuoteKind)), nameof(unescapeOptions));
                        }
                    }
                    else if (s[i].IsSingleQuote())
                    {
                        switch (unescapeOptions.QuoteKind)
                        {
                            case StringQuoteKind.SingleQuote:
                                // ' within ''
                                UnrecognizedEscapeOrAppend(sb, unescapeOptions.IsUnrecognizedEscapeVerbatim, nameof(s), new char[] { s[i] });
                                break;
                            case StringQuoteKind.TripleSingleQuote:
                                if (i + 1 == s.Length)
                                {
                                    // can't have unescaped ' as the last char in the string
                                    UnrecognizedEscapeOrAppend(sb, unescapeOptions.IsUnrecognizedEscapeVerbatim, nameof(s), new char[] { s[i] });
                                }
                                else if (i + 2 < s.Length && s[i + 1].IsSingleQuote() && s[i + 2].IsSingleQuote())
                                {
                                    // can't have unescaped triple quotes within triple quotes
                                    UnrecognizedEscapeOrAppend(sb, unescapeOptions.IsUnrecognizedEscapeVerbatim, nameof(s), new char[] { s[i] });
                                }
                                else
                                {
                                    sb.Append(s[i]);
                                }
                                break;
                            case StringQuoteKind.DoubleQuote:
                            case StringQuoteKind.TripleDoubleQuote:
                                sb.Append(s[i]);
                                break;
                            default:
                                throw new ArgumentException(string.Format("{0} is not a valid {1}.", unescapeOptions.QuoteKind, nameof(unescapeOptions.QuoteKind)), nameof(unescapeOptions));
                        }
                    }
                    else if (s[i].IsCarriageReturn() || s[i].IsLineFeed())
                    {
                        switch (unescapeOptions.QuoteKind)
                        {
                            case StringQuoteKind.DoubleQuote:
                            case StringQuoteKind.SingleQuote:
                                // unescaped newlines are not allowed
                                UnrecognizedEscapeOrAppend(sb, unescapeOptions.IsUnrecognizedEscapeVerbatim, nameof(s), new char[] { s[i] });
                                break;
                            case StringQuoteKind.TripleDoubleQuote:
                            case StringQuoteKind.TripleSingleQuote:
                                // unescaped newlines are allowed
                                sb.Append(s[i]);
                                break;
                            default:
                                throw new ArgumentException(string.Format("{0} is not a valid {1}.", unescapeOptions.QuoteKind, nameof(unescapeOptions.QuoteKind)), nameof(unescapeOptions));
                        }
                    }
                    else
                    {
                        sb.Append(s[i]);
                    }
                }
                return sb.ToString();
            }
        }

        private static void UnrecognizedEscapeOrAppend(StringBuilder sb, bool isUnrecognizedEscapeVerbatim, string paramName, char[] chars)
        {
            if (isUnrecognizedEscapeVerbatim)
            {
                foreach (char c in chars)
                {
                    sb.Append(c);
                }
            }
            else
            {
                throw new ArgumentException("Unrecognized escape sequence.", paramName);
            }
        }

        private static string ConvertFromUtf32(int utf32, bool isUnrecognizedEscapeVerbatim)
        {
            try
            {
                // System.ArgumentOutOfRangeException: A valid UTF32 value is between 0x000000 and 0x10ffff, inclusive, and should not include surrogate codepoint values (0x00d800 ~ 0x00dfff).
                return char.ConvertFromUtf32(utf32);
            }
            catch (ArgumentOutOfRangeException)
            {
                if (!isUnrecognizedEscapeVerbatim)
                {
                    throw;
                }
                return null;
            }
        }

        private static readonly Lazy<ReadOnlyDictionary<string, int>> LazyNameToCodePointDictionary = new Lazy<ReadOnlyDictionary<string, int>>(() =>
        {
            Dictionary<string, int> nameToCodePointDictionary = new Dictionary<string, int>();
            // for (int codePoint = 0; codePoint <= 0x10FFFF; codePoint++)
            foreach (UnicodeBlock block in UnicodeInfo.GetBlocks())
            {
                foreach (int codePoint in block.CodePointRange)
                {
                    // this should use the same rules as CharUtils.GetNameFromCodePoint
                    UnicodeCharInfo charInfo = UnicodeInfo.GetCharInfo(codePoint);
                    if (codePoint.IsUnifiedIdeograph())
                    {
                        nameToCodePointDictionary["CJK UNIFIED IDEOGRAPH-" + codePoint.ToString("X")] = codePoint;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(charInfo.Name) && charInfo.Category != UnicodeCategory.Control && charInfo.Category != UnicodeCategory.PrivateUse &&
                            charInfo.Category != UnicodeCategory.Surrogate && charInfo.Category != UnicodeCategory.OtherNotAssigned && !codePoint.IsTangutIdeograph())
                        {
                            nameToCodePointDictionary[charInfo.Name] = codePoint;
                        }
                    }
                    foreach (UnicodeNameAlias alias in charInfo.NameAliases)
                    {
                        if (!string.IsNullOrEmpty(alias.Name))
                        {
                            nameToCodePointDictionary[alias.Name] = codePoint;
                        }
                    }
                }
            }
            return new ReadOnlyDictionary<string, int>(nameToCodePointDictionary); // Count = 138182 with UnicodeInformation 2.5.0
        });

        private static string FindStringFromBracedName(string s, ref int i, bool isUnrecognizedEscapeVerbatim)
        {
            int j = 1;
            if (i + j < s.Length && s[i + j].IsLeftBrace())
            {
                bool foundRightBrace = false;
                StringBuilder sb = new StringBuilder();
                while (!foundRightBrace && i + j + 1 < s.Length)
                {
                    if (s[i + ++j].IsRightBrace())
                    {
                        foundRightBrace = true;
                    }
                    else
                    {
                        sb.Append(s[i + j]);
                    }
                }
                if (foundRightBrace)
                {
                    if (j >= 3) // {x}
                    {
                        if (LazyNameToCodePointDictionary.Value.TryGetValue(sb.ToString().ToUpperInvariant(), out int codePoint))
                        {
                            if (codePoint >= char.MinValue && codePoint <= char.MaxValue)
                            {
                                i += j;
                                return ((char)codePoint).ToString();
                            }
                            else
                            {
                                string temp = ConvertFromUtf32(codePoint, isUnrecognizedEscapeVerbatim);
                                if (temp != null)
                                {
                                    i += j;
                                    return temp;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
