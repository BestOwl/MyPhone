using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Utilities
{
    public static class CollectionExtensions
    {
        public static string ContentToString<T>(this List<T> list)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[ ");
            foreach (T item in list)
            {
                sb.Append(item);
                sb.Append(", ");
            }
            sb.Append(']');
            return sb.ToString();
        }
    }
}
