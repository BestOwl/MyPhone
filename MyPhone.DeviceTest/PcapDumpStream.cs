using SharpPcap;
using SharpPcap.LibPcap;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace GoodTimeStudio.MyPhone.DeviceTest
{
    public class PcapDumpStream : IInputStream, IOutputStream
    {
        private IInputStream _inputStream;
        private IOutputStream _outputStream;
        private CaptureFileWriterDevice _capture;

        public PcapDumpStream(IInputStream inputStreamToDump, IOutputStream outputStreamToDump, string dumpFileName)
        {
            _inputStream = inputStreamToDump;
            _outputStream = outputStreamToDump;
            _capture = new CaptureFileWriterDevice(dumpFileName);
            _capture.Open(new DeviceConfiguration());
        }

        public void Dispose()
        {
            _outputStream.Dispose();
            _inputStream.Dispose();
            _capture.Dispose();
        }

        public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
        {
            return AsyncInfo.Run<IBuffer, uint>((cts, progress) => Task.Run(async () =>
            {
                IBuffer buf = await _inputStream.ReadAsync(buffer, count, options);
                progress.Report(buf.Length);
                _capture.Write(buf.ToArray());
                return buf;
            }));
        }

        public IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
        {
            _capture.Write(buffer.ToArray());
            return _outputStream.WriteAsync(buffer);
        }

        public IAsyncOperation<bool> FlushAsync()
        {
            return _outputStream.FlushAsync();
        }
    }
}
