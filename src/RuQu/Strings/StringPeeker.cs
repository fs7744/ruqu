namespace RuQu.Strings
{
    public class PeekString : IPeekSlice<char>
    {
        private string str;

        public PeekString(string str)
        {
            this.str = str;
        }

        public override string ToString()
        {
            return str;
        }
    }

    public class StringPeeker : IPeeker<char>
    {
        private int readed = -1;
        private readonly string str;

        public int Readed => readed;

        public StringPeeker(string data)
        {
            this.str = data;
        }

        public void Read(int count)
        {
            readed = Math.Min(readed + count, this.str.Length - 1);
        }

        public bool TryPeek(int count, out IPeekSlice<char> data)
        {
            return TryPeekOffset(1, count, out data);
        }

        public bool TryPeekOffset(int offset, int count, out IPeekSlice<char> data)
        {
            var i = readed + offset + count;
            if (i > str.Length)
            {
                data = null;
                return false;
            }
            data = new PeekString(str.Substring(readed + offset, count));
            return true;
        }

        public bool TryPeek(out char data)
        {
            return TryPeekOffset(1, out data);
        }

        public bool TryPeekOffset(int offset, out char data)
        {
            var i = readed + offset;
            if (i >= str.Length)
            {
                data = char.MinValue;
                return false;
            }
            data = str[i]; ;
            return true;
        }
    }
}