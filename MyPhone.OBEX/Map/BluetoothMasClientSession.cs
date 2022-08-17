using GoodTimeStudio.MyPhone.OBEX.Bluetooth;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Devices.Bluetooth;
using Windows.Networking.Sockets;

namespace GoodTimeStudio.MyPhone.OBEX.Map
{
    public class BluetoothMasClientSession : BluetoothObexClientSession<MasClient>
    {
        public static readonly Guid MAP_Id = new Guid("00001132-0000-1000-8000-00805f9b34fb");

        public Version? ProfileVersion { get; private set; }

        public MapSupportedFeatures SupportedFeatures { get; private set; }

        public BluetoothMasClientSession(BluetoothDevice bluetoothDevice) : base(bluetoothDevice, MAP_Id, ObexServiceUuid.MessageAccess)
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
                    SupportedFeatures = (MapSupportedFeatures)BinaryPrimitives.ReadInt32BigEndian(
                        new ReadOnlySpan<byte>(rawAttributeValue.Skip(1).ToArray()));
                }
                else
                {
                    // For compatibility
                    SupportedFeatures = (MapSupportedFeatures)0x1F;
                }
            }

            return true;
        }

        public override MasClient CreateObexClient(StreamSocket socket)
        {
            return new MasClient(socket.InputStream, socket.OutputStream);
        }
    }
}
