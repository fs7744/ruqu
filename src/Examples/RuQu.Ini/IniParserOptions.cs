namespace RuQu
{
    public class IniParserOptions : IOptions<IniConfig>
    {
        internal IniConfig Config;
        internal IniSection Section;

        public int BufferSize { get; set; } = 4096;
        public IniConfig WriteObject { get => Config; }

        public IOptions<IniConfig> CloneWriteOptionsWithValue(IniConfig writeObject)
        {
            var clone = new IniParserOptions() { BufferSize = BufferSize, Config = writeObject };
            return clone;
        }

        IOptions<IniConfig> IOptions<IniConfig>.CloneReadOptions()
        {
            var clone = new IniParserOptions() { BufferSize = BufferSize, Config = new IniConfig() };
            return clone;
        }
    }
}