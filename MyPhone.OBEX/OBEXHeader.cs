using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage.Streams;

namespace MyPhone.OBEX
{
    public interface IOBEXHeader
    {
        HeaderId HeaderId { get; }

        /// <summary>
        /// Convert content into byte array
        /// </summary>
        /// <returns></returns>
        byte[] ToBytes();
    }

    public interface ILengthRequiredHeader
    {
        ushort GetValueLength();

        ushort GetHeaderLength()
        {
            return (ushort)(GetValueLength() + sizeof(HeaderId) + sizeof(ushort));
        }
    }

    public abstract class OBEXHeader : IOBEXHeader
    {
        public HeaderId HeaderId { get; protected set; }

        public OBEXHeader(HeaderId headerId)
        {
            HeaderId = headerId;
        }

        public abstract byte[] ToBytes();
    }

    public abstract class OBEXHeader<T> : OBEXHeader
    {
        protected OBEXHeader(HeaderId headerId, T value) : base(headerId)
        {
            Value = value;
        }

        public T Value { get; set; }

        public abstract T FromBytes(byte[] bytes);
    }

    public class Int32ValueHeader : OBEXHeader<int>
    {
        public Int32ValueHeader(HeaderId headerId, int value) : base(headerId, value)
        {
        }

        public override int FromBytes(byte[] bytes)
        {
            return BitConverter.ToInt32(bytes);
        }

        public override byte[] ToBytes()
        {
            byte[] ret = BitConverter.GetBytes(Value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(ret);
            }
            return ret;
        }
    }

    public class StringValueHeader : OBEXHeader<string>, ILengthRequiredHeader
    {
        public StringValueHeader(HeaderId headerId, string value) : base(headerId, value)
        {
        }

        public override string FromBytes(byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes, 0, bytes.Length - 1); //Remove \0 null terminator
        }

        public ushort GetValueLength()
        {
            return (ushort)(Value.Length + 1);
        }

        public override byte[] ToBytes()
        {
            byte[] ret = new byte[Encoding.ASCII.GetByteCount(Value) + 1]; // plus \0 null terminator
            Encoding.ASCII.GetBytes(Value, ret);
            ret[ret.Length - 1] = 0; // null terminator
            return ret;
        }
    }

    public class BytesHeader : OBEXHeader<byte[]>, ILengthRequiredHeader
    {
        public BytesHeader(HeaderId headerId, byte[] value) : base(headerId, value)
        {
        }

        public BytesHeader(HeaderId headerId, byte value) : this(headerId, new byte[] { value }) { }

        public override byte[] FromBytes(byte[] bytes)
        {
            return bytes;
        }

        public override byte[] ToBytes()
        {
            return Value;
        }

        public ushort GetValueLength()
        {
            return (ushort)Value.Length;
        }
    }
}
