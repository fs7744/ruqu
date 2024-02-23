namespace RuQu
{
    public class IniParserOptions : IOptions
    {
        internal IniConfig Config;
        internal IniSection Section;

        public int BufferSize { get; set; } = 4096;

        public IOptions Clone()
        {
            var clone = new IniParserOptions() { Config = new IniConfig() };
            return clone;
        }
    }
}