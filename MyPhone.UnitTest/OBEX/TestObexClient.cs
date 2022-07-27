using MyPhone.OBEX;
using MyPhone.UnitTest.Utilities;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Xunit;

namespace MyPhone.UnitTest.OBEX
{
    public class TestObexClient : IAsyncLifetime
    {
        private ObexClient _client;
        private InMemoryPipeStream _clientInputStream;
        private InMemoryPipeStream _clientOutputStream;

        private DataReader _serverReader;
        private DataWriter _serverWriter;

        public TestObexClient()
        {
            _clientInputStream = new InMemoryPipeStream();
            _clientOutputStream = new InMemoryPipeStream();

            _serverReader = new DataReader(_clientOutputStream); // client OutputStream = server InputStream
            _serverWriter = new DataWriter(_clientInputStream);

            _client = new ObexClient(_clientInputStream, _clientOutputStream);
        }

        public async Task InitializeAsync()
        {
            Task connect = _client.Connect(ObexServiceUuid.MessageAccess); // UUID does not matter here
            await Task.Run(async () =>
            {
                ObexPacket req = await ObexPacket.ReadFromStream(_serverReader, new ObexConnectPacket());
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

                ObexPacket response = new(
                    new ObexOpcode(ObexOperation.Success, true),
                    new BodyHeader(HeaderId.EndOfBody, "Hello world!")
                    );
                _serverWriter.WriteBuffer(response.ToBuffer());
                await _serverWriter.StoreAsync();
            });

            ObexPacket request = new(new ObexOpcode(ObexOperation.Get, true));
            ObexPacket response = await _client.RunObexRequest(request);
            Assert.True(response.Opcode.IsFinalBitSet);
            Assert.Equal(ObexOperation.Success, response.Opcode.ObexOperation);

            Assert.True(response.Headers.ContainsKey(HeaderId.Body));
            Assert.IsType<BodyHeader>(response.Headers[HeaderId.Body]);
            Assert.Equal("Hello world!", ((BodyHeader)response.Headers[HeaderId.Body]).Value);

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
                        new BodyHeader(HeaderId.Body, $"{i}")
                        );
                    _serverWriter.WriteBuffer(response.ToBuffer());
                    await _serverWriter.StoreAsync();
                }

                ObexPacket reqFinal = await ObexPacket.ReadFromStream(_serverReader);
                Assert.True(reqFinal.Opcode.IsFinalBitSet);
                Assert.Equal(ObexOperation.Get, reqFinal.Opcode.ObexOperation);

                ObexPacket responseFinal = new(
                    new ObexOpcode(ObexOperation.Success, true),
                    new BodyHeader(HeaderId.EndOfBody, $"{i}")
                    );
                _serverWriter.WriteBuffer(responseFinal.ToBuffer());
                await _serverWriter.StoreAsync();
            });

            ObexPacket request = new(new ObexOpcode(ObexOperation.Get, true));
            ObexPacket response = await _client.RunObexRequest(request);
            Assert.True(response.Opcode.IsFinalBitSet);
            Assert.Equal(ObexOperation.Success, response.Opcode.ObexOperation);

            Assert.True(response.Headers.ContainsKey(HeaderId.Body));
            Assert.IsType<BodyHeader>(response.Headers[HeaderId.Body]);
            StringBuilder sb = new();
            for (int j = 1; j <= 30; j++)
            {
                sb.Append(j);
            }
            Assert.Equal(sb.ToString(), ((BodyHeader)response.Headers[HeaderId.Body]).Value);
        }
    }
}
