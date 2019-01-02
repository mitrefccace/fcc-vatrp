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

using com.vtcsecure.ace.windows.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VATRP.Core.Model;
using Newtonsoft.Json;
using System.Net;

namespace com.vtcsecure.ace.windows.Utilities
{
    public static class ConfigLookup
    {

        public static ACEConfig LookupConfig(string address, string userName, string password)
        {
            string srvLookupUrl = "_rueconfig._tls.";
            srvLookupUrl += address; // concat with selected domain
            
            // cjm-sep17 -- just to save the example string to file
            //ACEConfig configToSave = JsonFactoryConfig.defaultConfig(ACEConfigStatusType.SRV_RECORD_NOT_FOUND);
            //JsonFactoryConfig.SaveDefaultConfig(JsonConvert.SerializeObject(configToSave, Formatting.Indented), JsonFactoryConfig.GetConfigFile());

            string[] srvRecords = SRVLookup.GetSRVRecords(srvLookupUrl);
            
            if (srvRecords.Length > 0) 
            {
                string record = srvRecords[0];
                if (!record.Equals(SRVLookup.NETWORK_SRV_ERROR_CONFIG_SERVICE))
                {
                    string requestUrl = "https://" + srvRecords[0] + "/config/v1/config.json";
                    ACEConfig config = JsonFactoryConfig.createConfigFromURL(requestUrl, userName, password);
                    return config;
                }
                else
                {
                    // cjm-sep17 -- this at least a test for local hhtp with auth server
                    // this must be running on my local machine 
                    // https://localhost:443/Linphone/config/v1/config.json
                    //IPHostEntry hostInfo = Dns.GetHostEntry("_sip._tcp.rueconfig.ddns.net");
                    //IPAddress localAddress = hostInfo.AddressList[0];
                    //string[] data = SRVLookup.GetSRVRecords("_sip._tcp.rueconfig.ddns.net");
                    string requestUrl = string.Empty;
                    if (address == "sip.linphone.org")
                    {
                        requestUrl = "https://localhost:443/Linphone/config/v1/config.json";
                    }
                    else
                    {
                        requestUrl = "https://localhost:443/ACL/config/v1/config.json";
                    }
                    ACEConfig config = JsonFactoryConfig.createConfigFromURL(requestUrl, userName, password);
                    return config;
                }
            }
            return JsonFactoryConfig.defaultConfig(ACEConfigStatusType.SRV_RECORD_NOT_FOUND);
        }

    }
}
