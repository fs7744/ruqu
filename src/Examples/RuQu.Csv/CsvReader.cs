using RuQu.Reader;
using System.Text;

namespace RuQu.Csv
{
    public class CsvReader : TextDataReader<string[]>
    {
        public CsvReader(string content, char separater = ',', bool fristIsHeader = false) : base(content)
        {
            Separater = separater;
            HasHeader = fristIsHeader;
        }

        public CsvReader(char[] content, char separater = ',', bool fristIsHeader = false) : base(content)
        {
            Separater = separater;
            HasHeader = fristIsHeader;
        }

        public CsvReader(ReadOnlyMemory<char> content, char separater = ',', bool fristIsHeader = false) : base(content)
        {
            Separater = separater;
            HasHeader = fristIsHeader;
        }

        public CsvReader(Memory<char> content, char separater = ',', bool fristIsHeader = false) : base(content)
        {
            Separater = separater;
            HasHeader = fristIsHeader;
        }

        public CsvReader(ReadOnlySpan<char> content, char separater = ',', bool fristIsHeader = false) : base(content)
        {
            Separater = separater;
            HasHeader = fristIsHeader;
        }

        public CsvReader(Span<char> content, char separater = ',', bool fristIsHeader = false) : base(content)
        {
            Separater = separater;
            HasHeader = fristIsHeader;
        }

        public CsvReader(Stream stream, int bufferSize = 256, char separater = ',', bool fristIsHeader = false) : base(stream, bufferSize)
        {
            Separater = separater;
            HasHeader = fristIsHeader;
        }

        public CsvReader(TextReader reader, int bufferSize = 256, char separater = ',', bool fristIsHeader = false) : base(reader, bufferSize)
        {
            Separater = separater;
            HasHeader = fristIsHeader;
        }

        public CsvReader(Stream stream, Encoding encoding, int bufferSize = 256, char separater = ',', bool fristIsHeader = false) : base(stream, encoding, bufferSize)
        {
            Separater = separater;
            HasHeader = fristIsHeader;
        }

        public char Separater { get; private set; } = ',';

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

        private bool ProcessFirstRow(out string[]? row)
        {
            var r = new List<string>();
            var hasValue = false;
            while (ProcessField(out var f))
            {
                r.Add(f);
                hasValue = true;
            }
            reader.IngoreCRLF();
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
            ReadOnlySpan<char> remaining;
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
                        s = remaining[..(i + pos)][..^1].ToString();
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
            s = reader.Readed.ToString();
            return true;
        }

        private bool ProcessField(out string? f)
        {
            if (!reader.Peek(out var c) || reader.IngoreCRLF())
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
            else if (c is '"')
            {
                reader.Consume(1);
                return TakeString(out f);
            }
            else
            {
                var i = reader.IndexOfAny(Separater, '\r', '\n');
                if (i == 0)
                {
                    f = string.Empty;
                }
                else if (i > 0)
                {
                    f = reader.Readed[..i].ToString();
                    reader.Consume(i);
                }
                else
                {
                    f = reader.Readed.ToString();
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
                    reader.IngoreCRLF();
                    return false;
                }
                row[i] = f;
            }
            reader.IngoreCRLF();
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