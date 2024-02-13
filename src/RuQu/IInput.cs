namespace RuQu
{
    public interface IPeeker<T>
    { 
        int Readed { get; }
        bool TryPeek(int count, out T data);
        bool Read(int count);
    }

    public interface IInput<Y>
    {
        Y Current { get; }
        int Index { get; }

        bool MoveNext();

        bool IsEof { get; }
    }
}