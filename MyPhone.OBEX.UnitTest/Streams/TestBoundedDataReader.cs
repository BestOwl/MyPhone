using GoodTimeStudio.MyPhone.OBEX.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Xunit;

namespace GoodTimeStudio.MyPhone.OBEX.UnitTest.Streams
{
    public class TestBoundedDataReader : IDisposable
    {
        private InMemoryPipeStream _stream;
        private DataWriter _writer;
        private DataReader _actualReader;

        public TestBoundedDataReader()
        {
            _stream = new InMemoryPipeStream();
            _writer = new DataWriter(_stream);
            _actualReader = new DataReader(_stream);
        }

        public void Dispose()
        {
            _writer.Dispose();
            _actualReader.Dispose();
            _stream.Dispose();
        }

        [Theory]
        [InlineData(0, 0, 1)]
        [InlineData(5, 4, 5)]
        [InlineData(4, 4, 5)]
        [InlineData(4, 4, 8)]
        [InlineData(8, 4, 8)]
        [InlineData(12, 4, 8)]
        public async Task TestQuotaExceeded(uint numberOfBytesExists, uint readQuota, uint numberOfBytesToRead)
        {
            byte[] write_buf = new byte[numberOfBytesExists];
            Random.Shared.NextBytes(write_buf);
            _writer.WriteBytes(write_buf);
            uint written = await _writer.StoreAsync();

            await _actualReader.LoadAsync(written);
            BoundedDataReader reader = new(_actualReader, readQuota);
            Assert.Throws<BoundedDataReaderQuotaExceedException>(() => reader.ReadBuffer(numberOfBytesToRead));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        public async Task TestQuotaReached(uint readQuota)
        {
            byte[] write_buf = new byte[readQuota];
            Random.Shared.NextBytes(write_buf);
            _writer.WriteBytes(write_buf);
            await _writer.StoreAsync();

            await _actualReader.LoadAsync(readQuota);
            BoundedDataReader reader = new(_actualReader, readQuota);
            byte[] read_buf = new byte[readQuota];
            reader.ReadBytes(read_buf);
            Assert.Equal(write_buf, read_buf);
        }
    }
}
