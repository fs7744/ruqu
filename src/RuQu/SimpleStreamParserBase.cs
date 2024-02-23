using RuQu.Reader;
using RuQu.Writer;
using System.Buffers;

namespace RuQu
{
    public abstract class SimpleStreamParserBase<T, Options> where Options : IOptions<T>
    {
        #region Read

        public virtual async ValueTask<T?> ReadAsync(Stream stream, Options options, CancellationToken cancellationToken = default)
        {
            Options opt = (Options)options.CloneReadOptions();
            IReadBuffer<byte> buffer = new ByteReadBuffer(stream, options.BufferSize);
            try
            {
                buffer = await buffer.ReadNextBufferAsync(cancellationToken).ConfigureAwait(false);
                HandleReadFirstBlock(buffer);
                do
                {
                    T? value = await ContinueReadAsync(buffer, opt, cancellationToken);

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
            Options opt = (Options)options.CloneReadOptions();
            return ContinueReadAsync(buffer, opt, cancellationToken);
        }

        public virtual ValueTask<T?> ReadAsync(Span<byte> content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<byte> buffer = new ReadOnlySpanReadBuffer<byte>(content);
            Options opt = (Options)options.CloneReadOptions();
            return ContinueReadAsync(buffer, opt, cancellationToken);
        }

        public virtual ValueTask<T?> ReadAsync(ReadOnlySpan<byte> content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<byte> buffer = new ReadOnlySpanReadBuffer<byte>(content);
            Options opt = (Options)options.CloneReadOptions();
            return ContinueReadAsync(buffer, opt, cancellationToken);
        }

        public virtual ValueTask<T?> ReadAsync(Memory<byte> content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<byte> buffer = new ReadOnlyMemoryReadBuffer<byte>(content);
            Options opt = (Options)options.CloneReadOptions();
            return ContinueReadAsync(buffer, opt, cancellationToken);
        }

        public virtual ValueTask<T?> ReadAsync(ReadOnlyMemory<byte> content, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<byte> buffer = new ReadOnlyMemoryReadBuffer<byte>(content);
            Options opt = (Options)options.CloneReadOptions();
            return ContinueReadAsync(buffer, opt, cancellationToken);
        }

        public virtual T? Read(Stream stream, Options options)
        {
            Options opt = (Options)options.CloneReadOptions();
            IReadBuffer<byte> buffer = new ByteReadBuffer(stream, options.BufferSize);
            try
            {
                buffer.ReadNextBuffer();
                HandleReadFirstBlock(buffer);
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
            Options opt = (Options)options.CloneReadOptions();
            return ContinueRead(buffer, opt);
        }

        public virtual T? Read(Span<byte> content, Options options)
        {
            IReadBuffer<byte> buffer = new ReadOnlySpanReadBuffer<byte>(content);
            Options opt = (Options)options.CloneReadOptions();
            return ContinueRead(buffer, opt);
        }

        public virtual T? Read(ReadOnlySpan<byte> content, Options options)
        {
            IReadBuffer<byte> buffer = new ReadOnlySpanReadBuffer<byte>(content);
            Options opt = (Options)options.CloneReadOptions();
            return ContinueRead(buffer, opt);
        }

        public virtual T? Read(Memory<byte> content, Options options)
        {
            IReadBuffer<byte> buffer = new ReadOnlyMemoryReadBuffer<byte>(content);
            Options opt = (Options)options.CloneReadOptions();
            return ContinueRead(buffer, opt);
        }

        public virtual T? Read(ReadOnlyMemory<byte> content, Options options)
        {
            IReadBuffer<byte> buffer = new ReadOnlyMemoryReadBuffer<byte>(content);
            Options opt = (Options)options.CloneReadOptions();
            return ContinueRead(buffer, opt);
        }

        protected virtual ValueTask<T?> ContinueReadAsync(IReadBuffer<byte> buffer, Options options, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(ContinueRead(buffer, options));
        }

        protected abstract T? ContinueRead(IReadBuffer<byte> buffer, Options options);

        protected virtual void HandleReadFirstBlock(IReadBuffer<byte> buffer)
        {
        }

        #endregion Read

        #region Write

        public virtual async ValueTask WriteAsync(T value, Stream stream, Options options, CancellationToken cancellationToken = default)
        {
            bool isFinalBlock;
            using var bufferWriter = new PooledByteBufferWriter(options.BufferSize);
            do
            {
                isFinalBlock = await ContinueWriteAsync(bufferWriter, options, cancellationToken);
                await bufferWriter.WriteToStreamAsync(stream, cancellationToken).ConfigureAwait(false);
                bufferWriter.Clear();
            }
            while (!isFinalBlock);
        }

        public virtual void Write(T value, Stream stream, Options options)
        {
            bool isFinalBlock;
            using var bufferWriter = new PooledByteBufferWriter(options.BufferSize);
            do
            {
                isFinalBlock = ContinueWrite(bufferWriter, options);
                bufferWriter.WriteToStream(stream);
                bufferWriter.Clear();
            }
            while (!isFinalBlock);
        }

        public virtual ValueTask<bool> ContinueWriteAsync(IBufferWriter<byte> writer, Options options, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(ContinueWrite(writer, options));
        }

        public abstract bool ContinueWrite(IBufferWriter<byte> writer, Options options);

        #endregion Write
    }
}