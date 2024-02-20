namespace RuQu
{
    public static unsafe partial class Chars
    {
        public static Peeker<char> AsPeeker(this string str) => new(str.AsSpan());

        public static bool TakeWhiteSpace(this ref Peeker<char> peeker, out ReadOnlySpan<char> span)
        {
            return peeker.Take(&char.IsWhiteSpace, out span);
        }

        public static bool TakeLine(this ref Peeker<char> peeker, out ReadOnlySpan<char> span)
        {
            int pos = peeker.index;
            if ((uint)pos >= (uint)peeker.Length)
            {
                span = default;
                return false;
            }

            ReadOnlySpan<char> remaining = peeker.span.Slice(pos);
            int foundLineLength = remaining.IndexOfAny('\r', '\n');
            if (foundLineLength >= 0)
            {
                span = remaining[0..foundLineLength];
                char ch = remaining[foundLineLength];
                pos += foundLineLength + 1;
                if (ch == '\r')
                {
                    if ((uint)pos < (uint)peeker.Length && peeker[pos] == '\n')
                    {
                        pos++;
                    }
                }
                peeker.index = pos;

                return true;
            }
            else
            {
                span = remaining;
                return true;
            }
        }
    }
}