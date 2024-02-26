using RuQu.Reader;
using System.Text;

namespace RuQu
{
    public abstract class SimpleCharParserBase<T>
    {
        public int BufferSize { get; set; } = 256;

        #region Read

        public virtual async ValueTask<T?> ReadAsync(TextReader reader, CancellationToken cancellationToken = default)
        {
            IReaderBuffer<char> buffer = new TextReaderBuffer(reader, BufferSize);
            try
            {
                return await ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
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

        public virtual async ValueTask<T?> ReadAsync(string content, CancellationToken cancellationToken = default)
        {
            IReaderBuffer<char> buffer = new StringReaderBuffer(content);
            try
            {
                return await ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        public virtual async ValueTask<T?> ReadAsync(char[] content, CancellationToken cancellationToken = default)
        {
            IReaderBuffer<char> buffer = new ArrayReaderBuffer<char>(content);
            try
            {
                return await ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        public virtual async ValueTask<T?> ReadAsync(Memory<char> content, CancellationToken cancellationToken = default)
        {
            IReaderBuffer<char> buffer = new ReadOnlyMemoryReaderBuffer<char>(content);
            try
            {
                return await ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        public virtual async ValueTask<T?> ReadAsync(ReadOnlyMemory<char> content, CancellationToken cancellationToken = default)
        {
            IReaderBuffer<char> buffer = new ReadOnlyMemoryReaderBuffer<char>(content);
            try
            {
                return await ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                buffer.Dispose();
            }
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
            var buffer = new StringReaderBuffer(content);
            try
            {
                return Read(buffer);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        public virtual T? Read(char[] content)
        {
            var buffer = new ArrayReaderBuffer<char>(content);
            try
            {
                return Read(buffer);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        public virtual T? Read(Span<char> content)
        {
            var buffer = new ReadOnlySpanReaderBuffer<char>(content);
            try
            {
                return Read(buffer);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        public virtual T? Read(ReadOnlySpan<char> content)
        {
            var buffer = new ReadOnlySpanReaderBuffer<char>(content);
            try
            {
                return Read(buffer);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        public virtual T? Read(Memory<char> content)
        {
            var buffer = new ReadOnlyMemoryReaderBuffer<char>(content);
            try
            {
                return Read(buffer);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        public virtual T? Read(ReadOnlyMemory<char> content)
        {
            var buffer = new ReadOnlyMemoryReaderBuffer<char>(content);
            try
            {
                return Read(buffer);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        public virtual T? Read(TextReader reader)
        {
            var buffer = new TextReaderBuffer(reader, BufferSize);
            try
            {
                return Read(buffer);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        protected virtual ValueTask<T?> ReadAsync(IReaderBuffer<char> buffer, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(Read(buffer));
        }

        protected abstract T? Read(IReaderBuffer<char> buffer);

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