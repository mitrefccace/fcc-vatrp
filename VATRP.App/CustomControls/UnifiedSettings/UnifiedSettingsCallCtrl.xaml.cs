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

using com.vtcsecure.ace.windows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{
    /// <summary>
    /// Interaction logic for UnifiedSettingsCallCtrl.xaml
    /// </summary>
    public partial class UnifiedSettingsCallCtrl : BaseUnifiedSettingsPanel
    {
        public UnifiedSettingsCallCtrl()
        {
            InitializeComponent();
        }

        public override void Initialize()
        {
            base.Initialize();
            this.CallPrefixTextBox.Text = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL, 
                 Configuration.ConfEntry.CALL_DIAL_PREFIX, "");
            this.CallSubstituteCheckBox.IsChecked = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                 Configuration.ConfEntry.CALL_DIAL_ESCAPE_PLUS, false);

            this.SendSipInfoDTMFCheckBox.IsChecked = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                 Configuration.ConfEntry.DTMF_SIP_INFO, false);

            this.SendInbandDTMFCheckBox.IsChecked = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.DTMF_INBAND, false);
        }

        #region ShowSettingsLevel
        public override void ShowAdvancedOptions(bool show)
        {
            base.ShowAdvancedOptions(show);
            System.Windows.Visibility visibleSetting = System.Windows.Visibility.Collapsed;
            if (show)
            {
                visibleSetting = System.Windows.Visibility.Visible;
            }
        }
        public override void ShowDebugOptions(bool show)
        {
            base.ShowDebugOptions(show);
            System.Windows.Visibility visibleSetting = System.Windows.Visibility.Collapsed;
            if (show)
            {
                visibleSetting = System.Windows.Visibility.Visible;
            }

            SendSipInfoDTMFLabel.Visibility = visibleSetting;
            SendSipInfoDTMFCheckBox.Visibility = visibleSetting;

            SendInbandDTMFLabel.Visibility = visibleSetting;
            SendInbandDTMFCheckBox.Visibility = visibleSetting;
        }
        public override void ShowSuperOptions(bool show)
        {
            base.ShowSuperOptions(show);
            System.Windows.Visibility visibleSetting = System.Windows.Visibility.Collapsed;
            if (show)
            {
                visibleSetting = System.Windows.Visibility.Visible;
            }

            // 1170-ready: set for ios only currently, but implementation in place
            CallPrefixLabel.Visibility = BaseUnifiedSettingsPanel.VisibilityForSuperSettingsAsPreview;// visibleSetting;
            CallPrefixTextBox.Visibility = BaseUnifiedSettingsPanel.VisibilityForSuperSettingsAsPreview; //visibleSetting;

            // 1170-ready: set for ios only currently, but implementation in place
            CallSubstituteLabel.Visibility = BaseUnifiedSettingsPanel.VisibilityForSuperSettingsAsPreview; //visibleSetting;
            CallSubstituteCheckBox.Visibility = BaseUnifiedSettingsPanel.VisibilityForSuperSettingsAsPreview; //visibleSetting;

            // set for ios only currently
            RepeatCallNotificationCheckBox.Visibility = System.Windows.Visibility.Collapsed; //visibleSetting;
            RepeatCallNotificationLabel.Visibility = System.Windows.Visibility.Collapsed; //visibleSetting;
            // this was in the ios menu, but not listed in spreadsheet
            SendInbandDTMFLabel.Visibility = System.Windows.Visibility.Collapsed; //visibleSetting;
            SendInbandDTMFCheckBox.Visibility = System.Windows.Visibility.Collapsed; //visibleSetting;

        }
        #endregion

        private void OnCallPrefixChanged(Object sender, RoutedEventArgs args)
        {
            Console.WriteLine("Prefix Changed");
            string newCallPrefix = this.CallPrefixTextBox.Text;
            string oldCallPrefix = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.CALL_DIAL_PREFIX, "");
            if (!newCallPrefix.Equals(oldCallPrefix))
            {
                ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                    Configuration.ConfEntry.CALL_DIAL_PREFIX, newCallPrefix);
                ServiceManager.Instance.ConfigurationService.SaveConfig();
            }
        }

        private void OnCallSubstituteEscapePlus(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Call Substitute Clicked");
            bool enabled = this.CallSubstituteCheckBox.IsChecked ?? false;
            if (enabled != ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL, Configuration.ConfEntry.CALL_DIAL_ESCAPE_PLUS, false))
            {
                ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL, Configuration.ConfEntry.CALL_DIAL_ESCAPE_PLUS, enabled);
                ServiceManager.Instance.ConfigurationService.SaveConfig();
            }
        }

        private void OnSendInbandDTMF(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Send Inband DTMF Clicked");
            bool enabled = this.SendInbandDTMFCheckBox.IsChecked ?? false;
            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.DTMF_INBAND, enabled);
            ServiceManager.Instance.ConfigurationService.SaveConfig();
            ServiceManager.Instance.ApplyDtmfInbandChanges();
        }

        private void OnSendSipInfoDTMF(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Send SIP INFO DTMF Clicked");
            bool enabled = this.SendSipInfoDTMFCheckBox.IsChecked ?? false;
            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.DTMF_SIP_INFO, enabled);
            ServiceManager.Instance.ConfigurationService.SaveConfig();
            ServiceManager.Instance.ApplyDtmfOnSIPInfoChanges();
        }

        private void OnRepeatCallNotification(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Repeat Call Notification Clicked");

        }
    }
}
