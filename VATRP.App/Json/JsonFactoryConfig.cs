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
using System.Net;
using System.IO;
using HockeyApp;
using System.Threading.Tasks;
using VATRP.Core.Model;
using com.vtcsecure.ace.windows.Utilities;

namespace com.vtcsecure.ace.windows.Json
{
    // note - Config.cs moved into Core.Model.ACEConfig
    public class JsonFactoryConfig
    {
        public readonly static string defaultConfigString = "{\"version\":1,\"expiration_time\":280," +
            "\"configuration_auth_password\":null," +
            "\"configuration_auth_expiration\":-1," +
            "\"sip_registration_maximum_threshold\":10," +
            "\"sip_register_usernames\":[]," + 
            "\"sip_auth_username\":null," +
            "\"sip_auth_password\":null," +
            "\"sip_register_domain\":null," +
            "\"sip_register_port\":5060," +
            "\"sip_register_transport\":\"tcp\"," +
            "\"enable_echo_cancellation\":true," +
            "\"enable_video\":true," +
            "\"enable_rtt\":true," +
            "\"enable_adaptive_rate\":true," +
            "\"enabled_codecs\":[\"H.264\",\"H.263\",\"VP8\",\"G.722\",\"G.711\"]," +
            "\"bwLimit\":\"high-fps\"," +
            "\"upload_bandwidth\":1500," +"\"download_bandwidth\":1500," +
            "\"enable_stun\":false," +
            "\"stun_server\":null," +
            "\"enable_ice\":true," +
            "\"logging\":\"info\"," +
            "\"sip_mwi_uri\":null," +
            "\"sip_videomail_uri\":null," +"\"video_resolution_maximum\":\"cif\"}";
/* @"{
  'version': 1,
  'expiration_time': 3600,
  'configuration_auth_password': '""',
  'configuration_auth_expiration': -1,
  'sip_registration_maximum_threshold': 10,
  'sip_register_usernames': [],
  'sip_auth_username': '""',
  'sip_auth_password': '""',
  'sip_register_domain': 'bc1.vatrp.com',
  'sip_register_port': 5060,
  'sip_register_transport': 'tcp',
  'enable_echo_cancellation': 'true',
  'enable_video': 'true',
  'enable_rtt': 'true',
  'enable_adaptive_rate': 'true',
  'enabled_codecs': ['H.264','H.263','VP8','G.722','G.711'],
  'bwLimit': 'high-fps',
  'upload_bandwidth': 660,
  'download_bandwidth': 660,
  'enable_stun': 'false',
  'stun_server': '""',
  'enable_ice': 'false',
  'logging': 'info',
  'sip_mwi_uri': '""',
  'sip_videomail_uri': '""',
  'video_resolution_maximum': 'cif'
}";*/
        // returns the default json config values
        public static ACEConfig defaultConfig()
        {
            return JsonDeserializer.JsonDeserialize<ACEConfig>(defaultConfigString);
        }
        public static ACEConfig defaultConfig(ACEConfigStatusType configStatus)
        {
            ACEConfig config = (ACEConfig)JsonDeserializer.JsonDeserialize<ACEConfig>(defaultConfigString);
            config.configStatus = configStatus;
            return config;
        }

        public static string GetConfigFile()
        {
            String ApplicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            ApplicationDataPath = Path.Combine(ApplicationDataPath, "VATRP");
            Directory.CreateDirectory(ApplicationDataPath);
  
            if (!string.IsNullOrEmpty(ApplicationDataPath))
            {
                try
                {
                    if (!Directory.Exists(ApplicationDataPath))
                    {
                        Directory.CreateDirectory(ApplicationDataPath);
                    }
                    if (ApplicationDataPath.LastIndexOf(Path.PathSeparator) != (ApplicationDataPath.Length - 1))
                    {
                        ApplicationDataPath += Path.DirectorySeparatorChar;
                    }
                    ApplicationDataPath = String.Concat(ApplicationDataPath, "config.json");
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            return ApplicationDataPath;
        }

        // cjm-sep17 -- this is the file im giving to John to host on our ACL server
        public static void SaveDefaultConfig(string obj, string filePath)
        {
            using (StreamWriter SW = new StreamWriter(filePath))
            {
                SW.WriteLine(obj);
            }
        }

        // VATRP-1271: Liz E. - condense this down and use the JSON handler that we already have
        public static ACEConfig createConfigFromURL(string url, string userName, string password)
        {
            //ACEConfig defaultval = defaultConfig();
            IFeedbackThread feedbackThread = HockeyClient.Current.CreateFeedbackThread();
            string stacktrace = null;
            try
            {
                ACEConfig aceConfig = JsonWebRequest.MakeJsonWebRequestAuthenticated<ACEConfig>(url, userName, password);
                // aceConfig should never be null at this point - throwing JsonException in a failure event. If it is null, there is a problem - 
                //    but let's log it and handle it
                if (aceConfig == null)
                {
                    aceConfig = JsonWebRequest.MakeHttpJsonWebRequest<ACEConfig>(url); // cjm-sep17
                    if (aceConfig == null)
                    {
                        aceConfig = JsonFactoryConfig.defaultConfig(ACEConfigStatusType.UNKNOWN);
                    }
                }
                else
                {
                    aceConfig.configStatus = ACEConfigStatusType.LOGIN_SUCCEESSFUL;
                }
                aceConfig.NormalizeValues();
                return aceConfig;
            }
            catch (JsonException ex)
            {
                // Once the codes that are sent back from the server are managed we can manage them here. For now, look
                //   for unauthorized in the message string as we know that this is returned currently.
                if ((ex.InnerException != null) && !string.IsNullOrEmpty(ex.InnerException.Message) &&
                    ex.InnerException.Message.ToLower().Contains("unauthorized"))
                {
                    ACEConfig config = JsonFactoryConfig.defaultConfig(ACEConfigStatusType.LOGIN_UNAUTHORIZED);
                    config.NormalizeValues();
                    return config; 
                }
                else 
                {
                    ACEConfig config;
                    switch (ex.jsonExceptionType)
                    {
                        case JsonExceptionType.DESERIALIZATION_FAILED: config = JsonFactoryConfig.defaultConfig(ACEConfigStatusType.UNABLE_TO_PARSE);
                            break;
                        case JsonExceptionType.CONNECTION_FAILED: config = JsonFactoryConfig.defaultConfig(ACEConfigStatusType.CONNECTION_FAILED);
                            break;
                        default: 
                            config = JsonFactoryConfig.defaultConfig(ACEConfigStatusType.UNKNOWN);
                            break;
                    }
                    config.NormalizeValues();
                    return config; 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                stacktrace = ex.StackTrace;
                ACEConfig config = JsonFactoryConfig.defaultConfig(ACEConfigStatusType.UNKNOWN);
                config.NormalizeValues();
                return config;
            }

            if ((feedbackThread != null) && !string.IsNullOrEmpty(stacktrace))
            {
                feedbackThread.PostFeedbackMessageAsync(url + "\n\n" + stacktrace, "noreply@ace.com", "json failed to deseralized", "Ace Logcat");
            }
            // note - this may be an invalid login - need to look for the correct unauthorized response assuming that there is one.
            //  Otherwise the app will use null response for now to assume unauthorized
            ACEConfig defaultConfig = JsonFactoryConfig.defaultConfig(ACEConfigStatusType.SRV_RECORD_NOT_FOUND);
            defaultConfig.NormalizeValues();
            return defaultConfig;
        }

    }
}

