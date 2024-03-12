using System.Collections;

namespace RuQu.Buffer
{
    public abstract class TextDataChunkReader<Row> : IDisposable, IEnumerable<Row>, IEnumerator<Row>
    {
        protected IChunkReader<char> reader;

        public Row Current { get; protected set; }

        object IEnumerator.Current => Current;

        public TextDataChunkReader(TextReader reader, int bufferSize = 128, SparseBufferGrowth growth = SparseBufferGrowth.Exponential)
        {
            this.reader = new TextSparseBufferReader(reader, bufferSize, growth);
        }

        public TextDataChunkReader(string content)
        {
            this.reader = new StringSparseBufferReader(content);
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

        public abstract bool MoveNext();

        public void Reset()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
    }
}