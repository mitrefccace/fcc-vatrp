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

namespace VATRP.Core.Model
{
	public class Configuration
	{
        public enum ConfSection
        {
            GENERAL,
            MAIN_WINDOW,
            CALL_WINDOW,
            MESSAGE_WINDOW,
            DIALPAD_WINDOW,
            CONTACT_WINDOW,
            DOCK_WINDOW,
            HISTORY_WINDOW,
            SELF_WINDOW,
            SETTINGS_WINDOW,
            MENUBAR,
            LINPHONE,
            ACCOUNT,
            REMOTE_VIDEO_VIEW,
            KEYPAD_WINDOW,
            CALLINFO_WINDOW,
            AUTHENTICATION_WINDOW
        }

        public enum ConfEntry
        {
            WINDOW_STATE, WINDOW_LEFT, WINDOW_TOP, WINDOW_WIDTH, WINDOW_HEIGHT,
            ACTIVE_WINDOW,
            PASSWD,
            LOGIN,
            SERVER_ADDRESS,
            SERVER_PORT,
            USERNAME,
            DISPLAYNAME,
            LINPHONE_USERAGENT,
            VATRP_VERSION,
            REQUEST_LINK,
            ACCOUNT_IN_USE,
            AUTO_ANSWER,
            AUTO_ANSWER_AFTER,
            AUTO_LOGIN,
            AVPF_ON,
            RTCP_FEEDBACK,
            DTMF_SIP_INFO,
            USE_RTT,
            ENABLE_ADAPTIVE_RATE_CTRL,
            CURRENT_PROVIDER,
            CALL_DIAL_PREFIX,
            CALL_DIAL_ESCAPE_PLUS,
            SHOW_LEGAL_RELEASE,
            LAST_MISSED_CALL_DATE,
            TEXT_SEND_MODE,
            DTMF_INBAND,
            PRIVACY_ENABLED
        }

        public static string LINPHONE_SIP_SERVER = string.Empty;
		public static ushort LINPHONE_SIP_PORT = 5060;
		public static string DISPLAY_NAME = "John Doe";
		public static string LINPHONE_USERAGENT = "VATRP";
        public static string VATRP_VERSION = "1.2.0";
        public static string DEFAULT_REQUEST = string.Empty;
	}
}
