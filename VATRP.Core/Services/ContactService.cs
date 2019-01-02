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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using VATRP.Core.Enums;
using VATRP.Core.Events;
using VATRP.Core.Extensions;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using VATRP.LinphoneWrapper;
using VATRP.LinphoneWrapper.Structs;


using System.Text;
using System.Security.Cryptography;
using System.Linq;

using System.Xml;
using System.Xml.Schema;

using System.Windows;
using log4net;
using VATRP.LinphoneWrapper.Enums;
using System.Threading.Tasks;


namespace VATRP.Core.Services
{
    public sealed class ContactService : IContactsService
    {
        private static readonly log4net.ILog _log = LogManager.GetLogger("APP");

       

        private readonly ServiceManagerBase _manager;
        private ObservableCollection<VATRPContact> _contacts;
        private ObservableCollection<VATRPUserGroup> _groups;

        public event EventHandler<ContactEventArgs> ContactAdded;
        public event EventHandler<ContactRemovedEventArgs> ContactRemoved;
        public event EventHandler<ContactStatusChangedEventArgs> ContactStatusChanged;
        public event EventHandler<EventArgs> GroupsChanged;
        public event EventHandler<ContactEventArgs> ContactsChanged;
        public event EventHandler<EventArgs> ContactsLoadCompleted;
        public event EventHandler<ContactEventArgs> LoggedInContactUpdated;
        public bool IsLoaded { get; private set; }

        private bool editing { get; set; } // use as a lock
        public ContactService(ServiceManagerBase manager)
        {
            this._manager = manager;
            IsLoaded = false;
        }

        public bool IsEditing()
        {
            return editing;
        }

        private void RemoveGroupFromContactList(string _groupName)
        {
            lock (Contacts)
            {
                foreach (var contact in Contacts)
                {
                    if (contact.IsGroupExistInGroupList(_groupName))
                        contact.RemoveGroupFromGroupList(_groupName);
                }
            }
        }

        private void RemoveGroup(int removed)
        {
            lock (this.Groups)
            {
                foreach (var gr in Groups)
                {
                    if (gr.ID == removed)
                    {
                        Groups.Remove(gr);
                        return;
                    }
                }
            }
        }

        private VATRPUserGroup FindGroups(int removed)
        {
            lock (this.Groups)
            {
                foreach (var gr in Groups)
                {
                    if (gr.ID == removed)
                    {
                        return gr;
                    }
                }
            }
            return null;
        }

        private void UpdateDataPath()
        {
            string contactsPath = _manager.BuildDataPath("contacts.db");
            try
            {
                LinphoneAPI.linphone_core_set_friends_database_path(_manager.LinphoneService.LinphoneCore, contactsPath);
            }
            catch (Exception ex)
            {
                // TODO
            }
        }

        private void LoadLinphoneContacts()
        {

            //************************************************************************************************************************************
            // Loading and Parsing the Linphone contacts.
            //************************************************************************************************************************************
            if (_manager.LinphoneService.LinphoneCore == IntPtr.Zero)
                return;

            IntPtr contactsPtr = LinphoneAPI.linphone_core_get_friend_list(_manager.LinphoneService.LinphoneCore);
            if (contactsPtr != IntPtr.Zero)
            {
                MSList curStruct;

                do
                {
                    curStruct.next = IntPtr.Zero;
                    curStruct.prev = IntPtr.Zero;
                    curStruct.data = IntPtr.Zero;

                    curStruct = (MSList)Marshal.PtrToStructure(contactsPtr, typeof(MSList));
                    if (curStruct.data != IntPtr.Zero)
                    {
                        IntPtr addressPtr = LinphoneAPI.linphone_friend_get_address(curStruct.data);
                        if (addressPtr == IntPtr.Zero)
                            continue;
                            //return;

                        string dn = "";
                        IntPtr tmpPtr = LinphoneAPI.linphone_address_get_display_name(addressPtr);
                        if (tmpPtr != IntPtr.Zero)
                        {
                            dn = Marshal.PtrToStringAnsi(tmpPtr);
                        }

                        string un = "";
                        tmpPtr = LinphoneAPI.linphone_address_get_username(addressPtr);
                        if (tmpPtr != IntPtr.Zero)
                        {
                            un = Marshal.PtrToStringAnsi(tmpPtr);
                        }

                        string host = "";
                        tmpPtr = LinphoneAPI.linphone_address_get_domain(addressPtr);
                        if (tmpPtr != IntPtr.Zero)
                        {
                            host = Marshal.PtrToStringAnsi(tmpPtr);
                        }

                        string refKey = "";
                        tmpPtr = LinphoneAPI.linphone_friend_get_ref_key(curStruct.data);
                        if (tmpPtr != IntPtr.Zero)
                            refKey = Marshal.PtrToStringAnsi(tmpPtr);

                        if (string.IsNullOrEmpty(refKey))
                        {
                            // generate refkey
                            refKey = Guid.NewGuid().ToString();
                            LinphoneAPI.linphone_friend_set_ref_key(curStruct.data, refKey);
                        }

                        int port = LinphoneAPI.linphone_address_get_port(addressPtr);

                        if (!string.IsNullOrWhiteSpace(un))
                        {
                            Debug.WriteLine(string.Format("Contacts count: {0}",Contacts.Count));

                            var cfgSipaddress = port == 0 ? string.Format("{0}@{1}", un, host):
                                string.Format("{0}@{1}:{2}", un, host, port);
                            VATRPContact contact = new VATRPContact(new ContactID(cfgSipaddress, IntPtr.Zero))
                            {
                                DisplayName = dn,
                                Fullname = dn.NotBlank() ? dn : un,
                                Gender = "male",
                                SipUsername = un,
                                RegistrationName = cfgSipaddress,
                                IsLinphoneContact = true,
                                LinphoneRefKey = refKey
                            };

                            //int _count = Contacts.Where(item => item.Fullname == contact.Fullname).Count();

                            //if (_count > 1)
                            //{
                            //    //  Find Last and Delete??
                            //    var _contact = (from c in Contacts 
                            //                    where c.LinphoneRefKey == contact.LinphoneRefKey
                            //                    orderby c.ID descending
                            //                    select c).FirstOrDefault();
                            //    if (_contact != null)
                            //    {
                            //        DeleteLinphoneContact(_contact);
                            //    }
                      
                            //}
                            
                            UpdateAvatar(contact);
                            Contacts.Add(contact); // cjm-sep17 -- this is the problem with having too many in the contacts list within ACE
                            Debug.WriteLine(string.Format("Contacts count: {0}",Contacts.Count));

                        }
                    }
                    contactsPtr = curStruct.next;
                } while (curStruct.next != IntPtr.Zero);
                
                //  Clean Up Duplicates
                //var _contacts = (from c in Contacts
                //                    where c.LinphoneRefKey.Trim().Length > 0
                //                    group c by c.Fullname into grp
                //                    where grp.Count() > 1
                //                 select grp).ToList();

                //var _contacts = Contacts.Where(con => con.LinphoneRefKey.Trim().Length > 0 && con.)
                //var _contacts = (from grp in Contacts.GroupBy(c => c.RegistrationName)
                //                .Where(grp => grp.Count() > 1)
                //                 select grp).ToList();

                var _contacts = (from r in Contacts
                                      where (r.LinphoneRefKey.Trim().Length > 0)
                                 select r).GroupBy(x => x.RegistrationName).Where(x => x.Count() > 1).ToList();

                //for (int i = 0; i < _contacts.Count(); i++)
                //{
                    //string _keyToClean = _contacts[i].Key;

                    //var _contact = (from c in Contacts 
                    //                where c.Fullname == _keyToClean
                    //                orderby c.ID descending
                    //                select c).LastOrDefault();

                    //if (_contact != null)
                    //{
                    //    DeleteLinphoneContact(_contact);
                    //}
                //}
                
            }

            LoadContactsOptions();

            if ( ContactsLoadCompleted != null)
                ContactsLoadCompleted(this, EventArgs.Empty);
        }

