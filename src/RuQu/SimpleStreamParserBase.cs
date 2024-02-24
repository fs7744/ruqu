using RuQu.Reader;
using RuQu.Writer;
using System.Text;

namespace RuQu
{
    public abstract class SimpleStreamParserBase<T, ReadState>
    {
        public int BufferSize { get; set; } = 4096;

        #region Read

        public virtual async ValueTask<T?> ReadAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            IReadBuffer<byte> buffer = new ByteReadBuffer(stream, BufferSize);
            var state = InitReadState();
            try
            {
                buffer = await buffer.ReadNextBufferAsync(cancellationToken).ConfigureAwait(false);
                HandleReadFirstBlock(buffer);
                do
                {
                    T? value = await ContinueReadAsync(buffer, ref state, cancellationToken);

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

        public virtual ValueTask<T?> ReadAsync(byte[] content, CancellationToken cancellationToken = default)
        {
            IReadBuffer<byte> buffer = new ArrayReadBuffer<byte>(content);

            var state = InitReadState();
            return ContinueReadAsync(buffer, ref state, cancellationToken);
        }

        public virtual ValueTask<T?> ReadAsync(Span<byte> content, CancellationToken cancellationToken = default)
        {
            IReadBuffer<byte> buffer = new ReadOnlySpanReadBuffer<byte>(content);

            var state = InitReadState();
            return ContinueReadAsync(buffer, ref state, cancellationToken);
        }

        public virtual ValueTask<T?> ReadAsync(ReadOnlySpan<byte> content, CancellationToken cancellationToken = default)
        {
            IReadBuffer<byte> buffer = new ReadOnlySpanReadBuffer<byte>(content);

            var state = InitReadState();
            return ContinueReadAsync(buffer, ref state, cancellationToken);
        }

        public virtual ValueTask<T?> ReadAsync(Memory<byte> content, CancellationToken cancellationToken = default)
        {
            IReadBuffer<byte> buffer = new ReadOnlyMemoryReadBuffer<byte>(content);

            var state = InitReadState();
            return ContinueReadAsync(buffer, ref state, cancellationToken);
        }

        public virtual ValueTask<T?> ReadAsync(ReadOnlyMemory<byte> content, CancellationToken cancellationToken = default)
        {
            IReadBuffer<byte> buffer = new ReadOnlyMemoryReadBuffer<byte>(content);

            var state = InitReadState();
            return ContinueReadAsync(buffer, ref state, cancellationToken);
        }

        public virtual T? Read(Stream stream)
        {
            IReadBuffer<byte> buffer = new ByteReadBuffer(stream, BufferSize);
            var state = InitReadState();
            try
            {
                buffer.ReadNextBuffer();
                HandleReadFirstBlock(buffer);
                do
                {
                    T? value = ContinueRead(buffer, ref state);

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

        public virtual T? Read(byte[] content)
        {
            IReadBuffer<byte> buffer = new ArrayReadBuffer<byte>(content);

            var state = InitReadState();
            return ContinueRead(buffer, ref state);
        }

        public virtual T? Read(Span<byte> content)
        {
            IReadBuffer<byte> buffer = new ReadOnlySpanReadBuffer<byte>(content);

            var state = InitReadState();
            return ContinueRead(buffer, ref state);
        }

        public virtual T? Read(ReadOnlySpan<byte> content)
        {
            IReadBuffer<byte> buffer = new ReadOnlySpanReadBuffer<byte>(content);

            var state = InitReadState();
            return ContinueRead(buffer, ref state);
        }

        public virtual T? Read(Memory<byte> content)
        {
            IReadBuffer<byte> buffer = new ReadOnlyMemoryReadBuffer<byte>(content);

            var state = InitReadState();
            return ContinueRead(buffer, ref state);
        }

        public virtual T? Read(ReadOnlyMemory<byte> content)
        {
            IReadBuffer<byte> buffer = new ReadOnlyMemoryReadBuffer<byte>(content);

            var state = InitReadState();
            return ContinueRead(buffer, ref state);
        }

        protected virtual ValueTask<T?> ContinueReadAsync(IReadBuffer<byte> buffer, ref ReadState state, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(ContinueRead(buffer, ref state));
        }

        protected abstract T? ContinueRead(IReadBuffer<byte> buffer, ref ReadState state);

        protected virtual void HandleReadFirstBlock(IReadBuffer<byte> buffer)
        {
        }

        protected abstract ReadState InitReadState();

        #endregion Read

        #region Write

        public virtual async ValueTask WriteAsync(T value, Stream stream, CancellationToken cancellationToken = default)
        {
            await foreach (var item in ContinueWriteAsync(value, cancellationToken).ConfigureAwait(false))
            {
                await stream.WriteAsync(item, cancellationToken).ConfigureAwait(false);
            }
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        public ValueTask<string> WriteToUTF8StringAsync(T value, CancellationToken cancellationToken = default) => WriteToAsync<string>(value, i => ValueTask.FromResult(Encoding.UTF8.GetString(i.Span)), cancellationToken);

        public virtual async ValueTask<R> WriteToAsync<R>(T value, Func<ReadOnlyMemory<byte>, ValueTask<R>> convert, CancellationToken cancellationToken = default)
        {
            using var bufferWriter = new PooledBufferWriter<byte>(BufferSize);
            await foreach (var item in ContinueWriteAsync(value, cancellationToken).ConfigureAwait(false))
            {
                var a = item;
                var des = bufferWriter.GetMemory(a.Length);
                a.CopyTo(des);
                bufferWriter.Advance(a.Length);
            }
            return await convert(bufferWriter.WrittenMemory);
        }

        public virtual async ValueTask<byte[]> WriteToBytesAsync(T value, CancellationToken cancellationToken = default)
        {
            using var bufferWriter = new PooledBufferWriter<byte>(BufferSize);
            await foreach (var item in ContinueWriteAsync(value, cancellationToken).ConfigureAwait(false))
            {
                var a = item;
                var des = bufferWriter.GetMemory(a.Length);
                a.CopyTo(des);
                bufferWriter.Advance(a.Length);
            }
            return bufferWriter.WrittenMemory.ToArray();
        }

        public virtual void Write(T value, Stream stream)
        {
            foreach (var item in ContinueWrite(value))
            {
                stream.Write(item.Span);
            }
            stream.Flush();
        }

        public virtual byte[] WriteToBytes(T value)
        {
            using var bufferWriter = new PooledBufferWriter<byte>(BufferSize);

            foreach (var item in ContinueWrite(value))
            {
                var a = item.Span;
                var des = bufferWriter.GetSpan(a.Length);
                a.CopyTo(des);
                bufferWriter.Advance(a.Length);
            }
            return bufferWriter.WrittenMemory.ToArray();
        }

        public string WriteToUTF8String(T value) => WriteTo<string>(value, i => Encoding.UTF8.GetString(i.Span));

        public virtual R WriteTo<R>(T value, Func<ReadOnlyMemory<byte>, R> convert)
        {
            using var bufferWriter = new PooledBufferWriter<byte>(BufferSize);

            foreach (var item in ContinueWrite(value))
            {
                var a = item.Span;
                var des = bufferWriter.GetSpan(a.Length);
                a.CopyTo(des);
                bufferWriter.Advance(a.Length);
            }
            return convert(bufferWriter.WrittenMemory);
        }

        protected virtual IAsyncEnumerable<ReadOnlyMemory<byte>> ContinueWriteAsync(T value, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ContinueWrite(value).ToAsyncEnumerable();
        }

        protected abstract IEnumerable<ReadOnlyMemory<byte>> ContinueWrite(T value);

        #endregion Write
    }
}