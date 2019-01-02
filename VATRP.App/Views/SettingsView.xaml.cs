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
using System.Windows.Shapes;
using com.vtcsecure.ace.windows.CustomControls;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.ViewModel;

namespace com.vtcsecure.ace.windows.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView
    {
        #region Members

        private SettingsViewModel _viewModel;
        #endregion

        #region Event
        public delegate void SettingsSavedDelegate();
        public event SettingsSavedDelegate SettingsSavedEvent;

        public delegate void ResetToDefaultDelegate();
        public event ResetToDefaultDelegate ResetToDefaultEvent;

        #endregion

        public SettingsView()
            : base(VATRPWindowType.SETTINGS_VIEW)
        {
            InitializeComponent();
        }

        internal void SetSettingsModel(SettingsViewModel settingsViewModel)
        {
            _viewModel = settingsViewModel;
            DataContext = _viewModel;
        }

        private void OnSave(object sender, RoutedEventArgs e)
        {
            if (SipSettingsPage != null && SipSettingsPage.IsChanged())
            {
                if (!SipSettingsPage.Save())
                {
                    SettingsTab.SelectedIndex = 0;
                    return;
                }
                _viewModel.SipSettingsChanged = true;
            }

            if (NetworkSettingsPage != null && NetworkSettingsPage.IsChanged())
            {
                if (!NetworkSettingsPage.Save())
                {
                    SettingsTab.SelectedIndex = 1;
                    return;
                }
                _viewModel.NetworkSettingsChanged = true;
            }

            if (CodecSettingsPage != null && CodecSettingsPage.IsChanged())
            {
                if (!CodecSettingsPage.Save())
                {
                    SettingsTab.SelectedIndex = 2;
                    return;
                }
                _viewModel.CodecSettingsChanged = true;
            }

            if (CallSettingsPage != null && CallSettingsPage.IsChanged())
            {
                if (!CallSettingsPage.Save())
                {
                    SettingsTab.SelectedIndex = 4;
                    return;
                }
                _viewModel.CallSettingsChanged = true;
            }

            if (MultimediaSettingsPage != null && MultimediaSettingsPage.IsChanged())
            {
                if (!MultimediaSettingsPage.Save())
                {
                    SettingsTab.SelectedIndex = 3;
                    return;
                }
                _viewModel.MediaSettingsChanged = true;
            }
            Close();
            if (SettingsSavedEvent != null)
            {
                SettingsSavedEvent();
                _viewModel.Reset();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.CallSettingsPage.ResetToDefaultEvent += OnResetToDefaultConfiguration;
        }

        private void OnResetToDefaultConfiguration()
        {
            Close();
            if (ResetToDefaultEvent != null)
                ResetToDefaultEvent();
            _viewModel.Reset();

        }

    }
}
