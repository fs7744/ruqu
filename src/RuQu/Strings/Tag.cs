namespace RuQu
{
    public static partial class Tag
    {
        public static Tag<InputString> Char(char c) => i => i.Current == c ? 1 : 0;
    }
}