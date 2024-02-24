# RuQu

RuQu just a parse helper lib, it want to be high performance, but the code maybe not simple.

![/doc/img/ruqu.png](https://raw.githubusercontent.com/fs7744/ruqu/main/doc/img/RuQu.png)

## HexColor String Parse Example

``` csharp
public class HexColorCharParser : SimpleCharParserBase<(byte red, byte green, byte blue), NoneReadState>
{
    public HexColorStreamParser()
    {
        BufferSize = 8;
    }

    protected override NoneReadState InitReadState()
    {
        return null;
    }

    protected override (byte red, byte green, byte blue) ContinueRead(IReadBuffer<char> buffer, ref NoneReadState state)
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

BenchmarkDotNet v0.13.12, Windows 11 (10.0.22000.2538/21H2/SunValley)
Intel Core i7-10700 CPU 2.90GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 8.0.200
  [Host]     : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2


```

| Method                      | Mean      | Error     | StdDev    | Median    | Gen0   | Gen1   | Allocated |
|---------------------------- |----------:|----------:|----------:|----------:|-------:|-------:|----------:|
| Hande_HexColor              |  46.20 ns |  0.730 ns |  0.609 ns |  46.21 ns | 0.0114 |      - |      96 B |
| RuQu_HexColor               |  62.77 ns |  1.252 ns |  1.671 ns |  62.52 ns | 0.0229 |      - |     192 B |
| RuQu_HexColor_Stream        | 125.64 ns |  2.276 ns |  2.129 ns | 125.75 ns | 0.0248 |      - |     208 B |
| RuQu_HexColor_WriteToString |  65.45 ns |  1.333 ns |  3.626 ns |  64.39 ns | 0.0315 |      - |     264 B |
| Superpower_HexColor         | 464.40 ns |  9.186 ns |  7.671 ns | 461.79 ns | 0.0954 |      - |     800 B |
| Pidgin_HexColor             | 282.57 ns |  3.472 ns |  3.248 ns | 282.52 ns | 0.0420 |      - |     352 B |
| Sprache_HexColor            | 643.78 ns | 12.498 ns | 21.558 ns | 645.83 ns | 0.3519 | 0.0010 |    2944 B |

## Ini Char Parse Example

``` csharp
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
```

**性能测试**

```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.22000.2538/21H2/SunValley)
Intel Core i7-10700 CPU 2.90GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 8.0.200
  [Host]     : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2


```

| Method              | Mean       | Error    | StdDev    | Median     | Gen0   | Gen1   | Allocated |
|-------------------- |-----------:|---------:|----------:|-----------:|-------:|-------:|----------:|
| Hande_Read_Ini      |   536.0 ns |  8.89 ns |   7.88 ns |   537.1 ns | 0.2136 |      - |   1.75 KB |
| RuQu_Read_Ini       |   481.5 ns |  9.21 ns |  11.65 ns |   480.7 ns | 0.1326 |      - |   1.09 KB |
| IniDataParser_Read  | 4,489.1 ns | 79.28 ns | 148.91 ns | 4,451.8 ns | 0.8392 | 0.0076 |   6.91 KB |
| RuQu_Write_Ini      |   799.7 ns | 24.96 ns |  69.98 ns |   774.4 ns | 1.0347 | 0.0315 |   8.45 KB |
| IniDataParser_Write | 1,241.5 ns | 19.97 ns |  17.70 ns | 1,244.6 ns | 0.2441 |      - |   2.01 KB |
