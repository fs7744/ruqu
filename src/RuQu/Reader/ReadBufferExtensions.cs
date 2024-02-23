using System.Diagnostics;

namespace RuQu.Reader
{
    public static class ReadBufferExtensions
    {
        public static ReadOnlySpan<byte> Utf8Bom => [0xEF, 0xBB, 0xBF];

        public static void IngoreUtf8Bom(this IReadBuffer<byte> buffer)
        {
            var remaining = buffer.Remaining;
            // Handle the UTF-8 BOM if present
            Debug.Assert(remaining.Length >= Utf8Bom.Length);
            if (remaining.StartsWith(Utf8Bom))
            {
                buffer.Offset(Utf8Bom.Length);
            }
        }

        public static int ReadLine(this IReadBuffer<char> buffer, out ReadOnlySpan<char> span)
        {
            ReadOnlySpan<char> remaining = buffer.Remaining;
            if (!remaining.IsEmpty)
            {
                int foundLineLength = remaining.IndexOfAny('\r', '\n');
                if (foundLineLength >= 0)
                {
                    span = remaining[0..foundLineLength];
                    char ch = remaining[foundLineLength];
                    var pos = foundLineLength + 1;
                    if (ch == '\r')
                    {
                        if ((uint)pos < (uint)remaining.Length && remaining[pos] == '\n')
                        {
                            pos++;
                        }
                    }
                    buffer.Offset(pos);
                    return pos;
                }
            }
            span = default;
            return 0;
        }
    }
}