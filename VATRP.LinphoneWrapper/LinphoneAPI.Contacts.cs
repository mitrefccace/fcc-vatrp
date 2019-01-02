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
 * Create a new empty LinphoneFriendList object.
 * @param[in] lc LinphoneCore object.
 * @return A new LinphoneFriendList object.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_create_friend_list(IntPtr lc);

        /**
 * Removes a friend list.
 * @param[in] lc LinphoneCore object
 * @param[in] list LinphoneFriendList object
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_remove_friend_list(IntPtr lc, IntPtr list);

        /**
 * Add a friend list.
 * @param[in] lc LinphoneCore object
 * @param[in] list LinphoneFriendList object
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_add_friend_list(IntPtr lc, IntPtr list);



        /**
 * Get the URI associated with the friend list.
 * @param[in] list LinphoneFriendList object.
 * @return The URI associated with the friend list.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_list_get_uri(IntPtr list);

/**
 * Set the URI associated with the friend list.
 * @param[in] list LinphoneFriendList object.
 * @param[in] rls_uri The URI to associate with the friend list.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_friend_list_set_uri(IntPtr list, string uri);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_list_get_callbacks(IntPtr list);

        /**
 * Returns the vCard object associated to this friend, if any
 * @param[in] fr LinphoneFriend object
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_get_vcard(IntPtr fr);

        /**
         * Binds a vCard object to a friend
         * @param[in] fr LinphoneFriend object
         * @param[in] vcard The vCard object to bind
         */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_friend_set_vcard(IntPtr fr, IntPtr vcard);

        /**
         * Creates a vCard object associated to this friend if there isn't one yet and if the full name is available, either by the parameter or the one in the friend's SIP URI
         * @param[in] fr LinphoneFriend object
         * @param[in] name The full name of the friend or NULL to use the one from the friend's SIP URI
         * @return true if the vCard has been created, false if it wasn't possible (for exemple if name and the friend's SIP URI are null or if the friend's SIP URI doesn't have a display name), or if there is already one vcard
         */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_friend_create_vcard(IntPtr fr, string name);

        /**
         * Contructor same as linphone_friend_new() + linphone_friend_set_address()
         * @param vcard a vCard object
         * @return a new #LinphoneFriend with \link linphone_friend_get_vcard() vCard initialized \endlink
         */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_new_from_vcard(IntPtr vcard);

        /**
         * Creates and adds LinphoneFriend objects to LinphoneCore from a file that contains the vCard(s) to parse
         * @param[in] lc the LinphoneCore object
         * @param[in] vcard_file the path to a file that contains the vCard(s) to parse
         * @return the amount of linphone friends created
         */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_friend_list_import_friends_from_vcard4_file(IntPtr friendsList, string vcard_file);


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_friend_list_export_friends_as_vcard4_file(IntPtr friendsList,
            string vcard_file);

        /**
         * Sets the database filename where friends will be stored.
         * If the file does not exist, it will be created.
         * @ingroup initializing
         * @param lc the linphone core
         * @param path filesystem path
        **/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_set_friends_database_path(IntPtr lc, string path);

        /**
         * Migrates the friends from the linphonerc to the database if not done yet
         * @ingroup initializing
         * @param lc the linphone core
        **/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_migrate_friends_from_rc_to_db(IntPtr lc);

        /**
 * Set the display name for this friend
 * @param lf #LinphoneFriend object
 * @param name 
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_friend_set_name(IntPtr lf, string name);

        /**
 * Create a default LinphoneFriend.
 * @param[in] lc #LinphoneCore object
 * @return The created #LinphoneFriend object
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_create_friend(IntPtr lc);

        /**
         * Create a LinphoneFriend from the given address.
         * @param[in] lc #LinphoneCore object
         * @param[in] address A string containing the address to create the LinphoneFriend from
         * @return The created #LinphoneFriend object
         */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_create_friend_with_address(IntPtr lc, string address);

        /**
 * Add a friend to the current buddy list, if \link linphone_friend_enable_subscribes() subscription attribute \endlink is set, a SIP SUBSCRIBE message is sent.
 * @param lc #LinphoneCore object
 * @param fr #LinphoneFriend to add
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_add_friend(IntPtr lc, IntPtr fr);

        /**
         * remove a friend from the buddy list
         * @param lc #LinphoneCore object
         * @param fr #LinphoneFriend to add
         */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_core_remove_friend(IntPtr lc, IntPtr fr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_default_friend_list(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_friend_list(IntPtr lc);

        //  Added MITRE-fjr 4/27
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_core_get_friends_lists(IntPtr lc);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_get_address(IntPtr lf);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_set_address(IntPtr fr, IntPtr address);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_new_with_address(string addr);

        // Added MITRE-fjr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_list_get_core(IntPtr c) ;

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_friend_enable_subscribes(IntPtr fr, bool val);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_friend_set_inc_subscribe_policy(IntPtr fr, int val);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_friend_edit(IntPtr fr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_friend_done(IntPtr fr);
        
        //  Added 5/5/2017 MITRE-fjr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_friend_unref(IntPtr fr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_list_cbs_ref(IntPtr cbs);

/**
 * Release a reference to a LinphoneFriendListCbs object.
 * @param[in] cbs LinphoneFriendListCbs object.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_friend_list_cbs_unref(IntPtr cbs);

/**
 * Retrieve the user pointer associated with a LinphoneFriendListCbs object.
 * @param[in] cbs LinphoneFriendListCbs object.
 * @return The user pointer associated with the LinphoneFriendListCbs object.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_list_cbs_get_user_data(IntPtr cbs);

/**
 * Assign a user pointer to a LinphoneFriendListCbs object.
 * @param[in] cbs LinphoneFriendListCbs object.
 * @param[in] ud The user pointer to associate with the LinphoneFriendListCbs object.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_friend_list_cbs_set_user_data(IntPtr cbs, IntPtr ud);

/**
 * Get the contact created callback.
 * @param[in] cbs LinphoneFriendListCbs object.
 * @return The current contact created callback.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_list_cbs_get_contact_created(IntPtr cbs);

/**
 * Set the contact created callback.
 * @param[in] cbs LinphoneFriendListCbs object.
 * @param[in] cb The contact created to be used.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_friend_list_cbs_set_contact_created(IntPtr cbs, IntPtr cb);

/**
 * Get the contact deleted callback.
 * @param[in] cbs LinphoneFriendListCbs object.
 * @return The current contact deleted callback.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_list_cbs_get_contact_deleted(IntPtr cbs);

/**
 * Set the contact deleted callback.
 * @param[in] cbs LinphoneFriendListCbs object.
 * @param[in] cb The contact deleted to be used.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_friend_list_cbs_set_contact_deleted(IntPtr cbs, IntPtr cb);

/**
 * Get the contact updated callback.
 * @param[in] cbs LinphoneFriendListCbs object.
 * @return The current contact updated callback.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_list_cbs_get_contact_updated(IntPtr cbs);

/**
 * Set the contact updated callback.
 * @param[in] cbs LinphoneFriendListCbs object.
 * @param[in] cb The contact updated to be used.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_friend_list_cbs_set_contact_updated(IntPtr cbs, IntPtr cb);

/**
 * Get the sync status changed callback.
 * @param[in] cbs LinphoneFriendListCbs object.
 * @return The current sync status changedcallback.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_list_cbs_get_sync_status_changed(IntPtr cbs);

/**
 * Set the contact updated callback.
 * @param[in] cbs LinphoneFriendListCbs object.
 * @param[in] cb The sync status changed to be used.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_friend_list_cbs_set_sync_status_changed(IntPtr cbs, IntPtr cb);

/**
 * Starts a CardDAV synchronization using value set using linphone_friend_list_set_uri.
 * @param[in] list LinphoneFriendList object.
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_friend_list_synchronize_friends_from_server(IntPtr list);

        /**
 * Add a friend to a friend list. If or when a remote CardDAV server will be attached to the list, the friend will be sent to the server.
 * @param[in] list LinphoneFriendList object.
 * @param[in] friend LinphoneFriend object to add to the friend list.
 * @return LinphoneFriendListOK if successfully added, LinphoneFriendListInvalidFriend if the friend is not valid.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_friend_list_add_friend(IntPtr list, IntPtr lf);

/**
 * Add a friend to a friend list. The friend will never be sent to a remote CardDAV server.
 * Warning! LinphoneFriends added this way will be removed on the next synchronization, and the callback contact_deleted will be called.
 * @param[in] list LinphoneFriendList object.
 * @param[in] friend LinphoneFriend object to add to the friend list.
 * @return LinphoneFriendListOK if successfully added, LinphoneFriendListInvalidFriend if the friend is not valid.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int linphone_friend_list_add_local_friend(IntPtr list, IntPtr lf);

/**
 * Remove a friend from a friend list.
 * @param[in] list LinphoneFriendList object.
 * @param[in] friend LinphoneFriend object to remove from the friend list.
 * @return LinphoneFriendListOK if removed successfully, LinphoneFriendListNonExistentFriend if the friend is not in the list.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern LinphoneFriendListStatus linphone_friend_list_remove_friend(IntPtr list, IntPtr afriend);


        //  Added 4/11/2017 MITRE-fjr
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_friend_list_update_dirty_friends(IntPtr LinphoneFriendList);
/**
 * Retrieves the list of LinphoneFriend from this LinphoneFriendList.
 * @param[in] list LinphoneFriendList object
 * @return \mslist{LinphoneFriend} a list of LinphoneFriend
 */

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_list_get_friends(IntPtr list);

/**
 * Find a friend in the friend list using a LinphoneAddress.
 * @param[in] list LinphoneFriendList object.
 * @param[in] address LinphoneAddress object of the friend we want to search for.
 * @return A LinphoneFriend if found, NULL otherwise.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_list_find_friend_by_address(IntPtr list, IntPtr address);

/**
 * Find a friend in the friend list using an URI string.
 * @param[in] list LinphoneFriendList object.
 * @param[in] uri A string containing the URI of the friend we want to search for.
 * @return A LinphoneFriend if found, NULL otherwise.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_list_find_friend_by_uri(IntPtr list, string uri);

/**
 * Find a frient in the friend list using a ref key.
 * @param[in] list LinphoneFriendList object.
 * @param[in] ref_key The ref key string of the friend we want to search for.
 * @return A LinphoneFriend if found, NULL otherwise.
**/

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_list_find_friend_by_ref_key(IntPtr list, string ref_key);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_friend_list_close_subscriptions(IntPtr list);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_vcard_get_uid(IntPtr vCard);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_friend_set_ref_key(IntPtr lf, string key);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void linphone_friend_save(IntPtr LinphoneFriend, IntPtr LinphoneCore);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr linphone_friend_get_ref_key(IntPtr lf);
    }
}
