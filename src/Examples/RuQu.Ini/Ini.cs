using RuQu.Strings;

namespace RuQu
{
    public class Ini
    {
        private static Ini instance = new Ini();

        public Func<IPeeker<char>, bool> WhiteSpace = Chars.IngoreWhiteSpace.Map(i => i > 0);

        public Func<IPeeker<char>, bool> Comment = Chars.In(";#/").Delimited(Chars.NotCRLF.ToSlice().Opt(), Chars.IsCRLF, "Comment not right").Map((c, x, y, z) => c);

        public Func<IPeeker<char>, string> SectionName = Chars.Is('[').Delimited(Chars.Not(']').ToSlice().Once("Section name is required."), Chars.Is(']'), "Section name must end with ']'").Map((c, x, y, z) => y?.ToString());

        public Func<IPeeker<char>, string> Key = Chars.Not('=').ToSlice().Once("key is required.").Map(i => i.ToString());
        public Func<IPeeker<char>, char> Separater = Chars.Is('=').Once("Section name is required.");

        public Func<IPeeker<char>, string> Value = Chars.NotCRLF.ToSlice().Once("value is required.").Map(i =>
        {
            var v = i.ToString().Trim();
            return v.StartsWith('"') ? v[1..^1] : v;
        });

        public IDictionary<string, string> ParseString(string content)
        {
            var input = Input.From(content);
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            while (input.TryPeek(out var v))
            {
                if (!(WhiteSpace(input) || Comment(input) || Section(input, dict)))
                {
                    throw new NotSupportedException(v.ToString());
                }
            }
            return dict;
        }

        public bool SectionContentEnd(IPeeker<char> input)
        {
            return !input.TryPeek(out var v) || v is '[';
        }

        public bool Section(StringPeeker input, Dictionary<string, string> dict)
        {
            var name = SectionName(input);
            if (name == null) return false;
            while (!SectionContentEnd(input))
            {
                if (WhiteSpace(input) || Comment(input))
                {
                    continue;
                }
                SectionKV(input, dict, name);
            }
            return true;
        }

        public void SectionKV(StringPeeker input, Dictionary<string, string> dict, string name)
        {
            var k = Key(input);
            Separater(input);
            var v = Value(input);
            k = $"{name}:{k.Trim()}";
            dict.Add(k, v.Trim());
        }

        public static IDictionary<string, string> Parse(string content)
        {
            return instance.ParseString(content);
        }
    }

    public static class IniStruct
    {
        public static IDictionary<string, string> Parse(string content)
        {
            var input = content.AsPeeker();
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

        private static bool IsCommentStart(char c) => c == ';' || c == '#' || c == '/';

        private static unsafe bool Comment(ref Peeker<char> input)
        {
            return input.Is(&IsCommentStart, out var _) && input.TakeLine(out var _);
        }

        private static bool IsSectionStart(char c) => c == '[';

        private static bool Section(ref Peeker<char> input, Dictionary<string, string> dict)
        {
            var name = SectionName(ref input);
            if (name == null) return false;
            while (input.TryPeek(out var v) && v is not '[')
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
            if (input.Is(&IsSectionStart, out var c))
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