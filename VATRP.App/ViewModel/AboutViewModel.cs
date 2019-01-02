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
using System.Windows;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class AboutViewModel : ViewModelBase
    {
        private string _appVersion;
        private string _appName;
        private string _copyright;
        private string _linphoneVersion;
        private string _domain;

        public AboutViewModel()
        {
            LoadVersion();
        }

        private void LoadVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            AppName = assembly.GetName().Name;

            var version = assembly.GetName().Version;
            AppVersion = string.Format("Version {0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);

            string linphoneLibraryVersion = VATRP.LinphoneWrapper.LinphoneAPI.linphone_core_get_version_asString();
            LinphoneLibVersion = string.Format("Core Version {0}", linphoneLibraryVersion);
            //Copyright = "Copyright 2017-2018";

            Domain  =  string.Format("CDN Domain: {0}",Services.ServiceManager.CDN_DOMAIN.ToString());
        }

        #region Properties

        public string AppName
        {
            get { return _appName; }
            set
            {
                _appName = value; 
                OnPropertyChanged("AppName");
            }
        }

        public string AppVersion
        {
            get { return _appVersion; }
            set
            {
                _appVersion = value; 
                OnPropertyChanged("AppVersion");
            }
        }

        public string LinphoneLibVersion
        {
            get { return _linphoneVersion; }
            set
            {
                _linphoneVersion = value;
                OnPropertyChanged("LinphoneVersion");
            }
        }

        public string Copyright
        {
            get { return _copyright; }
            set
            {
                _copyright = value; 
                OnPropertyChanged("Copyright");
            }
        }

        public string Domain
        {
            get { return _domain; }
            set
            {
                _domain = value;
                OnPropertyChanged("Domain");
            }
        }

        #endregion

    }
}
