using RuQu.Reader;
using System.Runtime.CompilerServices;
using System.Text;

namespace RuQu.UT.Reader
{
    public class TextReaderBufferUT
    {
        [Fact]
        public void WhenCtor()
        {
            var b = "123";
            var r = new TextReaderBuffer(new StringReader(b), 1);
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
            var b = "123";
            var r = new TextReaderBuffer(new StringReader(b), 1);
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
            Assert.Equal(char.MinValue, cc);
        }

        [Fact]
        public async Task PeekAsyncTest()
        {
            var b = "123";
            var r = new TextReaderBuffer(new StringReader(b), 1);
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
            var b = "123";
            var r = new TextReaderBuffer(new StringReader(b), 1);

            Assert.False(r.Peek(0, out var data));
            Assert.Equal(0, data.Length);
            Assert.True(data.IsEmpty);

            Assert.True(r.Peek(1, out data));
            Assert.Equal(1, data.Length);
            Assert.Equal("1", data.ToString());

            Assert.True(r.Peek(2, out data));
            Assert.Equal(2, data.Length);
            Assert.Equal("12", data.ToString());

            Assert.True(r.Peek(3, out data));
            Assert.Equal(3, data.Length);
            Assert.Equal("123", data.ToString());

            Assert.False(r.Peek(4, out data));
            Assert.Equal(0, data.Length);
            Assert.True(data.IsEmpty);
        }

        [Fact]
        public async Task PeekCountAsyncTest()
        {
            var b = "123";
            var r = new TextReaderBuffer(new StringReader(b), 1);

            var data = await r.PeekAsync(0);
            Assert.False(data.HasValue);

            data = await r.PeekAsync(1);
            Assert.True(data.HasValue);
            Assert.Equal(1, data.Value.Length);
            Assert.Equal("1", data.ToString());

            data = await r.PeekAsync(2);
            Assert.True(data.HasValue);
            Assert.Equal(2, data.Value.Length);
            Assert.Equal("12", data.ToString());

            data = await r.PeekAsync(3);
            Assert.True(data.HasValue);
            Assert.Equal(3, data.Value.Length);
            Assert.Equal("123", data.ToString());

            data = await r.PeekAsync(4);
            Assert.False(data.HasValue);
        }

        [Fact]
        public void PeekByOffsetTest()
        {
            var b = "123";
            var r = new TextReaderBuffer(new StringReader(b), 1);
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
            Assert.Equal(char.MinValue, cc);
        }

        [Fact]
        public async Task PeekByOffsetAsyncTest()
        {
            var b = "123";
            var r = new TextReaderBuffer(new StringReader(b), 1);
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
        extern static ref char[] GetSet_buffer(TextReaderBuffer c);


        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_offset")]
        extern static ref int GetSet_offset(TextReaderBuffer c);
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_count")]
        extern static ref int GetSet_count(TextReaderBuffer c);
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_isReaded")]
        extern static ref bool GetSet_isReaded(TextReaderBuffer c);

        [Fact]
        public void AdvanceBufferTest()
        {
            var r = new TextReaderBuffer(new StringReader(""), 1);
            ref char[] f = ref GetSet_buffer(r);
            ref int o = ref GetSet_offset(r);
            ref int c = ref GetSet_count(r);
            ref bool rd = ref GetSet_isReaded(r);
            rd = true;
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
                f[i] = i.ToString().Last();
            }

            o = 2;
            c = 5;
            r.AdvanceBuffer(1);
            Assert.Equal(l, f.Length);
            Assert.Equal('2', f[0]);
            Assert.Equal('3', f[1]);
            Assert.Equal('4', f[2]);
            Assert.Equal('3', f[3]);

            r.AdvanceBuffer(1023);
            Assert.Equal(1024, f.Length);
            Assert.Equal('2', f[0]);
            Assert.Equal('3', f[1]);
            Assert.Equal('4', f[2]);
            Assert.Equal(char.MinValue, f[3]);
        }
    }
}