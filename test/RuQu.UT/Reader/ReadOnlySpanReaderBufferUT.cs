using RuQu.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuQu.UT.Reader
{
    public struct TestData
    { 
        public int Data { get; set; }
    }

    public class ReadOnlySpanReaderBufferUT
    {
        [Fact]
        public void WhenCtor()
        {
            var b = new char[] { '1', '2', '3' };
            var r = new ReadOnlySpanReaderBuffer<char>(b);
            Assert.Equal(0, r.Index);
            Assert.Equal(0, r.ConsumedCount);
            Assert.False(r.IsEOF);
            Assert.False(r.ReadNextBuffer(1));
            Assert.False(r.ReadNextBufferAsync(1).Result);
            Assert.Equal(3, r.Readed.Length);
            for (int i = 0; i < b.Length; i++)
            {
                Assert.Equal(b[i], r.Readed[i]);
            }

            r = new ReadOnlySpanReaderBuffer<char>((ReadOnlySpan<char>)b.AsSpan());
            Assert.Equal(0, r.Index);
            Assert.Equal(0, r.ConsumedCount);
            Assert.False(r.IsEOF);
            Assert.Equal(3, r.Readed.Length);
            for (int i = 0; i < b.Length; i++)
            {
                Assert.Equal(b[i], r.Readed[i]);
            }
        }

        [Fact]
        public void WhenCtor_Class()
        {
            var b = new TestData[] { new TestData() { Data = 6 }, new TestData() { Data = 236 }, new TestData() { Data = 776 } };
            var r = new ReadOnlySpanReaderBuffer<TestData>(b);
            Assert.Equal(0, r.Index);
            Assert.Equal(0, r.ConsumedCount);
            Assert.False(r.IsEOF);
            Assert.False(r.ReadNextBuffer(1));
            Assert.False(r.ReadNextBufferAsync(1).Result);
            Assert.Equal(3, r.Readed.Length);
            for (int i = 0; i < b.Length; i++)
            {
                Assert.Equal(b[i], r.Readed[i]);
            }

            r = new ReadOnlySpanReaderBuffer<TestData>((ReadOnlySpan<TestData>)b.AsSpan());
            Assert.Equal(0, r.Index);
            Assert.Equal(0, r.ConsumedCount);
            Assert.False(r.IsEOF);
            Assert.Equal(3, r.Readed.Length);
            for (int i = 0; i < b.Length; i++)
            {
                Assert.Equal(b[i], r.Readed[i]);
            }
        }

        [Fact]
        public void PeekTest()
        {
            var b = new char[] { '1', '2', '3' };
            var r = new ReadOnlySpanReaderBuffer<char>(b);
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
        public void Peek_ClassTest()
        {
            var b = new TestData[] { new TestData() { Data = 6 }, new TestData() { Data = 236 }, new TestData() { Data = 776 } };
            var r = new ReadOnlySpanReaderBuffer<TestData>(b);
            for (int i = 0; i < b.Length; i++)
            {
                Assert.Equal(i, r.Index);
                Assert.Equal(i, r.ConsumedCount);
                Assert.True(r.Peek(out var c));
                Assert.Equal(b[i].Data, c.Data);
                r.Consume(1);
            }
            Assert.True(r.IsEOF);
            Assert.False(r.Peek(out var cc));
            Assert.Equal(0, cc.Data);
        }

        [Fact]
        public async Task PeekAsyncTest()
        {
            var b = new char[] { '1', '2', '3' };
            var r = new ReadOnlySpanReaderBuffer<char>(b);
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
        public async Task PeekAsync_ClassTest()
        {
            var b = new TestData[] { new TestData() { Data = 6 }, new TestData() { Data = 236 }, new TestData() { Data = 776 } };
            var r = new ReadOnlySpanReaderBuffer<TestData>(b);
            for (int i = 0; i < b.Length; i++)
            {
                Assert.Equal(i, r.Index);
                Assert.Equal(i, r.ConsumedCount);
                var c = await r.PeekAsync();
                Assert.True(c.HasValue);
                Assert.Equal(b[i].Data, c.Value.Data);
                r.Consume(1);
            }
            Assert.True(r.IsEOF);
            var cc = await r.PeekAsync();
            Assert.False(cc.HasValue);
            Assert.Null(cc);
        }

        [Fact]
        public void PeekCountTest()
        {
            var b = new char[] { '1', '2', '3' };
            var r = new ReadOnlySpanReaderBuffer<char>(b);

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
        public void PeekCount_ClassTest()
        {
            var b = new TestData[] { new TestData() { Data = 6 }, new TestData() { Data = 236 }, new TestData() { Data = 776 } };
            var r = new ReadOnlySpanReaderBuffer<TestData>(b);

            Assert.False(r.Peek(0, out var data));
            Assert.Equal(0, data.Length);
            Assert.True(data.IsEmpty);

            Assert.True(r.Peek(1, out data));
            Assert.Equal(1, data.Length);
            for (int i = 0; i < data.Length; i++)
            {
                Assert.Equal(b[i].Data, data[i].Data);
            }

            Assert.True(r.Peek(2, out data));
            Assert.Equal(2, data.Length);
            for (int i = 0; i < data.Length; i++)
            {
                Assert.Equal(b[i].Data, data[i].Data);
            }

            Assert.True(r.Peek(3, out data));
            Assert.Equal(3, data.Length);
            for (int i = 0; i < data.Length; i++)
            {
                Assert.Equal(b[i].Data, data[i].Data);
            }

            Assert.False(r.Peek(4, out data));
            Assert.Equal(0, data.Length);
            Assert.True(data.IsEmpty);
        }

        [Fact]
        public async Task PeekCountAsyncTest()
        {
            var b = new char[] { '1', '2', '3' };
            var r = new ReadOnlySpanReaderBuffer<char>(b);

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
        public async Task PeekCountAsync_ClassTest()
        {
            var b = new TestData[] { new TestData() { Data = 6 }, new TestData() { Data = 236 }, new TestData() { Data = 776 } };
            var r = new ReadOnlySpanReaderBuffer<TestData>(b);

            var data = await r.PeekAsync(0);
            Assert.False(data.HasValue);

            data = await r.PeekAsync(1);
            Assert.True(data.HasValue);
            Assert.Equal(1, data.Value.Length);
            for (int i = 0; i < data.Value.Length; i++)
            {
                Assert.Equal(b[i].Data, data.Value.Span[i].Data);
            }

            data = await r.PeekAsync(2);
            Assert.True(data.HasValue);
            Assert.Equal(2, data.Value.Length);
            for (int i = 0; i < data.Value.Length; i++)
            {
                Assert.Equal(b[i].Data, data.Value.Span[i].Data);
            }

            data = await r.PeekAsync(3);
            Assert.True(data.HasValue);
            Assert.Equal(3, data.Value.Length);
            for (int i = 0; i < data.Value.Length; i++)
            {
                Assert.Equal(b[i].Data, data.Value.Span[i].Data);
            }

            data = await r.PeekAsync(4);
            Assert.False(data.HasValue);
        }

        [Fact]
        public void PeekByOffsetTest()
        {
            var b = new char[] { '1', '2', '3' };
            var r = new ReadOnlySpanReaderBuffer<char>(b);
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
        public void PeekByOffset_ClassTest()
        {
            var b = new TestData[] { new TestData() { Data = 6 }, new TestData() { Data = 236 }, new TestData() { Data = 776 } };
            var r = new ReadOnlySpanReaderBuffer<TestData>(b);
            for (int i = 0; i < b.Length; i++)
            {
                Assert.Equal(0, r.Index);
                Assert.Equal(0, r.ConsumedCount);
                Assert.True(r.PeekByOffset(i, out var c));
                Assert.Equal(b[i].Data, c.Data);
            }
            Assert.Equal(0, r.ConsumedCount);
            Assert.False(r.IsEOF);
            Assert.False(r.PeekByOffset(3, out var cc));
            Assert.Equal(0, cc.Data);
        }

        [Fact]
        public async Task PeekByOffsetAsyncTest()
        {
            var b = new char[] { '1', '2', '3' };
            var r = new ReadOnlySpanReaderBuffer<char>(b);
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

        [Fact]
        public async Task PeekByOffsetAsync_ClassTest()
        {
            var b = new TestData[] { new TestData() { Data = 6 }, new TestData() { Data = 236 }, new TestData() { Data = 776 } };
            var r = new ReadOnlySpanReaderBuffer<TestData>(b);
            for (int i = 0; i < b.Length; i++)
            {
                Assert.Equal(0, r.Index);
                Assert.Equal(0, r.ConsumedCount);
                var c = await r.PeekByOffsetAsync(i);
                Assert.True(c.HasValue);
                Assert.Equal(b[i].Data, c.Value.Data);
            }
            Assert.Equal(0, r.ConsumedCount);
            Assert.False(r.IsEOF);
            var cc = await r.PeekByOffsetAsync(3);
            Assert.False(cc.HasValue);
        }
    }
}