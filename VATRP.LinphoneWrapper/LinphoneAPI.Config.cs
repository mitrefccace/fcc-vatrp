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
using System.Threading.Tasks;
using VATRP.LinphoneWrapper.Enums;

namespace VATRP.LinphoneWrapper
{
    public static partial class LinphoneAPI
    {
        /**void linphone_proxy_config_set_dial_prefix (LinphoneProxyConfig* cfg, const char* prefix)
         * Sets a dialing prefix to be automatically prepended when inviting a number with linphone_core_invite(); This dialing prefix shall usually be the country code of the country where the user is living.
         * */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_proxy_config_set_dial_prefix(IntPtr cfg, string route);

        /** void linphone_proxy_config_set_dial_escape_plus	(LinphoneProxyConfig * 	cfg, bool_t val)
         * Sets whether liblinphone should replace "+" by international calling prefix in dialed numbers (passed to linphone_core_invite ).
         * */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_proxy_config_set_dial_escape_plus(IntPtr cfg, bool enable);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_config(IntPtr linphoneCore);

        //[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        //public static extern IntPtr lp_config_set_int(IntPtr lpconfig, string section, string key, int value);

        // Added 3/28 MITRE-fjr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_config_set_int(IntPtr lpconfig, string section, string key, int value);

        // cjm-jul18 -- adding so i can change the from_domain on login
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_config_set_string(IntPtr lpconfig, string section, string key, string value);

        // cjm-jul18 -- adding so i can change the from_domain on login
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_config_get_string(IntPtr lpconfig, string section, string key, string default_string );

        // cjm-jul18 -- adding so i can change the from_domain on login, returns LinphoneStatus
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_config_sync(IntPtr lpconfig);
    }
}
