namespace MyPhone.OBEX
{
    public enum HeaderId : byte
    {
        /// <summary>
        /// Number of objects (used by Connect) 
        /// </summary>
        Count = 0xC0,

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
        SessionSequenceNumber = 0x93,


        /// <summary>
        /// The Single Response Mode header is Mandatory in the first packet IF GOEP 2.0 or later is used; otherwise Excluded
        /// </summary>
        SingleResponseMode = 0x97,
    }

    public enum Opcode : byte
    {
        // MAS Op
        Connect = 0x80,
        Disconnect = 0x81,
        Put = 0x02,
        PutAlter = 0x82,
        Get = 0x03,
        GetAlter = 0x83,
        SetPath = 0x85,
        Session = 0x87,
        Abort = 0xFF,

        // MAS Response Code
        Success = 0xA0,
        SuccessAlt = 0x20,
        Continue = 0x90,
        ContinueAlt = 0x10,


        //OBEX Response Code
        //OBEX_OK = 0xA0,
        OBEX_CREATED = 0xA1,
        OBEX_ACCEPTED = 0xA2,
        OBEX_NOT_AUTHORITATIVE = 0xA3,
        OBEX_NO_CONTENT = 0xA4,
        OBEX_RESET = 0xA5,
        OBEX_PARTIAL = 0xA6,
        OBEX_MULT_CHOICE = 0xB0,
        OBEX_MOVED_PERM = 0xB1,
        OBEX_MOVED_TEMP = 0xB2,
        OBEX_SEE_OTHER = 0xB3,
        OBEX_NOT_MODIFIED = 0xB4,
        OBEX_USE_PROXY = 0xB5,
        OBEX_BAD_REQUEST = 0xC0,
        OBEX_UNAUTHORIZED = 0xC1,
        OBEX_PAYMENT_REQUIRED = 0xC2,
        OBEX_FORBIDDEN = 0xC3,
        OBEX_NOT_FOUND = 0xC4,
        OBEX_BAD_METHOD = 0xC5,
        OBEX_NOT_ACCEPTABLE = 0xC6,
        OBEX_PROXY_AUTH = 0xC7,
        OBEX_TIMEOUT = 0xC8,
        OBEX_CONFLICT = 0xC9,
        OBEX_GONE = 0xCA,
        OBEX_LENGTH_REQUIRED = 0xCB,
        OBEX_PRECON_FAILED = 0xCC,
        OBEX_ENTITY_TOO_LARGE = 0xCD,
        OBEX_REQ_TOO_LARGE = 0xCE,
        OBEX_UNSUPPORTED_TYPE = 0xCF,
        ERNAL_ERROR = 0xD0,
        OBEX_NOT_IMPLEMENTED = 0xD1,
        OBEX_BAD_GATEWAY = 0xD2,
        OBEX_UNAVAILABLE = 0xD3,
        OBEX_GATEWAY_TIMEOUT = 0xD4,
        OBEX_VERSION = 0xD5,
        OBEX_DATABASE_FULL = 0xE0,
        OBEX_DATABASE_LOCKED = 0xE1,

    }

    public static class MasConstants
    {
        public static readonly byte CHARSET_NATIVE = 0x00;
        public static readonly byte CHARSET_UTF8 = 0x01;

        public static readonly byte ATTACHMENT_OFF = 0x00;
        public static readonly byte ATTACHMENT_ON = 0x01;
    }

}
