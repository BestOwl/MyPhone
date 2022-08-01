using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Windows.Storage.Streams;

namespace GoodTimeStudio.MyPhone.OBEX.UnitTest.Streams
{
    public class TestInMemoryPipeStream : IDisposable
    {
        private InMemoryPipeStream _stream;
        private DataReader _reader;
        private DataWriter _writer;

        public TestInMemoryPipeStream()
        {
            _stream = new InMemoryPipeStream();
            _reader = new DataReader(_stream);
            _writer = new DataWriter(_stream);
        }

        public void Dispose()
        {
            _reader.Dispose();
            _writer.Dispose();
            _stream.Dispose();
        }

        [Fact]
        public async Task TestBlockingRead()
        {
            Task<uint> loadTask = _reader.LoadAsync(64).AsTask();
            await Task.Delay(10);
            Assert.False(loadTask.IsCompleted);

            _writer.WriteBytes(new byte[] { 0x80, 0x81, 0x82, 0x84 });
            await _writer.StoreAsync();
            Assert.Equal((uint)4, await loadTask);
        }

        [Fact]
        public async Task TestDataIntegrity()
        {
            uint bytes1 = _writer.WriteString("Hello world! ");
            await _writer.StoreAsync();

            uint bytes2 = _writer.WriteString("Foobar! ");
            bytes2 += _writer.WriteString("TestDataIntegrity!\n");
            await _writer.StoreAsync();

            uint loaded = await _reader.LoadAsync(bytes1);
            Assert.Equal(bytes1, loaded);
            Assert.Equal("Hello world! ", _reader.ReadString(loaded));

            loaded = await _reader.LoadAsync(bytes2);
            Assert.Equal(bytes2, loaded);
            Assert.Equal("Foobar! TestDataIntegrity!\n", _reader.ReadString(loaded));
        }

        [Fact(Timeout = 2000)]
        public async Task TestTimeout()
        {
            _stream.Timeout = 1000;
            uint loaded = await _reader.LoadAsync(8);
            Assert.Equal((uint)0, loaded);
        }
    }
}
