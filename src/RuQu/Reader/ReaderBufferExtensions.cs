using System.Diagnostics;

namespace RuQu.Reader
{
    public static class ReaderBufferExtensions
    {
        public static ReadOnlySpan<byte> Utf8Bom => [0xEF, 0xBB, 0xBF];

        public static void IngoreUtf8Bom(this IReaderBuffer<byte> buffer)
        {
            if (buffer.Peek(out var _))
            {
                var remaining = buffer.Readed;
                // Handle the UTF-8 BOM if present
                Debug.Assert(remaining.Length >= Utf8Bom.Length);
                if (remaining.StartsWith(Utf8Bom))
                {
                    buffer.Consume(Utf8Bom.Length);
                }
            }
        }

        public static T Tag<T>(this IReaderBuffer<T> buffer, T tag) where T : struct, IEquatable<T>
        {
            if (!buffer.Peek(out var t))
            {
                throw new ParseException($"Expect {tag} at {buffer.Index} but got eof");
            }

            if (tag.Equals(t))
            {
                buffer.Consume(1);
                return tag;
            }

            throw new ParseException($"Expect {tag} at {buffer.Index} but got {t}");
        }

        public static bool OptionTag<T>(this IReaderBuffer<T> buffer, T tag) where T : struct, IEquatable<T>
        {
            if (buffer.Peek(out var t))
            {
                if (tag.Equals(t))
                {
                    buffer.Consume(1);
                    return true;
                }
            }

            return false;
        }

        public static ReadOnlySpan<char> AsciiHexDigit(this IReaderBuffer<char> buffer, int count)
        {
            if (buffer.IsEOF)
            {
                throw new ParseException($"Expect {count} AsciiHexDigit char at {buffer.Index} but got eof");
            }

            if (!buffer.Peek(count, out var t))
            {
                throw new ParseException($"Expect {count} AsciiHexDigit char at {buffer.Index} but no enough data");
            }
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

        public static int IndexOfAny<T>(this IReaderBuffer<T> buffer, T value0, T value1, T value2) where T : struct, IEquatable<T>?
        {
            if (buffer.IsEOF)
            {
                return -1;
            }
            if (buffer is IFixedReaderBuffer<T> fixedReaderBuffer)
            {
                return fixedReaderBuffer.Readed.IndexOfAny(value0, value1, value2);
            }
            int pos = 0;
            int len;
            do
            {
                var r = buffer.Readed;
                len = r.Length;
                var charBufferSpan = r[pos..];
                int idxOf = charBufferSpan.IndexOfAny(value0, value1, value2);
                if (idxOf >= 0)
                {
                    return idxOf + pos;
                }
                else
                {
                    pos += charBufferSpan.Length;
                }
            } while (buffer.ReadNextBuffer(len));
            return -1;
        }

        public static int LastIndexOfAny<T>(this IReaderBuffer<T> buffer, T value0, T value1) where T : struct, IEquatable<T>?
        {
            if (buffer.IsEOF)
            {
                return -1;
            }
            if (buffer is IFixedReaderBuffer<T> fixedReaderBuffer)
            {
                return fixedReaderBuffer.Readed.LastIndexOfAny(value0, value1);
            }
            int pos = 0;
            int len;
            do
            {
                var r = buffer.Readed;
                len = r.Length;
                var charBufferSpan = r[pos..];
                int idxOf = charBufferSpan.LastIndexOfAny(value0, value1);
                if (idxOf >= 0)
                {
                    return idxOf + pos;
                }
                else
                {
                    pos += charBufferSpan.Length;
                }
            } while (buffer.ReadNextBuffer(len));
            return -1;
        }


        public static int IndexOf<T>(this IReaderBuffer<T> buffer, T value) where T : struct, IEquatable<T>?
        {
            if (buffer.IsEOF)
            {
                return -1;
            }
            if (buffer is IFixedReaderBuffer<T> fixedReaderBuffer)
            {
                return fixedReaderBuffer.Readed.IndexOf(value);
            }
            int pos = 0;
            int len;
            do
            {
                var r = buffer.Readed;
                len = r.Length;
                var charBufferSpan = r[pos..];
                int idxOf = charBufferSpan.IndexOf(value);
                if (idxOf >= 0)
                {
                    return idxOf + pos;
                }
                else
                {
                    pos += charBufferSpan.Length;
                }
            } while (buffer.ReadNextBuffer(len));
            return -1;
        }

        public static bool Line(this IReaderBuffer<char> buffer, out ReadOnlySpan<char> line)
        {
            if (buffer.IsEOF)
            {
                line = default;
                return false;
            }
            if (buffer is IFixedReaderBuffer<char> fixedReaderBuffer)
            {
                var charBufferSpan = fixedReaderBuffer.Readed;
                int idxOfNewline = charBufferSpan.IndexOfAny('\r', '\n');
                if (idxOfNewline >= 0)
                {
                    line = charBufferSpan[..idxOfNewline];
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
                    line = charBufferSpan;
                    buffer.Consume(charBufferSpan.Length);
                    return true;
                }
            }

            int charPos = 0;
            int len;
            ReadOnlySpan<char> remaining;
            do
            {
                remaining = buffer.Readed;
                len = remaining.Length;
                var charBufferSpan = remaining[charPos..];
                int idxOfNewline = charBufferSpan.IndexOfAny('\r', '\n');
                if (idxOfNewline >= 0)
                {
                    line = remaining[0..(charPos + idxOfNewline)];
                    char ch = charBufferSpan[idxOfNewline];
                    charPos += idxOfNewline + 1;

                    if (ch == '\r')
                    {
                        if ((uint)charPos < (uint)remaining.Length && remaining[charPos] == '\n')
                        {
                            charPos++;
                        }
                    }
                    buffer.Consume(charPos);
                    return true;
                }
                else
                {
                    charPos += remaining.Length;
                }
            } while (buffer.ReadNextBuffer(len));
            line = remaining;
            buffer.Consume(len);
            return true;
        }

        public static bool IngoreCRLF(this IReaderBuffer<char> buffer)
        {
            if (buffer.Peek(out var c))
            {
                var r = false;
                if (c is '\r')
                {
                    r = true;
                    buffer.Consume(1);
                    if (!buffer.Peek(out c))
                    {
                        return r;
                    }
                }

                if (c is '\n')
                {
                    r = true;
                    buffer.Consume(1);
                }
                return r;
            }
            return false;
        }

        public static void Eof<T>(this IReaderBuffer<T> buffer) where T : struct
        {
            if (!buffer.IsEOF)
            {
                buffer.Peek(out var t);
                throw new ParseException($"Expect eof at {buffer.Index} but got {t}");
            }
        }
    }
}