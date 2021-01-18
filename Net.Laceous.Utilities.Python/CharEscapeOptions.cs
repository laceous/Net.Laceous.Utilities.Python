namespace Net.Laceous.Utilities.Python
{
    public class CharEscapeOptions
    {
        public CharEscapeLetter EscapeLetter { get; set; }

        public CharEscapeLetter EscapeLetterFallback { get; set; }

        public CharEscapeLetter SurrogatePairEscapeLetter { get; set; }

        public CharEscapeLetter SurrogatePairEscapeLetterFallback { get; set; }

        public bool UseLowerCaseHex { get; set; }

        public bool UseShortEscape { get; set; }

        public CharEscapeOptions(CharEscapeLetter escapeLetter = CharEscapeLetter.LowerCaseU4,
            CharEscapeLetter escapeLetterFallback = CharEscapeLetter.LowerCaseU4, CharEscapeLetter surrogatePairEscapeLetter = CharEscapeLetter.UpperCaseU8,
            CharEscapeLetter surrogatePairEscapeLetterFallback = CharEscapeLetter.UpperCaseU8, bool useLowerCaseHex = false, bool useShortEscape = false)
        {
            EscapeLetter = escapeLetter;
            EscapeLetterFallback = escapeLetterFallback;
            SurrogatePairEscapeLetter = surrogatePairEscapeLetter;
            SurrogatePairEscapeLetterFallback = surrogatePairEscapeLetterFallback;
            UseLowerCaseHex = useLowerCaseHex;
            UseShortEscape = useShortEscape;
        }
    }
}
