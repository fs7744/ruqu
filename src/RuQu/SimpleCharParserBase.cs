using RuQu.Reader;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;

namespace RuQu
{
    public abstract class SimpleCharParserBase<T, Options, State> where State : new() where Options : IReadOptions
    {
        public async ValueTask<T?> ReadAsync(TextReader reader, Options options, CancellationToken cancellationToken = default)
        {
            IReadBuffer<char> bufferState = new CharReadBuffer(reader, options.BufferSize);
            State state = new();
            try
            {
                while (true)
                {
                    bufferState = await bufferState.ReadNextBufferAsync(cancellationToken).ConfigureAwait(false);
                    T? value = ContinueRead(ref bufferState, ref state);

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
            State state = new();
            return ValueTask.FromResult(ContinueRead(ref bufferState, ref state));
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
            State state = new();
            return ContinueRead(ref bufferState, ref state);
        }

        public T? Read(TextReader reader, Options options)
        {
            IReadBuffer<char> bufferState = new CharReadBuffer(reader, options.BufferSize);
            State state = new();
            try
            {
                while (true)
                {
                    bufferState.ReadNextBuffer();
                    T? value = ContinueRead(ref bufferState, ref state);

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

        protected abstract T? ContinueRead(ref IReadBuffer<char> bufferState, ref State state);
    }
}
