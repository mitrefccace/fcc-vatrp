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
using System.Runtime.InteropServices;

namespace VATRP.LinphoneWrapper
{
    public static class Win32NativeAPI
    {
        [DllImport("winmm.dll", EntryPoint = "PlaySound", SetLastError = true)]
        internal static extern bool PlaySound(byte[] ptrToSound, UIntPtr hmod, SoundFlags fdwSound);
        
        [DllImport("winmm.dll", EntryPoint = "PlaySound", SetLastError = true)]
        internal static extern bool PlaySound(IntPtr ptrToSound, UIntPtr hmod, SoundFlags fdwSound);

        [Flags]
        internal enum SoundFlags
        {
            SND_SYNC = 0x0000, // play synchronously (default)
            SND_ASYNC = 0x0001, // play asynchronously
            SND_NODEFAULT = 0x0002, // silence (!default) if sound not found
            SND_MEMORY = 0x0004, // pszSound points to a memory file
            SND_LOOP = 0x0008, // loop the sound until next sndPlaySound
            SND_NOSTOP = 0x0010, // don't stop any currently playing sound
            SND_NOWAIT = 0x00002000, // don't wait if the driver is busy
            SND_ALIAS = 0x00010000, // name is a registry alias
            SND_ALIAS_ID = 0x00110000, // alias is a predefined id
            SND_FILENAME = 0x00020000, // name is file name
            SND_PURGE = 0x0040 // purge non-static events for task
        }

        [DllImport("user32.dll")]
        public static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hWnd);
    }
}
