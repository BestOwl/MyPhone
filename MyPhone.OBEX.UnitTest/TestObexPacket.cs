using GoodTimeStudio.MyPhone.OBEX.Headers;
using GoodTimeStudio.MyPhone.OBEX.UnitTest.Streams;
using Windows.Storage.Streams;

namespace GoodTimeStudio.MyPhone.OBEX.UnitTest
{
    public class TestObexPacket : IDisposable
    {
        private InMemoryPipeStream _stream;
        private DataReader _reader;
        private DataWriter _writer;

        public TestObexPacket()
        {
            _stream = new InMemoryPipeStream();
            _reader = new DataReader(_stream);
            _writer = new DataWriter(_stream);
        }

        public void Dispose()
        {
            _writer.Dispose();
            _reader.Dispose();
            _stream.Dispose();
        }

        [Fact]
        public async Task TestReadFromStream_SimplePacket()
        {
            ObexPacket originalPacket = new ObexPacket(new ObexOpcode(ObexOperation.Get, true));
            _writer.WriteBuffer(originalPacket.ToBuffer());
            await _writer.StoreAsync();

            ObexPacket readPacket = await ObexPacket.ReadFromStream(_reader);
            Assert.Equal(originalPacket, readPacket);
        }

        [Fact]
        public async Task TestReadFromStream_PacketWithHeaders()
        {
            ObexPacket originalPacket = new ObexPacket(new ObexOpcode(ObexOperation.Get, true), new ObexHeader(HeaderId.Name, "Foobar", true));
            _writer.WriteBuffer(originalPacket.ToBuffer());
            await _writer.StoreAsync();

            ObexPacket readPacket = await ObexPacket.ReadFromStream(_reader);
            Assert.Equal(originalPacket, readPacket);
        }
    }
}
