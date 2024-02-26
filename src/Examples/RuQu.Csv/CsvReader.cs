﻿using RuQu.Reader;
using System;
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
            while (ProcessField(out var f))
            {
                r.Add(f);
            }
            reader.IngoreCRLF();
            row = r.ToArray();
            FieldCount = row.Length;
            return true;
        }

        private bool TakeString(out string s)
        {
            if (reader.IsEOF)
            {
                throw new ParseException($"Expect some string end with '\"' at {reader.Index} but got eof");
            }

            int charPos = 0;
            int len;
            ReadOnlySpan<char> remaining;
            do
            {
                remaining = reader.Readed;
                len = remaining.Length;
                var charBufferSpan = remaining[charPos..];
                var i = charBufferSpan.IndexOf('"');
                if (i >= 0)
                {
                    if (i+ 1 >= remaining.Length)
                    {
                        if (reader.ReadNextBuffer(len))
                        {
                            continue;
                        }
                        else
                        {
                            s = remaining[..(charPos + i)].ToString();
                            return true;
                        }
                    }
                    if (remaining[i + 1] is '"')
                    {
                        charPos += i + 2;
                        continue;
                    }
                    s = remaining[..(charPos + i)].ToString();
                    return true;
                }
                else
                {
                    charPos += remaining.Length;
                }
            }
            while (reader.ReadNextBuffer(len));
            throw new ParseException($"Expect string end with '\"' at {reader.Index} but got eof");
        }

        private bool TakeField(out string s)
        {
            int charPos = 0;
            int len;
            ReadOnlySpan<char> remaining;
            do
            {
                remaining = reader.Readed;
                len = remaining.Length;
                var charBufferSpan = remaining[charPos..];
                var i = charBufferSpan.IndexOf(Separater);
                if (i >= 0)
                {
                    charPos += i;
                    s = remaining[..charPos].ToString();
                    reader.Consume(charPos);
                    return true;
                }
                else
                {
                    charPos += remaining.Length;
                }
            }
            while (reader.ReadNextBuffer(len));
            throw new ParseException($"Expect string end with '\"' at {reader.Index} but got eof");
        }

        private bool ProcessField(out string f)
        {
            throw new NotImplementedException();
            //    if (!reader.Peek(out var c) || reader.IngoreCRLF())
            //    {
            //        f = null;
            //        return false;
            //    }
            //    if (c == Separater)
            //    {
            //        f = string.Empty;
            //        reader.Consume(1);
            //        return true;
            //    }
            //    else if (c is '"')
            //    {
            //        reader.Consume(1);
            //        if (TakeString(out f))
            //        { 

            //        }

            //    }
            //    else
            //    { 

            //    }
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

    //public class CsvReader : TextDataReader<string[]>
    //{
    //    public CsvReader(TextReader reader, int bufferSize) : base(reader, bufferSize)
    //    {
    //    }

    //    private List<string> _current = new List<string>();
    //    public char Separater { get; set; } = ',';

    //    protected override bool ContinueRead(IReadBuffer<char> reader, out string[] row)
    //    {
    //        var buffer = reader.Remaining;
    //        if (buffer.IsEmpty)
    //        {
    //            return ToRow(out row);
    //        }
    //        if (buffer[0] is '#')
    //        {
    //            var c = reader.ReadLine(out var line);
    //            reader.AdvanceBuffer(c);
    //            return ToRow(out row);
    //        }
    //        return ToRow(out row);

    //    }

    //    private bool ToRow(out string[] row)
    //    {
    //        if (_current.Count == 0)
    //        {
    //            row = null;
    //            return false;
    //        }
    //        row = [.. _current];
    //        _current.Clear();
    //        return true;
    //    }
    //}

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