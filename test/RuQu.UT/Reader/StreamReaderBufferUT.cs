using RuQu.Reader;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;

namespace RuQu.UT.Reader
{
    public class StreamReaderBufferUT
    {
        [Fact]
        public void WhenCtor()
        {
            var b = Encoding.UTF8.GetBytes("123");
            var r = new StreamReaderBuffer(new MemoryStream(b), 1);
            Assert.Equal(0, r.Index);
            Assert.Equal(0, r.ConsumedCount);
            Assert.False(r.IsEOF);
            Assert.True(r.ReadNextBuffer(1));
            Assert.False(r.ReadNextBufferAsync(1).Result);
            Assert.Equal(3, r.Readed.Length);
            for (int i = 0; i < b.Length; i++)
            {
                Assert.Equal(b[i], r.Readed[i]);
            }
            Assert.Equal(3, r.ReadedMemory.Length);
            for (int i = 0; i < b.Length; i++)
            {
                Assert.Equal(b[i], r.ReadedMemory.Span[i]);
            }
        }

        [Fact]
        public void PeekTest()
        {
            var b = Encoding.UTF8.GetBytes("123");
            var r = new StreamReaderBuffer(new MemoryStream(b), 1);
            for (int i = 0; i < b.Length; i++)
            {
                Assert.Equal(i, r.Index);
                Assert.Equal(i, r.ConsumedCount);
                Assert.True(r.Peek(out var c));
                Assert.Equal(b[i], c);
                r.Consume(1);
            }
            Assert.Equal(3, r.ConsumedCount);
            Assert.True(r.IsEOF);
            Assert.False(r.Peek(out var cc));
            Assert.Equal(byte.MinValue, cc);
        }


        [Fact]
        public async Task PeekAsyncTest()
        {
            var b = Encoding.UTF8.GetBytes("123");
            var r = new StreamReaderBuffer(new MemoryStream(b), 1);
            for (int i = 0; i < b.Length; i++)
            {
                Assert.Equal(i, r.Index);
                Assert.Equal(i, r.ConsumedCount);
                var c = await r.PeekAsync();
                Assert.True(c.HasValue);
                Assert.Equal(b[i], c);
                r.Consume(1);
            }
            Assert.Equal(3, r.ConsumedCount);
            Assert.True(r.IsEOF);
            var cc = await r.PeekAsync();
            Assert.False(cc.HasValue);
            Assert.Null(cc);
        }


        [Fact]
        public void PeekCountTest()
        {
            var b = Encoding.UTF8.GetBytes("123");
            var r = new StreamReaderBuffer(new MemoryStream(b), 1);

            Assert.False(r.Peek(0, out var data));
            Assert.Equal(0, data.Length);
            Assert.True(data.IsEmpty);

            Assert.True(r.Peek(1, out data));
            Assert.Equal(1, data.Length);
            Assert.Equal("1", Encoding.UTF8.GetString(data));

            Assert.True(r.Peek(2, out data));
            Assert.Equal(2, data.Length);
            Assert.Equal("12", Encoding.UTF8.GetString(data));

            Assert.True(r.Peek(3, out data));
            Assert.Equal(3, data.Length);
            Assert.Equal("123", Encoding.UTF8.GetString(data));

            Assert.False(r.Peek(4, out data));
            Assert.Equal(0, data.Length);
            Assert.True(data.IsEmpty);
        }

        [Fact]
        public async Task PeekCountAsyncTest()
        {
            var b = Encoding.UTF8.GetBytes("123");
            var r = new StreamReaderBuffer(new MemoryStream(b), 1);

            var data = await r.PeekAsync(0);
            Assert.False(data.HasValue);

            data = await r.PeekAsync(1);
            Assert.True(data.HasValue);
            Assert.Equal(1, data.Value.Length);
            Assert.Equal("1", Encoding.UTF8.GetString(data.Value.Span));

            data = await r.PeekAsync(2);
            Assert.True(data.HasValue);
            Assert.Equal(2, data.Value.Length);
            Assert.Equal("12", Encoding.UTF8.GetString(data.Value.Span));

            data = await r.PeekAsync(3);
            Assert.True(data.HasValue);
            Assert.Equal(3, data.Value.Length);
            Assert.Equal("123", Encoding.UTF8.GetString(data.Value.Span));

            data = await r.PeekAsync(4);
            Assert.False(data.HasValue);
        }

