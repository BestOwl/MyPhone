using GoodTimeStudio.MyPhone.OBEX.Streams;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage.Streams;

namespace GoodTimeStudio.MyPhone.OBEX.Headers
{
    public class AppParameterHeaderInterpreter : IObexHeaderInterpreter<AppParameterDictionary>
    {
        public AppParameterDictionary GetValue(HeaderId headerId, BoundedDataReader reader)
        {
            AppParameterDictionary dict = new();
            while (reader.RemainingQuota > 0)
            {
                byte tagId = reader.ReadByte();
                byte len = reader.ReadByte();
                IBuffer buf = reader.ReadBuffer(len);
                dict[tagId] = new AppParameter(tagId, buf);
            }
            return dict;
        }
    }
}
