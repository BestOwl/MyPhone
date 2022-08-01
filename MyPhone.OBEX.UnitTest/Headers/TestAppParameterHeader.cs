using GoodTimeStudio.MyPhone.OBEX.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.OBEX.UnitTest.Headers
{
    public class TestAppParameterHeader
    {
        [Fact]
        public void TestBuilderAndInterpreter()
        {
            AppParameter originalAppParameter = new AppParameter(0x01, new byte[] { 0x01, 0x02, 0x03, 0x04 }.AsBuffer());
            AppParameterHeaderBuilder builder = new(originalAppParameter);
            ObexHeader header = builder.Build();
            
            AppParameterDictionary dict = header.GetValueAsAppParameters();
            AppParameter appParameter = dict[0x01];
            Assert.Equal(originalAppParameter, appParameter);
        }
    }
}
