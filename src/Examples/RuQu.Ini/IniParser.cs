using RuQu.Reader;

namespace RuQu
{
    public class IniParser : SimpleCharParserBase<IniConfig, IniParserReadState>
    {
        public static readonly IniParser Instance = new IniParser();

        private static readonly ReadOnlyMemory<char> NewLine = Environment.NewLine.AsMemory();
        private static readonly ReadOnlyMemory<char> SectionBegin = "[".AsMemory();
        private static readonly ReadOnlyMemory<char> SectionEnd = "]".AsMemory();
        private static readonly ReadOnlyMemory<char> Separator = "=".AsMemory();


        protected override IniConfig? ContinueRead(IReadBuffer<char> buffer, ref IniParserReadState state)
        {
            int count;
            var total = 0;
            do
            {
                count = buffer.ReadLine(out var rawLine);
                total += count;
                if (count == 0 && buffer.IsFinalBlock)
                {
                    rawLine = buffer.Remaining;
                    total += rawLine.Length;
                }
                var line = rawLine.Trim();

                // Ignore blank lines

                if (line.IsEmpty || line.IsWhiteSpace())
                {
                    continue;
                }
                // Ignore comments
                if (line[0] is ';' or '#' or '/')
                {
                    continue;
                }
                // [Section:header]
                if (line[0] == '[' && line[^1] == ']')
                {
                    // remove the brackets
                    state.Section = new IniSection();
                    state.Config.Add(line[1..^1].Trim().ToString(), state.Section);
                    continue;
                }

                // key = value OR "value"
                int separator = line.IndexOf('=');
                if (separator < 0)
                {
                    throw new FormatException(rawLine.ToString());
                }

                string key = line[0..separator].Trim().ToString();
                var value = line[(separator + 1)..].Trim();

                // Remove quotes
                if (value.Length > 1 && value[0] == '"' && value[^1] == '"')
                {
                    value = value[1..^1];
                }

                state.Section[key] = value.ToString();
            } while (count > 0);
            return buffer.IsFinalBlock ? state.Config : null;
        }

        protected override IniParserReadState InitReadState()
        {
            return new IniParserReadState();
        }

        protected override IEnumerable<ReadOnlyMemory<char>> ContinueWrite(IniConfig value)
        {
            foreach (var item in value)
            {
                yield return SectionBegin;
                yield return item.Key.AsMemory();
                yield return SectionEnd;
                yield return NewLine;

                foreach (var c in item.Value)
                {
                    yield return c.Key.AsMemory();
                    yield return Separator;
                    yield return c.Value.AsMemory();
                    yield return NewLine;
                }
            }
        }
    }
}