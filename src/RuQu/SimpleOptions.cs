namespace RuQu
{
    public class SimpleOptions : IOptions
    {
        public int BufferSize { get; set; } = 4096;

        public IOptions Clone()
        {
            return this;
        }
    }
}