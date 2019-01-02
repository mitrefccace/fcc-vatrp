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

namespace VATRP.LinphoneWrapper.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct LinphoneCoreVTable
    {
        public IntPtr global_state_changed; //<Notifies global state changes
        public IntPtr registration_state_changed; // Notifies registration state changes
        public IntPtr call_state_changed; // Notifies call state changes
        public IntPtr notify_presence_received; // Notify received presence events
        public IntPtr notify_presence_received_for_uri_or_tel; //  Added 3/28 MITRE-fjr
        public IntPtr new_subscription_requested; // Notify about pending presence subscription request
        public IntPtr auth_info_requested; // Ask the application some authentication information
        public IntPtr authentication_requested; // Added 3/28 MITRE-fjr
        public IntPtr call_log_updated; // Notifies that call log list has been updated
        public IntPtr message_received; // A message is received, can be text or external body
        public IntPtr message_received_unable_decrypt; //  Added 3/28 MITRE-fjr
        public IntPtr is_composing_received; // An is-composing notification has been received
        public IntPtr dtmf_received; // A dtmf has been received received
        public IntPtr refer_received; // An out of call refer was received
        public IntPtr call_encryption_changed; // Notifies on change in the encryption of call streams
        public IntPtr transfer_state_changed; // Notifies when a transfer is in progress
        public IntPtr buddy_info_updated; // A LinphoneFriend's BuddyInfo has changed
        public IntPtr call_stats_updated; // Notifies on refreshing of call's statistics.
        public IntPtr info_received; // Notifies an incoming informational message received.
        public IntPtr subscription_state_changed; // Notifies subscription state change
        public IntPtr notify_received; // Notifies a an event notification, see linphone_core_subscribe()
        public IntPtr publish_state_changed; // Notifies publish state change (only from #LinphoneEvent api)
        public IntPtr configuring_status; // Notifies configuring status changes
        public IntPtr display_status; // @deprecated Callback that notifies various events with human readable text.
        public IntPtr display_message; // @deprecated Callback to display a message to the user
        public IntPtr display_warning; // @deprecated Callback to display a warning to the user
        public IntPtr display_url; // @deprecated
        public IntPtr show; // @deprecated Notifies the application that it should show up
        public IntPtr text_received; // @deprecated, use #message_received instead <br> A text message has been received
        public IntPtr file_transfer_recv; // @deprecated Callback to store file received attached to a #LinphoneChatMessage 
        public IntPtr file_transfer_send; // @deprecated Callback to collect file chunk to be sent for a #LinphoneChatMessage 
        public IntPtr file_transfer_progress_indication; // @deprecated Callback to indicate file transfer progress 
        public IntPtr network_reachable; // Callback to report IP network status (I.E up/down )
        public IntPtr log_collection_upload_state_changed; // Callback to upload collected logs 
        public IntPtr log_collection_upload_progress_indication; // Callback to indicate log collection upload progress 
        public IntPtr friend_list_created; //  Added 3/28 MITRE-fjr
        public IntPtr friend_list_removed; //  Added 3/28 MITRE-fjr
        public IntPtr user_data; //User data associated with the above callbacks 

    };
}