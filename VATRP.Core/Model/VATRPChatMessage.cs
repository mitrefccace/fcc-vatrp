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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using VATRP.Core.Enums;
using VATRP.Core.Extensions;
using VATRP.Core.Model.Utils;
using VATRP.LinphoneWrapper.Enums;

namespace VATRP.Core.Model
{
    public class VATRPChatMessage : INotifyPropertyChanged, IComparable<VATRPChatMessage>, ICloneable
    {
        protected VATRPChat _chatID;
        protected string _content;
        protected bool _isSeparator;
        protected string _fileName;
        protected bool _isHeadingMessage;
        protected bool _isIncompleteMessage;
        protected LinphoneChatMessageState _status;
        protected DateTime _time;
        private MessageDirection _direction;
        private bool _isRead;
        private bool _isRTTMessage;
        private bool _isRTTStartMarker;
        private bool _isRTTEndMarker;
        private bool _isRTTMarker;
        private bool _isDeclineMessage;

        public static string DECLINE_PREFIX = "@@info@@ ";

        public VATRPChatMessage()
        {
            ID = Tools.GenerateMessageId();
            ContentType = MessageContentType.Text;
            this.MessageTime = DateTime.Now;
            _isIncompleteMessage = false;
            NativePtr = IntPtr.Zero;
            _isSeparator = false;
            _isRTTStartMarker = false;
            _isRTTEndMarker = false;
            _isRTTMarker = false;
            _isRTTMessage = false;
            _isDeclineMessage = false;
            RttCallPtr = IntPtr.Zero;
        }

        public VATRPChatMessage(MessageContentType contentType) :this()
        {
            this.ContentType = contentType;

        }

        public int CompareTo(VATRPChatMessage other)
        {
            if (other == null)
            {
                return -1;
            }
            return this.MessageTime.CompareTo(other.MessageTime);
        }

        public VATRPChat Chat
        {
            get
            {
                return this._chatID;
            }
            set
            {
                this._chatID = value;
                this.OnPropertyChanged("Chat");
            }
        }

        public string Content
        {
            get
            {
                return this._content;
            }
            set
            {
                if (_content != value)
                {
                    _content = value;
                    this.OnPropertyChanged("Content");
                }
            }
        }

        public bool ShowInList
        {
            get
            {
                return (this.Direction == MessageDirection.Incoming || !this.IsIncompleteMessage ) && !IsSeparator &&
                       !IsRTTStartMarker && !IsRTTEndMarker;
            }
        }

        public IntPtr NativePtr { get; set; }

        public MessageContentType ContentType { get; set; }
		
        public IntPtr RttCallPtr { get; set; }

        public MessageDirection Direction
        {
            get { return _direction; }
            set
            {
                _direction = value;
                OnPropertyChanged("Direction");
                OnPropertyChanged("HasDeliveryStatus");
            }
        }

        public bool IsRead
        {
            get { return _isRead; }
            set
            {
                _isRead = value; 
                OnPropertyChanged("IsRead");
            }
        }

        public string ID { get; set; }
        
        public string FileName
        {
            get
            {
                return this._fileName;
            }
            set
            {
                this._fileName = value;
                this.OnPropertyChanged("FileName");
            }
        }
        
        public bool IsHeadingMessage
        {
            get
            {
                return ((this.ContentType == MessageContentType.Info) && this._isHeadingMessage);
            }
            set
            {
                if (this.ContentType == MessageContentType.Info)
                {
                    this._isHeadingMessage = value;
                }
            }
        }
        
        public bool IsIncompleteMessage
        {
            get
            {
                return this._isIncompleteMessage;
            }
            set
            {
                this._isIncompleteMessage = value;
                OnPropertyChanged("ShowInList");
            }
        }

        public bool IsRTTMessage
        {
            get
            {
                return this._isRTTMessage;
            }
            set
            {
                this._isRTTMessage = value;
                OnPropertyChanged("IsRTTMessage");
            }
        }

        public bool IsRTTStartMarker
        {
            get
            {
                return this._isRTTStartMarker;
            }
            set
            {
                this._isRTTStartMarker = value;
                OnPropertyChanged("IsRTTStartMarker");
            }
        }

        public bool IsRTTEndMarker
        {
            get
            {
                return this._isRTTEndMarker;
            }
            set
            {
                this._isRTTEndMarker = value;
                OnPropertyChanged("IsRTTEndMarker");
            }
        }

        public bool IsRTTMarker
        {
            get
            {
                return this._isRTTMarker || _isRTTMessage;
            }
            set
            {
                this._isRTTMarker = value;
                OnPropertyChanged("IsRTTMarker");
            }
        }

        public bool IsDeclineMessage
        {
            get
            {
                return this._isDeclineMessage;
            }
            set
            {
                this._isDeclineMessage = value;
                OnPropertyChanged("IsDeclineMessage");
            }
        }

        public bool ShowRTTMarker
        {
            get
            {
                return IsRTTMarker && IsRTTMessage && Content.NotBlank();
            }
        }
        public LinphoneChatMessageState Status
        {
            get
            {
                return this._status;
            }
            set
            {
                this._status = value;
                this.OnPropertyChanged("Status");
            }
        }

        public bool IsSeparator
        {
            get
            {
                return this._isSeparator;
            }
            set
            {
                this._isSeparator = value;
                this.OnPropertyChanged("IsSeparator");
            }
        }

        public bool HasDeliveryStatus
        {
            get
            {
                return (this.Direction == MessageDirection.Outgoing) && !IsRTTMessage ;
            }
        }

        public DateTime MessageTime
        {
            get
            {
                return this._time;
            }
            set
            {
                this._time = value;
                this.OnPropertyChanged("MessageTime");
            }
        }

        public long UtcTimestamp { get; set; }

        #region INotifyPropertyChanged Interface

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region ICloneable
        
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        
        #endregion

    }
}

