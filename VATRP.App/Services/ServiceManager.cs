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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Timers;
using System.Web.Script.Serialization;
using System.Windows;
using Windows.Devices.Geolocation;
using log4net;
using VATRP.Core.Extensions;
using VATRP.Core.Model;
using VATRP.Core.Services;
using VATRP.Core.Interfaces;
using VATRP.LinphoneWrapper.Enums;
using System.Threading.Tasks;


namespace com.vtcsecure.ace.windows.Services
{
    internal class ServiceManager : ServiceManagerBase
    {
        //public const string CDN_DOMAIN = "cdn.vatrp.net";
        //public const string CDN_DOMAIN_URL = "http://" + CDN_DOMAIN + "/domains.json";

        //http://isolherbal.com/nvn/domains.json
        //public const string CDN_DOMAIN = "appregent.com"; //"isolherbal.com";
        //public const string CDN_DOMAIN_URL ="https://www." +  CDN_DOMAIN + "/ace.json"; //"/Ace/domains2.json";

        public static string CDN_DOMAIN
        {
            get
            {
                return m_CDN_DOMAIN;
            }
            set
            {
                m_CDN_DOMAIN = value;
            }
        }

        public static string CDN_DOMAIN_URL
        {
            get
            {
                return m_CDN_DOMAIN_URL;
            }
            set
            {
                m_CDN_DOMAIN_URL = value;
            }
        }

        public static string CDN_DOMAIN_PATH
        {
            get
            {
                return m_CDN_DOMAIN_PATH;
            }
            set
            {
                m_CDN_DOMAIN_PATH = value;
            }
        }

        #region Members
        private static string m_CDN_DOMAIN = string.Empty;
        private static string m_CDN_DOMAIN_URL = string.Empty;
        private static string m_CDN_DOMAIN_PATH = string.Empty;
        private static bool m_OverRideLocalProvidersList = false;

        private static readonly ILog LOG = LogManager.GetLogger(typeof(ServiceManager));
        private string _applicationDataPath;
        private static ServiceManager _singleton;
        private IConfigurationService _configurationService;
        private IContactsService _contactService;
        private IChatService _chatService;
        private IHistoryService _historyService;
        private ISoundService _soundService;
        private IAccountService _accountService;
        private ILinphoneService _linphoneService;
        private IProviderService _providerService;
        private Timer _locationRequestTimer;
        private WebClient _webClient;
        private bool _geoLocationFailure = false;
        private bool _geoLocaionUnauthorized = false;
        private bool _displayLocationWarning = true;
        private string _locationString;

        #endregion

        #region Event
        public delegate void NewAccountRegisteredDelegate(string accountId);
        public event NewAccountRegisteredDelegate NewAccountRegisteredEvent;

        public event EventHandler LinphoneCoreStartedEvent;
        public event EventHandler LinphoneCoreStoppedEvent;
        #endregion

        public static ServiceManager Instance
        {
            get { return _singleton ?? (_singleton = new ServiceManager()); }
        }

        public string ApplicationDataPath
        {
            get
            {
                if (_applicationDataPath == null)
                {
                    String applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    _applicationDataPath = Path.Combine(applicationData, "VATRP");
                    Directory.CreateDirectory(_applicationDataPath);
                }
                return _applicationDataPath;
            }
        }

