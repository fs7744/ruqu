using System.Runtime.CompilerServices;

namespace RuQu.Reader
{
    public interface IReadBuffer<T> : IDisposable
    {
        public ReadOnlySpan<T> Remaining { get; }

        public int RemainingCount{ get; }

        public bool IsFinalBlock { get; }

        public void Offset(int count);

        public void AdvanceBuffer(int bytesConsumed);

        public void ReadNextBuffer();

        public ValueTask<IReadBuffer<T>> ReadNextBufferAsync(CancellationToken cancellationToken);
    }
}