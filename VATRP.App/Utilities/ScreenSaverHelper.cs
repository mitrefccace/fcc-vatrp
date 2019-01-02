#region copyright
/**
 * Copyright © The MITRE Corporation.
 *
 * This program is licensed under the terms of the GNU General Public License Version 2, as published by the Free Software Foundation. This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  Please see the
 * GNU General Public License for more details.
 
 * This program or code contains content developed by The MITRE Corporation. If this program is used in a deployment or embedded within another project, MITRE requests that you send an email to opensource@mitre.org to let us know where and how this program is being used.
 
 * NOTICE
 * This (software/technical data) was produced for the U. S. Government under Contract Number HHSM-500-2012-00008I, and is subject to Federal Acquisition Regulation Clause 52.227-14, Rights in Data-General. No other use other than that granted to the U. S. Government, or to those acting on behalf of the U. S. Government under that Clause is authorized without the express written permission of The MITRE Corporation. For further information, please contact The MITRE Corporation, Contracts Management Office, 7515 Colshire Drive, McLean, VA  22102-7539, (703) 983-6000.
 */
#endregion

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using Win32Api = com.vtcsecure.ace.windows.Services.Win32NativeAPI;

namespace com.vtcsecure.ace.windows.Utilities
{
    public static class ScreenSaverHelper
    {
        // Signatures for unmanaged calls
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool SystemParametersInfo(int uAction, int uParam, ref int lpvParam, int flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool SystemParametersInfo(int uAction, int uParam, ref bool lpvParam, int flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int PostMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr OpenDesktop(string hDesktop, int Flags, bool Inherit, uint DesiredAccess);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool CloseDesktop(IntPtr hDesktop);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDesktopWindowsProc callback, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern IntPtr GetProcessWindowStation();

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern IntPtr SetProcessWindowStation(IntPtr hWinSta);
        
        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        // Callbacks
        private delegate bool EnumDesktopWindowsProc(IntPtr hDesktop, IntPtr lParam);

        // Constants
        private const int SPI_GETSCREENSAVERACTIVE = 16;
        private const int SPI_SETSCREENSAVERACTIVE = 17;
        private const int SPI_GETSCREENSAVERTIMEOUT = 14;
        private const int SPI_SETSCREENSAVERTIMEOUT = 15;
        private const int SPI_GETSCREENSAVERRUNNING = 114;
        private const int SPIF_SENDWININICHANGE = 2;

        private const uint DESKTOP_WRITEOBJECTS = 0x0080;
        private const uint DESKTOP_READOBJECTS = 0x0001;
        private const int WM_CLOSE = 16;
        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

        // Returns TRUE if the screen saver is active (enabled, but not necessarily running).
        public static bool IsScreenSaverActive()
        {
            bool isActive = false;

            SystemParametersInfo(SPI_GETSCREENSAVERACTIVE, 0, ref isActive, 0);
            return isActive;
        }

        // Pass in TRUE(1) to activate or FALSE(0) to deactivate the screen saver.
        public static void SetScreenSaverActive(int active)
        {
            int nullVar = 0;

            SystemParametersInfo(SPI_SETSCREENSAVERACTIVE, active, ref nullVar, SPIF_SENDWININICHANGE);
        }

        // Returns TRUE if the screen saver is actually running
        public static bool IsScreenSaverRunning()
        {
            bool isRunning = false;

            SystemParametersInfo(SPI_GETSCREENSAVERRUNNING, 0, ref isRunning, 0);
            return isRunning;
        }

        // From Microsoft's Knowledge Base article #140723: http://support.microsoft.com/kb/140723
        // "How to force a screen saver to close once started in Windows NT, Windows 2000, and Windows Server 2003"

        public static void KillScreenSaver()
        {
            IntPtr hDesktop = OpenDesktop("Screen-saver", 0, false, DESKTOP_READOBJECTS | DESKTOP_WRITEOBJECTS);
            if (hDesktop != IntPtr.Zero)
            {
                EnumDesktopWindows(hDesktop, new EnumDesktopWindowsProc(KillScreenSaverFunc), IntPtr.Zero);
                CloseDesktop(hDesktop);
            }
            else
            {
                PostMessage(GetForegroundWindow(), WM_CLOSE, 0, 0);
            }
        }

        private static bool KillScreenSaverFunc(IntPtr hWnd, IntPtr lParam)
        {
            if (IsWindowVisible(hWnd))
                PostMessage(hWnd, WM_CLOSE, 0, 0);
            return true;
        }

        internal static void SimulateMouseMoveEvent(Window activeWindow)
        {
            if (activeWindow == null)
                return;
            Win32Api.POINT pos;
            Win32Api.GetCursorPos(out pos);
            System.Drawing.Rectangle screenBounds; 
            try
            {
                System.Windows.Forms.Screen currentScreen =
                    System.Windows.Forms.Screen.FromHandle(
                        new System.Windows.Interop.WindowInteropHelper(activeWindow).Handle);
                screenBounds = currentScreen.Bounds;
            }
            catch
            {
                screenBounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            }
            var outputX = pos.X * 65535 / screenBounds.Width;
            var outputY = pos.Y * 65535 / screenBounds.Height;
            mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, (uint)outputX, (uint)outputY + 1, 0, UIntPtr.Zero);
        }
    }
}