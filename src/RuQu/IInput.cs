namespace RuQu
{
    public interface IPeeker<T>
    {
        int Readed { get; }

        bool TryPeek(int count, out IPeekSlice<T> data);

        bool TryPeek(out T data);

        void Read(int count);
    }

    public interface IPeekSlice<T>
    {
    }
}