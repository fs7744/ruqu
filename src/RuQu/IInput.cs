using RuQu.Strings;

namespace RuQu
{
    public interface IPeeker<T>
    {
        int Readed { get; }

        bool TryPeek(int count, out PeekSlice<T> data);

        bool TryPeek(out T data);

        void Read(int count);
    }

    public ref struct PeekSlice<T>
    {
    }
}