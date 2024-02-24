using RuQu.Reader;
using System.Text;

namespace RuQu
{
    public abstract class SimpleCharParserBase<T, ReadState>
    {
        public int BufferSize { get; set; } = 256;

        #region Read

        public virtual async ValueTask<T?> ReadAsync(TextReader reader, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> buffer = new CharReadBuffer(reader, BufferSize);
            var state = InitReadState();
            try
            {
                while (true)
                {
                    buffer = await buffer.ReadNextBufferAsync(cancellationToken).ConfigureAwait(false);
                    T? value = await ContinueReadAsync(buffer, ref state, cancellationToken).ConfigureAwait(false);

                    if (buffer.IsFinalBlock)
                    {
                        return value;
                    }
                }
            }
            finally
            {
                buffer.Dispose();
            }
        }

        public virtual ValueTask<T?> ReadAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            return ReadAsync(new StreamReader(stream, bufferSize: BufferSize), cancellationToken);
        }

        public virtual ValueTask<T?> ReadAsync(Stream stream, System.Text.Encoding encoding, CancellationToken cancellationToken = default)
        {
            return ReadAsync(new StreamReader(stream, encoding, bufferSize: BufferSize), cancellationToken);
        }

        public virtual ValueTask<T?> ReadAsync(string content, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> buffer = new StringReadBuffer(content);
            var state = InitReadState();
            return ContinueReadAsync(buffer, ref state, cancellationToken);
        }

        public virtual ValueTask<T?> ReadAsync(char[] content, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> buffer = new ArrayReadBuffer<char>(content);
            var state = InitReadState();
            return ContinueReadAsync(buffer, ref state, cancellationToken);
        }

        public virtual ValueTask<T?> ReadAsync(Span<char> content, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> buffer = new ReadOnlySpanReadBuffer<char>(content);
            var state = InitReadState();
            return ContinueReadAsync(buffer, ref state, cancellationToken);
        }

        public virtual ValueTask<T?> ReadAsync(ReadOnlySpan<char> content, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> buffer = new ReadOnlySpanReadBuffer<char>(content);
            var state = InitReadState();
            return ContinueReadAsync(buffer, ref state, cancellationToken);
        }

        public virtual ValueTask<T?> ReadAsync(Memory<char> content, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> buffer = new ReadOnlyMemoryReadBuffer<char>(content);
            var state = InitReadState();
            return ContinueReadAsync(buffer, ref state, cancellationToken);
        }

        public virtual ValueTask<T?> ReadAsync(ReadOnlyMemory<char> content, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> buffer = new ReadOnlyMemoryReadBuffer<char>(content);
            var state = InitReadState();
            return ContinueReadAsync(buffer, ref state, cancellationToken);
        }

        public virtual T? Read(Stream stream)
        {
            return Read(new StreamReader(stream, bufferSize: BufferSize));
        }

        public virtual T? Read(Stream stream, System.Text.Encoding encoding)
        {
            return Read(new StreamReader(stream, encoding, bufferSize: BufferSize));
        }

        public virtual T? Read(string content)
        {
            IReadBuffer<char> buffer = new StringReadBuffer(content);
            var state = InitReadState();
            return ContinueRead(buffer, ref state);
        }

        public virtual T? Read(char[] content)
        {
            IReadBuffer<char> buffer = new ArrayReadBuffer<char>(content);
            var state = InitReadState();
            return ContinueRead(buffer, ref state);
        }

        public virtual T? Read(Span<char> content)
        {
            IReadBuffer<char> buffer = new ReadOnlySpanReadBuffer<char>(content);
            var state = InitReadState();
            return ContinueRead(buffer, ref state);
        }

        public virtual T? Read(ReadOnlySpan<char> content)
        {
            IReadBuffer<char> buffer = new ReadOnlySpanReadBuffer<char>(content);
            var state = InitReadState();
            return ContinueRead(buffer, ref state);
        }

        public virtual T? Read(Memory<char> content)
        {
            IReadBuffer<char> buffer = new ReadOnlyMemoryReadBuffer<char>(content);
            var state = InitReadState();
            return ContinueRead(buffer, ref state);
        }

        public virtual T? Read(ReadOnlyMemory<char> content)
        {
            IReadBuffer<char> buffer = new ReadOnlyMemoryReadBuffer<char>(content);
            var state = InitReadState();
            return ContinueRead(buffer, ref state);
        }

        public virtual T? Read(TextReader reader)
        {
            IReadBuffer<char> buffer = new CharReadBuffer(reader, BufferSize);
            var state = InitReadState();
            try
            {
                while (true)
                {
                    buffer.ReadNextBuffer();
                    T? value = ContinueRead(buffer, ref state);

                    if (buffer.IsFinalBlock)
                    {
                        return value;
                    }
                }
            }
            finally
            {
                buffer.Dispose();
            }
        }

        protected virtual ValueTask<T?> ContinueReadAsync(IReadBuffer<char> buffer, ref ReadState state, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(ContinueRead(buffer, ref state));
        }

        protected abstract T? ContinueRead(IReadBuffer<char> buffer, ref ReadState state);

        protected abstract ReadState InitReadState();

        #endregion Read

        #region Write

        public virtual async ValueTask<string> WriteToStringAsync(T value, CancellationToken cancellationToken = default)
        {
            var sb = new StringBuilder(BufferSize);
            await foreach (var item in ContinueWriteAsync(value, cancellationToken).ConfigureAwait(false))
            {
                sb.Append(item);
            }
            return sb.ToString();
        }

        public virtual async ValueTask WriteAsync(T value, TextWriter writer, CancellationToken cancellationToken = default)
        {
            await foreach (var item in ContinueWriteAsync(value, cancellationToken).ConfigureAwait(false))
            {
                await writer.WriteAsync(item, cancellationToken).ConfigureAwait(false);
            }
            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual ValueTask WriteAsync(T value, Stream stream, System.Text.Encoding encoding, CancellationToken cancellationToken = default)
        {
            return WriteAsync(value, new StreamWriter(stream, encoding, bufferSize: BufferSize), cancellationToken);
        }

        public virtual void Write(T value, Stream stream, System.Text.Encoding encoding)
        {
            Write(value, new StreamWriter(stream, encoding, bufferSize: BufferSize));
        }

        public virtual void Write(T value, TextWriter writer)
        {
            foreach (var item in ContinueWrite(value))
            {
                writer.Write(item.Span);
            }
            writer.Flush();
        }

        public virtual string WriteToString(T value)
        {
            var sb = new StringBuilder(BufferSize);
            foreach (var item in ContinueWrite(value))
            {
                sb.Append(item.Span);
            }
            return sb.ToString();
        }

        protected virtual IAsyncEnumerable<ReadOnlyMemory<char>> ContinueWriteAsync(T value, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ContinueWrite(value).ToAsyncEnumerable();
        }

        protected abstract IEnumerable<ReadOnlyMemory<char>> ContinueWrite(T value);

        #endregion Write
    }
}