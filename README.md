# RuQu

RuQu just a parse helper lib, it want to be high performance, but the code maybe not simple.

![/doc/img/ruqu.png](https://raw.githubusercontent.com/fs7744/ruqu/main/doc/img/RuQu.png)

## HexColor String Parse Example

``` csharp
public class HexColorCharParser : SimpleCharParserBase<(byte red, byte green, byte blue), SimpleOptions<(byte red, byte green, byte blue)>>
{
    protected override (byte red, byte green, byte blue) ContinueRead(IReadBuffer<char> buffer, SimpleOptions<(byte red, byte green, byte blue)> options)
    {
        var bytes = buffer.Remaining;
        if (bytes.Length > 7)
        {
            throw new FormatException("Only 7 utf-8 chars");
        }

        if (!buffer.IsFinalBlock && bytes.Length < 7)
        {
            buffer.AdvanceBuffer(0);
            return default;
        }

        if (buffer.IsFinalBlock && bytes.Length < 7)
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

    public override bool ContinueWrite(IBufferWriter<char> writer, SimpleOptions<(byte red, byte green, byte blue)> options)
    {
        var span = writer.GetSpan(7);
        span[0] = '#';
        (byte red, byte green, byte blue) = options.WriteObject;
        var str = Convert.ToHexString(new byte[] { red, green, blue });
        str.CopyTo(span.Slice(1));
        writer.Advance(str.Length + 1);
        return true;
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
| Method                      | Mean      | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|---------------------------- |----------:|---------:|---------:|-------:|-------:|----------:|
| Hande_HexColor              |  25.93 ns | 0.131 ns | 0.123 ns | 0.0051 |      - |      96 B |
| RuQu_HexColor               |  32.09 ns | 0.351 ns | 0.328 ns | 0.0102 |      - |     192 B |
| RuQu_HexColor_Stream        |  56.39 ns | 0.762 ns | 0.713 ns | 0.0110 |      - |     208 B |
| RuQu_HexColor_WriteToString |  37.97 ns | 0.564 ns | 0.528 ns | 0.0123 |      - |     232 B |
| Superpower_HexColor         | 295.87 ns | 2.706 ns | 2.531 ns | 0.0424 |      - |     800 B |
| Pidgin_HexColor             | 138.74 ns | 2.593 ns | 2.299 ns | 0.0186 |      - |     352 B |
| Sprache_HexColor            | 311.89 ns | 4.468 ns | 3.961 ns | 0.1564 | 0.0005 |    2944 B |


## Ini Char Parse Example

``` csharp
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
| Hande_Ini     |   279.8 ns |  5.55 ns |  5.19 ns | 0.0949 |      - |   1.75 KB |
| RuQu_Ini      |   283.4 ns |  4.70 ns |  4.39 ns | 0.0587 |      - |   1.09 KB |
| IniDataParser | 2,310.2 ns | 31.58 ns | 29.54 ns | 0.3738 | 0.0038 |   6.91 KB |

