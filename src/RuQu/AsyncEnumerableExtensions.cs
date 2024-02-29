namespace RuQu
{
    public static class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> enumerable) =>
           new SynchronousAsyncEnumerable<T>(enumerable);

        private class SynchronousAsyncEnumerable<T> : IAsyncEnumerable<T>
        {
            private readonly IEnumerable<T> _enumerable;

            public SynchronousAsyncEnumerable(IEnumerable<T> enumerable) =>
                _enumerable = enumerable;

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
                new SynchronousAsyncEnumerator<T>(_enumerable.GetEnumerator());
        }

        private class SynchronousAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _enumerator;

            public T Current => _enumerator.Current;

            public SynchronousAsyncEnumerator(IEnumerator<T> enumerator) =>
                _enumerator = enumerator;

            public ValueTask DisposeAsync()
            {
                _enumerator.Dispose();
                return ValueTask.CompletedTask;
            }

            public ValueTask<bool> MoveNextAsync() =>
               ValueTask.FromResult(_enumerator.MoveNext());
        }
    }
}