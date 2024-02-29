using RuQu.Reader;
using System.Text;

namespace RuQu.UT.Reader
{
    public class MemoryMappedFileReaderBufferUT
    {
        string file = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString());
        byte[] b;

        public MemoryMappedFileReaderBufferUT()
        {
            File.WriteAllText(file, "123");
            b = Encoding.UTF8.GetBytes("123");
        }

        [Fact]
        public void WhenCtor()
        {
            var r = new MemoryMappedFileReaderBuffer(file);
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
            Assert.Equal(3, r.ReadedMemory.Length);
            for (int i = 0; i < b.Length; i++)
            {
                Assert.Equal(b[i], r.ReadedMemory.Span[i]);
            }
        }

        [Fact]
        public void PeekTest()
        {
            var r = new MemoryMappedFileReaderBuffer(file);
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
            var r = new MemoryMappedFileReaderBuffer(file);
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
            var r = new MemoryMappedFileReaderBuffer(file);

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
            var r = new MemoryMappedFileReaderBuffer(file);

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
            var r = new MemoryMappedFileReaderBuffer(file);
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
            var r = new MemoryMappedFileReaderBuffer(file);
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
    }
}