using RuQu.Reader;
using System.Buffers;

namespace RuQu
{
    public class IniParser : SimpleCharParserBase<IniConfig, IniParserOptions>
    {
        public static readonly IniParser Instance = new IniParser();

        protected override IniConfig? ContinueRead(IReadBuffer<char> buffer, IniParserOptions options)
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
                    options.Section = new IniSection();
                    options.Config.Add(line[1..^1].Trim().ToString(), options.Section);
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

                options.Section[key] = value.ToString();
            } while (count > 0);
            return buffer.IsFinalBlock ? options.Config : null;
        }

        public override bool ContinueWrite(IBufferWriter<char> writer, IniParserOptions options)
        {
            throw new NotImplementedException();
        }
    }
}