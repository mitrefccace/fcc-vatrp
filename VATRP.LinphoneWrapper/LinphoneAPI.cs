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
using VATRP.LinphoneWrapper.Enums;
using VATRP.LinphoneWrapper.Structs;

namespace VATRP.LinphoneWrapper
{
    public static partial class LinphoneAPI
    {
        public const string DllName = "linphone.dll";
        public const string MSBase_DllName = "mediastreamer_base.dll";
        public const string MSVoip_DllName = "mediastreamer_voip.dll";

        #region Constants

        public const int LC_SIP_TRANSPORT_RANDOM = -1; // Randomly chose a sip port for this transport
        public const int LC_SIP_TRANSPORT_DISABLED = 0; // Disable a sip transport

        public const int LINPHONE_FIND_PAYLOAD_IGNORE_RATE = -1;
        // Wildcard value used by #linphone_core_find_payload_type to ignore rate in search algorithm

        public const int LINPHONE_FIND_PAYLOAD_IGNORE_CHANNELS = -1;
        // Wildcard value used by #linphone_core_find_payload_type to ignore channel in search algorithm

        public static int LINPHONE_CALL_STATS_AUDIO = 0;
        public static int LINPHONE_CALL_STATS_VIDEO = 1;
        public static int LINPHONE_CALL_STATS_TEXT = 2;


        #endregion

        #region 3.12.0

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_proxy_config_set_custom_header(IntPtr proxyCfg, string headerName,string headerValue);

        #endregion

        #region Methods

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_version();

        public static string linphone_core_get_version_asString()
        {
            IntPtr ptr = linphone_core_get_version();
            // assume returned string is utf-8 encoded
            return PtrToStringUtf8(ptr);
        }

        public static string PtrToStringUtf8(IntPtr ptr) // aPtr is nul-terminated
        {
            if (ptr == IntPtr.Zero)
                return "";
            int len = 0;
            while (System.Runtime.InteropServices.Marshal.ReadByte(ptr, len) != 0)
                len++;
            if (len == 0)
                return "";
            byte[] array = new byte[len];
            System.Runtime.InteropServices.Marshal.Copy(ptr, array, 0, len);
            return System.Text.Encoding.UTF8.GetString(array);
        }

