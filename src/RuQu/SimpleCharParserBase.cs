using RuQu.Reader;

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

        public virtual async ValueTask WriteAsync(TextWriter writer, Options options, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public virtual ValueTask WriteAsync(Stream stream, System.Text.Encoding encoding, Options options, CancellationToken cancellationToken = default)
        {
            return WriteAsync(new StreamWriter(stream, encoding, bufferSize: options.BufferSize), options, cancellationToken);
        }

        public virtual void Write(Stream stream, System.Text.Encoding encoding, Options options)
        {
            Write(new StreamWriter(stream, encoding, bufferSize: options.BufferSize), options);
        }

        public virtual void Write(TextWriter writer, Options options)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}