using MyPhone.OBEX;
using MyPhone.OBEX.Pbap;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace MyPhone.Demo
{
    class PBAP
    {
        static BluetoothPbapClientSession PbapClientSession;

        public async static Task Main(string[] args)
        {
        select:
            Console.Clear();
            var deviceId = await SelectDevice();

            if (string.IsNullOrEmpty(deviceId))
                goto select;
            else if (deviceId == "q")
                return;


            DrawLine();
            BluetoothDevice BTDevice = await BluetoothDevice.FromIdAsync(deviceId);
            PbapClientSession = new BluetoothPbapClientSession(BTDevice);

            try
            {
                 await PbapClientSession.Connect();
            }
            catch (BluetoothObexSessionException ex)
            {
                Console.WriteLine(ex.Message);
                goto restart;
            }

            DrawLine();
            try
            {
                await PbapClientSession.ObexClient.PullPhoneBook("telecom/cch.vcf");
            }
            catch (ObexRequestException ex)
            {
                Console.WriteLine(ex.Message);
                goto restart;
            }

        restart:
            Console.WriteLine("Enter q to exit or other keys to try again...");
            var c = Console.ReadKey();

            if (PbapClientSession != null)
                PbapClientSession.Dispose();

            if (c.KeyChar.Equals('q'))
            {
                return;
            }
            else
            {
                goto select;
            }
        }

        private static void DrawLine()
        {
            Console.WriteLine(new string('*', 50));
        }

        private static async Task<string> SelectDevice()
        {
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(BluetoothDevice.GetDeviceSelectorFromPairingState(true));
            for (int i = 0; i < devices.Count; i++)
            {
                Console.WriteLine(i + " #:    " + devices[i].Name + "    " + devices[i].Id);
            }

            Console.WriteLine("Please input device id to select or 'i' for iPhone or 'q' to quit: ");

            string ent = Console.ReadLine();

            if (ent == "i") { return await SelectiPhone(); }
            else if (ent == "q") { return "q"; }
            else
            {

                if (int.TryParse(ent, out int s))
                {
                    if (s >= 0 && s < devices.Count)
                    {
                        Console.WriteLine("Selected: " + devices[s].Name + "    " + devices[s].Id);
                        return devices[s].Id;
                    }
                }
            }

            return "";
        }

        private static async Task<string> SelectiPhone()
        {
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(BluetoothDevice.GetDeviceSelectorFromPairingState(true));
            for (int i = 0; i < devices.Count; i++)
            {
                if (devices[i].Name.IndexOf("iphone", StringComparison.CurrentCultureIgnoreCase) != -1)
                {
                    Console.WriteLine("Selected: " + devices[i].Name + "    " + devices[i].Id);
                    return devices[i].Id;
                }
            }

            Console.WriteLine("No iPhone found.");
            return string.Empty;
        }
    }
}
