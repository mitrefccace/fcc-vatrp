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
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Enums;
using VATRP.Core.Events;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using VATRP.LinphoneWrapper.Enums;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class LocalContactViewModel : ViewModelBase
    {
        private VATRPContact _contact;
        private IContactsService _contactService;
        private LinphoneRegistrationState _registrationState;
        private int _videoMailCount;

        public LocalContactViewModel()
        {
            _videoMailCount = 0;
            _registrationState = LinphoneRegistrationState.LinphoneRegistrationCleared;
        }

        public LocalContactViewModel(IContactsService contactSvc)
        {
            this._contactService = contactSvc;
            this._contactService.LoggedInContactUpdated += OnLocalContactChanged;
        }

        private void OnLocalContactChanged(object sender, VATRP.Core.Events.ContactEventArgs e)
        {
            if (ServiceManager.Instance.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke((Action)(() => this.OnLocalContactChanged(sender, e)));
                return;
            }
            Contact = this._contactService.FindContact(e.Contact);
        }

        public VATRPContact Contact
        {
            get { return this._contact; }
            set
            {
                this._contact = value; 
                OnPropertyChanged("Contact");
            }
        }

        public LinphoneRegistrationState RegistrationState
        {
            get { return _registrationState; }
            set
            {
                _registrationState = value;
                OnPropertyChanged("RegistrationState");
            }
        }

        public int VideoMailCount
        {
            get { return _videoMailCount; }
            set
            {
                _videoMailCount = value;
                OnPropertyChanged("VideoMailCount");
            }
        }
    }
}

