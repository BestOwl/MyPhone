using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

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
