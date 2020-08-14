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

using com.vtcsecure.ace.windows.Enums;
using com.vtcsecure.ace.windows.Services;
using Microsoft.Win32;
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
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{
    /// <summary>
    /// Interaction logic for UnifiedSettingsGeneralCtrl.xaml
    /// </summary>
    public partial class UnifiedSettingsGeneralCtrl : BaseUnifiedSettingsPanel
    {
        public CallViewCtrl CallControl;

        public UnifiedSettingsGeneralCtrl()
        {
            InitializeComponent();
            Title = "General";
            this.Loaded += UnifiedSettingsGeneralCtrl_Loaded;
        }

        // ToDo - VATRP98populate when we know where the settings are stored
        private void UnifiedSettingsGeneralCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        public override void Initialize()
        {

            //************************************************************************************************************************************
            // Initilize of More==>Settings==>General
            //************************************************************************************************************************************
            base.Initialize();
            // intialize start on boot:
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string applicationName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            if (registryKey.GetValue(applicationName) == null)
            {
                // the application is not set to run at startup
                StartAtBootCheckbox.IsChecked = false;
            }
            else
            {
                // the application is set to run at startup
                StartAtBootCheckbox.IsChecked = true;
            }

            bool autoAnswerEnabled = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                    Configuration.ConfEntry.AUTO_ANSWER, false);
            AutoAnswerAfterNotificationCheckBox.IsChecked = autoAnswerEnabled;
            bool privacyEnabled = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                    Configuration.ConfEntry.PRIVACY_ENABLED, false);
            PrivacyCheckBox.IsChecked = privacyEnabled;

            if (App.CurrentAccount == null)
                return;
            string transport = App.CurrentAccount.Transport;
            if (!string.IsNullOrEmpty(transport) && !transport.Equals("TLS"))  // unencrypted, tls = encrypted
            {
                SipEncryptionCheckbox.IsChecked = false;
                SipEncryptionValueLabel.Content = "Disabled";
            }
            else
            {
                SipEncryptionCheckbox.IsChecked = true;
                SipEncryptionValueLabel.Content = "Enabled";
            }

            MuteMicrophoneCheckBox.IsChecked = App.CurrentAccount.MuteMicrophone;
            MuteSpeakerCheckBox.IsChecked = App.CurrentAccount.MuteSpeaker;
            EchoCancelCheckBox.IsChecked = App.CurrentAccount.EchoCancel;
            ShowSelfViewCheckBox.IsChecked = App.CurrentAccount.ShowSelfView;
        }

        public override void ShowAdvancedOptions(bool show)
        {
            base.ShowAdvancedOptions(show);
            System.Windows.Visibility visibleSetting = System.Windows.Visibility.Collapsed;
            if (show)
            {
                visibleSetting = System.Windows.Visibility.Visible;
            }
//            VideoMailUriLabel.Visibility = visibleSetting;
//            VideoMailUriTextBox.Visibility = visibleSetting;
        }

        public override void ShowSuperOptions(bool show)
        {
            base.ShowSuperOptions(show);

            // 1170-ready: this is specified as android only. is implemented for windows.
            StartAtBootLabel.Visibility = BaseUnifiedSettingsPanel.VisibilityForSuperSettingsAsPreview;
            StartAtBootCheckbox.Visibility = BaseUnifiedSettingsPanel.VisibilityForSuperSettingsAsPreview;

            // this is specified as android only.
            WifiOnlyLabel.Visibility = System.Windows.Visibility.Collapsed;
            WifiOnlyCheckBox.Visibility = System.Windows.Visibility.Collapsed;

            // this is specified for android and ios. Implemented.
            AutoAnswerAfterNotificationCheckBox.Visibility = BaseUnifiedSettingsPanel.VisibilityForSuperSettingsAsPreview;
            AutoAnswerAfterNotificationLabel.Visibility = BaseUnifiedSettingsPanel.VisibilityForSuperSettingsAsPreview;
                
        }

        /// <summary>
        /// Start a Boot setting changed from More==>Settings==>General
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStartOnBoot(object sender, RoutedEventArgs e)
        {
            string applicationName = "VATRP";

            bool enabled = this.StartAtBootCheckbox.IsChecked ?? false;
            if (enabled)
            {
                string startupPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    key.SetValue(applicationName, "\"" + startupPath + "\"");
                }
            }
            else
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    key.DeleteValue(applicationName, false);
                }
            }
        }

        private void OnWifiOnly(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Auto Answer Call Clicked");
            bool enabled = WifiOnlyCheckBox.IsChecked ?? false;
            // placeholder - not yet indicated for windows
        }
        private void OnSipEncryption(object sender, RoutedEventArgs e)
        {

            //************************************************************************************************************************************
            //SIP Encryption setting changed from More==>Settings==>General
            //************************************************************************************************************************************
            Console.WriteLine("SIP Encryption Clicked");
            if (App.CurrentAccount == null)
                return;
            bool enabled = SipEncryptionCheckbox.IsChecked ?? false;
            bool changed = false;
            if (!enabled)  // unencrypted = "TCP", tls = encrypted
            {
                if (!App.CurrentAccount.Transport.Equals("TCP"))
                {
                    App.CurrentAccount.Transport = "TCP";
                    App.CurrentAccount.HostPort = 5060;
                    changed = true;
                }
            }
            else
            {
                if (!App.CurrentAccount.Transport.Equals("TLS"))
                {
                    App.CurrentAccount.Transport = "TLS";
                    App.CurrentAccount.HostPort = 5061;
                    changed = true;
                }
            }
            if (changed)
            {
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.RegistrationChanged);
            }
        }

        /// <summary>
        /// Auto Answer After Notification setting changed from More==>Settings==>General
        /// </summary>
        /// <param name="sender">Default callback argument</param>
        /// <param name="e">Default callback argument</param>
        private void OnAutoAnswerAfterNotification(object sender, RoutedEventArgs e)
        {
            bool enabled = AutoAnswerAfterNotificationCheckBox.IsChecked ?? false;
            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                 Configuration.ConfEntry.AUTO_ANSWER, enabled);
            ServiceManager.Instance.ConfigurationService.SaveConfig();
        }

        public override void UpdateForMenuSettingChange(ACEMenuSettingsUpdateType menuSetting)
        {
            if (App.CurrentAccount == null)
                return;

            switch (menuSetting)
            {
                case ACEMenuSettingsUpdateType.MuteMicrophoneMenu: MuteMicrophoneCheckBox.IsChecked = App.CurrentAccount.MuteMicrophone;
                    break;
                case ACEMenuSettingsUpdateType.MuteSpeakerMenu: MuteSpeakerCheckBox.IsChecked = App.CurrentAccount.MuteSpeaker;
                    break;
                case ACEMenuSettingsUpdateType.ShowSelfViewMenu: ShowSelfViewCheckBox.IsChecked = App.CurrentAccount.ShowSelfView;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Mute Microphone setting changed from More==>Settings==>General
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMuteMicrophone(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;
            Console.WriteLine("Mute Microphone Clicked");
            bool enabled = MuteMicrophoneCheckBox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.MuteMicrophone)
            {
                App.CurrentAccount.MuteMicrophone = enabled;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();

                if ((CallControl != null) && CallControl.IsLoaded)
                {
                    CallControl.UpdateMuteSettingsIfOpen();
                }
            }
        }

        /// <summary>
        /// Mute Speaker setting changed from More==>Settings==>General
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMuteSpeaker(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;
            Console.WriteLine("Mute Speaker Clicked");
            bool enabled = MuteSpeakerCheckBox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.MuteSpeaker)
            {
                App.CurrentAccount.MuteSpeaker = enabled;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();

                if ((CallControl != null) && CallControl.IsLoaded)
                {
                    CallControl.UpdateMuteSettingsIfOpen();
                }
            }
        }

        /// <summary>
        /// Echo Cancel setting changed from More==>Settings==>General
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEchoCancel(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;
            Console.WriteLine("Echo Cancel Call Clicked");
            bool enabled = this.EchoCancelCheckBox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.EchoCancel)
            {
                App.CurrentAccount.EchoCancel = enabled;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();
            }
        }

        private void OnShowSelfView(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;
            Console.WriteLine("Show Self View Clicked");
            if (App.CurrentAccount == null)
                return;
            bool enable = this.ShowSelfViewCheckBox.IsChecked ?? true;
            if (enable != App.CurrentAccount.ShowSelfView)
            {
                App.CurrentAccount.ShowSelfView = enable;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();

                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.ShowSelfViewChanged);

            }
        }

        /// <summary>
        /// High Contrast setting changed from More==>Settings==>General
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHighContrast(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Coming Soon: High Contrast Theme");
            if (HighContrastCheckBox.IsChecked ?? false)
            {
                System.Windows.MessageBox.Show("Coming soon", "VATRP", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Checkbox callback handler for privacy settings changed from More==>Settings==>General.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPrivacyCheck(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;
            bool enabled = this.PrivacyCheckBox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.EnablePrivacy)
            {
                App.CurrentAccount.EnablePrivacy = enabled;
                ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                     Configuration.ConfEntry.PRIVACY_ENABLED, enabled);
            }
        }

        public void disableElements()
        {
            MuteSpeakerCheckBox.IsEnabled = false;
            MuteMicrophoneCheckBox.IsEnabled = false;
            EchoCancelCheckBox.IsEnabled = false;
            ShowSelfViewCheckBox.IsEnabled = false;
            SipEncryptionCheckbox.IsEnabled = false;
            SipEncryptionCheckbox.Visibility = Visibility.Hidden;
            SipEncryptionValueLabel.Visibility = Visibility.Visible;
        }
    }
}
