namespace RuQu.Strings
{
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

        public bool TryPeek(int count, out PeekSlice<char> data)
        {
            throw new NotImplementedException();
        }

        public bool TryPeek(out char data)
        {
            var i = readed + 1;
            if (i >= str.Length)
            {
                data = char.MinValue;
                return false;
            }
            data = str[i];
            return true;
        }
    }
}