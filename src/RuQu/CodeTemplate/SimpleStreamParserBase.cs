using RuQu.Reader;

namespace RuQu.CodeTemplate
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
            var bufferState = new ReadBuffer(options.BufferSize);
            State state = new();
            try
            {
                bufferState = await bufferState.ReadFromStreamAsync(stream, cancellationToken).ConfigureAwait(false);
                HandleFirstBlock(ref bufferState);
                do
                {
                    T? value = ContinueRead(ref bufferState, ref state);

                    if (bufferState.IsFinalBlock)
                    {
                        return value;
                    }
                    bufferState = await bufferState.ReadFromStreamAsync(stream, cancellationToken).ConfigureAwait(false);
                } while (true);
            }
            finally
            {
                bufferState.Dispose();
            }
        }

        public T? Read(Stream stream, Options options)
        {
            var bufferState = new ReadBuffer(options.BufferSize);
            State state = new();
            try
            {
                bufferState.ReadFromStream(stream);
                HandleFirstBlock(ref bufferState);
                do
                {
                    T? value = ContinueRead(ref bufferState, ref state);

                    if (bufferState.IsFinalBlock)
                    {
                        return value;
                    }
                    bufferState.ReadFromStream(stream);
                } while (true);
            }
            finally
            {
                bufferState.Dispose();
            }
        }

        protected abstract T? ContinueRead(ref ReadBuffer bufferState, ref State state);

        protected virtual void HandleFirstBlock(ref ReadBuffer bufferState)
        {
        }
    }
}