using RuQu.Reader;
using System.Buffers;

namespace RuQu.Buffer
{
    public static class ChunkBufferExtensions
    {
        public static bool TryPeek<T>(this IChunkReader<T> buffer, out T t)
        {
            var chunk = buffer.GetCurrentChunk();
            if (chunk == null || chunk.Length == 0 || chunk.Index >= chunk.Length)
            {
                t = default;
                return false;
            }
            t = chunk[chunk.Index];
            return true;
        }

        public static bool TryPeek<T>(this IChunkReader<T> buffer, int count, out ReadOnlySequence<T> t)
        {
            var chunk = buffer.GetCurrentChunk();
            if (chunk == null || chunk.Length == 0)
            {
                t = default;
                return false;
            }
            var m = chunk.Memory;
            if (count <= m.Length)
            {
                t = new ReadOnlySequence<T>(m[..count]);
                return true;
            }

            var c = count - m.Length;
            IChunk<T> next;
            while (c > 0)
            {
                next = chunk.Next();
                if (next == null)
                {
                    break;
                }
                var memory = next.Memory;
                if (c <= memory.Length)
                {
                    t = new ReadOnlySequence<T>(chunk as ReadOnlySequenceSegment<T>, chunk.Index, next as ReadOnlySequenceSegment<T>, c - 1);
                    return true;
                }
                c -= memory.Length;
            }
            t = default;
            return false;
        }

        public static T Tag<T>(this IChunkReader<T> buffer, T tag) where T : IEquatable<T>
        {
            if (!buffer.TryPeek(out var t))
            {
                throw new ParseException($"Expect {tag} but got eof");
            }

            if (tag.Equals(t))
            {
                buffer.GetCurrentChunk().Consume(1);
                return tag;
            }

            throw new ParseException($"Expect {tag} but got {t}");
        }

        public static ReadOnlySequence<char> AsciiHexDigit(this IChunkReader<char> buffer, int count)
        {
            if (!buffer.TryPeek(count, out var r))
            {
                throw new ParseException($"Expect {count} AsciiHexDigit char but got eof");
            }
            if (r.IsSingleSegment)
            {
                foreach (var c in r.FirstSpan)
                {
                    if (!char.IsAsciiHexDigit(c))
                    {
                        throw new ParseException($"Expect AsciiHexDigit char but got {c}");
                    }
                }
                buffer.GetCurrentChunk().Consume(count);
                return r;
            }
            foreach (var item in r)
            {
                foreach (var c in r.FirstSpan)
                {
                    if (!char.IsAsciiHexDigit(c))
                    {
                        throw new ParseException($"Expect AsciiHexDigit char but got {c}");
                    }
                }
            }

            buffer.GetCurrentChunk().Consume(count);
            return r;
        }

        public static void Eof<T>(this IChunkReader<T> buffer)
        {
            if (!buffer.TryPeek(out var t))
            {
                throw new ParseException($"Expect eof but got {t}");
            }
        }

        public static bool Line(this IChunkReader<char> reader, out ReadOnlySequence<char> line)
        {
            if (reader.IsEOF)
            {
                line = default;
                return false;
            }

            var buffer = reader.GetCurrentChunk();
            if (buffer is ISingleChunkReader<char> r)
            {
                var charBufferSpan = r.Span;
                int idxOfNewline = charBufferSpan.IndexOfAny('\r', '\n');
                if (idxOfNewline >= 0)
                {
                    line = new ReadOnlySequence<char>(r.Memory[..idxOfNewline]);
                    char ch = charBufferSpan[idxOfNewline];
                    if (ch == '\r')
                    {
                        var pos = idxOfNewline + 1;
                        if ((uint)pos < (uint)charBufferSpan.Length && charBufferSpan[pos] == '\n')
                        {
                            idxOfNewline++;
                        }
                    }
                    buffer.Consume(idxOfNewline + 1);
                    return true;
                }
                else
                {
                    line = new ReadOnlySequence<char>(r.Memory);
                    buffer.Consume(charBufferSpan.Length);
                    return true;
                }
            }

            var next = buffer;
            do
            {
                var charBufferSpan = next.Span;
                int idxOfNewline = charBufferSpan.IndexOfAny('\r', '\n');
                if (idxOfNewline >= 0)
                {
                    var f1 = buffer as ReadOnlySequenceSegment<char>;
                    var e1 = next as ReadOnlySequenceSegment<char>;
                    line = new ReadOnlySequence<char>(f1, buffer.Index, e1, idxOfNewline - 1); ;
                    char ch = charBufferSpan[idxOfNewline];
                    if (ch == '\r')
                    {
                        var pos = idxOfNewline + 1;
                        if ((uint)pos < (uint)charBufferSpan.Length && charBufferSpan[pos] == '\n')
                        {
                            idxOfNewline++;
                        }
                    }
                    next.Consume(idxOfNewline + 1);
                    return true;
                }
                next = next.Next();
            } while (next != null);
            var f = buffer as ReadOnlySequenceSegment<char>;
            var e = next as ReadOnlySequenceSegment<char>;
            var el = e.Memory.Length;
            line = new ReadOnlySequence<char>(f, buffer.Index, e, el - 1); ;
            buffer.Consume(el);
            return true;
        }
    }
}