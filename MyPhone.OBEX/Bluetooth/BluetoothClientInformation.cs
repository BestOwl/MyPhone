using System;
using System.Collections.Generic;
using System.Text;
using Windows.Networking;

namespace GoodTimeStudio.MyPhone.OBEX.Bluetooth
{
    public class BluetoothClientInformation
    {

        public HostName HostName { get; }

        public string ServiceName { get; }

        public BluetoothClientInformation(HostName hostName, string serviceName)
        {
            HostName = hostName;
            ServiceName = serviceName;
        }

        public override bool Equals(object? obj)
        {
            return obj is BluetoothClientInformation information &&
                   EqualityComparer<HostName>.Default.Equals(HostName, information.HostName) &&
                   ServiceName == information.ServiceName;
        }

        public override int GetHashCode()
        {
            int hashCode = 735311851;
            hashCode = hashCode * -1521134295 + EqualityComparer<HostName>.Default.GetHashCode(HostName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ServiceName);
            return hashCode;
        }

        public static bool operator ==(BluetoothClientInformation? left, BluetoothClientInformation? right)
        {
            return EqualityComparer<BluetoothClientInformation>.Default.Equals(left!, right!);
        }

        public static bool operator !=(BluetoothClientInformation? left, BluetoothClientInformation? right)
        {
            return !(left == right);
        }
    }
}
