using GoodTimeStudio.MyPhone.OBEX.Headers;
using GoodTimeStudio.MyPhone.OBEX.UnitTest.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Storage.Streams;
using Xunit.Abstractions;

namespace GoodTimeStudio.MyPhone.OBEX.UnitTest
{
    public class TestObexClient : IAsyncLifetime
    {
        private readonly ITestOutputHelper _output;

        private ObexClient _client;
        private InMemoryPipeStream _clientInputStream;
        private InMemoryPipeStream _clientOutputStream;

        private DataReader _serverReader;
        private DataWriter _serverWriter;

        public TestObexClient(ITestOutputHelper outputHelper)
        {
            _output = outputHelper;

            _clientInputStream = new InMemoryPipeStream();
            _clientOutputStream = new InMemoryPipeStream();
            _clientInputStream.Timeout = int.MaxValue;
            _clientOutputStream.Timeout = int.MaxValue;

            _serverReader = new DataReader(_clientOutputStream); // client OutputStream = server InputStream
            _serverWriter = new DataWriter(_clientInputStream);

            _client = new ObexClient(_clientInputStream, _clientOutputStream);
        }

        public async Task InitializeAsync()
        {
            Task connect = _client.ConnectAsync(ObexServiceUuid.MessageAccess); // UUID does not matter here
            await Task.Run(async () =>
            {
                ObexConnectPacket req = await ObexPacket.ReadFromStream<ObexConnectPacket>(_serverReader);
                Assert.True(req.Opcode.IsFinalBitSet);
                Assert.Equal(ObexOperation.Connect, req.Opcode.ObexOperation);

                ObexConnectPacket success = new();
                success.Opcode = new ObexOpcode(ObexOperation.Success, true);
                _serverWriter.WriteBuffer(success.ToBuffer());
                await _serverWriter.StoreAsync();
            });
            await connect;
        }

        public Task DisposeAsync()
        {
            _serverWriter.Dispose();
            _serverReader.Dispose();

            _clientOutputStream.Dispose();
            _clientInputStream.Dispose();

            return Task.CompletedTask;
        }

        [Fact]
        public async Task TestRunObexRequest_GetSingleResponseBody()
        {
            Task fakeServerTask = Task.Run(async () =>
            {
                ObexPacket req = await ObexPacket.ReadFromStream(_serverReader);
                Assert.True(req.Opcode.IsFinalBitSet);
                Assert.Equal(ObexOperation.Get, req.Opcode.ObexOperation);
                _output.WriteLine("Fake server: request received.");

                ObexPacket response = new(
                    new ObexOpcode(ObexOperation.Success, true),
                    new ObexHeader(HeaderId.EndOfBody, "Hello world!", true, Encoding.UTF8)
                    );
                IBuffer buf = response.ToBuffer();
                _output.WriteLine("Response packet:");
                _output.WriteLine(BitConverter.ToString(buf.ToArray()));
                _serverWriter.WriteBuffer(buf);
                await _serverWriter.StoreAsync();
            });

            ObexPacket request = new(new ObexOpcode(ObexOperation.Get, true));

            _output.WriteLine("Request packet:");
            var buf = request.ToBuffer().ToArray();
            _output.WriteLine(BitConverter.ToString(buf));

            ObexPacket response = await _client.RunObexRequestAsync(request);
            Assert.True(response.Opcode.IsFinalBitSet);
            Assert.Equal(ObexOperation.Success, response.Opcode.ObexOperation);
            Assert.Equal("Hello world!", response.GetBodyContentAsUtf8String(true));

            await fakeServerTask;
        }

        [Fact]
        public async Task TestRunObexRequest_GetMultipleResponseBody()
        {
            _clientInputStream.Timeout = int.MaxValue;
            _clientOutputStream.Timeout = int.MaxValue;

            int i = 1;
            Task fakeServerTask = Task.Run(async () =>
            {
                for (; i < 30; i++)
                {
                    ObexPacket req = await ObexPacket.ReadFromStream(_serverReader);
                    Assert.True(req.Opcode.IsFinalBitSet);
                    Assert.Equal(ObexOperation.Get, req.Opcode.ObexOperation);

                    ObexPacket response = new(
                        new ObexOpcode(ObexOperation.Continue, true),
                        new ObexHeader(HeaderId.Body, $"{i}", false, Encoding.BigEndianUnicode)
                        );
                    _serverWriter.WriteBuffer(response.ToBuffer());
                    await _serverWriter.StoreAsync();
                }

                ObexPacket reqFinal = await ObexPacket.ReadFromStream(_serverReader);
                Assert.True(reqFinal.Opcode.IsFinalBitSet);
                Assert.Equal(ObexOperation.Get, reqFinal.Opcode.ObexOperation);

                ObexPacket responseFinal = new(
                    new ObexOpcode(ObexOperation.Success, true),
                    new ObexHeader(HeaderId.EndOfBody, $"{i}", false, Encoding.BigEndianUnicode)
                    );
                _serverWriter.WriteBuffer(responseFinal.ToBuffer());
                await _serverWriter.StoreAsync();
            });

            ObexPacket request = new(new ObexOpcode(ObexOperation.Get, true));
            ObexPacket response = await _client.RunObexRequestAsync(request);
            Assert.True(response.Opcode.IsFinalBitSet);
            Assert.Equal(ObexOperation.Success, response.Opcode.ObexOperation);

            StringBuilder sb = new();
            for (int j = 1; j <= 30; j++)
            {
                sb.Append(j);
            }
            Assert.Equal(sb.ToString(), response.GetBodyContentAsUnicodeString(false));
        }
    }
}
