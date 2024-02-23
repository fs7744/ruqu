namespace RuQu
{
    public class IniConfig : Dictionary<string, IniSection>
    {
        public IniConfig() : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}