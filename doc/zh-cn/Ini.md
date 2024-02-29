上一篇 用 HexColor 作为示例，可能过于简单

这里再补充一个 ini 解析的示例

由于实在写不动用其他库解析 ini 了, 春节都要过完了，累了，写不动了， 

所以随意找了一份解析ini的库， 仅供参考，对比不准确，毕竟完整库包含了更多功能

### 先看看结果

```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3085/23H2/2023Update/SunValley3)
Intel Core i7-9750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.200
  [Host]     : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2


```
| Method        | Mean       | Error    | StdDev    | Gen0   | Gen1   | Allocated |
|-------------- |-----------:|---------:|----------:|-------:|-------:|----------:|
| Hande_Ini     |   567.9 ns | 11.24 ns |  21.66 ns | 0.2851 |      - |   1.75 KB |
| RuQu_Ini      | 1,691.4 ns | 33.48 ns |  64.51 ns | 0.4177 |      - |   2.56 KB |
| IniDataParser | 4,836.3 ns | 94.44 ns | 167.87 ns | 1.1215 | 0.0076 |   6.91 KB |

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


### 先看来自 dotnet `Microsoft.Extensions.Configuration` 中解析 ini 的代码

``` csharp
public static IDictionary<string, string?> Read(string content)
{
    var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
    using (var reader = new StringReader(content))
    {
        string sectionPrefix = string.Empty;

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
                sectionPrefix = string.Concat(line.AsSpan(1, line.Length - 2).Trim(), ConfigurationPath.KeyDelimiter);
                continue;
            }

            // key = value OR "value"
            int separator = line.IndexOf('=');
            if (separator < 0)
            {
                throw new FormatException(rawLine);
            }

            string key = sectionPrefix + line.Substring(0, separator).Trim();
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
```

### 再来看看部分函数语义优化的代码

``` csharp
public class Ini
{
    private static Ini instance = new Ini();

    public Func<IPeeker<char>, bool> WhiteSpace = Chars.IngoreWhiteSpace.Map(i => i > 0);

    public Func<IPeeker<char>, bool> Comment = Chars.In(";#/").Delimited(Chars.NotCRLF.ToSlice().Opt(), Chars.IsCRLF, "Comment not right").Map((c, x, y, z) => c);

    public Func<IPeeker<char>, string> SectionName = Chars.Is('[').Delimited(Chars.Not(']').ToSlice().Once("Section name is required."), Chars.Is(']'), "Section name must end with ']'").Map((c, x, y, z) => y?.ToString());

    public Func<IPeeker<char>, string> Key = Chars.Not('=').ToSlice().Once("key is required.").Map(i => i.ToString());
    public Func<IPeeker<char>, char> Separater = Chars.Is('=').Once("Section name is required.");

    public Func<IPeeker<char>, string> Value = Chars.NotCRLF.ToSlice().Once("value is required.").Map(i =>
    {
        var v = i.ToString().Trim();
        return v.StartsWith('"') ? v[1..^1] : v;
    });

    public IDictionary<string, string> ParseString(string content)
    {
        var input = Input.From(content);
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        while (input.TryPeek(out var v))
        {
            if (!(WhiteSpace(input) || Comment(input) || Section(input, dict)))
            {
                throw new NotSupportedException(v.ToString());
            }
        }
        return dict;
    }

    public bool SectionContentEnd(IPeeker<char> input)
    {
        return !input.TryPeek(out var v) || v is '[';
    }

    public bool Section(StringPeeker input, Dictionary<string, string> dict)
    {
        var name = SectionName(input);
        if (name == null) return false;
        while (!SectionContentEnd(input))
        {
            if (WhiteSpace(input) || Comment(input))
            {
                continue;
            }
            SectionKV(input, dict, name);
        }
        return true;
    }

    public void SectionKV(StringPeeker input, Dictionary<string, string> dict, string name)
    {
        var k = Key(input);
        Separater(input);
        var v = Value(input);
        k = $"{name}:{k.Trim()}";
        dict.Add(k, v.Trim());
    }

    public static IDictionary<string, string> Parse(string content)
    {
        return instance.ParseString(content);
    }
}
```

### 最后截取 部分 ini 解析库的代码 仅供参考

``` csharp
public IniData Parse(string iniDataString)
{
    IniData iniData = (Configuration.CaseInsensitive ? new IniDataCaseInsensitive() : new IniData());
    iniData.Configuration = Configuration.Clone();
    if (string.IsNullOrEmpty(iniDataString))
    {
        return iniData;
    }

    _errorExceptions.Clear();
    _currentCommentListTemp.Clear();
    _currentSectionNameTemp = null;
    try
    {
        string[] array = iniDataString.Split(new string[2] { "\n", "\r\n" }, StringSplitOptions.None);
        for (int i = 0; i < array.Length; i++)
        {
            string text = array[i];
            if (text.Trim() == string.Empty)
            {
                continue;
            }

            try
            {
                ProcessLine(text, iniData);
            }
            catch (Exception ex)
            {
                ParsingException ex2 = new ParsingException(ex.Message, i + 1, text, ex);
                if (Configuration.ThrowExceptionsOnError)
                {
                    throw ex2;
                }

                _errorExceptions.Add(ex2);
            }
        }

        if (_currentCommentListTemp.Count > 0)
        {
            if (iniData.Sections.Count > 0)
            {
                iniData.Sections.GetSectionData(_currentSectionNameTemp).TrailingComments.AddRange(_currentCommentListTemp);
            }
            else if (iniData.Global.Count > 0)
            {
                iniData.Global.GetLast().Comments.AddRange(_currentCommentListTemp);
            }

            _currentCommentListTemp.Clear();
        }
    }
    catch (Exception item)
    {
        _errorExceptions.Add(item);
        if (Configuration.ThrowExceptionsOnError)
        {
            throw;
        }
    }

    if (HasError)
    {
        return null;
    }

    return (IniData)iniData.Clone();
}

```