using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage.Streams;

namespace GoodTimeStudio.MyPhone.OBEX.Streams
{
    /// <summary>
    /// A data reader with a limit on the number of bytes that can be read.
    /// 
    /// A <see cref="DataReaderQuotaExceedException"/> will be thrown if the number of bytes a operation 
    /// try to read exceed the limit.
    /// </summary>
    public class BoundedDataReader : IDataReader
    {
        private IDataReader _reader;
        public uint RemainingQuota;

        public BoundedDataReader(IDataReader actualReader, uint readQuota)
        {
            _reader = actualReader;
            RemainingQuota = readQuota;
        }

        private void ConsumeBytes(uint numberOfBytes)
        {
            if (RemainingQuota < numberOfBytes)
            {
                throw new BoundedDataReaderQuotaExceedException(RemainingQuota + numberOfBytes, numberOfBytes);
            }
            RemainingQuota -= numberOfBytes;
        }

        public byte ReadByte()
        {
            ConsumeBytes(sizeof(byte));
            return _reader.ReadByte();
        }

        public void ReadBytes(byte[] value)
        {
            ConsumeBytes((uint)value.Length);
            _reader.ReadBytes(value);
        }

        public IBuffer ReadBuffer(uint length)
        {
            ConsumeBytes(length);
            return _reader.ReadBuffer(length);
        }

        public bool ReadBoolean()
        {
            ConsumeBytes(sizeof(bool));
            return _reader.ReadBoolean();
        }

        public Guid ReadGuid()
        {
            // TODO: what is the size of Guid
            throw new NotImplementedException();
        }

        public short ReadInt16()
        {
            ConsumeBytes(sizeof(short));
            return _reader.ReadInt16();
        }

        public int ReadInt32()
        {
            ConsumeBytes(sizeof(int));
            return _reader.ReadInt32();
        }

        public long ReadInt64()
        {
            ConsumeBytes(sizeof(long));
            return _reader.ReadInt64();
        }

        public ushort ReadUInt16()
        {
            ConsumeBytes(sizeof(ushort));
            return _reader.ReadUInt16();
        }

        public uint ReadUInt32()
        {
            ConsumeBytes(sizeof(uint));
            return _reader.ReadUInt32();
        }

        public ulong ReadUInt64()
        {
            ConsumeBytes(sizeof(ulong));
            return _reader.ReadUInt64();
        }

        public float ReadSingle()
        {
            ConsumeBytes(sizeof(float));
            return _reader.ReadSingle();
        }

        public double ReadDouble()
        {
            ConsumeBytes(sizeof(double));
            return _reader.ReadDouble();
        }

        public string ReadString(uint codeUnitCount)
        {
            ConsumeBytes(codeUnitCount);
            return _reader.ReadString(codeUnitCount);
        }

        public DateTimeOffset ReadDateTime()
        {
            throw new NotImplementedException();
        }

        public TimeSpan ReadTimeSpan()
        {
            throw new NotImplementedException();
        }

        public DataReaderLoadOperation LoadAsync(uint count)
        {
            throw new InvalidOperationException("Now allowed");
        }

        public IBuffer DetachBuffer()
        {
            return _reader.DetachBuffer();
        }

        public IInputStream DetachStream()
        {
            throw new InvalidOperationException("Now allowed");
        }

        public ByteOrder ByteOrder { get => _reader.ByteOrder; set => _reader.ByteOrder = value; }
        public InputStreamOptions InputStreamOptions { get => _reader.InputStreamOptions; set => throw new InvalidOperationException("Now allowed"); }

        public uint UnconsumedBufferLength => _reader.UnconsumedBufferLength;

        public Windows.Storage.Streams.UnicodeEncoding UnicodeEncoding { get => _reader.UnicodeEncoding; set => _reader.UnicodeEncoding = value; }
    }

    public class BoundedDataReaderQuotaExceedException : Exception
    {
        public uint RemainingQuota { get; }
        public uint NumberOfBytesAttemptToRead { get; }

        public BoundedDataReaderQuotaExceedException(uint remainingQuota, uint numberOfBytesAttemptToRead)
            : base($"Attempt to read {numberOfBytesAttemptToRead} bytes with BoundedDataReader, exceeding the remaining quota ${remainingQuota} bytes.")
        {
            RemainingQuota = remainingQuota;
            NumberOfBytesAttemptToRead = numberOfBytesAttemptToRead;
        }


    }
}
