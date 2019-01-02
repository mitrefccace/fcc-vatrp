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
    public class VATRPCallEvent : VATRPHistoryEvent, IComparable<VATRPCallEvent>
    {
        private DateTime startTime;
        private DateTime endTime;
        private int _duration = 0;
        private string _codecs = string.Empty;

        public VATRPCallEvent() 
            : this(null, null)
        {
            
        }

        public VATRPCallEvent(string localParty, string remoteParty)
            : base (localParty, remoteParty)
        {
            this.startTime = base._date;
            this.endTime = base._date;
        }

        public string Codec
        {
            get { return _codecs; }
            set { _codecs = value; }
        }

        public DateTime StartTime
        {
            get { return this.startTime; }
            set
            {
                this.startTime = value;
                NotifyPropertyChanged("StartTime");
            }
        }

        public DateTime EndTime
        {
            get { return this.endTime; }
            set
            {
                this.endTime = value;
                NotifyPropertyChanged("EndTime");
                TimeSpan timeSpan = this.endTime - this.startTime;
                _duration = (int)timeSpan.TotalSeconds;
                NotifyPropertyChanged("Duration");
            }
        }

        public string LocalParty
        {
            get { return this._localParty; }
            set
            {
                this._localParty = value;
                NotifyPropertyChanged("LocalParty");
            }
        }

        public string RemoteParty
        {
            get { return this._remoteParty; }
            set
            {
                this._remoteParty = value;
                NotifyPropertyChanged("RemoteParty");
            }
        }

        public string Username
        {
            get { return this._username; }
            set
            {
                this._username = value;
                NotifyPropertyChanged("Username");
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

        public int Duration
        {
            get
            {
                return _duration;
            }
        }

        public int CompareTo(VATRPCallEvent other)
        {
            if (other == null)
            {
                return -1;
            }
            return other.StartTime.CompareTo(this.StartTime);
        }
       
    }
}
