using GoodTimeStudio.MyPhone.OBEX.Extensions;
using GoodTimeStudio.MyPhone.OBEX.Streams;
using GoodTimeStudio.MyPhone.OBEX.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;

namespace GoodTimeStudio.MyPhone.OBEX.Headers
{
    public class AppParameter
    {
        public byte TagId { get; set; }

        public byte ContentLength { get => (byte)Buffer.Length; }

        public byte TotalLength { get => (byte)(ContentLength + 2 * sizeof(byte)); }

        public IBuffer Buffer { get; }

        public AppParameter(byte tagId, IBuffer buffer)
        {
            if (buffer.Length + 2 * sizeof(byte) > byte.MaxValue)
            {
                throw new InvalidOperationException("Content buffer size too large. Max 126 bytes.");
            }

            TagId = tagId;
            Buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        }

        public AppParameter(byte tagId, byte[] buf) : this(tagId, buf.AsBuffer()) { }

        public AppParameter(byte tagId, byte b) : this(tagId, new byte[] { b }) { }

        public AppParameter(byte tagId, ushort value) : this(tagId, value.ToBuffer()) { }

        public R GetValue<I, R>() where I : IAppParameterInterpreter<R>, new()
        {
            I interpreter = new I();
            return GetValue(interpreter);
        }

        public T GetValue<T>(IAppParameterInterpreter<T> interpreter)
        {
            using (IRandomAccessStream stream = Buffer.AsStream().AsRandomAccessStream())
            using (DataReader actualReader = new DataReader(stream))
            {
                uint loaded = actualReader.LoadAsync(ContentLength).GetResults();
                Debug.Assert(loaded == ContentLength);
                BoundedDataReader reader = new BoundedDataReader(actualReader, ContentLength);
                return interpreter.GetValue(TagId, reader);
            }
        }

        public ushort GetValueAsUInt16()
        {
            return GetValue<UInt16ParamInterpreter, ushort>();
        }

        public static AppParameter FromDataReader(BoundedDataReader reader)
        {
            byte tagId = reader.ReadByte();
            byte contentLength = reader.ReadByte();
            IBuffer buf = reader.ReadBuffer(contentLength);
            return new AppParameter(tagId, buf);
        }

        public override bool Equals(object? obj)
        {
            return obj is AppParameter parameter &&
                   TagId == parameter.TagId &&
                   WindowsRuntimeBufferEqualityComparer.Default.Equals(Buffer, parameter.Buffer);
        }

        public override int GetHashCode()
        {
            int hashCode = -913184727;
            hashCode = hashCode * -1521134295 + TagId.GetHashCode();
            hashCode = hashCode * -1521134295 + WindowsRuntimeBufferEqualityComparer.Default.GetHashCode(Buffer);
            return hashCode;
        }
    }
}