        // cjm-sep17 -- trying to fix 401 error in WS during 1st registration attempt
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_nat_address(IntPtr lc, string addr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_logs(IntPtr file);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_disable_logs();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_logs_with_cb(IntPtr logfunc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_log_level_mask(OrtpLogLevel loglevel);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_new(IntPtr vtable, string config_path, string factory_config,
            IntPtr userdata);

        //[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        //public static extern IntPtr linphone_core_new_with_config(IntPtr vtable, string config_path, string factory_config,
        //    IntPtr userdata);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_destroy(IntPtr lc);

        //  Added MITRE-fjr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_unref(IntPtr LinphoneCore);

        //  Added MITRE-fjr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_remove_listener(IntPtr LinphoneCore, IntPtr vTable);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_create_proxy_config(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_clear_proxy_config(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_default_proxy_config(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_proxy_config_normalize_sip_uri(IntPtr proxy, string username);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_primary_contact(IntPtr proxy, string contact_params);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_auth_info_new(string username, string userid, string passwd,
            string ha1, string realm, string domain);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_create_nat_policy(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_remove_auth_info(IntPtr lc, IntPtr auth_info);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_add_auth_info(IntPtr lc, IntPtr info);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_find_auth_info(IntPtr lc, string realm, string username, string sip_domain);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_auth_info_set_tls_cert_path(IntPtr auth_info, string cert_path);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern string linphone_auth_info_get_tls_cert_path(IntPtr auth_info);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_clear_all_auth_info(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_proxy_config_set_identity(IntPtr obj, string identity);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_proxy_config_set_server_addr(IntPtr cfg, string server_addr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_proxy_config_enable_register(IntPtr obj, bool val);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_proxy_config_set_avpf_mode(IntPtr cfg, LinphoneAVPFMode mode);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_proxy_config_set_avpf_rr_interval(IntPtr cfg, byte interval);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_address_destroy(IntPtr u);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_add_proxy_config(IntPtr lc, IntPtr cfg);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_default_proxy_config(IntPtr lc, IntPtr config);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_iterate(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_keep_alive(IntPtr lc, bool enable);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_keep_alive_enabled(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_create_default_call_parameters(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_params_add_custom_header(IntPtr cp, string header_name, string header_value);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_params_enable_video(IntPtr cp, bool enabled);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_params_set_audio_bandwidth_limit(IntPtr cp, int kbit);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_call_params_video_enabled(IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_params_set_video_direction(IntPtr cp, LinphoneMediaDirection dir);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_in_call(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_current_call(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_params_enable_early_media_sending(IntPtr cp, bool enabled);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_invite(IntPtr lc, string url);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_invite_address(IntPtr lc, IntPtr addr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_invite_with_params(IntPtr lc, string url, IntPtr param);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_params_destroy(IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_update_call(IntPtr lc, IntPtr call, IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_accept_call_update(IntPtr lc, IntPtr call, IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_create_call_params(IntPtr lc, IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_terminate_call(IntPtr lc, IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_terminate_all_calls(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_default_proxy(IntPtr lc, ref IntPtr config);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_proxy_config_is_registered(IntPtr config);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern LinphoneRegistrationState linphone_proxy_config_get_state(IntPtr cfg);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_proxy_config_edit(IntPtr config);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_proxy_config_refresh_register(IntPtr cfg); // cjm-aug17

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_proxy_config_done(IntPtr config);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_call_get_current_quality(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_call_get_average_quality(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_remote_address_as_string(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_media_encryption(IntPtr lc, LinphoneMediaEncryption menc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern LinphoneMediaEncryption linphone_core_get_media_encryption(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_is_media_encryption_mandatory(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_accept_call_with_params(IntPtr lc, IntPtr call, IntPtr callparams);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_decline_call(IntPtr lc, IntPtr call, LinphoneReason reason);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_redirect_call(IntPtr lc, IntPtr call, string redirect_uri);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_pause_call(IntPtr lc, IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]

        public static extern int linphone_core_pause_all_calls(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_resume_call(IntPtr lc, IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_defer_call_update(IntPtr lc, IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_start_recording(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_stop_recording(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_params_set_record_file(IntPtr callparams, string filename);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_params_get_record_file(IntPtr callparams);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_sip_transports(IntPtr lc, IntPtr tr_config);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_user_agent(IntPtr lc, string ua_name, string version);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_play_file(IntPtr lc, string file);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_record_file(IntPtr lc, string file);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_play_dtmf(IntPtr lc, char dtmf, int duration_ms);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_stop_dtmf(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_use_files(IntPtr lc, bool yesno);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_get_state(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_ref(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_unref(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]

        public static extern IntPtr linphone_call_get_chat_room(IntPtr call);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_verify_server_certificates(IntPtr lc, bool yesno);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_verify_server_cn(IntPtr lc, bool yesno);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_root_ca(IntPtr lc, string path);

        // cjm-sep17 -- maybe TLS key can be used for WS decrypting 
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_tls_cert(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_root_ca(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_ringback(IntPtr lc, string path);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_ringback(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_download_bandwidth(IntPtr lc, int bw);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_upload_bandwidth(IntPtr lc, int bw);

/**
 * Specify a ring back tone to be played to far end during incoming calls.
 * @param[in] lc #LinphoneCore object
 * @param[in] ring The path to the ring back tone to be played.
 * @ingroup media_parameters
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_remote_ringback_tone(IntPtr lc, string ring);

/**
 * Get the ring back tone played to far end during incoming calls.
 * @param[in] lc #LinphoneCore object
 * @ingroup media_parameters
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_remote_ringback_tone(IntPtr lc);

/**
 * Enable or disable the ring play during an incoming early media call.
 * @param[in] lc #LinphoneCore object
 * @param[in] enable A boolean value telling whether to enable ringing during an incoming early media call.
 * @ingroup media_parameters
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_ring_during_incoming_early_media(IntPtr lc, bool enable);

/**
 * Tells whether the ring play is enabled during an incoming early media call.
 * @param[in] lc #LinphoneCore object
 * @ingroup media_paramaters
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_get_ring_during_incoming_early_media(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_play_local(IntPtr lc, string audiofile);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_echo_cancellation(IntPtr lc, bool val);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_echo_cancellation_enabled(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]

        public static extern void linphone_core_enable_echo_limiter(IntPtr lc, bool val);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_echo_limiter_enabled(IntPtr lc);

/**
 * Enable or disable the microphone.
 * @param[in] lc #LinphoneCore object
 * @param[in] enable TRUE to enable the microphone, FALSE to disable it.
 * @ingroup media_parameters
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_mic(IntPtr lc, bool enable);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_call_media_in_progress(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_network_reachable(IntPtr lc, bool value);
        
        /**
 * @ingroup network_parameters
 * return network state either as positioned by the application or by linphone itself.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_is_network_reachable(IntPtr lc);

/**
 * @ingroup network_parameters
 * This method is called by the application to notify the linphone core library when the SIP network is reachable.
 * This is for advanced usage, when SIP and RTP layers are required to use different interfaces.
 * Most applications just need linphone_core_set_network_reachable().
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_sip_network_reachable(IntPtr lc, bool value);

/**
 * @ingroup network_parameters
 * This method is called by the application to notify the linphone core library when the media (RTP) network is reachable.
 * This is for advanced usage, when SIP and RTP layers are required to use different interfaces.
 * Most applications just need linphone_core_set_network_reachable().
 */
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern	void linphone_core_set_media_network_reachable(IntPtr lc, bool value);

/**
 * Tells whether the microphone is enabled.
 * @param[in] lc #LinphoneCore object
 * @return TRUE if the microphone is enabled, FALSE if disabled.
 * @ingroup media_parameters
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_mic_enabled(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_is_rtp_muted(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_get_rtp_no_xmit_on_audio_mute(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_rtp_no_xmit_on_audio_mute(IntPtr lc, bool val);

        /* video support */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_video_supported(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_video_preset(IntPtr lc, string preset);

/**
 * Enable or disable video capture.
 *
 * This function does not have any effect during calls. It just indicates the #LinphoneCore to
 * initiate future calls with video capture or not.
 * @param[in] lc #LinphoneCore object.
 * @param[in] enable TRUE to enable video capture, FALSE to disable it.
 * @ingroup media_parameters
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_video_capture(IntPtr lc, bool enable);

/**
 * Enable or disable video display.
 *
 * This function does not have any effect during calls. It just indicates the #LinphoneCore to
 * initiate future calls with video display or not.
 * @param[in] lc #LinphoneCore object.
 * @param[in] enable TRUE to enable video display, FALSE to disable it.
 * @ingroup media_parameters
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_video_display(IntPtr lc, bool enable);


/**
 * Enable or disable video source reuse when switching from preview to actual video call.
 *
 * This source reuse is useful when you always display the preview, even before calls are initiated.
 * By keeping the video source for the transition to a real video call, you will smooth out the
 * source close/reopen cycle.
 *
 * This function does not have any effect durfing calls. It just indicates the #LinphoneCore to
 * initiate future calls with video source reuse or not.
 * Also, at the end of a video call, the source will be closed whatsoever for now.
 * @param[in] lc #LinphoneCore object
 * @param[in] enable TRUE to enable video source reuse. FALSE to disable it for subsequent calls.
 * @ingroup media_parameters
 *
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_video_source_reuse(IntPtr lc, bool enable);

/**
 * Tells whether video capture is enabled.
 * @param[in] lc #LinphoneCore object.
 * @return TRUE if video capture is enabled, FALSE if disabled.
 * @ingroup media_parameters
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_video_capture_enabled(IntPtr lc);

/**
 * Tells whether video display is enabled.
 * @param[in] lc #LinphoneCore object.
 * @return TRUE if video display is enabled, FALSE if disabled.
 * @ingroup media_parameters
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_video_display_enabled(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_video_policy(IntPtr lc, IntPtr policy);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_video_policy(IntPtr lc);

/**
 * Returns the zero terminated table of supported video resolutions.
 *
 * @ingroup media_parameters
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_supported_video_sizes(IntPtr lc);

/**
 * Sets the preferred video size.
 *
 * @ingroup media_parameters
 * This applies only to the stream that is captured and sent to the remote party,
 * since we accept all standard video size on the receive path.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_preferred_video_size(IntPtr lc, IntPtr vsize);

/**
 * Sets the video size for the captured (preview) video.
 * This method is for advanced usage where a video capture must be set independently of the size of the stream actually sent through the call.
 * This allows for example to have the preview window with HD resolution even if due to bandwidth constraint the sent video size is small.
 * Using this feature increases the CPU consumption, since a rescaling will be done internally.
 * @ingroup media_parameters
 * @param lc the linphone core
 * @param vsize the video resolution choosed for capuring and previewing. It can be (0,0) to not request any specific preview size and let the core optimize the processing.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_preview_video_size(IntPtr lc, IntPtr vsize);

/**
 * Sets the preview video size by its name. See linphone_core_set_preview_video_size() for more information about this feature.
 *
 * @ingroup media_parameters
 * Video resolution names are: qcif, svga, cif, vga, 4cif, svga ...
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_preview_video_size_by_name(IntPtr lc, string name);

/**
 * Returns video size for the captured video if it was previously set by linphone_core_set_preview_video_size(), otherwise returns a 0,0 size.
 * @see linphone_core_set_preview_video_size()
 * @ingroup media_parameters
 * @param lc the core
 * @return a MSVideoSize
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern MSVideoSize linphone_core_get_preview_video_size(IntPtr lc);

/**
 * Returns the effective video size for the captured video as provided by the camera.
 * When preview is disabled or not yet started, this function returns a zeroed video size.
 * @see linphone_core_set_preview_video_size()
 * @ingroup media_parameters
 * @param lc the core
 * @return a MSVideoSize
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern MSVideoSize linphone_core_get_current_preview_video_size(IntPtr lc);

/**
 * Returns the current preferred video size for sending.
 *
 * @ingroup media_parameters
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern MSVideoSize linphone_core_get_preferred_video_size(IntPtr lc);

/**
 * Get the name of the current preferred video size for sending.
 * @param[in] lc #LinphoneCore object.
 * @return A string containing the name of the current preferred video size (to be freed with ms_free()).
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_preferred_video_size_name(IntPtr lc);

/**
 * Sets the preferred video size by its name.
 *
 * @ingroup media_parameters
 * This is identical to linphone_core_set_preferred_video_size() except
 * that it takes the name of the video resolution as input.
 * Video resolution names are: qcif, svga, cif, vga, 4cif, svga ...
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_preferred_video_size_by_name(IntPtr lc, string name);

/**
 * Set the preferred frame rate for video.
 * Based on the available bandwidth constraints and network conditions, the video encoder
 * remains free to lower the framerate. There is no warranty that the preferred frame rate be the actual framerate.
 * used during a call. Default value is 0, which means "use encoder's default fps value".
 * @ingroup media_parameters
 * @param lc the LinphoneCore
 * @param fps the target frame rate in number of frames per seconds.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_preferred_framerate(IntPtr lc, float fps);

/**
 * Returns the preferred video framerate, previously set by linphone_core_set_preferred_framerate().
 * @ingroup media_parameters
 * @param lc the linphone core
 * @return frame rate in number of frames per seconds.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_core_get_preferred_framerate(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_video_preview(IntPtr lc, bool val);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_video_preview_enabled(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]

        public static extern void linphone_core_enable_self_view(IntPtr lc, bool val);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_self_view_enabled(IntPtr lc);


/**
 * Update detection of camera devices.
 *
 * Use this function when the application is notified of USB plug events, so that
 * list of available hardwares for video capture is updated.
 * @param[in] lc #LinphoneCore object.
 * @ingroup media_parameters
 **/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_reload_video_devices(IntPtr lc);


        /* returns a null terminated static array of string describing the webcams */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_video_devices(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_video_device(IntPtr lc, string id);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_video_device(IntPtr lc);

/* Set and get static picture to be used when "Static picture" is the video device */
/**
 * Set the path to the image file to stream when "Static picture" is set as the video device.
 * @param[in] lc #LinphoneCore object.
 * @param[in] path The path to the image file to use.
 * @ingroup media_parameters
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_static_picture(IntPtr lc, string path);

/**
 * Get the path to the image file streamed when "Static picture" is set as the video device.
 * @param[in] lc #LinphoneCore object.
 * @return The path to the image file streamed when "Static picture" is set as the video device.
 * @ingroup media_parameters
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_static_picture(IntPtr lc);

/**
 * Set the frame rate for static picture.
 * @param[in] lc #LinphoneCore object.
 * @param[in] fps The new frame rate to use for static picture.
 * @ingroup media_parameters
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_static_picture_fps(IntPtr lc, float fps);

/**
 * Get the frame rate for static picture
 * @param[in] lc #LinphoneCore object.
 * @return The frame rate used for static picture.
 * @ingroup media_parameters
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_core_get_static_picture_fps(IntPtr lc);

/*function to be used for eventually setting window decorations (icons, title...)*/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_native_video_window_id(IntPtr lc);

/**
 * @ingroup media_parameters
 * Set the native video window id where the video is to be displayed.
 * For MacOS, Linux, Windows: if not set or LINPHONE_VIDEO_DISPLAY_AUTO the core will create its own window, unless the special id LINPHONE_VIDEO_DISPLAY_NONE is given.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_native_video_window_id(IntPtr lc, long id);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_native_preview_window_id(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_native_preview_window_id(IntPtr lc, long id);

/**
 * Tells the core to use a separate window for local camera preview video, instead of
 * inserting local view within the remote video window.
 * @param[in] lc #LinphoneCore object.
 * @param[in] yesno TRUE to use a separate window, FALSE to insert the preview in the remote video window.
 * @ingroup media_parameters
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_use_preview_window(IntPtr lc, bool yesno);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_device_rotation(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_device_rotation(IntPtr lc, int rotation);

/**
 * Get the camera sensor rotation.
 *
 * This is needed on some mobile platforms to get the number of degrees the camera sensor
 * is rotated relative to the screen.
 *
 * @param lc The linphone core related to the operation
 * @return The camera sensor rotation in degrees (0 to 360) or -1 if it could not be retrieved
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_camera_sensor_rotation(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]

        public static extern byte linphone_call_asked_to_autoanswer(IntPtr call);

/**
 * Get the remote address of the current call.
 * @param[in] lc LinphoneCore object.
 * @return The remote address of the current call or NULL if there is no current call.
 * @ingroup call_control
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_current_call_remote_address(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_remote_address(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_get_dir(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_call_log(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_refer_to(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_call_has_transfer_pending(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_transferer_call(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_transfer_target_call(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_replaced_call(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_get_duration(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_current_params(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_params_copy(IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_remote_params(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_enable_camera(IntPtr lc, bool enabled);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_call_camera_enabled(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_take_video_snapshot(IntPtr call, string file);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_take_preview_snapshot(IntPtr call, string file);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_get_reason(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_error_info(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_video_jittcomp(IntPtr lc, int milliseconds);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_audio_port(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_get_audio_port_range(IntPtr lc, ref int min_port, ref int max_port);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_video_port(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_get_video_port_range(IntPtr lc, ref int min_port, ref int max_port);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_nortp_timeout(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_audio_port(IntPtr lc, int port);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_audio_port_range(IntPtr lc, int min_port, int max_port);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_video_port(IntPtr lc, int port);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_video_port_range(IntPtr lc, int min_port, int max_port);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_nortp_timeout(IntPtr lc, int port);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_use_info_for_dtmf(IntPtr lc, bool use_info);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_get_use_info_for_dtmf(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_use_rfc2833_for_dtmf(IntPtr lc, bool use_rfc2833);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_get_use_rfc2833_for_dtmf(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_sip_port(IntPtr lc, int port);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_sip_port(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_sip_transports(IntPtr lc, IntPtr transports);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_get_sip_transports_used(IntPtr lc, IntPtr tr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_sip_transport_supported(IntPtr lc, LinphoneTransportType tp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_inc_timeout(IntPtr lc, int seconds);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_inc_timeout(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_in_call_timeout(IntPtr lc, int seconds);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_in_call_timeout(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_delayed_timeout(IntPtr lc, int seconds);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_delayed_timeout(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_stun_server(IntPtr lc, string server);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_stun_server(IntPtr lc);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_nat_policy(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_nat_policy(IntPtr lc, IntPtr natPolicy);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_nat_policy_set_stun_server(IntPtr natPolicy, string stunServer);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_nat_policy_enable_stun(IntPtr natPolicy, bool enable);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_nat_policy_enable_turn(IntPtr natPolicy, bool enable);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool linphone_nat_policy_ice_enabled(IntPtr natPolicy);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_nat_policy_set_stun_server_username(IntPtr natPolicy, string username);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern string linphone_nat_policy_get_stun_server(IntPtr natPolicy);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_nat_policy_enable_ice(IntPtr natPolicy, bool enable);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_nat_policy_clear(IntPtr natPolicy);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_avpf_mode(IntPtr lc, LinphoneAVPFMode mode);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_avpf_mode(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_avpf_rr_interval(IntPtr lc, int interval);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_avpf_rr_interval(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_adaptive_rate_control(IntPtr lc, bool enabled);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_adaptive_rate_control_enabled(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_adaptive_rate_algorithm(IntPtr lc, string algorithm);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_adaptive_rate_algorithm(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_ipv6(IntPtr lc, bool enabled);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_ipv6_enabled(IntPtr lc);
        /**
 * Returns the list of available audio codecs.
 * @param[in] lc The LinphoneCore object
 * @return \mslist{PayloadType}
 *
 * This list is unmodifiable. The ->data field of the MSList points a PayloadType
 * structure holding the codec information.
 * It is possible to make copy of the list with ms_list_copy() in order to modify it
 * (such as the order of codecs).

**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_audio_codecs(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_audio_codecs(IntPtr lc, IntPtr codecs);

/**
 * Returns the list of available video codecs.
 * @param[in] lc The LinphoneCore object
 * @return \mslist{PayloadType}
 *
 * This list is unmodifiable. The ->data field of the MSList points a PayloadType
 * structure holding the codec information.
 * It is possible to make copy of the list with ms_list_copy() in order to modify it
 * (such as the order of codecs).

**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_video_codecs(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_set_video_codecs(IntPtr lc, IntPtr codecs);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_generic_confort_noise(IntPtr lc, bool enabled);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_generic_confort_noise_enabled(IntPtr lc);

/**
 * Tells whether the specified payload type is enabled.
 * @param[in] lc #LinphoneCore object.
 * @param[in] pt The #LinphonePayloadType we want to know is enabled or not.
 * @return TRUE if the payload type is enabled, FALSE if disabled.

 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_payload_type_enabled(IntPtr lc, IntPtr pt);

/**
 * Tells whether the specified payload type represents a variable bitrate codec.
 * @param[in] lc #LinphoneCore object.
 * @param[in] pt The #LinphonePayloadType we want to know
 * @return TRUE if the payload type represents a VBR codec, FALSE if disabled.

 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_payload_type_is_vbr(IntPtr lc, IntPtr pt);

/**
 * Set an explicit bitrate (IP bitrate, not codec bitrate) for a given codec, in kbit/s.
 * @param[in] lc the #LinphoneCore object
 * @param[in] pt the #LinphonePayloadType to modify.
 * @param[in] bitrate the IP bitrate in kbit/s.

**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_payload_type_bitrate(IntPtr lc, IntPtr pt, int bitrate);

/**
 * Get the bitrate explicitely set with linphone_core_set_payload_type_bitrate().
 * @param[in] lc the #LinphoneCore object
 * @param[in] pt the #LinphonePayloadType to modify.
 * @return bitrate the IP bitrate in kbit/s, or -1 if an error occurred.

**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_payload_type_bitrate(IntPtr lc, IntPtr pt);

/**
 * Enable or disable the use of the specified payload type.
 * @param[in] lc #LinphoneCore object.
 * @param[in] pt The #LinphonePayloadType to enable or disable. It can be retrieved using #linphone_core_find_payload_type
 * @param[in] enable TRUE to enable the payload type, FALSE to disable it.
 * @return 0 if successful, any other value otherwise.

 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_enable_payload_type(IntPtr lc, IntPtr pt, bool enable);


/**
 * Get payload type from mime type and clock rate

 * This function searches in audio and video codecs for the given payload type name and clockrate.
 * @param lc #LinphoneCore object
 * @param type payload mime type (I.E SPEEX, PCMU, VP8)
 * @param rate can be #LINPHONE_FIND_PAYLOAD_IGNORE_RATE
 * @param channels  number of channels, can be #LINPHONE_FIND_PAYLOAD_IGNORE_CHANNELS
 * @return Returns NULL if not found.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_find_payload_type(IntPtr lc, string type, int rate, int channels);

/**

 * Returns the payload type number assigned for this codec.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_payload_type_number(IntPtr lc, IntPtr pt);

/**
 * Force a number for a payload type. The LinphoneCore does payload type number assignment automatically. THis function is to be used mainly for tests, in order
 * to override the automatic assignment mechanism.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_payload_type_number(IntPtr lc, IntPtr pt, int number);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_payload_type_description(IntPtr lc, IntPtr pt);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_check_payload_type_usability(IntPtr lc, IntPtr pt);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_send_dtmf(IntPtr lc, char dtmf);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_send_dtmfs(IntPtr call, string dtmfs);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_cancel_dtmfs(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_create_address(IntPtr lc, string address);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_proxy_config_set_route(IntPtr proxy, string route);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_proxy_config_set_expires(IntPtr proxy, int expires);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern LinphoneReason linphone_proxy_config_get_error(IntPtr cfg);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_address_set_transport(IntPtr u, int transport);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_address_set_port(IntPtr u, int port);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_address_as_string_uri_only(IntPtr u);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_address_as_string(IntPtr u);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_refresh_registers(IntPtr lc);


        #region Call Info

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_params_get_media_encryption(IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_params_get_privacy(IntPtr cp);

        // https://github.com/BelledonneCommunications/linphone/blob/master/include/linphone/types.h#L782
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_params_set_privacy(IntPtr cp, int privacyMask);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_call_params_get_received_framerate(IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern MSVideoSizeDef linphone_call_params_get_received_video_size(IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_params_get_rtp_profile(IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_call_params_get_sent_framerate(IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern MSVideoSizeDef linphone_call_params_get_sent_video_size(IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_params_get_used_audio_codec(IntPtr cp);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_params_get_used_video_codec(IntPtr cp);


        // Add MITRE-fjr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_factory_get();
         // Add MITRE-fjr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_factory_create_core_cbs(IntPtr LinphoneFactory);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_factory_create_core(IntPtr LinphoneFactory, IntPtr LinphoneCoreCbs, string config_path, string factory_config);

        //  Added MITRE-fjr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_add_listener(IntPtr LinphoneCore, IntPtr LinphoneCoreVTable);
        
        //  Added MITRE-fjr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_add_callbacks(IntPtr LinphoneCore, IntPtr LinphoneCoreCbs);
        #endregion

        #region Call Statistics

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_audio_stats(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_get_video_stats(IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_call_stats_get_sender_loss_rate(IntPtr stats);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_call_stats_get_receiver_loss_rate(IntPtr stats);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_call_stats_get_sender_interarrival_jitter(IntPtr stats, IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_call_stats_get_receiver_interarrival_jitter(IntPtr stats, IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern RtpStats linphone_call_stats_get_rtp_stats(IntPtr stats);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt64 linphone_call_stats_get_late_packets_cumulative_number(IntPtr stats, IntPtr call);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_call_stats_get_download_bandwidth(IntPtr stats);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_call_stats_get_upload_bandwidth(IntPtr stats);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_stats_get_ice_state(IntPtr stats);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_stats_get_upnp_state(IntPtr stats);

        #endregion

        #region RTT

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_params_enable_realtime_text(IntPtr cp, bool yesno);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_call_params_realtime_text_enabled(IntPtr cp); // CJM: RTT


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_chat_database_path(IntPtr lc, string path);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_chat_room(IntPtr lc, IntPtr addr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_chat_room_from_uri(IntPtr lc, string to);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_delete_chat_room(IntPtr lc, IntPtr cr);

/**
 * Unconditionally disable incoming chat messages.
 * @param lc the core
 * @param deny_reason the deny reason (#LinphoneReasonNone has no effect).
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_disable_chat(IntPtr lc, LinphoneReason deny_reason);

/**
 * Enable reception of incoming chat messages.
 * By default it is enabled but it can be disabled with linphone_core_disable_chat().
 * @param lc the core
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_enable_chat(IntPtr lc);

/**
 * Returns whether chat is enabled.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_core_chat_enabled(IntPtr lc);

/**
 * Create a message attached to a dedicated chat room;
 * @param cr the chat room.
 * @param message text message, NULL if absent.
 * @return a new #LinphoneChatMessage
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_room_create_message(IntPtr cr, string message);

/**
 * Create a message attached to a dedicated chat room;
 * @param cr the chat room.
 * @param message text message, NULL if absent.
 * @param external_body_url the URL given in external body or NULL.
 * @param state the LinphoneChatMessage.State of the message.
 * @param time the time_t at which the message has been received/sent.
 * @param is_read TRUE if the message should be flagged as read, FALSE otherwise.
 * @param is_incoming TRUE if the message has been received, FALSE otherwise.
 * @return a new #LinphoneChatMessage
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_room_create_message_2(IntPtr cr, string message,
            string external_body_url, LinphoneChatMessageState state, uint time, bool is_read, bool is_incoming);

/**
 * Acquire a reference to the chat room.
 * @param[in] cr The chat room.
 * @return The same chat room.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_room_ref(IntPtr cr);

/**
 * Release reference to the chat room.
 * @param[in] cr The chat room.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_room_unref(IntPtr cr);

/**
 * Retrieve the user pointer associated with the chat room.
 * @param[in] cr The chat room.
 * @return The user pointer associated with the chat room.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_room_get_user_data(IntPtr cr);

/**
 * Assign a user pointer to the chat room.
 * @param[in] cr The chat room.
 * @param[in] ud The user pointer to associate with the chat room.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_room_set_user_data(IntPtr cr, IntPtr ud);

        /**
 * Create a message attached to a dedicated chat room with a particular content.
 * Use #linphone_chat_room_send_message to initiate the transfer
 * @param cr the chat room.
 * @param initial_content #LinphoneContent initial content. #LinphoneCoreVTable.file_transfer_send is invoked later to notify file transfer progress and collect next chunk of the message if #LinphoneContent.data is NULL.
 * @return a new #LinphoneChatMessage
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_room_create_file_transfer_message(IntPtr cr, IntPtr initial_content);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_room_get_peer_address(IntPtr cr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_room_send_message(IntPtr cr, string msg);

/**
 * Send a message to peer member of this chat room.
 * @param[in] cr LinphoneChatRoom object
 * @param[in] msg LinphoneChatMessage object
 * The state of the message sending will be notified via the callbacks defined in the LinphoneChatMessageCbs object that can be obtained
 * by calling linphone_chat_message_get_callbacks().
 * The LinphoneChatMessage reference is transfered to the function and thus doesn't need to be unref'd by the application.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_room_send_chat_message(IntPtr cr, IntPtr msg);

/**
 * Mark all messages of the conversation as read
 * @param[in] cr The #LinphoneChatRoom object corresponding to the conversation.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_room_mark_as_read(IntPtr cr);

/**
 * Delete a message from the chat room history.
 * @param[in] cr The #LinphoneChatRoom object corresponding to the conversation.
 * @param[in] msg The #LinphoneChatMessage object to remove.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_room_delete_message(IntPtr cr, IntPtr msg);

/**
 * Delete all messages from the history
 * @param[in] cr The #LinphoneChatRoom object corresponding to the conversation.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_room_delete_history(IntPtr cr);

/**
 * Gets the number of messages in a chat room.
 * @param[in] cr The #LinphoneChatRoom object corresponding to the conversation for which size has to be computed
 * @return the number of messages.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_chat_room_get_history_size(IntPtr cr);

/**
 * Gets nb_message most recent messages from cr chat room, sorted from oldest to most recent.
 * @param[in] cr The #LinphoneChatRoom object corresponding to the conversation for which messages should be retrieved
 * @param[in] nb_message Number of message to retrieve. 0 means everything.
 * @return \mslist{LinphoneChatMessage}
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_room_get_history(IntPtr cr, int nb_message);

/**
 * Gets the partial list of messages in the given range, sorted from oldest to most recent.
 * @param[in] cr The #LinphoneChatRoom object corresponding to the conversation for which messages should be retrieved
 * @param[in] begin The first message of the range to be retrieved. History most recent message has index 0.
 * @param[in] end The last message of the range to be retrieved. History oldest message has index of history size - 1 (use #linphone_chat_room_get_history_size to retrieve history size)
 * @return \mslist{LinphoneChatMessage}
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_room_get_history_range(IntPtr cr, int begin, int end);

/**
 * Notifies the destination of the chat message being composed that the user is typing a new message.
 * @param[in] cr The #LinphoneChatRoom object corresponding to the conversation for which a new message is being typed.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_room_compose(IntPtr cr);

/**
 * Tells whether the remote is currently composing a message.
 * @param[in] cr The "LinphoneChatRoom object corresponding to the conversation.
 * @return TRUE if the remote is currently composing a message, FALSE otherwise.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_chat_room_is_remote_composing(IntPtr cr);

/**
 * Gets the number of unread messages in the chatroom.
 * @param[in] cr The "LinphoneChatRoom object corresponding to the conversation.
 * @return the number of unread messages.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_chat_room_get_unread_messages_count(IntPtr cr);

/**
 * Returns back pointer to LinphoneCore object.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_room_get_core(IntPtr cr);

/**
 * When realtime text is enabled #linphone_call_params_realtime_text_enabled, #LinphoneCoreIsComposingReceivedCb is call everytime a char is received from peer.
 * At the end of remote typing a regular #LinphoneChatMessage is received with committed data from #LinphoneCoreMessageReceivedCb.
 * @param[in] msg LinphoneChatMessage
 * @returns  RFC 4103/T.140 char
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint linphone_chat_room_get_char(IntPtr cr);

/**
 * Returns an list of chat rooms
 * @param[in] lc #LinphoneCore object
 * @return \mslist{LinphoneChatRoom}
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_chat_rooms(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint linphone_chat_message_store(IntPtr msg);

/**
 * Returns a #LinphoneChatMessageState as a string.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_state_to_string(LinphoneChatMessageState state);

/**
 * Get the state of the message
 *@param message #LinphoneChatMessage obj
 *@return #LinphoneChatMessageState
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern LinphoneChatMessageState linphone_chat_message_get_state(IntPtr message);

/**
 * Duplicate a LinphoneChatMessage
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_clone(IntPtr message);

/**
 * Acquire a reference to the chat message.
 * @param msg the chat message
 * @return the same chat message
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_ref(IntPtr msg);

/**
 * Release reference to the chat message.
 * @param msg the chat message.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_unref(IntPtr msg);

/**
 * Destroys a LinphoneChatMessage.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_destroy(IntPtr msg);

/**
 * Set origin of the message
 * @param[in] message #LinphoneChatMessage obj
 * @param[in] from #LinphoneAddress origin of this message (copied)
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_set_from_address(IntPtr message, IntPtr addr);

/**
 * Get origin of the message
 * @param[in] message #LinphoneChatMessage obj
 * @return #LinphoneAddress
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_get_from_address(IntPtr message);

/**
 * Set destination of the message
 * @param[in] message #LinphoneChatMessage obj
 * @param[in] to #LinphoneAddress destination of this message (copied)
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_set_to_address(IntPtr message, IntPtr addr);

/**
 * Get destination of the message
 * @param[in] message #LinphoneChatMessage obj
 * @return #LinphoneAddress
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_get_to_address(IntPtr message);

/**
 * Linphone message can carry external body as defined by rfc2017
 * @param message #LinphoneChatMessage
 * @return external body url or NULL if not present.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern string linphone_chat_message_get_external_body_url(IntPtr message);

/**
 * Linphone message can carry external body as defined by rfc2017
 *
 * @param message a LinphoneChatMessage
 * @param url ex: access-type=URL; URL="http://www.foo.com/file"
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_set_external_body_url(IntPtr message, string url);

/**
 * Get the file_transfer_information (used by call backs to recover informations during a rcs file transfer)
 *
 * @param message #LinphoneChatMessage
 * @return a pointer to the LinphoneContent structure or NULL if not present.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_get_file_transfer_information(IntPtr message);

/**
 * Start the download of the file referenced in a LinphoneChatMessage from remote server.
 * @param[in] message LinphoneChatMessage object.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_download_file(IntPtr message);

/**
 * Cancel an ongoing file transfer attached to this message.(upload or download)
 * @param msg	#LinphoneChatMessage
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_cancel_file_transfer(IntPtr msg);

/**
 * Linphone message has an app-specific field that can store a text. The application might want
 * to use it for keeping data over restarts, like thumbnail image path.
 * @param message #LinphoneChatMessage
 * @return the application-specific data or NULL if none has been stored.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern string linphone_chat_message_get_appdata(IntPtr message);

/**
 * Linphone message has an app-specific field that can store a text. The application might want
 * to use it for keeping data over restarts, like thumbnail image path.
 *
 * Invoking this function will attempt to update the message storage to reflect the change if it is
 * enabled.
 *
 * @param message #LinphoneChatMessage
 * @param data the data to store into the message
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_set_appdata(IntPtr message, string data);

/**
 * Get text part of this message
 * @return text or NULL if no text.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_get_text(IntPtr message);

/**
 * Get the time the message was sent.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint linphone_chat_message_get_time(IntPtr message);

/**
 * Returns the chatroom this message belongs to.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_get_chat_room(IntPtr msg);

/**
 * get peer address \link linphone_core_get_chat_room() associated to \endlink this #LinphoneChatRoom
 * @param cr #LinphoneChatRoom object
 * @return #LinphoneAddress peer address
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_get_peer_address(IntPtr msg);

/**
 * Returns the origin address of a message if it was a outgoing message, or the destination address if it was an incoming message.
 *@param message #LinphoneChatMessage obj
 *@return #LinphoneAddress
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_get_local_address(IntPtr message);

/**
 * Add custom headers to the message.
 * @param message the message
 * @param header_name name of the header_name
 * @param header_value header value
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_add_custom_header(IntPtr message, string header_name,
            string header_value);

/**
 * Retrieve a custom header value given its name.
 * @param message the message
 * @param header_name header name searched
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern string linphone_chat_message_get_custom_header(IntPtr message, string header_name);

/**
 * Returns TRUE if the message has been read, otherwise returns FALSE.
 * @param message the message
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_chat_message_is_read(IntPtr message);

/**
 * Returns TRUE if the message has been sent, returns FALSE if the message has been received.
 * @param message the message
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_chat_message_is_outgoing(IntPtr message);

/**
 * Returns the id used to identify this message in the storage database
 * @param message the message
 * @return the id
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint linphone_chat_message_get_storage_id(IntPtr message);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern LinphoneReason linphone_chat_message_get_reason(IntPtr msg);

/**
 * Get full details about delivery error of a chat message.
 * @param msg a LinphoneChatMessage
 * @return a LinphoneErrorInfo describing the details.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_get_error_info(IntPtr msg);

/**
 * Set the path to the file to read from or write to during the file transfer.
 * @param[in] msg LinphoneChatMessage object
 * @param[in] filepath The path to the file to use for the file transfer.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_set_file_transfer_filepath(IntPtr msg, string filepath);

/**
 * Get the path to the file to read from or write to during the file transfer.
 * @param[in] msg LinphoneChatMessage object
 * @return The path to the file to use for the file transfer.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern string linphone_chat_message_get_file_transfer_filepath(IntPtr msg);

/**
 * Fulfill a chat message char by char. Message linked to a Real Time Text Call send char in realtime following RFC 4103/T.140
 * To commit a message, use #linphone_chat_room_send_message
 * @param[in] msg LinphoneChatMessage
 * @param[in] character T.140 char
 * @returns 0 if succeed.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_chat_message_put_char(IntPtr msg, uint charater);

/**
 * get Curent Call associated to this chatroom if any
 * To commit a message, use #linphone_chat_room_send_message
 * @param[in] room LinphoneChatRomm
 * @returns LinphoneCall or NULL.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_room_get_call(IntPtr room);


/**
 * Get the LinphoneChatMessageCbs object associated with the LinphoneChatMessage.
 * @param[in] msg LinphoneChatMessage object
 * @return The LinphoneChatMessageCbs object associated with the LinphoneChatMessage.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_get_callbacks(IntPtr msg);

/**
 * Acquire a reference to the LinphoneChatMessageCbs object.
 * @param[in] cbs LinphoneChatMessageCbs object.
 * @return The same LinphoneChatMessageCbs object.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_cbs_ref(IntPtr cbs);

/**
 * Release reference to the LinphoneChatMessageCbs object.
 * @param[in] cbs LinphoneChatMessageCbs object.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_cbs_unref(IntPtr cbs);

/**
 * Assign a user pointer to the LinphoneChatMessageCbs object.
 * @param[in] cbs LinphoneChatMessageCbs object.
 * @param[in] ud The user pointer to associate with the LinphoneChatMessageCbs object.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_cbs_set_user_data(IntPtr cbs, IntPtr ud);

/**
 * Get the message state changed callback.
 * @param[in] cbs LinphoneChatMessageCbs object.
 * @return The current message state changed callback.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_cbs_get_msg_state_changed(IntPtr cbs);

/**
 * Set the message state changed callback.
 * @param[in] cbs LinphoneChatMessageCbs object.
 * @param[in] cb The message state changed callback to be used.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_cbs_set_msg_state_changed(IntPtr cbs, IntPtr cb);

/**
 * Get the file transfer receive callback.
 * @param[in] cbs LinphoneChatMessageCbs object.
 * @return The current file transfer receive callback.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_cbs_get_file_transfer_recv(IntPtr cbs);

/**
 * Set the file transfer receive callback.
 * @param[in] cbs LinphoneChatMessageCbs object.
 * @param[in] cb The file transfer receive callback to be used.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_cbs_set_file_transfer_recv(IntPtr cbs, IntPtr cb);

/**
 * Get the file transfer send callback.
 * @param[in] cbs LinphoneChatMessageCbs object.
 * @return The current file transfer send callback.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_cbs_get_file_transfer_send(IntPtr cbs);

/**
 * Set the file transfer send callback.
 * @param[in] cbs LinphoneChatMessageCbs object.
 * @param[in] cb The file transfer send callback to be used.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_cbs_set_file_transfer_send(IntPtr cbs, IntPtr cb);

/**
 * Get the file transfer progress indication callback.
 * @param[in] cbs LinphoneChatMessageCbs object.
 * @return The current file transfer progress indication callback.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_chat_message_cbs_get_file_transfer_progress_indication(IntPtr cbs);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_chat_message_cbs_set_file_transfer_progress_indication(IntPtr cbs, IntPtr cb);

        #endregion

        #region Call history

/**
 * Get the list of call logs (past calls).
 * @param[in] lc LinphoneCore object
 * @return \mslist{LinphoneCallLog}
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_call_logs(IntPtr lc);

/**
 * Get the list of call logs (past calls) that matches the given #LinphoneAddress.
 * At the contrary of linphone_core_get_call_logs, it is your responsability to unref the logs and free this list once you are done using it.
 * @param[in] lc LinphoneCore object
 * @return \mslist{LinphoneCallLog}
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_call_history_for_address(IntPtr lc, IntPtr addr);

/**
 * Erase the call log.
 * @param[in] lc LinphoneCore object
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_clear_call_logs(IntPtr lc);

/**
 * Get the number of missed calls.
 * Once checked, this counter can be reset with linphone_core_reset_missed_calls_count().
 * @param[in] lc #LinphoneCore object.
 * @return The number of missed calls.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_core_get_missed_calls_count(IntPtr lc);

/**
 * Reset the counter of missed calls.
 * @param[in] lc #LinphoneCore object.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_reset_missed_calls_count(IntPtr lc);

/**
 * Remove a specific call log from call history list.
 * This function destroys the call log object. It must not be accessed anymore by the application after calling this function.
 * @param[in] lc #LinphoneCore object
 * @param[in] call_log #LinphoneCallLog object to remove.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_remove_call_log(IntPtr lc, IntPtr call_log);

/**
 * Sets the database filename where call logs will be stored.
 * If the file does not exist, it will be created.
 * @ingroup initializing
 * @param lc the linphone core
 * @param path filesystem path
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_call_logs_database_path(IntPtr lc, string path);

/**
 * Migrates the call logs from the linphonerc to the database if not done yet
 * @ingroup initializing
 * @param lc the linphone core
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_migrate_logs_from_rc_to_db(IntPtr lc);


        #endregion

        #region CallLog

        /**
 * Get the call ID used by the call.
 * @param[in] cl LinphoneCallLog object
 * @return The call ID used by the call as a string.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_log_get_call_id(IntPtr cl);

/**
 * Get the direction of the call.
 * @param[in] cl LinphoneCallLog object
 * @return The direction of the call.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern LinphoneCallDir linphone_call_log_get_dir(IntPtr cl);

/**
 * Get the duration of the call since connected.
 * @param[in] cl LinphoneCallLog object
 * @return The duration of the call in seconds.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_call_log_get_duration(IntPtr cl);

/**
 * Get the origin address (ie from) of the call.
 * @param[in] cl LinphoneCallLog object
 * @return The origin address (ie from) of the call.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_log_get_from_address(IntPtr cl);

/**
 * Get the RTP statistics computed locally regarding the call.
 * @param[in] cl LinphoneCallLog object
 * @return The RTP statistics that have been computed locally for the call.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_log_get_local_stats(IntPtr cl);

/**
 * Get the overall quality indication of the call.
 * @param[in] cl LinphoneCallLog object
 * @return The overall quality indication of the call.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float linphone_call_log_get_quality(IntPtr cl);

/**
 * Get the persistent reference key associated to the call log.
 *
 * The reference key can be for example an id to an external database.
 * It is stored in the config file, thus can survive to process exits/restarts.
 *
 * @param[in] cl LinphoneCallLog object
 * @return The reference key string that has been associated to the call log, or NULL if none has been associated.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_log_get_ref_key(IntPtr cl);

/**
 * Get the remote address (that is from or to depending on call direction).
 * @param[in] cl LinphoneCallLog object
 * @return The remote address of the call.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_log_get_remote_address(IntPtr cl);

/**
 * Get the RTP statistics computed by the remote end and sent back via RTCP.
 * @note Not implemented yet.
 * @param[in] cl LinphoneCallLog object
 * @return The RTP statistics that have been computed by the remote end for the call.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_log_get_remote_stats(IntPtr cl);

/**
 * Get the start date of the call.
 * @param[in] cl LinphoneCallLog object
 * @return The date of the beginning of the call.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint linphone_call_log_get_start_date(IntPtr cl);

/**
 * Get the status of the call.
 * @param[in] cl LinphoneCallLog object
 * @return The status of the call.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern LinphoneCallStatus linphone_call_log_get_status(IntPtr cl);

/**
 * Get the destination address (ie to) of the call.
 * @param[in] cl LinphoneCallLog object
 * @return The destination address (ie to) of the call.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_call_log_get_to_address(IntPtr cl);

/**
 * Associate a persistent reference key to the call log.
 *
 * The reference key can be for example an id to an external database.
 * It is stored in the config file, thus can survive to process exits/restarts.
 *
 * @param[in] cl LinphoneCallLog object
 * @param[in] refkey The reference key string to associate to the call log.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_call_log_set_ref_key(IntPtr cl, string refkey);

/**
 * Tell whether video was enabled at the end of the call or not.
 * @param[in] cl LinphoneCallLog object
 * @return A boolean value telling whether video was enabled at the end of the call.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte linphone_call_log_video_enabled(IntPtr cl);

        #endregion

        #region Security

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_zrtp_secrets_file(IntPtr lc, string file);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_zrtp_secrets_file(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_user_certificates_path(IntPtr lc, string path);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_user_certificates_path(IntPtr lc);

        #endregion

        #endregion

        [DllImport("libmsopenh264.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libmsopenh264_init(IntPtr f);

        [DllImport("ortp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ortp_free(IntPtr p);  
    }
}
