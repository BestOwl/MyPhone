using GoodTimeStudio.MyPhone.OBEX.Streams;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoodTimeStudio.MyPhone.OBEX.Headers
{
    public interface IAppParameterInterpreter<T>
    {
        /// <summary>
        /// Get the value from byte stream 
        /// </summary>
        /// <param name="headerId"></param>
        /// <param name="reader">
        /// A data reader with a limit on the number of bytes that can be read.
        /// 
        /// <see cref="BoundedDataReader.RemainingQuota"/> Indicates the length of the header content  </param>
        /// <returns>the value</returns>
        /// <exception cref="ObexException">Throws when the value cannot be parsed from byte stream (e.g. illegal format).</exception>
        T GetValue(byte tagId, BoundedDataReader reader);
    }

    public class UInt16ParamInterpreter : IAppParameterInterpreter<ushort>
    {
        public ushort GetValue(byte tagId, BoundedDataReader reader)
        {
            if (reader.RemainingQuota != 2)
            {
                throw new ObexException("Invalid ushort buffer, len != 2");
            }
            return reader.ReadUInt16();
        }
    }
}
