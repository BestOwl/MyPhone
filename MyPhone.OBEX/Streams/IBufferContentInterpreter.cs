using System;
using System.Collections.Generic;
using System.Text;

namespace GoodTimeStudio.MyPhone.OBEX.Streams
{
    public interface IBufferContentInterpreter<T>
    {
        /// <summary>
        /// Get the value from bytes buffer
        /// </summary>
        /// <returns>the value</returns>
        /// <exception cref="ObexException">Throws when the value cannot be parsed from byte stream (e.g. illegal format).</exception>
        T GetValue(ReadOnlySpan<byte> buffer);
    }
}
