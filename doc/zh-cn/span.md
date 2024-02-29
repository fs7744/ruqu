# 骚操作之 持有 ReadOnlySpan 数据

`ReadOnlySpan<T>` 可以说现在高性能操作的重要基石

其原理有兴趣的同学可以看 2018 的介绍[`Span<T>`文章](https://learn.microsoft.com/zh-cn/archive/msdn-magazine/2018/january/csharp-all-about-span-exploring-a-new-net-mainstay)

其为了保障大家安全使用做了相应的限制

那么有没方法绕过呢？

## 在class中持有 ReadOnlySpan

直接持有是不可能的，本身为 `ref struct` 就保障了大家写不出持有它的代码

但是我们可以玩骚操作，无法持有你，我们可以创造一个一模一样的你

如下面代码，我们获取span 对应的指针

``` csharp
public unsafe class ReadOnlySpanReaderBuffer<T> 
{
    internal void* _buffer;
    internal int _length;

    public ReadOnlySpanReaderBuffer(Span<T> span)
    {
        _buffer = Unsafe.AsPointer(ref span.GetPinnableReference());
        _length = span.Length;
    }

    public ReadOnlySpanReaderBuffer(ReadOnlySpan<T> span)
    {
        _buffer = Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
        _length = span.Length;
    }
```

在需要使用的时候通过指针重新创建一个一模一样的span

``` csharp
public ReadOnlySpan<T> Readed
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => new ReadOnlySpan<T>(_buffer, _length);
}
```

## 将 Span<T> 转换成 Memory<T>

同样出于安全考虑，默认`Span<T>` 无法转换成 `Memory<T>`

但是我们可以玩骚操作，无法转换你，我们可以创造一个

首先我们需要建立一个Memory的基础类, 通过它来告诉 `Memory<T>` 如何拿去我们从 `Span<T>`里面偷出来的指针

```csharp
public sealed unsafe class UnmanagedMemoryManager<T> : MemoryManager<T>
{
    private readonly T* _pointer;
    private readonly int _length;

    public UnmanagedMemoryManager(Span<T> span)
    {
        fixed (T* ptr = &MemoryMarshal.GetReference(span))
        {
            _pointer = ptr;
            _length = span.Length;
        }
    }

    public UnmanagedMemoryManager(T* pointer, int length)
    {
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
        _pointer = pointer;
        _length = length;
    }

    public UnmanagedMemoryManager(nint pointer, int length) : this((T*)pointer.ToPointer(), length) { }

    public override Span<T> GetSpan() => new Span<T>(_pointer, _length);

    // 一切的关键就在这个方法
    public override MemoryHandle Pin(int elementIndex = 0)
    {
        if (elementIndex < 0 || elementIndex >= _length)
            throw new ArgumentOutOfRangeException(nameof(elementIndex));
        return new MemoryHandle(_pointer + elementIndex);
    }

    public override void Unpin() { }

    protected override void Dispose(bool disposing) { }
}

```

在需要使用的时候通过指针重新创建一个一模一样的Memory

``` csharp
public ReadOnlyMemory<T> ReadedMemory
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => new UnmanagedMemoryManager<T>((IntPtr)_buffer, _length).Memory;
}
```

## 道路千万条，安全第一条，大家慎用骚操作啊
