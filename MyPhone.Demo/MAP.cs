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
using MixERP.Net.VCards.Models;
using MyPhone.OBEX.Map;
using System.Collections.Concurrent;

namespace MyPhone.Demo
{
    class MAP
    {
        static BluetoothMasClientSession MasClientSession;
        static BluetoothMnsServerSession MnsServerSession;
        static ConcurrentQueue<string> NewMessageQueue = new ConcurrentQueue<string>();

        public async static Task Main(string[] args)
        {
        select:
            Console.Clear();
            var deviceId = await SelectDevice();

            if (string.IsNullOrEmpty(deviceId))
                goto select;
            else if (deviceId == "q")
                return;

            try
            {
                DrawLine();
                BluetoothDevice BTDevice = await BluetoothDevice.FromIdAsync(deviceId);
                MasClientSession = new BluetoothMasClientSession(BTDevice);
                await MasClientSession.Connect();
                Console.WriteLine($"MAS service connected");

                DrawLine();
                MnsServerSession = new BluetoothMnsServerSession();
                await MnsServerSession.StartServer();
                Console.WriteLine("MNS server started");
            }
            catch (BluetoothObexSessionException ex)
            {
                Console.WriteLine(ex.Message);
                goto restart;
            }

            DrawLine();
            try
            {
                await MasClientSession.ObexClient.SetNotificationRegistration(true);
                MnsServerSession.ClientAccepted += MnsServerSession_ClientAccepted;
            }
            catch (ObexRequestException ex)
            {
                Console.WriteLine("RemoteNotificationRegister failed. " + ex.Message);
                goto restart;
            }
            Console.WriteLine($"RemoteNotificationRegister success");

            Console.WriteLine();
            DrawLine();
            Console.WriteLine("Message Access Service and Message Notification Service established, waiting for event");
            Console.WriteLine("Press any key to abort");
            DrawLine();

            while (!Console.KeyAvailable)
            {
                if (NewMessageQueue.TryDequeue(out string handle))
                {
                    Console.WriteLine("event received");
                    BMessage bMsg;
                    try
                    {
                        bMsg = await MasClientSession.ObexClient.GetMessage(handle);
                    }
                    catch (ObexRequestException ex)
                    {
                        Console.WriteLine(ex.Message);
                        goto restart;
                    }

                    DrawLine();
                    Console.WriteLine("New message received");
                    Console.WriteLine($"Sender: ");
                    Console.WriteLine("\t" + bMsg.Sender.FormattedName);
                    Console.Write("\tTel: ");
                    if (bMsg.Sender.Telephones != null)
                    {
                        foreach (Telephone telephone in bMsg.Sender.Telephones)
                        {
                            Console.Write(telephone.Number);
                            Console.Write("; ");
                        }
                    }
                    Console.WriteLine();
                    if (bMsg.Sender.Emails != null)
                    {
                        Console.Write("\tEmail address: ");
                        foreach (Email email in bMsg.Sender.Emails)
                        {
                            Console.Write(email.EmailAddress);
                            Console.Write("; ");
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine($"Body: {bMsg.Body}");
                }
            }

            return;

        restart:

            Console.WriteLine("Enter q to exit or other keys to try again...");
            var c = Console.ReadKey();

            if (MnsServerSession != null)
                MnsServerSession.Dispose();
            if (MasClientSession != null)
                MasClientSession.Dispose();

            if (c.KeyChar.Equals('q'))
            {
                return;
            }
            else
            {
                goto select;
            }
        }

        private static void MnsServerSession_ClientAccepted(BluetoothObexServerSession<MnsServer> sender, BluetoothObexServerSessionClientAcceptedEventArgs<MnsServer> e)
        {
            e.ObexServer.MessageReceived += ObexServer_MessageReceived;
        }

        private static void ObexServer_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            NewMessageQueue.Enqueue(e.MessageHandle);
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
