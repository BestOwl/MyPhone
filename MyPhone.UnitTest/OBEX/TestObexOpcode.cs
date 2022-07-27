using MyPhone.OBEX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MyPhone.UnitTest.OBEX
{
    public class TestObexOpcode
    {
        [Theory]
        [InlineData(0x80, ObexOperation.Connect, true)]

        [InlineData(0x20, ObexOperation.Success, false)]
        [InlineData(0xA0, ObexOperation.Success, true)]

        [InlineData(0x21, ObexOperation.Created, false)]
        [InlineData(0xA1, ObexOperation.Created, true)]

        [InlineData(0x4F, ObexOperation.UnsupportedMediaType, false)]
        [InlineData(0xCF, ObexOperation.UnsupportedMediaType, true)]


        [InlineData(0x10, ObexOperation.Continue, false)]
        [InlineData(0x90, ObexOperation.Continue, true)]
        public void TestConstructor_RawOpcode(byte rawOpcode, ObexOperation operation, bool finalBit)
        {
            ObexOpcode opcode = new ObexOpcode(rawOpcode);
            Assert.Equal(operation, opcode.ObexOperation);
            Assert.Equal(finalBit, opcode.IsFinalBitSet);
        }

        [Theory]
        [InlineData(0x00)]
        [InlineData(0x01)]
        [InlineData(0x05)]
        [InlineData(0x11)] // Up to user define, not define yet
        [InlineData(0x1F)] // Up to user define, not define yet

        public void TestConstructor_RawOpcode_Illegal(byte rawOpcode)
        {
            Assert.Throws<InvalidObexOpcodeException>(() => new ObexOpcode(rawOpcode));
        }

        [Theory]
        [InlineData(ObexOperation.Get, false, 0x03)]
        [InlineData(ObexOperation.Get, true, 0x83)]
        public void TestConstructor_ObexOpcodeValue(ObexOperation opcodeValue, bool setFinalBit, byte expectedValue)
        {
            ObexOpcode opcode = new(opcodeValue, setFinalBit);
            Assert.Equal(expectedValue, opcode.Value);
            Assert.Equal(setFinalBit, opcode.IsFinalBitSet);
        }

        [Theory]
        [InlineData(ObexOperation.Connect)]
        [InlineData(ObexOperation.Disconnect)]
        [InlineData(ObexOperation.SetPath)]
        [InlineData(ObexOperation.Session)]
        [InlineData(ObexOperation.Abort)]
        public void TestConstructor_ObexOpcodeValue_HighBitMustBeSet(ObexOperation opcodeValue)
        {
            Assert.Throws<ArgumentException>(() => new ObexOpcode(opcodeValue, false));
        }
    }
}
