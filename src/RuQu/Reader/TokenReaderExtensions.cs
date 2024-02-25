namespace RuQu.Reader
{
    public class ParseException : Exception
    {
        public ParseException(string? message) : base(message)
        {
        }
    }

    public static class TokenReaderExtensions
    {
        public static T Tag<T>(this IReaderBuffer<T> buffer, T tag) where T : IEquatable<T>
        {
            if (buffer.IsEOF)
            {
                throw new ParseException($"Expect {tag} at {buffer.Index} but got eof");
            }

            var t = buffer.Peek(1);
            if (tag.Equals(t[0]))
            {
                buffer.Consume(1);
                return tag;
            }

            throw new ParseException($"Expect {tag} at {buffer.Index} but got {t[0]}");
        }

        public static ReadOnlySpan<char> AsciiHexDigit(this IReaderBuffer<char> buffer, int count)
        {
            if (buffer.IsEOF)
            {
                throw new ParseException($"Expect {count} AsciiHexDigit char at {buffer.Index} but got eof");
            }

            var t = buffer.Peek(count);
            for (int i = 0; i < count; i++)
            {
                var c = t[i];
                if (!char.IsAsciiHexDigit(c))
                {
                    throw new ParseException($"Expect AsciiHexDigit char at {buffer.Index + i} but got {c}");
                }
            }

            buffer.Consume(count);
            return t;
        }

        public static void Eof(this IReaderBuffer<char> buffer)
        {
            if (!buffer.IsEOF)
            {
                throw new ParseException($"Expect eof at {buffer.Index} but got {buffer.Peek(1)[0]}");
            }
        }
    }
}