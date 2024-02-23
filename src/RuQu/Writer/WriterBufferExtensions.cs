using RuQu.Writer;

namespace RuQu
{
    public static class WriterBufferExtensions
    {
        public static ValueTask WriteToStreamAsync(this PooledBufferWriter<byte> buffer, Stream destination, CancellationToken cancellationToken)
        {
            return destination.WriteAsync(buffer.WrittenMemory, cancellationToken);
        }

        public static void WriteToStream(this PooledBufferWriter<byte> buffer, Stream destination)
        {
            destination.Write(buffer.WrittenMemory.Span);
        }
    }
}