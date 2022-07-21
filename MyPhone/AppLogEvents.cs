using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone
{
    internal static class AppLogEvents
    {
        internal static EventId AppLaunch = new(0, nameof(AppLaunch));
        internal static EventId DeviceSetup = new(1, nameof(DeviceSetup));

        internal static EventId CallServiceConnect = new(1001, nameof(CallServiceConnect));
        internal static EventId CallServiceDisconnect = new(1002, nameof(CallServiceDisconnect));
        internal static EventId CallServicePhoneLineDiscovery = new(1003, nameof(CallServicePhoneLineDiscovery));

        internal static EventId SmsServiceConnect = new(2001, nameof(SmsServiceConnect));
        internal static EventId SmsServiceDisconnect = new(2002, nameof(SmsServiceDisconnect));
        internal static EventId SmsServiceMnsAccepted = new(2003, nameof(SmsServiceMnsAccepted));
        internal static EventId SmsServiceMessageReceived = new(2004, nameof(SmsServiceMessageReceived));

        internal static EventId PhonebookServiceConnect = new(3001, nameof(PhonebookServiceConnect));
        internal static EventId PhonebookServiceDisconnect = new(3002, nameof(PhonebookServiceDisconnect));
    }
}