        private void LoadContactsOptions()
        {
            //****************************************************************************************************
            // Load the contacts from local database contacts.db
            // Database path is C:\Users\User12\AppData\Roaming\VATRP\acetest@sip.linphone.org\contacts.db
            //****************************************************************************************************

            if (editing)
                return;
            editing = true;
            try
            {
                var connectionString = string.Format("data source={0}", _manager.LinphoneService.ContactsDbPath);// + ";Version=3;Password=1234;");
                using (var dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    lock (this.Contacts)
                    {
                        foreach (VATRPContact contact in this.Contacts)
                        {
                            if (contact.IsLoggedIn) //If loggedin contact name and database contact name is same then continue
                                continue;

                            //  ToDo: Test
                            //  Display name was NOT consistent, changed the where clause to ref key, sets the id correctly now
                            //  Update 4/11/2017 MITRE-fjr
                            using (var cmd = new SQLiteCommand(dbConnection) { CommandText = @"SELECT id FROM friends WHERE trim(ref_key) = ?" })
                            {
                                var refKey = string.Format(@"{0}", contact.LinphoneRefKey);
                                var uriParam = new SQLiteParameter(DbType.AnsiString) { Value = refKey };
                                cmd.Parameters.Add(uriParam);
                                var dbReader = cmd.ExecuteReader();
                                if (dbReader.Read())
                                {
                                    contact.DbID = dbReader.GetInt32(0);
                                }
                            }

                            //  Old Code 4/11/2017 MITRE-fjr
                            //using (var cmd = new SQLiteCommand(dbConnection) { CommandText = @"SELECT id FROM friends WHERE sip_uri = ?" })
                            //{
                            //    var sipUri = string.Format(@"""{0}"" <sip:{1}>", contact.DisplayName, contact.ID);
                            //    var uriParam = new SQLiteParameter(DbType.AnsiString) {Value = sipUri};
                            //    cmd.Parameters.Add(uriParam);
                            //    var dbReader = cmd.ExecuteReader();
                            //    if (dbReader.Read())
                            //    {
                            //        contact.DbID = dbReader.GetInt32(0);
                            //    }
                            //}

                            // update contact 

                            using (var cmd = new SQLiteCommand(dbConnection) { CommandText = @"SELECT is_favorite FROM friend_options WHERE id = ?" })
                            {
                                var idParam = new SQLiteParameter(DbType.Int32);
                                idParam.Value = contact.DbID;
                                cmd.Parameters.Add(idParam);
                                var dbReader = cmd.ExecuteReader();
                                if (dbReader.Read())
                                {
                                    contact.IsFavorite = dbReader.GetBoolean(0);
                                }
                            }
                        }
                    }
                    dbConnection.Close();
                }
            }
            catch (SQLiteException ex)
            {
                Debug.WriteLine("Sqlite error: " + ex.ToString());
            }
            finally
            {
                editing = false;
            }
        }

