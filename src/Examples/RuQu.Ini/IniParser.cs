﻿using RuQu.Reader;

namespace RuQu
{
    public class IniParser : SimpleCharParserBase<IniConfig, IniParserOptions>
    {
        public static readonly IniParser Instance = new IniParser();

        protected override IniConfig? ContinueRead(IReadBuffer<char> bufferState, IniParserOptions state)
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
                string value = line[(separator + 1)..].Trim().ToString();

                // Remove quotes
                if (value.Length > 1 && value[0] == '"' && value[^1] == '"')
                {
                    value = value[1..^1];
                }

                state.Section[key] = value;
            } while (count > 0);
            return bufferState.IsFinalBlock ? state.Config : null;
        }
    }

    public class IniSection : Dictionary<string, string>
    {
        public IniSection() : base(StringComparer.OrdinalIgnoreCase)
        {

        }
    }

    public class IniConfig : Dictionary<string, IniSection>
    {
        public IniConfig() : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}