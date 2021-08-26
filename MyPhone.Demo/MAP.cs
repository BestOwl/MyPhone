using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using MyPhone.OBEX;

namespace MyPhone.Demo
{
    class MAP
    {
        
        public static void Main(string[] args)
        {
            Do().Wait();
        }

        public static async Task Do()
        {

        select:
            Console.Clear();
            var deviceId = await SelectDevice();

            if(string.IsNullOrEmpty(deviceId)) 
                goto select;
            else if(deviceId == "q")
                return;


            MapClient mapClient = new MapClient();
            bool success;


            DrawLine();            
            success = await mapClient.ClientBTConnect(deviceId);
            Console.WriteLine($"ClientBTConnect success is: {success}");
            if (!success)
            {
                Console.WriteLine("Not able to locally connect to the selected device BT hardware id.");
                goto restart;
            }

            DrawLine();
            success = await mapClient.MasObexConnect();
            Console.WriteLine($"MasObexConnect success is: {success}");
            if (!success)
            {
                Console.WriteLine("Not able to remotely connect to the selected device based on MAS protocol.");
                goto restart;
            }

            DrawLine();
            success = await mapClient.GetMessages();
            Console.WriteLine($"GetMessages success is: {success}");

            DrawLine();
            success = await mapClient.GetMASInstanceInformation();
            Console.WriteLine($"GetMASInstanceInformation success is: {success}");



            //DrawLine();
            //success = await mapClient.PushMessage();
            //Console.WriteLine($"PushMessage success is: {success}");


            //DrawLine();
            //success = await mapClient.GetFolderList();
            //Console.WriteLine($"GetFolderList success is: {success}");


            DrawLine();
            success = await mapClient.RemoteNotificationRegister();
            Console.WriteLine($"RemoteNotificationRegister success is: {success}");

            if (success)
            {
                DrawLine();
                await mapClient.BuildPcMns();
            }

        restart:

            Console.WriteLine("Enter q to exit or other keys to try again...");
            var c= Console.ReadKey();

            if (mapClient.BT_MNS_Provider != null)
                mapClient.BT_MNS_Provider.StopAdvertising();

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

            if (ent == "i") { return await SelectiPhone();  }
            else if(ent=="q") { return "q"; }
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
