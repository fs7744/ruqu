using RuQu.Reader;
using RuQu.Writer;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading;

namespace RuQu
{
    public abstract class SimpleCharParserBase<T, Options> where Options : IOptions<T>
    {
        #region Read

        public virtual async ValueTask<T?> ReadAsync(TextReader reader, Options options, CancellationToken cancellationToken = default)
        {
            Options opt = (Options)options.CloneReadOptions();
            IReadBuffer<char> buffer = new CharReadBuffer(reader, options.BufferSize);
            try
            {
                while (true)
                {
                    buffer = await buffer.ReadNextBufferAsync(cancellationToken).ConfigureAwait(false);
                    T? value = await ContinueReadAsync(buffer, opt, cancellationToken);

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

        public virtual ValueTask<T?> ReadAsync(Stream stream, Options options, CancellationToken cancellationToken = default)
        {
            return ReadAsync(new StreamReader(stream, bufferSize: options.BufferSize), options, cancellationToken);
        }

        public virtual ValueTask<T?> ReadAsync(Stream stream, System.Text.Encoding encoding, Options options, CancellationToken cancellationToken = default)
        {
            return ReadAsync(new StreamReader(stream, encoding, bufferSize: options.BufferSize), options, cancellationToken);
        }

        public virtual ValueTask<T?> ReadAsync(string content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> buffer = new StringReadBuffer(content);
            Options opt = (Options)options.CloneReadOptions();
            return ContinueReadAsync(buffer, opt, cancellationToken);
        }

        public virtual ValueTask<T?> ReadAsync(char[] content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> buffer = new ArrayReadBuffer<char>(content);
            Options opt = (Options)options.CloneReadOptions();
            return ContinueReadAsync(buffer, opt, cancellationToken);
        }

        public virtual ValueTask<T?> ReadAsync(Span<char> content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> buffer = new ReadOnlySpanReadBuffer<char>(content);
            Options opt = (Options)options.CloneReadOptions();
            return ContinueReadAsync(buffer, opt, cancellationToken);
        }

        public virtual ValueTask<T?> ReadAsync(ReadOnlySpan<char> content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> buffer = new ReadOnlySpanReadBuffer<char>(content);
            Options opt = (Options)options.CloneReadOptions();
            return ContinueReadAsync(buffer, opt, cancellationToken);
        }

        public virtual ValueTask<T?> ReadAsync(Memory<char> content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> buffer = new ReadOnlyMemoryReadBuffer<char>(content);
            Options opt = (Options)options.CloneReadOptions();
            return ContinueReadAsync(buffer, opt, cancellationToken);
        }

        public virtual ValueTask<T?> ReadAsync(ReadOnlyMemory<char> content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> buffer = new ReadOnlyMemoryReadBuffer<char>(content);
            Options opt = (Options)options.CloneReadOptions();
            return ContinueReadAsync(buffer, opt, cancellationToken);
        }

        public virtual T? Read(Stream stream, Options options)
        {
            return Read(new StreamReader(stream, bufferSize: options.BufferSize), options);
        }

        public virtual T? Read(Stream stream, System.Text.Encoding encoding, Options options)
        {
            return Read(new StreamReader(stream, encoding, bufferSize: options.BufferSize), options);
        }

        public virtual T? Read(string content, Options options)
        {
            IReadBuffer<char> buffer = new StringReadBuffer(content);
            Options opt = (Options)options.CloneReadOptions();
            return ContinueRead(buffer, opt);
        }

        public virtual T? Read(char[] content, Options options)
        {
            IReadBuffer<char> buffer = new ArrayReadBuffer<char>(content);
            Options opt = (Options)options.CloneReadOptions();
            return ContinueRead(buffer, opt);
        }

        public virtual T? Read(Span<char> content, Options options)
        {
            IReadBuffer<char> buffer = new ReadOnlySpanReadBuffer<char>(content);
            Options opt = (Options)options.CloneReadOptions();
            return ContinueRead(buffer, opt);
        }

        public virtual T? Read(ReadOnlySpan<char> content, Options options)
        {
            IReadBuffer<char> buffer = new ReadOnlySpanReadBuffer<char>(content);
            Options opt = (Options)options.CloneReadOptions();
            return ContinueRead(buffer, opt);
        }

        public virtual T? Read(Memory<char> content, Options options)
        {
            IReadBuffer<char> buffer = new ReadOnlyMemoryReadBuffer<char>(content);
            Options opt = (Options)options.CloneReadOptions();
            return ContinueRead(buffer, opt);
        }

        public virtual T? Read(ReadOnlyMemory<char> content, Options options)
        {
            IReadBuffer<char> buffer = new ReadOnlyMemoryReadBuffer<char>(content);
            Options opt = (Options)options.CloneReadOptions();
            return ContinueRead(buffer, opt);
        }

        public virtual T? Read(TextReader reader, Options options)
        {
            Options opt = (Options)options.CloneReadOptions();
            IReadBuffer<char> buffer = new CharReadBuffer(reader, options.BufferSize);
            try
            {
                while (true)
                {
                    buffer.ReadNextBuffer();
                    T? value = ContinueRead(buffer, opt);

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

        protected virtual ValueTask<T?> ContinueReadAsync(IReadBuffer<char> buffer, Options options, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(ContinueRead(buffer, options));
        }

        protected abstract T? ContinueRead(IReadBuffer<char> buffer, Options options);

        #endregion Read

        #region Write

        public virtual async ValueTask<string> WriteToStringAsync(T value, Options options, CancellationToken cancellationToken = default)
        {
            bool isFinalBlock;
            using var bufferWriter = new PooledBufferWriter<char>(options.BufferSize);
            Options opt = (Options)options.CloneWriteOptionsWithValue(value);
            var sb = new StringBuilder(options.BufferSize);
            do
            {
                isFinalBlock = await ContinueWriteAsync(bufferWriter, opt, cancellationToken);
                sb.Append(bufferWriter.WrittenMemory);
                bufferWriter.Clear();
            }
            while (!isFinalBlock);
            return sb.ToString();
        }

        public virtual async ValueTask WriteAsync(T value, TextWriter writer, Options options, CancellationToken cancellationToken = default)
        {
            bool isFinalBlock;
            using var bufferWriter = new PooledBufferWriter<char>(options.BufferSize);
            Options opt = (Options)options.CloneWriteOptionsWithValue(value);
            do
            {
                isFinalBlock = await ContinueWriteAsync(bufferWriter, opt, cancellationToken);
                await writer.WriteAsync(bufferWriter.WrittenMemory, cancellationToken).ConfigureAwait(false);
                bufferWriter.Clear();
            }
            while (!isFinalBlock);
            await writer.FlushAsync().ConfigureAwait(false);
        }

        public virtual ValueTask WriteAsync(T value, Stream stream, System.Text.Encoding encoding, Options options, CancellationToken cancellationToken = default)
        {
            return WriteAsync(value, new StreamWriter(stream, encoding, bufferSize: options.BufferSize), options, cancellationToken);
        }

        public virtual void Write(T value, Stream stream, System.Text.Encoding encoding, Options options)
        {
            Write(value, new StreamWriter(stream, encoding, bufferSize: options.BufferSize), options);
        }

        public virtual void Write(T value, TextWriter writer, Options options)
        {
            bool isFinalBlock;
            using var bufferWriter = new PooledBufferWriter<char>(options.BufferSize);
            Options opt = (Options)options.CloneWriteOptionsWithValue(value);
            do
            {
                isFinalBlock = ContinueWrite(bufferWriter, opt);
                writer.Write(bufferWriter.WrittenMemory.Span);
                bufferWriter.Clear();
            }
            while (!isFinalBlock);
            writer.Flush();
        }

        public virtual string WriteToString(T value, Options options)
        {
            bool isFinalBlock;
            using var bufferWriter = new PooledBufferWriter<char>(options.BufferSize);
            Options opt = (Options)options.CloneWriteOptionsWithValue(value);
            var sb = new StringBuilder(options.BufferSize);
            do
            {
                isFinalBlock = ContinueWrite(bufferWriter, opt);
                sb.Append(bufferWriter.WrittenMemory);
                bufferWriter.Clear();
            }
            while (!isFinalBlock);
            return sb.ToString();
        }

        public virtual ValueTask<bool> ContinueWriteAsync(IBufferWriter<char> writer, Options options, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(ContinueWrite(writer, options));
        }

        public abstract bool ContinueWrite(IBufferWriter<char> writer, Options options);

        #endregion
    }
}