        public string GetPWFile()
        {
            //*************************************************************************************************************************************************************
            // Get Password file path.
            //*************************************************************************************************************************************************************
            string path = ApplicationDataPath;
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    if (path.LastIndexOf(Path.PathSeparator) != (path.Length - 1))
                    {
                        path += Path.DirectorySeparatorChar;
                    }
                    path += "user.dat";
                }
                catch (Exception ex)
                {

                    return "";
                }
            }
            return path;
        }

        #region Overrides
        public override string BuildStoragePath(string folder)
        {
            try
            {
                return Path.Combine(ApplicationDataPath, folder);
            }
            catch
            {

            }
            return Environment.CurrentDirectory;
        }

        public override string BuildDataPath(string folder)
        {
            if (App.CurrentAccount == null)
                return BuildStoragePath(folder);
            try
            {
                var privateDataPath = Path.Combine(ApplicationDataPath,
                    string.Format("{0}@{1}", App.CurrentAccount.Username,
                        App.CurrentAccount.ProxyHostname));

                if (!Directory.Exists(privateDataPath))
                    Directory.CreateDirectory(privateDataPath);
                return Path.Combine(privateDataPath, folder);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception occurred: " + ex.ToString());
            }
            return BuildStoragePath(folder);
        }

        public override IConfigurationService ConfigurationService
        {
            get { return _configurationService ?? (_configurationService = new XmlConfigurationService(this, true)); }
        }

        public override IContactsService ContactService
        {
            get { return _contactService ?? (_contactService = new ContactService(this)); }
        }

        public override IChatService ChatService
        {
            get { return _chatService ?? (_chatService = new ChatsService(this)); }
        }

        public override IHistoryService HistoryService
        {
            get { return _historyService ?? (_historyService = new HistoryService(this)); }
        }

        public override ISoundService SoundService
        {
            get { return _soundService ?? (_soundService = new SoundService(this)); }
        }

        public override IAccountService AccountService
        {
            get { return _accountService ?? (_accountService = new AccountService(this)); }
        }

        public override ILinphoneService LinphoneService
        {
            get { return _linphoneService ?? (_linphoneService = new LinphoneService(this)); }
        }
        public override IProviderService ProviderService
        {
            get { return _providerService ?? (_providerService = new ProviderService(this)); }
        }

        public override System.Windows.Threading.Dispatcher Dispatcher
        {
            get
            {
                if (Application.Current != null)
                    return Application.Current.Dispatcher;
                return null;
            }
        }
        #endregion

        #region Properties

        public IntPtr ActiveCallPtr { get; set; }

        internal bool ConfigurationStopped { get; set; }
        internal bool LinphoneCoreStopped { get; set; }
        internal bool HistoryServiceStopped { get; set; }

        internal bool AccountServiceStopped { get; set; }
        internal bool ProviderServiceStopped { get; set; }

        public bool AllowGeoLocationRequest { get; set; }

        public string LocationString
        {
            get
            {
                if (AllowGeoLocationRequest && !_geoLocationFailure && !_geoLocaionUnauthorized)
                    return _locationString;
                return string.Empty;
            }
            set { _locationString = value; }
        }

        #endregion

        private ServiceManager()
        {
            ConfigurationStopped = true;
            LinphoneCoreStopped = true;
            HistoryServiceStopped = true;
            AccountServiceStopped = true;
            ProviderServiceStopped = true;
            LocationString = string.Empty;
            AllowGeoLocationRequest = true;

            //  2/17/2017 fjr Added Config File
            //  Store the Domain for the rest of the application
            if (!String.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["domain"]))
            {
                m_CDN_DOMAIN = System.Configuration.ConfigurationManager.AppSettings["domain"];
            }

            //  Store the Domain URL
            if (!String.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["domainURL"]))
            {
                m_CDN_DOMAIN_PATH = System.Configuration.ConfigurationManager.AppSettings["domainURL"];
            }

            //  Flag to use HTTP or HTTPs
            if (!String.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["useHTTPs"]))
            {
                //  2/20/2017 fjr Added HTTPS support
                m_CDN_DOMAIN_URL = System.Configuration.ConfigurationManager.AppSettings["useHTTPs"] == "true" ? "https://" + m_CDN_DOMAIN_PATH + "/ace.json" : "http://" + m_CDN_DOMAIN_PATH + "/ace.json";
            }

            //  Flag to Override Local Providers List Flag 
            if (!String.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["overWriteLocalProvidersList"]))
            {
                m_OverRideLocalProvidersList = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["overWriteLocalProvidersList"].Trim());
            }

        }

        public bool Initialize()
        {
            _webClient = new WebClient();
            _webClient.DownloadStringCompleted += CredentialsReceived;

            this.ConfigurationService.ServiceStarted += OnConfigurationServiceStarted;
            this.AccountService.ServiceStarted += OnAccountsServiceStarted;
            this.ProviderService.ServiceStarted += OnProviderServiceStarted;
            this.HistoryService.ServiceStarted += OnHistoryServiceStarted;
            this.LinphoneService.ServiceStarted += OnLinphoneServiceStarted;
            this.ContactService.ServiceStarted += OnContactserviecStarted;

            this.ConfigurationService.ServiceStopped += OnConfigurationServiceStopped;
            // cjm-sep17 -- this may be a typo below "Servicestarted" and its causing problems storing accounts into the list for caching
            this.AccountService.ServiceStopped += OnAccountsServiceStopped;
            this.ProviderService.ServiceStopped += OnProviderServiceStopped;
            this.HistoryService.ServiceStopped += OnHistoryServiceStopped;
            this.LinphoneService.ServiceStopped += OnLinphoneServiceStopped;
            return true;
        }

        public async Task LoadProvidersFromCDNAsync(string uri)
        {
            CDN_DOMAIN_URL = uri;
            await UpdateProvidersList();
        }

        private void OnConfigurationServiceStarted(object sender, EventArgs args)
        {
            App.CurrentAccount = LoadActiveAccount();
        }

        private void OnAccountsServiceStarted(object sender, EventArgs args)
        {
            App.CurrentAccount = LoadActiveAccount();
            AccountServiceStopped = false;
        }

        private async void OnProviderServiceStarted(object sender, EventArgs args)
        {
            await UpdateProvidersList(); //Download the provider list. 
            ProviderServiceStopped = false;
        }

        private void OnLinphoneServiceStarted(object sender, EventArgs args)
        {
            var os = Environment.OSVersion;
            LOG.Info(string.Format("OS: {0} Platform: {1}", os.VersionString, os.Platform));

            if (os.Version.Major > 6 || (os.Version.Major == 6 && os.Version.Minor == 2))
            {
                GetGeolocation();
            }
            else
            {
                LOG.Warn("GeoLocation is not supported");
            }
            LinphoneCoreStopped = false;
            if (LinphoneCoreStartedEvent != null)
                LinphoneCoreStartedEvent(this, EventArgs.Empty);
        }

        private void OnContactserviecStarted(object sender, EventArgs e)
        {

            //********************************************************************************************************************************************
            // After contacts found , Starting the Chat service.
            //********************************************************************************************************************************************
            ChatService.Start();
        }

        private void OnHistoryServiceStarted(object sender, EventArgs args)
        {
            HistoryServiceStopped = false;
        }

        private void OnConfigurationServiceStopped(object sender, EventArgs args)
        {
            ConfigurationStopped = true;
        }
        private void OnAccountsServiceStopped(object sender, EventArgs args)
        {
            AccountServiceStopped = true;
        }
        private void OnProviderServiceStopped(object sender, EventArgs args)
        {
            ProviderServiceStopped = true;
        }
        private void OnLinphoneServiceStopped(object sender, EventArgs args)
        {
            LinphoneCoreStopped = true;
            if (LinphoneCoreStoppedEvent != null)
                LinphoneCoreStoppedEvent(this, EventArgs.Empty);
        }

        private void OnHistoryServiceStopped(object sender, EventArgs args)
        {
            HistoryServiceStopped = true;
        }

        internal bool WaitForServiceCompletion(int secToWait)
        {
            int nWait = secToWait * 10;
            while (nWait-- > 0)
            {
                if (LinphoneCoreStopped && HistoryServiceStopped && AccountServiceStopped && ProviderServiceStopped &&
                    ConfigurationStopped)
                    return true;
                System.Threading.Thread.Sleep(100);
            }
            return false;
        }

        internal bool Start()
        {
            LOG.Info("Starting services...");
            var retVal = true;
            retVal &= ConfigurationService.Start();
            retVal &= AccountService.Start();
            retVal &= SoundService.Start();
            retVal &= ProviderService.Start();
            return retVal;
        }

        public bool UpdateLinphoneConfig() // cjm-aug17 -- this receives data from app registration data and passes into linphone core for registration process...
        {
            if (App.CurrentAccount == null)
            {
                LOG.Warn("Can't update linphone config. Account is no configured");
                return false;
            }

            this.LinphoneService.LinphoneConfig.ProxyHost = string.IsNullOrEmpty(App.CurrentAccount.ProxyHostname) ?
                Configuration.LINPHONE_SIP_SERVER : App.CurrentAccount.ProxyHostname;
            LinphoneService.LinphoneConfig.ProxyPort = App.CurrentAccount.ProxyPort;
            LinphoneService.LinphoneConfig.UserAgent = ConfigurationService.Get(Configuration.ConfSection.LINPHONE, Configuration.ConfEntry.LINPHONE_USERAGENT,
                    Configuration.LINPHONE_USERAGENT);

            LinphoneService.LinphoneConfig.AuthID = App.CurrentAccount.AuthID;
            LinphoneService.LinphoneConfig.Username = App.CurrentAccount.RegistrationUser;
            LinphoneService.LinphoneConfig.DisplayName = App.CurrentAccount.DisplayName;
            LinphoneService.LinphoneConfig.Password = App.CurrentAccount.RegistrationPassword;
            LinphoneService.LinphoneConfig.PhoneNumber = App.CurrentAccount.PhoneNumber;

            string[] transportList = { "UDP", "TCP", "DTLS", "TLS" };
            if (transportList.All(s => App.CurrentAccount.Transport != s))
            {
                App.CurrentAccount.Transport = "TCP";
                AccountService.Save();
            }

            // set geolocation
            LinphoneService.LinphoneConfig.GeolocationURI = App.CurrentAccount.GeolocationURI;

            LinphoneService.LinphoneConfig.Transport = App.CurrentAccount.Transport;
            LinphoneService.LinphoneConfig.EnableSTUN = App.CurrentAccount.EnableSTUN;
            LinphoneService.LinphoneConfig.STUNAddress = App.CurrentAccount.STUNAddress;
            LinphoneService.LinphoneConfig.STUNPort = App.CurrentAccount.STUNPort;
            LinphoneService.LinphoneConfig.MediaEncryption = GetMediaEncryptionText(App.CurrentAccount.MediaEncryption);
            LinphoneService.LinphoneConfig.EnableAVPF = App.CurrentAccount.EnableAVPF;
            // cardDAV
            if (!String.IsNullOrEmpty(App.CurrentAccount.CardDavServerPath))
            {
                LinphoneService.RemoveCardDAVAuthInfo();
                LinphoneService.LinphoneConfig.CardDavRealm = App.CurrentAccount.CardDavRealm;

                LinphoneService.LinphoneConfig.CardDavUser = App.CurrentAccount.RegistrationUser;
                LinphoneService.LinphoneConfig.CardDavPass = App.CurrentAccount.RegistrationPassword;
                LinphoneService.LinphoneConfig.CardDavServer = App.CurrentAccount.CardDavServerPath;

                try
                {
                    Uri cardDavServer = new Uri(LinphoneService.LinphoneConfig.CardDavServer);
                    LinphoneService.LinphoneConfig.CardDavDomain = cardDavServer.Host;
                }
                catch (Exception ex)
                {
                    LOG.Error(string.Format("Parse CardDAV server. {0}\n{1}",
                        LinphoneService.LinphoneConfig.CardDavServer, ex.Message));
                }
            }
            LOG.Info("Linphone service configured for account: " + App.CurrentAccount.RegistrationUser);
            return true;
        }

        internal void Stop()
        {
            LOG.Info("Stopping services...");
            HistoryService.Stop();
            ConfigurationService.Stop();
            LinphoneService.Unregister(true);
            LinphoneService.Stop();
            AccountService.Stop();
            ProviderService.Stop();
            try
            {
                if (_locationRequestTimer != null)
                    _locationRequestTimer.Dispose();
            }
            catch
            {

            }
        }

        internal bool RequestLinphoneCredentials(string username, string passwd)
        {
            bool retValue = true;
            var requestLink = ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.REQUEST_LINK, Configuration.DEFAULT_REQUEST);
            var request = (HttpWebRequest)WebRequest.Create(requestLink);

            var postData = string.Format("{{ \"user\" : {{ \"email\" : \"{0}\", \"password\" : \"{1}\" }} }}", username, passwd);
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.ContentLength = data.Length;

            try
            {
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            try
            {
                var response = (HttpWebResponse)request.GetResponse();

                var responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    var responseString = new StreamReader(responseStream).ReadToEnd();
                    ParseHttpResponse(responseString);
                }
                else
                {
                    retValue = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return retValue;
        }

        private void ParseHttpResponse(string response)
        {
            // parse response stream, 
            var jss = new JavaScriptSerializer();
            var dict = jss.Deserialize<Dictionary<string, string>>(response);

            if (dict.ContainsKey("pbx_extension"))
            {
                App.CurrentAccount.RegistrationUser = dict["pbx_extension"];
            }
            if (dict.ContainsKey("auth_token"))
            {
                App.CurrentAccount.RegistrationPassword = dict["auth_token"];
            }

            if (UpdateLinphoneConfig())
            {
                if (LinphoneService.Start(true))
                    LinphoneService.Register();
            }
        }

        private void CredentialsReceived(object sender, DownloadStringCompletedEventArgs e)
        {

        }

        internal VATRPAccount LoadActiveAccount()
        {
            var accountUID = ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.ACCOUNT_IN_USE, "");
            if (string.IsNullOrEmpty(accountUID))
                return null;
            var account = AccountService.FindAccount(accountUID);
            return account;
        }

        /// <summary>
        /// Builds list of Providers
        /// </summary>
        /// <remarks>
        /// This method will attempt to load the list of 
        /// providers from the json file hosted on the ace
        /// config server and if it cannot then it will append 
        /// only the Custom option to the drop-down menu.
        /// </remarks>
        /// <returns>Task</returns>
        private async Task UpdateProvidersList()
        {
            bool status = await LoadJsonProvidersAsync();
            
            if (status)
            {
                return;
            }

            ProviderService.AddProvider(new VATRPServiceProvider()
            {
                Label = "Custom",
                Address = null
            });
            /*
            string[] labels = { "Sorenson", "Purple", "ZVRS", "Convo", "Global" };
            foreach (var label in labels)
            {
                if (ProviderService.FindProvider(label) == null)
                    ProviderService.AddProvider(new VATRPServiceProvider()
                    {
                        Label = label,
                        Address = null
                    });
            }
            */
        }

        /// <summary>
        /// Fetch the list of Providers from the CDN server.
        /// </summary>
        /// <remarks>
        /// If any provider configurations are returned from the server then
        /// override the local list of providers with the server's list.
        /// This methods attemps to download the logo, URL, and other information
        /// about the provider from the server.
        /// </remarks>
        /// <param>void</param>
        /// <returns>Task<bool></returns>
        private async Task<bool> LoadJsonProvidersAsync()
        {
            var imgCachePath = BuildStoragePath("img");
            try
            {
                List<VATRPDomain> domains = await Utilities.JsonWebRequest.MakeJsonWebRequestAsync<List<VATRPDomain>>(CDN_DOMAIN_URL);

                //  Added 3/3/2017 fjr Override Local Providers with the server's List
                if (m_OverRideLocalProvidersList)
                {
                    if (domains != null &&
                            domains.Count > 0)
                    {
                        ProviderService.ClearProvidersList();
                        VATRPServiceProvider CustomProvider = new VATRPServiceProvider();
                        CustomProvider.Address = null;
                        CustomProvider.Label = "Custom";
                        ProviderService.AddProvider(CustomProvider);
                    }
                }

                // add these into the cache
                foreach (VATRPDomain domain in domains)
                {
                    VATRPServiceProvider provider = ProviderService.FindProviderLooseSearch(domain.name);
                    if (provider == null)
                    {
                        provider = new VATRPServiceProvider();
                        provider.Label = domain.name;
                        provider.Address = domain.domain;
                        provider.ImageURI = domain.icon2x;
                        provider.IconURI = domain.icon;
                        ProviderService.AddProvider(provider);
                    }
                    else
                    {
                        // update the provider information
                        provider.Label = domain.name;
                        provider.Address = domain.domain;
                        provider.ImageURI = domain.icon2x;
                        provider.IconURI = domain.icon;
                    }

                    if (provider.ImageURI.NotBlank())
                        provider.LoadImage(imgCachePath, false);
                    if (provider.IconURI.NotBlank())
                        provider.LoadImage(imgCachePath, true);

                }

                VATRPServiceProvider noLogoProvider = ProviderService.FindProviderLooseSearch("_nologo");
                if (noLogoProvider == null)
                {
                    noLogoProvider = new VATRPServiceProvider();
                    ProviderService.AddProvider(noLogoProvider);
                }
                return true;
            }
            catch (Exception ex)
            {
                // either the domains were mal-formed or we are not able to get to the internet. If this is the case, then allow the cached/defaults.
                return false;
            }
        }

        internal static void LogError(string message, Exception ex)
        {
            LOG.Error(string.Format("Exception occurred in {0}: {1}", message, ex.Message));
        }

        internal void SaveAccountSettings()
        {
            if (App.CurrentAccount == null)
                return;

            AccountService.Save();
        }

        internal bool StartLinphoneService()
        {

            //**************************************************************************************************************************
            // Starting Linephone Service
            //**************************************************************************************************************************
            if (App.CurrentAccount == null)
                return false;
            if (!LinphoneService.Start(true))
                return false;

            LinphoneService.UpdateAdvancedParameters(App.CurrentAccount);
            if (App.CurrentAccount.AudioCodecsList.Count > 0)
                LinphoneService.UpdateNativeCodecs(App.CurrentAccount, CodecType.Audio);
            else
                LinphoneService.FillCodecsList(App.CurrentAccount, CodecType.Audio);

            if (App.CurrentAccount.VideoCodecsList.Count > 0)
                LinphoneService.UpdateNativeCodecs(App.CurrentAccount, CodecType.Video);
            else
                LinphoneService.FillCodecsList(App.CurrentAccount, CodecType.Video);

            LinphoneService.UpdateNetworkingParameters(App.CurrentAccount);
            LinphoneService.configureFmtpCodec();
            ApplyAVPFChanges();
            ApplyDtmfOnSIPInfoChanges();
            ApplyDtmfInbandChanges();
            ApplyMediaSettingsChanges();
            ApplyCallSettingsChanges();
            return true;
        }

        private void ApplyCallSettingsChanges()
        {
            LinphoneService.SetIncomingCallRingingTimeout(300);
        }

        internal void Register()
        {
            if (App.CurrentAccount != null && !string.IsNullOrEmpty(App.CurrentAccount.Username))
            {
                LinphoneService.Register();
            }
        }

        internal void RegisterNewAccount(string id)
        {
            if (NewAccountRegisteredEvent != null)
                NewAccountRegisteredEvent(id);
        }

        internal void ApplyCodecChanges()
        {
            var retValue = LinphoneService.UpdateCodecsAccessibility(App.CurrentAccount,
                CodecType.Audio);

            retValue &= LinphoneService.UpdateCodecsAccessibility(App.CurrentAccount, CodecType.Video);

            if (!retValue)
                SaveAccountSettings();
        }

        internal void ApplyNetworkingChanges()
        {
            LinphoneService.UpdateNetworkingParameters(App.CurrentAccount);
        }

        internal void AdvancedSettings()
        {
            LinphoneService.UpdateAdvancedParameters(App.CurrentAccount);
        }

        internal void ApplyAVPFChanges()
        {
            // VATRP-1507: Tie RTCP and AVPF together:
            string rtcpFeedback = this.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.RTCP_FEEDBACK, "Implicit");
            // if RTCPFeedback = Off then RTCP and AVPF are both off
            // if RTCPFeedback = Implicit then RTCP is on, AVPF is off
            // if RTCPFeedback = Explicit then RTCP is on, AVPF = on
            //LinphoneAVPFMode avpfMode = LinphoneAVPFMode.LinphoneAVPFEnabled;
            // Note: we could make the RTCP also be a bool, but using this method in case we need to handle something differently in the future.
            //    eg - is there something that happens if we want rtcp off and avpf on?

            LinphoneService.SetRTCPFeedback(rtcpFeedback);


            // commenting this in case we need somethinghere from the compiler debug statement

            //            var mode = LinphoneAVPFMode.LinphoneAVPFEnabled;
            //#if DEBUG
            //            if (!this.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
            //                Configuration.ConfEntry.AVPF_ON, true))
            //            {
            //                mode = LinphoneAVPFMode.LinphoneAVPFDisabled;
            //            }
            //#endif
            //            LinphoneService.SetAVPFMode(mode);
        }

        internal void ApplyDtmfOnSIPInfoChanges()
        {
            bool val = this.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.DTMF_SIP_INFO, false);
            LinphoneService.SendDtmfAsSipInfo(val);
        }

        internal void ApplyDtmfInbandChanges()
        {
            // ToDo VATRP-3039 enable RFC2833/4733 
            //bool val = this.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
            //    Configuration.ConfEntry.DTMF_INBAND, false);
            LinphoneService.SendDtmfAsTelephoneEvent(true);
        }

        private LinphoneMediaEncryption GetMediaEncryptionText(string s)
        {

            switch (s)
            {
                case "Encrypted (DTLS)":
                    return LinphoneMediaEncryption.LinphoneMediaEncryptionDTLS;
                case "Encrypted (SRTP)":
                    return LinphoneMediaEncryption.LinphoneMediaEncryptionSRTP;
                case "Encrypted (ZRTP)":
                    return LinphoneMediaEncryption.LinphoneMediaEncryptionZRTP;
                default:
                    return LinphoneMediaEncryption.LinphoneMediaEncryptionNone;
            }
        }

        // Note: if the selected device is not available, use the default, and store the default for the current user
        internal void ApplyMediaSettingsChanges()
        {
            // this should never be an issue, but just in case
            if (App.CurrentAccount == null)
                return;
            LinphoneService.LinphoneConfig.MediaEncryption = GetMediaEncryptionText(App.CurrentAccount.MediaEncryption);
            LinphoneService.UpdateMediaSettings(App.CurrentAccount);
            bool accountChanged = false;
            // prior to setting the devices we need to ensure that the selected devices are still options.
            //  for example - did the user unplug the camera since the last run?
            if (!string.IsNullOrEmpty(App.CurrentAccount.SelectedCameraId))
            {
                List<VATRPDevice> availableCameras = GetAvailableCameras();
                if (IsDeviceAvailable(App.CurrentAccount.SelectedCameraId, availableCameras))
                {
                    LinphoneService.SetCamera(App.CurrentAccount.SelectedCameraId);
                }
                else
                {
                    // update the stored setting in the account because the previously selected device is not available
                    VATRPDevice device = GetSelectedCamera();
                    if (device != null)
                    {
                        App.CurrentAccount.SelectedCameraId = device.deviceId;
                        accountChanged = true;
                    }
                }
            }
            if (!string.IsNullOrEmpty(App.CurrentAccount.SelectedMicrophoneId))
            {
                List<VATRPDevice> availableMicrophones = GetAvailableMicrophones();
                if (IsDeviceAvailable(App.CurrentAccount.SelectedMicrophoneId, availableMicrophones))
                {
                    LinphoneService.SetCaptureDevice(App.CurrentAccount.SelectedMicrophoneId);
                }
                else
                {
                    // update the stored setting in the account because the previously selected device is not available
                    VATRPDevice device = GetSelectedMicrophone();
                    if (device != null)
                    {
                        App.CurrentAccount.SelectedMicrophoneId = device.deviceId;
                        accountChanged = true;
                    }
                }
            }
            if (!string.IsNullOrEmpty(App.CurrentAccount.SelectedSpeakerId))
            {
                List<VATRPDevice> availableSpeakers = GetAvailableSpeakers();
                if (IsDeviceAvailable(App.CurrentAccount.SelectedSpeakerId, availableSpeakers))
                {
                    LinphoneService.SetSpeakers(App.CurrentAccount.SelectedSpeakerId);
                }
                else
                {
                    // update the stored setting in the account because the previously selected device is not available
                    VATRPDevice device = GetSelectedSpeakers();
                    if (device != null)
                    {
                        App.CurrentAccount.SelectedSpeakerId = device.deviceId;
                        accountChanged = true;
                    }
                }
            }
            // if there was a change to the account, save it
            if (accountChanged)
            {
                SaveAccountSettings();
            }
        }

        internal void UpdateLoggedinContact()
        {
            if (App.CurrentAccount == null || !App.CurrentAccount.Username.NotBlank())
                return;
            var contactAddress = string.Format("{0}@{1}", App.CurrentAccount.Username,
                App.CurrentAccount.ProxyHostname);
            VATRPContact contact = this.ContactService.FindLoggedInContact();
            bool addLogedInContact = true;
            if (contact != null)
            {
                if (contact.SipUsername == contactAddress)
                {
                    contact.IsLoggedIn = false;
                    addLogedInContact = false;
                }
            }

            if (addLogedInContact)
            {
                var contactID = new ContactID(contactAddress, IntPtr.Zero);
                contact = new VATRPContact(contactID)
                {
                    IsLoggedIn = true,
                    Fullname = App.CurrentAccount.Username,
                    DisplayName = App.CurrentAccount.DisplayName,
                    RegistrationName =
                        string.Format("sip:{0}@{1}", App.CurrentAccount.Username, App.CurrentAccount.ProxyHostname)
                };
                contact.Initials = contact.Fullname.Substring(0, 1).ToUpper();
                this.ContactService.AddContact(contact, string.Empty);
            }
        }

        public bool IsRttAvailable
        {
            get
            {
                return (ConfigurationService != null &&
                        (ActiveCallPtr != IntPtr.Zero && ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                            Configuration.ConfEntry.USE_RTT, true)));
            }
        }

        internal void StartLocationRequestTimer()
        {
            if (_locationRequestTimer == null)
            {
                _locationRequestTimer = new Timer(120000) { AutoReset = false };
                _locationRequestTimer.Elapsed += LocatioTimerElapsed;
            }

            if (!_locationRequestTimer.Enabled && !_geoLocationFailure && !_geoLocaionUnauthorized)
            {
                _locationRequestTimer.Start();
            }
        }

        private void LocatioTimerElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            GetGeolocation();
        }

        internal async void GetGeolocation()
        {
            try
            {
                // cjm-sep17 -- This is for mobile devices with GPS data only
                Geolocator loc = new Geolocator();
                try
                {
                    loc.DesiredAccuracy = PositionAccuracy.High;
                    Geoposition pos = await loc.GetGeopositionAsync();
                    var lat = pos.Coordinate.Latitude;
                    var lang = pos.Coordinate.Longitude;
                    LocationString = string.Format("<geo:{0},{1}>", lat, lang);
                    StartLocationRequestTimer();
                    _displayLocationWarning = false;
                }
                catch (System.UnauthorizedAccessException ex)
                {
                    // handle error
                    LogError("GetGeolocation", ex);
                    _geoLocaionUnauthorized = true;
                    if (_displayLocationWarning)
                    {
                        // Instruct user to enable location services
                        // only show this one time per execution 
                        _displayLocationWarning = false;
                        string msg = "Please Enable Location Services";
                        string caption = "Error Detecting Your Location";
                        MessageBoxButton button = MessageBoxButton.OK;
                        MessageBox.Show(msg, caption, button, MessageBoxImage.Warning);
                    }
                }
            }
            catch (TypeLoadException ex)
            {
                _geoLocationFailure = true;
                LogError("GetGeolocation", ex);
            }
            catch (PlatformNotSupportedException ex)
            {
                _geoLocationFailure = true;
                LogError("GetGeolocation", ex);
            }
            catch (Exception ex)
            {
                LogError("GetGeolocation", ex);
                _geoLocationFailure = true;
            }
        }

        public void ClearProxyInformation()
        {
            LinphoneService.ClearProxyInformation();
        }

        public void ClearAccountInformation()
        {
            LinphoneService.ClearAccountInformation();
        }

        internal void StartupLinphoneCore()
        {
            ServiceManager.Instance.LinphoneService.RemoveDBPassword();
            ServiceManager.Instance.LinphoneService.RemoveCallHistoryDBPassword();
            ServiceManager.Instance.LinphoneService.RemoveChatHistoryDBPassword();

            if (UpdateLinphoneConfig())
            {
                if (StartLinphoneService())
                {
                    LoadPrivateData();
                    Register();
                }
            }
        }

        public bool IsDeviceAvailable(string deviceId, List<VATRPDevice> deviceList)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return false;
            }
            if (deviceList == null)
            {
                return false;
            }
            foreach (VATRPDevice device in deviceList)
            {
                if (device.deviceId.Equals(deviceId))
                {
                    return true;
                }
            }
            return false;
        }

        public List<VATRPDevice> GetAvailableCameras()
        {
            return LinphoneService.GetAvailableCameras();
        }
        public void SetCamera(string deviceId)
        {
            LinphoneService.SetCamera(deviceId);
        }
        public VATRPDevice GetSelectedCamera()
        {
            return LinphoneService.GetSelectedCamera();
        }

        public List<VATRPDevice> GetAvailableMicrophones()
        {
            return LinphoneService.GetAvailableMicrophones();

        }
        public void SetCaptureDevice(string deviceId)
        {
            LinphoneService.SetCaptureDevice(deviceId);
        }
        public VATRPDevice GetSelectedMicrophone()
        {
            return LinphoneService.GetSelectedMicrophone();
        }

        public List<VATRPDevice> GetAvailableSpeakers()
        {
            return LinphoneService.GetAvailableSpeakers();
        }
        public void SetSpeakers(string deviceId)
        {
            LinphoneService.SetSpeakers(deviceId);
        }
        public VATRPDevice GetSelectedSpeakers()
        {
            return LinphoneService.GetSelectedSpeakers();
        }

        private void LoadPrivateData()
        {
            //****************************************************************************************************************************************************
            // Getting the contacts, starting contact service, chat service etc.
            //****************************************************************************************************************************************************
            LinphoneService.UpdatePrivateDataPath();
            HistoryService.Start();
            ContactService.Start();
            ChatService.Start();       
        }
    }
}