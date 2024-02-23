using RuQu.Reader;

namespace RuQu
{
    public abstract class SimpleCharParserBase<T, Options> where Options : IOptions
    {
        public async ValueTask<T?> ReadAsync(TextReader reader, Options options, CancellationToken cancellationToken = default)
        {
            Options state = (Options)options.Clone();
            IReadBuffer<char> bufferState = new CharReadBuffer(reader, state.BufferSize);
            try
            {
                while (true)
                {
                    bufferState = await bufferState.ReadNextBufferAsync(cancellationToken).ConfigureAwait(false);
                    T? value = ContinueRead(bufferState, state);

                    if (bufferState.IsFinalBlock)
                    {
                        return value;
                    }
                }
            }
            finally
            {
                bufferState.Dispose();
            }
        }

        public ValueTask<T?> ReadAsync(Stream stream, Options options, CancellationToken cancellationToken = default)
        {
            return ReadAsync(new StreamReader(stream, bufferSize: options.BufferSize), options, cancellationToken);
        }

        public ValueTask<T?> ReadAsync(Stream stream, System.Text.Encoding encoding, Options options, CancellationToken cancellationToken = default)
        {
            return ReadAsync(new StreamReader(stream, encoding, bufferSize: options.BufferSize), options, cancellationToken);
        }

        public ValueTask<T?> ReadAsync(string content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> bufferState = new StringReadBuffer(content);
            Options state = (Options)options.Clone();
            return ValueTask.FromResult(ContinueRead(bufferState, state));
        }

        public ValueTask<T?> ReadAsync(char[] content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> bufferState = new CharArrayReadBuffer(content);
            Options state = (Options)options.Clone();
            return ValueTask.FromResult(ContinueRead(bufferState, state));
        }

        public ValueTask<T?> ReadAsync(Span<char> content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> bufferState = new ReadOnlySpanReadBuffer<char>(content);
            Options state = (Options)options.Clone();
            return ValueTask.FromResult(ContinueRead(bufferState, state));
        }

        public ValueTask<T?> ReadAsync(ReadOnlySpan<char> content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> bufferState = new ReadOnlySpanReadBuffer<char>(content);
            Options state = (Options)options.Clone();
            return ValueTask.FromResult(ContinueRead(bufferState, state));
        }

        public ValueTask<T?> ReadAsync(Memory<char> content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> bufferState = new ReadOnlyMemoryReadBuffer<char>(content);
            Options state = (Options)options.Clone();
            return ValueTask.FromResult(ContinueRead(bufferState, state));
        }

        public ValueTask<T?> ReadAsync(ReadOnlyMemory<char> content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> bufferState = new ReadOnlyMemoryReadBuffer<char>(content);
            Options state = (Options)options.Clone();
            return ValueTask.FromResult(ContinueRead(bufferState, state));
        }

        public T? Read(Stream stream, Options options)
        {
            return Read(new StreamReader(stream, bufferSize: options.BufferSize), options);
        }

        public T? Read(Stream stream, System.Text.Encoding encoding, Options options)
        {
            return Read(new StreamReader(stream, encoding, bufferSize: options.BufferSize), options);
        }

        public T? Read(string content, Options options)
        {
            IReadBuffer<char> bufferState = new StringReadBuffer(content);
            Options state = (Options)options.Clone();
            return ContinueRead(bufferState, state);
        }

        public T? Read(char[] content, Options options)
        {
            IReadBuffer<char> bufferState = new CharArrayReadBuffer(content);
            Options state = (Options)options.Clone();
            return ContinueRead(bufferState, state);
        }

        public T? Read(Span<char> content, Options options)
        {
            IReadBuffer<char> bufferState = new ReadOnlySpanReadBuffer<char>(content);
            Options state = (Options)options.Clone();
            return ContinueRead(bufferState, state);
        }

        public T? Read(ReadOnlySpan<char> content, Options options)
        {
            IReadBuffer<char> bufferState = new ReadOnlySpanReadBuffer<char>(content);
            Options state = (Options)options.Clone();
            return ContinueRead(bufferState, state);
        }

        public T? Read(Memory<char> content, Options options)
        {
            IReadBuffer<char> bufferState = new ReadOnlyMemoryReadBuffer<char>(content);
            Options state = (Options)options.Clone();
            return ContinueRead(bufferState, state);
        }

        public T? Read(ReadOnlyMemory<char> content, Options options)
        {
            IReadBuffer<char> bufferState = new ReadOnlyMemoryReadBuffer<char>(content);
            Options state = (Options)options.Clone();
            return ContinueRead(bufferState, state);
        }

        public T? Read(TextReader reader, Options options)
        {
            Options state = (Options)options.Clone();
            IReadBuffer<char> bufferState = new CharReadBuffer(reader, state.BufferSize);
            try
            {
                while (true)
                {
                    bufferState.ReadNextBuffer();
                    T? value = ContinueRead(bufferState, state);

                    if (bufferState.IsFinalBlock)
                    {
                        return value;
                    }
                }
            }
            finally
            {
                bufferState.Dispose();
            }
        }

        protected abstract T? ContinueRead(IReadBuffer<char> bufferState, Options state);
    }
}
