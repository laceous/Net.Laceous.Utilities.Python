# Net.Laceous.Utilities.Python

This code was removed from [Net.Laceous.Utilities](https://github.com/laceous/Net.Laceous.Utilities). Re-adding here for posterity.

Trying to keep [NetUnicodeInfo](https://github.com/GoldenCrystal/NetUnicodeInfo) and [unicodedata](https://github.com/python/cpython/blob/master/Modules/unicodedata.c) in-sync feels like it could be a pain over time.

## Char and string escaping

### Python

```python
import sys
import clr
# path to Net.Laceous.Utilities.Python.dll and UnicodeInformation.dll
sys.path.append('/path/to/Net.Laceous.Utilities.Python')
clr.AddReference('Net.Laceous.Utilities.Python')
from Net.Laceous.Utilities.Python import *

# work around the following error:
# UnicodeEncodeError: 'utf-8' codec can't encode characters in position x-x: surrogates not allowed
def sp(s):
    return s.encode('utf-16', 'surrogatepass').decode('utf-16', 'replace')

ceOptions = CharEscapeOptions(escapeLetter = CharEscapeLetter.LowerCaseU4, escapeLetterFallback = CharEscapeLetter.LowerCaseU4, surrogatePairEscapeLetter = CharEscapeLetter.UpperCaseN1, surrogatePairEscapeLetterFallback = CharEscapeLetter.UpperCaseU8, useLowerCaseHex = False, useShortEscape = True)
seOptions = StringEscapeOptions(escapeKind = StringEscapeKind.EscapeNonAscii, escapeSurrogatePairs = True)
uOptions = StringUnescapeOptions(isUnrecognizedEscapeVerbatim = True, quoteKind = StringQuoteKind.DoubleQuote)

sOriginal = "abc ABC 123 ÄÖÜ ㄱㄴㄷ 😁😃😓"
sEscaped = StringUtils.Escape(sOriginal, stringEscapeOptions = seOptions, charEscapeOptions = ceOptions)
sUnescaped = StringUtils.Unescape(sEscaped, unescapeOptions = uOptions)
print(f"\"{sEscaped}\"") # "abc ABC 123 \u00C4\u00D6\u00DC \u3131\u3134\u3137 \N{Grinning Face With Smiling Eyes}\N{Smiling Face With Open Mouth}\N{Face With Cold Sweat}"
print(sp(sUnescaped))    # abc ABC 123 ÄÖÜ ㄱㄴㄷ 😁😃😓

# two_char_emoji_1 = '\uD83D\uDE01' # 😁
# two_char_emoji_2 = '\U0001F601'   # 😁
# print(two_char_emoji_1)     # fail
# print(sp(two_char_emoji_1)) # success
# print(two_char_emoji_2)     # success
#
# single_surrogate = '\uD83D' # illegal
# print(single_surrogate)     # fail
# print(sp(single_surrogate)) # success as \uFFFD
#
# python -> dotnet : a single surrogate (e.g. \uD83D) will pass through as the replacement character (\uFFFD)
# python -> dotnet : a surrogate pair (e.g. \uD83D\uDE01 or \U0001F601) will pass through correctly
# dotnet -> python : a single surrogate (e.g. \uD83D) will pass through as the replacement character (\uFFFD)
# dotnet -> python : a surrogate pair will pass through individually (e.g. \uD83D\uDE01) so the sp function is needed to deal with it
```

The above was tested with Python 3.8 and [Python.NET](http://pythonnet.github.io/)

Supported [Python escape sequences](https://docs.python.org/3/reference/lexical_analysis.html#string-and-bytes-literals):
* `\\` (Backslash)
* `\'` (Single quote)
* `\"` (Double quote)
* `\a` (Bell)
* `\b` (Backspace)
* `\f` (Formfeed)
* `\n` (Linefeed)
* `\r` (Carriage return)
* `\t` (Horizontal tab)
* `\v` (Vertical tab)
* `\O` or `\OO` or `\OOO` (Variable length octal escape sequence; 0-777)
* `\uHHHH` (16-bit hex value)
* `\UHHHHHHHH` (32-bit hex value)
* `\N{name}` (Character named *name* in the Unicode database)
  * This requires [UnicodeInformation](https://www.nuget.org/packages/UnicodeInformation/)

Supported quote types:
* "String"
* 'String'
* """String"""
* '''String'''
