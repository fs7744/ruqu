# RuQu

RuQu just a parse helper lib, it want to be high performance, but the code maybe not simple.

![/doc/img/ruqu.png](https://raw.githubusercontent.com/fs7744/ruqu/main/doc/img/RuQu.png)

## HexColor String Parse Example

``` csharp
public class HexColorCharParser : SimpleCharParserBase<(byte red, byte green, byte blue), SimpleReadOptions, IntState>
{
    protected override (byte red, byte green, byte blue) ContinueRead(ref IReadBuffer<char> bufferState, ref IntState state)
    {
        var bytes = bufferState.Remaining;
        if (bytes.Length > 7)
        {
            throw new FormatException("Only 7 utf-8 chars");
        }

        if (!bufferState.IsFinalBlock && bytes.Length < 7)
        {
            bufferState.AdvanceBuffer(0);
            return default;
        }

        if (bufferState.IsFinalBlock && bytes.Length < 7)
        {
            throw new FormatException("Must 7 utf-8 chars");
        }

        if (bytes[0] is not '#')
        {
            throw new FormatException("No perfix with #");
        }

        var c = new string(bytes[1..]);

        return (Convert.ToByte(c[0..2], 16), Convert.ToByte(c[2..4], 16), Convert.ToByte(c[4..6], 16));
    }
}
```

**性能测试**

```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3155/23H2/2023Update/SunValley3)
13th Gen Intel Core i9-13900KF, 1 CPU, 32 logical and 24 physical cores
.NET SDK 8.0.200
  [Host]     : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2


```
| Method               | Mean      | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|--------------------- |----------:|---------:|---------:|-------:|-------:|----------:|
| Hande_HexColor       |  25.60 ns | 0.257 ns | 0.240 ns | 0.0051 |      - |      96 B |
| RuQu_HexColor        |  31.05 ns | 0.413 ns | 0.386 ns | 0.0102 |      - |     192 B |
| RuQu_HexColor_Stream |  59.26 ns | 1.167 ns | 1.297 ns | 0.0118 |      - |     224 B |
| Superpower_HexColor  | 323.70 ns | 5.129 ns | 4.797 ns | 0.0424 |      - |     800 B |
| Pidgin_HexColor      | 137.06 ns | 1.035 ns | 0.968 ns | 0.0186 |      - |     352 B |
| Sprache_HexColor     | 312.23 ns | 2.697 ns | 2.523 ns | 0.1564 | 0.0005 |    2944 B |

## Ini Char Parse Example

``` csharp
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
```

**性能测试**

```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3155/23H2/2023Update/SunValley3)
13th Gen Intel Core i9-13900KF, 1 CPU, 32 logical and 24 physical cores
.NET SDK 8.0.200
  [Host]     : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2


```
| Method        | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|-------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| Hande_Ini     |   267.3 ns |  4.72 ns |  4.41 ns | 0.0949 |      - |   1.75 KB |
| RuQu_Ini      |   285.0 ns |  5.51 ns |  4.88 ns | 0.0672 |      - |   1.24 KB |
| IniDataParser | 2,264.0 ns | 19.23 ns | 17.99 ns | 0.3738 | 0.0038 |   6.91 KB |

