namespace RuQu
{
    public unsafe static partial class PeekerExtensions
    {
        public static bool Is<T>(ref Peeker<T> peeker, Func<T, bool> predicate, out T c)
        {
            if (peeker.TryPeek(out c) && predicate(c))
            {
                peeker.Read(1);
                return true;
            }
            return false;
        }

        public static bool Is<T>(ref Peeker<T> peeker, delegate*<T, bool> predicate, out T c)
        {
            if (peeker.TryPeek(out c) && predicate(c))
            {
                peeker.Read(1);
                return true;
            }
            return false;
        }

        public static bool IsAny<T>(ref Peeker<T> peeker, out T c)
        {
            if (peeker.TryPeek(out c))
            {
                peeker.Read(1);
                return true;
            }
            return false;
        }
    }
}