        public void CleanUpDuplicates()
        {
            // cjm-sep17 -- method to remove duplicates form the Db directly 
            var connectionString = string.Format("data source={0}", _manager.LinphoneService.ContactsDbPath);
            using (var dbConnection = new SQLiteConnection(connectionString))
            {
                dbConnection.Open();
                using (var cmd = new SQLiteCommand(dbConnection) { CommandText = @"DELETE FROM friends WHERE rowid NOT IN
                                                                                 (SELECT min(rowid) FROM friends GROUP BY sip_uri);" })
                {
                    cmd.ExecuteNonQuery();
                }
                dbConnection.Close();
            }
        }

        private void RemoveFavoriteOption(VATRPContact contact)
        {
            //****************************************************************************************************
            // Remove the Contact from Favorite in Local Database. This method called when Favorite contact is deleted from contact list. Then it first remove the Favorite 
            // from database. Database path is C:\Users\User12\AppData\Roaming\VATRP\acetest@sip.linphone.org\contacts.db
            //****************************************************************************************************
           // AddDBPassword2();

            if (contact.DbID == 0)
                return;
            if (editing)
                return;
            editing = true;
            try
            {
                var connectionString = string.Format("data source={0}", _manager.LinphoneService.ContactsDbPath);// + ";Version=3;Password=1234;");
                using (var dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    using (
                        var cmd = new SQLiteCommand(dbConnection)
                        {
                            CommandText = @"DELETE FROM friend_options WHERE id = ?"
                        })
                    {
                        var idParam = new SQLiteParameter(DbType.Int32) {Value = contact.DbID};
                        cmd.Parameters.Add(idParam);
                        cmd.ExecuteNonQuery();
                    }
                    dbConnection.Close();
                }
            }
            catch (SQLiteException ex)
            {
                Debug.WriteLine("Sqlite error: " + ex.ToString());
            }
            finally
            {
                editing = false;
            }
        }

        public void UpdateFavoriteOption(VATRPContact contact)
        {
            //*************************************************************************************************************************************************
            // When contact added to Favorite or Removed from Favorite.
            //*************************************************************************************************************************************************
            if (contact.DbID == 0)
                return;
            if (editing)
                return;
            editing = true;
            try
            {
                var connectionString = string.Format("data source={0}", _manager.LinphoneService.ContactsDbPath);// + ";Version=3;Password=1234;");
                using (var dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();

                    using (
                        var cmd = new SQLiteCommand(dbConnection)
                        {
                            CommandText = @"INSERT OR REPLACE INTO friend_options (id, is_favorite) VALUES ( ?, ?)"
                        })
                    {
                        var idParam = new SQLiteParameter(DbType.Int32) {Value = contact.DbID};
                        cmd.Parameters.Add(idParam);

                        var favParam = new SQLiteParameter(DbType.Int32) {Value = contact.IsFavorite ? 1 : 0};
                        cmd.Parameters.Add(favParam);

                        cmd.ExecuteNonQuery();
                    }
                    dbConnection.Close();
                }
            }
            catch (SQLiteException ex)
            {
                Debug.WriteLine("Sqlite error: " + ex.ToString());
            }
            finally
            {
                editing = false;
            }
        }

        #region VCARD

        public int ImportVcardFromXdoc(XmlDocument xDoc)
        {     
            if (_manager.LinphoneService.LinphoneCore == IntPtr.Zero)
            {
                return 0;
            }
            int returnCount = 0;
            foreach (XmlNode node in xDoc.DocumentElement.ChildNodes)
            {
                string fn = null, ln = null, address = null;

                // first node is the url ... have to go to nexted loc node 
                foreach (XmlNode locNode in node)
                {
                    // thereare a couple child nodes here so only take data from node named loc 
                    if (locNode.Name == "fn")
                    {
                        fn = locNode.InnerText;
                    }
                    else if (locNode.Name == "n")
                    {
                        foreach (XmlNode locNode1 in locNode)
                        {
                            if (locNode1.Name == "surname")
                            {
                                ln = locNode1.InnerText;
                            }
                        }
                    }
                    else if (locNode.Name == "email")
                    {
                        foreach (XmlNode locNode1 in locNode)
                        {
                            if (locNode1.Name == "text")
                            {
                                address = locNode1.InnerText;
                            }
                        }
                    }
                }
                string tempVcardPath = createVcard(fn, ln, address);
                returnCount = returnCount + importXcardVcard(tempVcardPath);
                File.Delete(tempVcardPath);
            }
            CleanUpDuplicates();
            UpdateDataPath();
            LoadLinphoneContacts();
            return returnCount;
        }
    
        public int ImportVCards(string vcardPath)
        {

            //System.Windows.Forms.MessageBox.Show("XML Path" + vcardPath);
            //**************************************************************************************************************
            // Import Vcard contact in contact list.
            //**************************************************************************************************************

            int returnCount = 0;
            if (_manager.LinphoneService.LinphoneCore == IntPtr.Zero)
                return 0;
          //  System.Windows.Forms.MessageBox.Show("XML Path 1" + vcardPath);

            if (isXCard(vcardPath)){
                //System.Windows.Forms.MessageBox.Show("Valid XML Path" + vcardPath);
                XmlDocument xDoc = new XmlDocument();
                try
                {
                    xDoc.Load(vcardPath);
                }
                catch (Exception ex)
                {
                    //System.Windows.Forms.MessageBox.Show(ex.InnerException.ToString());
                    throw;
                }
                //load up the xml from the location 
               

                // cycle through each child noed 
                foreach (XmlNode node in xDoc.DocumentElement.ChildNodes)
                {
                    string fn=null, ln=null, address=null;

                    // first node is the url ... have to go to nexted loc node 
                    foreach (XmlNode locNode in node)
                    {
                        // thereare a couple child nodes here so only take data from node named loc 
                        if (locNode.Name == "fn")
                        {
                            fn = locNode.InnerText;
                        }
                        else if (locNode.Name == "n")
                        {
                            foreach (XmlNode locNode1 in locNode)
                            {
                                if (locNode1.Name == "surname")
                                {
                                    ln = locNode1.InnerText;
                                }
                            }
                        }
                        else if (locNode.Name == "email")
                        {
                            foreach (XmlNode locNode1 in locNode)
                            {
                                if (locNode1.Name == "text")
                                {
                                    address = locNode1.InnerText;
                                }
                            }
                        }
                    }

                   // System.Windows.Forms.MessageBox.Show("Card: " + fn + " " + ln + " " + address);

                   string tempVcardPath= createVcard(fn, ln, address);
                   returnCount = returnCount+importXcardVcard(tempVcardPath);
                   File.Delete(tempVcardPath);
                }
                CleanUpDuplicates();
                UpdateDataPath();
                LoadLinphoneContacts();
                return returnCount;
            }
            else
            {
                return importXcardVcard(vcardPath);
            }

            return 0;

            //IntPtr vcardsList = LinphoneAPI.linphone_vcard_list_from_vcard4_file(vcardPath);

            //if (vcardsList != IntPtr.Zero)
            //{
            //    MSList curStruct;
            //    do
            //    {
            //        curStruct.next = IntPtr.Zero;
            //        curStruct.prev = IntPtr.Zero;
            //        curStruct.data = IntPtr.Zero;

            //        curStruct = (MSList)Marshal.PtrToStructure(vcardsList, typeof(MSList));
            //        if (curStruct.data != IntPtr.Zero)
            //        {
            //            string fullname = string.Empty;
            //            IntPtr tmpPtr = LinphoneAPI.linphone_vcard_get_full_name(curStruct.data);
            //            if (tmpPtr != IntPtr.Zero)
            //            {
            //                fullname = Marshal.PtrToStringAnsi(tmpPtr);
            //            }

            //            IntPtr addressListPtr = LinphoneAPI.linphone_vcard_get_sip_addresses(curStruct.data);

            //            if (addressListPtr == IntPtr.Zero)
            //                continue;
            //            MSList addressdata;
            //            string sipAddress = "";
            //            do
            //            {
            //                addressdata.next = IntPtr.Zero;
            //                addressdata.prev = IntPtr.Zero;
            //                addressdata.data = IntPtr.Zero;

            //                addressdata = (MSList)Marshal.PtrToStructure(addressListPtr, typeof(MSList));
            //                if (addressdata.data != IntPtr.Zero)
            //                {
            //                    sipAddress = Marshal.PtrToStringAnsi(addressdata.data);
            //                    break;
            //                }
            //                addressListPtr = addressdata.next;
            //            } while (addressdata.data != IntPtr.Zero);


            //            if (!string.IsNullOrWhiteSpace(sipAddress) && !string.IsNullOrEmpty(fullname))
            //            {
            //                string un, host;
            //                int port;
            //                if ( VATRPCall.ParseSipAddress(sipAddress, out un, out host, out port) )
            //                    AddLinphoneContact(fullname, un, sipAddress);
            //            }
            //        }
            //        vcardsList = curStruct.next;
            //    } while (curStruct.next != IntPtr.Zero);
            //}
            //return 0;
        }


        public string createVcard(string fn, string ln, string address)
        {

            string vCard = "BEGIN:VCARD" + Environment.NewLine + "VERSION:4.0" + Environment.NewLine;
            if (address != null)
            {
                vCard = vCard + "IMPP:sip:" + address + Environment.NewLine;
            }
            if (fn != null)
            {
                vCard = vCard + "FN:" + fn + Environment.NewLine;
            }
            vCard = vCard + "END:VCARD";

            string tempPath = Path.GetTempFileName(); // Path.GetTempPath();

            tempPath = tempPath.Replace(".tmp", ".vcf");
            using (StreamWriter sw = new StreamWriter(tempPath))
            {
                sw.WriteLine(vCard);
                sw.Close();
            }

           // _log.Info("====================================================" + "Card Created: " + tempPath);

           // System.Windows.Forms.MessageBox.Show("Card Created: " + tempPath);
            return tempPath;

        }
        
        //  Added 4/11/2017 MITRE-fjr based off old import code
        public int importXcardVcard(string vcardPath)
        {

            // System.Windows.Forms.MessageBox.Show("importXcardVcard: " + vcardPath);
            //*****************************************************************************************************
            // THIS METHOD IS CREATED BY MK ON DATED 20-12-2016 FOR IMPORT MULTIPLE VCARD FROM XCARD FILE
            //*****************************************************************************************************
            // cjm-sep17 -- need to check core otherwise signout in middle of import will break
            if (_manager.LinphoneService.LinphoneCore == IntPtr.Zero)
            {
                return 0;
            }
            IntPtr friendlist = LinphoneAPI.linphone_core_get_default_friend_list(_manager.LinphoneService.LinphoneCore);
            int _count = LinphoneAPI.linphone_friend_list_import_friends_from_vcard4_file(friendlist, vcardPath);
            //bool _loadContacts = false;
            IntPtr vcardsList = LinphoneAPI.linphone_core_get_friend_list(_manager.LinphoneService.LinphoneCore);

            if (vcardsList != IntPtr.Zero)
            {
                MSList curStruct;
                do
                {
                    curStruct.next = IntPtr.Zero;
                    curStruct.prev = IntPtr.Zero;
                    curStruct.data = IntPtr.Zero;

                    curStruct = (MSList)Marshal.PtrToStructure(vcardsList, typeof(MSList));
                    if (curStruct.data != IntPtr.Zero)
                    {
                        IntPtr addressPtr = LinphoneAPI.linphone_friend_get_address(curStruct.data);
                        if (addressPtr == IntPtr.Zero)
                            continue;

                        string dn = "";
                        IntPtr tmpPtr = LinphoneAPI.linphone_address_get_display_name(addressPtr);
                        if (tmpPtr != IntPtr.Zero)
                        {
                            dn = Marshal.PtrToStringAnsi(tmpPtr);
                        }

                        string un = "";
                        tmpPtr = LinphoneAPI.linphone_address_get_username(addressPtr);
                        if (tmpPtr != IntPtr.Zero)
                        {
                            un = Marshal.PtrToStringAnsi(tmpPtr);
                        }

                        string host = "";
                        tmpPtr = LinphoneAPI.linphone_address_get_domain(addressPtr);
                        if (tmpPtr != IntPtr.Zero)
                        {
                            host = Marshal.PtrToStringAnsi(tmpPtr);
                        }

                        string refKey = "";
                        tmpPtr = LinphoneAPI.linphone_friend_get_ref_key(curStruct.data);
                        if (tmpPtr != IntPtr.Zero)
                            refKey = Marshal.PtrToStringAnsi(tmpPtr);

                        if (string.IsNullOrEmpty(refKey))
                        {
                            // generate refkey
                            refKey = Guid.NewGuid().ToString();
                            LinphoneAPI.linphone_friend_set_ref_key(curStruct.data, refKey);

                            // Added MITRE-fjr 4/27 Save here??
                           // LinphoneAPI.linphone_friend_save(curStruct.data, _manager.LinphoneService.LinphoneCore);
                        }

                        int port = LinphoneAPI.linphone_address_get_port(addressPtr);

                        if (!string.IsNullOrWhiteSpace(un))
                        {
                            var sipAddress = port == 0 ? string.Format("sip:{0}@{1}", un, host) : string.Format("{0}@{1}:{2}", un, host, port);
                            var fullname = dn.NotBlank() ? dn : un;
                            if (!string.IsNullOrWhiteSpace(sipAddress) && !string.IsNullOrEmpty(fullname))
                            {
                                //_loadContacts = true;
                            }
                        }
                    }
    
                    LinphoneAPI.linphone_friend_done(curStruct.data);
                    //  Added 5/5 MITRE-fjr
                    //LinphoneAPI.linphone_friend_unref(curStruct.data);

                    vcardsList = curStruct.next;
                } while (curStruct.next != IntPtr.Zero);

                //  Check for Imported Contact
                //if (_loadContacts)
                //if (ReadyToLoad)
                //{
                //    LoadLinphoneContacts();
                //}
            }
            return 0;

            ////IntPtr friendlist = LinphoneAPI.linphone_core_get_default_friend_list(_manager.LinphoneService.LinphoneCore);
            ////int _count = LinphoneAPI.linphone_friend_list_import_friends_from_vcard4_file(friendlist, vcardPath);
            ////IntPtr friends = LinphoneAPI.linphone_friend_list_get_friends(friendlist);
            
            ////IntPtr vFriendList = LinphoneAPI.linphone_core_get_friend_list(_manager.LinphoneService.LinphoneCore);

            ////if (vFriendList != IntPtr.Zero && _count > 0)
            ////{
            ////    MSList curStruct;
            ////    do
            ////    {
            ////        curStruct.next = IntPtr.Zero;
            ////        curStruct.prev = IntPtr.Zero;
            ////        curStruct.data = IntPtr.Zero;

            ////        curStruct = (MSList)Marshal.PtrToStructure(vFriendList, typeof(MSList));
            ////        if (curStruct.data != IntPtr.Zero)
            ////        {
            ////            string fullname = string.Empty;
            ////            IntPtr tmpPtr = LinphoneAPI.linphone_friend_get_name(curStruct.data);
            ////            if (tmpPtr != IntPtr.Zero)
            ////            {
            ////                fullname = Marshal.PtrToStringAnsi(tmpPtr);
            ////            }

            ////            IntPtr linPhoneAddress = LinphoneAPI.linphone_friend_get_addresses(curStruct.data);

            ////            IntPtr linPhoneAddress2 = LinphoneAPI.linphone_friend_list_get_uri(curStruct.data);
            ////            string _test = Marshal.PtrToStringAnsi(linPhoneAddress2);
                        







            ////            if (linPhoneAddress == IntPtr.Zero)
            ////                continue;

            ////           //LinphoneVcard* linphone_friend_get_vcard  ( LinphoneFriend *  fr ) 
            ////           // IntPtr _vcard = LinphoneAPI.linphone_friend_get_vcard(curStruct.data);


            ////            MSList addressdata;
            ////            string sipAddress = "";
            ////            do
            ////            {
            ////                addressdata.next = IntPtr.Zero;
            ////                addressdata.prev = IntPtr.Zero;
            ////                addressdata.data = IntPtr.Zero;

            ////                addressdata = (MSList)Marshal.PtrToStructure(linPhoneAddress, typeof(MSList));
            ////                if (addressdata.data != IntPtr.Zero)
            ////                {
            ////                    sipAddress = Marshal.PtrToStringAnsi(addressdata.data);
            ////                    sipAddress = Marshal.PtrToStringAnsi(addressdata.next);
            ////                    sipAddress = Marshal.PtrToStringAnsi(addressdata.prev);
            ////                    break;
            ////                }
            ////                linPhoneAddress = addressdata.next;
            ////            } while (addressdata.data != IntPtr.Zero);


            ////            if (!string.IsNullOrWhiteSpace(sipAddress) && !string.IsNullOrEmpty(fullname))
            ////            {
            ////                string un, host;
            ////                int port;
            ////                if (VATRPCall.ParseSipAddress(sipAddress, out un, out host, out port))
            ////                    AddLinphoneContact(fullname, un, sipAddress);
            ////            }
            ////        }
            ////        vFriendList = curStruct.next;
            ////    } while (curStruct.next != IntPtr.Zero);
            ////}

            ////return _count;
        }

        //public int importXcardVcard(string vcardPath)
        //{

        //    // System.Windows.Forms.MessageBox.Show("importXcardVcard: " + vcardPath);
        //    //*****************************************************************************************************
        //    // THIS METHOD IS CREATED BY MK ON DATED 20-12-2016 FOR IMPORT MULTIPLE VCARD FROM XCARD FILE
        //    //*****************************************************************************************************


        //    IntPtr vcardsList = LinphoneAPI.linphone_vcard_list_from_vcard4_file(vcardPath);

        //    if (vcardsList != IntPtr.Zero)
        //    {
        //        MSList curStruct;
        //        do
        //        {
        //            curStruct.next = IntPtr.Zero;
        //            curStruct.prev = IntPtr.Zero;
        //            curStruct.data = IntPtr.Zero;

        //            curStruct = (MSList)Marshal.PtrToStructure(vcardsList, typeof(MSList));
        //            if (curStruct.data != IntPtr.Zero)
        //            {
        //                string fullname = string.Empty;
        //                IntPtr tmpPtr = LinphoneAPI.linphone_vcard_get_full_name(curStruct.data);
        //                if (tmpPtr != IntPtr.Zero)
        //                {
        //                    fullname = Marshal.PtrToStringAnsi(tmpPtr);
        //                }

        //                IntPtr addressListPtr = LinphoneAPI.linphone_vcard_get_sip_addresses(curStruct.data);

        //                if (addressListPtr == IntPtr.Zero)
        //                    continue;
        //                MSList addressdata;
        //                string sipAddress = "";
        //                do
        //                {
        //                    addressdata.next = IntPtr.Zero;
        //                    addressdata.prev = IntPtr.Zero;
        //                    addressdata.data = IntPtr.Zero;

        //                    addressdata = (MSList)Marshal.PtrToStructure(addressListPtr, typeof(MSList));
        //                    if (addressdata.data != IntPtr.Zero)
        //                    {
        //                        sipAddress = Marshal.PtrToStringAnsi(addressdata.data);
        //                        break;
        //                    }
        //                    addressListPtr = addressdata.next;
        //                } while (addressdata.data != IntPtr.Zero);


        //                if (!string.IsNullOrWhiteSpace(sipAddress) && !string.IsNullOrEmpty(fullname))
        //                {
        //                    string un, host;
        //                    int port;
        //                    if (VATRPCall.ParseSipAddress(sipAddress, out un, out host, out port))
        //                        AddLinphoneContact(fullname, un, sipAddress);
        //                }
        //            }
        //            vcardsList = curStruct.next;
        //        } while (curStruct.next != IntPtr.Zero);
        //    }
        //    return 0;

        //}
        public int mod_importXcardVcard(string vcardPath)
        {

           // System.Windows.Forms.MessageBox.Show("importXcardVcard: " + vcardPath);
            //*****************************************************************************************************
            // THIS METHOD IS CREATED BY MK ON DATED 20-12-2016 FOR IMPORT MULTIPLE VCARD FROM XCARD FILE
            //*****************************************************************************************************

            
            //IntPtr vcardsList = LinphoneAPI.linphone_vcard_list_from_vcard4_file(vcardPath);
            //IntPtr vcardsList = IntPtr.Zero;

            //int _count = LinphoneAPI.linphone_friend_list_import_friends_from_vcard4_file(LinphoneFriendList2, vcardPath);
            
            //IntPtr contactsPtr = LinphoneAPI.linphone_core_get_friend_list(_manager.LinphoneService.LinphoneCore);

            //IntPtr friendlist = LinphoneAPI.linphone_core_create_friend_list(_manager.LinphoneService.LinphoneCore);

            IntPtr friendlist = LinphoneAPI.linphone_core_get_default_friend_list(_manager.LinphoneService.LinphoneCore);

            int _count = LinphoneAPI.linphone_friend_list_import_friends_from_vcard4_file(friendlist, vcardPath);

            IntPtr friends = LinphoneAPI.linphone_friend_list_get_friends(friendlist);

            //if (vcardsList != IntPtr.Zero)
            if (_count > 0)
            {

                IntPtr _temp = LinphoneAPI.linphone_friend_list_get_display_name(friendlist); 

               // IntPtr tmpPtr = LinphoneAPI.linphone_friend_get_name(curStruct.data);

                MSList curStruct;
                //do
                //{
                    curStruct.next = IntPtr.Zero;
                    curStruct.prev = IntPtr.Zero;
                    curStruct.data = IntPtr.Zero;

                    curStruct = (MSList)Marshal.PtrToStructure(friends, typeof(MSList));
                //}
                ////    if (curStruct.data != IntPtr.Zero)
                ////    {
                        string fullname = string.Empty;
                ////        //IntPtr tmpPtr = LinphoneAPI.linphone_vcard_get_full_name(curStruct.data);

                ////        //const char *  linphone_friend_get_name (const LinphoneFriend *lf)  
                ////        IntPtr tmpPtr = LinphoneAPI.linphone_friend_get_name(curStruct.data);

                ////        if (tmpPtr != IntPtr.Zero)
                ////        {
                ////            fullname = Marshal.PtrToStringAnsi(tmpPtr);
                ////        }

                ////        IntPtr addressListPtr = LinphoneAPI.linphone_vcard_get_sip_addresses(curStruct.data);

                ////        if (addressListPtr == IntPtr.Zero)
                ////            continue;
                ////        MSList addressdata;
                        string sipAddress = "";
                ////        do
                ////        {
                ////            addressdata.next = IntPtr.Zero;
                ////            addressdata.prev = IntPtr.Zero;
                ////            addressdata.data = IntPtr.Zero;

                ////            addressdata = (MSList)Marshal.PtrToStructure(addressListPtr, typeof(MSList));
                ////            if (addressdata.data != IntPtr.Zero)
                ////            {
                ////                sipAddress = Marshal.PtrToStringAnsi(addressdata.data);
                ////                break;
                ////            }
                ////            addressListPtr = addressdata.next;
                ////        } while (addressdata.data != IntPtr.Zero);


                        if (!string.IsNullOrWhiteSpace(sipAddress) && !string.IsNullOrEmpty(fullname))
                        {
                            string un, host;
                            int port;
                            if (VATRPCall.ParseSipAddress(sipAddress, out un, out host, out port))
                                AddLinphoneContact(fullname, un, sipAddress);
                        }
                //    }
                ////    vcardsList = curStruct.next;
                ////} while (curStruct.next != IntPtr.Zero);
            }
            return _count;

        }

        public void ExportVCards(string vcardPath)
        {
            //**************************************************************************************************************
            // Export Vcard contact from contact list.
            //**************************************************************************************************************
            if (_manager.LinphoneService.LinphoneCore == IntPtr.Zero)
                return;
            IntPtr defList = LinphoneAPI.linphone_core_get_default_friend_list(_manager.LinphoneService.LinphoneCore);
            if (defList != IntPtr.Zero)
            {
                LinphoneAPI.linphone_friend_list_export_friends_as_vcard4_file(defList, vcardPath);
            }
        }

        #endregion

        public void AddLinphoneContact(string name, string username, string address)
        {


           
           // RemoveDBPassword2();
            //**************************************************************************************************************
            // Add contact in Linphone contact, Also called when Import Vcard contact in contact list.
            //**************************************************************************************************************
            if (_manager.LinphoneService.LinphoneCore == IntPtr.Zero)
                return;

            var sipAddress = address.TrimSipPrefix();

            //var contactID1 = new ContactID(sipAddress, IntPtr.Zero);
            //VATRPContact contact1 = FindContact(contactID1);
            //if (contact1 != null)
            //{
            //    return;
            //}

            var fqdn = string.Format("{0} <sip:{1}>", name,  sipAddress);
            IntPtr friendPtr = LinphoneAPI.linphone_friend_new_with_address(fqdn);
            if (friendPtr != IntPtr.Zero)
            {
                IntPtr friendList =
                    LinphoneAPI.linphone_core_get_default_friend_list(_manager.LinphoneService.LinphoneCore);
                LinphoneAPI.linphone_friend_edit(friendPtr); //AGW to eliminate carddav sync issues
                LinphoneAPI.linphone_friend_set_name(friendPtr, name);
                LinphoneAPI.linphone_friend_enable_subscribes(friendPtr, false);
                LinphoneAPI.linphone_friend_set_inc_subscribe_policy(friendPtr, 1);
                LinphoneAPI.linphone_friend_list_add_friend(friendList, friendPtr);
                var refKey = Guid.NewGuid().ToString();
                LinphoneAPI.linphone_friend_set_ref_key(friendPtr, refKey);
                LinphoneAPI.linphone_friend_done(friendPtr); //AGW to eliminate carddav sync issues
                var contactID = new ContactID(sipAddress, IntPtr.Zero);
                VATRPContact contact = FindContact(contactID);
                if (contact == null)
                {
                    contact = new VATRPContact(new ContactID(sipAddress, IntPtr.Zero))
                    {
                        DisplayName =  name,
                        Fullname = name,
                        Gender = "male",
                        SipUsername = username,
                        RegistrationName = sipAddress,
                        IsLinphoneContact = true,
                        LinphoneRefKey = refKey
                    };
                    Contacts.Add(contact);
                }
                else
                {
                    contact.DisplayName = name;
                    contact.Fullname = name;
                    contact.IsLinphoneContact = true;
                    contact.SipUsername = username;
                    contact.LinphoneRefKey = refKey;
                }
                UpdateAvatar(contact);
                UpdateContactDbId(contact);

                if (ContactAdded != null)
                {
                    Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                    {
                        ContactAdded(this, new ContactEventArgs(new ContactID(contact)));
                    }));
                }
            }
        }

