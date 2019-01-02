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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using com.vtcsecure.ace.windows.Services;

namespace com.vtcsecure.ace.windows.Model
{
    public class FlashWindowHelper
    {
        private IntPtr theWindowPtr;
        public FlashWindowHelper()
        {
            theWindowPtr = IntPtr.Zero;
        }

        public void FlashWindow(Window wnd)
        {
            try
            {
                Window window = Window.GetWindow(wnd);
                if (window == null)
                    return;
                var wih = new WindowInteropHelper(wnd);
                theWindowPtr = wih.EnsureHandle();
                if (theWindowPtr != IntPtr.Zero)
                    Flash(theWindowPtr, 100);
            }
            catch (Exception ex)
            {
                ServiceManager.LogError("FlashWindow", ex);
            }
        }

        public void StopFlashing()
        {
            try
            {
                if (theWindowPtr == IntPtr.Zero)
                    return;

                if (Win2000OrLater)
                {
                    FLASHWINFO fi = CreateFlashInfoStruct(theWindowPtr, FLASHW_STOP, uint.MaxValue, 0);
                    FlashWindowEx(ref fi);
                }
                theWindowPtr = IntPtr.Zero;
            }
            catch (Exception ex)
            {
                ServiceManager.LogError("StopFlashing", ex);
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            /// <summary>
            /// The size of the structure in bytes.
            /// </summary>
            public uint cbSize;
            /// <summary>
            /// A Handle to the Window to be Flashed. The window can be either opened or minimized.
            /// </summary>
            public IntPtr hwnd;
            /// <summary>
            /// The Flash Status.
            /// </summary>
            public uint dwFlags;
            /// <summary>
            /// The number of times to Flash the window.
            /// </summary>
            public uint uCount;
            /// <summary>
            /// The rate at which the Window is to be flashed, in milliseconds. If Zero, the function uses the default cursor blink rate.
            /// </summary>
            public uint dwTimeout;
        }

        /// <summary>
        /// Stop flashing. The system restores the window to its original stae.
        /// </summary>
        public const uint FLASHW_STOP = 0;

        /// <summary>
        /// Flash the window caption.
        /// </summary>
        public const uint FLASHW_CAPTION = 1;

        /// <summary>
        /// Flash the taskbar button.
        /// </summary>
        public const uint FLASHW_TRAY = 2;

        /// <summary>
        /// Flash both the window caption and taskbar button.
        /// This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags.
        /// </summary>
        public const uint FLASHW_ALL = 3;

        /// <summary>
        /// Flash continuously, until the FLASHW_STOP flag is set.
        /// </summary>
        public const uint FLASHW_TIMER = 4;

        /// <summary>
        /// Flash continuously until the window comes to the foreground.
        /// </summary>
        public const uint FLASHW_TIMERNOFG = 12;

        /// <summary>
        /// Flash the spacified Window (Form) until it recieves focus.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        public static bool Flash(IntPtr hwnd)
        {
            // Make sure we're running under Windows 2000 or later
            if (Win2000OrLater)
            {
                FLASHWINFO fi = CreateFlashInfoStruct(hwnd, FLASHW_ALL | FLASHW_TIMERNOFG, uint.MaxValue, 0);

                return FlashWindowEx(ref fi);
            }
            return false;
        }

        private static FLASHWINFO CreateFlashInfoStruct(IntPtr handle, uint flags, uint count, uint timeout)
        {
            FLASHWINFO fi = new FLASHWINFO();
            fi.cbSize = Convert.ToUInt32(Marshal.SizeOf(fi));
            fi.hwnd = handle;
            fi.dwFlags = flags;
            fi.uCount = count;
            fi.dwTimeout = timeout;
            return fi;
        }

        /// <summary>
        /// Flash the specified Window (form) for the specified number of times
        /// </summary>
        /// <param name="hwnd">The handle of the Window to Flash.</param>
        /// <param name="count">The number of times to Flash.</param>
        /// <returns></returns>
        public static bool Flash(IntPtr hwnd, uint count)
        {
            if (Win2000OrLater)
            {
                FLASHWINFO fi = CreateFlashInfoStruct(hwnd, FLASHW_ALL | FLASHW_TIMERNOFG, count, 0);

                return FlashWindowEx(ref fi);
            }

            return false;
        }

        /// <summary>
        /// A boolean value indicating whether the application is running on Windows 2000 or later.
        /// </summary>
        private static bool Win2000OrLater
        {
            get { return Environment.OSVersion.Version.Major >= 5; }
        }
    }
}
