using CommunityToolkit.Mvvm.ComponentModel;
using GoodTimeStudio.MyPhone.Models;
using Microsoft.UI.Dispatching;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace GoodTimeStudio.MyPhone.Controls
{
    public partial class BluetoothDeviceListViewModel : ObservableObject
    {
        private DeviceWatcher _DeviceWatcher;
        private DispatcherQueue _dispatcherQueue;
        private TaskCompletionSource<bool>? _waitForFullStop;

        public ObservableCollection<DeviceInformationEx> Devices;

        public BluetoothDeviceListViewModel()
        {
            Devices = new ObservableCollection<DeviceInformationEx>();
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            //var s1 = BluetoothDevice.GetDeviceSelectorFromClassOfDevice(BluetoothClassOfDevice.FromParts(BluetoothMajorClass.Phone, BluetoothMinorClass.PhoneCellular, BluetoothServiceCapabilities.TelephoneService));
            //var s2 = PhoneLineTransportDevice.GetDeviceSelector(PhoneLineTransport.Bluetooth);
            //_DeviceWatcher = DeviceInformation.CreateWatcher(s2);
            //_DeviceWatcher = DeviceInformation.CreateWatcher("System.Devices.AepService.ProtocolId:=\"{E0CBF06C-CD8B-4647-BB8A-263B43F0F974}\" ", null, DeviceInformationKind.AssociationEndpointService);

            // Currently Bluetooth APIs don't provide a selector to get ALL devices that are both paired and non-paired.
            // So we need to do it mannually, this is the Bluetooth selector that include both paired and unpaird devices
            // 

            // TODO: use IDeviceService

            string bluttoothSelector = "System.Devices.Aep.ProtocolId:=\"{E0CBF06C-CD8B-4647-BB8A-263B43F0F974}\" AND (System.Devices.Aep.CanPair:=System.StructuredQueryType.Boolean#True OR System.Devices.Aep.IsPaired:=System.StructuredQueryType.Boolean#True)";
            _DeviceWatcher = DeviceInformation.CreateWatcher(bluttoothSelector, null, DeviceInformationKind.AssociationEndpoint);

            _DeviceWatcher.Added += _DeviceWatcher_Added;
            _DeviceWatcher.Removed += _DeviceWatcher_Removed;
            _DeviceWatcher.Updated += _DeviceWatcher_Updated;
            _DeviceWatcher.Stopped += _DeviceWatcher_Stopped;
        }

        private void _DeviceWatcher_Stopped(DeviceWatcher sender, object args)
        {
            _waitForFullStop!.SetResult(true);
        }

        private void _DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            _dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
            {
                var de = Devices.Where(d => d.Id == args.Id).FirstOrDefault();
                if (de != null)
                {
                    de.Update(args);
                }
            });
        }

        private void _DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            _dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
            {
                var de = Devices.Where(d => d.Id == args.Id).FirstOrDefault();
                if (de != null)
                {
                    Devices.Remove(de);
                }
            });
        }


        private void _DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            // Since we have the collection databound to a UI element, we need to update the collection on the UI thread.
            _dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
            {
                Devices.Add(new DeviceInformationEx(args));
            });
        }

        public async Task DeviceScanStart()
        {
            switch (_DeviceWatcher.Status)
            {
                case DeviceWatcherStatus.Created:
                case DeviceWatcherStatus.Stopped:
                case DeviceWatcherStatus.Aborted:
                    _DeviceWatcher.Start();
                    break;
                case DeviceWatcherStatus.Stopping:
                    await _waitForFullStop!.Task;
                    break;
                default:
                    break;
            }
        }

        public void DeviceScanStop()
        {
            switch (_DeviceWatcher.Status)
            {
                case DeviceWatcherStatus.EnumerationCompleted:
                case DeviceWatcherStatus.Started:
                    _DeviceWatcher.Stop();
                    _waitForFullStop = new TaskCompletionSource<bool>();
                    break;
                default:
                    break;
            }
        }
    }
}
