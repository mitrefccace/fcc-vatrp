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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using com.vtcsecure.ace.windows.Interfaces;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.CustomControls
{
    /// <summary>
    /// Interaction logic for ProviderLoginScreen.xaml
    /// </summary>
    public partial class SipSettingsCtrl : ISettings
    {
        public SipSettingsCtrl()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;
            var config = ServiceManager.Instance.ConfigurationService;
            if (config == null)
                return;
            AuthIDBox.Text = App.CurrentAccount.AuthID;
            LoginBox.Text = App.CurrentAccount.Username;
            PasswdBox.Password = App.CurrentAccount.Password;
            HostnameBox.Text = App.CurrentAccount.ProxyHostname;
            HostPortBox.Text = App.CurrentAccount.ProxyPort.ToString();
            foreach (var item in TransportBox.Items)
            {
                var s = item as TextBlock;
                if (s != null && s.Text.Contains(App.CurrentAccount.Transport))
                {
                    TransportBox.SelectedItem = item;
                    break;
                }
            }
            
        }

        #region ISettings

        public bool IsChanged()
        {
            if (App.CurrentAccount == null)
                return false;
            if (AuthIDBox.Text != App.CurrentAccount.AuthID)
                return true;

            if (LoginBox.Text != App.CurrentAccount.Username)
                return true;
            if (PasswdBox.Password != App.CurrentAccount.Password)
                return true;
            if (HostnameBox.Text != App.CurrentAccount.ProxyHostname)
                return true;
            ushort port = 0;
            ushort.TryParse(HostPortBox.Text, out port);

            if (port != App.CurrentAccount.ProxyPort && port != 0)
                return true;

            var s = TransportBox.SelectedItem as TextBlock;
            if (s != null && !s.Text.Contains(App.CurrentAccount.Transport))
            {
                return true;
            }

            return false;
        }
        
        private string GetTransport(String s){
            if(s == "Unencrypted (TCP)"){
                return "TCP";
            }
            
            if(s == "Encrypted (TLS"){
                return "TLS";
            }
            return "TCP";
        }

        public bool Save()
        {
            if (App.CurrentAccount == null)
                return false;
            
            if (string.IsNullOrWhiteSpace(LoginBox.Text))
            {
                MessageBox.Show("Incorrect login", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(PasswdBox.Password))
            {
                MessageBox.Show("Empty password is not allowed", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(HostnameBox.Text))
            {
                MessageBox.Show("Incorrect SIP Server Address", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            
            ushort port = 0;

            ushort.TryParse(HostPortBox.Text, out port);
            if (port < 1 || port > 65535)
            {
                MessageBox.Show("Incorrect SIP Server Port", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            App.CurrentAccount.ProxyPort = port;
            App.CurrentAccount.AuthID = AuthIDBox.Text;
            App.CurrentAccount.Username = LoginBox.Text;
            App.CurrentAccount.Password = PasswdBox.Password;
            App.CurrentAccount.ProxyHostname = HostnameBox.Text;
            App.CurrentAccount.RegistrationUser = LoginBox.Text;
            App.CurrentAccount.RegistrationPassword = PasswdBox.Password;
            var s = TransportBox.SelectedItem as TextBlock;
            if (s != null )
            {
                App.CurrentAccount.Transport = GetTransport(s.Text);
            }
            return true;
        }

        #endregion
    }
}
