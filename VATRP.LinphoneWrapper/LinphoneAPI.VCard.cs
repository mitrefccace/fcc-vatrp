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
        /**
 * Creates a LinphoneVCard object that has a pointer to an empty vCard
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_vcard_new();

        /**
         * Deletes a LinphoneVCard object properly
         * @param[in] vCard the LinphoneVCard to destroy
         */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_vcard_free(IntPtr vCard);

        /**
         * Uses belcard to parse the content of a file and returns all the vcards it contains as LinphoneVCards, or NULL if it contains none.
         * @param[in] file the path to the file to parse
         * @return \mslist{LinphoneVCard}
         */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_vcard_list_from_vcard4_file(string file);

       
        /**
         * Uses belcard to parse the content of a buffer and returns all the vcards it contains as LinphoneVCards, or NULL if it contains none.
         * @param[in] buffer the buffer to parse
         * @return \mslist{LinphoneVCard}
         */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_vcard_list_from_vcard4_buffer(string buffer);

        /**
         * Uses belcard to parse the content of a buffer and returns one vCard if possible, or NULL otherwise.
         * @param[in] buffer the buffer to parse
         * @return a LinphoneVCard if one could be parsed, or NULL otherwise
         */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_vcard_new_from_vcard4_buffer(string buffer);

        /**
         * Returns the vCard4 representation of the LinphoneVCard.
         * @param[in] vCard the LinphoneVCard
         * @return a string that represents the vCard
         */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_vcard_as_vcard4_string(IntPtr vCard);

        /**
         * Sets the FN attribute of the vCard (which is mandatory).
         * @param[in] vCard the LinphoneVCard
         * @param[in] name the display name to set for the vCard
         */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_vcard_set_full_name(IntPtr vCard, string name);

        /**
         * Returns the FN attribute of the vCard, or NULL if it isn't set yet.
         * @param[in] vCard the LinphoneVCard
         * @return the display name of the vCard, or NULL
         */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_vcard_get_full_name(IntPtr vCard);

        /**
         * Adds a SIP address in the vCard, using the IMPP property
         * @param[in] vCard the LinphoneVCard
         * @param[in] sip_address the SIP address to add
         */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_vcard_add_sip_address(IntPtr vCard, string sip_address);

        /**
         * Removes a SIP address in the vCard (if it exists), using the IMPP property
         * @param[in] vCard the LinphoneVCard
         * @param[in] sip_address the SIP address to remove
         */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_vcard_remove_sip_address(IntPtr vCard, string sip_address);

        /**
         * Edits the preferred SIP address in the vCard (or the first one), using the IMPP property
         * @param[in] vCard the LinphoneVCard
         * @param[in] sip_address the new SIP address
         */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_vcard_edit_main_sip_address(IntPtr vCard, string sip_address);

        /**
         * Returns the list of SIP addresses (as string) in the vCard (all the IMPP attributes that has an URI value starting by "sip:") or NULL
         * @param[in] vCard the LinphoneVCard
         * @return \mslist{string}
         */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_vcard_get_sip_addresses(IntPtr vCard);

        // Added MITRE-fjr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_get_name(IntPtr LinphoneFriend);
        
        // Added MITRE-fjr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_get_addresses(IntPtr LinphoneFriend);
        

        // Added MITRE-fjr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_list_get_display_name(IntPtr LinphoneFriendList);

        /**
         * Generates a random unique id for the vCard.
         * If is required to be able to synchronize the vCard with a CardDAV server
         * @param[in] vCard the LinphoneVCard
         * @return TRUE if operation is successful, otherwise FALSE (for example if it already has an unique ID)
         */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_vcard_generate_unique_id(IntPtr vCard);
    }
}
