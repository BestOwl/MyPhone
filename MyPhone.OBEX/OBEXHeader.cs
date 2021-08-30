using System;
using System.Collections.Generic;
using System.Net;
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

        void FromBytes(byte[] bytes);

        /// <summary>
        /// Get the length if this header is a fixed length header, otherwise return 0
        /// </summary>
        /// <returns></returns>
        ushort GetFixedLength();

    }

    public abstract class OBEXHeader<T> : IOBEXHeader
    {
        public HeaderId HeaderId { get; protected set; }

        protected OBEXHeader(HeaderId headerId) 
        {
            HeaderId = headerId;
        }

        protected OBEXHeader(HeaderId headerId, T value) : this(headerId)
        {
            Value = value;
        }

        public T Value { get; set; }

        public abstract byte[] ToBytes();

        public abstract void FromBytes(byte[] bytes);

        public virtual ushort GetFixedLength()
        {
            return 0;
        }

    }

    public class Int32ValueHeader : OBEXHeader<int>
    {
        public Int32ValueHeader(HeaderId headerId) : base(headerId) { }

        public Int32ValueHeader(HeaderId headerId, int value) : base(headerId, value) { }

        public override void FromBytes(byte[] bytes)
        {
            if (HeaderId.Equals(HeaderId.ConnectionId))
            {
                Value = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes));
            }
            else
            {
                Value = BitConverter.ToInt32(bytes);
            }
            
        }

        public override byte[] ToBytes()
        {
            byte[] ret;

            if (HeaderId.Equals(HeaderId.ConnectionId))
            {
                ret = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Value));
            }
            else
            {
                ret = BitConverter.GetBytes(Value);
            }
            return ret;
        }

        public override ushort GetFixedLength()
        {
            return sizeof(int);
        }
    }

    public class StringValueHeader : OBEXHeader<string>
    {
        public StringValueHeader(HeaderId headerId) : base(headerId) { }

        public StringValueHeader(HeaderId headerId, string value) : base(headerId, value) { }

        public override void FromBytes(byte[] bytes)
        {
            Value = Encoding.ASCII.GetString(bytes, 0, bytes.Length - 1); //Remove \0 null terminator
        }

        public override byte[] ToBytes()
        {
            byte[] ret = new byte[Encoding.ASCII.GetByteCount(Value) + 1]; // plus \0 null terminator
            Encoding.ASCII.GetBytes(Value, ret);
            ret[ret.Length - 1] = 0; // null terminator
            return ret;
        }
    }

    public class BytesHeader : OBEXHeader<byte[]>
    {
        public BytesHeader(HeaderId headerId) : base(headerId) { }

        public BytesHeader(HeaderId headerId, byte[] value) : base(headerId, value)
        {
        }

        public BytesHeader(HeaderId headerId, byte value) : this(headerId, new byte[] { value }) { }

        public override void FromBytes(byte[] bytes)
        {
            Value = bytes;
        }

        public override byte[] ToBytes()
        {
            return Value;
        }

    }
}
