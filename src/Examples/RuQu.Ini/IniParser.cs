using RuQu.Reader;

namespace RuQu
{
    public class IniParser : SimpleCharParserBase<IDictionary<string, string>, SimpleReadOptions, IniParserState>
    {
        public static readonly IniParser Instance = new IniParser();

        protected override IDictionary<string, string>? ContinueRead(ref IReadBuffer<char> bufferState, ref IniParserState state)
        {
            int count;
            var total = 0;
            do
            {
                count = bufferState.ReadLine(out var rawLine);
                total += count;
                if (count == 0 && bufferState.IsFinalBlock)
                {
                    rawLine = bufferState.Remaining;
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
                    state.SectionPix = string.Concat(line[1..^1].Trim(), ":");
                    continue;
                }

                // key = value OR "value"
                int separator = line.IndexOf('=');
                if (separator < 0)
                {
                    throw new FormatException(rawLine.ToString());
                }

                string key = state.SectionPix + line[0..separator].Trim().ToString();
                string value = line[(separator + 1)..].Trim().ToString();

                // Remove quotes
                if (value.Length > 1 && value[0] == '"' && value[^1] == '"')
                {
                    value = value[1..^1];
                }

                if (state.Dict.ContainsKey(key))
                {
                    throw new FormatException(key);
                }

                state.Dict[key] = value;
            } while (count > 0);
            return bufferState.IsFinalBlock ? state.Dict : null;
        }
    }
}