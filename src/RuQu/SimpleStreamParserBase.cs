using RuQu.Reader;
using System.Reflection.PortableExecutable;

namespace RuQu
{
    public interface IReadOptions
    {
        public int BufferSize { get; set; }
    }

    public struct IntState
    {
        public int CurrentState;
    }

    public class SimpleReadOptions : IReadOptions
    {
        public int BufferSize { get; set; } = 4096;
    }

    public abstract class SimpleStreamParserBase<T, Options, State> where State : new() where Options : IReadOptions
    {
        public async ValueTask<T?> ReadAsync(Stream stream, Options options, CancellationToken cancellationToken = default)
        {
            var bufferState = new ByteReadBuffer(stream, options.BufferSize);
            State state = new();
            try
            {
                bufferState = (ByteReadBuffer)await bufferState.ReadNextBufferAsync(cancellationToken).ConfigureAwait(false);
                HandleFirstBlock(ref bufferState);
                do
                {
                    T? value = ContinueRead(ref bufferState, ref state);

                    if (bufferState.IsFinalBlock)
                    {
                        return value;
                    }
                    bufferState = (ByteReadBuffer)await bufferState.ReadNextBufferAsync(cancellationToken).ConfigureAwait(false);
                } while (true);
            }
            finally
            {
                bufferState.Dispose();
            }
        }

        public T? Read(Stream stream, Options options)
        {
            var bufferState = new ByteReadBuffer(stream, options.BufferSize);
            State state = new();
            try
            {
                bufferState.ReadNextBuffer();
                HandleFirstBlock(ref bufferState);
                do
                {
                    T? value = ContinueRead(ref bufferState, ref state);

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

        protected abstract T? ContinueRead(ref ByteReadBuffer bufferState, ref State state);

        protected virtual void HandleFirstBlock(ref ByteReadBuffer bufferState)
        {
        }
    }
}