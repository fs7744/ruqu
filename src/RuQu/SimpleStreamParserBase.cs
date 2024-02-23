using RuQu.Reader;

namespace RuQu
{
    public abstract class SimpleStreamParserBase<T, Options> where Options : IOptions
    {
        #region Read

        public virtual async ValueTask<T?> ReadAsync(Stream stream, Options options, CancellationToken cancellationToken = default)
        {
            Options opt = (Options)options.Clone();
            IReadBuffer<byte> buffer = new ByteReadBuffer(stream, options.BufferSize);
            try
            {
                buffer = await buffer.ReadNextBufferAsync(cancellationToken).ConfigureAwait(false);
                HandleFirstBlock(buffer);
                do
                {
                    T? value = ContinueRead(buffer, opt);

                    if (buffer.IsFinalBlock)
                    {
                        return value;
                    }
                    buffer = await buffer.ReadNextBufferAsync(cancellationToken).ConfigureAwait(false);
                } while (true);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        public virtual ValueTask<T?> ReadAsync(byte[] content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<byte> buffer = new ArrayReadBuffer<byte>(content);
            Options opt = (Options)options.Clone();
            return ValueTask.FromResult(ContinueRead(buffer, opt));
        }

        public virtual ValueTask<T?> ReadAsync(Span<byte> content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<byte> buffer = new ReadOnlySpanReadBuffer<byte>(content);
            Options opt = (Options)options.Clone();
            return ValueTask.FromResult(ContinueRead(buffer, opt));
        }

        public virtual ValueTask<T?> ReadAsync(ReadOnlySpan<byte> content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<byte> buffer = new ReadOnlySpanReadBuffer<byte>(content);
            Options opt = (Options)options.Clone();
            return ValueTask.FromResult(ContinueRead(buffer, opt));
        }

        public virtual ValueTask<T?> ReadAsync(Memory<byte> content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<byte> buffer = new ReadOnlyMemoryReadBuffer<byte>(content);
            Options opt = (Options)options.Clone();
            return ValueTask.FromResult(ContinueRead(buffer, opt));
        }

        public virtual ValueTask<T?> ReadAsync(ReadOnlyMemory<byte> content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<byte> buffer = new ReadOnlyMemoryReadBuffer<byte>(content);
            Options opt = (Options)options.Clone();
            return ValueTask.FromResult(ContinueRead(buffer, opt));
        }

        public virtual T? Read(Stream stream, Options options)
        {
            Options opt = (Options)options.Clone();
            IReadBuffer<byte> buffer = new ByteReadBuffer(stream, options.BufferSize);
            try
            {
                buffer.ReadNextBuffer();
                HandleFirstBlock(buffer);
                do
                {
                    T? value = ContinueRead(buffer, opt);

                    if (buffer.IsFinalBlock)
                    {
                        return value;
                    }
                    buffer.ReadNextBuffer();
                } while (true);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        public virtual T? Read(byte[] content, Options options)
        {
            IReadBuffer<byte> buffer = new ArrayReadBuffer<byte>(content);
            Options opt = (Options)options.Clone();
            return ContinueRead(buffer, opt);
        }

        public virtual T? Read(Span<byte> content, Options options)
        {
            IReadBuffer<byte> buffer = new ReadOnlySpanReadBuffer<byte>(content);
            Options opt = (Options)options.Clone();
            return ContinueRead(buffer, opt);
        }

        public virtual T? Read(ReadOnlySpan<byte> content, Options options)
        {
            IReadBuffer<byte> buffer = new ReadOnlySpanReadBuffer<byte>(content);
            Options opt = (Options)options.Clone();
            return ContinueRead(buffer, opt);
        }

        public virtual T? Read(Memory<byte> content, Options options)
        {
            IReadBuffer<byte> buffer = new ReadOnlyMemoryReadBuffer<byte>(content);
            Options opt = (Options)options.Clone();
            return ContinueRead(buffer, opt);
        }

        public virtual T? Read(ReadOnlyMemory<byte> content, Options options)
        {
            IReadBuffer<byte> buffer = new ReadOnlyMemoryReadBuffer<byte>(content);
            Options opt = (Options)options.Clone();
            return ContinueRead(buffer, opt);
        }

        protected abstract T? ContinueRead(IReadBuffer<byte> buffer, Options options);

        protected virtual void HandleFirstBlock(IReadBuffer<byte> buffer)
        {
        }

        #endregion Read
    }
}