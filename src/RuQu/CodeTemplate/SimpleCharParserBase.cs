using RuQu.Reader;

namespace RuQu.CodeTemplate
{
    public abstract class SimpleCharParserBase<T, Options, State> where State : new() where Options : IReadOptions
    {
        public async ValueTask<T?> ReadAsync(TextReader reader, Options options, CancellationToken cancellationToken = default)
        {
            var bufferState = new ReadCharBuffer(options.BufferSize);
            State state = new();
            try
            {
                while (true)
                {
                    bufferState = await bufferState.ReadFromStreamAsync(reader, cancellationToken).ConfigureAwait(false);
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
            return ReadAsync(new StringReader(content), options, cancellationToken);
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
            return Read(new StringReader(content), options);
        }

        public T? Read(TextReader reader, Options options)
        {
            var bufferState = new ReadCharBuffer(options.BufferSize);
            State state = new();
            try
            {
                while (true)
                {
                    bufferState.ReadFromStream(reader);
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

        protected abstract T? ContinueRead(ref ReadCharBuffer bufferState, ref State state);
    }
}
