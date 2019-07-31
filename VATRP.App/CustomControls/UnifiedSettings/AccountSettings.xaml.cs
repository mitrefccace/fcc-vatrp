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
using log4net;

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{
    /// <summary>
    /// Interaction logic for AccountSettings.xaml
    /// </summary>
    public partial class AccountSettings : BaseUnifiedSettingsPanel
    {
        private ILog LOG;
        public event UnifiedSettings_EnableSettings ShowSettingsUpdate;

        public delegate void VideoMailHandler();
        public event VideoMailHandler updateVideomail;

        public delegate void ProviderListHandler(string proxyHostName);
        public event ProviderListHandler updateProviders;

        public delegate void FocusControlHandler();
        public event FocusControlHandler ShiftFocus;

        public AccountSettings()
        {
            InitializeComponent();
            this.Loaded += AccountSettings_Loaded;
        }

        void AccountSettings_Loaded(object sender, RoutedEventArgs e)
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
                string transport = App.CurrentAccount.Transport;
                CardDAVServerTextBox.Text = App.CurrentAccount.CardDavServerPath;
                CardDAVRealmTextBox.Text = App.CurrentAccount.CardDavRealm;
                MWIUriTextBox.Text = App.CurrentAccount.MWIUri;
                ContactsURITextBox.Text = App.CurrentAccount.ContactsURI; // rmh-sep18
                CDNTextBox.Text = App.CurrentAccount.CDN;
                GeolocationTextBox.Text = App.CurrentAccount.GeolocationURI;

                MWIUriTextBox.KeyDown += new KeyEventHandler(handle_keydown);
                CardDAVServerTextBox.KeyDown += new KeyEventHandler(handle_keydown);
                ContactsURITextBox.KeyDown += new KeyEventHandler(handle_keydown);
                CDNTextBox.KeyDown += new KeyEventHandler(handle_keydown);
                GeolocationTextBox.KeyDown += new KeyEventHandler(handle_keydown);

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

                VideoMailUriTextBox.Text = App.CurrentAccount.VideoMailUri;
                LOG = LogManager.GetLogger(typeof(VATRP.Core.Services.LinphoneService));
            }
        }

        #region SettingsLevel
        public override void ShowDebugOptions(bool show)
        {
            System.Windows.Visibility visibleSetting = System.Windows.Visibility.Collapsed;
            if (show)
            {
                visibleSetting = System.Windows.Visibility.Visible;
            }
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
                // MessageBox.Show("Incorrect login", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (string.IsNullOrWhiteSpace(PasswordTextBox.Password))
            {
                errorMessage.Append("Please enter a password.");
                // MessageBox.Show("Empty password is not allowed", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (string.IsNullOrWhiteSpace(DomainTextBox.Text))
            {
                errorMessage.Append("Please enter a SIP Server Address.");
                // MessageBox.Show("Incorrect SIP Server Address", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
            }


            ushort port = 0;

            ushort.TryParse(ProxyTextBox.Text, out port);
            if (port < 1 || port > 65535)
            {
                errorMessage.Append("Please enter a valid SIP Server Port.");
                // MessageBox.Show("Incorrect SIP Server Port", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    isChanged = true;
                }
                if (ValueChanged(App.CurrentAccount.ProxyHostname, DomainTextBox.Text))
                {
                    App.CurrentAccount.ProxyHostname = DomainTextBox.Text;
                    isChanged = true;
                }
                if (ValueChanged(App.CurrentAccount.RegistrationUser, UserNameTextBox.Text))
                {
                    App.CurrentAccount.RegistrationUser = UserNameTextBox.Text;
                    isChanged = true;
                    resyncContacts = true;
                }
                if (ValueChanged(App.CurrentAccount.RegistrationPassword, PasswordTextBox.Password))
                {
                    App.CurrentAccount.RegistrationPassword = PasswordTextBox.Password;
                    isChanged = true;
                    resyncContacts = true;
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
                if (ValueChanged(App.CurrentAccount.ContactsURI, ContactsURITextBox.Text))
                {
                    App.CurrentAccount.ContactsURI = ContactsURITextBox.Text;
                    isChanged = true;
                }
                // App.CurrentAccount.Transport = (string)TransportValueLabel.Content; // saved when changed
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
                ushort oldHostPort = App.CurrentAccount.HostPort;
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

        /// <summary>
        /// Saves the Contacts URI when the user clicks out of it
        /// </summary>
        /// <remarks>
        /// When the user changes the contacts URI, this method verifies the URI is valid. If the URI 
        /// is valid, that URI will be saved to the Current Account, otherwise an error message will
        /// be displayed requesting a valid URI and the Account's URI will not be changed.
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns>void
        /// </returns>
        public void OnContactsURIChanged(Object sender, RoutedEventArgs args)
        {
            Console.WriteLine("Contacts URI Changed");
            if ((App.CurrentAccount == null) || !this.IsVisible)
                return;

            string uri = ContactsURITextBox.Text;
            try
            {
                if (App.CurrentAccount.ContactsURI != uri)
                {
                    if (!ValidURI(uri))
                    {
                        return;
                    }
                    App.CurrentAccount.ContactsURI = uri;
                    ServiceManager.Instance.SaveAccountSettings();
                }
            }
            catch (Exception e)
            {
                LOG.Debug(e.ToString());
            }
        }

        /// <summary>
        /// Updates the provider drop down menu options.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns>void</returns>
        public async void OnCDNChanged(Object sender, RoutedEventArgs args)
        {
            string uri = CDNTextBox.Text;
            if (ValidURI(uri))
            {
                App.CurrentAccount.CDN = uri;
                ServiceManager.Instance.SaveAccountSettings();
                await ServiceManager.Instance.LoadProvidersFromCDNAsync(uri);

                if (updateProviders != null)
                {
                    updateProviders(App.CurrentAccount.ProxyHostname);
                }
            }
        }

        /// <summary>
        /// Updates the geolocation URI when a user clicks out of the geolocation text box's focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns>void</returns>
        public void OnGeolocationChanged(Object sender, RoutedEventArgs args)
        {
            if (App.CurrentAccount == null)
            {
                return;
            }
            string uri = GeolocationTextBox.Text;
            if (ValidURI(uri, httpsOnly:false) && uri != App.CurrentAccount.GeolocationURI)
            {
                MessageBoxResult result = MessageBox.Show("Would you like your location to be sent during Registration attempts?", "Geolocation Updated", MessageBoxButton.YesNo);
                App.CurrentAccount.SendLocationWithRegistration = (result == MessageBoxResult.Yes);
                App.CurrentAccount.GeolocationURI = uri;
                ServiceManager.Instance.SaveAccountSettings();
            }
        }

        // cjm-sep17 -- disable TCP combobox for use on login page
        public void disableElements()
        {
            TransportComboBox.IsEnabled = false;
        }
        #endregion

        #region VoiceMail Uri & MWI
        private void OnVideoMailUriChanged(Object sender, RoutedEventArgs args)
        {
            Console.WriteLine("VideoMail URI Changed");
            if (App.CurrentAccount == null)
                return;
            string oldVideoMailUri = App.CurrentAccount.VideoMailUri;
            string newVideoMailUri = VideoMailUriTextBox.Text;
            if (string.IsNullOrEmpty(newVideoMailUri))
            {
                VideoMailUriTextBox.Text = oldVideoMailUri;
            }
            else
            {
                if (!string.IsNullOrEmpty(newVideoMailUri))
                {
                    try
                    {
                        App.CurrentAccount.VideoMailUri = newVideoMailUri;
                        ServiceManager.Instance.SaveAccountSettings();
                    }
                    catch (Exception e)
                    {
                        LOG.Debug(e.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Callback for MWI text edit field. 
        /// </summary>
        /// <remarks>
        /// This will be activated when it looses focus from the user 
        /// entering a value.URI muse be in the following format
        /// "<sip:username@proxyhost>". If the string is null
        /// then the previous subscription should be unregistered.
        /// </remarks>
        /// <param name="sender">object</param>
        /// <param name="args">RoutedEventArgs</param>
        /// <returns>void</returns>
        private void MWIUriTextBox_LostFocus(object sender, RoutedEventArgs args)
        {
            if (App.CurrentAccount == null) { return; }

            string uri = MWIUriTextBox.Text;

            try
            {
                if (!string.IsNullOrEmpty(uri))
                {
                    if (!CheckMwiUri(uri))
                    {
                        MessageBox.Show("Please provide a valid MWI Address\n\n sip:username@proxyhost.", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
                        MWIUriTextBox.Text = string.Empty;
                        return;
                    }
                }
                else
                {
                    // Erase the videomail count
                    App.CurrentAccount.VideoMailCount = 0;

                    if (updateVideomail != null)
                    {
                        updateVideomail();
                    }
                }

                App.CurrentAccount.MWIUri = uri;
                ServiceManager.Instance.LinphoneService.updateLinphoneConfig("sip", "contact", uri);
                ServiceManager.Instance.SaveAccountSettings();

                if (!string.IsNullOrEmpty(uri) && !uri.Contains('<') && !uri.Contains('>'))
                {
                    uri = $"<{uri}>";
                }
                
                ServiceManager.Instance.LinphoneService.SubscribeForVideoMWI(uri);
            }
            catch (Exception e)
            {
                LOG.Debug(e.ToString());
            }
        }

        /// <summary>
        /// Error check for MWI string
        /// </summary>
        /// <remarks>
        ///  Checks to make sure the user has requested to use a properly formated 
        ///  string. Specifically it checks for "sip:", "@" and url extension ".***".
        /// </remarks>
        /// <param name="uri">string that we are sanity checking</param>
        /// <returns> True if the MWI URI is well formed with a TLd and contains
        /// "sip:" and "@"
        /// </returns>

        // !TODO: update TLDs' that are checked, then uncomment it's reference in MWIUriTextBox_LostFocus() method
        private bool CheckMwiUri(string uri)
        {
            //List<string> tlds = new List<string>(new string[] { ".com", ".net", ".gov", ".co.uk", "org" });

            //foreach (var tld in tlds)
            //{
            //    if (uri.Contains(tld))
            //    {
            if (uri.Contains("sip:") && uri.Contains("@"))
            {
                return true;
            }
            //    }
            //}

            return false;
        }

        /// <summary>
        /// Callback for keydown handling 
        /// </summary>
        /// <remarks>
        /// When the user hits the enter button, focus will be 
        /// lost from the text edit field.This means the lost
        /// focus callback will be evoked. 
        /// </remarks>
        /// <param name="sender">object</param>
        /// <param name="e">KeyEventArgs</param>
        /// <returns>void</returns>
        private void handle_keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (e.Key == Key.Enter)
                {
                    if (ShiftFocus != null)
                    {
                        ShiftFocus();
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Validates a proper URI for Importing Contacts.
        /// </summary>
        /// <remarks>
        /// Do some simple sanity checking to determine if the URI specified is well-formed or empty. 
        /// If it is not then throw error. Well-formed means that the string is in valid URL format.
        /// </remarks>
        /// <param name="address">string that we are sanity checking</param>
        /// <param name="httpsOnly">(Default:true) Whether or not this URI must use HTTPS</param>
        /// <returns>True if the URI is well formed or empty. Well-formed URI contain in them a TLD  
        /// (such as ".com", ".net", ".gov", ".co.uk", or ".org") or "localhost"
        /// </returns>
        private bool ValidURI(string address, bool httpsOnly=true)
        {
            // !TODO: update TLDs' that are checked

            bool isValid = true;
            //bool isValid = false;
            //List<string> tlds = new List<string>(new string[] { ".com", ".net", ".gov", ".co.uk", ".org" });
            if (string.IsNullOrEmpty(address))
            {
                return isValid;
            }
            //foreach (var tld in tlds)
            //{
            //    if (address.Contains(tld))
            //    {
            //        isValid = true;
            //        break;
            //    }
            //}
            //if (address.Contains("localhost"))
            //{
            //    isValid = true;
            //}
            //if (!isValid)
            //{
            //    MessageBox.Show("Contacts URI must contain either a Top Level Domain (ex. com) or 'localhost'", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return isValid;
            //}
            if (!address.Contains("https://") && httpsOnly)
            {
                isValid = false;
                MessageBox.Show("URI must start with 'https://'", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return isValid;
        }
    }
}