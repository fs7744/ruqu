namespace RuQu
{
    public static class Ini
    {
        public static IDictionary<string, string> Parse(string content)
        {
            var input = content.AsCharPeeker();
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            while (input.TryPeek(out var v))
            {
                if (!(WhiteSpace(ref input) || Comment(ref input) || Section(ref input, dict)))
                {
                    throw new NotSupportedException(v.ToString());
                }
            }
            return dict;
        }

        private static bool WhiteSpace(ref Peeker<char> input)
        {
            return input.TakeWhiteSpace(out var _);
        }

        private static unsafe bool Comment(ref Peeker<char> input)
        {
            return input.IsIn(";#/", out var _) && input.TakeLine(out var _);
        }

        private static bool Section(ref Peeker<char> input, Dictionary<string, string> dict)
        {
            var name = SectionName(ref input);
            if (name == null) return false;
            while (input.IsNot('[', out var _))
            {
                if (WhiteSpace(ref input) || Comment(ref input))
                {
                    continue;
                }
                SectionKV(ref input, dict, name);
            }
            return true;
        }

        public static unsafe void SectionKV(ref Peeker<char> input, Dictionary<string, string> dict, string name)
        {
            if (!input.TakeLine(out var line))
            {
                return;
            }

            int separator = line.IndexOf('=');
            if (separator < 0)
            {
                throw new FormatException(line.ToString());
            }

            var k = line[0..separator].Trim();
            var value = line[(separator + 1)..].Trim();

            // Remove quotes
            if (value.Length > 1 && value[0] == '"' && value[^1] == '"')
            {
                value = value[1..^1];
            }
            dict.Add($"{name}:{k}", value.ToString());
        }

        private static unsafe string SectionName(ref Peeker<char> input)
        {
            if (input.Is('['))
            {
                if (!input.TakeLine(out var l))
                {
                    throw new FormatException("Section name is required.");
                }

                var s = l.IndexOf(']');
                if (s <= 0)
                {
                    throw new FormatException("Section name is required.");
                }
                return l[0..s].ToString();
            }
            return null;
        }
    }
}