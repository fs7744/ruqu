# RuQu

RuQu just a parse helper lib, it want to be high performance, but the code maybe not simple.

![/doc/img/ruqu.png](https://raw.githubusercontent.com/fs7744/ruqu/main/doc/img/RuQu.png)

## HexColor String Parse Example

``` csharp
public static class HexColor
{
    private static void TagStart(ref Peeker<char> input)
    {
        if (!input.TryPeek(out var tag) || tag is not '#')
        {
            throw new FormatException("No perfix with #");
        }
        input.Read();
    }

    private static byte HexDigitColor(ref Peeker<char> input)
    {
        if (!input.TryPeek(2, out var str) || !char.IsAsciiHexDigit(str[0]) || !char.IsAsciiHexDigit(str[1]))
        {
            throw new FormatException("One color must be 2 AsciiHexDigit");
        }
        input.Read(2);
        return Convert.ToByte(str.ToString(), 16);
    }

    private static void NoMore(ref Peeker<char> input)
    {
        if (input.TryPeek(out var _))
        {
            throw new FormatException("Only 7 chars");
        }
    }

    public static (byte red, byte green, byte blue) Parse(string str)
    {
        var input = str.AsPeeker();
        TagStart(ref input);
        var r = (HexDigitColor(ref input), HexDigitColor(ref input), HexDigitColor(ref input));
        NoMore(ref input);
        return r;
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

| Method              | Mean      | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|-------------------- |----------:|----------:|----------:|-------:|-------:|----------:|
| Hande_HexColor      |  45.93 ns |  0.599 ns |  0.531 ns | 0.0114 |      - |      96 B |
| RuQu_HexColor       |  58.98 ns |  0.564 ns |  0.500 ns | 0.0114 |      - |      96 B |
| Superpower_HexColor | 453.29 ns |  7.207 ns |  6.741 ns | 0.0954 |      - |     800 B |
| Sprache_HexColor    | 615.32 ns | 11.622 ns | 10.871 ns | 0.3519 | 0.0010 |    2944 B |
