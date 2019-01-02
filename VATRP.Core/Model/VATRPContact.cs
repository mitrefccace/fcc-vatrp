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
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using VATRP.Core.Enums;
using VATRP.Core.Extensions;

namespace VATRP.Core.Model
{
    public sealed class VATRPContact : ContactID, IEquatable<VATRPContact>, IComparable, IComparable<VATRPContact>
    {
        private string _displayName;
        private string _email;
        private string _firstname;
        private string _fullname;
        private string _initials;
        private bool _isEnableOnlineNotifications;
        private bool _isFavorite;
        private string _homePhone;
        private bool _isConnected;
        private bool _isLoggedIn;
        private string _lastname;
        private DateTime? _lastSeen;
        private string _middlename;
        private string _mobilePhone;
        private string _sipUsername;
        private string _registrationName;
        private string _gender;
        private bool _onlineNotification;
        private UserStatus _status;
        private uint _unreadMsgCount;
        private bool _isLinphoneContact;
        private List<int> _groupIdList;
        private string _avatar;
        private int _dbId;

        public VATRPContact()
        {
            this.Init();
        }

        public VATRPContact(ContactID contactID):this()
        {
            if (contactID != null)
            {
                base.ID = contactID.ID;
                ID = contactID.ID;
            }
        }

        public void AddGroup(string _groupName)
        {
            if (!string.IsNullOrEmpty(_groupName) && !this.IsGroupExistInGroupList(_groupName))
            {
                lock (this.GroupList)
                {
                    this.GroupList.Add(_groupName);
                }
            }
        }

        public void AddGroupsToList(List<string> _groupList)
        {
            if ((_groupList != null) && (_groupList.Count > 0))
            {
                foreach (string str in _groupList)
                {
                    if (!string.IsNullOrEmpty(str))
                    {
                        this.AddGroup(str);
                    }
                }
            }
        }

        public int CompareTo(VATRPContact other)
        {
            return base.ID.CompareTo(other.ID);
        }

        public bool Equals(VATRPContact other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }
            return (object.ReferenceEquals(this, other) || (base.ID == other.ID));
        }

        public override bool Equals(object obj)
        {
            return ((obj is VATRPContact) && this.Equals(obj as VATRPContact));
        }

        public override int GetHashCode()
        {
            return base.ID.GetHashCode();
        }

        private void Init()
        {
            this.Status = UserStatus.Offline;
            base.ID = string.Empty;
            this.DisplayName = string.Empty;
            this.Group = string.Empty;
            this.OnlineNotification = false;
            this.GroupList = new List<string>();
            this.UnreadMsgCount = 0;
            this.IsUpdated = true;
            this.IsStatusUpdated = true;
            this.Email = string.Empty;
            this.Firstname = string.Empty;
            this.Fullname = string.Empty;
            this.Initials = string.Empty;
            this.HomePhone = string.Empty;
            this.Lastname = string.Empty;
            this.Middlename = string.Empty;
            this.MobilePhone = string.Empty;
            this.Gender = string.Empty;
            this.SipUsername = string.Empty;
            this._avatar = string.Empty;
            this.IsLinphoneContact = false;
            this.RegistrationName = string.Empty;
            this.DbID = 0;
            this.LinphoneRefKey = string.Empty;
        }

