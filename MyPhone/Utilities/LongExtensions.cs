using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Utilities
{
    public static class LongExtensions
    {
        public static string ToHexString(this long l) 
        {
            return Convert.ToHexString(BitConverter.GetBytes(l));
        }

        public static string ToHexString(this ulong l)
        {
            return Convert.ToHexString(BitConverter.GetBytes(l));
        }
    }
}
