using System;
using System.Collections.Generic;
using System.Text;

namespace GoodTimeStudio.MyPhone.OBEX.Headers
{
    /// <summary>
    /// Throws when the desired <see cref="ObexHeader"/> can not be found in <see cref="ObexPacket"/>.
    /// </summary>
    public class ObexHeaderNotFoundException : ObexException
    {
        public HeaderId HeaderId { get; }

        public ObexHeaderNotFoundException(HeaderId headerId) : base($"Can not find such header, HeaderId: {headerId} ")
        {
            HeaderId = headerId;
        }
    }

    /// <summary>
    /// Throws when the desired <see cref="AppParameter"/> can not be found in <see cref="AppParameterDictionary"/>.
    /// </summary>
    public class ObexAppParameterNotFoundException : ObexException
    {
        public byte TagId { get; }
        public ObexAppParameterNotFoundException(byte tagId) : base($"Can not find such app parameter, TagId: {tagId}")
        {
            TagId = tagId;
        }
    }
}
