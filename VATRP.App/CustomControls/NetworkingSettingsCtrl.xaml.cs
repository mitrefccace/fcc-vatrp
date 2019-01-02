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
    /// Interaction logic for NetworkSettingsCtrl.xaml
    /// </summary>
    public partial class NetworkSettingsCtrl : ISettings
    {
        public NetworkSettingsCtrl()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;
            EnableStunCheckBox.IsChecked = App.CurrentAccount.EnableSTUN;
            StunHostnameBox.Text = App.CurrentAccount.STUNAddress;
            StunHostPortBox.Text = App.CurrentAccount.STUNPort.ToString();
        }

        #region ISettings

        public bool IsChanged()
        {
            if (App.CurrentAccount == null)
                return false;

            var enabled = EnableStunCheckBox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.EnableSTUN)
                return true;

            if (StunHostnameBox.Text != App.CurrentAccount.STUNAddress)
                return true;
            ushort port = 0;
            ushort.TryParse(StunHostPortBox.Text, out port);

            if (port != App.CurrentAccount.STUNPort && port != 0)
                return true;

            return false;
        }

        public bool Save()
        {
            if (App.CurrentAccount == null)
                return false;
            var stunEnabled = EnableStunCheckBox.IsChecked ?? false;

            if (!stunEnabled)
                return true;

            if (string.IsNullOrWhiteSpace(StunHostnameBox.Text))
            {
                MessageBox.Show("Incorrect STUN address", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            
            ushort port = 0;

            ushort.TryParse(StunHostPortBox.Text, out port);
            if ( port < 1 || port > 65535)
            {
                MessageBox.Show("Incorrect STUN port", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            App.CurrentAccount.STUNAddress = StunHostnameBox.Text;
            App.CurrentAccount.STUNPort = port;
            App.CurrentAccount.EnableSTUN = stunEnabled;
            return true;
        }

        #endregion
    }
}
