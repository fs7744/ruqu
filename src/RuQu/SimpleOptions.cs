namespace RuQu
{
    public class SimpleOptions<T> : IOptions<T>
    {
        public int BufferSize { get; set; } = 4096;
        public T WriteObject { get; private set; }

        public IOptions<T> CloneReadOptions()
        {
            return this;
        }

        public IOptions<T> CloneWriteOptionsWithValue(T writeObject)
        {
            WriteObject = writeObject;
            return this;
        }
    }
}