using System.Text;

namespace RuQu
{
    public static unsafe partial class Chars
    {
        public static Peeker<char> AsCharPeeker(this string str) => new(str);

        public static Peeker<char> AsCharPeeker(this byte[] bytes, Encoding encoding) => new(encoding.GetString(bytes));

        public static Peeker<char> AsCharPeeker(this byte[] bytes) => new(Encoding.UTF8.GetString(bytes));

        public static bool TakeWhiteSpace(this ref Peeker<char> peeker, out ReadOnlySpan<char> span)
        {
            return peeker.Take(&char.IsWhiteSpace, out span);
        }

        public static bool StartsWith(this ref Peeker<char> peeker, ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            int pos = peeker.index;
            if (pos >= peeker.Length || pos + value.Length >= peeker.Length)
            {
                return false;
            }
            ReadOnlySpan<char> remaining = peeker.span[pos..];
            if (remaining.StartsWith(value, comparisonType))
            {
                peeker.Read(value.Length);
                return true;
            }
            return false;
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