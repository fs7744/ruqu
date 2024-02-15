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
}