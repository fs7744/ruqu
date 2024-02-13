# 前因

在春节前了解到 Rust语言有一个叫 **[nom](https://github.com/rust-bakery/nom)** 的解析库

它可以让你创建安全的解析器，而不会占用内存或影响性能。

它依靠 Rust 强大的类型系统和内存安全来生成既正确又高效的解析器，并使用函数，宏和特征来抽象出容易出错的管道。

nom 核心是解析器组合器，而解析器组合器是高阶函数，可以接受多个解析器作为输入，并返回一个新的解析器作为输出。

这种方式让你可以为简单的任务(如：解析某个字符串或数字)构建解析器，并使用组合器函数将它们组合成一个递归下降(recursive descent)的解析器。

组合解析的好处包括可测试性，可维护性和可读性。每个部件都非常小且具有自我隔离性，从而使整个解析器由模块化组件构成。

由此萌生在其他非函数语言探究函数抽象代价的想法，如何优雅语义化代码前提下追求极致的性能应该是一个非常复杂的问题

所以在春节期间选择了熟悉的 c# 语言（避免对语言的不熟悉导致无法充分利用性能）进行了尝试

# 调研

春节一开始，为了解除这个让我寝食难安的想法，没有片刻的休息，便开始了纠结反复长期的探究。

当然解析库的顶流必然是[ANTLR](https://www.antlr.org/)，其完全避免了大家在词法语法解析这项费事费力极为复杂的事情上浪费时间

当然我研究ANTLR的话就完全偏离之前预定的函数抽象代价的想法

那么 c# 语言有没有过其他人有过类似用函数思想进行解析器简化抽象的想法呢？

一番查找，发现还真有人在多年前就做尝试，代表者为[Sprache](https://github.com/sprache/Sprache)

其不完全是典型函数组合思想，而是c#语言中`Linq`这数据处理函数库以及其类sql表达结合形式

## 以HexColor解析举例说明

HexColor 为16进制颜色值，其以 "#" 开头, 接着6个16进制数(0-9和a-f,大小写不敏感), 每2个值构成一组, 从左到右为 RGB 通道值, 可以简写为 `#R(hex, hex)G(hex, hex)B(hex, hex)`.

所以解析器逻辑可以概括为：去掉开头的 "#", 如果随后的6个字符为16进制数, 则分为三组, 并将每组的值从16进制转换为10进制.

### 手写HexColor解析器

如果手写代码，该解析器代码如下：

``` csharp
private static (byte red, byte green, byte blue) HexColorHande(string str)
{
    if (str.Length == 7 && str[0] is '#')
    {
        for (var i = 1; i < str.Length; i++)
        {
            if (!char.IsAsciiHexDigit(str[i]))
            {
                throw new FormatException("Must has 6 AsciiHexDigit");
            }
        }
        return (Convert.ToByte(str[1..3], 16), Convert.ToByte(str[3..5], 16), Convert.ToByte(str[5..7], 16));
    }
    throw new ArgumentException("No perfix with #");
}
```

除了Range运算符产生子串之外，没有多余分配和判断消耗，以此作为性能基线，应该可以明显看出其他实现产生的代价

### Sprache 的HexColor解析器

``` csharp
public static class SpracheHexColorTest
{
    /// 通过字段缓存函数，避免多次构建
    private static Sprache.Parser<(byte red, byte green, byte blue)> identifier;

    static SpracheHexColorTest()
    {
        identifier =
        from leading in Sprache.Parse.Char('#').Once()
        from s in Sprache.Parse.LetterOrDigit.Repeat(6).Text()
        select (Convert.ToByte(s[0..2], 16), Convert.ToByte(s[2..4], 16), Convert.ToByte(s[4..6], 16));
    }

    public static (byte red, byte green, byte blue) Parse(string content) => identifier.Parse(content);
}
```

可以看到代码还是非常语义化，虽然个人觉得类sql形式还是有点不习惯，但其实也是对于`Linq`过长链条代码语义复杂问题的比较好的解决形式

那么性能对比结果会是怎么样呢？

| Method                | Mean      | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|---------------------- |----------:|---------:|---------:|-------:|-------:|----------:|
| Hande_HexColor        |  59.04 ns | 0.484 ns | 0.429 ns | 0.0153 |      - |      96 B |
| Superpower_HexColor   | 475.21 ns | 3.690 ns | 3.081 ns | 0.1268 |      - |     800 B |
| Sprache_HexColor      | 621.27 ns | 4.610 ns | 4.312 ns | 0.4692 | 0.0010 |    2944 B |

ps： Superpower 号称是 Sprache 的性能优化版本，两者核心思路一致，只是一些实现细节有所差别，两者使用形式一致，这里就不展示其代码了

可以看到有着比较明显的差别，当然在一般项目中，这点消耗完全没必要考虑（但我的目的本身就是吹毛求疵，这些差异肯定是要探究的）

经过简单查阅代码，其主要有以下差异：

* 函数实际是以`delegate`作为基石

    所以对于手写的解析器肯定有更多的`delegate`调用以及次数代价，不过这点由于语言特性，如果依旧要函数优化语义，多半没有其他优化选择了

* 函数不支持多返回值

    所以封装了`Result`结构体，以此包含更多所需的结果，当然也带来了更多的分配消耗

* `IEnumerable<T>`作为Sprache另外一个基石

    在这里实际导致无法使用原始的`string`，毕竟string的还是一个比较复杂的对象

### 优先尝试减少多返回值

通过 `out` 避免`Result`结构体
``` csharp
public delegate int Is<T>(IPeeker<T> input, out T t);

// 一些函数实现举例
public static Is<char> Is(char c) => Parser.Is<char>(i => i == c, 1);

public static Is<T> Is<T>(Func<T, bool> predicate, int count) => (IPeeker<T> i, out T t) =>
{
    if (i.TryPeek(out t) && predicate(t))
    {
        return count;
    }
    return 0;
};

public static Func<IPeeker<T>, T> Once<T>(this Is<T> predicate, Func<IPeeker<T>, Exception> ex) => i =>
{
    var c = predicate(i, out var t);
    if (c <= 0)
    {
        throw ex(i);
    }
    i.Read(c);
    return t;
};

// HexColor解析器
public static class HexColorOnlyChar
{
    private static readonly Func<IPeeker<char>, char> tag_Start = Chars.Is('#').Once("# is Required.");

    private static Func<IPeeker<char>, char[]> HexDigit = Chars.IsAsciiHexDigit.Repeat(6, "Must has 6 AsciiHexDigit");

    public static (byte red, byte green, byte blue) Parse(string str)
    {
        var input = Input.From(str);
        tag_Start(input);
        var s = new string(HexDigit(input));
        var r = (Convert.ToByte(s[0..2], 16), Convert.ToByte(s[2..4], 16), Convert.ToByte(s[4..6], 16));
        if (input.TryPeek(out var c))
        {
            throw new FormatException(c.ToString());
        }
        return r;
    }
}
```

ps： 这里实际略掉了一些 错误信息 以及行列计算的信息，为了简单，暂时不讨论，忽略其影响，重心先看与基线的差异


| Method                | Mean      | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|---------------------- |----------:|---------:|---------:|-------:|-------:|----------:|
| Hande_HexColor        |  59.04 ns | 0.484 ns | 0.429 ns | 0.0153 |      - |      96 B |
| RuQu_HexColorOnlyChar | 170.10 ns | 2.317 ns | 2.168 ns | 0.0572 |      - |     360 B |
| Sprache_HexColor      | 621.27 ns | 4.610 ns | 4.312 ns | 0.4692 | 0.0010 |    2944 B |

可以看到，减少了非常多处理逻辑后，性能好了很多，我们再做更多尝试

### 最终实现

``` csharp
public static class HexColor
{
    /// Take<char> 实际调用 TryPeek(int count, out IPeekSlice<char> data) ， 降低 
    private static readonly Func<IPeeker<char>, string> HexDigitString = Parser.Take<char>(6, "Must has 6 AsciiHexDigit").Map(ii =>
    {
        var s = ii.ToString();
        for (var i = 0; i < s.Length; i++)
        {
            if (!char.IsAsciiHexDigit(s[i]))
            {
                throw new FormatException("Must has 6 AsciiHexDigit");
            }
        }
        return s;
    });

    private static readonly Action<IPeeker<char>> NoMore = Chars.Any.NoMore("Only 7 chars");
    private static readonly Func<IPeeker<char>, char> TagStart = Chars.Is('#').Once("# is Required.");

    public static (byte red, byte green, byte blue) Parse(string str)
    {
        var input = Input.From(str);
        TagStart(input);
        var s = HexDigitString(input);
        NoMore(input);
        return (Convert.ToByte(s[0..2], 16), Convert.ToByte(s[2..4], 16), Convert.ToByte(s[4..6], 16));
    }
}
```

对比上一版核心降低了 `IEnumerable<char>` 与 `new string` 代价

``` csharp
/// TryPeek 通过Substring 减少遍历次数
public bool TryPeek(int count, out IPeekSlice<char> data)
{
    var i = readed + count;
    if (i >= str.Length)
    {
        data = null;
        return false;
    }
    data = new PeekString(str.Substring(readed + 1, count));
    return true;
}

/// PeekString 减低了 toArray 和 new string 的消耗
public class PeekString : IPeekSlice<char>
{
    private string str;

    public PeekString(string str)
    {
        this.str = str;
    }

    public override string ToString()
    {
        return str;
    }
}
```

**完整性能测试结果**

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

``` csharp
// * Legends *
  Mean      : Arithmetic mean of all measurements
  Error     : Half of 99.9% confidence interval
  StdDev    : Standard deviation of all measurements
  Gen0      : GC Generation 0 collects per 1000 operations
  Gen1      : GC Generation 1 collects per 1000 operations
  Allocated : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
  1 ns      : 1 Nanosecond (0.000000001 sec)
```

可以看到性能比上一版减少了一半，非常接近手写那版，如果后续不存在函数实现的限制，该方式将会是比较接近一行一行优化的手写方式的尝试方案

# 总结

* `delegate` 肯定会有调用消耗，不过单次非常小，只是函数抽象一般量大
* 抽象可以简化问题，但也容易屏蔽特定场景优化，带来多余消耗
* 编码还是需要安静环境，不然容易注意力分散而昏头浪费时间，哈哈哈哈

虽然春节花费了不少时间，确定了一些认知，但性能与优雅语义化依然如太极阴阳平衡艰难

完整代码参考 https://github.com/fs7744/ruqu

该代码仅为早期研究，不代表最终成型，也缺乏完整功能，还有待持续开发

后续会以 ini 文件解析探究更多复杂语义函数实现