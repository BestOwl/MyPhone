using System;
using System.Collections.Generic;
using System.Text;

namespace MyPhone.OBEX
{
    public enum HeaderId : byte
    {
        /// <summary>
        /// Number of objects (used by Connect) 
        /// </summary>
        Count = 0xC,

        /// <summary>
        /// Name of the object (often a file name) 
        /// </summary>
        Name = 0x01,

        /// <summary>
        /// Type of object - e.g. text, html, binary, manufacturer specific
        /// </summary>
        Type = 0x42,

        /// <summary>
        /// Length of the object in bytes
        /// </summary>
        Length = 0xC3,

        /// <summary>
        /// Date/time stamp – ISO 8601 version - preferred
        /// </summary>
        Time = 0x44,

        /// <summary>
        /// Date/time stamp – 4 byte version (for compatibility only)
        /// </summary>
        Time_Compatiable = 0xC4,

        /// <summary>
        /// Text description of the object 
        /// </summary>
        Description = 0x05,

        /// <summary>
        /// Name of service that operation is targeted to
        /// </summary>
        Target = 0x46,

        /// <summary>
        /// An HTTP 1.x header 
        /// </summary>
        HTTP = 0x47,

        /// <summary>
        /// A chunk of the object body
        /// </summary>
        Body = 0x48,

        /// <summary>
        /// The final chunk of the object body
        /// </summary>
        EndOfBody = 0x49,

        /// <summary>
        /// Identifies the OBEX application, used to tell if talking to a peer
        /// </summary>
        Who = 0x4A,

        /// <summary>
        /// An identifier used for OBEX connection multiplexing 
        /// </summary>
        ConnectionId = 0xCB,

        /// <summary>
        /// Extended application request & response information
        /// </summary>
        ApplicationParameters = 0x4C,

        /// <summary>
        /// Authentication digest-challenge
        /// </summary>
        AuthChallenge = 0x4D,

        /// <summary>
        /// Authentication digest-response
        /// </summary>
        AuthResponse = 0x4E,

        /// <summary>
        /// Indicates the creator of an object 
        /// </summary>
        CreatorId = 0xCF,

        /// <summary>
        /// Uniquely identifies the network client (OBEX server)
        /// </summary>
        WAN_UUID = 0x50,

        /// <summary>
        /// OBEX Object class of object 
        /// </summary>
        ObjectClass = 0x51,

        /// <summary>
        /// Parameters used in session commands/responses
        /// </summary>
        SessionParameters = 0x52,

        /// <summary>
        /// Sequence number used in each OBEX packet for reliability
        /// </summary>
        SessionSequenceNumber = 0x93
    }
}
