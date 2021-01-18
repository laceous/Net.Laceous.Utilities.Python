namespace Net.Laceous.Utilities.Python
{
    public class StringUnescapeOptions
    {
        public bool IsUnrecognizedEscapeVerbatim { get; set; }

        public StringQuoteKind QuoteKind { get; set; }

        public StringUnescapeOptions(bool isUnrecognizedEscapeVerbatim = false, StringQuoteKind quoteKind = StringQuoteKind.DoubleQuote)
        {
            IsUnrecognizedEscapeVerbatim = isUnrecognizedEscapeVerbatim;
            QuoteKind = quoteKind;
        }
    }
}
