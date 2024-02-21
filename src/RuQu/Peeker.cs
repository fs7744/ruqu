namespace RuQu
{

    public interface IPeeker<T>
    {
        public int Length { get; }

        public T this[int index] { get; }

        public bool TryPeekOffset(int offset, out T data);

        public bool TryPeekOffset(int offset, int count, out ReadOnlySpan<T> data);

        public void Read(int count);
    }
}