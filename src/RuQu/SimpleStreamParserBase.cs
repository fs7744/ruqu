﻿using RuQu.Reader;

namespace RuQu
{
    public abstract class SimpleStreamParserBase<T, Options> where Options : IOptions
    {
        #region Read

        public virtual async ValueTask<T?> ReadAsync(Stream stream, Options options, CancellationToken cancellationToken = default)
        {
            Options state = (Options)options.Clone();
            IReadBuffer<byte> bufferState = new ByteReadBuffer(stream, state.BufferSize);
            try
            {
                bufferState = await bufferState.ReadNextBufferAsync(cancellationToken).ConfigureAwait(false);
                HandleFirstBlock(bufferState);
                do
                {
                    T? value = ContinueRead(bufferState, state);

                    if (bufferState.IsFinalBlock)
                    {
                        return value;
                    }
                    bufferState = await bufferState.ReadNextBufferAsync(cancellationToken).ConfigureAwait(false);
                } while (true);
            }
            finally
            {
                bufferState.Dispose();
            }
        }

        public virtual ValueTask<T?> ReadAsync(byte[] content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<byte> bufferState = new ArrayReadBuffer<byte>(content);
            Options state = (Options)options.Clone();
            return ValueTask.FromResult(ContinueRead(bufferState, state));
        }

        public virtual ValueTask<T?> ReadAsync(Span<byte> content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<byte> bufferState = new ReadOnlySpanReadBuffer<byte>(content);
            Options state = (Options)options.Clone();
            return ValueTask.FromResult(ContinueRead(bufferState, state));
        }

        public virtual ValueTask<T?> ReadAsync(ReadOnlySpan<byte> content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<byte> bufferState = new ReadOnlySpanReadBuffer<byte>(content);
            Options state = (Options)options.Clone();
            return ValueTask.FromResult(ContinueRead(bufferState, state));
        }

        public virtual ValueTask<T?> ReadAsync(Memory<byte> content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<byte> bufferState = new ReadOnlyMemoryReadBuffer<byte>(content);
            Options state = (Options)options.Clone();
            return ValueTask.FromResult(ContinueRead(bufferState, state));
        }

        public virtual ValueTask<T?> ReadAsync(ReadOnlyMemory<byte> content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<byte> bufferState = new ReadOnlyMemoryReadBuffer<byte>(content);
            Options state = (Options)options.Clone();
            return ValueTask.FromResult(ContinueRead(bufferState, state));
        }

        public virtual T? Read(Stream stream, Options options)
        {
            Options state = (Options)options.Clone();
            IReadBuffer<byte> bufferState = new ByteReadBuffer(stream, state.BufferSize);
            try
            {
                bufferState.ReadNextBuffer();
                HandleFirstBlock(bufferState);
                do
                {
                    T? value = ContinueRead(bufferState, state);

                    if (bufferState.IsFinalBlock)
                    {
                        return value;
                    }
                    bufferState.ReadNextBuffer();
                } while (true);
            }
            finally
            {
                bufferState.Dispose();
            }
        }

        public virtual T? Read(byte[] content, Options options)
        {
            IReadBuffer<byte> bufferState = new ArrayReadBuffer<byte>(content);
            Options state = (Options)options.Clone();
            return ContinueRead(bufferState, state);
        }

        public virtual T? Read(Span<byte> content, Options options)
        {
            IReadBuffer<byte> bufferState = new ReadOnlySpanReadBuffer<byte>(content);
            Options state = (Options)options.Clone();
            return ContinueRead(bufferState, state);
        }

        public virtual T? Read(ReadOnlySpan<byte> content, Options options)
        {
            IReadBuffer<byte> bufferState = new ReadOnlySpanReadBuffer<byte>(content);
            Options state = (Options)options.Clone();
            return ContinueRead(bufferState, state);
        }

        public virtual T? Read(Memory<byte> content, Options options)
        {
            IReadBuffer<byte> bufferState = new ReadOnlyMemoryReadBuffer<byte>(content);
            Options state = (Options)options.Clone();
            return ContinueRead(bufferState, state);
        }

        public virtual T? Read(ReadOnlyMemory<byte> content, Options options)
        {
            IReadBuffer<byte> bufferState = new ReadOnlyMemoryReadBuffer<byte>(content);
            Options state = (Options)options.Clone();
            return ContinueRead(bufferState, state);
        }

        protected abstract T? ContinueRead(IReadBuffer<byte> bufferState, Options state);

        protected virtual void HandleFirstBlock(IReadBuffer<byte> bufferState)
        {
        }

        #endregion Read
    }
}