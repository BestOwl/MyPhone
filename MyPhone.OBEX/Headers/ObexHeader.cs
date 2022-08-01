using GoodTimeStudio.MyPhone.OBEX.Extensions;
using GoodTimeStudio.MyPhone.OBEX.Streams;
using GoodTimeStudio.MyPhone.OBEX.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;

namespace GoodTimeStudio.MyPhone.OBEX.Headers
{
    public class ObexHeader
    {
        public HeaderId HeaderId { get; set; }

        public ushort BufferLength { get => (ushort)Buffer.Length; }

        public ushort HeaderTotalLength { get => (ushort)(BufferLength + sizeof(HeaderId) + sizeof(ushort)); }

        public IBuffer Buffer { get; }

        public ObexHeaderEncoding Encoding { get => GetEncodingFromHeaderId(HeaderId); }

        public ObexHeader(HeaderId headerId, IBuffer buffer)
        {
            if (buffer.Length + sizeof(HeaderId) + sizeof(ushort) > ushort.MaxValue)
            {
                throw new InvalidOperationException("Buffer size too large.");
            }
            HeaderId = headerId;
            Buffer = buffer;
        }

        public ObexHeader(HeaderId headerId, byte b) : this(headerId, new byte[] { b }.AsBuffer())
        {
        }

        public ObexHeader(HeaderId headerId, int i) : this(headerId, i.ToBuffer())
        { 
        }

        public ObexHeader(HeaderId headerId, string text, bool nullTerminated,
            UnicodeEncoding stringEncoding = UnicodeEncoding.Utf16BE)
            : this(headerId, text.ToBuffer(stringEncoding, nullTerminated))
        {
        }

        public R GetValue<I, R>() where I : IObexHeaderInterpreter<R>, new()
        {
            I interpreter = new I();
            return GetValue(interpreter);
        }

        public T GetValue<T>(IObexHeaderInterpreter<T> interpreter)
        {
            using (IRandomAccessStream stream = Buffer.AsStream().AsRandomAccessStream())
            using (DataReader actualReader = new DataReader(stream))
            {
                uint loaded = actualReader.LoadAsync(BufferLength).GetResults();
                Debug.Assert(loaded == BufferLength);
                BoundedDataReader reader = new BoundedDataReader(actualReader, BufferLength);
                return interpreter.GetValue(HeaderId, reader);
            }
        }

        public string GetValueAsUtf8String(bool stringIsNullTerminated)
        {
            return GetValue(new StringHeaderInterpreter(Windows.Storage.Streams.UnicodeEncoding.Utf8, stringIsNullTerminated));
        }

        public string GetValueAsUnicodeString(bool stringIsNullTerminated)
        {
            return GetValue(new StringHeaderInterpreter(Windows.Storage.Streams.UnicodeEncoding.Utf16BE, stringIsNullTerminated));
        }

        public AppParameterDictionary GetValueAsAppParameters()
        {
            return GetValue<AppParameterHeaderInterpreter, AppParameterDictionary>();
        }

        public void WriteToStream(IDataWriter writer)
        {
            writer.WriteByte((byte)HeaderId);
            switch (Encoding)
            {
                case ObexHeaderEncoding.UnicodeString:
                case ObexHeaderEncoding.ByteSequence:
                    writer.WriteUInt16(HeaderTotalLength);
                    break;
            }
            writer.WriteBuffer(Buffer);
        }

        public static ObexHeader ReadFromStream(IDataReader reader)
        {
            HeaderId headerId = (HeaderId)reader.ReadByte();
            ObexHeaderEncoding encoding = GetEncodingFromHeaderId(headerId);
            switch (encoding)
            {
                case ObexHeaderEncoding.UnicodeString:
                case ObexHeaderEncoding.ByteSequence:
                    ushort len = reader.ReadUInt16();
                    const ushort prefix = sizeof(HeaderId) + sizeof(ushort);
                    if (len < prefix)
                    {
                        throw new ObexException("Malformed header length.");
                    }
                    len -= prefix;
                    return new ObexHeader(headerId, reader.ReadBuffer(len));
                case ObexHeaderEncoding.OneByteQuantity:
                    return new ObexHeader(headerId, reader.ReadBuffer(1));
                case ObexHeaderEncoding.FourByteQuantity:
                    return new ObexHeader(headerId, reader.ReadBuffer(4));
                default:
                    throw new InvalidOperationException("Unreachable code reached!");
            }
        }

        public static ObexHeaderEncoding GetEncodingFromHeaderId(HeaderId headerId)
        {
            return (ObexHeaderEncoding)((byte)headerId >> 6);
        }

        public override bool Equals(object? obj)
        {
            return obj is ObexHeader header &&
                   HeaderId == header.HeaderId &&
                   WindowsRuntimeBufferEqualityComparer.Default.Equals(Buffer, header.Buffer);
        }

        public override int GetHashCode()
        {
            int hashCode = -310111661;
            hashCode = hashCode * -1521134295 + HeaderId.GetHashCode();
            hashCode = hashCode * -1521134295 + WindowsRuntimeBufferEqualityComparer.Default.GetHashCode(Buffer);
            return hashCode;
        }
    }

    public enum ObexHeaderEncoding : byte
    {
        UnicodeString = 0,
        ByteSequence,
        OneByteQuantity,
        FourByteQuantity
    }

}
