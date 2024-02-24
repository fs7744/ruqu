namespace RuQu
{
    public struct IniParserReadState
    {
        public IniConfig Config;
        public IniSection Section;

        public IniParserReadState()
        {
            Config = new IniConfig();
        }
    }
}