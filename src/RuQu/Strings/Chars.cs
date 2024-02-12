namespace RuQu
{
    public static partial class Chars
    {

        public static readonly Predicate<IInput<char>> IsWhiteSpace = i => char.IsWhiteSpace(i.Current);

        public static readonly Predicate<IInput<char>> IngoreWhiteSpace = IsWhiteSpace.RepeatUntilNot().Ingore();


        public static readonly Predicate<IInput<char>> Any = i => true;
        public static Predicate<IInput<char>> In(char[] arrary) => i => arrary.Contains(i.Current);

        public static Predicate<IInput<char>> In(string str) => i => str.IndexOf(i.Current) > -1;

        public static Predicate<IInput<char>> Is(char c) => i => i.Current == c;

        public static Predicate<IInput<char>> Is(Func<char, bool> predicate) => i => predicate(i.Current);
    }
}