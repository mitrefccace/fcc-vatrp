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
using System.Data.Linq.Mapping;
using System.Security;
using System.Security.Cryptography;
using VATRP.Core.Utilities;

namespace VATRP.Core.Model
{
    [Table(Name = "ACCOUNTS")]
    public class VATRPAccount
    {
        #region Members

        private string _videoPreset;
        private string _videoMailUri;
        private string _mwiUri;
        private string _preferredVideoId;
        private string _transport;
        private string _mediaEncryption;
        private string _loginMethod;

        // I do not think that we want to write this out locally, we will be looking it up.
        //   Stashing it here for now so that we can reference it for the technical support sheet.
        //   Also, I think that we may wind up referencing this instead of copying the information over as we proceed.
        public ACEConfig configuration;
        #endregion

        #region Properties

        [Column(IsPrimaryKey = true, DbType = "NVARCHAR(50) NOT NULL ", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public string AccountID { get; set; }

        [Column]
        public ACEConfigStatusType configStatus { get; set; }

        [Column]
        public string AuthID { get; set; }

        [Column]
        public string Username { get; set; }

        // [ Column ]
        public string Password { get; set; }

        [Column]
        public string PhoneNumber { get; set; }

        [Column]
        public string Provider { get; set; }

        [Column]
        public string DialAroundProviderAddress { get; set; }

        [Column]
        public VATRPAccountType AccountType { get; set; }

//        [Column]
//        public bool AutoLogin { get; set; }

        [Column]
        public bool RememberPassword { get; set; }

        [Column]
        public string RegistrationUser { get; set; }

        [Column]
        public string RegistrationPassword { get; set; }

        [Column]
        public string ProxyHostname { get; set; }

        [Column]
        public bool UseOutboundProxy { get; set; }

        [Column]
        public string OutboundProxy { get; set; }

        [Column]
        public string DisplayName { get; set; }

        [Column]
        public ushort HostPort { get; set; }

        [Column]
        public string Transport
        {
            get { return _transport; }
            set
            {
                _transport = !string.IsNullOrWhiteSpace(value) ? value : "TCP"; 
            }
        }

        [Column]
        public string LoginMethod
        {
            get { return _loginMethod; }
            set
            {
                _loginMethod = !string.IsNullOrWhiteSpace(value) ? value : "Old";
            }
        }

        [Column]
        public string MediaEncryption
        {
            get { return _mediaEncryption; }
            set
            {
                _mediaEncryption = !string.IsNullOrWhiteSpace(value) ? value : "Unencrypted";
            }
        }

        [Column]
        public bool EnableSTUN { get; set; }

        [Column]
        public bool EnableTURN { get; set; }

        [Column]
        public string STUNAddress { get; set; }

        [Column]
        public string STUNUsername { get; set; }

        [Column]
        public ushort STUNPort { get; set; }
        public ushort DefaultSTUNPort { get; set; }

        [Column]
        public bool EnableICE { get; set; }

        [Column]
        public string ICEAddress { get; set; }

        [Column]
        public ushort ICEPort { get; set; }

        [Column] 
        public bool EnableAVPF { get; set; }

        [Column]
        public bool MuteMicrophone { get; set; }

        [Column]
        public bool MuteSpeaker { get; set; }

        [Column]
        public bool EnableTechCallLog { get; set; } // cjm-aug17
        
        [Column]
        public bool EnableProviderPingLog { get; set; } // cjm-aug17

        [Column]
        public string SelectedCameraId { get; set; }

        [Column]
        public string SelectedSpeakerId { get; set; }

        [Column]
        public string SelectedMicrophoneId { get; set; }
        
        [Column]
        public bool EchoCancel { get; set; }

        [Column]
        public bool VideoAutomaticallyStart { get; set; }

        [Column]
        public bool VideoAutomaticallyAccept { get; set; }

        private bool enableVideo;
        [Column]
        public bool EnableVideo 
        {
            get
            {
                return true;
            }
            set
            {
                enableVideo = value;
            }
        }

        [Column]
        public bool ShowSelfView { get; set; }

        [Column]
        public string PreferredVideoId
        {
            get { return _preferredVideoId; }
            set
            {
                _preferredVideoId = !string.IsNullOrWhiteSpace(value) ? value : "cif";
            }
        }

        [Column]
        public string VideoPreset
        {
            get { return _videoPreset; }
            set
            {
                // Liz E. - linphone uses null to get the default preset. Allow null here.
                _videoPreset = value;
            }
        }

        [Column]
        public string VideoMailUri
        {
            get { return _videoMailUri; }
            set
            {
                // Liz E. - linphone uses null to get the default preset. Allow null here.
                _videoMailUri = value;
            }
        }

        [Column]
        public string MWIUri
        {
            get { return _mwiUri; }
            set
            {
                _mwiUri = value;
            }
        }

        public List<VATRPCodec> AudioCodecsList = new List<VATRPCodec>();
        public List<VATRPCodec> VideoCodecsList = new List<VATRPCodec>();

        [Column]
        public bool UserNeedsAgentView { get; set; }

        [Column]
        public int VideoMailCount { get; set; }

        [Column]
        public float PreferredFPS { get; set; }

        [Column]
        public bool EnableAdaptiveRate { get; set; }
        [Column]
		
        public string AdaptiveRateAlgorithm { get; set; }
        [Column]
        public int UploadBandwidth { get; set; }

        [Column]
        public int DownloadBandwidth { get; set; }

        [Column]
        public bool EnableQualityOfService { get; set; }

        [Column]
        public int SipDscpValue { get; set; }

        [Column]
        public int AudioDscpValue { get; set; }

        [Column]
        public int VideoDscpValue { get; set; }

        [Column]
        public bool EnableIPv6 { get; set; }

        [Column]
        public string Logging { get; set; }

        [Column]
        public string RTTFontFamily { get; set; }

        [Column]
        public string CardDavServerPath { get; set; }

        [Column]
        public string CardDavRealm { get; set; }

        [Column]
        public string ContactsURI { get; set; }

        [Column]
        public string CDN { get; set; }  

        [Column]
        public string GeolocationURI { get; set; }

        [Column]
        public bool SendLocationWithRegistration { get; set; }

        [Column]
        public int RTTFontSize { get; set; }

        [Column]
        public bool DisableAudioCodecs { get; set; }

        [Column]
        public bool EnablePrivacy { get; set; }

        #endregion

        #region Methods

        public VATRPAccount()
        {
            configuration = new ACEConfig(); // cjm-sep17
            AccountID = Guid.NewGuid().ToString();
            HostPort = Configuration.LINPHONE_SIP_PORT;
            ProxyHostname = Configuration.LINPHONE_SIP_SERVER;
            OutboundProxy = "";
            Transport = "TCP";
            MediaEncryption = "Unencrypted";
            EnableAVPF = false;
            PreferredVideoId = "cif";
            STUNPort = 3478;
            DefaultSTUNPort = 3478;
            //STUNAddress = $"stun.server.com:{DefaultSTUNPort}";
            STUNAddress = string.Empty;
            AuthID = string.Empty;
            Username = string.Empty;
            Password = string.Empty;
            Provider = string.Empty;
            RegistrationUser = string.Empty;
            RegistrationPassword = string.Empty;
            DisplayName = string.Empty;
            VideoAutomaticallyStart = true;
            VideoAutomaticallyAccept = true;
            EnableVideo = true;
            ShowSelfView = true;
            PreferredVideoId = string.Empty;
            VideoPreset = null;
            MuteMicrophone = false;
            MuteSpeaker = false;
            EnableTechCallLog = true; //cjm-aug17
            EnableProviderPingLog = true; //cjm-aug17
            EchoCancel = true;
            UseOutboundProxy = true; // CHANGED ON DATED 09-12-2016 BY MK 
            VideoPreset = "high-fps";
            SelectedCameraId = string.Empty;
            SelectedMicrophoneId = string.Empty;
            SelectedSpeakerId = string.Empty;
            UserNeedsAgentView = false;
            VideoMailCount = 0;
            PreferredFPS = 30;
            EnableAdaptiveRate = true;
            UploadBandwidth = 1500;
            DownloadBandwidth = 1500;
            EnableQualityOfService = true;
            AdaptiveRateAlgorithm = "Simple";
            SipDscpValue = 24;
            AudioDscpValue = 46;
            VideoDscpValue = 46;
            EnableIPv6 = false;
            Logging = "Info";
            RTTFontFamily = "Segoe UI";
            CardDavServerPath = string.Empty;
            CardDavRealm = string.Empty;
            ContactsURI = string.Empty;
            CDN = string.Empty;
            GeolocationURI = string.Empty;
            SendLocationWithRegistration = false;
            DisableAudioCodecs = false;
            EnableSTUN = true;
            EnableICE = false;
            EnablePrivacy = false;
        }
		
        public void StorePassword(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && !string.IsNullOrEmpty(Password))
            {
                DataProtectionHelper.WriteProtectedBytesToFile(filePath, Password);
                Password = "";
            }
        }

        public void ReadPassword(string filePath)
        {
            // if autologin is set, load the password for this user
            if (!string.IsNullOrEmpty(filePath)) 
            {
                // method below handles checking for file existance
                Password = DataProtectionHelper.ReadUnprotectedBytesFromProtectedFile(filePath);
            }
        }

        #endregion
    }
}
