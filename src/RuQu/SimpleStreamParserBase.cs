using RuQu.Reader;
using RuQu.Writer;
using System.Text;

namespace RuQu
{
    public abstract class SimpleStreamParserBase<T>
    {
        public int BufferSize { get; set; } = 4096;

        #region Read

        public virtual async ValueTask<T?> ReadAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            IReaderBuffer<byte> buffer = new StreamReaderBuffer(stream, BufferSize);
            try
            {
                return await ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        public virtual async ValueTask<T?> ReadAsync(byte[] content, CancellationToken cancellationToken = default)
        {
            IReaderBuffer<byte> buffer = new ArrayReaderBuffer<byte>(content);
            try
            {
                return await ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        public virtual async ValueTask<T?> ReadAsync(Memory<byte> content, CancellationToken cancellationToken = default)
        {
            IReaderBuffer<byte> buffer = new ReadOnlyMemoryReaderBuffer<byte>(content);
            try
            {
                return await ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        public virtual async ValueTask<T?> ReadAsync(ReadOnlyMemory<byte> content, CancellationToken cancellationToken = default)
        {
            IReaderBuffer<byte> buffer = new ReadOnlyMemoryReaderBuffer<byte>(content);
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
            IReaderBuffer<byte> buffer = new StreamReaderBuffer(stream, BufferSize);
            try
            {
                return Read(buffer);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        public virtual T? Read(byte[] content)
        {
            var buffer = new ArrayReaderBuffer<byte>(content);
            try
            {
                return Read(buffer);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        public virtual T? Read(Span<byte> content)
        {
            var buffer = new ReadOnlySpanReaderBuffer<byte>(content);
            try
            {
                return Read(buffer);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        public virtual T? Read(ReadOnlySpan<byte> content)
        {
            var buffer = new ReadOnlySpanReaderBuffer<byte>(content);
            try
            {
                return Read(buffer);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        public virtual T? Read(Memory<byte> content)
        {
            var buffer = new ReadOnlyMemoryReaderBuffer<byte>(content);
            try
            {
                return Read(buffer);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        public virtual T? Read(ReadOnlyMemory<byte> content)
        {
            var buffer = new ReadOnlyMemoryReaderBuffer<byte>(content);
            try
            {
                return Read(buffer);
            }
            finally
            {
                buffer.Dispose();
            }
        }

        protected virtual ValueTask<T?> ReadAsync(IReaderBuffer<byte> buffer, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(Read(buffer));
        }

        protected abstract T? Read(IReaderBuffer<byte> buffer);

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