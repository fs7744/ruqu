﻿using RuQu.Reader;

namespace RuQu
{
    public abstract class SimpleCharParserBase<T, Options> where Options : IOptions
    {
        #region Read

        public virtual async ValueTask<T?> ReadAsync(TextReader reader, Options options, CancellationToken cancellationToken = default)
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
            IReadBuffer<char> bufferState = new StringReadBuffer(content);
            Options state = (Options)options.Clone();
            return ValueTask.FromResult(ContinueRead(bufferState, state));
        }

        public virtual ValueTask<T?> ReadAsync(char[] content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> bufferState = new ArrayReadBuffer<char>(content);
            Options state = (Options)options.Clone();
            return ValueTask.FromResult(ContinueRead(bufferState, state));
        }

        public virtual ValueTask<T?> ReadAsync(Span<char> content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> bufferState = new ReadOnlySpanReadBuffer<char>(content);
            Options state = (Options)options.Clone();
            return ValueTask.FromResult(ContinueRead(bufferState, state));
        }

        public virtual ValueTask<T?> ReadAsync(ReadOnlySpan<char> content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> bufferState = new ReadOnlySpanReadBuffer<char>(content);
            Options state = (Options)options.Clone();
            return ValueTask.FromResult(ContinueRead(bufferState, state));
        }

        public virtual ValueTask<T?> ReadAsync(Memory<char> content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> bufferState = new ReadOnlyMemoryReadBuffer<char>(content);
            Options state = (Options)options.Clone();
            return ValueTask.FromResult(ContinueRead(bufferState, state));
        }

        public virtual ValueTask<T?> ReadAsync(ReadOnlyMemory<char> content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> bufferState = new ReadOnlyMemoryReadBuffer<char>(content);
            Options state = (Options)options.Clone();
            return ValueTask.FromResult(ContinueRead(bufferState, state));
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
            IReadBuffer<char> bufferState = new StringReadBuffer(content);
            Options state = (Options)options.Clone();
            return ContinueRead(bufferState, state);
        }

        public virtual T? Read(char[] content, Options options)
        {
            IReadBuffer<char> bufferState = new ArrayReadBuffer<char>(content);
            Options state = (Options)options.Clone();
            return ContinueRead(bufferState, state);
        }

        public virtual T? Read(Span<char> content, Options options)
        {
            IReadBuffer<char> bufferState = new ReadOnlySpanReadBuffer<char>(content);
            Options state = (Options)options.Clone();
            return ContinueRead(bufferState, state);
        }

        public virtual T? Read(ReadOnlySpan<char> content, Options options)
        {
            IReadBuffer<char> bufferState = new ReadOnlySpanReadBuffer<char>(content);
            Options state = (Options)options.Clone();
            return ContinueRead(bufferState, state);
        }

        public virtual T? Read(Memory<char> content, Options options)
        {
            IReadBuffer<char> bufferState = new ReadOnlyMemoryReadBuffer<char>(content);
            Options state = (Options)options.Clone();
            return ContinueRead(bufferState, state);
        }

        public virtual T? Read(ReadOnlyMemory<char> content, Options options)
        {
            IReadBuffer<char> bufferState = new ReadOnlyMemoryReadBuffer<char>(content);
            Options state = (Options)options.Clone();
            return ContinueRead(bufferState, state);
        }

        public virtual T? Read(TextReader reader, Options options)
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

        #endregion Read
    }
}