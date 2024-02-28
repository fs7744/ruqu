using System.Buffers;
using System.Runtime.InteropServices;

namespace RuQu.Writer
{
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

        public override MemoryHandle Pin(int elementIndex = 0)
        {
            if (elementIndex < 0 || elementIndex >= _length)
                throw new ArgumentOutOfRangeException(nameof(elementIndex));
            return new MemoryHandle(_pointer + elementIndex);
        }

        public override void Unpin() { }

        protected override void Dispose(bool disposing) { }
    }
}
