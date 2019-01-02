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
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using com.vtcsecure.ace.windows.Enums;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Model;
using VATRP.LinphoneWrapper;
using VATRP.LinphoneWrapper.Enums;
using VATRP.LinphoneWrapper.Structs;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private bool _isSipsettingsEnabled;
        private bool _isCodecSettingsEnabled;
        private bool _isNetworkSettingsEnabled;
        private bool _isMultimediaSettingsEnabled;
        private bool _isTestingSettingsEnabled;
        private int _selectedPage;

        public SettingsViewModel()
        {
            Reset();
        }

        #region Properties

        public bool SipSettingsChanged { get; set; }
        public bool NetworkSettingsChanged { get; set; }
        public bool CodecSettingsChanged { get; set; }
        public bool CallSettingsChanged { get;  set; }
        public bool MediaSettingsChanged { get; set; }

        public int SelectedPage
        {
            get { return _selectedPage; }
            set
            {
                _selectedPage = value;
                OnPropertyChanged("SelectedPage");
            }
        }

        #endregion

        
        internal void Reset()
        {
            CodecSettingsChanged = false;
            NetworkSettingsChanged = false;
            SipSettingsChanged = false;
            CallSettingsChanged = false;
            MediaSettingsChanged = false;
        }

        internal void SetActiveSettings(Enums.VATRPSettings settingsType)
        {
            Reset();
            switch (settingsType)
            {
                case VATRPSettings.VATRPSettings_SIP:
                    SelectedPage = 0;
                    break;
                case VATRPSettings.VATRPSettings_Codec:
                    SelectedPage = 2;
                    break;
                case VATRPSettings.VATRPSettings_Multimedia:
                    SelectedPage = 3;
                    break;
                case VATRPSettings.VATRPSettings_Network:
                    SelectedPage = 1;
                    break;
                case VATRPSettings.VATRPSettings_Test:
                    SelectedPage = 4;
                    break;
                default:
                    SelectedPage = -1;
                    break;
            }
        }
    }
}