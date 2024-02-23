namespace RuQu
{
    public class IniParserOptions : IOptions
    {
        internal IDictionary<string, string> Dict;
        internal string SectionPix;

        public int BufferSize { get; set; } = 4096;

        public IOptions Clone()
        {
            var clone = new IniParserOptions() { Dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) };
            return clone;
        }
    }
}