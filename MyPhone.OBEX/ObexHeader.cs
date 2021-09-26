using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Windows.Storage.Streams;

namespace MyPhone.OBEX
{
    public interface IObexHeader
    {
        HeaderId HeaderId { get; }

        /// <summary>
        /// Convert content into byte array
        /// </summary>
        /// <returns>Byte array of the content. If this header does not contains any content, return null</returns>
        byte[]? ToBytes();

        void FromBytes(byte[] bytes);

        /// <summary>
        /// Get the length if this header is a fixed length header, otherwise return 0
        /// </summary>
        /// <returns></returns>
        ushort GetFixedLength();

    }

    public abstract class ObexHeader<T> : IObexHeader
    {
        public HeaderId HeaderId { get; protected set; }

        protected ObexHeader(HeaderId headerId) 
        {
            HeaderId = headerId;
        }

        protected ObexHeader(HeaderId headerId, T value) : this(headerId)
        {
            Value = value;
        }

        public T? Value { get; set; }

        public abstract byte[]? ToBytes();

        public abstract void FromBytes(byte[] bytes);

        public virtual ushort GetFixedLength()
        {
            return 0;
        }

    }

    public class Int32ValueHeader : ObexHeader<int>
    {
        public Int32ValueHeader(HeaderId headerId) : base(headerId) { }

        public Int32ValueHeader(HeaderId headerId, int value) : base(headerId, value) { }

        public override void FromBytes(byte[] bytes)
        {
            if (HeaderId.Equals(HeaderId.ConnectionId))
            {
                Value = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, 0));
            }
            else
            {
                Value = BitConverter.ToInt32(bytes, 0);
            }
            
        }

        public override byte[]? ToBytes()
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

    public class AsciiStringValueHeader : ObexHeader<string>
    {
        public AsciiStringValueHeader(HeaderId headerId) : base(headerId) { }

        public AsciiStringValueHeader(HeaderId headerId, string value) : base(headerId, value) { }

        public override void FromBytes(byte[] bytes)
        {
            Value = Encoding.ASCII.GetString(bytes, 0, bytes.Length - 1); //Remove \0 null terminator
        }

        public override byte[]? ToBytes()
        {
            if (Value != null)
            {
                byte[] ret = new byte[Encoding.ASCII.GetByteCount(Value) + 1]; // plus \0 null terminator
                Encoding.ASCII.GetBytes(Value, 0, Value.Length, ret, 0);
                ret[ret.Length - 1] = 0; // null terminator
                return ret;
            }
            return null;
        }
    }

    public class BodyHeader : ObexHeader<string>
    {
        public BodyHeader(HeaderId headerId) : base(headerId) { }

        public BodyHeader(HeaderId headerId, string value) : base(headerId, value) { }

        public override void FromBytes(byte[] bytes)
        {
            Value = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        public override byte[]? ToBytes()
        {
            if (Value != null)
            {
                byte[] ret = new byte[Encoding.UTF8.GetByteCount(Value)]; // plus \0 null terminator
                Encoding.UTF8.GetBytes(Value, 0, Value.Length, ret, 0);
                return ret;
            }
            return null;
        }
    }

    public class Utf8StringValueHeader : ObexHeader<string>
    {
        public Utf8StringValueHeader(HeaderId headerId) : base(headerId) { }

        public Utf8StringValueHeader(HeaderId headerId, string value) : base(headerId, value) { }


        public override void FromBytes(byte[] bytes)
        {
            Value = Encoding.UTF8.GetString(bytes, 0, bytes.Length - 1); //Remove \0 null terminator
        }

        public override byte[]? ToBytes()
        {
            if (Value != null)
            {
                byte[] ret = new byte[Encoding.Unicode.GetByteCount(Value) + 1]; // plus \0 null terminator
                Encoding.UTF8.GetBytes(Value, 0, Value.Length, ret, 0);
                ret[ret.Length - 1] = 0; // null terminator
                return ret;
            }
            return null;
        }
    }

    public class UnicodeStringValueHeader : ObexHeader<string>
    {
        public UnicodeStringValueHeader(HeaderId headerId) : base(headerId) { }

        public UnicodeStringValueHeader(HeaderId headerId, string value) : base(headerId, value) { }


        public override void FromBytes(byte[] bytes)
        {
            Value = Encoding.BigEndianUnicode.GetString(bytes, 0, bytes.Length - 2); //Remove \0 null terminator
        }

        public override byte[]? ToBytes()
        {
            if (Value != null)
            {
                byte[] ret = new byte[Encoding.Unicode.GetByteCount(Value) + 2]; // plus \0 null terminator
                Encoding.BigEndianUnicode.GetBytes(Value, 0, Value.Length, ret, 0);
                ret[ret.Length - 1] = 0; // null terminator
                ret[ret.Length - 2] = 0; // null terminator
                return ret;
            }
            return null;
        }
    }

    public class BytesHeader : ObexHeader<byte[]>
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

        public override byte[]? ToBytes()
        {
            return Value;
        }

    }
}
