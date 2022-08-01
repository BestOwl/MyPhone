using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.OBEX.UnitTest.Utilities
{
    public class TestByteArrayEqualityComparer
    {
        [Fact]
        public void TestEquals()
        {
            byte[] a = new byte[] { 1, 2, 3 };
            byte[] b = new byte[] { 1, 2, 3 };
            Assert.Equal(a, b);
        }

        [Fact]
        public void TestEquals_LargeArray()
        {
            byte[] a = new byte[65535];
            Random.Shared.NextBytes(a);
            byte[] b = new byte[65535];
            a.CopyTo(b, 0);
            Assert.Equal(a, b);
        }
    }
}
