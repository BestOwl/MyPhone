using System;
using System.Runtime.Serialization;

namespace MyPhone.OBEX
{
    /// <summary>
    /// Represent errors that occurred during the processing of OBEX packets.
    /// </summary>
    public class ObexException : Exception
    {
        public ObexException()
        {
        }

        public ObexException(string message) : base(message)
        {
        }

        public ObexException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// The exception that is thrown when a OBEX request is unsuccessful (i.e. the remote service return an unsuccessful opcode)
    /// </summary>
    public class ObexRequestException : ObexException
    {
        /// <summary>
        /// Represents a non-successful response opcode
        /// </summary>
        public Opcode Opcode { get; }

        public ObexRequestException(Opcode opcode)
        {
            Opcode = opcode;
        }

        public ObexRequestException(Opcode opcode, string message) : base(message)
        {
            Opcode = opcode;
        }

        public ObexRequestException(Opcode opcode, string message, Exception innerException) : base(message, innerException)
        {
            Opcode = opcode;
        }
    }
}
