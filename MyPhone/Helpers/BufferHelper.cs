using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;

namespace GoodTimeStudio.MyPhone.Helpers
{
    public class BufferHelper
    {
        /// <summary>
        /// Convert a <see cref="IBuffer"/> to hex string.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string BufferToHexString(IBuffer buffer)
        {
            return Convert.ToHexString(buffer.ToArray());
        }
    }
}
