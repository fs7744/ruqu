# RuQu

RuQu just a parse helper lib, it want to be high performance, but the code maybe not simple.

![/doc/img/ruqu.png](https://raw.githubusercontent.com/fs7744/ruqu/main/doc/img/RuQu.png)

## HexColor String Parse Example

``` csharp
public class HexColorCharParser : SimpleCharParserBase<(byte red, byte green, byte blue)>
{
    public HexColorCharParser()
    {
        BufferSize = 8;
    }

    protected override (byte red, byte green, byte blue) Read(IReaderBuffer<char> buffer)
    {
        buffer.Tag('#');
        var c = buffer.AsciiHexDigit(6).ToString();
        buffer.Eof();
        return (Convert.ToByte(c[0..2], 16), Convert.ToByte(c[2..4], 16), Convert.ToByte(c[4..6], 16));
    }

    private static readonly ReadOnlyMemory<char> Tag = "#".AsMemory();

    protected override IEnumerable<ReadOnlyMemory<char>> ContinueWrite((byte red, byte green, byte blue) value)
    {
        (byte red, byte green, byte blue) = value;
        yield return Tag;
        yield return Convert.ToHexString(new byte[] { red, green, blue }).AsMemory();
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
| Hande_HexColor              |  26.35 ns | 0.080 ns | 0.075 ns | 0.0051 |      - |      96 B |
| RuQu_HexColor               |  43.51 ns | 0.210 ns | 0.186 ns | 0.0089 |      - |     168 B |
| RuQu_HexColor_Stream        |  82.63 ns | 0.879 ns | 0.779 ns | 0.0101 |      - |     192 B |
| RuQu_HexColor_WriteToString |  37.34 ns | 0.675 ns | 0.632 ns | 0.0136 |      - |     256 B |
| Superpower_HexColor         | 320.00 ns | 4.630 ns | 4.331 ns | 0.0424 |      - |     800 B |
| Pidgin_HexColor             | 143.35 ns | 0.979 ns | 0.868 ns | 0.0186 |      - |     352 B |
| Sprache_HexColor            | 323.23 ns | 2.526 ns | 2.362 ns | 0.1564 | 0.0005 |    2944 B |


## Ini Char Parse Example

``` csharp
public class IniParser : SimpleCharParserBase<IniConfig>
{
    public static readonly IniParser Instance = new IniParser();

    private static readonly ReadOnlyMemory<char> NewLine = Environment.NewLine.AsMemory();
    private static readonly ReadOnlyMemory<char> SectionBegin = "[".AsMemory();
    private static readonly ReadOnlyMemory<char> SectionEnd = "]".AsMemory();
    private static readonly ReadOnlyMemory<char> Separator = "=".AsMemory();

    protected override IniConfig? Read(IReaderBuffer<char> buffer)
    {
        var config = new IniConfig();
        IniSection section = null;
        while (buffer.Line(out var rawLine))
        {
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
                section = new IniSection();
                config.Add(line[1..^1].Trim().ToString(), section);
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

            section[key] = value.ToString();
        }
        return config;
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
```

**性能测试**

```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3155/23H2/2023Update/SunValley3)
13th Gen Intel Core i9-13900KF, 1 CPU, 32 logical and 24 physical cores
.NET SDK 8.0.200
  [Host]     : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2


```
| Method              | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|-------------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
| Hande_Read_Ini      |   290.2 ns |  2.92 ns |  2.73 ns | 0.0949 |      - |    1792 B |
| RuQu_Read_Ini       |   292.3 ns |  2.04 ns |  1.90 ns | 0.0548 |      - |    1032 B |
| IniDataParser_Read  | 2,390.6 ns | 15.42 ns | 14.42 ns | 0.3738 | 0.0038 |    7080 B |
| RuQu_Write_Ini      |   289.8 ns |  3.76 ns |  3.14 ns | 0.0491 |      - |     928 B |
| IniDataParser_Write |   637.3 ns |  5.38 ns |  5.03 ns | 0.1087 |      - |    2056 B |