        private void UpdateContactDbId(VATRPContact contact)
        {

            //**************************************************************************************************************
            // When Import Vcard contact in contact list, then updating the contact.
            //**************************************************************************************************************
            // RemoveDBPassword2();
            // AddDBPassword2();

            if (editing)
                return;
            editing = true;
            try
            {
                var connectionString = string.Format("data source={0}", _manager.LinphoneService.ContactsDbPath);// + ";Version=3;Password=1234;");
                using (var dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    using (
                        var cmd = new SQLiteCommand(dbConnection)
                        {
                            CommandText = @"SELECT id FROM friends WHERE sip_uri = ?"
                        })
                    {
                        //var sipUri = string.Format(@"""{0}"" <sip:{1}>", contact.DisplayName, contact.ID);
                        var sipUri = string.Format(@"sip:{0}", contact.DisplayName, contact.ID);
                       
                        var uriParam = new SQLiteParameter(DbType.AnsiString) { Value = sipUri };
                        cmd.Parameters.Add(uriParam);
                        var dbReader = cmd.ExecuteReader();
                        if (dbReader.Read())
                        {
                            contact.DbID = dbReader.GetInt32(0);
                        }
                    }
                    dbConnection.Close();

                    //  Added MITRE-fjr ??
                    dbConnection.Dispose();
                }
            }
            catch (SQLiteException ex)
            {
                Debug.WriteLine("Sqlite error: " + ex.ToString());
            }
            finally
            {
                editing = false;
            }
        }

