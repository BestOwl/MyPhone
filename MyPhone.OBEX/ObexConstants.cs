namespace GoodTimeStudio.MyPhone.OBEX
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

    public enum ObexOperation : byte
    {
        // OBEX Request Code

        // High bit always set
        Connect = 0x80,
        Disconnect = 0x81,
        SetPath = 0x85,
        Session = 0x87,
        Abort = 0xFF,

        Put = 0x02,
        Get = 0x03,
        Action = 0x06,
        

        //OBEX Response Code
        Continue = 0x10,

        Success = 0x20,
        Created = 0x21,
        Accepted = 0x22,
        NotAuthoritative = 0x23,
        NoContent = 0x24,
        ResetContent = 0x25,
        PartialContent = 0x26,

        MultipleChoices = 0x30,
        MovedPermanently = 0x31,
        MovedTemporarily = 0x32,
        SeeOther = 0x33,
        NotModified = 0x34,
        UseProxy = 0x35,

        BadRequest = 0x40,
        Unauthorized = 0x41,
        PaymentRequired = 0x42,
        Forbidden = 0x43,
        NotFound = 0x44,
        MethodNotAllowed = 0x45,
        NotAcceptable = 0x46,
        ProxyAuthenticationRequired = 0x47,
        RequestTimeout = 0x48,
        Conflict = 0x49,
        Gone = 0x4A,
        LengthRequired = 0x4B,
        PreconditionFailed = 0x4C,
        RequestedEntityTooLarge = 0x4D,
        RequestUrlTooLarge = 0x4E,
        UnsupportedMediaType = 0x4F,

        InternalServerError = 0x50,
        NotImplemented = 0x51,
        BadGateway = 0x52,
        ServiceUnavailable = 0x53,
        GatewayTimeout = 0x54,
        VersionNotSupported = 0x55,
        
        DatabaseFull = 0x60,
        DatabaseLocked = 0x61,
    }

    public static class MasConstants
    {
        public static readonly byte CHARSET_NATIVE = 0x00;
        public static readonly byte CHARSET_UTF8 = 0x01;

        public static readonly byte ATTACHMENT_OFF = 0x00;
        public static readonly byte ATTACHMENT_ON = 0x01;
    }

}
