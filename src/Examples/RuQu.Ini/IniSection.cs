namespace RuQu
{
    public class IniSection : Dictionary<string, string>
    {
        public IniSection() : base(StringComparer.OrdinalIgnoreCase)
        {

        }
    }
}