        [Fact]
        public void PeekByOffsetTest()
        {
            var b = Encoding.UTF8.GetBytes("123");
            var r = new StreamReaderBuffer(new MemoryStream(b), 1);
            for (int i = 0; i < b.Length; i++)
            {
                Assert.Equal(0, r.Index);
                Assert.Equal(0, r.ConsumedCount);
                Assert.True(r.PeekByOffset(i, out var c));
                Assert.Equal(b[i], c);
            }
            Assert.Equal(0, r.ConsumedCount);
            Assert.False(r.IsEOF);
            Assert.False(r.PeekByOffset(3, out var cc));
            Assert.Equal(byte.MinValue, cc);
        }

        [Fact]
        public async Task PeekByOffsetAsyncTest()
        {
            var b = Encoding.UTF8.GetBytes("123");
            var r = new StreamReaderBuffer(new MemoryStream(b), 1);
            for (int i = 0; i < b.Length; i++)
            {
                Assert.Equal(0, r.Index);
                Assert.Equal(0, r.ConsumedCount);
                var c = await r.PeekByOffsetAsync(i);
                Assert.True(c.HasValue);
                Assert.Equal(b[i], c);
            }
            Assert.Equal(0, r.ConsumedCount);
            Assert.False(r.IsEOF);
            var cc = await r.PeekByOffsetAsync(3);
            Assert.False(cc.HasValue);
        }

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_buffer")]
        extern static ref byte[] GetSet_buffer(StreamReaderBuffer c);


        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_offset")]
        extern static ref int GetSet_offset(StreamReaderBuffer c);
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_count")]
        extern static ref int GetSet_count(StreamReaderBuffer c);

        [Fact]
        public void AdvanceBufferTest()
        {
            var b = Encoding.UTF8.GetBytes("123");
            var r = new StreamReaderBuffer(new MemoryStream(b), 1);
            ref byte[] f = ref GetSet_buffer(r);
            ref int o = ref GetSet_offset(r);
            ref int c = ref GetSet_count(r);
            var l = f.Length;
            Assert.True(l > 0);
            Assert.Equal(0, c);
            Assert.Equal(0, o);
            r.AdvanceBuffer(0);
            Assert.Equal(0, c);
            Assert.Equal(0, o);
            Assert.Equal(l, f.Length);

            for (int i = 0; i < l; i++)
            {
                f[i] = (byte)i;
            }

            o = 2;
            c = 5;
            r.AdvanceBuffer(1);
            Assert.Equal(l, f.Length);
            Assert.Equal((byte)2, f[0]);
            Assert.Equal((byte)3, f[1]);
            Assert.Equal((byte)4, f[2]);
            Assert.Equal((byte)3, f[3]);

            r.AdvanceBuffer(1023);
            Assert.Equal(1024, f.Length);
            Assert.Equal((byte)2, f[0]);
            Assert.Equal((byte)3, f[1]);
            Assert.Equal((byte)4, f[2]);
            Assert.Equal((byte)0, f[3]);
            var span = r.Readed;
            Assert.Equal(6, span.Length);

            Assert.Equal((byte)2, span[0]);
            Assert.Equal((byte)3, span[1]);
            Assert.Equal((byte)4, span[2]);
            Assert.Equal((byte)49, span[3]);
            Assert.Equal((byte)50, span[4]);
            Assert.Equal((byte)51, span[5]);
        }
    }
}