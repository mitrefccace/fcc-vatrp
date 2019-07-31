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
    /// Interaction logic for UnifiedSettingsMainCtrl.xaml
    /// </summary>
    // ToDo VATRP-985: Unified Settings: Make it so that if the edit boxes are done editing the data validates/updates immediately
    public partial class UnifiedSettingsMainCtrl : BaseUnifiedSettingsPanel
    {
        public event UnifiedSettings_EnableSettings ShowSettingsUpdate;

        public UnifiedSettingsMainCtrl()
        {
            InitializeComponent();
            Title = "Settings";
            this.Loaded += UnifiedSettingsMainCtrl_Loaded;
            
        }

        void UnifiedSettingsMainCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        public override void Initialize()
        {
            base.Initialize();
            if (App.CurrentAccount != null)
            {
                UserIdTextBox.Text = App.CurrentAccount.AuthID;
                UserNameTextBox.Text = App.CurrentAccount.Username;
                PasswordTextBox.Password = App.CurrentAccount.Password;
                DomainTextBox.Text = App.CurrentAccount.ProxyHostname;
                ProxyTextBox.Text = Convert.ToString(App.CurrentAccount.HostPort);
                CardDAVServerTextBox.Text = App.CurrentAccount.CardDavServerPath;
                CardDAVRealmTextBox.Text = App.CurrentAccount.CardDavRealm;
                string transport = App.CurrentAccount.Transport;
                if (string.IsNullOrWhiteSpace(transport))
                {
                    transport = "TCP";
                }
                foreach (var item in TransportComboBox.Items)
                {
                    var tb = item as TextBlock;
                    string itemString = tb.Text;
                    if (itemString.Equals(transport))
                    {
                        TransportComboBox.SelectedItem = item;
                        TextBlock selectedItem = TransportComboBox.SelectedItem as TextBlock;
                        string test = selectedItem.Text;
                        break;
                    }
                }
                this.EnableVideoCheckBox.IsChecked = App.CurrentAccount.EnableVideo;
            }

            this.EnableRTTCheckBox.IsChecked = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.USE_RTT, true);
            this.AutoAnswerCheckBox.IsChecked = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AUTO_ANSWER, false);
            this.AvpfCheckbox.IsChecked = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AVPF_ON, true);
        }

        #region SettingsLevel
        public override void ShowDebugOptions(bool show)
        {
            System.Windows.Visibility visibleSetting = System.Windows.Visibility.Collapsed;
            if (show)
            {
                visibleSetting = System.Windows.Visibility.Visible;
            }
            DebugMenuLabel.Visibility = visibleSetting;
            AutoAnswerLabel.Visibility = visibleSetting;
            AutoAnswerCheckBox.Visibility = visibleSetting;
            ClearSettingsButton.Visibility = visibleSetting;
        }

        public override void ShowAdvancedOptions(bool show)
        {
            System.Windows.Visibility visibleSetting = System.Windows.Visibility.Collapsed;
            if (show)
            {
                visibleSetting = System.Windows.Visibility.Visible;
            }
            UserIdLabel.Visibility = visibleSetting;
            UserIdTextBox.Visibility = visibleSetting;

            ProxyLabel.Visibility = visibleSetting;
            ProxyTextBox.Visibility = visibleSetting;

            TransportLabel.Visibility = visibleSetting;
            TransportComboBox.Visibility = visibleSetting;

            OutboundProxyLabel.Visibility = visibleSetting;
            OutboundProxyCheckbox.Visibility = visibleSetting;

//            AvpfLabel.Visibility = visibleSetting;
//            AvpfCheckbox.Visibility = visibleSetting;

            PreferencesLabel.Visibility = visibleSetting;


//            EnableRTTLabel.Visibility = visibleSetting;
//            EnableRTTCheckBox.Visibility = visibleSetting;

            AudioButton.Visibility = visibleSetting;
            AudioButtonLabel.Visibility = visibleSetting;

            VideoButton.Visibility = visibleSetting;
            VideoButtonLabel.Visibility = visibleSetting;

            CallButton.Visibility = visibleSetting;
            CallButtonLabel.Visibility = visibleSetting;

            NetworkButton.Visibility = visibleSetting;
            NetworkButtonLabel.Visibility = visibleSetting;

            // not yet specified for windows
            AdvancedButton.Visibility = visibleSetting;
            AdvancedButtonLabel.Visibility = visibleSetting;

        }

        public override void ShowSuperOptions(bool show)
        {
            base.ShowSuperOptions(show);
            System.Windows.Visibility visibleSetting = System.Windows.Visibility.Collapsed;
            if (show)
            {
                visibleSetting = System.Windows.Visibility.Visible;
            }
            RunWizardButton.Visibility = visibleSetting;
            ClearSettingsButton.Visibility = visibleSetting;

            PasswordLabel.Visibility = visibleSetting;
            PasswordTextBox.Visibility = visibleSetting;

            ReleaseCoreButton.Visibility = visibleSetting;
            ClearCacheButton.Visibility = visibleSetting;
            BatteryAlertButton.Visibility = visibleSetting;

//            EnableVideoLabel.Visibility = visibleSetting;
//            EnableVideoCheckBox.Visibility = visibleSetting;

        }
        #endregion

        private bool IsTransportChanged()
        {
            if (App.CurrentAccount == null)
                return false;

            var transportText = TransportComboBox.SelectedItem as TextBlock;
            string transportString = transportText.Text;
            if ((string.IsNullOrWhiteSpace(transportString) && !string.IsNullOrWhiteSpace(App.CurrentAccount.Transport)) ||
                (!string.IsNullOrWhiteSpace(transportString) && string.IsNullOrWhiteSpace(App.CurrentAccount.Transport)))
                return true;
            if ((!string.IsNullOrWhiteSpace(transportString) && !string.IsNullOrWhiteSpace(App.CurrentAccount.Transport)) &&
                (!transportString.Equals(App.CurrentAccount.Transport)))
                return true;

            return false;
        }

        #region Settings Menu
        private void OnGeneralSettings(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("GeneralSettings Clicked");
            OnContentChanging(UnifiedSettingsContentType.GeneralContent);
        }

        private void OnAudioVideo(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("AudioVideo Clicked");
            OnContentChanging(UnifiedSettingsContentType.AudioVideoContent);
        }

        private void OnTheme(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Theme Clicked");
            OnContentChanging(UnifiedSettingsContentType.ThemeContent);
        }

        private void OnText(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("OnText Clicked");
            OnContentChanging(UnifiedSettingsContentType.TextContent);
        }

        private void OnSummary(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Summary Clicked");
            OnContentChanging(UnifiedSettingsContentType.SummaryContent);
        }
        #endregion

        #region SIP Account
        private void OnRunAssistant(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Run Assistant Clicked");
            if (MessageBox.Show("Launching the Wizard will delete any existing proxy configuration. Are you sure you want to proceed?", "Run Wizard", MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // run the wizard
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.RunWizard);
            }
        }

        private void OnClearSettings(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Clear Settings Clicked");
            if (MessageBox.Show("Launching the Wizard will delete any existing proxy configuration. Are you sure you want to proceed?", "Clear Settings", MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.ClearSettings);
            }
        }

        private void OnSaveAccount(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;
            StringBuilder errorMessage = new StringBuilder("");

            if (string.IsNullOrWhiteSpace(UserNameTextBox.Text))
            {
                errorMessage.Append("Please enter a user name.");
//                MessageBox.Show("Incorrect login", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (string.IsNullOrWhiteSpace(PasswordTextBox.Password))
            {
                errorMessage.Append("Please enter a password.");
//                MessageBox.Show("Empty password is not allowed", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (string.IsNullOrWhiteSpace(DomainTextBox.Text))
            {
                errorMessage.Append("Please enter a SIP Server Address.");
//                MessageBox.Show("Incorrect SIP Server Address", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
            }


            ushort port = 0;

            ushort.TryParse(ProxyTextBox.Text, out port);
            if (port < 1 || port > 65535)
            {
                errorMessage.Append("Please enter a valid SIP Server Port.");
//                MessageBox.Show("Incorrect SIP Server Port", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (!string.IsNullOrEmpty(errorMessage.ToString()))
            {
                MessageBox.Show(errorMessage.ToString(), "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (App.CurrentAccount != null)
            {
                bool isChanged = false;
                bool resyncContacts = false;
                if (App.CurrentAccount.HostPort != port)
                {
                    App.CurrentAccount.HostPort = port;
                    isChanged = true;
                }
                if (ValueChanged(App.CurrentAccount.AuthID, UserIdTextBox.Text))
                {
                    App.CurrentAccount.AuthID = UserIdTextBox.Text;
                    isChanged = true;
                }
                if (ValueChanged(App.CurrentAccount.Username, UserNameTextBox.Text))
                { 
                    App.CurrentAccount.Username = UserNameTextBox.Text;
                    // let the UI reflect the change.
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.UserNameChanged);
                    isChanged = true;
                }
                if (ValueChanged(App.CurrentAccount.Password, PasswordTextBox.Password))
                {
                    App.CurrentAccount.Password = PasswordTextBox.Password;
                    resyncContacts = true;
                    isChanged = true;
                }
                if (ValueChanged(App.CurrentAccount.ProxyHostname, DomainTextBox.Text))
                { 
                    App.CurrentAccount.ProxyHostname = DomainTextBox.Text;
                    isChanged = true;
                }
                if (ValueChanged(App.CurrentAccount.RegistrationPassword, PasswordTextBox.Password))
                { 
                    App.CurrentAccount.RegistrationPassword = PasswordTextBox.Password;
                    resyncContacts = true;
                    isChanged = true;
                }
                if (ValueChanged(App.CurrentAccount.RegistrationUser, UserNameTextBox.Text))
                {
                    App.CurrentAccount.RegistrationUser = UserNameTextBox.Text;
                    resyncContacts = true;
                    isChanged = true;
                }
                if (ValueChanged(App.CurrentAccount.CardDavServerPath, CardDAVServerTextBox.Text))
                {
                    App.CurrentAccount.CardDavServerPath = CardDAVServerTextBox.Text;
                    resyncContacts = true;
                }
                if (ValueChanged(App.CurrentAccount.CardDavRealm, CardDAVRealmTextBox.Text))
                {
                    App.CurrentAccount.CardDavRealm = CardDAVRealmTextBox.Text;
                    resyncContacts = true;
                }
                //App.CurrentAccount.Transport = (string)TransportValueLabel.Content; // saved when changed
                if (isChanged)
                {
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.RegistrationChanged);
                }
                if (resyncContacts)
                {
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.CardDavConfigChanged);
                }
            }
        }

        private bool ValueChanged(string oldString, string newString)
        {
            if ((!string.IsNullOrEmpty(newString) && !string.IsNullOrEmpty(oldString)) &&
                !newString.Equals(oldString))
            {
                return true;
            }
            return false;
        }

        public void OnUserNameChanged(Object sender, RoutedEventArgs args)
        {
            if ((App.CurrentAccount == null) || !this.IsVisible)
                return;
            string newUserName = UserNameTextBox.Text;
            if (string.IsNullOrEmpty(newUserName))
            {
                string oldUserName = App.CurrentAccount.Username;
                UserNameTextBox.Text = oldUserName;
            }
        }

        public void OnPasswordChanged(Object sender, RoutedEventArgs args)
        {
            if ((App.CurrentAccount == null) || !this.IsVisible)
                return;
            string newPassword = PasswordTextBox.Password;
            if (string.IsNullOrEmpty(newPassword))
            {
                string oldPassword = App.CurrentAccount.Password;
                PasswordTextBox.Password = oldPassword;
            }
        }
        public void OnDomainChanged(Object sender, RoutedEventArgs args)
        {
            if ((App.CurrentAccount == null) || !this.IsVisible)
                return;
            string newDomain = this.DomainTextBox.Text;
            if (string.IsNullOrEmpty(newDomain))
            {
                string oldDomain = App.CurrentAccount.ProxyHostname;
                this.DomainTextBox.Text = oldDomain;
            }
        }

        public void OnHostPortChanged(Object sender, RoutedEventArgs args)
        {
            if ((App.CurrentAccount == null) || !this.IsVisible)
                return;
            string newHostPort = ProxyTextBox.Text;
            if (string.IsNullOrEmpty(newHostPort))
            {
                int oldHostPort = App.CurrentAccount.HostPort;
                ProxyTextBox.Text = Convert.ToString(oldHostPort);
            }
        }

        private void OnTransportChanged(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Transport Clicked");
            if (App.CurrentAccount == null)
                return;
            if (IsTransportChanged())
            {
                var transportText = TransportComboBox.SelectedItem as TextBlock;
                string transportString = transportText.Text;
                App.CurrentAccount.Transport = transportString;
                if (transportString.ToUpper().Equals("TCP") || transportString.ToUpper().Equals("UDP"))
                {
                    App.CurrentAccount.HostPort = 5060;
                }
                else if (transportString.ToUpper().Equals("TLS"))
                {
                    App.CurrentAccount.HostPort = 25061;
                }
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.RegistrationChanged);
            }
        }

        public void OnCardDAVServerChanged(Object sender, RoutedEventArgs args)
        {
            if ((App.CurrentAccount == null) || !this.IsVisible)
                return;
            if (App.CurrentAccount.CardDavServerPath != CardDAVServerTextBox.Text)
            {
                App.CurrentAccount.CardDavServerPath = CardDAVServerTextBox.Text;
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.CardDavConfigChanged);
            }
        }

        public void OnCardDAVRealmChanged(Object sender, RoutedEventArgs args)
        {
            if ((App.CurrentAccount == null) || !this.IsVisible)
                return;
            if (App.CurrentAccount.CardDavRealm != CardDAVRealmTextBox.Text)
            {
                App.CurrentAccount.CardDavRealm = CardDAVRealmTextBox.Text;
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.CardDavConfigChanged);
            }
        }

        // ToDo VATRP-985 - Liz E. - not sure where the outbound proxy setting lives
        private void OnOutboundProxy(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Outbound Proxy Clicked");
            if (App.CurrentAccount == null)
                return;
            bool enabled = OutboundProxyCheckbox.IsChecked ?? false;
            App.CurrentAccount.UseOutboundProxy = enabled;
            ServiceManager.Instance.SaveAccountSettings();
        }
        private void OnAvpf(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("AVPF Clicked");
            bool enabled = AvpfCheckbox.IsChecked ?? false;
            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AVPF_ON, enabled);
            ServiceManager.Instance.ConfigurationService.SaveConfig();
            ServiceManager.Instance.ApplyAVPFChanges();
        }
        #endregion

        #region Preferences
        private void OnEnableVideo(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Enable Video Clicked");
            if (App.CurrentAccount != null)
            {
                bool enabled = EnableVideoCheckBox.IsChecked ?? false;
                if (App.CurrentAccount.EnableVideo != enabled)
                {
                    App.CurrentAccount.EnableVideo = enabled;
                    ServiceManager.Instance.SaveAccountSettings();
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.VideoPolicyChanged);
                }
            }                        
        }

        private void OnEnableRTT(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Enable Real Time Text Call Clicked");
            bool enabled = EnableRTTCheckBox.IsChecked ?? false;
            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.USE_RTT, enabled);
            ServiceManager.Instance.ConfigurationService.SaveConfig();
        }

        // Liz E. - the spreadsheet calls for an rtt checkbox - using that instead.
        /*
        private void OnTextPreferences(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Text Preferences Clicked");
            OnContentChanging(UnifiedSettingsContentType.TextContent);
        }
         * */

        private void OnAudioPreferences(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Audio Preferences Clicked");
            OnContentChanging(UnifiedSettingsContentType.AudioSettingsContent);
        }

        private void OnVideoPreferences(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Video Preferences Clicked");
            OnContentChanging(UnifiedSettingsContentType.VideoSettingsContent);
        }

        private void OnCallPreferences(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Call Preferences Clicked");
            OnContentChanging(UnifiedSettingsContentType.CallSettingsContent);
        }

        private void OnNetworkPreferences(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Network Preferences Clicked");
            OnContentChanging(UnifiedSettingsContentType.NetworkSettingsContent);
        }

        private void OnAdvancedPreferences(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Advanced Preferences Clicked");
            OnContentChanging(UnifiedSettingsContentType.AdvancedSettingsContent);
        }
        #endregion

        #region DebugMenu
        private void OnReleaseCore(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Release Core Clicked");
        }

        private void OnClearCache(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Clear Cache Clicked");
        }

        private void OnBatteryAlert(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Battery Alert Clicked");
        }

        private void OnAutoAnswerCall(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Auto Answer Call Clicked");
            bool enabled = AutoAnswerCheckBox.IsChecked ?? false;
            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AUTO_ANSWER, enabled);
            ServiceManager.Instance.ConfigurationService.SaveConfig();

        }
        #endregion

    }
}
