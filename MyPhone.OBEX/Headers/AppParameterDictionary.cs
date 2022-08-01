using System;
using System.Collections.Generic;
using System.Text;

namespace GoodTimeStudio.MyPhone.OBEX.Headers
{
    public class AppParameterDictionary : Dictionary<byte, AppParameter>
    {
        public new AppParameter this[byte key]
        {
            get
            {
                if (TryGetValue(key, out var value))
                {
                    return value;
                }
                else
                {
                    throw new ObexAppParameterNotFoundException(key);
                }
            }
            set
            {
                base[key] = value;
            }
        }
    }
}
