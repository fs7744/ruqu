namespace RuQu
{
    public interface IInput
    {
        int Index { get; }

        bool MoveNext();
    }

    public interface IInput<X, Y> : IInput where X : IEnumerable<Y>
    {
        Y Current { get; }
    }
}