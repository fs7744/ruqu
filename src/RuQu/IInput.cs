namespace RuQu
{
    public interface IInput<Y>
    {
        Y Current { get; }
        int Index { get; }

        bool MoveNext();

        bool IsEof { get; }
    }
}