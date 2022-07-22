using GoodTimeStudio.MyPhone.Helpers;
using Windows.ApplicationModel.Calls;
using Windows.Devices.Enumeration;

namespace GoodTimeStudio.MyPhone.DeviceTest
{
    public class TestPhoneLineTransportHelper : IAssemblyFixture<BluetoothDeviceFixture>
    {
        private readonly BluetoothDeviceFixture _bthFixture;

        public TestPhoneLineTransportHelper(BluetoothDeviceFixture fixture)
        {
            _bthFixture = fixture;
        }

        [Fact]
        public async Task TestGetPhoneLineTransportFromBluetoothDevice()
        {
            PhoneLineTransportDevice? phoneLine = await PhoneLineTransportHelper.GetPhoneLineTransportFromBluetoothDevice(_bthFixture.BluetoothDevice);
            Assert.NotNull(phoneLine);

            DeviceInformation phoneLineDevInfo = await DeviceInformation.CreateFromIdAsync(phoneLine!.DeviceId);
            Assert.Equal(_bthFixture.BluetoothDevice.Name, phoneLineDevInfo.Name);
            Console.WriteLine($"Corresponding PhoneLineTransportDevice: {phoneLineDevInfo.Id}");
        }


    }
}
