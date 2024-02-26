using System.Collections;
using System.Text;

namespace RuQu.Reader
{
    public abstract class TextDataReader<Row> : IDisposable, IEnumerable<Row>, IEnumerator<Row>, IAsyncEnumerable<Row>, IAsyncEnumerator<Row>
    {
        protected IReaderBuffer<char> reader;

        public Row Current { get; protected set; }

        object IEnumerator.Current => Current;

        public TextDataReader(Stream stream, int bufferSize)
        {
            this.reader = new TextReaderBuffer(new StreamReader(stream, Encoding.UTF8), bufferSize);
        }

        public TextDataReader(Stream stream, Encoding encoding, int bufferSize)
        {
            this.reader = new TextReaderBuffer(new StreamReader(stream, encoding), bufferSize);
        }

        public TextDataReader(TextReader reader, int bufferSize)
        {
            this.reader = new TextReaderBuffer(reader, bufferSize);
        }

        public TextDataReader(string content)
        {
            this.reader = new StringReaderBuffer(content);
        }

        public TextDataReader(char[] content)
        {
            this.reader = new ArrayReaderBuffer<char>(content);
        }

        public TextDataReader(ReadOnlyMemory<char> content)
        {
            this.reader = new ReadOnlyMemoryReaderBuffer<char>(content);
        }

        public TextDataReader(Memory<char> content)
        {
            this.reader = new ReadOnlyMemoryReaderBuffer<char>(content);
        }

        public TextDataReader(ReadOnlySpan<char> content)
        {
            this.reader = new ReadOnlySpanReaderBuffer<char>(content);
        }

        public TextDataReader(Span<char> content)
        {
            this.reader = new ReadOnlySpanReaderBuffer<char>(content);
        }

        public void Dispose()
        {
            if (reader != null)
            {
                reader.Dispose();
                reader = null;
            }
        }

        public IEnumerator<Row> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        public IAsyncEnumerator<Row> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return this;
        }

        public abstract bool MoveNext();

        public virtual void Reset()
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask<bool> MoveNextAsync()
        {
            return ValueTask.FromResult(MoveNext());
        }

        public virtual ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}