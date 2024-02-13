using RuQu.Strings;

namespace RuQu
{
    public static partial class Input
    {
        public static StringPeeker From(string str)
        { 
            return new StringPeeker(str);
        }
    }
}