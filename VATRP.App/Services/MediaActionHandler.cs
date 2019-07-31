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

using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using VATRP.Core.Extensions;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using VATRP.Core.Services;
using VATRP.LinphoneWrapper.Enums;

namespace com.vtcsecure.ace.windows.Services
{
    internal static class MediaActionHandler
    {
        /// <summary>
        /// Configures the client for placing phone calls and validates user input.
        /// </summary>
        /// <remarks>
        /// Remote URI Format: sip:user@sip.linphone.org
        /// </remarks>
        /// <param name="remoteUri"> The SIP URI the call is being placed to.</param>
        /// <returns>True if the call was placed successfully.</returns>
        internal static bool MakeVideoCall(string remoteUri) 
        {
            string dialPadPhonePattern = @"^(?:(?:\(?(?:00|\+)([1-4]\d\d|[1-9]\d?)\)?)?[\-\.\ \\\/]?)?((?:\(?\d{1,}\)?[\-\.\ \\\/]?){0,})(?:[\-\.\ \\\/]?(?:#|ext\.?|extension|x)[\-\.\ \\\/]?(\d+))?$";
            string pattern = @"[ \-\s\t\n\r\(\)\*\#\+\.]*";
            string replacement = "";
            Regex rgx = new Regex(pattern);
            Regex dialPadPhoneCheckRgx = new Regex(dialPadPhonePattern);

            //string result = rgx.Replace(remoteUri, replacement);
            Match match = dialPadPhoneCheckRgx.Match(remoteUri);
            if (match.Success)
            {
                string result = rgx.Replace(remoteUri, replacement);
                // Perserving the leading 1 if there is one:
                //
                /*
                if (result.Length > 10 && result[0] == '1')
                    result = result.Remove(0, 1);*/
                remoteUri = result;
            }

            //******************************** Maake Video Call **********************************************************************************************
            // This method is called when user tap on call button on dial pad. or When user select the Contact from Contact list (All/Favorites) 
            // It will called only when there is a valid number entered for a call (After validation)
            //*************************************************************************************************************************************************
            ILinphoneService _linphoneService = ServiceManager.Instance.LinphoneService;

            if (!_linphoneService.CanMakeVideoCall())
            {
                MessageBox.Show("Video call not supported yet.", "VATRP", MessageBoxButton.OK,
                   MessageBoxImage.Warning);
                return false;
            }

            if (MainWindow.RegistrationState != LinphoneRegistrationState.LinphoneRegistrationOk)
            {
                MessageBox.Show("Not Registered. Please register first", "VATRP", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
            // VATRP-2506, 2496: check to see if there is currently a call in progress. IF there is a call in progress, prevent 
            //   an outgoing call. 
            int callCount = ServiceManager.Instance.LinphoneService.GetActiveCallsCount; // It checks if user already connected a call. if yes then it returns false.
            if (callCount > 0)
            {
                return false;  // i think this can be a quiet failure - the user is already in a call.
            }
            
            bool muteMicrophone = false;
            bool muteSpeaker = false;
//            bool enableVideo = true;
            if (App.CurrentAccount != null)
            {
                muteMicrophone = App.CurrentAccount.MuteMicrophone;
                muteSpeaker = App.CurrentAccount.MuteSpeaker;
//                enableVideo = App.CurrentAccount.EnableVideo;
            }

            var target = string.Empty;

            string un, host;
            int port;
            VATRPCall.ParseSipAddress(remoteUri, out un, out host, out port);

            // https://www.twilio.com/docs/glossary/what-e164
            // https://en.wikipedia.org/wiki/E.164
            Regex rE164 = new Regex(@"^(\+|00)?[1-9]\d{4,14}$");
            bool isE164 = rE164.IsMatch(un);

            if (!host.NotBlank())
            {
                // set proxy to selected provider
                // find selected provider host
                var provider =
                    ServiceManager.Instance.ProviderService.FindProviderLooseSearch(
                        ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                            Configuration.ConfEntry.CURRENT_PROVIDER, ""));
                
                //Use dial-around provider if selected
                if (!string.IsNullOrEmpty(App.CurrentAccount?.DialAroundProviderAddress))
                {
                    target = string.Format("sip:{0}@{1}", un, App.CurrentAccount.DialAroundProviderAddress);
                }
                else if (provider != null)
                {
                    target = string.Format("sip:{0}@{1}", un, provider.Address);
                }
                else if (App.CurrentAccount != null)
                {
                    target = string.Format("sip:{0}@{1}", un, App.CurrentAccount.ProxyHostname);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(App.CurrentAccount?.DialAroundProviderAddress))
                {
                    target = string.Format("sip:{0}@{1}", un, App.CurrentAccount.DialAroundProviderAddress);
                }
                else
                {
                    target = string.Format("sip:{0}@{1}:{2}", un, host, port);
                }
            }

            if (isE164)
            {
                target += ";user=phone";
            }
            else
            {
                target += ";user=dialstring";
            }

            var privacyMask = VATRP.Core.Enums.LinphonePrivacy.LinphonePrivacyDefault;
            if (App.CurrentAccount.EnablePrivacy)
            {
                privacyMask = VATRP.Core.Enums.LinphonePrivacy.LinphonePrivacySession;
            }
            
        // update video policy settings prior to making a call
        _linphoneService.MakeCall(target, /* destination */
                                      true, /* videoOn */
                                      ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                                        Configuration.ConfEntry.USE_RTT, true), /* rttEnabled */
                                      muteMicrophone, /* muteMicrophone */
                                      muteSpeaker, /* muteSpeaker */
                                      true, /* enableVideo */
                                      App.CurrentAccount.GeolocationURI, /* geolocation */
                                      privacyMask); /* privacyMask */
            return true;
        }
       
    }
}
