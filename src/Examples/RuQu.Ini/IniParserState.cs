namespace RuQu
{
    public class IniParserState
    {
        public IDictionary<string, string> Dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public string SectionPix;
    }
}