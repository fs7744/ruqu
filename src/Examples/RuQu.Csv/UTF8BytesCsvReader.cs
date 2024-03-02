using RuQu.Reader;
using System.Text;

namespace RuQu.Csv
{
    public class UTF8BytesCsvReader : StreamDataReader<string[]>
    {
        public UTF8BytesCsvReader(Stream stream, int bufferSize = 4096, byte separater = (byte)',', bool fristIsHeader = false) : base(stream, bufferSize)
        {
            Separater = separater;
            HasHeader = fristIsHeader;
        }

        public byte Separater { get; private set; }

        public bool HasHeader { get; private set; }

        public string[] Header { get; private set; }

        public int FieldCount { get; private set; }

        public override bool MoveNext()
        {
            string[] row;
            if (HasHeader && Header == null)
            {
                if (!ProcessFirstRow(out row))
                {
                    throw new ParseException("Missing header");
                }
                Header = row;
            }

            var r = FieldCount == 0 ? ProcessFirstRow(out row) : ProcessRow(out row);
            Current = row;
            return r;
        }

        public bool IngoreCRLF()
        {
            if (reader.Peek(out byte c))
            {
                var r = false;
                if (c is (byte)'\r')
                {
                    r = true;
                    reader.Consume(1);
                    if (!reader.Peek(out c))
                    {
                        return r;
                    }
                }

                if (c is (byte)'\n')
                {
                    r = true;
                    reader.Consume(1);
                }
                return r;
            }
            return false;
        }

        private bool ProcessFirstRow(out string[]? row)
        {
            var r = new List<string>();
            var hasValue = false;
            while (ProcessField(out var f))
            {
                r.Add(f);
                hasValue = true;
            }
            IngoreCRLF();
            row = r.ToArray();
            FieldCount = row.Length;
            return hasValue;
        }

        private bool TakeString(out string s)
        {
            if (reader.IsEOF)
            {
                throw new ParseException($"Expect some string end with '\"' at {reader.Index} but got eof");
            }

            int pos = 0;
            int len;
            ReadOnlySpan<byte> remaining;
            do
            {
            ddd:
                remaining = reader.Readed;
                len = remaining.Length;
                var charBufferSpan = remaining[pos..];
                var i = charBufferSpan.IndexOf(Separater);
                if (i >= 0)
                {
                    if ((i + pos) == 0)
                    {
                        pos++;
                    }
                    else if (remaining[pos + i - 1] == '"')
                    {
                        s = Encoding.UTF8.GetString(remaining[..(i + pos)][..^1]);
                        reader.Consume(pos + i + 1);
                        return true;
                    }
                    else
                    {
                        pos += i + 1;
                        goto ddd;
                    }
                }
                else
                {
                    pos = len;
                }
            } while (reader.ReadNextBuffer(len));
            s = Encoding.UTF8.GetString(reader.Readed);
            return true;
        }

        private bool ProcessField(out string? f)
        {
            if (!reader.Peek(out var c) || IngoreCRLF())
            {
                f = null;
                return false;
            }
            if (c == Separater)
            {
                f = string.Empty;
                reader.Consume(1);
                return true;
            }
            else if (c is (byte)'"')
            {
                reader.Consume(1);
                return TakeString(out f);
            }
            else
            {
                var i = reader.IndexOfAny(Separater, (byte)'\r', (byte)'\n');
                if (i == 0)
                {
                    f = string.Empty;
                }
                else if (i > 0)
                {
                    f = Encoding.UTF8.GetString(reader.Readed[..i]);
                    reader.Consume(i);
                }
                else
                {
                    f = Encoding.UTF8.GetString(reader.Readed);
                    reader.Consume(f.Length);
                }
                if (reader.Peek(out var cc) && cc == Separater)
                {
                    reader.Consume(1);
                }
                return true;
            }
        }

        private bool ProcessRow(out string[]? row)
        {
            row = new string[FieldCount];

            for (int i = 0; i < FieldCount; i++)
            {
                if (!ProcessField(out var f))
                {
                    IngoreCRLF();
                    return false;
                }
                row[i] = f;
            }
            IngoreCRLF();
            return true;
        }
    }

    //public class CsvWriter
    //{
    //    public CsvWriter(TextWriter writer)
    //    {
    //    }

    //    public void WriteRow(ReadOnlySpan<string> row)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public ValueTask WriteRowAsync(ReadOnlySpan<string> row)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}