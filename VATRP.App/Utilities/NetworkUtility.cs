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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace com.vtcsecure.ace.windows.Utilities
{
    public static class NetworkUtility
    {
        public static string INTERNET_UNAVAILABLE = "Please check your network connection and try again.";

        public static bool IsInternetAvailable()
        {
            try
            {
                // Liz E. - ToDo later - we may prefer to add a method that allows us to test if the cdn provider is available, and another to test if the 
                //   domain for the provider is available. Note: Google will work ipv4 & ipv6
                Dns.GetHostEntry("www.google.com"); //using System.Net;
                return true;
            }
            catch (SocketException ex)
            {
                return false;
            }
        }

        public static bool IsCDNAvailable()
        {
            try
            {
                // Liz E. - ToDo later - we may prefer to add a method that allows us to test if the cdn provider is available, and another to test if the 
                //   domain for the provider is available. Note: Google will work ipv4 & ipv6
                Dns.GetHostEntry(ServiceManager.CDN_DOMAIN); //using System.Net;
                return true;
            }
            catch (SocketException ex)
            {
                return false;
            }
        }
    }
}
