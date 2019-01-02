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

using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace com.vtcsecure.ace.windows.Utilities
{

    public class SRVLookup
    {
        private static readonly log4net.ILog _log = LogManager.GetLogger(typeof(App));

        public const string NETWORK_SRV_ERROR_CONFIG_SERVICE = "Network SRV error: Config Service";

        public SRVLookup()
        {
        }

        [DllImport("dnsapi", EntryPoint = "DnsQuery_W", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
         private static extern int DnsQuery([MarshalAs(UnmanagedType.VBByRefStr)]ref string pszName, QueryTypes wType, QueryOptions options, int aipServers, ref IntPtr ppQueryResults, int pReserved);

        [DllImport("dnsapi", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void DnsRecordListFree(IntPtr pRecordList, int FreeType);


        public static string[] GetSRVRecords(string needle)
        {

            IntPtr ptr1 = IntPtr.Zero;
            IntPtr ptr2 = IntPtr.Zero;
            SRVRecord recSRV;
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                throw new NotSupportedException();
            }
            ArrayList list1 = new ArrayList();
            try
            {

                int num1 = SRVLookup.DnsQuery(ref needle, QueryTypes.DNS_TYPE_SRV, QueryOptions.DNS_QUERY_BYPASS_CACHE, 0, ref ptr1, 0);
                if (num1 != 0)
                {
                    if (num1 == 9003)
                    {
                        list1.Add(NETWORK_SRV_ERROR_CONFIG_SERVICE);
                    }
                    else
                    {
                        Win32Exception ex = new Win32Exception(num1);
                        _log.Error("SRVLookup.GetSRVRecords: an exception occured during lookup. Details: " + ex.Message);
                        return new string[] { SRVLookup.NETWORK_SRV_ERROR_CONFIG_SERVICE };
                    }
                }
                for (ptr2 = ptr1; !ptr2.Equals(IntPtr.Zero); ptr2 = recSRV.pNext)
                {
                    recSRV = (SRVRecord)Marshal.PtrToStructure(ptr2, typeof(SRVRecord));
                    if (recSRV.wType == (short)QueryTypes.DNS_TYPE_SRV)
                    {
                        string text1 = Marshal.PtrToStringAuto(recSRV.pNameTarget);
                        text1 += ":" + recSRV.wPort;
                        list1.Add(text1);
                    }
                }
            }
            finally
            {
                SRVLookup.DnsRecordListFree(ptr1, 0);
            }
            return (string[])list1.ToArray(typeof(string));
        }

        private enum QueryOptions
        {
            DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE = 1,
            DNS_QUERY_BYPASS_CACHE = 8,
            DNS_QUERY_DONT_RESET_TTL_VALUES = 0x100000,
            DNS_QUERY_NO_HOSTS_FILE = 0x40,
            DNS_QUERY_NO_LOCAL_NAME = 0x20,
            DNS_QUERY_NO_NETBT = 0x80,
            DNS_QUERY_NO_RECURSION = 4,
            DNS_QUERY_NO_WIRE_QUERY = 0x10,
            DNS_QUERY_RESERVED = -16777216,
            DNS_QUERY_RETURN_MESSAGE = 0x200,
            DNS_QUERY_STANDARD = 0,
            DNS_QUERY_TREAT_AS_FQDN = 0x1000,
            DNS_QUERY_USE_TCP_ONLY = 2,
            DNS_QUERY_WIRE_ONLY = 0x100
        }

        private enum QueryTypes
        {
            DNS_TYPE_A = 0x0001,
            DNS_TYPE_MX = 0x000f,
            DNS_TYPE_SRV = 0x0021
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct SRVRecord
        {
            public IntPtr pNext;
            public string pName;
            public short wType;
            public short wDataLength;
            public int flags;
            public int dwTtl;
            public int dwReserved;
            public IntPtr pNameTarget;
            public short wPriority;
            public short wWeight;
            public short wPort;
            public short Pad;
        }
    }
}
