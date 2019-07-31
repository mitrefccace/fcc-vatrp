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
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.Utilities
{
    public static class TechnicalSupportInfoBuilder
    {
        public const string TECHNICAL_SUPPORT_FILE_NAME = "TechnicalSupportSheet.txt";
        public static string GetACEVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;

            return string.Format("Version {0}.{1}.{2}", version.Major, version.Minor, version.Build);
        }

        public static string GetLinphoneVersion()
        {
            return VATRP.LinphoneWrapper.LinphoneAPI.linphone_core_get_version_asString();
        }

        public static string CreateAndGetTechnicalSupportInfoAsTextFile(bool verbose)
        {
            string data = GetFullTechnicalSupportStringWithOs(verbose);
            string technicalSupportFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            technicalSupportFilePath += Path.DirectorySeparatorChar + TECHNICAL_SUPPORT_FILE_NAME;
            if (File.Exists(technicalSupportFilePath))
            {
                File.Delete(technicalSupportFilePath);
            }
            try
            {
                if (!File.Exists(technicalSupportFilePath))
                {
                    // Create a file to write to.
                    using (StreamWriter sw = new StreamWriter(File.Open(technicalSupportFilePath, FileMode.Create), Encoding.UTF8))
                    {
                        sw.Write(data);
                    }
                }
                return technicalSupportFilePath;
            }
            catch (Exception ex)
            {
                // unable to write the file
                return ""; // we were unable to write the file
            }
        }

        public static string GetFullTechnicalSupportStringWithOs(bool verbose)
        {
            StringBuilder tssString = new StringBuilder();
            tssString.AppendLine("VATRP Version: " + GetACEVersion());
            tssString.AppendLine("Core Version: " + GetLinphoneVersion());
            tssString.AppendLine("Operating System: " + GetFriendlyOsNameWithServicePack());
            tssString.AppendLine("  Full Build Version: " + Environment.OSVersion.VersionString);
            tssString.AppendLine("Configuration Information:");
            tssString.AppendLine(GetStringForTechnicalSupprtString(true));

            return tssString.ToString();
        }

        /// <summary>
        /// Returns information about the system configuration. 
        /// The Verbose flag will ultimately be used to help us determine how much information to include
        /// in case we want different information in an email versus what we display on screen.
        /// </summary>
        /// <param name="verbose"></param>
        /// <returns></returns>
        public static string GetStringForTechnicalSupprtString(bool verbose)
        {
            StringBuilder configString = new StringBuilder();
            if (App.CurrentAccount != null)
            {
                ACEConfig config = App.CurrentAccount.configuration;
                // Liz E. Note: Commenting out the items that are not yet being stored above. Uncomment and access correct information as needed
                //  (rather than printing items that are not yet in use stored)
                //        public int version { get; set; }
                if (config != null)
                {
                    configString.AppendLine("Configuration Version: " + config.version);
                    configString.AppendLine("Expiration Time: " + config.expiration_time); // this should be converted to date time, I am sure
                    //       public List<string> sip_register_usernames { get; set; }
                    configString.AppendLine("SIP Register Usernames: " + string.Join(", ", config.sip_register_usernames.ToArray()));
                }
                else
                {
                    configString.AppendLine("Configuration Version: defaults used - no config found");
                    configString.AppendLine("Expiration Time: n/a"); // this should be converted to date time, I am sure
                }
                //         public int expiration_time { get; set; }

                // not sure we want auth information printed in the technical spec sheet. name maybe, but not password.
                //       public string configuration_auth_password { get; set; }
                //       public int configuration_auth_expiration { get; set; }

                //       public int sip_registration_maximum_threshold { get; set; }
                //configString.AppendLine("SIP Registration Maximum Threshold: " + config.sip_registration_maximum_threshold);

                //       public string sip_auth_username { get; set; }
                configString.AppendLine("Username: " + (App.CurrentAccount.Username ?? ""));
                //       public string sip_auth_password { get; set; }
                //configString.AppendLine("SIP Auth Password: " + sip_auth_password);
                //       public string sip_register_domain { get; set; }
                configString.AppendLine("Domain: " + App.CurrentAccount.ProxyHostname);
                //       public int sip_register_port { get; set; }
                configString.AppendLine("Port: " + App.CurrentAccount.HostPort);
                //       public string sip_register_transport { get; set; }
                configString.AppendLine("Transport: " + App.CurrentAccount.Transport);

                //       public bool enable_echo_cancellation { get; set; }
                configString.AppendLine("Enable Echo Cancellation: " + App.CurrentAccount.EchoCancel.ToString());
                //       public bool enable_video { get; set; }
                configString.AppendLine("Enable Video: " + App.CurrentAccount.EnableVideo.ToString());
                //       public bool enable_rtt { get; set; }
                bool enable_rtt = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL, Configuration.ConfEntry.USE_RTT, true);
                configString.AppendLine("Enable RTT: " + enable_rtt.ToString());
                //       public bool enable_adaptive_rate { get; set; }
                configString.AppendLine("Enable AVPF: " + App.CurrentAccount.EnableAVPF.ToString()); // is this correct?
                //       public bool enable_stun { get; set; }
                configString.AppendLine("Enable STUN: " + App.CurrentAccount.EnableSTUN.ToString());
                //       public string stun_server { get; set; }
                configString.AppendLine("STUN Server: " + (App.CurrentAccount.STUNAddress ?? ""));
                configString.AppendLine("STUN Port: " + App.CurrentAccount.STUNPort);
                //       public bool enable_ice { get; set; }
                configString.AppendLine("Enable ICE: " + App.CurrentAccount.EnableICE.ToString());
                configString.AppendLine("Enable IPv6: " + App.CurrentAccount.EnableIPv6.ToString());
                //       public List<string> enabled_codecs { get; set; }
                //           configString.AppendLine("Enabled Codecs: " + string.Join(", ", enabled_codecs.ToArray()));
                //       public string bwLimit { get; set; }

                //       public int upload_bandwidth { get; set; }
                //       public int download_bandwidth { get; set; }
                //       public string logging { get; set; }
                //       public string sip_mwi_uri { get; set; }
                //       public string video_resolution_maximum { get; set; }

                //       public string sip_videomail_uri { get; set; }
                configString.AppendLine("Video Mail URI: " + (App.CurrentAccount.VideoMailUri ?? ""));

                string linphoneInfo = ServiceManager.Instance.LinphoneService.GetTechnicalSupportInfo();
                if (linphoneInfo != null) 
                    configString.Append(linphoneInfo);
                //        public bool user_is_agent { get; set; } // do not include this yet - this is a POC feature at the moment
            }
            return configString.ToString();
        }


        // move these next two methods into builder class so that all of this information can readily be used to email the tss.
        // also make an accessor for the friendly os name.
        public static string HKLM_GetString(string path, string key)
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(path);
                if (rk == null) return "";
                return (string)rk.GetValue(key);
            }
            catch { return ""; }
        }

        public static string GetFriendlyOsName()
        {
            string ProductName = HKLM_GetString(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName");
            string CSDVersion = HKLM_GetString(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CSDVersion");
            if (ProductName != "")
            {
                return (ProductName.StartsWith("Microsoft") ? "" : "Microsoft ") + ProductName +
                            (CSDVersion != "" ? " " + CSDVersion : "");
            }
            return "";
        }

        public static string GetFriendlyOsNameWithServicePack()
        {
             OperatingSystem os = Environment.OSVersion;
             return TechnicalSupportInfoBuilder.GetFriendlyOsName() + " " + os.ServicePack;

        }

    }
}
