using GoodTimeStudio.MyPhone.OBEX.Extensions;
using GoodTimeStudio.MyPhone.OBEX.Streams;
using GoodTimeStudio.MyPhone.OBEX.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage.Streams;

namespace GoodTimeStudio.MyPhone.OBEX.Headers
{
    public class ObexHeader
    {
        public HeaderId HeaderId { get; set; }

        public ushort BufferLength { get => (ushort)Buffer.Length; }

        public ushort HeaderTotalLength { get => (ushort)(BufferLength + sizeof(HeaderId) + sizeof(ushort)); }

        public byte[] Buffer { get; }

        public ObexHeaderEncoding Encoding { get => GetEncodingFromHeaderId(HeaderId); }

        public ObexHeader(HeaderId headerId, byte[] buffer)
        {
            if (buffer.Length + sizeof(HeaderId) + sizeof(ushort) > ushort.MaxValue)
            {
                throw new InvalidOperationException("Buffer size too large.");
            }
            HeaderId = headerId;
            Buffer = buffer;
        }

        public ObexHeader(HeaderId headerId, byte b) : this(headerId, new byte[] { b })
        {
        }

        public ObexHeader(HeaderId headerId, int i) : this(headerId, i.ToBigEndianBytes())
        { 
        }

        public ObexHeader(HeaderId headerId, string text, bool nullTerminated, Encoding stringEncoding)
            : this(headerId, text.ToBytes(stringEncoding, nullTerminated))
        {
        }

        public R GetValue<I, R>() where I : IBufferContentInterpreter<R>, new()
        {
            I interpreter = new I();
            return GetValue(interpreter);
        }

        public T GetValue<T>(IBufferContentInterpreter<T> interpreter)
        {
            return interpreter.GetValue(Buffer);
        }

        public string GetValueAsUtf8String(bool stringIsNullTerminated)
        {
            return GetValue(new StringInterpreter(System.Text.Encoding.UTF8, stringIsNullTerminated));
        }

        public string GetValueAsUnicodeString(bool stringIsNullTerminated)
        {
            return GetValue(new StringInterpreter(System.Text.Encoding.BigEndianUnicode, stringIsNullTerminated));
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
            writer.WriteBytes(Buffer);
        }

        public static ObexHeader ReadFromStream(IDataReader reader)
        {
            HeaderId headerId = (HeaderId)reader.ReadByte();
            ObexHeaderEncoding encoding = GetEncodingFromHeaderId(headerId);
            ushort len;
            switch (encoding)
            {
                case ObexHeaderEncoding.UnicodeString:
                case ObexHeaderEncoding.ByteSequence:
                    len = reader.ReadUInt16();
                    const ushort prefix = sizeof(HeaderId) + sizeof(ushort);
                    if (len < prefix)
                    {
                        throw new ObexException("Malformed header length.");
                    }
                    len -= prefix;
                    break;
                case ObexHeaderEncoding.OneByteQuantity:
                    len = 1;
                    break;
                case ObexHeaderEncoding.FourByteQuantity:
                    len = 4;
                    break;
                default:
                    throw new InvalidOperationException("Unreachable code reached!");
            }
            byte[] buffer = new byte[len];
            reader.ReadBytes(buffer);
            return new ObexHeader(headerId, buffer);
        }

        public static ObexHeaderEncoding GetEncodingFromHeaderId(HeaderId headerId)
        {
            return (ObexHeaderEncoding)((byte)headerId >> 6);
        }

        public override bool Equals(object? obj)
        {
            return obj is ObexHeader header &&
                   HeaderId == header.HeaderId &&
                   ByteArrayEqualityComparer.Default.Equals(Buffer, header.Buffer);
        }

        public override int GetHashCode()
        {
            int hashCode = -310111661;
            hashCode = hashCode * -1521134295 + HeaderId.GetHashCode();
            hashCode = hashCode * -1521134295 + ByteArrayEqualityComparer.Default.GetHashCode(Buffer);
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
