namespace RuQu
{
    public static unsafe partial class Bytes
    {
        public static Peeker<byte> AsBytePeeker(this byte[] bytes) => new(bytes);
    }
}