        public bool IsGroupExistInGroupList(string _groupName)
        {
            lock (this.GroupList)
            {
                using (List<string>.Enumerator enumerator = this.GroupList.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current != null && enumerator.Current.CompareTo(_groupName) == 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void RemoveGroupFromGroupList(string _groupName)
        {
            lock (this.GroupList)
            {
                this.GroupList.Remove(_groupName);
            }
        }
        public static bool operator ==(VATRPContact one, VATRPContact two)
        {
            return (object.ReferenceEquals(one, two) || one.Equals(two));
        }

        public static bool operator !=(VATRPContact one, VATRPContact two)
        {
            return !(one == two);
        }

        int IComparable.CompareTo(object obj)
        {
            if (!(obj is VATRPContact))
            {
                throw new ArgumentException("Argument is not a ContactElement", "obj");
            }
            return this.CompareTo((VATRPContact)obj);
        }

        public int DbID
        {
            get { return _dbId; }
            set { _dbId = value; }
        }

        public string ContactName_ForUI
        {
            get
            {
                if (DisplayName.NotBlank())
                    return DisplayName;
                if (Fullname.NotBlank())
                    return Fullname;
                return ID;
            }
        }

        public string DisplayName
        {
            get
            {
                return this._displayName;
            }
            set
            {
                if (this._displayName != value)
                {
                    this._displayName = value;
                    OnPropertyChanged("ContactName_ForUI");
                }
            }
        }
        public string RegistrationName
        {
            get
            {
                return this._registrationName;
            }
            set
            {
                if (this._registrationName != value)
                {
                    this._registrationName = value;
                    OnPropertyChanged("RegistrationName");
                    OnPropertyChanged("ContactAddress_ForUI");
                }
            }
        }

        public string ContactAddress_ForUI
        {
            get
            {
                _registrationName.TrimSipPrefix();
                return _registrationName;
            }
        }


        public string Email
        {
            get
            {
                return this._email;
            }
            set
            {
                this._email = value;
                OnPropertyChanged("Email");
            }
        }

        public string Avatar
        {
            get
            {
                return this._avatar;
            }
            set
            {
                this._avatar = value;
                OnPropertyChanged("Avatar");
            }
        }

        public string Firstname
        {
            get
            {
                return this._firstname;
            }
            set
            {
                this._firstname = value;
                OnPropertyChanged("Firstname");
            }
        }

        public string Fullname
        {
            get
            {
                return this._fullname;
            }
            set
            {
                char[] delimiters = {' '};
                string[] splitStrings = value.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                this._fullname = value;
                var initialString = string.Empty;
                for (int i = 0; i < splitStrings.Length; i++)
                {
                    initialString += splitStrings[i].Substring(0, 1).ToUpper();
                }

                Initials = initialString;
                OnPropertyChanged("Fullname");
                OnPropertyChanged("ContactName_ForUI");
            }
        }

        public string Gender
        {
            get
            {
                return this._gender;
            }
            set
            {
                this._gender = value;
                OnPropertyChanged("Gender");
            }
        }


        public string Group { get; set; }

        public string LinphoneRefKey { get; set; }

        public List<int> GroupIdList
        {
            get
            {
                if (_groupIdList == null)
                    _groupIdList = new List<int>();
                return _groupIdList;
            }
            set
            {
                _groupIdList = value;
                OnPropertyChanged("GroupIdList");
            }
        }


        public List<string> GroupList { get; set; }

        public bool HasUnreadMsg
        {
            get
            {
                return (this.UnreadMsgCount > 0);
            }
        }

        public string HomePhone
        {
            get
            {
                return this._homePhone;
            }
            set
            {
                this._homePhone = value;
                OnPropertyChanged("HomePhone");
            }
        }
        public string Initials
        {
            get
            {
                return this._initials;
            }
            set
            {
                this._initials = value;
                OnPropertyChanged("Initials");
            }
        }

        public bool IsConnected
        {
            get
            {
                return this._isConnected;
            }
            set
            {
                this._isConnected = value;
                OnPropertyChanged("LastActivity");
            }
        }

        public bool IsLinphoneContact
        {
            get
            {
                return this._isLinphoneContact;
            }
            set
            {
                this._isLinphoneContact = value;
                OnPropertyChanged("IsLinphoneContact");
            }
        }
        public bool IsLoggedIn
        {
            get
            {
                return this._isLoggedIn;
            }
            set
            {
                this._isLoggedIn = value;
                OnPropertyChanged("IsLoggedIn");
            }
        }
        public bool IsEnableOnlineNotifications
        {
            get
            {
                return this._isEnableOnlineNotifications;
            }
            set
            {
                if (this._isEnableOnlineNotifications != value)
                {
                    this._isEnableOnlineNotifications = value;
                    OnPropertyChanged("IsEnableOnlineNotifications");
                }
            }
        }

        public bool IsFavorite
        {
            //*************************************************************************************************************************************************
            // Check contact is exist in Favorite or not?
            //*************************************************************************************************************************************************
            get
            {
                return this._isFavorite;
            }
            set
            {
                if (this._isFavorite != value)
                {
                    this._isFavorite = value;
                    OnPropertyChanged("IsFavorite");
                }
            }
        }

        public bool IsStatusUpdated { get; set; }


        public bool IsUpdated { get; set; }

        public string Lastname
        {
            get
            {
                return this._lastname;
            }
            set
            {
                this._lastname = value;
                OnPropertyChanged("Lastname");
            }
        }


        public DateTime? LastSeen
        {
            get
            {
                return this._lastSeen;
            }
            set
            {
                DateTime? lastSeen = this._lastSeen;
                DateTime? curValue = value;
                if ((lastSeen.HasValue != curValue.HasValue) || (lastSeen.HasValue && (lastSeen.GetValueOrDefault() != curValue.GetValueOrDefault())))
                {
                    this._lastSeen = value;
                    OnPropertyChanged("LastSeen");
                    OnPropertyChanged("LastActivity");
                }
            }
        }

        public string Middlename
        {
            get
            {
                return this._middlename;
            }
            set
            {
                this._middlename = value;
                OnPropertyChanged("Middlename");
            }
        }

        public string MobilePhone
        {
            get
            {
                return this._mobilePhone;
            }
            set
            {
                this._mobilePhone = value;
                OnPropertyChanged("MobilePhone");
            }
        }

        public string SipUsername
        {
            get
            {
                return this._sipUsername;
            }
            set
            {
                this._sipUsername = value;
                OnPropertyChanged("SipUsername");
            }
        }

        public bool OnlineNotification
        {
            get
            {
                return this._onlineNotification;
            }
            set
            {
                if (this._onlineNotification != value)
                {
                    this._onlineNotification = value;
                    OnPropertyChanged("OnlineNotification");
                }
            }
        }
        

        public UserStatus Status
        {
            get
            {
                return this._status;
            }
            set
            {
                if (this._status != value)
                {
                    this._status = value;
                    OnPropertyChanged("Status");
                }
            }
        }
        
        public uint UnreadMsgCount
        {
            get
            {
                return this._unreadMsgCount;
            }
            set
            {
                this._unreadMsgCount = value;
                OnPropertyChanged("UnreadMsgCount");
                OnPropertyChanged("HasUnreadMsg");
            }
        }
    }
}
