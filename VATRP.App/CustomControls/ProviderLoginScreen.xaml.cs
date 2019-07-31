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
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Model;
using com.vtcsecure.ace.windows.Utilities;
using System.Collections.ObjectModel;
using com.vtcsecure.ace.windows.CustomControls.UnifiedSettings;
using com.vtcsecure.ace.windows;
using Microsoft.Win32;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace com.vtcsecure.ace.windows.CustomControls
{
    /// <summary>
    /// Interaction logic for ProviderLoginScreen.xaml
    /// </summary>
    public partial class ProviderLoginScreen : UserControl
    {

        #region Members
        public ObservableCollection<VATRPServiceProvider> ProviderList { get; private set; }
        private readonly MainWindow _mainWnd;
        private int _pollTime;
        private ushort _hostPort;
        private string _address;
        private SettingsWindow _settingsWindow;
        
        private string _versionInfo;
        private const string _placeholderText = "Enter a SIP URI";

        private bool _loginWithOld;
        private bool _accountNotYetLoaded;
        private bool _needToAddAccount;
        #endregion

        public ProviderLoginScreen(MainWindow theMain)
        {
            InitializeComponent();
            _mainWnd = theMain;
            DataContext = this;
            
            _versionInfo = GetVersion();
            _pollTime = 5000;
            
            ProviderList = new ObservableCollection<VATRPServiceProvider>();
            HostnameBox.LostFocus += new RoutedEventHandler(ServerAddressEntered);
            HostnameBox.KeyDown += new KeyEventHandler(handle_keydown);

            Initialize();
        }

        /// <summary>
        /// Initialize the login screen GUI if we have internet access.
        /// If not, all UI fields are set to inactive.
        /// </summary>
        public void Initialize()
        {
            // Initilize the form and fill all combo box.
            bool internetAvailable = NetworkUtility.IsCDNAvailable();
            if (NetworkUtility.IsCDNAvailable())
            {
                InitializeToProvider("STL Test");
                InternetUnavailableGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                InternetUnavailableGrid.Visibility = Visibility.Visible;
            }
            ProviderComboBox.IsEnabled = internetAvailable;
            LoginCmd.IsEnabled = internetAvailable;
            UserNameBox.IsEnabled = internetAvailable;
            PasswdBox.IsEnabled = internetAvailable;
            TransportComboBox.IsEnabled = internetAvailable;

            AuthIDBox.IsEnabled = internetAvailable;
            OutboundProxyServerBox.IsEnabled = internetAvailable;
            HostPortBox.IsEnabled = internetAvailable;

            AutoLoginBox.IsEnabled = internetAvailable;

            // Controls whether or not to check the account list
            // cached file for current account information.
            _accountNotYetLoaded = true;
        }

        public int PollTime
        {
            get { return _pollTime; }
            set { _pollTime = value; }
        }

        public ushort HostPort
        {
            get { return _hostPort; }
            set { _hostPort = value; }
        }

        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }

        public string VersionInfo
        {
            get { return _versionInfo; }
            set { _versionInfo = value; }
        }

        public bool LoginWithOld
        {
            get { return _loginWithOld; }
            set { _loginWithOld = value; }
        }

        private string GetVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return string.Format("Version {0}.{1}.{2}", version.Major, version.Minor, version.Build);
            //return string.Format("Version {0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        /// <summary>
        /// Populate the Provider dropdown menu.
        /// </summary>
        /// <param name="providerName">The name of the provider to populate the dropdown menu.</param>
        public void InitializeToProvider(string providerName)
        {
            List<VATRPServiceProvider> serviceProviderList = ServiceManager.Instance.ProviderService.GetProviderListFullInfo(); //Get the list of all providers and display in combo box.
            // sort the list and ensure the "Custom" field goes to the bottom.
            serviceProviderList.Sort(delegate (VATRPServiceProvider a, VATRPServiceProvider b) {
                if (string.Equals(a.Label, "Custom"))
                    return 1;
                if (string.Equals(b.Label, "Custom"))
                    return -1;
                return a.Label.CompareTo(b.Label); // Sorting of provider list.
            });
            ProviderList.Clear();
            foreach (VATRPServiceProvider provider in serviceProviderList)
            {
                if (provider.Label == "_nologo") // If in address there is "_nologo" then it will not added in the combo box.
                    continue;
                ProviderList.Add(provider);
            }
            PopulateProviderFields(providerName);
        }

        ///<summary>
        /// Populate the Provider and Address UI fields with the relevant information
        /// from the given providerName argument. If this provider does not exist, or
        /// providerName is null, populate the fields with the first element in the ProviderList.
        ///</summary>
        ///<param name="providerName">he name of the provider whose information we want to load into the UI</param>
        ///<returns>void</returns>
        public void PopulateProviderFields(String providerName)
        {
            // VATRP1271 - TODO - add a check to ensure that this has not changed prior to doing anything further.
            VATRPServiceProvider serviceProvider = ServiceManager.Instance.ProviderService.FindProviderLooseSearch(providerName);
            if (serviceProvider == null && ProviderList.Count() > 0)
                serviceProvider = ProviderList[0];

            ProviderComboBox.SelectedItem = serviceProvider;
        }

    public void InitializeToAccount(VATRPAccount account)
        {
            if (account != null)
            {
                UserNameBox.Text = account.PhoneNumber; // The VATRP PhoneNumber variable holds either a phone number or registrtion username
                AuthIDBox.Text = account.AuthID;
                OutboundProxyServerBox.Text = account.OutboundProxy;
                InitializeToProvider(account.ProxyHostname);
                RememberPasswordBox.IsChecked = account.RememberPassword;
                AutoLoginBox.IsChecked = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                    Configuration.ConfEntry.AUTO_LOGIN, false);
                //AutoLoginBox.IsChecked = account.AutoLogin;
                HostPort = account.HostPort;
                HostPortBox.Text = account.HostPort.ToString();

                string transport = account.Transport;
                if (string.IsNullOrWhiteSpace(transport))
                {
                    transport = "TCP";
                }
                foreach (var item in TransportComboBox.Items)
                {
                    var tb = item as TextBlock;
                    string itemString = tb.Text;
                    if (itemString.Equals(transport, StringComparison.InvariantCultureIgnoreCase))
                    {
                        TransportComboBox.SelectedItem = item;
                        TextBlock selectedItem = TransportComboBox.SelectedItem as TextBlock;
                        if (selectedItem != null)
                        {
                            string test = selectedItem.Text;
                        }
                        break;
                    }
                }
                string hostName = account.ProxyHostname;
                if (!string.IsNullOrWhiteSpace(hostName))
                {
                    HostnameBox.Text = hostName;
                }
                string loginMethod = account.LoginMethod;
                if (string.IsNullOrWhiteSpace(loginMethod))
                {
                    loginMethod = "Old";
                }
                foreach (var item in LoginComboBox.Items)
                {
                    var tb = item as TextBlock;
                    string itemString = tb.Text;
                    if (itemString.Equals(loginMethod))
                    {
                        LoginComboBox.SelectedItem = item;
                        TextBlock selectedItem = LoginComboBox.SelectedItem as TextBlock;
                        if (selectedItem != null)
                        {
                            string test = selectedItem.Text;
                        }
                        break;
                    }
                }
            }
        }

        private void TryAgain_Click(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void OnForgotpassword(object sender, RequestNavigateEventArgs e)
        {

        }

        private void OnRegister(object sender, RequestNavigateEventArgs e)
        {

        }

        ///<summary>
        /// Do some simple sanity checking to determine if the URI specified is well-formed.
        /// If it is not then throw error. Well-formed means that the string is in valid URL format.
        ///</summary>
        ///<param name="address">The address that we are sanity checking.</param>
        ///<returns>bool</returns>

        // !TODO: update TLDs' that are checked, then uncomment it's reference in LoginCmd_Click() & ServerAddressEntered() methods
        private bool AddressURLSanityCheck(string address)
        {
            List<string> tlds = new List<string>(new string[] { ".com", ".net", ".gov", ".co.uk", ".org" });
            foreach (var tld in tlds)
            {
                if (address.Contains(tld))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Callback for login button clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoginCmd_Click(object sender, RoutedEventArgs e)
        {
            //*******************************Login Clicked Event ***********************************
            // This method will be called when user click on Login button.
            //**************************************************************************************          
            // cjm-sep17
            string phonePattern = @"^(?:(?:\(?(?:00|\+)([1-4]\d\d|[1-9]\d?)\)?)?[\-\.\ \\\/]?)?((?:\(?\d{1,}\)?[\-\.\ \\\/]?){0,})(?:[\-\.\ \\\/]?(?:#|ext\.?|extension|x)[\-\.\ \\\/]?(\d+))?$";
            string pattern = @"[ \-\s\t\n\r\(\)\+\.]*";
            string replacement = "";

            Regex phoneCheckRgx = new Regex(phonePattern);
            Regex rgx = new Regex(pattern);
             
            if (_settingsWindow != null)
            {
                _settingsWindow.Close();
                _settingsWindow = null;
            }

            string userName = UserNameBox.Text;
            string password = PasswdBox.Password;
            string authId = AuthIDBox.Text;

            Match match = phoneCheckRgx.Match(userName);
            if (match.Success)
            {
                string result = rgx.Replace(userName, replacement);
                // Perserving the leading 1 if there is one:
                //
                /*if (result.Length > 10 && result[0] == '1')
                    result = result.Remove(0, 1);*/
                userName = result;
            }
            
            string outboundProxy = OutboundProxyServerBox.Text;
            HostPort = ushort.Parse(HostPortBox.Text);

            if (string.IsNullOrWhiteSpace(userName)) // If username is blank then it will throw error message.
            {
                MessageBox.Show("Please fill Username field", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            if (string.IsNullOrEmpty(password))// If password is blank then it will throw error message.
            {
                MessageBox.Show("Please fill Password field", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(Address) || string.Equals(Address, _placeholderText)) // If address is blank or the watermark then throw error.
            {
                MessageBox.Show("Please fill the Server address field", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(HostPortBox.Text) || string.Equals(HostPortBox.Text, _placeholderText)) // If address is blank or the watermark then throw error.
            {
                MessageBox.Show("Please fill the Host Server Port field", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //if (!AddressURLSanityCheck(Address))
            //{
            //    MessageBox.Show("Please provide a valid Server address.", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}

            var account = LoadCachedAccountFromFile(UserNameBox.Text);

            if (account != null)
            {
                App.CurrentAccount = account;
            }
            else if(_needToAddAccount)
            {
                App.CurrentAccount.AccountID = Guid.NewGuid().ToString();
            }

            if (LoginWithOld)
            {
                Login_Old(authId, outboundProxy, userName, password, _needToAddAccount);       
                return;
            }
            if (App.CurrentAccount.configuration.HasExpired() || _needToAddAccount)
            {
                ACEConfig config = ConfigLookup.LookupConfig(Address, authId, password);
                // I think this should just end up being null - never serving a defualt string
                // does having a factory defualt make sense if the app is not associated with a 
                // single provider?
                if ((config == null) || (config.configStatus == ACEConfigStatusType.LOGIN_UNAUTHORIZED))
                {
                    // handle login failed
                    PasswdBox.Password = string.Empty;
                    AuthIDBox.Text = string.Empty;          
                    string msg = "The login failed. Please enter a valid username and password.";
                    string caption = "Error with authentication";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBox.Show(msg, caption, button, MessageBoxImage.Stop);
                    return;
                }
                config.SetDownloadDate();
                // VATRP-1271: to do - handle ACEConfigStatusType Appropriately. For the moment (during devel & debug) show the resulting message to the user.
                if (config.configStatus != ACEConfigStatusType.LOGIN_SUCCEESSFUL) 
                {
                    string message = "";
                    switch (config.configStatus)
                    {
                        // TODO note : the text here is a little bit different for each message - enough to let the developer know what to look for
                        //   without being too much for the user. Once we have codes worked out we can use codes in our messages that will help
                        //   in customer support.
                        case ACEConfigStatusType.CONNECTION_FAILED:
                            message = "Unable to obtain configuration information from the server.";
                            break;
                        case ACEConfigStatusType.SRV_RECORD_NOT_FOUND: 
                            message = "The SRV Record was not found.";
                            break;
                        case ACEConfigStatusType.UNABLE_TO_PARSE:
                            message = "Unable to parse the configuration information.";
                            break;
                        default:
                            message = "An error occured while obtaining the configuration. Status Type=" + config.configStatus.ToString();
                            break;
                    }
                    if (!string.IsNullOrEmpty(message))
                    {
                        MessageBox.Show(message, "Error Obtaining Configuration Status");
                    }
                    return;
                }
                else
                {
                    if (string.IsNullOrEmpty(config.sip_auth_password) || string.IsNullOrEmpty(config.phone_number))
                    {
                        config.phone_number = userName;
                        config.sip_auth_password = password;
                    }
                    // VATRP-1899: This is a quick and dirty solution for POC. It will be funational, but not the end implementation we will want.
                    //  This will ultimately be set by the configuration resources from Ace Connect.
                    if (config.phone_number.Equals("agent_1"))
                    {
                        config.user_is_agent = true;
                    }
                    else
                    {
                        config.user_is_agent = false;
                    }
                    config.UpdateVATRPAccountFromACEConfig(App.CurrentAccount);
                    UpdateConfigServiceFromACEConfig(config);
                }
            }
            Login_New(authId, outboundProxy, userName, _needToAddAccount);
        }

        /// <summary>
        /// Loads an account from the cached account list file
        /// </summary>
        /// <remarks>
        /// Since we can start the login process with either a config
        /// file or the text edit fields its necessary to place this
        /// cache file lookup in a wrapper that checks if it has already
        /// been called. Otherwise, it's possible for information to be
        /// overwritten when it is called following a config file upload.
        ///
        /// _needToAddAccount will be set to True if the file does not exist
        /// in the cached file accounts list so that it will be added in the
        /// future.
        /// </remarks>
        /// <returns>the account and a boolean if it needs to be added</returns>
        private VATRPAccount LoadCachedAccountFromFile(string username)
        {
            VATRPAccount loadedAccount = null;

            if (_accountNotYetLoaded)
            {
                loadedAccount = ServiceManager.Instance.AccountService.FindAccount(username, Address);
                _needToAddAccount = loadedAccount == null;
            }

            _accountNotYetLoaded = false;
            return loadedAccount;
        }

        private void UpdateConfigServiceFromACEConfig(ACEConfig config)
        {
            if (config != null)
            {
                ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                                                                 Configuration.ConfEntry.USE_RTT, config.enable_rtt);
            }
        }

        /// <summary>
        /// New login which handles remote configuration parsing.
        /// </summary>
        /// <param name="authId"></param>
        /// <param name="outboundProxy"></param>
        /// <param name="username"></param>
        /// <param name="addAccount"></param>
        private void Login_New(string authId, string outboundProxy, string username, bool addAccount )
        {
            //*********************************************************************************************************************************
            // Login in the VATRP application
            //*********************************************************************************************************************************
            VATRPCredential sipCredential = App.CurrentAccount.configuration.FindCredential("sip", username);
            if (sipCredential == null)
            {
                UserNameBox.Text = string.Empty;
                string msg = string.Format("VRS Account information is not present for {0}", Address);
                string caption = "Error finding your account";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBox.Show(msg, caption, button, MessageBoxImage.Stop);
                return;
            }

            App.CurrentAccount.AuthID = sipCredential.username;
            App.CurrentAccount.OutboundProxy = outboundProxy;
            App.CurrentAccount.Username = sipCredential.username;
            App.CurrentAccount.RegistrationUser = sipCredential.username;
            App.CurrentAccount.Password = sipCredential.password;
            App.CurrentAccount.RegistrationPassword = sipCredential.password;
            App.CurrentAccount.ProxyHostname = Address;
            App.CurrentAccount.HostPort = HostPort;
            App.CurrentAccount.RememberPassword = RememberPasswordBox.IsChecked ?? false;

            bool autoLogin = AutoLoginBox.IsChecked ?? false;
            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                                                            Configuration.ConfEntry.AUTO_LOGIN, autoLogin);
            if (autoLogin)
            {
                App.CurrentAccount.StorePassword(ServiceManager.Instance.GetPWFile());
            }

            var transportText = TransportComboBox.SelectedItem as TextBlock;
            if (transportText != null)
            {
                App.CurrentAccount.Transport = transportText.Text;
            }

            if (LoginWithOld)
            {
                App.CurrentAccount.LoginMethod = "Old";
            }
            else
            {
                App.CurrentAccount.LoginMethod = "New";
            }

            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                                                             Configuration.ConfEntry.ACCOUNT_IN_USE, App.CurrentAccount.AccountID);
            // cjm-aug17
            if (addAccount)
            {
                ServiceManager.Instance.AccountService.AddAccount(App.CurrentAccount);
            }

            ServiceManager.Instance.AccountService.Save();
            ServiceManager.Instance.RegisterNewAccount(App.CurrentAccount.AccountID);
        }

        /// <summary>
        /// Original login process which ignores configuration.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="outboundProxy"></param>
        /// <param name="pw"></param>
        /// <param name="addAccount"></param>
        private void Login_Old(string authId, string outboundProxy, string userName, string pw, bool addAccount)
        {
            App.CurrentAccount.AuthID = authId;
            App.CurrentAccount.OutboundProxy = outboundProxy;
            App.CurrentAccount.Username = userName;
            App.CurrentAccount.RegistrationUser = userName;
            App.CurrentAccount.Password = pw;
            App.CurrentAccount.RegistrationPassword = pw;
            App.CurrentAccount.HostPort = HostPort;
            App.CurrentAccount.RememberPassword = false;
            App.CurrentAccount.ProxyHostname = (HostnameBox.Text != string.Empty) ? HostnameBox.Text : Address;

            UpdateNetLogger(App.CurrentAccount.ProxyHostname);

            bool autoLogin = AutoLoginBox.IsChecked ?? false;
            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                                                            Configuration.ConfEntry.AUTO_LOGIN, autoLogin);
            if (autoLogin)
            {
                App.CurrentAccount.StorePassword(ServiceManager.Instance.GetPWFile());
            }

            var transportText = TransportComboBox.SelectedItem as TextBlock;
            if (transportText != null)
                App.CurrentAccount.Transport = transportText.Text;
            if (LoginWithOld)
            {
                App.CurrentAccount.LoginMethod = "Old";
            }
            else
            {
                App.CurrentAccount.LoginMethod = "New";
            }

            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                                                             Configuration.ConfEntry.ACCOUNT_IN_USE, App.CurrentAccount.AccountID);    
            if (addAccount)
            {
                ServiceManager.Instance.AccountService.AddAccount(App.CurrentAccount);
            }

            ServiceManager.Instance.AccountService.Save();
            ServiceManager.Instance.RegisterNewAccount(App.CurrentAccount.AccountID);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;
            var config = ServiceManager.Instance.ConfigurationService;
            if (config == null)
                return;
            AuthIDBox.Text = App.CurrentAccount.AuthID;
            UserNameBox.Text = App.CurrentAccount.PhoneNumber;
            Address = App.CurrentAccount.ProxyHostname;
            RememberPasswordBox.IsChecked = false;
            AutoLoginBox.IsChecked = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL, Configuration.ConfEntry.AUTO_LOGIN, false);
            HostPortBox.IsReadOnly = true;
            HostPortBox.Background = Brushes.LightGray;
            HostPortBox.Foreground = Brushes.Black;

            switch (App.CurrentAccount.AccountType)
            {
                case VATRPAccountType.VideoRelayService:
                    VatrpDefaultLabel.Content = "Select Default VRS Provider";
                    break;
                case VATRPAccountType.IP_Relay:
                    VatrpDefaultLabel.Content = "Select Default IP-Relay Provider";
                    break;
                case VATRPAccountType.IP_CTS:
                    VatrpDefaultLabel.Content = "Select Default IP-CTS Provider";
                    break;
            }
        }

        ///<summary>
        /// callback for server address field
        /// When this item looses focus, the network utility
        /// logger will be updated for the potential new value.
        /// It will remove the original ping timer object and 
        /// create a new one with the new server address to hit.
        ///</summary>
        ///<param name="sender"></param>
        ///<param name="e"></param>
        ///<returns>void</returns>
        public void ServerAddressEntered(object sender, EventArgs e)
        {
            string uri = HostnameBox.Text;
            //if (AddressURLSanityCheck(uri))
            //{
                UpdateNetLogger(uri); 
            //}
        }

        /// <summary>
        /// Callback for keydown handling 
        /// </summary>
        /// <remarks>
        /// When the user hits the enter button, focus will be 
        /// lost from the hostname textbox.This means the lost
        /// focus callback for the server address will be executed
        /// thus updating this value before logging in. 
        /// 
        /// Without this callback, if the user were to hit enter 
        /// after typing in a custom address, the value would not 
        /// be updated and an error would be displayed to the user.
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns>void</returns>
        private void handle_keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (e.Key == Key.Enter)
                {
                    HostnameBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
            }
        }

        ///<summary>
        /// Callback that triggers whenever a new provider is selected from the
        /// dropdown menu.
        /// Set the address field to the value associated with this provider.
        ///</summary>
        ///<param name="sender"></param>
        ///<param name="args">The ComboBox object that was changed.</param>
        ///<returns>void</returns>
        public void OnProviderChanged(object sender, SelectionChangedEventArgs args)
        {
            // VATRP1271 - TODO - add a check to ensure that this has not changed prior to doign anything further.
            VATRPServiceProvider provider = (VATRPServiceProvider)ProviderComboBox.SelectedItem;
            if (provider != null)
            {
                // update the ing logging for the new provider
                UpdateNetLogger(provider.Address);

                // Reset the content being disaplyed in the form
                AuthIDBox.Text = string.Empty;
                PasswdBox.Password = string.Empty;
                HostnameBox.Text = provider.Address;
                Address = provider.Address;
                UserNameBox.Text = string.Empty;

                App.CurrentAccount.OutboundProxy = string.Empty;
                OutboundProxyServerBox.Text = string.Empty;

                if (string.Equals(provider.Label, "Custom"))
                {
                    HostnameBox.IsReadOnly = false;
                    HostnameBox.Background = Brushes.White;
                    // Trigger the initial textbox lost focus event to set the placeholder text.
                    OnAddressTextboxLostFocus(sender, null);
                }
                else
                {
                    HostnameBox.IsReadOnly = true;
                    HostnameBox.Background = Brushes.LightGray;
                    HostnameBox.Foreground = Brushes.Black;
                }
            }
        }

        /// <summary>
        /// Updates the nework logger for the selected domain
        /// </summary>
        /// <param name="address"></param>
        private void UpdateNetLogger(string address)
        {
            // cjm-sep17 -- generate a Network Ping Logger
            try
            {
                if (_mainWnd.NetLogger == null)
                {
                    _mainWnd.NetLogger = new NetworkLogger(address);
                }
                else
                {
                    // adjust the ping logger
                    _mainWnd.NetLogger.Reset();
                    _mainWnd.NetLogger.DomainName = address;
                }
                _mainWnd.NetLogger.PingProviderNowAsync();
                // _mainWnd.NetLogger.PollProviderAsync(PollTime); remove the polling for now. It's causing crashes and the feature doesn't seem very importantF
            }
            catch (Exception ex)
            {
                // handles case where we cannot connect to the network 
                NetworkLogger.HandleError(ACENetworkError.Unreachable, address, ex.ToString());
                return;
            }
        }

        /// <summary>
        /// Callback for transport selection change event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Selection Changed Event Arguments</param>
        public void OnTransportChanged(object sender, SelectionChangedEventArgs args)
        {
            // This prevent null exception 
            if (App.CurrentAccount == null) {return;}

            var transportText = TransportComboBox.SelectedItem as TextBlock;
            App.CurrentAccount.Transport = transportText.Text;
            string transport = transportText.Text.ToLower();

            // Must check if HostPortBox exists before accessing it
            // When a users un-registers, this method is called before the HostPortBox is initialized
            if (transport == "tcp")
            {
                HostPort = App.CurrentAccount.HostPort = 5060;
                if (HostPortBox != null)
                {
                    HostPortBox.IsReadOnly = true;
                    HostPortBox.Background = Brushes.LightGray;
                    HostPortBox.Foreground = Brushes.Black;
                }
            }
            else if(transport == "tls")
            {
                HostPort = App.CurrentAccount.HostPort = 5061;
                if (HostPortBox != null)
                {
                    HostPortBox.IsReadOnly = true;
                    HostPortBox.Background = Brushes.LightGray;
                    HostPortBox.Foreground = Brushes.Black;
                }
            }
            else if(transport == "udp")
            {
                HostPort = App.CurrentAccount.HostPort = 5060;
                if (HostPortBox != null)
                {
                    HostPortBox.IsReadOnly = false;
                    HostPortBox.Background = Brushes.White;
                }
            }
            if (HostPortBox != null)
            {
                HostPortBox.Text = HostPort.ToString();
            }
        }

        /// <summary>
        /// Callback for username change event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Selection Changed Event Arguments</param>
        private void OnUserNameLostFocus(object sender, RoutedEventArgs e)
        {
            // This prevents a null exception 
            if (App.CurrentAccount == null) { return; }
            App.CurrentAccount.PhoneNumber = UserNameBox.Text;
            App.CurrentAccount.Username = UserNameBox.Text;
            App.CurrentAccount.RegistrationUser = UserNameBox.Text;
        }

        /// <summary>
        /// Handles selection of new vs old login method
        /// New will attempt to fetch a configuration file
        /// from a remote URI and load it during the login
        /// action. Old, will ignore this part of the process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void OnLoginChanged(object sender, SelectionChangedEventArgs args)
        {
            var loginText = LoginComboBox.SelectedItem as TextBlock;
            if (loginText.Text.ToLower() == "old")
            {
                LoginWithOld = true;
                // AuthIdLabel.Visibility = Visibility.Collapsed;
                // AuthIDBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                LoginWithOld = false;
                AuthIdLabel.Visibility = Visibility.Visible;
                AuthIDBox.Visibility = Visibility.Visible;
            }
            if (App.CurrentAccount == null)
            {
                AuthIDBox.Text = string.Empty;
                UserNameBox.Text = string.Empty;  
            }
            PasswdBox.Password = string.Empty;
        }

        public void AdvancedSetup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_settingsWindow == null)
                {
                    // cjm-sep17 -- some menu items need to be disabled 
                    // since the linphoneCore obj is null at this time
                    App.CurrentAccount.DisableAudioCodecs = true;
                    _settingsWindow = _mainWnd.BuildSettingsWindow();
                    _settingsWindow.GeneralSettings.disableElements();
                    _settingsWindow.AudioVideoSettings.disableElements();
                    _settingsWindow.AccountSettings.disableElements();
                    _settingsWindow.AdvancedSettings.disableElements();
                    _settingsWindow.AccountSettings.updateProviders += InitializeToProvider;
                }
                _settingsWindow.Show();
            }
            catch (Exception ex)
            {
                return;
            }
        }

        ///<summary>
        /// Method called when Provider Configuration button is clicked.
        /// Allows the user to select a JSON configuration file which is
        /// then stored into the Account's Configuration data member and 
        /// ussed to login the user
        ///</summary>
        ///<param name="sender"></param>
        ///<param name="e"></param>
        ///<returns>void</returns>
        public void ProviderConfig_Click(object sender, RoutedEventArgs e)
        { 
            ACEConfig config;
            try
            {
                var openDlg = new OpenFileDialog()
                {
                    CheckFileExists = true,
                    CheckPathExists = true,
                    Filter = "Json files (*.json)|*.json",
                    FilterIndex = 0,
                    ShowReadOnly = false,
                };

                if (openDlg.ShowDialog() != true)
                    return;

                string jsonContents = "";
                using (StreamReader sr = new StreamReader(openDlg.FileName))
                {
                    jsonContents = sr.ReadToEnd();
                    sr.Close();
                }

                config = JsonConvert.DeserializeObject<ACEConfig>(jsonContents);
                config.NormalizeValues();
            }
            catch (Exception ex)
            {
                string msg = "Invalid configuration file provided. Failed to parse json file.";
                string caption = "Load failed";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBox.Show(msg, caption, button, MessageBoxImage.Error);
                return;
            }

            // Update UI
            // NOTE - this muse be called in its current location, otherwise,
            // it is possible that the text edit fields on the login page UI
            // will be reset to null after they are set below because the
            // on-provider-change callback will execute beforehand. 
            VATRPServiceProvider serviceProvider = ServiceManager.Instance.ProviderService.FindProviderLooseSearch("Custom");
            ProviderComboBox.SelectedItem = serviceProvider;

            config.SetDownloadDate();

            // Check to see if this account is cached elsewhere
            var account = LoadCachedAccountFromFile(App.CurrentAccount.Username);
            if (account != null)
            {
                App.CurrentAccount = account;
            }

            // Store configuration in current account
            config.UpdateVATRPAccountFromACEConfig_login(App.CurrentAccount);

            UserNameBox.Text = App.CurrentAccount.PhoneNumber;
            PasswdBox.Password = App.CurrentAccount.Password;
            Address = App.CurrentAccount.ProxyHostname;
            AuthIDBox.Text = App.CurrentAccount.AuthID;
            OutboundProxyServerBox.Text = App.CurrentAccount.OutboundProxy;
            string hostName = App.CurrentAccount.ProxyHostname;
            if (!string.IsNullOrWhiteSpace(hostName))
            {
                HostnameBox.Text = hostName;
                HostnameBox.Foreground = Brushes.Black;
            }

            // Update UI
            string transport = App.CurrentAccount.Transport;
            if (!transport.Equals(TransportComboBox.Text))
            {
                foreach (var item in TransportComboBox.Items)
                {
                    var tb = item as TextBlock;
                    string itemString = tb.Text;
                    if (itemString.Equals(transport, StringComparison.InvariantCultureIgnoreCase))
                    {
                        TransportComboBox.SelectedItem = item;
                        TextBlock selectedItem = TransportComboBox.SelectedItem as TextBlock;
                        if (selectedItem != null)
                        {
                            string test = selectedItem.Text;
                        }
                        break;
                    }
                }
            }
            if (config.sip_register_port != null && config.sip_register_port != 0)
            {
                HostPortBox.Text = config.sip_register_port.ToString();
                HostPort = (UInt16)config.sip_register_port;
                App.CurrentAccount.HostPort = HostPort;
            }
        }

        ///<summary>
        /// Callback that triggers whenever the Address textbox is entered
        /// (focused). Remove the watermark text and set the text color to black.
        /// Right now this is only registered as a callback when the Address field is
        /// changed. It allows us to remove the placeholder text when the Address textbox
        /// is focused
        ///</summary>
        ///<param name="sender"></param>
        ///<param name="e"></param>
        ///<returns>void</returns>
        private void OnAddressTextboxFocus(object sender, RoutedEventArgs e)
        {
            VATRPServiceProvider provider = (VATRPServiceProvider)ProviderComboBox.SelectedItem;//ServiceManager.Instance.ProviderService.FindProvider(providerName);
            if (provider == null)
                return;
            if (!string.Equals(provider.Label, "Custom"))
                return;
            // If the placeholder is there then remove it and set the text color to black.
            if (string.Equals(HostnameBox.Text, _placeholderText))
            {
                HostnameBox.Text = "";
                HostnameBox.Foreground = Brushes.Black;
            }
            Address = HostnameBox.Text;
        }

        ///<summary>
        /// Callback that triggers whenever the Address textbox is left
        /// (unfocused). If the size of the text is 0 then set the text color
        /// to gray and add the placeholder text.
        /// Right now this is only registered as a callback when the Address field is
        /// changed. It allows us to display placeholder text when no address is provided
        /// in the address field.
        ///</summary>
        ///<param name="sender"></param>
        ///<param name="e"></param>
        ///<returns>void</returns>
        private void OnAddressTextboxLostFocus(object sender, RoutedEventArgs e)
        {
            var provider = (VATRPServiceProvider)ProviderComboBox.SelectedItem;//ServiceManager.Instance.ProviderService.FindProvider(providerName);
            if (provider == null)
                return;
            if (!string.Equals(provider.Label, "Custom"))
                return;
            // If there is no text then reset the placeholder text.
            if (HostnameBox.Text.Length == 0)
            {
                HostnameBox.Text = _placeholderText;
                HostnameBox.Foreground = Brushes.LightGray;
            }
            Address = HostnameBox.Text;
        }
    }
}
