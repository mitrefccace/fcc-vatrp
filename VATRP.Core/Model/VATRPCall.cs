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
using VATRP.LinphoneWrapper.Enums;

namespace VATRP.Core.Model
{
    public class VATRPCall
    {
        private LinphoneCallDir callDirection = LinphoneCallDir.LinphoneCallIncoming;
        private VATRPCallState callState = VATRPCallState.None;
        private CallParams _from;
        private CallParams _to ;
        private string _displayName;
        private DateTime callStartTime;
        private DateTime callEstablishTime;
        private bool _videoEnabled;
        private VATRPChat _chatRoom;
        private string _linphoneMessage;

        public System.DateTime CallEstablishTime
        {
            get { return callEstablishTime; }
            set { callEstablishTime = value; }
        }

        public System.DateTime CallStartTime
        {
            get { return callStartTime; }
            set { callStartTime = value; }
        }

        public System.IntPtr NativeCallPtr
        {
            get; set;
        }

        public VATRPCall()
        {
            this.NativeCallPtr = IntPtr.Zero;
        }

        public VATRPCall(IntPtr callPtr)
        {
            this.NativeCallPtr = callPtr;
        }

        public VATRPCallState CallState
        {
            get { return callState; }
            set { callState = value; }
        }

        public VATRPChat ChatRoom
        {
            get { return _chatRoom; }
            set { _chatRoom = value; }
        }

        public LinphoneCallDir CallDirection
        {
            get { return callDirection; }
            set { callDirection = value; }
        }

        public CallParams From
        {
            get
            {
                if (_from == null)
                    _from = new CallParams();
                return _from;
            }
            set { _from = value; }
        }

        public CallParams To
        {
            get
            {
                if (_to == null)
                    _to = new CallParams();
                return _to;
            }
            set { _to = value; }
        }

        public CallParams RemoteParty { get; set; }
		
        public string DisplayName
        {
            get
            {
                if (callDirection == LinphoneCallDir.LinphoneCallIncoming)
                    return this.From.DisplayName;
                return this.To.DisplayName;
            }
            set { _displayName = value; }
        }

        public bool VideoEnabled
        {
            get { return _videoEnabled; }
            set { _videoEnabled = value; }
        }

        public string LinphoneMessage
        {
            get { return _linphoneMessage; }
            set { _linphoneMessage = value; }
        }

        public int SipErrorCode { get; set; }
		
        public static bool ParseSipAddress(string sipAddress, out string username, out string hostname, out int port)
        {
            //******************************** SIP address / Dialled number parsing ********************************
            // SIP address is the end user number to call.
            //*******************************************************************************************

            username = string.Empty;
            hostname = string.Empty;
            port = 0;

            if (string.IsNullOrEmpty(sipAddress))
                return false;

            int pos = sipAddress.LastIndexOf("sip:", StringComparison.InvariantCulture);
            username = pos == -1 ? sipAddress : sipAddress.Substring(pos + 4);

            pos = username.LastIndexOf("@", StringComparison.InvariantCulture);
            if (pos != -1)
            {
                hostname = username.Substring(pos + 1);
                username = username.Substring(0, pos);
                var hParts = hostname.Split(new[] {';'}); // trim user=phone
                if (hParts.Length > 0)
                    hostname = hParts[0];
                pos = hostname.LastIndexOf(":", StringComparison.InvariantCulture);
                if (pos != -1)
                {
                    try
                    {
                        port = Convert.ToInt32(hostname.Substring(pos + 1));
                    }
                    catch 
                    {
                    }
                    hostname = hostname.Substring(0, pos);
                }
            }
            return true;
        }

        internal static bool ParseSipAddressEx(string sipAddress, out string displayname, out string username,
            out string hostname, out int port)
        {
            displayname = string.Empty;
            username = string.Empty;
            hostname = string.Empty;
            port = 0;

            bool bRetVal = false;

            int posStart = sipAddress.IndexOf("\"", StringComparison.InvariantCulture);
            if (posStart != -1)
            {
                int posEnd = posStart;
                int posTmp = posEnd;
                while ((posTmp = sipAddress.IndexOf("\"", posTmp + 1, StringComparison.InvariantCulture)) != -1)
                {
                    if (sipAddress[posTmp - 1] != '\\')
                    {
                        posEnd = posTmp;
                        break;
                    }
                }

                if (posStart < posEnd)
                {
                    int posFirst = sipAddress.IndexOf("<", posEnd + 1, StringComparison.InvariantCulture);
                    if (posFirst != -1)
                    {
                        int posLast = sipAddress.IndexOf(">", posFirst + 1, StringComparison.InvariantCulture);
                        if (posFirst + 1 < posLast)
                        {
                            displayname = sipAddress.Substring(posStart + 1, posEnd - posStart - 1);

                            string stripped = sipAddress.Substring(posFirst + 1, posLast - posFirst - 1);
                            bRetVal = VATRPCall.ParseSipAddress(stripped, out username, out hostname, out port);
                        }
                    }
                }
            }
            else
            {
                bRetVal = ParseSipAddress(sipAddress, out username, out hostname, out port);
            }

            return bRetVal;
        }

        public virtual bool Equals(VATRPCall other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }
            if (object.ReferenceEquals(other, this))
            {
                return true;
            }

            return (this.NativeCallPtr == other.NativeCallPtr);
        }

        public override bool Equals(object obj)
        {
            return ((obj is VATRPCall) && this.Equals((VATRPCall) obj));
        }

        public override int GetHashCode()
        {
            return this.NativeCallPtr.GetHashCode();
        }
    }
}