        //private void UpdateContactDbId(VATRPContact contact)
        //{

        //    //**************************************************************************************************************
        //    // When Import Vcard contact in contact list, then updating the contact.
        //    //**************************************************************************************************************
        //   // RemoveDBPassword2();
        //   // AddDBPassword2();

        //    if (editing)
        //        return;
        //    editing = true;
        //    try
        //    {
        //        var connectionString = string.Format("data source={0}", _manager.LinphoneService.ContactsDbPath);// + ";Version=3;Password=1234;");
        //        using (var dbConnection = new SQLiteConnection(connectionString))
        //        {
        //            dbConnection.Open();
        //            using (
        //                var cmd = new SQLiteCommand(dbConnection)
        //                {
        //                    //  Changed SQL MITRE-fjr
        //                    //CommandText = @"SELECT id FROM friends WHERE sip_uri = ?"
        //                    CommandText = @"SELECT id FROM friends WHERE ref_key = ?"
        //                })
        //            {
        //                //var sipUri = string.Format(@"""{0}"" <sip:{1}>", contact.DisplayName, contact.ID);
        //                //var uriParam = new SQLiteParameter(DbType.AnsiString) {Value = sipUri};
        //                //  Display name was NOT consistent, changed the where clause to ref key, sets the id correctly now
        //                //  Changed 4/11 MITRE-fjr
        //                var refKey = string.Format(@"{0}", contact.LinphoneRefKey);
        //                var uriParam = new SQLiteParameter(DbType.AnsiString) { Value = refKey };

