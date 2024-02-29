# 以解析csv数据为例，讨论string、char[]、stream 不同类型来源是否能进行高性能读取解析封装可能性

## 篇幅较长，所以首先列举结果，也就是我们的目的

核心目的为探索特定场景对不同类型数据进行统一抽象，并达到足够高性能，也就是一份代码实现，对不同类型数据依然高性能

以下为结果，也就是我们的目的： 

对1w行 csv 数据的string进行 [RFC4180 csv标准](https://datatracker.ietf.org/doc/html/rfc4180)进行解析，

string 类型 csv 应该比 StringReader 性能更高

甚至对比大家使用非常多的 csvhelper 不应该性能差太多

测试代码如下

``` csharp
[MemoryDiagnoser]
public class CsvTest
{
    private const string testdata = """
            a,b
            1,2
            3sss,3333
            1,2
            3sss,3333
            1,2
/// 1w 行
            """;

    private CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        Mode = CsvMode.RFC4180,
    };

    [Benchmark]
    public void CsvHelper_Read()
    {
        using var sr = new StringReader(testdata);
        using var csv = new CsvHelper.CsvReader(sr, config);
        var records = new List<string[]>();
        csv.Read();
        csv.ReadHeader();
        while (csv.Read())
        {
            var record = new string[csv.ColumnCount];
            for (var i = 0; i < record.Length; i++)
            {
                record[i] = csv.GetField(i);
            }
            records.Add(record);
        }
        //var d = records.ToArray();
    }

    [Benchmark]
    public void RuQu_Read_Csv_StringReader()
    {
        using var sr = new StringReader(testdata);
        using var reader = new RuQu.Csv.CsvReader(sr, fristIsHeader: true);
        var d = reader.ToArray();
    }

    [Benchmark]
    public void RuQu_Read_Csv_String()
    {
        using var reader = new RuQu.Csv.CsvReader(testdata, fristIsHeader: true);
        var d = reader.ToArray();
    }
}
```

性能测试结果：

```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3155/23H2/2023Update/SunValley3)
13th Gen Intel Core i9-13900KF, 1 CPU, 32 logical and 24 physical cores
.NET SDK 8.0.200
  [Host]     : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2


```
| Method                     | Mean     | Error   | StdDev  | Gen0    | Gen1    | Gen2    | Allocated |
|--------------------------- |---------:|--------:|--------:|--------:|--------:|--------:|----------:|
| CsvHelper_Read             | 816.5 μs | 7.67 μs | 7.17 μs | 82.0313 | 81.0547 | 41.0156 |    1.2 MB |
| RuQu_Read_Csv_StringReader | 406.1 μs | 1.83 μs | 1.53 μs | 62.5000 | 52.2461 |       - |   1.13 MB |
| RuQu_Read_Csv_String       | 363.3 μs | 4.27 μs | 3.99 μs | 62.5000 | 52.2461 |       - |   1.13 MB |


那么这样的表现，如何达到呢？我们就从最初我的思考开始

## 数据类型多样性

众所周知，我们可以将csv 这样的文本数据用各种各样的数据类型或者存储形式承载
比如：

``` csharp
csv
 |--- string    "a,b\r\n1,2\r\n3,4"
 |--- char[]
 |--- byte[]
 |--- MemoryStream
 |--- NetworkStream
 |--- ....
```

那么我们是否能对这些类型进行封装抽象，然后以一份代码实现 csv 解析，并达到高性能呢？

## 数据类型归类

根据数据类型特点，我们可以归类为两种

* 无需编码转换的固定长度数组
    - string
    - char[]

* 需要编码转换的不明确长度的来源
    - byte[]
    - MemoryStream
    - NetworkStream

那么我们以后者更高的复杂度抽象肯定能兼容前者

## 高性能基石

其次以 csv 解析实现考虑，字符对比，查找必然是首要考虑

现在这方面首选必然是 ```ReadOnlySpan<T>```

其主要对于我们解析有两大优势

1. 减少数据复制

    ReadOnlySpan<T>实例通常用于引用数组的元素或数组的一部分。 但是，与数组不同， ReadOnlySpan<T> 实例可以指向堆栈上托管的内存、本机内存或托管的内存。

    其[实现的部分代码](https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/ReadOnlySpan.cs#L49)如下

    ``` csharp
    public readonly ref struct ReadOnlySpan<T>
    {
        /// <summary>A byref or a native ptr.</summary>
        internal readonly ref T _reference;
        /// <summary>The number of elements this ReadOnlySpan contains.</summary>
        private readonly int _length;

        /// <summary>
        /// Creates a new read-only span over the entirety of the target array.
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <remarks>Returns default when <paramref name="array"/> is null.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan(T[]? array)
        {
            if (array == null)
            {
                this = default;
                return; // returns default
            }

            _reference = ref MemoryMarshal.GetArrayDataReference(array);
            _length = array.Length;
        }

        public override string ToString()
        {
            if (typeof(T) == typeof(char))
            {
                return new string(new ReadOnlySpan<char>(ref Unsafe.As<T, char>(ref _reference), _length));
            }
            return $"System.ReadOnlySpan<{typeof(T).Name}>[{_length}]";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> Slice(int start, int length)
        {
    #if TARGET_64BIT
            // See comment in Span<T>.Slice for how this works.
            if ((ulong)(uint)start + (ulong)(uint)length > (ulong)(uint)_length)
                ThrowHelper.ThrowArgumentOutOfRangeException();
    #else
            if ((uint)start > (uint)_length || (uint)length > (uint)(_length - start))
                ThrowHelper.ThrowArgumentOutOfRangeException();
    #endif

            return new ReadOnlySpan<T>(ref Unsafe.Add(ref _reference, (nint)(uint)start /* force zero-extension */), length);
        }
    ```

    从上述三个方法可以看出，其通过指针等操作，以 struct 极小代价能让我们共享访问数组数据或者片段

2. span 有 SIMD 优化

    span 有着很多 [SIMD](https://zh.wikipedia.org/wiki/%E5%8D%95%E6%8C%87%E4%BB%A4%E6%B5%81%E5%A4%9A%E6%95%B0%E6%8D%AE%E6%B5%81)优化

    SIMD，即Single Instruction, Multiple Data，一条指令操作多个数据．是CPU基本指令集的扩展．主要用于提供fine grain parallelism，即小碎数据的并行操作．比如说图像处理，图像的数据常用的数据类型是RGB565, RGBA8888, YUV422等格式，这些格式的数据特点是一个像素点的一个分量总是用小于等于８bit的数据表示的．如果使用传统的处理器做计算，虽然处理器的寄存器是32位或是64位的，处理这些数据确只能用于他们的低８位，似乎有点浪费．如果把64位寄存器拆成８个８位寄存器就能同时完成８个操作，计算效率提升了８倍．

    以下是 [span 部分代码示例](https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/SpanHelpers.Char.cs#L14)

    ``` csharp
    internal static partial class SpanHelpers // .Char
    {
        public static int IndexOf(ref char searchSpace, int searchSpaceLength, ref char value, int valueLength)
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            if (valueLength == 0)
                return 0;  // A zero-length sequence is always treated as "found" at the start of the search space.

            int valueTailLength = valueLength - 1;
            if (valueTailLength == 0)
            {
                // for single-char values use plain IndexOf
                return IndexOfChar(ref searchSpace, value, searchSpaceLength);
            }

            nint offset = 0;
            char valueHead = value;
            int searchSpaceMinusValueTailLength = searchSpaceLength - valueTailLength;
            if (Vector128.IsHardwareAccelerated && searchSpaceMinusValueTailLength >= Vector128<ushort>.Count)
            {
                goto SEARCH_TWO_CHARS;
            }

            ref byte valueTail = ref Unsafe.As<char, byte>(ref Unsafe.Add(ref value, 1));
            int remainingSearchSpaceLength = searchSpaceMinusValueTailLength;

            while (remainingSearchSpaceLength > 0)
            {
                // Do a quick search for the first element of "value".
                // Using the non-packed variant as the input is short and would not benefit from the packed implementation.
                int relativeIndex = NonPackedIndexOfChar(ref Unsafe.Add(ref searchSpace, offset), valueHead, remainingSearchSpaceLength);
                if (relativeIndex < 0)
                    break;

                remainingSearchSpaceLength -= relativeIndex;
                offset += relativeIndex;

                if (remainingSearchSpaceLength <= 0)
                    break;  // The unsearched portion is now shorter than the sequence we're looking for. So it can't be there.

                // Found the first element of "value". See if the tail matches.
                if (SequenceEqual(
                        ref Unsafe.As<char, byte>(ref Unsafe.Add(ref searchSpace, offset + 1)),
                        ref valueTail,
                        (nuint)(uint)valueTailLength * 2))
                {
                    return (int)offset;  // The tail matched. Return a successful find.
                }

                remainingSearchSpaceLength--;
                offset++;
            }
            return -1;

            // Based on http://0x80.pl/articles/simd-strfind.html#algorithm-1-generic-simd "Algorithm 1: Generic SIMD" by Wojciech Mula
            // Some details about the implementation can also be found in https://github.com/dotnet/runtime/pull/63285
        SEARCH_TWO_CHARS:
            if (Vector512.IsHardwareAccelerated && searchSpaceMinusValueTailLength - Vector512<ushort>.Count >= 0)
            {
                // Find the last unique (which is not equal to ch1) character
                // the algorithm is fine if both are equal, just a little bit less efficient
                ushort ch2Val = Unsafe.Add(ref value, valueTailLength);
                nint ch1ch2Distance = (nint)(uint)valueTailLength;
                while (ch2Val == valueHead && ch1ch2Distance > 1)
                    ch2Val = Unsafe.Add(ref value, --ch1ch2Distance);

                Vector512<ushort> ch1 = Vector512.Create((ushort)valueHead);
                Vector512<ushort> ch2 = Vector512.Create(ch2Val);

                nint searchSpaceMinusValueTailLengthAndVector =
                    searchSpaceMinusValueTailLength - (nint)Vector512<ushort>.Count;

                do
                {
                    // Make sure we don't go out of bounds
                    Debug.Assert(offset + ch1ch2Distance + Vector512<ushort>.Count <= searchSpaceLength);

                    Vector512<ushort> cmpCh2 = Vector512.Equals(ch2, Vector512.LoadUnsafe(ref searchSpace, (nuint)(offset + ch1ch2Distance)));
                    Vector512<ushort> cmpCh1 = Vector512.Equals(ch1, Vector512.LoadUnsafe(ref searchSpace, (nuint)offset));
                    Vector512<byte> cmpAnd = (cmpCh1 & cmpCh2).AsByte();

                    // Early out: cmpAnd is all zeros
                    if (cmpAnd != Vector512<byte>.Zero)
                    {
                        goto CANDIDATE_FOUND;
                    }

                LOOP_FOOTER:
                    offset += Vector512<ushort>.Count;

                    if (offset == searchSpaceMinusValueTailLength)
                        return -1;

                    // Overlap with the current chunk for trailing elements
                    if (offset > searchSpaceMinusValueTailLengthAndVector)
                        offset = searchSpaceMinusValueTailLengthAndVector;

                    continue;
    ```

## 接口抽象

接下来尝试抽象

``` csharp
public interface IReaderBuffer<T> : IDisposable where T : struct
{
    public int ConsumedCount { get; }
    public int Index { get; }
    public ReadOnlySpan<T> Readed { get; }
    public bool IsEOF { get; }

    /// 标记已读， 以方便释放空间
    public void Consume(int count);
    
    /// 不同场景可以预览不同数组数据， 要求使用方法 就可以在预览未读取数据时将数据读取到数组中
    public bool Peek(int count, out ReadOnlySpan<T> data);

    public bool Peek(out T data);

    public bool PeekByOffset(int offset, out T data);

    /// 读取下一份数据
    public bool ReadNextBuffer(int count);
}

/// 此接口用于表明 固定长度的类型， 以便于我们可以做性能优化
public interface IFixedReaderBuffer<T> : IReaderBuffer<T> where T : struct
{
}
```

## String 对应buffer 实现

非常简单，基本就是string 的直接方法

``` csharp
public class StringReaderBuffer : IFixedReaderBuffer<char>
{
    internal string _buffer;
    internal int _offset;
    internal int _consumedCount;

    public StringReaderBuffer(string content)
    {
        _buffer = content;
    }

    public ReadOnlySpan<char> Readed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _buffer.AsSpan(_offset);
    }

    public bool IsEOF
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _offset == _buffer.Length;
    }

    public int ConsumedCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _consumedCount;
    }

    public int Index
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _offset;
    }

    public void Consume(int count)
    {
        _offset += count;
        _consumedCount += count;
    }

    public void Dispose()
    {
    }

    public bool Peek(int count, out ReadOnlySpan<char> data)
    {
        if (_offset + count > _buffer.Length)
        {
            data = default;
            return false;
        }
        data = _buffer.AsSpan(_offset, count);
        return true;
    }

    public bool Peek(out char data)
    {
        if (_offset >= _buffer.Length)
        {
            data = default;
            return false;
        }
        data = _buffer[_offset];
        return true;
    }

    public bool PeekByOffset(int offset, out char data)
    {
        var o = _offset + offset;
        if (o >= _buffer.Length)
        {
            data = default;
            return false;
        }
        data = _buffer[o];
        return true;
    }

    public bool ReadNextBuffer(int count) => false;
}
```

## TextReader 对 buffer 实现

这里使用对 TextReader 封装，主要考虑到避免 字符编码 的复杂度

该实现参考自 `System.Text.Json` 内 [ReadBufferState](https://github.com/dotnet/runtime/blob/main/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/ReadBufferState.cs)

不一定是最优方式（欢迎大家提供更优秀方式）

``` csharp
public class TextReaderBuffer : IReaderBuffer<char>
{
    internal char[] _buffer;
    internal int _offset;
    internal int _count;
    internal int _maxCount;
    internal int _consumedCount;
    private TextReader _reader;
    private bool _isFinalBlock;
    private bool _isReaded;

    public ReadOnlySpan<char> Readed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (!_isReaded)
            {
                ReadNextBuffer(1);
                _isReaded = true;
            }
            return _buffer.AsSpan(_offset, _count - _offset);
        }
    }

    public bool IsEOF
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isFinalBlock && _offset == _count;
    }

    public int ConsumedCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _consumedCount;
    }

    public int Index
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _offset;
    }

    public TextReaderBuffer(TextReader reader, int initialBufferSize)
    {
        if (initialBufferSize <= 0)
        {
            initialBufferSize = 256;
        }
        _buffer = ArrayPool<char>.Shared.Rent(initialBufferSize);
        _consumedCount = _count = _offset = 0;
        _reader = reader;
    }

    public void Consume(int count)
    {
        _offset += count;
        _consumedCount += count;
    }

    /// 调整buffer 数组大小，以便能更有效多读取数据，减少数据迁移带来的数组操作
    public void AdvanceBuffer(int count)
    {
        var remaining = _buffer.Length - _count + _offset;
        if (remaining <= (_buffer.Length / 2) && _buffer.Length != int.MaxValue)
        {
            // We have less than half the buffer available, double the buffer size.
            char[] oldBuffer = _buffer;
            int oldMaxCount = _maxCount;
            var newSize = (_buffer.Length < (int.MaxValue / 2)) ? _buffer.Length * 2 : int.MaxValue;
            while (newSize < count)
            {
                newSize *= (newSize < (int.MaxValue / 2)) ? newSize * 2 : int.MaxValue;
            }
            char[] newBuffer = ArrayPool<char>.Shared.Rent(newSize);
            // Copy the unprocessed data to the new buffer while shifting the processed bytes.
            Buffer.BlockCopy(oldBuffer, _offset, newBuffer, 0, _count - _offset);
            _buffer = newBuffer;
            // Clear and return the old buffer
            new Span<char>(oldBuffer, 0, oldMaxCount).Clear();
            ArrayPool<char>.Shared.Return(oldBuffer);
            _maxCount = _count;
            _count -= _offset;
            _offset = 0;
        }
        else if (_offset != 0)
        {
            _count -= _offset;
            // Shift the processed bytes to the beginning of buffer to make more room.
            Buffer.BlockCopy(_buffer, _offset, _buffer, 0, _count);
            _offset = 0;
        }
    }

    public void Dispose()
    {
        if (_buffer != null)
        {
            new Span<char>(_buffer, 0, _maxCount).Clear();
            char[] toReturn = _buffer;
            ArrayPool<char>.Shared.Return(toReturn);
            _buffer = null!;
        }
    }

    public bool Peek(int count, out ReadOnlySpan<char> data)
    {
        if (!_isReaded)
        {
            ReadNextBuffer(count);
            _isReaded = true;
        }
        if (!_isFinalBlock && count + _offset > _count)
        {
            ReadNextBuffer(count);
        }
        if (_offset + count > _count)
        {
            data = default;
            return false;
        }
        data = _buffer.AsSpan(_offset, count);
        return true;
    }

    public bool Peek(out char data)
    {
        if (!_isReaded)
        {
            ReadNextBuffer(1);
            _isReaded = true;
        }
        if (!_isFinalBlock && 1 + _offset > _count)
        {
            ReadNextBuffer(1);
        }
        if (_offset >= _count)
        {
            data = default;
            return false;
        }
        data = _buffer[_offset];
        return true;
    }

    public bool PeekByOffset(int offset, out char data)
    {
        var o = offset + 1;
        if (!_isReaded)
        {
            ReadNextBuffer(o);
            _isReaded = true;
        }
        if (!_isFinalBlock && o > _count)
        {
            ReadNextBuffer(o);
        }
        if (_offset >= _count)
        {
            data = default;
            return false;
        }
        data = _buffer[o];
        return true;
    }

    public bool ReadNextBuffer(int count)
    {
        if (!_isFinalBlock)
        {
            AdvanceBuffer(count);
            do
            {
                int readCount = _reader.Read(_buffer.AsSpan(_count));
                if (readCount == 0)
                {
                    _isFinalBlock = true;
                    break;
                }

                _count += readCount;
            }
            while (_count < _buffer.Length);

            if (_count > _maxCount)
            {
                _maxCount = _count;
            }
            return true;
        }
        return false;
    }
}
```

## RFC4180 csv标准 解析实现

PS: 不一定完全正确，毕竟没有完整测试过，仅供参考，哈哈

可以看到，由于要考虑不确定长度的抽象， 代码还是有一定复杂度的

``` csharp
public class CsvReader : TextDataReader<string[]>
{
    public CsvReader(string content, char separater = ',', bool fristIsHeader = false) : base(content)
    {
        Separater = separater;
        HasHeader = fristIsHeader;
    }

    public CsvReader(TextReader reader, int bufferSize = 256, char separater = ',', bool fristIsHeader = false) : base(reader, bufferSize)
    {
        Separater = separater;
        HasHeader = fristIsHeader;
    }

    public char Separater { get; private set; } = ',';

    public bool HasHeader { get; private set; }

    public string[] Header { get; private set; }

    public int FieldCount { get; private set; }

    public override bool MoveNext()
    {
        string[] row;
        if (HasHeader && Header == null)
        {
            if (!ProcessFirstRow(out row))
            {
                throw new ParseException("Missing header");
            }
            Header = row;
        }

        var r = FieldCount == 0 ? ProcessFirstRow(out row) : ProcessRow(out row);
        Current = row;
        return r;
    }

    private bool ProcessFirstRow(out string[]? row)
    {
        var r = new List<string>();
        var hasValue = false;
        while (ProcessField(out var f))
        {
            r.Add(f);
            hasValue = true;
        }
        reader.IngoreCRLF();
        row = r.ToArray();
        FieldCount = row.Length;
        return hasValue;
    }

    private bool TakeString(out string s)
    {
        if (reader.IsEOF)
        {
            throw new ParseException($"Expect some string end with '\"' at {reader.Index} but got eof");
        }

        int pos = 0;
        int len;
        ReadOnlySpan<char> remaining;
        do
        {
            remaining = reader.Readed;
            len = remaining.Length;
            var charBufferSpan = remaining[pos..];
            var i = charBufferSpan.IndexOf(Separater);
            if (i >= 0)
            {
                if (reader.PeekByOffset(i + 1, out var n) && n == Separater)
                {
                    pos += i + 2;
                    continue;
                }
                s = remaining[..i].ToString();
                reader.Consume(i + 1);
                return true;
            }
            else
            {
                pos += charBufferSpan.Length;
            }
        } while (reader.ReadNextBuffer(len));
        s = reader.Readed.ToString();
        return true;
    }

    private bool ProcessField(out string? f)
    {
        if (!reader.Peek(out var c) || reader.IngoreCRLF())
        {
            f = null;
            return false;
        }
        if (c == Separater)
        {
            f = string.Empty;
            reader.Consume(1);
            return true;
        }
        else if (c is '"')
        {
            /// 读取可能转义的字段数据
            reader.Consume(1);
            return TakeString(out f);
        }
        else
        {
            /// 读取不包含转义的普通字段数据
            var i = reader.IndexOfAny(Separater, '\r', '\n');
            if (i == 0)
            {
                f = string.Empty;
            }
            else if (i > 0)
            {
                f = reader.Readed[..i].ToString();
                reader.Consume(i);
            }
            else
            {
                f = reader.Readed.ToString();
                reader.Consume(f.Length);
            }
            if (reader.Peek(out var cc) && cc == Separater)
            {
                reader.Consume(1);
            }
            return true;
        }
    }

    private bool ProcessRow(out string[]? row)
    {
        row = new string[FieldCount];

        for (int i = 0; i < FieldCount; i++)
        {
            if (!ProcessField(out var f))
            {
                reader.IngoreCRLF();
                return false;
            }
            row[i] = f;
        }
        reader.IngoreCRLF();
        return true;
    }

}

```

至于其性能，就是最顶上的结果

达到了预期，不算浪费秃头掉发了

完整代码参考 https://github.com/fs7744/ruqu