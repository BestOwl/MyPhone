using GoodTimeStudio.MyPhone.Helpers;
using log4net;
using MyPhone.IntegrationTest.Attributes;
using Windows.ApplicationModel.Calls;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Xunit;

namespace MyPhone.IntegrationTest
{
    [Test]
    public class TestPhoneLineTransportHelper
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(TestPhoneLineTransportHelper).Name);

        [TestMethod]
        public async Task TestGetPhoneLineTransportFromBluetoothDevice()
        {
            logger.Info("Prerequisite: ");
            logger.Info("1. Your PC must have Bluetooth enabled. ");
            logger.Info("2. Your must have a bluetooth device (typically a mobile phone) that support Hands-Free Profile. ");
            logger.Info("3. Your bluetooth device is enabled. ");
            logger.Info("4. Your bluetooth device must have paired to this PC. ");
            Console.WriteLine();
            logger.Info("If all prerequisite are met, press enter to continue. Otherwise, type q to exit");

            string? input = Console.ReadLine();
            if (input == "q" || input == "Q")
            {
                throw new EnvironmentNotSupportedException();
            }
            Console.WriteLine();

            var devices = (await DeviceInformation.FindAllAsync(BluetoothDevice.GetDeviceSelectorFromPairingState(true))).ToList();
            logger.Info("Paired Bluetooth devices:");
            for (int i = 0; i < devices.Count; i++)
            {
                logger.InfoFormat(" #{0} |  {1}", i, devices[i].Name);
            }
            Console.WriteLine();

            DeviceInformation? devInfo = null;

            if (devices.Count == 0)
            {
                throw new EnvironmentNotSupportedException("Cannot find paired bluetooth device. Abort.");
            }
            else if (devices.Count == 1)
            {
                devInfo = devices[0];
            }
            else
            {
                do
                {
                    logger.Info("ℹ️⌨️ Please choose the device by typing the device index ");
                    int i = ConsoleHelper.ReadNumberIndexFromConsole(logger);
                    if (i >= 0 || i < devices.Count)
                    {
                        devInfo = devices[i];
                    }
                }
                while (devInfo == null);
            }

            logger.InfoFormat("Selected device: {0}", devInfo.Name);
            BluetoothDevice bth = await BluetoothDevice.FromIdAsync(devInfo.Id);
            PhoneLineTransportDevice? phoneLine = await PhoneLineTransportHelper.GetPhoneLineTransportFromBluetoothDevice(bth);
            Assert.NotNull(phoneLine);

            DeviceInformation phoneLineDevInfo = await DeviceInformation.CreateFromIdAsync(phoneLine!.DeviceId);
            Assert.Equal(bth.Name, phoneLineDevInfo.Name);
            logger.InfoFormat("Corresponding PhoneLineTransportDevice: {0}", phoneLineDevInfo.Id);
        }


    }
}