        //                cmd.Parameters.Add(uriParam);
        //                var dbReader = cmd.ExecuteReader();
        //                if (dbReader.Read())
        //                {
        //                    contact.DbID = dbReader.GetInt32(0);
        //                }
        //            }
        //            dbConnection.Close();
        //        }
        //    }
        //    catch (SQLiteException ex)
        //    {
        //        Debug.WriteLine("Sqlite error: " + ex.ToString());
        //    }
        //    finally
        //    {
        //        editing = false;
        //    }
        //}

        public void EditLinphoneContact(VATRPContact contact, string newname, string newsipassdress)
        {

            //************************************************************************************************************************
            // Edit contact in Contact list.
            //************************************************************************************************************************
            if (_manager.LinphoneService.LinphoneCore == IntPtr.Zero)
                return;

            IntPtr friendList = LinphoneAPI.linphone_core_get_default_friend_list(_manager.LinphoneService.LinphoneCore);
            if (friendList == IntPtr.Zero)
                return;

            //  Changed 5/2/2017 MITRE-fjr
            // old way IntPtr friendPtr = LinphoneAPI.linphone_friend_list_find_friend_by_ref_key(friendList,contact.LinphoneRefKey);

            //  Updated 5/2/2017 MITRE-fjr
            string _uri = string.Format("sip:{0}", contact.RegistrationName);
            IntPtr friendPtr = LinphoneAPI.linphone_friend_list_find_friend_by_uri(friendList, _uri);

            if (friendPtr == IntPtr.Zero)
                return;

            var newfqdn = string.Format("sip:{0}", newsipassdress);
            IntPtr addressPtr = LinphoneAPI.linphone_core_create_address(
                _manager.LinphoneService.LinphoneCore, string.Format("{0} <{1}>", newname, newfqdn));
            if (addressPtr != IntPtr.Zero)
            {
                LinphoneAPI.linphone_friend_edit(friendPtr);
                int i = LinphoneAPI.linphone_friend_set_name(friendPtr, newname);
                Debug.Print(string.Format("Friends updated: {0}", i));
                LinphoneAPI.linphone_friend_set_address(friendPtr, addressPtr);
                LinphoneAPI.linphone_friend_enable_subscribes(friendPtr, false);
                LinphoneAPI.linphone_friend_set_inc_subscribe_policy(friendPtr, 1);
                LinphoneAPI.linphone_friend_done(friendPtr);
                //  Added 5/5/MITRE-fjr
                //LinphoneAPI.linphone_friend_unref(friendPtr);


                string un = "";
                IntPtr unPtr = LinphoneAPI.linphone_address_get_username(addressPtr);
                if (unPtr != IntPtr.Zero)
                {
                    un = Marshal.PtrToStringAnsi(unPtr);
                }

                contact.ID = newsipassdress;
                contact.DisplayName = newname;
                contact.Fullname = newname;
                contact.SipUsername = un;
                contact.RegistrationName = newsipassdress;
            }
            UpdateAvatar(contact);
            UpdateContactDbId(contact);

            if (ContactsChanged != null)
                ContactsChanged(this, new ContactEventArgs(new ContactID(contact)));
        }

    
        //  Worked MITRE-fjr
        public void DeleteLinphoneContact(VATRPContact contact)
        {
            //****************************************************************************************************
            // Remove the contacts from All/Favorite contact list.
            //****************************************************************************************************
            if (_manager.LinphoneService.LinphoneCore == IntPtr.Zero)
                return;

            IntPtr friendList = LinphoneAPI.linphone_core_get_default_friend_list(_manager.LinphoneService.LinphoneCore);
            //LinphoneAPI.linphone_friend_list_update_dirty_friends(friendList);

            if (friendList != IntPtr.Zero)
            {
                string _uri = string.Format("sip:{0}", contact.RegistrationName);
                //IntPtr friendPtr = LinphoneAPI.linphone_friend_list_find_friend_by_ref_key(friendList, contact.LinphoneRefKey);
                IntPtr friendPtr = LinphoneAPI.linphone_friend_list_find_friend_by_uri(friendList, _uri);
                if (friendPtr == IntPtr.Zero)
                    return;

                if (friendList != IntPtr.Zero)
                {
                    LinphoneFriendListStatus _status = LinphoneAPI.linphone_friend_list_remove_friend(friendList, friendPtr);
                }

                RemoveFavoriteOption(contact);
                try
                {
                    if (contact.Avatar.NotBlank() && File.Exists(contact.Avatar))
                    {
                        File.Delete(contact.Avatar);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("failed to remove file: " + ex.Message);
                }
                RemoveContact(contact.ID, true);

            }
        }
        public void AddContact(VATRPContact contact, string group)
        {

            //************************************************************************************************************************************
            // Adding contact in Chat history and Call History.
            //************************************************************************************************************************************
            // update avatar
            UpdateAvatar(contact);
            Contacts.Add(contact);
            if (ContactAdded != null)
                ContactAdded(this, new ContactEventArgs(new ContactID(contact)));

            if (contact.IsLoggedIn && LoggedInContactUpdated != null)
            {
                LoggedInContactUpdated(this, new ContactEventArgs(new ContactID(contact)));
            }
        }

        public VATRPContact FindContact(ContactID contactID)
        {
            //****************************************************************************************************
            // Finding a Contact.
            //****************************************************************************************************
            if (contactID == null)
            {
                return null;
            }
            return this.FindContact(contactID.ID);
        }

        public VATRPContact FindContact(int contactID)
        {
            lock (this.Contacts)
            {
                foreach (VATRPContact contact in this.Contacts)
                {
                    if ((contact != null) && (contact.ID == contactID.ToString()))
                    {
                        return contact;
                    }
                }
            }
            return null;
        }
        public VATRPContact FindContactByPhone(string phoneNumber)
        {
            if (!phoneNumber.NotBlank())
                return null;
            lock (this.Contacts)
            {
                foreach (VATRPContact contact in this.Contacts)
                {
                    if ((contact != null) && (contact.HomePhone == phoneNumber ||
                        contact.MobilePhone == phoneNumber))
                    {
                        return contact;
                    }
                }
            }
            return null;
        }

        public VATRPContact FindLoggedInContact()
        {

            //****************************************************************************************************
            // Check, Is contact is alredy logged in?
            //****************************************************************************************************
            lock (this.Contacts)
            {
                foreach (VATRPContact contact in this.Contacts)
                {
                    if ((contact != null) && contact.IsLoggedIn)
                    {
                        return contact;
                    }
                }
            }
            return null;
        }

        private VATRPContact FindContact(string id)
        {
            //****************************************************************************************************
            // Check the contact in Contacts.db database. Matching the Contact ID.
            //****************************************************************************************************
            lock (this.Contacts)
            {
                foreach (var contact in this.Contacts)
                {
                    if ((contact != null) && (contact.ID == id))
                    {
                        return contact;
                    }
                }
            }
            return null;
        }

        public void RemoveContact(string id, bool isUserAction)
        {
            //*************************************************************************************************************************************
            // Remove Contact from Contact list.
            //*************************************************************************************************************************************
            lock (this.Contacts)
            {
                VATRPContact contact = null;
                foreach (VATRPContact c in this.Contacts)
                {
                    if ((c != null) && (c.ID == id))
                    {
                        contact = c;
                        break;
                    }
                }

                if (contact != null)
                {
                    if (ContactRemoved != null)
                        ContactRemoved(this, new ContactRemovedEventArgs(new ContactID(contact), isUserAction));

                    this.Contacts.Remove(contact);
                }

            }
        }

        public void RemoveContacts()
        {
            lock (this.Contacts)
            {
                while (this.Contacts.Count > 0)
                {
                    foreach (VATRPContact contact in this.Contacts)
                    {
                        if (contact != null)
                        {
                            if (ContactRemoved != null)
                                ContactRemoved(this, new ContactRemovedEventArgs(new ContactID(contact), true));
                            this.Contacts.Remove(contact);
                            break;
                        }
                    }
                }
            }
        }

        public void RenameContact(ContactID contactID, string nick)
        {

        }

        private void UpdateAvatar(VATRPContact contact)
        {
            //*****************************************************************************************************************
            // Set the Avatar image if exist.
            //*****************************************************************************************************************
            var supportedFormats = new[] {"jpg", "jpeg", "png", "bmp"};
            for (var i = 0; i < supportedFormats.Length; i++)
            {
                var avatar = _manager.BuildDataPath(string.Format("{0}.{1}", contact.ID, supportedFormats[i]));
                if (File.Exists(avatar))
                {
                    contact.Avatar = avatar;
                    return;
                }
            }
        }

        internal bool SearchContactIfExistOrCreateNew(ContactID contactID, out VATRPContact contact)
        {
            bool flag = false;
            contact = this.FindContact(contactID);
            if (contact == null)
            {
                flag = true;
                contact = new VATRPContact(contactID);
            }
            return flag;
        }

        public ObservableCollection<VATRPContact> Contacts
        {
            get
            {
                if (this._contacts == null)
                {
                    this._contacts = new ObservableCollection<VATRPContact>();
                }
                return this._contacts;
            }
            private set { this._contacts = value; }
        }

        public ObservableCollection<VATRPUserGroup> Groups
        {
            get
            {
                if (this._groups == null)
                {
                    this._groups = new ObservableCollection<VATRPUserGroup>();
                }
                return this._groups;
            }
            private set { this._groups = value; }
        }

        private string GetGroupName(int groupID)
        {
            lock (Groups)
            {
                foreach (var group in Groups)
                {
                    if (group.ID == groupID)
                        return group.Name;
                }
            }

            return groupID == 0 ? "All" : string.Empty;
        }

        private void InitializeContactOptions()
        {
            if (!File.Exists(_manager.LinphoneService.ContactsDbPath))
                return;
            string sqlString = @"CREATE TABLE IF NOT EXISTS friend_options (id INTEGER NOT NULL," +
                               " is_favorite INTEGER NOT NULL DEFAULT 0, PRIMARY KEY (id))";
            if (editing)
                return;
            editing = true;
            try
            {
                var connectionString = string.Format("data source={0}", _manager.LinphoneService.ContactsDbPath);// + ";Version=3;Password=1234;");
                using (var dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    using (var cmd = new SQLiteCommand(dbConnection) {CommandText = sqlString})
                    {
                        cmd.ExecuteNonQuery();
                    }
                    dbConnection.Close();
                }
            }
            catch (SQLiteException ex)
            {
                Debug.WriteLine("Sqlite error: " + ex.ToString());
            }
            finally
            {
                editing = false;
            }
        }

        
        public bool Start()
        {

            //*********************************************************************************************************************************
            // Setting the Event of LinphoneServices.
            //**********************************************************************************************************************************
            IsLoaded = false;
            InitializeContactOptions();
            _manager.LinphoneService.CardDAVContactCreated += LinphoneCardDavContactCreated;
            _manager.LinphoneService.CardDAVContactUpdated += LinphoneCardDavContactUpdated;
            _manager.LinphoneService.CardDAVContactDeleted += LinphoneCardDavContactDeleted;
            _manager.LinphoneService.CardDAVSyncEvent += LinphoneCardDavSynced;

            // cjm-sep17 made this a task to try and un-freeze UI if there are a lot of contacts / contacts bug...
            Task T = Task.Run((Action)delegate ()
            {
                LoadLinphoneContacts();
            });
            
            _manager.LinphoneService.CardDAVSync();

            IsLoaded = true;
            if (ServiceStarted != null)
                ServiceStarted(this, EventArgs.Empty);
            return true;
        }

        public bool Stop()
        {
            _manager.LinphoneService.CardDAVContactCreated -= LinphoneCardDavContactCreated;
            _manager.LinphoneService.CardDAVContactUpdated -= LinphoneCardDavContactUpdated;
            _manager.LinphoneService.CardDAVContactDeleted -= LinphoneCardDavContactDeleted;
            _manager.LinphoneService.CardDAVSyncEvent -= LinphoneCardDavSynced;

            RemoveContacts();
            if (ServiceStopped != null)
                ServiceStopped(this, EventArgs.Empty);
            return true;
        }

        private void LinphoneCardDavContactCreated(CardDavContactEventArgs args)
        {
            if (args.NewContactPtr == IntPtr.Zero) return;
            IntPtr addressPtr = LinphoneAPI.linphone_friend_get_address(args.NewContactPtr);
            if (addressPtr == IntPtr.Zero)
                return;
            string dn = "";
            IntPtr tmpPtr = LinphoneAPI.linphone_address_get_display_name(addressPtr);
            if (tmpPtr != IntPtr.Zero)
            {
                dn = Marshal.PtrToStringAnsi(tmpPtr);
            }

            string un = "";
            tmpPtr = LinphoneAPI.linphone_address_get_username(addressPtr);
            if (tmpPtr != IntPtr.Zero)
            {
                un = Marshal.PtrToStringAnsi(tmpPtr);
            }

            string host = "";
            tmpPtr = LinphoneAPI.linphone_address_get_domain(addressPtr);
            if (tmpPtr != IntPtr.Zero)
            {
                host = Marshal.PtrToStringAnsi(tmpPtr);
            }

            int port = LinphoneAPI.linphone_address_get_port(addressPtr);

            if (string.IsNullOrWhiteSpace(un)) return;
            var cfgSipaddress = port == 0 ? string.Format("{0}@{1}", un, host) :
                string.Format("{0}@{1}:{2}", un, host, port);

            var refKey = "";
            IntPtr refKeyPtr = LinphoneAPI.linphone_friend_get_ref_key(args.NewContactPtr);
            if (refKeyPtr != IntPtr.Zero)
                refKey = Marshal.PtrToStringAnsi(refKeyPtr);

            if (String.IsNullOrEmpty(refKey))
            {
                refKey = Guid.NewGuid().ToString();
                LinphoneAPI.linphone_friend_set_ref_key(args.NewContactPtr, refKey);
            }
            VATRPContact contact = new VATRPContact(new ContactID(cfgSipaddress, IntPtr.Zero))
            {
                DisplayName = dn,
                Fullname = dn.NotBlank() ? dn : un,
                Gender = "male",
                SipUsername = un,
                RegistrationName = cfgSipaddress,
                IsLinphoneContact = true,
                LinphoneRefKey = refKey
            };
            AddContact(contact, string.Empty);
        }

        private void LinphoneCardDavContactUpdated(CardDavContactEventArgs args)
        {

        }

        private void LinphoneCardDavContactDeleted(CardDavContactEventArgs args)
        {
            if (args.ChangedContactPtr == IntPtr.Zero) return;
            IntPtr addressPtr = LinphoneAPI.linphone_friend_get_address(args.ChangedContactPtr);
            if (addressPtr == IntPtr.Zero) return;
            string dn = string.Empty;
            IntPtr tmpPtr = LinphoneAPI.linphone_address_get_display_name(addressPtr);
            if (tmpPtr != IntPtr.Zero)
            {
                dn = Marshal.PtrToStringAnsi(tmpPtr);
            }

            if (string.IsNullOrEmpty(dn))
                return;

            string un = "";
            tmpPtr = LinphoneAPI.linphone_address_get_username(addressPtr);
            if (tmpPtr != IntPtr.Zero)
            {
                un = Marshal.PtrToStringAnsi(tmpPtr);
            }

            string host = "";
            tmpPtr = LinphoneAPI.linphone_address_get_domain(addressPtr);
            if (tmpPtr != IntPtr.Zero)
            {
                host = Marshal.PtrToStringAnsi(tmpPtr);
            }

            var cfgSipAddress = string.Format("{0}@{1}", un, host);

            VATRPContact contact = FindContact(new ContactID(cfgSipAddress, IntPtr.Zero));
            if (contact != null)
            {
                RemoveFavoriteOption(contact);
                try
                {
                    if (contact.Avatar.NotBlank() && File.Exists(contact.Avatar))
                    {
                        File.Delete(contact.Avatar);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("failed to remove file: " + ex.Message);
                }
                RemoveContact(cfgSipAddress, false);
            }
        }

        private void LinphoneCardDavSynced(CardDavSyncEventArgs args)
        {

        }

        public event EventHandler<EventArgs> ServiceStarted;
        public event EventHandler<EventArgs> ServiceStopped;



        private string Encrypt(string clearText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        private string Decrypt(string cipherText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }



        private bool isXCard(string path)
        {

            try
            {
               var result = new XmlDocument();
               result.Load(path);
                return true;
            }
            catch (XmlException ex)
            {
                return false;
            }

            return true;

        }

        public void AddDBPassword2()
        {
            try
            {
                //string conn = @"Data Source=database.s3db;Password=Mypass;";
                string conn = string.Format("data source={0}", _manager.LinphoneService.ContactsDbPath + ";Password=1234;");
                SQLiteConnection connection = new SQLiteConnection(conn);
                connection.Open();
                //Some code
                connection.ChangePassword("1234"); //connection.ChangePassword("");
                connection.Close();

                //UpdatePrivateDataPath();
            }
            //if it is the first time sets the password in the database
            catch
            {
                string conn = string.Format("data source={0}", _manager.LinphoneService.ContactsDbPath + ";");
                SQLiteConnection connection = new SQLiteConnection(conn);
                connection.Open();
                //Some code
                connection.ChangePassword("1234");
                connection.Close();

                //UpdatePrivateDataPath();
            }
        }

        public void RemoveDBPassword2()
        {
            try
            {
                //string conn = @"Data Source=database.s3db;Password=Mypass;";
                string conn = string.Format("data source={0}", _manager.LinphoneService.ContactsDbPath + ";Password=1234;");
                SQLiteConnection connection = new SQLiteConnection(conn);
                connection.Open();
                //Some code
                connection.ChangePassword(""); //connection.ChangePassword("");
                connection.Close();

                //UpdatePrivateDataPath();
            }
            //if it is the first time sets the password in the database
            catch
            {
                string conn = string.Format("data source={0}", _manager.LinphoneService.ContactsDbPath + ";");
                SQLiteConnection connection = new SQLiteConnection(conn);
                connection.Open();
                //Some code
                connection.ChangePassword("");
                connection.Close();

                //UpdatePrivateDataPath();
            }
        }
    }
}

