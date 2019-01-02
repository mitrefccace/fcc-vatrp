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
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VATRP.Core.Enums;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class ContactViewModel : IComparable<ContactViewModel>, INotifyPropertyChanged
    {
        private VATRPContact _contact;
        private int _viewModelID;
        private int IdIncremental;
        private bool _isSelected;
        private SolidColorBrush backColor;
        private ImageSource _avatar;
        private DateTime _lastUnreadMessageTime;

        public SolidColorBrush ContactStateBrush
        {
            get { return backColor; }
            set
            {
                backColor = value;
                OnPropertyChanged("ContactStateBrush");
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public ContactViewModel()
        {
            _avatar = null;
        }

        public ContactViewModel(VATRPContact contact)
        {
            //****************************************************************************************************
            // Setting the Contact Status color. If online then Green else Grey
            //****************************************************************************************************
            this._viewModelID = ++IdIncremental;
            this._contact = contact;

            this.backColor = contact.Status != UserStatus.Offline ? new SolidColorBrush(Color.FromArgb(255, 115, 215, 120)) : new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
            this._contact.PropertyChanged += _contact_PropertyChanged;
            LoadContactAvatar();
        }

        private void LoadContactAvatar()
        {

            //****************************************************************************************************
            // Display the contact Avatar
            //****************************************************************************************************
            if (_contact != null && !string.IsNullOrWhiteSpace(_contact.Avatar))
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

        private void LoadCommonAvatar()
        {

            //****************************************************************************************************
            // If contact image (Avatar) is not available then display common place holder for user avatar.
            // Contact Avatar are different for Male and Female
            //****************************************************************************************************
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

        private void _contact_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ContactName_ForUI")
            {
                OnPropertyChanged("ContactUI");
            }
            else if (e.PropertyName == "Avatar")
            {
                LoadContactAvatar();
            }
            else if (e.PropertyName == "HasUnreadMsg")
            {
                OnPropertyChanged("UnreadMsgCount");
            }
        }
        
        public int CompareTo(ContactViewModel other)
        {
            if (other == null)
            {
                return -1;
            }

            return ((int) this.ViewModelID).CompareTo(other.ViewModelID);
        }
        
        public VATRPContact Contact
        {
            get { return this._contact; }
            set { this._contact = value; }
        }

        public string ContactUI
        {
            get
            {
                var vatrpContact = this.Contact;
                if (vatrpContact != null) 
                    return vatrpContact.ContactName_ForUI;
                return string.Empty;
            }
        }

        public int UnreadMsgCount
        {
            get
            {
                var vatrpContact = this.Contact;
                if (vatrpContact != null)
                    return (int)vatrpContact.UnreadMsgCount;
                return 0;
            }
        }

        public DateTime LastUnreadMessageTime
        {
            get
            {
                return _lastUnreadMessageTime; 
                
            }
            set
            {
                _lastUnreadMessageTime = value;
                OnPropertyChanged("LastUnreadMessageTime");
            }
        }

        public int ViewModelID
        {
            get { return this._viewModelID; }
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

