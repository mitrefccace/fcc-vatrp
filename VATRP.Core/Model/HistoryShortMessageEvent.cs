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
    public class VATRPMessageEvent : VATRPHistoryEvent, IComparable<VATRPMessageEvent>
    {
        private string content;
        private bool? delivered;
        private string _chatID;
        private bool _seen;
        private string cost; 
        public VATRPMessageEvent()
            :this(null, null, null)
        {
            delivered = null;
        }

        public VATRPMessageEvent(string chatId, string localParty, string remoteParty)
            : base(localParty, remoteParty)
        {
            _chatID = chatId;
            delivered = null;
        }

        // Internal column for the associated chat GUID value
        public string ChatID
        {
            get { return _chatID; }
            set
            {
                _chatID = value;
                NotifyPropertyChanged("ChatID");
            }
        }

      
        public String Content
        {
            get { return this.content; }
            set
            {
                this.content = value;
                NotifyPropertyChanged("Content");
            }
        }        

        public bool? IsDelivered
        {
            get { return this.delivered; }
            set
            {
                if (delivered != value)
                {
                    this.delivered = value;
                    NotifyPropertyChanged("IsDelivered");
                }
            }
        }

        public bool Seen
        {
            get { return this._seen; }
            set
            {
                this._seen = value;
                NotifyPropertyChanged("Seen");
            }
        }

        public DateTime Date
        {
            get { return this._date; }
            set
            {
                this._date = value;
                NotifyPropertyChanged("Date");
            }
        }

        public StatusType Status
        {
            get { return this._status; }
            set
            {
                this._status = value;
                NotifyPropertyChanged("Status");
            }
        }

        public String Cost
        {
            get { return this.cost; }
            set
            {
                this.cost = value;
                NotifyPropertyChanged("Cost");
            }
        }

        #region IComparable implementaion

        public int CompareTo(VATRPMessageEvent other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            return other._date.CompareTo(this._date);
        }

        #endregion
        
        
    }
}
