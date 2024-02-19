namespace RuQu
{
    public static unsafe partial class Charss
    {
        public static Peeker<char> AsPeeker(this string str) => new(str.AsSpan());

        public static bool TakeWhiteSpace(this ref Peeker<char> peeker, out ReadOnlySpan<char> span)
        {
            return peeker.Take(&char.IsWhiteSpace, out span);
        }

        //private static bool IsCR(char c) => c == '\r';

        //private static bool IsLF(char c) => c == '\n';

        private static bool IsNotCROrLF(char c) => !(c == '\r' || c == '\n');

        public static bool TakeLine(this ref Peeker<char> peeker, out ReadOnlySpan<char> span)
        {
            if (peeker.Take(&IsNotCROrLF, out span))
            {
                if (peeker.TryPeek(out var c))
                {
                    if (c == '\r')
                    {
                        peeker.Read(1);
                        if (!peeker.TryPeek(out c))
                        {
                            return true;
                        }
                    }

                    if (c == '\n')
                    {
                        peeker.Read(1);
                    }
                }
                return true;
            }
            return false;
        }
    }
}