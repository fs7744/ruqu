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

        public override bool MoveNext()
        {
            throw new NotImplementedException();
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