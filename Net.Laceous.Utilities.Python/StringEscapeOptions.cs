namespace Net.Laceous.Utilities.Python
{
    public class StringEscapeOptions
    {
        public StringEscapeKind EscapeKind { get; set; }

        public bool EscapeSurrogatePairs { get; set; }

        public StringEscapeOptions(StringEscapeKind escapeKind = StringEscapeKind.EscapeAll, bool escapeSurrogatePairs = false)
        {
            EscapeKind = escapeKind;
            EscapeSurrogatePairs = escapeSurrogatePairs;
        }
    }
}
