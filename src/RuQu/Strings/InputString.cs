namespace RuQu
{
    public class InputString : IInput<string, char>
    {
        private readonly string data;

        public InputString(string data)
        {
            this.data = data;
        }

        public int Index { get; private set; }

        public char Current => data[Index];

        public bool MoveNext()
        {
            Index++;
            if (Index >= data.Length)
            {
                Index = data.Length;
                return false;
            }
            return true;
        }

        public string TakeString(int count)
        {
            if (count <= 0) return string.Empty;
            var r = data.Substring(Index, count);
            Index += r.Length;
            return r;
        }
    }
}