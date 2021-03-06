﻿#region copyright
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
using com.vtcsecure.ace.windows.Enums;
using com.vtcsecure.ace.windows.Interfaces;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.ViewModel;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.CustomControls
{
    /// <summary>
    /// Interaction logic for MainSettingsCtrl.xaml
    /// </summary>
    public partial class MainSettingsCtrl
    {

        #region Events
        public delegate void SettingsChangeDelegate(VATRPSettings settingsType);
        public event SettingsChangeDelegate SipSettingsChangeClicked;
        public event SettingsChangeDelegate CodecSettingsChangeClicked;
        public event SettingsChangeDelegate MultimediaSettingsChangeClicked;
        public event SettingsChangeDelegate NetworkSettingsChangeClicked;
        public event SettingsChangeDelegate CallSettingsChangeClicked;
        #endregion

        public MainSettingsCtrl()
        {
            InitializeComponent();
        }

        private void OnSipSettings(object sender, RoutedEventArgs e)
        {
            if (SipSettingsChangeClicked != null)
                SipSettingsChangeClicked(VATRPSettings.VATRPSettings_SIP);
        }

        private void OnCodecSettings(object sender, RoutedEventArgs e)
        {
            if (CodecSettingsChangeClicked != null)
                CodecSettingsChangeClicked(VATRPSettings.VATRPSettings_Codec);
        }

        private void OnMultimediaSettings(object sender, RoutedEventArgs e)
        {
            if (MultimediaSettingsChangeClicked != null) 
                MultimediaSettingsChangeClicked(VATRPSettings.VATRPSettings_Multimedia);
        }

        private void OnNetworkSettings(object sender, RoutedEventArgs e)
        {
            if (NetworkSettingsChangeClicked != null) 
                NetworkSettingsChangeClicked(VATRPSettings.VATRPSettings_Network);
        }

        private void OnCallSettings(object sender, RoutedEventArgs e)
        {
            if (CallSettingsChangeClicked != null)
                CallSettingsChangeClicked(VATRPSettings.VATRPSettings_Test);
        }

    }
}
