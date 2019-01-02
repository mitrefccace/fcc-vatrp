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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VATRP.Core.Enums;
using VATRP.Core.Extensions;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class HistoryCallEventViewModel : ViewModelBase, IComparable<HistoryCallEventViewModel>
    {
        #region Members

        private VATRPCallEvent _callEvent;
        private VATRPContact _contact;
        private SolidColorBrush _backColor;
        private string _displayName;
        private ImageSource _avatar;
        private ImageSource _callStateIndicator;
        private int _statusWidth;
        private string _callDate;
        private string _duration;
        private bool _allowAddContact;
        
        #endregion

        #region Constructor
        public HistoryCallEventViewModel()
        {
            _displayName = string.Empty;
            _avatar = null;
            _callStateIndicator = null;
            _statusWidth = 16;
            _allowAddContact = true;
        }

        public HistoryCallEventViewModel(VATRPCallEvent callEvent, VATRPContact contact)
            : this()
        {
            this._callEvent = callEvent;
            this._backColor = callEvent.Status == VATRPHistoryEvent.StatusType.Missed ? new SolidColorBrush(Color.FromArgb(255, 0xFE, 0xCD, 0xCD)) : new SolidColorBrush(Color.FromArgb(255, 0xE9, 0xEF, 0xE9));

            UpdateContact(contact);
            LoadCallStateIndicator();

            DateTime callTime =
                VATRP.Core.Model.Utils.Time.ConvertUtcTimeToLocalTime(
                    VATRP.Core.Model.Utils.Time.ConvertDateTimeToLong(callEvent.StartTime) / 1000);
            string dateFormat = "d/MM h:mm tt";
            var diffTime = DateTime.Now - callTime;
            if (diffTime.Days == 0)
                dateFormat = "h:mm tt";
            else if (diffTime.Days < 8)
                dateFormat = "ddd h:mm tt";
            else if (diffTime.Days > 365)
                dateFormat = "dd/MM/yy h:mm tt";  //Added YY in date format by MK on dated 02-NOV-2016 TO DISPLAY YEAR IN CALL DATE

            CallDate = callTime.ToString(dateFormat);
        }
        #endregion

        #region Methods

        internal void UpdateContact(VATRPContact newContact)
        {
            if (_contact != null && newContact == null)
            {
                _contact.PropertyChanged -= OnContactPropertyChanged;
            }

            _contact = newContact;
            _callEvent.Contact = newContact;
            if (_contact != null)
            {
                _contact.PropertyChanged += OnContactPropertyChanged;
                if (_contact.IsLinphoneContact)
                    _allowAddContact = false;
            }
            LoadContactAvatar();
        }
		
        private void LoadContactAvatar()
        {
            if (_contact != null && _contact.Avatar.NotBlank())
            {
                try
                {
                    byte[] data = File.ReadAllBytes(_contact.Avatar);
                    var source = new BitmapImage();
                    source.BeginInit();
                    source.StreamSource = new MemoryStream(data);
                    source.EndInit();

                    Avatar = source;
                    // use public setter
                }
                catch (Exception ex)
                {
                    LoadCommonAvatar();
                }
            }
            else
            {
                LoadCommonAvatar();
            }
        }

        private void LoadCallStateIndicator()
        {
            try
            {
                var uriString = "pack://application:,,,/VATRP;component/Resources/incoming.png";
                if (_callEvent != null)
                    switch (_callEvent.Status)
                    {
                        case VATRPHistoryEvent.StatusType.Failed:
                        case VATRPHistoryEvent.StatusType.Outgoing:
                            uriString = "pack://application:,,,/VATRP;component/Resources/outgoing.png";
                            break;
                        case VATRPHistoryEvent.StatusType.Incoming:
                            break;
                        case VATRPHistoryEvent.StatusType.Missed:
                            uriString = "pack://application:,,,/VATRP;component/Resources/missed.png";
                            _statusWidth = 26;
                            break;
                    }

                CallStatusIndicator = new BitmapImage(new Uri(uriString));
                
            }
            catch (Exception ex)
            {

            }
        }

        private void LoadCommonAvatar()
        {
            var avatarUri = "pack://application:,,,/VATRP;component/Resources/male.png";
            if (_contact != null && _contact.Gender.ToLower() == "female")
                avatarUri = "pack://application:,,,/VATRP;component/Resources/female.png";
            try
            {
                Avatar = new BitmapImage(new Uri(avatarUri));
                // use public setter
            }
            catch (Exception ex)
            {

            }
        }

        #endregion
        
        #region Events

        private void OnContactPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string propertyName = e.PropertyName ?? "";
            if (propertyName == "ContactName_ForUI")
            {
                OnPropertyChanged("DisplayName");
            }
            else if (propertyName == "ContactAddress_ForUI")
            {
                if (_contact != null && CallEvent.RemoteParty.TrimSipPrefix() != _contact.ID)
                {
                    UpdateContact(null);
                    OnPropertyChanged("DisplayName");
                }
            }
            else if (propertyName == "Avatar")
            {
                LoadContactAvatar();
            }
        }

        #endregion

        #region Properties
        public SolidColorBrush CallEventStateBrush
        {
            get { return _backColor; }
            set
            {
                _backColor = value;
                OnPropertyChanged("CallEventStateBrush");
            }
        }

        public DateTime SortDate
        {
            get
            {
                var vatrpCallEvent = this._callEvent;
                if (vatrpCallEvent != null) 
                    return vatrpCallEvent.StartTime;
                return DateTime.Now;
            }
        }
        public VATRPCallEvent CallEvent
        {
            get { return this._callEvent; }
            set
            {
                this._callEvent = value;
                OnPropertyChanged("CallEvent");
            }
        }

        public VATRPContact Contact
        {
            get { return this._contact; }
            set { this._contact = value; }
        }

        public string DisplayName
        {
            get
            {
                var vatrpCallEvent = this._callEvent;
                if (vatrpCallEvent != null) 
                    return vatrpCallEvent.DisplayName;
                return string.Empty;
            }
        }

        public ImageSource Avatar
        {
            get { return _avatar; }
            set
            {
                _avatar = value;
                OnPropertyChanged("Avatar");
            }
        }

        public ImageSource CallStatusIndicator
        {
            get { return _callStateIndicator; }
            set
            {
                _callStateIndicator = value;
                OnPropertyChanged("CallStatusIndicator");
            }
        }

        public int StatusImageWidth
        {
            get { return _statusWidth; }
            set
            {
                _statusWidth = value;
                OnPropertyChanged("StatusImageWidth");
            }
        }

        public string CallDate
        {
            get { return _callDate; }
            set
            {
                _callDate = value; 
                OnPropertyChanged("CallDate");
            }
        }

        public string Duration
        {
            get
            {
                if (CallEvent == null || CallEvent.Duration == -1)
                {
                    return "0m 0s";
                }
                int hours = 0, minutes = 0, seconds = 0;
                if (CallEvent.Duration > 0)
                {
                    seconds = CallEvent.Duration % 60;
                    minutes = CallEvent.Duration / 60;
                    hours = CallEvent.Duration / 3600;
                }

                if (hours > 0)
                {
                    minutes %= 60;
                    return string.Format("{0}h {1:00}m {2:00}s", hours, minutes, seconds);
                }

                if (minutes > 0)
                {
                    return string.Format("{0}m {1:00}s", minutes, seconds);
                }

                return string.Format("0m {0}s", seconds);
            }
        }

        public bool AllowAddContact
        {
            get { return _allowAddContact; }
            set
            {
                _allowAddContact = value;
                OnPropertyChanged("AllowAddContact");
            }
        }

        #endregion
        
        #region IComparable interface

        public int CompareTo(HistoryCallEventViewModel other)
        {
            if (other == null)
            {
                return -1;
            }

            return (this.CallEvent).CompareTo(other.CallEvent);
        }
        
        #endregion
    }
}

