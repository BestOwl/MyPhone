using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.OBEX.Pbap
{
    [Flags]
    public enum PbapSupportedFeatures : int
    {
        None = 0,
        Download = 1,
        Browsing = 2,
        DatabaseIdentifier = 4,
        FolderVersionCounters = 8,
        VCardSelecting = 16,
        EnhancedMissedCalls = 32,
        X_BT_UCI_VCardProperty = 64,
        X_BT_UID_VCardProperty = 128,
        ContactReferencing = 256,
        DefaultContactImageFormat = 512
    }
}
