using GoodTimeStudio.MyPhone.OBEX.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace GoodTimeStudio.MyPhone.OBEX.UnitTest.Headers
{
    public class TestObexHeader
    {
        [Theory]
        [InlineData(HeaderId.ConnectionId, ObexHeaderEncoding.FourByteQuantity)]
        [InlineData(HeaderId.Name, ObexHeaderEncoding.UnicodeString)]
        [InlineData(HeaderId.Body, ObexHeaderEncoding.ByteSequence)]
        [InlineData(HeaderId.SingleResponseMode, ObexHeaderEncoding.OneByteQuantity)]
        public void TestHeaderEncoding(HeaderId headerId, ObexHeaderEncoding expectedEncoding)
        {
            ObexHeader header = new ObexHeader(headerId, 1);
            Assert.Equal(expectedEncoding, header.Encoding);
        }

        [Fact]
        public void TestBuiltinHeaderInterpreter_UnicodeString()
        {
            const string originalString = "Foobar";
            ObexHeader header = new ObexHeader(HeaderId.Name, originalString, true);
            Assert.Equal(originalString, header.GetValueAsUnicodeString(true));
        }

        [Fact]
        public void TestBuiltinHeaderInterpreter_Utf8String()
        {
            const string originalString = "Foobar";
            ObexHeader header = new ObexHeader(HeaderId.Name, originalString, false, Windows.Storage.Streams.UnicodeEncoding.Utf8);
            Assert.Equal(originalString, header.GetValueAsUtf8String(false));
        }

        [Fact]
        public void TestEquals()
        {
            ObexHeader headerA = new ObexHeader(HeaderId.Body, 32);
            ObexHeader headerB = new ObexHeader(HeaderId.Body, 32);
            Assert.Equal(headerA, headerB);

            DataWriter writer = new();
            writer.WriteByte(1);
            writer.WriteByte(2);
            headerA = new ObexHeader(HeaderId.Body, writer.DetachBuffer());
            writer.Dispose();
            headerB = new ObexHeader(HeaderId.Body, new byte[] { 1, 2 }.AsBuffer());
            Assert.Equal(headerA, headerB);
        }
    }
}
