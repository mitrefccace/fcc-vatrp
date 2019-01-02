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
using System.ComponentModel;

namespace VATRP.Core.Model
{
    public abstract class VATRPHistoryEvent : IComparable<VATRPHistoryEvent>, INotifyPropertyChanged
    {
        protected string _remoteParty;
        protected string _localParty;
        protected bool seen;
        protected StatusType _status;
        protected DateTime _date;
        protected string _displayName;
        protected string _username;
        protected VATRPContact _contact;
        protected string _guid;

        [Flags]
        public enum StatusType
        {
            Outgoing = 0x01 << 0,
            Incoming = 0x01 << 1,
            Missed = 0x01 << 2,
            Failed = 0x01 << 3,
            Rejected = 0x01 << 4,
            Sent = 0x01 << 5,
            Received = 0x01 << 6,
            All = Outgoing | Incoming | Missed | Failed | Rejected | Sent | Received
        }

        protected VATRPHistoryEvent()
        {
            _status = StatusType.Missed;
            _date = DateTime.Now;
            _guid = Guid.NewGuid().ToString();
        }

        protected VATRPHistoryEvent(string localParty, string remoteParty):
            this()
        {
            this._localParty = localParty;
            this._remoteParty = remoteParty;
        }

        public VATRPContact Contact
        {
            get { return this._contact; }
            set
            {
                this._contact = value;
                NotifyPropertyChanged("Contact");
                NotifyPropertyChanged("DisplayName");
            }
        }

        public string DisplayName
        {
            get
            {
                var vatrpContact = this.Contact;
                if (vatrpContact != null && vatrpContact.IsLinphoneContact && 
                    !string.IsNullOrEmpty(vatrpContact.DisplayName))
                    return this.Contact.DisplayName;

                return _displayName;
            }
            set
            {
                this._displayName = value;
                NotifyPropertyChanged("DisplayName");
            }
        }

        public string CallGuid
        {
            get { return _guid; }
            set { _guid = value; }
        }

        public int CompareTo(VATRPHistoryEvent other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            return other._date.CompareTo(this._date);
        }



        public bool ContainsFilteredData(string searchData)
        {
            if (DisplayName != null && DisplayName.Contains(searchData))
                return true;

            if (_remoteParty != null && _remoteParty.Contains(searchData))
                return true;

            return false;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify that a property changed
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
