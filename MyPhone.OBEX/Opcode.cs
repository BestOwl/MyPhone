using System;
using System.Collections.Generic;
using System.Text;

namespace MyPhone.OBEX
{
    public enum Opcode : byte
    {
        Connect = 0x80,
        Disconnect = 0x81,
        Put = 0x02,
        PutAlter = 0x82,
        Get = 0x03,
        GetAlter = 0x83,
        SetPath = 0x85,
        Session = 0x87,
        Abort = 0xFF
    }
}
