using System.Runtime.CompilerServices;
using System.Text;

namespace RuQu
{
    public class StringPeeker : IPeeker<char>
    {
        internal readonly string str;
        internal int index;

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return str.Length; }
        }

        public StringPeeker(string data)
        {
            this.str = data;
            this.index = 0;
        }

        public char this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return str[index];
            }
        }

        public bool TryPeekOffset(int offset, out char data)
        {
            var i = index + offset;
            if (i >= str.Length)
            {
                data = default;
                return false;
            }
            data = str[i];
            return true;
        }

        public bool TryPeekOffset(int offset, int count, out ReadOnlySpan<char> data)
        {
            var i = index + offset + count;
            if (i > Length)
            {
                data = null;
                return false;
            }
            data = str.AsSpan().Slice(index + offset, count);
            return true;
        }

        public void Read(int count)
        {
            index = Math.Min(index + count, Length);
        }
    }

    public static unsafe partial class Chars
    {
        public static StringPeeker AsCharPeeker(this string str) => new StringPeeker(str);

        public static StringPeeker AsCharPeeker(this byte[] bytes, Encoding encoding) => new StringPeeker(encoding.GetString(bytes));

        public static StringPeeker AsCharPeeker(this byte[] bytes) => new StringPeeker(Encoding.UTF8.GetString(bytes));

        public static bool TakeWhiteSpace(this IPeeker<char> peeker, out ReadOnlySpan<char> span)
        {
            return peeker.Take(&char.IsWhiteSpace, out span);
        }

        public static bool StartsWith(this StringPeeker peeker, ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            int pos = peeker.index;
            if (pos >= peeker.Length || pos + value.Length >= peeker.Length)
            {
                return false;
            }
            ReadOnlySpan<char> remaining = peeker.str.AsSpan()[pos..];
            if (remaining.StartsWith(value, comparisonType))
            {
                peeker.Read(value.Length);
                return true;
            }
            return false;
        }

        public static bool TakeLine(this StringPeeker peeker, out ReadOnlySpan<char> span)
        {
            int pos = peeker.index;
            if ((uint)pos >= (uint)peeker.Length)
            {
                span = default;
                return false;
            }

            ReadOnlySpan<char> remaining = peeker.str.AsSpan().Slice(pos);
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


        public static bool StartsWith(this StringPeeker peeker, ReadOnlySpan<char> value)
        {
            int pos = peeker.index;
            if (pos >= peeker.Length || pos + value.Length >= peeker.Length)
            {
                return false;
            }
            ReadOnlySpan<char> remaining = peeker.str.AsSpan()[pos..];
            if (remaining.StartsWith(value))
            {
                peeker.Read(value.Length);
                return true;
            }
            return false;
        }

        public static bool Take(this StringPeeker peeker, delegate*<ReadOnlySpan<char>, int> predicate, out ReadOnlySpan<char> span)
        {
            int pos = peeker.index;
            if ((uint)pos >= (uint)peeker.Length)
            {
                span = default;
                return false;
            }

            ReadOnlySpan<char> remaining = peeker.str.AsSpan()[pos..];
            int foundLineLength = predicate(remaining);
            if (foundLineLength >= 0)
            {
                span = remaining[0..foundLineLength];
                peeker.index = pos + foundLineLength + 1;
                return true;
            }
            else
            {
                span = default;
                return false;
            }
        }

        public static bool TakeRemaining(this StringPeeker peeker, out ReadOnlySpan<char> span)
        {
            int pos = peeker.index;
            if (pos >= peeker.Length)
            {
                span = default;
                return false;
            }

            span = peeker.str.AsSpan()[pos..];
            peeker.index = peeker.Length;
            return true;
        }
    }
}