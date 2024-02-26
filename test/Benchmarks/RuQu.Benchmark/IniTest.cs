using BenchmarkDotNet.Attributes;
using IniParser.Model;
using IniParser.Parser;
using Microsoft.Extensions.Configuration;
using RuQu.Reader;

namespace RuQu.Benchmark
{
    [MemoryDiagnoser]
    public class IniTest
    {
        private const string testdata = """

                [package]
                name="test"
                version="1.1.2"
                  ;sddd
                 [package2]
                name = "test2"
                version = "1.1.2"

                """;
        private readonly IniConfig iniConfig;
        private readonly IniData pdata;
        IniDataParser parser = new IniDataParser();

        public IniTest()
        {
            iniConfig = IniParser.Instance.Read(testdata);
            pdata = parser.Parse(testdata);
        }

        public static IDictionary<string, string?> Read(string content)
        {
            var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            using (var reader = new StringReader(content))
            {
                string sectionPix = string.Empty;

                while (reader.Peek() != -1)
                {
                    string rawLine = reader.ReadLine()!; // Since Peak didn't return -1, stream hasn't ended.
                    string line = rawLine.Trim();

                    // Ignore blank lines
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    // Ignore comments
                    if (line[0] is ';' or '#' or '/')
                    {
                        continue;
                    }
                    // [Section:header]
                    if (line[0] == '[' && line[line.Length - 1] == ']')
                    {
                        // remove the brackets
                        sectionPix = string.Concat(line.AsSpan(1, line.Length - 2).Trim(), ConfigurationPath.KeyDelimiter);
                        continue;
                    }

                    // key = value OR "value"
                    int separator = line.IndexOf('=');
                    if (separator < 0)
                    {
                        throw new FormatException(rawLine);
                    }

                    string key = sectionPix + line.Substring(0, separator).Trim();
                    string value = line.Substring(separator + 1).Trim();

                    // Remove quotes
                    if (value.Length > 1 && value[0] == '"' && value[value.Length - 1] == '"')
                    {
                        value = value.Substring(1, value.Length - 2);
                    }

                    if (data.ContainsKey(key))
                    {
                        throw new FormatException(key);
                    }

                    data[key] = value;
                }
            }
            return data;
        }


        [Benchmark]
        public void Hande_Read_Ini()
        {
            Read(testdata);
        }

        [Benchmark]
        public void RuQu_Read_Ini()
        {
            IniParser.Instance.Read(testdata);
        }

        [Benchmark]
        public void IniDataParser_Read()
        {
            parser.Parse(testdata);
        }

        [Benchmark]
        public void RuQu_Write_Ini()
        {
            IniParser.Instance.WriteToString(iniConfig);
        }

        [Benchmark]
        public void IniDataParser_Write()
        {
            pdata.ToString();
        }
    }
}