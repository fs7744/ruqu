namespace RuQu
{
    public interface IOptions
    {
        public int BufferSize { get; set; }

        public IOptions Clone();
    }
}