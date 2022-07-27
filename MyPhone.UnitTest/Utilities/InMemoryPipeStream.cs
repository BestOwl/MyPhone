using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace MyPhone.UnitTest.Utilities
{
    public class InMemoryPipeStream : IInputStream, IOutputStream
    {
        private ManualResetEvent _dataReady = new ManualResetEvent(false);
        private ConcurrentQueue<BufferCrate> _buffers = new ConcurrentQueue<BufferCrate>();

        public bool DataAvailable { get { return !_buffers.IsEmpty; } }

        /// <summary>
        /// Asynchronous read timeout in milliseconds 
        /// </summary>
        public int Timeout { get; set; } = 2000; // default to 2 seconds

        public void Dispose()
        {
            _dataReady.Dispose();
        }

        public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
        {
            return AsyncInfo.Run<IBuffer, uint>((__, progressReport) => Task.Run(() =>
            {
                _dataReady.WaitOne(Timeout);
                BufferCrate crate;
                if (_buffers.TryPeek(out crate!))
                {
                    if (crate.CopyTo(buffer, count))
                    {
                        _buffers.TryDequeue(out _);
                    }
                }

                if (!DataAvailable)
                {
                    _dataReady.Reset();
                }

                progressReport.Report(buffer.Length);
                return buffer;
            }));
        }

        private class BufferCrate
        {
            public IBuffer Buffer { get; }
            public uint RemainingLength { get => Buffer.Length - _offset; }
            private uint _offset = 0;

            public BufferCrate(IBuffer buffer)
            {
                Buffer = buffer;
            }

            public bool CopyTo(IBuffer dstBuffer, uint count)
            {
                uint bytesToCopy = Math.Min(RemainingLength, count);
                Buffer.CopyTo(_offset, dstBuffer, 0, bytesToCopy);
                dstBuffer.Length = bytesToCopy;
                _offset += bytesToCopy;
                return RemainingLength == 0;
            }
        }

        public IAsyncOperation<bool> FlushAsync()
        {
            return AsyncInfo.Run(_ => Task.Run(() =>
            {
                return true;
            }));
        }

        public IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
        {
            return AsyncInfo.Run<uint, uint>((_, progressReport) => Task.Run(() =>
            {
                IBuffer copy = new Windows.Storage.Streams.Buffer(buffer.Capacity);
                buffer.CopyTo(copy);
                copy.Length = buffer.Length;
                _buffers.Enqueue(new BufferCrate(copy));
                progressReport.Report(buffer.Length);
                _dataReady.Set();
                return buffer.Length;
            }));
        }
    }
}
