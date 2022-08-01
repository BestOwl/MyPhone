using GoodTimeStudio.MyPhone.OBEX.Streams;

namespace GoodTimeStudio.MyPhone.OBEX.Headers
{
    public interface IObexHeaderInterpreter<T>
    {
        /// <summary>
        /// Get the value from byte stream 
        /// </summary>
        /// <param name="headerId"></param>
        /// <param name="reader">
        /// A data reader with a limit on the number of bytes that can be read.
        /// 
        /// <see cref="BoundedDataReader.RemainingQuota"/> Indicates the length of the header content  </param>
        /// <returns>the value</returns>
        /// <exception cref="ObexException">Throws when the value cannot be parsed from byte stream (e.g. illegal format).</exception>
        T GetValue(HeaderId headerId, BoundedDataReader reader);
    }

    public class StringHeaderInterpreter : IObexHeaderInterpreter<string>
    {
        public Windows.Storage.Streams.UnicodeEncoding StringEncoding { get; }
        public bool StringIsNullTerminated { get; }

        public StringHeaderInterpreter(Windows.Storage.Streams.UnicodeEncoding stringEncoding, bool stringIsNullTerminated)
        {
            StringEncoding = stringEncoding;
            StringIsNullTerminated = stringIsNullTerminated;
        }

        public string GetValue(HeaderId headerId, BoundedDataReader reader)
        {
            reader.UnicodeEncoding = StringEncoding;
            uint len = reader.RemainingQuota;
            if (StringEncoding == Windows.Storage.Streams.UnicodeEncoding.Utf8)
            {
                if (StringIsNullTerminated)
                {
                    if (len < 1)
                    {
                        throw new ObexException("Header buffer content is illegal: buffer length is 0");
                    }
                    len -= 1;
                }
            }
            else
            {
                if (StringIsNullTerminated)
                {
                    if (len < 2)
                    {
                        throw new ObexException("Header buffer content is illegal: buffer length is less than 2");
                    }
                    len -= 2;
                }
                len /= 2;
            }
            return reader.ReadString(len);
        }
    }
}
