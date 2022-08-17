using GoodTimeStudio.MyPhone.OBEX.Bluetooth;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Devices.Bluetooth;
using Windows.Networking.Sockets;

namespace GoodTimeStudio.MyPhone.OBEX.Pbap
{
    public class BluetoothPbapClientSession : BluetoothObexClientSession<PbapClient>
    {
        public static readonly Guid PHONE_BOOK_ACCESS_ID = new Guid("0000112f-0000-1000-8000-00805f9b34fb");

        public PbapSupportedFeatures SupportedFeatures { get; private set; }

        public Version? ProfileVersion { get; private set;}

        public BluetoothPbapClientSession(BluetoothDevice bluetoothDevice) : base(bluetoothDevice, PHONE_BOOK_ACCESS_ID, ObexServiceUuid.PhonebookAccess)
        {
        }

        protected override bool CheckFeaturesRequirementBySdpRecords()
        {
            Debug.Assert(SdpRecords != null);

            {
                if (SdpRecords.TryGetValue(0x9, out IReadOnlyCollection<byte>? rawAttributeValue)
                    && rawAttributeValue != null
                    && rawAttributeValue.Count >= 10)
                {
                    ProfileVersion = new Version(rawAttributeValue.ElementAt(8), rawAttributeValue.ElementAt(9));
                }
                else
                {
                    return false;
                }
            }

            {
                if (SdpRecords.TryGetValue(0x317, out IReadOnlyCollection<byte>? rawAttributeValue) && rawAttributeValue != null)
                {
                    SupportedFeatures = (PbapSupportedFeatures)BinaryPrimitives.ReadInt32BigEndian(
                        new ReadOnlySpan<byte>(rawAttributeValue.Skip(1).ToArray()));
                }
                else
                {
                    // For compatibility
                    SupportedFeatures = (PbapSupportedFeatures)0x3;
                }
            }
            
            return true;
        }

        public override PbapClient CreateObexClient(StreamSocket socket)
        {
            Debug.Assert(ProfileVersion != null);
            return new PbapClient(socket.InputStream, socket.OutputStream, SupportedFeatures, ProfileVersion);
        }
    }
}
