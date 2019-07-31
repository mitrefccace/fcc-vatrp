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
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace VATRP.Core.Model
{
    public enum ACEConfigStatusType
    {
        LOGIN_SUCCEESSFUL,
        LOGIN_UNAUTHORIZED,
        SRV_RECORD_NOT_FOUND,
        UNABLE_TO_PARSE,
        CONNECTION_FAILED,
        UNKNOWN
    }

    public class Ice_server
    {
        public string uri { get; set; }

        public Ice_server()
        {

        }
    }

    public class ACEConfig
    {
        [JsonProperty("display-name")]
        public string display_name { get; set; }

        [JsonProperty("phone-number")]
        public string phone_number { get; set; }

        [JsonProperty("outbound-proxies")]
        public List<string> outbound_proxies { get; set; }

        [JsonProperty]
        public bool sendLocationWithRegistration { get; set; }

        [JsonProperty]
        public string carddav { get; set; }

        [JsonProperty("ice-servers")]
        public List<Ice_server> ice_servers = new List<Ice_server>();

        [JsonProperty]
        public ACEConfigStatusType configStatus { get; set; }

        [JsonProperty]
        public int version { get; set; }

        [JsonProperty("lifetime")]
        public int expiration_time { get; set; }

        [JsonProperty]
        public string configuration_auth_password { get; set; }

        [JsonProperty]
        public int configuration_auth_expiration { get; set; }

        [JsonProperty]
        public int sip_registration_maximum_threshold { get; set; }

        [JsonProperty]
        public List<string> sip_register_usernames { get; set; }

        [JsonProperty]
        public string sip_auth_username { get; set; }

        [JsonProperty]
        public string sip_auth_password { get; set; }

        [JsonProperty("provider-domain")]
        public string sip_register_domain { get; set; }

        [JsonProperty("sip-port")]
        public int sip_register_port { get; set; }

        [JsonProperty]
        public string sip_register_transport { get; set; }

        [JsonProperty]
        public bool enable_echo_cancellation { get; set; }

        [JsonProperty]
        public bool enable_video { get; set; }

        [JsonProperty]
        public bool enable_rtt { get; set; }

        [JsonProperty]
        public bool enable_adaptive_rate { get; set; }

        [JsonProperty]
        public bool enable_stun { get; set; }

        [JsonProperty]
        public bool enable_turn { get; set; }

        [JsonProperty]
        public string stun_server { get; set; }

        [JsonProperty]
        public bool enable_ice { get; set; }

        [JsonProperty]
        public int upload_bandwidth { get; set; }

        [JsonProperty]
        public int download_bandwidth { get; set; }

        [JsonProperty]
        public string logging { get; set; }

        [JsonProperty("mwi")]
        public string sip_mwi_uri { get; set; }

        [JsonProperty("videomail")]
        public string sip_videomail_uri { get; set; }

        [JsonProperty]
        public string video_resolution_maximum { get; set; }

        [JsonProperty]
        public bool user_is_agent { get; set; }

        [JsonProperty]
        public string bwLimit { get; set; }

        [JsonProperty]
        public List<string> enabled_codecs { get; set; }

        [JsonProperty]
        public List<VATRPCredential> credentials = new List<VATRPCredential>(); // cjm-sep17

        [JsonProperty]
        public DateTime downloadDate; // cjm-sep17 -- configuration refresh 

        [JsonProperty]
        public string contacts { get; set; } //cjm-sep17 for XML import

        [JsonProperty]
        public bool ping_provider { get; set; } // cjm-sep17

        [JsonProperty]
        public bool log_call { get; set; } // cjm-sep17

        public ACEConfig()
        {
            configStatus = ACEConfigStatusType.UNKNOWN;
            sip_register_usernames = new List<string>();
            enabled_codecs = new List<string>();
            ice_servers = new List<Ice_server>();
        }

        // cjm-sep17 -- configuration refresh 
        public bool HasExpired()
        {
            if (DateTime.Compare(downloadDate, DateTime.MinValue) != 0)
            {
                DateTime today = DateTime.Now;
                TimeSpan interval = today - downloadDate;
                if (interval.Days <= this.expiration_time)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true; // cjm-sep17 -- force download for first time
            }
        }

        // cjm-sep17 -- store this information in order to determine age of the cached config file
        public void SetDownloadDate()
        {
            downloadDate = DateTime.Now;
        }

        // cjm-sep17  
        public VATRPCredential FindCredential(string serviceId, string username)
        {
            if (username != null)
            {
                return credentials.Find(x => x.realm == serviceId && x.username == username);
            }
            else
            {
                return credentials.Find(x => x.realm == serviceId);
            }
        }

        public void NormalizeValues()
        {
            display_name = NormalizeValue(display_name);
            outbound_proxies = NormalizeValues(outbound_proxies);
            foreach (Ice_server server in ice_servers)
            {
                server.uri = NormalizeValue(server.uri);
            }

            configuration_auth_password = NormalizeValue(configuration_auth_password);
            //        public List<string> sip_register_usernames { get; set; }
            sip_register_usernames = NormalizeValues(sip_register_usernames);
            //        public string sip_auth_username { get; set; }
            sip_auth_username = NormalizeValue(sip_auth_username);
            //        public string sip_auth_password { get; set; }
            sip_auth_password = NormalizeValue(sip_auth_password);
            //        public string sip_register_domain { get; set; }
            sip_register_domain = NormalizeValue(sip_register_domain);
            //        public string sip_register_transport { get; set; }
            sip_register_transport = NormalizeValue(sip_register_transport);

            //        public string stun_server { get; set; }
            stun_server = NormalizeValue(stun_server);

            //        public List<string> enabled_codecs { get; set; }
            enabled_codecs = NormalizeValues(enabled_codecs);
            //        public string bwLimit { get; set; }
            bwLimit = NormalizeValue(bwLimit);
            //        public string logging { get; set; }
            logging = NormalizeValue(logging);
            //        public string sip_mwi_uri { get; set; }
            sip_mwi_uri = NormalizeValue(sip_mwi_uri);
            //        public string sip_videomail_uri { get; set; }
            sip_videomail_uri = NormalizeValue(sip_videomail_uri);
            //        public string video_resolution_maximum { get; set; }
            video_resolution_maximum = NormalizeValue(video_resolution_maximum);
        }
        private List<string> NormalizeValues(List<string> values)
        {
            if (values == null)
            {
                values = new List<string>();
            }
            for (int i = 0; i < values.Count; i++)
            {
                values[i] = NormalizeValue(values[i]);
            }
            return values;
        }
        private string NormalizeValue(string value)
        {
            // these are spefcific cases to return ""
            if (string.IsNullOrEmpty(value) || value.Equals("\"") || value.Equals("\"\""))
            {
                return "";
            }
            else
            {
                // remove start and end quotes - but not all quotes in case there is data that allows quotes later.
                if (value.EndsWith("\""))
                {
                    value = value.Substring(0, value.Length - 1);
                }
                if (value.StartsWith("\""))
                {
                    if (value.Length == 1)
                        return "";
                    if (value.Length > 1)
                    {
                        return value.Substring(1, value.Length - 1);
                    }
                }
            }
            return value;
        }

        ///<summary>
        /// Method called when a user attempts to load a config
        /// file in through the Login page. This Update method
        /// is set to work with the config file formatted as
        /// specified in the RUE spec section 11.2
        ///</summary>
        ///<param name="accountToUpdate">The account to update with the config file contents</param>
        public void UpdateVATRPAccountFromACEConfig_login(VATRPAccount accountToUpdate)
        {
            // Fields Required from config file
            accountToUpdate.configuration = this;
            accountToUpdate.ProxyHostname = this.sip_register_domain;
            accountToUpdate.RegistrationUser = this.phone_number;
            accountToUpdate.Username = this.phone_number;
            accountToUpdate.PhoneNumber = this.phone_number;

            // Set the outbound proxy if there are any loaded from the config file
            if (this.outbound_proxies.Any())
            {
                accountToUpdate.OutboundProxy = this.outbound_proxies[0];
                accountToUpdate.UseOutboundProxy = true;
            }

            int port = this.sip_register_port;
            if (port > 0)
            {
                accountToUpdate.HostPort = (UInt16)port;
                accountToUpdate.Transport = "UDP";
            }
            accountToUpdate.ContactsURI = this.contacts;
            accountToUpdate.SendLocationWithRegistration = this.sendLocationWithRegistration;

            // Find set of credentials that will be used
            accountToUpdate.RegistrationUser = "";
            accountToUpdate.Username = "";
            accountToUpdate.RegistrationPassword = "";
            accountToUpdate.Password = "";
            accountToUpdate.AuthID = "";
            foreach (VATRPCredential credential in this.credentials)
            {
                if (credential.realm == sip_register_domain)
                {
                    // Update just the current account, not the config file
                    accountToUpdate.RegistrationPassword = credential.password;
                    accountToUpdate.Password = credential.password;
                    accountToUpdate.AuthID = credential.username;
                    break;
                }
            }

            // Fields Optional from config file
            accountToUpdate.DisplayName = this.display_name;
            accountToUpdate.MWIUri = this.sip_mwi_uri;
            accountToUpdate.VideoMailUri = this.sip_videomail_uri;
            accountToUpdate.CardDavRealm = this.carddav;
            if (this.ice_servers.Count > 0)
            {
                accountToUpdate.ICEAddress = ice_servers[0].uri;
            }
        }

        public void UpdateVATRPAccountFromACEConfig(VATRPAccount accountToUpdate)
        {
            // items not handled
            //        public int version { get; set; }
            //         public int expiration_time { get; set; }

            //       public string configuration_auth_password { get; set; }
            //       public int configuration_auth_expiration { get; set; }

            //       public int sip_registration_maximum_threshold { get; set; }
            //       public List<string> sip_register_usernames { get; set; }

            //       public List<string> enabled_codecs { get; set; }
            //       public string bwLimit { get; set; }
            //       public int upload_bandwidth { get; set; }
            //       public int download_bandwidth { get; set; }
            //       public string logging { get; set; }
            //       public string sip_mwi_uri { get; set; }

            //       public string video_resolution_maximum { get; set; }

            //       public bool enable_rtt { get; set; }  --> set in configuration service

            //        public bool user_is_agent { get; set; }
            //var trimChars = new[] { '\"' };
            accountToUpdate.configuration = this;
            accountToUpdate.configStatus = this.configStatus;
            //       public string sip_auth_username { get; set; }
            string phone_number = "";
            if (!string.IsNullOrEmpty(this.phone_number))
            {
                phone_number = this.phone_number;
                if (!string.IsNullOrWhiteSpace(phone_number))
                {
                    accountToUpdate.RegistrationUser = phone_number;
                    accountToUpdate.Username = phone_number;
                    accountToUpdate.PhoneNumber = phone_number;
                }
            }
            //       public string sip_auth_password { get; set; }
            string password = "";
            if (!string.IsNullOrEmpty(this.sip_auth_password))
            {
                password = this.sip_auth_password;
                if (!string.IsNullOrWhiteSpace(password))
                {
                    accountToUpdate.RegistrationPassword = password;
                    accountToUpdate.Password = password;
                }
            }
            //       public string sip_register_domain { get; set; }
            string domain = "";
            if (!string.IsNullOrEmpty(this.sip_register_domain))
            {
                domain = this.sip_register_domain;
                if (!string.IsNullOrWhiteSpace(domain))
                {
                    accountToUpdate.ProxyHostname = domain;
                }
            }
            //       public int sip_register_port { get; set; }
            var port = this.sip_register_port;
            if (port > 0)
            {
                accountToUpdate.HostPort = (UInt16)port;
            }
            //       public string sip_register_transport { get; set; }
            string transport = "";
            if (!string.IsNullOrEmpty(this.sip_register_transport))
            {
                transport = this.sip_register_transport;
                if (!string.IsNullOrWhiteSpace(transport))
                {
                    accountToUpdate.Transport = transport;
                }
            }

            //       public bool enable_echo_cancellation { get; set; }
            accountToUpdate.EchoCancel = this.enable_echo_cancellation;
            //       public bool enable_video { get; set; }
            accountToUpdate.EnableVideo = this.enable_video;
            accountToUpdate.VideoAutomaticallyStart = this.enable_video;

            //       public bool enable_adaptive_rate { get; set; }
            // TEMP - 3512, ignore accountToUpdate.EnableAdaptiveRate = this.enable_adaptive_rate;
            accountToUpdate.EnableAdaptiveRate = this.enable_adaptive_rate;
            accountToUpdate.DownloadBandwidth = this.download_bandwidth;
            accountToUpdate.UploadBandwidth = this.upload_bandwidth;

            //       public bool enable_stun { get; set; }
            accountToUpdate.EnableSTUN = this.enable_stun;
            accountToUpdate.EnableTURN = this.enable_turn;
            //       public string stun_server { get; set; }
            accountToUpdate.STUNAddress = this.stun_server ?? string.Empty;
            var stunServer = accountToUpdate.STUNAddress.Split(':');
            if (stunServer.Length > 1)
            {
                accountToUpdate.STUNAddress = stunServer[0];
                accountToUpdate.STUNPort = Convert.ToUInt16(stunServer[1]);
            }
            //       public bool enable_ice { get; set; }
            accountToUpdate.EnableICE = this.enable_ice;

            //       public string sip_videomail_uri { get; set; }
            accountToUpdate.VideoMailUri = (sip_videomail_uri ?? string.Empty);

            // on successful login, we need to update the following in config: (list in progress)
            // this.enable_rtt;
            accountToUpdate.EnableProviderPingLog = this.ping_provider;
            accountToUpdate.EnableTechCallLog = this.log_call;
            accountToUpdate.UserNeedsAgentView = user_is_agent;
            // not working, commenting out to get the other items in
            /*
            // update available codecs
            accountToUpdate.AvailableAudioCodecsList.Clear();
            accountToUpdate.AvailableVideoCodecsList.Clear();
            bool pcmuAvailable = false;
            bool pcmaAvailable = false;
            foreach (VATRPCodec codec in accountToUpdate.AudioCodecsList)
            {
                if (codec.CodecName.ToLower().Equals("pcmu"))
                {
                    pcmuAvailable = true;
                }
                if (codec.CodecName.ToLower().Equals("pcma"))
                {
                    pcmaAvailable = true;
                }

                foreach (string enabled_codec in enabled_codecs)
                {
                    string codecName = enabled_codec.Replace(".", "");
                    if (!string.IsNullOrEmpty(codecName))
                    {
                        if (codecName.ToLower().Equals(codec.CodecName.ToLower()))
                        {
                            accountToUpdate.AvailableAudioCodecsList.Add(codec);
                        }
                    }
                }
            }
            // handle special cases
            if (pcmuAvailable && pcmaAvailable)
            {
                // add the g711 codec for display
                VATRPCodec newCodec = new VATRPCodec();
                newCodec.CodecName = "G711";
                newCodec.Description = "";
                newCodec.Channels = 0;
                newCodec.IPBitRate = 0;
                newCodec.IsUsable = false;
                newCodec.Priority = -1;
                newCodec.Rate = 0;
                if (accountToUpdate.AvailableAudioCodecsList.Count > 0)
                {
                    int index = 0;
                    foreach (VATRPCodec codec in accountToUpdate.AvailableAudioCodecsList)
                    {
                        index++;
                        if (codec.CodecName.ToLower().Equals("g722"))
                        {
                            accountToUpdate.AvailableAudioCodecsList.Insert(index, newCodec);
                        }
                    }
                }
                else
                {
                    accountToUpdate.AvailableAudioCodecsList.Add(newCodec);
                }
            }

            foreach (VATRPCodec codec in accountToUpdate.VideoCodecsList)
            {
                foreach (string enabled_codec in enabled_codecs)
                {
                    string codecName = enabled_codec.Replace(".", "");
                    if (codecName.ToLower().Equals(codec.CodecName.ToLower()))
                    {
                        accountToUpdate.AvailableVideoCodecsList.Add(codec);
                    }
                }
            }

            */
            //implimment codec selection support

            /*
            newAccount.MuteMicrophone //missing
             *  public bool  enable_ice { get; set; } missing
             *         public bool  enable_rtt { get; set; }     //missing
             *         
             *         public int version { get; set; }// missing
             *          public string logging { get; set; } missing
             *          public bool AutoLogin { get; set; //missing
             *          sip_register_usernames //missing
            */

        }


    }
}
