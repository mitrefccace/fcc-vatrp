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
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_ms_factory(IntPtr lc);

        [DllImport(MSBase_DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_factory_get_snd_card_manager(IntPtr f);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_sound_devices(IntPtr lc);

        [DllImport(MSBase_DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_snd_card_manager_get_list(IntPtr m);

        [DllImport(MSBase_DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_snd_card_get_string_id(IntPtr obj);

        [DllImport(MSBase_DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_snd_card_get_name(IntPtr obj);

        [DllImport(MSBase_DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_snd_card_get_driver_type(IntPtr obj);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_reload_sound_devices(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_sound_device_can_capture(IntPtr lc, string device);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_sound_device_can_playback(IntPtr lc, string device);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_ring_level(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_play_level(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_rec_level(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_ring_level(IntPtr lc, int level);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_play_level(IntPtr lc, int level);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]

        public static extern void linphone_core_set_mic_gain_db(IntPtr lc, float level);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_core_get_mic_gain_db(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_playback_gain_db(IntPtr lc, float level);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_core_get_playback_gain_db(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]

        public static extern void linphone_core_set_rec_level(IntPtr lc, int level);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_ringer_device(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_playback_device(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_capture_device(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_ringer_device(IntPtr lc, string devid);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_playback_device(IntPtr lc, string devid);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_capture_device(IntPtr lc, string devid);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_stop_ringing(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_ring(IntPtr lc, string path);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_ring(IntPtr lc);

        [DllImport(MSVoip_DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ms_static_image_set_default_image(string path);

        [DllImport(MSBase_DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void video_stream_change_camera(IntPtr stream, IntPtr cam);

        [DllImport(MSBase_DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_video_device(IntPtr call);
    }
}
