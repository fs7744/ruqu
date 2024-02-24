namespace RuQu
{
    public interface IOptions<T>
    {
        public T WriteObject { get; }

        public IOptions<T> CloneReadOptions();

        public IOptions<T> CloneWriteOptionsWithValue(T writeObject);
    }
}