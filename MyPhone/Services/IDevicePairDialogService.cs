using System;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Services
{
    /// <summary>
    /// The <see langword="interface"/> for a service that open a device pair dialog UI
    /// </summary>
    public interface IDevicePairDialogService
    {
        /// <summary>
        /// Show a dialog UI present the pair info and the PIN number, and asking user before continue to pair
        /// </summary>
        /// <param name="deviceName">The user-friendly name of the device</param>
        /// <param name="pairPIN">Pair PIN number</param>
        /// <param name="timeout">
        /// An optional time limit for the user to perform action. If timeout, the dialog will automatically hide.</param>
        /// <returns>Whether the user agrees to contine</returns>
        Task<bool> ShowPairDialogAsync(string deviceName, string pairPIN, TimeSpan? timeout = null);

        /// <summary>
        /// Hide the dialog
        /// </summary>
        void HideDialog();
    }
}
