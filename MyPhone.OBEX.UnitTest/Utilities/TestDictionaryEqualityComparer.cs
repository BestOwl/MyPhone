using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.OBEX.UnitTest.Utilities
{
    public class TestDictionaryEqualityComparer
    {
        [Fact]
        public void TestEquals_Empty()
        {
            var d1 = new Dictionary<string, object>();
            var d2 = new Dictionary<string, object>();

            Assert.Equal(d1, d2);
        }

        [Fact]
        public void TestEquals()
        {
            var d1 = new Dictionary<string, int>();
            var d2 = new Dictionary<string, int>();

            d1["a"] = 1;
            d2["a"] = 1;
            Assert.Equal(d1, d2);
        }

        [Fact]
        public void TestNotEquals()
        {
            var d1 = new Dictionary<string, int>();
            var d2 = new Dictionary<string, int>();

            d1["a"] = 1;
            Assert.NotEqual(d1, d2);

            d2["a"] = 1;
            Assert.Equal(d1, d2);
        }
    }
}
