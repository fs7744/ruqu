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

BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3085/23H2/2023Update/SunValley3)
Intel Core i7-9750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.101
  [Host]     : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2


```

| Method                | Mean      | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|---------------------- |----------:|---------:|---------:|-------:|-------:|----------:|
| Hande_HexColor        |  59.04 ns | 0.484 ns | 0.429 ns | 0.0153 |      - |      96 B |
| RuQu_HexColor         |  81.76 ns | 0.551 ns | 0.489 ns | 0.0305 |      - |     192 B |
| RuQu_HexColorOnlyChar | 170.10 ns | 2.317 ns | 2.168 ns | 0.0572 |      - |     360 B |
| Superpower_HexColor   | 475.21 ns | 3.690 ns | 3.081 ns | 0.1268 |      - |     800 B |
| Sprache_HexColor      | 621.27 ns | 4.610 ns | 4.312 ns | 0.4692 | 0.0010 |    2944 B |
