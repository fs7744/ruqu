//using System.Collections;
//using System.Threading;

//namespace RuQu.Reader
//{
//    public abstract class TextDataReader<Row> : IDisposable, IEnumerable<Row>, IAsyncEnumerable<Row>
//    {
//        protected IReadBuffer<char> reader;
//        private object? enumerator;

//        public TextDataReader(TextReader reader, int bufferSize)
//        {
//            this.reader = new CharReadBuffer(reader, bufferSize);
//        }

//        public void Dispose()
//        {
//            if (reader != null)
//            {
//                reader.Dispose();
//                reader = null;
//            }
//            enumerator = null;
//        }

//        protected IEnumerable<Row> Read()
//        {
//            try
//            {
//                while (true)
//                {
//                    if (!reader.IsFinalBlock)
//                    {
//                        reader.ReadNextBuffer();
//                    }
//                    if (ContinueRead(reader, out var row))
//                    {
//                        yield return row;
//                    }
//                    else if (reader.IsFinalBlock && reader.RemainingCount == 0)
//                    {
//                        break;
//                    }
//                }
//            }
//            finally
//            {
//                reader.Dispose();
//            }
//        }

//        protected async IAsyncEnumerator<Row> ReadAsync(CancellationToken cancellationToken = default)
//        {
//            try
//            {
//                while (true)
//                {
//                    if (!reader.IsFinalBlock)
//                    {
//                        await reader.ReadNextBufferAsync(cancellationToken);
//                    }
//                    if (await ContinueReadAsync(reader, out var row, cancellationToken))
//                    {
//                        yield return row;
//                    }
//                    else if (reader.IsFinalBlock && reader.RemainingCount == 0)
//                    {
//                        break;
//                    }
//                }
//            }
//            finally
//            {
//                reader.Dispose();
//            }
//        }

//        protected abstract bool ContinueRead(IReadBuffer<char> reader, out Row? row);

//        protected virtual ValueTask<bool> ContinueReadAsync(IReadBuffer<char> reader, out Row? row, CancellationToken cancellationToken)
//        {
//            cancellationToken.ThrowIfCancellationRequested();
//            return ValueTask.FromResult(ContinueRead(reader, out row));
//        }

//        public IEnumerator<Row> GetEnumerator()
//        {
//            if (enumerator == null)
//            {
//                enumerator = Read().GetEnumerator();
//            }
//            if (enumerator is IEnumerator<Row> e)
//            {
//                return e;
//            }
//            throw new InvalidOperationException("Read by async mode");
//        }

//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return GetEnumerator();
//        }

//        public IAsyncEnumerator<Row> GetAsyncEnumerator(CancellationToken cancellationToken = default)
//        {
//            if (enumerator == null)
//            {
//                enumerator = ReadAsync(cancellationToken);
//            }
//            if (enumerator is IAsyncEnumerator<Row> e)
//            {
//                return e;
//            }
//            throw new InvalidOperationException("Read by sync mode");
//        }
//    }
//}