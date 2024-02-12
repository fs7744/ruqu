namespace RuQu
{
    public static partial class Parser
    {
        public static Predicate<IInput<char>> Is(char c) => i => i.Current == c;

        public static Predicate<IInput<char>> Is(Func<char, bool> predicate) => i => predicate(i.Current);
    }
}