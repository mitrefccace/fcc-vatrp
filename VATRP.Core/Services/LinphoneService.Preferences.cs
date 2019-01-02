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

namespace VATRP.Core.Services
{
    public partial class LinphoneService
    {
        public class Preferences
        {
            public Preferences()
            {
                IsOutboundProxyOn = true;
                Expires = 280; // cjm-aug17 : originally 280
                CardDavUser = string.Empty;
                CardDavPass = string.Empty;
                CardDavRealm = string.Empty;
                CardDavDomain = string.Empty;
                CardDavServer = string.Empty;
            }

            public string PhoneNumber { get; set; }
            public string DisplayName { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }

            public string Realm { get; set; }

            public string ProxyHost { get; set; }

            public int ProxyPort { get; set; }

            public string UserAgent { get; set; }
            public string Version { get; set; }

            public string Transport { get; set; }
			
            public string STUNAddress { get; set; }
			
            public int STUNPort { get; set; }
			
            public bool EnableSTUN { get; set; }
			
            public bool EnableAVPF { get; set; }
			
            public LinphoneWrapper.Enums.LinphoneMediaEncryption MediaEncryption { get; set; }
			
            public string AuthID { get; set; }

            public bool IsOutboundProxyOn { get; set; }

            public int Expires { get; set; }

            public string GeolocationURI { get; set; }

            public string CardDavUser { get; set; }
            public string CardDavPass { get; set; }
            public string CardDavRealm { get; set; }
            public string CardDavServer { get; set; }
            public string CardDavDomain { get; set; }
        }
    }
}
