using RuQu.Reader;
using RuQu.Writer;
using System.Text;

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
            Options opt = (Options)options.CloneWriteOptionsWithValue(value);
            await foreach (var item in ContinueWriteAsync(opt, cancellationToken).ConfigureAwait(false))
            {
                await stream.WriteAsync(item, cancellationToken).ConfigureAwait(false);
            }
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        public ValueTask<string> WriteToUTF8StringAsync(T value, Options options, CancellationToken cancellationToken = default) => WriteToAsync<string>(value, options, i => ValueTask.FromResult(Encoding.UTF8.GetString(i.Span)), cancellationToken);

        public virtual async ValueTask<R> WriteToAsync<R>(T value, Options options, Func<ReadOnlyMemory<byte>, ValueTask<R>> convert, CancellationToken cancellationToken = default)
        {
            Options opt = (Options)options.CloneWriteOptionsWithValue(value);
            using var bufferWriter = new PooledBufferWriter<byte>(opt.BufferSize);
            await foreach (var item in ContinueWriteAsync(opt, cancellationToken).ConfigureAwait(false))
            {
                var a = item;
                var des = bufferWriter.GetMemory(a.Length);
                a.CopyTo(des);
                bufferWriter.Advance(a.Length);
            }
            return await convert(bufferWriter.WrittenMemory);
        }

        public virtual async ValueTask<byte[]> WriteToBytesAsync(T value, Options options, CancellationToken cancellationToken = default)
        {
            Options opt = (Options)options.CloneWriteOptionsWithValue(value);
            using var bufferWriter = new PooledBufferWriter<byte>(opt.BufferSize);
            await foreach (var item in ContinueWriteAsync(opt, cancellationToken).ConfigureAwait(false))
            {
                var a = item;
                var des = bufferWriter.GetMemory(a.Length);
                a.CopyTo(des);
                bufferWriter.Advance(a.Length);
            }
            return bufferWriter.WrittenMemory.ToArray();
        }

        public virtual void Write(T value, Stream stream, Options options)
        {
            Options opt = (Options)options.CloneWriteOptionsWithValue(value);
            foreach (var item in ContinueWrite(opt))
            {
                stream.Write(item.Span);
            }
            stream.Flush();
        }

        public virtual byte[] WriteToBytes(T value, Options options)
        {
            using var bufferWriter = new PooledBufferWriter<byte>(options.BufferSize);
            Options opt = (Options)options.CloneWriteOptionsWithValue(value);
            foreach (var item in ContinueWrite(opt))
            {
                var a = item.Span;
                var des = bufferWriter.GetSpan(a.Length);
                a.CopyTo(des);
                bufferWriter.Advance(a.Length);
            }
            return bufferWriter.WrittenMemory.ToArray();
        }

        public string WriteToUTF8String(T value, Options options) => WriteTo<string>(value, options, i => Encoding.UTF8.GetString(i.Span));

        public virtual R WriteTo<R>(T value, Options options, Func<ReadOnlyMemory<byte>, R> convert)
        {
            using var bufferWriter = new PooledBufferWriter<byte>(options.BufferSize);
            Options opt = (Options)options.CloneWriteOptionsWithValue(value);
            foreach (var item in ContinueWrite(opt))
            {
                var a = item.Span;
                var des = bufferWriter.GetSpan(a.Length);
                a.CopyTo(des);
                bufferWriter.Advance(a.Length);
            }
            return convert(bufferWriter.WrittenMemory);
        }

        public virtual IAsyncEnumerable<ReadOnlyMemory<byte>> ContinueWriteAsync(Options options, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ContinueWrite(options).ToAsyncEnumerable();
        }

        public abstract IEnumerable<ReadOnlyMemory<byte>> ContinueWrite(Options options);

        #endregion Write
    }
}