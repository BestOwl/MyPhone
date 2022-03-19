using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;

namespace GoodTimeStudio.MyPhone
{
    /// <summary>
    /// Temporary solution to display notification icon on notification area (system tray)
    /// as Windows App SDK does not offer this API currently as of 1.0
    /// </summary>
    public class NotifyIcon
    {
        private const uint WM_CONTEXTMENU = 0x007B;
        private const uint WM_APP = 0x8000;
        private const uint WM_NOTIFYICON = WM_APP + 1;

        private readonly IntPtr _hWnd;
        private readonly DestroyMenuSafeHandle _hMenu;

        private SUBCLASSPROC _subClassWndProc;
        private NOTIFYICONDATAW _nid;

        public NotifyIcon(IntPtr hWnd, IntPtr hIcon)
        {
            _hWnd = hWnd;

            //PInvoke.CreateWindowEx(
            //    WINDOW_EX_STYLE.WS_EX_NOACTIVATE | WINDOW_EX_STYLE.WS_EX_LAYERED | WINDOW_EX_STYLE.WS_EX_TOPMOST,
            //    "GoodTime"
            //    )

            _nid = new NOTIFYICONDATAW
            {
                cbSize = (uint)Marshal.SizeOf(typeof(NOTIFYICONDATAW)),
                uFlags = NOTIFY_ICON_DATA_FLAGS.NIF_MESSAGE | NOTIFY_ICON_DATA_FLAGS.NIF_ICON | NOTIFY_ICON_DATA_FLAGS.NIF_TIP | NOTIFY_ICON_DATA_FLAGS.NIF_SHOWTIP,
                uCallbackMessage = WM_NOTIFYICON,
                hWnd = (HWND)hWnd,
                uID = 0,
                szTip = "My Phone",
                hIcon = (Windows.Win32.UI.WindowsAndMessaging.HICON)hIcon,
                Anonymous = new NOTIFYICONDATAW._Anonymous_e__Union { uVersion = 4 },
            };
            if (!PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_ADD, _nid))
            {
                throw new PlatformNotSupportedException("Failed to create notification icon");
            }

            // Set the behaviour version, opt-in WM_CONTEXTMENU message
            if (!PInvoke.Shell_NotifyIcon(NOTIFY_ICON_MESSAGE.NIM_SETVERSION, _nid))
            {
                throw new PlatformNotSupportedException("Failed to set notification icon behaviour version");
            }

            _subClassWndProc = new SUBCLASSPROC(notifyWindowProc);
            PInvoke.SetWindowSubclass(
                (HWND)hWnd,
                _subClassWndProc,
                1u,                 // uIdSubclass
                0u                  // dwRefData
            );

            // Init context menu
            // Temporary regression: WinUI 3 does not currently support muti-window,
            //      so we use classic Win32 context menu for now 
            // TODO: use WinUI 3 to draw context menu
            _hMenu = PInvoke.CreatePopupMenu_SafeHandle();
            unsafe
            {
                fixed (char* str = "Open App")
                {
                    PInvoke.InsertMenuItem(_hMenu, 0, false, new MENUITEMINFOW
                    {
                        cbSize = (uint)Marshal.SizeOf(typeof(MENUITEMINFOW)),
                        fMask = MENU_ITEM_MASK.MIIM_FTYPE | MENU_ITEM_MASK.MIIM_STRING,
                        fType = MENU_ITEM_TYPE.MFT_STRING,
                        dwTypeData = (PWSTR)str
                    });
                }
                fixed (char* str = "Exit")
                {
                    PInvoke.InsertMenuItem(_hMenu, 1, false, new MENUITEMINFOW
                    {
                        cbSize = (uint)Marshal.SizeOf(typeof(MENUITEMINFOW)),
                        fMask = MENU_ITEM_MASK.MIIM_FTYPE | MENU_ITEM_MASK.MIIM_STRING,
                        fType = MENU_ITEM_TYPE.MFT_STRING,
                        dwTypeData = (PWSTR)str
                    });
                }
            }

            // Assign HMENU to a window to that it can be automatically destory when the window closes
            PInvoke.SetMenu((HWND) _hWnd, _hMenu);
        }

        private void showContextMenu(int x, int y)
        {
            /*
             * The current window must be the foreground window before the application calls 
             * TrackPopupMenu or TrackPopupMenuEx. 
             * Otherwise, the menu will not disappear when the user clicks outside of the menu
             * or the window that created the menu (if it is visible)
             */
            PInvoke.SetForegroundWindow((HWND)_hWnd);

            uint flag_TPM_LEFTALIGN = 0x0000;
            BOOL result = PInvoke.TrackPopupMenuEx(_hMenu, flag_TPM_LEFTALIGN, x, y, (HWND)_hWnd, null);
            if (result.Value == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        private LRESULT notifyWindowProc(HWND hWND, uint uMsg, WPARAM wParam, LPARAM lParam, nuint uldSubClass, nuint dwRefData)
        {
            switch (uMsg)
            {
                case WM_NOTIFYICON:
                    if (lowWord((int) lParam.Value) == WM_CONTEXTMENU)
                    {
                        int x = lowWord((int)wParam.Value);
                        int y = highWord((int)wParam.Value);

                        System.Diagnostics.Debug.WriteLine("notify icon show context menu, x: " + x + "    y: " + y);
                        showContextMenu(x, y);
                        
                    }
                    return new LRESULT(0);
            }
            return PInvoke.DefSubclassProc(hWND, uMsg, wParam, lParam);
        }
        private int lowWord(int word)
        {
            return word & 0xFFFF;
        }

        private int highWord(int word)
        {
            return (word >> 16) & 0xFFFF;
        }
    }
}
