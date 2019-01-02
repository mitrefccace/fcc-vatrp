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
using System.Net.NetworkInformation;
using VATRP.Core.Model;
using System.Threading.Tasks;
using log4net;

namespace com.vtcsecure.ace.windows.Utilities
{
    public enum ACENetworkError
    {
        Unreachable,
        Unknown
    }

    public class NetworkLogger
    {
        #region Members

        private Ping _pingSender;
        private string _domainName;
        private IPAddress _address;
        private PingReply _reply;
        private bool _error;
        private ILog LOG;
        private System.Timers.Timer _pingTimer;
        private bool _asyncComplete;
        #endregion

        public NetworkLogger()
        {
            Initialize();
        }

        public NetworkLogger(string URI)
        {
            Initialize();
            DomainName = URI;
        }

        private void Initialize()
        {
            _pingSender = new Ping();
            LOG = LogManager.GetLogger("PingInfo");
            _asyncComplete = true;
        }

        #region properties 

        public bool Error
        {
            get { return _error; }
            set { _error = value; }
        }

        public bool AsyncCompleted
        {
            get { return _asyncComplete; }
            set { _asyncComplete = value; }
        }

        public string DomainName
        {
            get { return _domainName; }
            set
            {
                _domainName = value;
                if (value != null)
                {
                    _address = Dns.GetHostAddresses(value)[0];
                }      
            }
        }

        public IPAddress IpAddress
        {
            get { return _address; }
            set
            {
                _address = value;
                _domainName = null;
            }
        }
        #endregion

        #region Methods

        // Handles the case where there is no network connection and thus the ping object cannot be instantiated
        public static void HandleError(ACENetworkError error, string address, string ex)
        {
            ILog ErrorLog = LogManager.GetLogger("PingInfo");
            string msg = string.Empty;
            switch (error)
            {
                case ACENetworkError.Unreachable:
                    msg = "<" + address + ">" + " UNREACHABLE - please ensure you are currently connected to the internet";
                    break;
                case ACENetworkError.Unknown:
                    msg = "UNKNOWN NETWORK ERRROR - failed to create the network logger ping object";
                    break;
            }
            ErrorLog.Info(msg);
            ErrorLog.Info(ex);
        }

        // Reset provate data members
        public void Reset()
        {
            IpAddress = null;
            DestroyPingTimer();
        }

        // Destroy Current Timer
        private void DestroyPingTimer()
        {
            if (_pingTimer != null)
            {
                _pingTimer.Stop();
                _pingTimer.Dispose();
                _pingTimer = null;
            }
        }

        // Ping once when provider dropdown changed
        public void PingProviderNowAsync()
        {
            this.PingProviderAsync(null, null);
        }

        // Continously Ping 
        public void PollProviderAsync(int msUpdateTime)
        {
            if (_pingTimer == null)
            {
                _pingTimer = new System.Timers.Timer();
                _pingTimer.Elapsed += new System.Timers.ElapsedEventHandler(PingProviderAsync);
                _pingTimer.Enabled = true;
                _pingTimer.Interval = msUpdateTime;
                _pingTimer.AutoReset = true;
                _pingTimer.Start();
            }
        }

        // Async Ping Method
        public async void PingProviderAsync(object source, System.Timers.ElapsedEventArgs e)
        {
            if (App.CurrentAccount.EnableProviderPingLog && AsyncCompleted && IpAddress != null)
            {
                try
                {
                    AsyncCompleted = false;
                    _reply = await _pingSender.SendPingAsync(IpAddress);
                    LOG.Info(String.Format("Pinging {0} [{1}]", DomainName, IpAddress.ToString()));

                    if (_reply.Status == IPStatus.Success)
                    {
                        LOG.Info(String.Format("   Reply from {0}: bytes={1} RTT={2} TTL={3}",
                                                IpAddress.ToString(), _reply.Buffer.Length, _reply.RoundtripTime, _reply.Options.Ttl));
                        Error = false;
                    }
                    else
                    {
                        LOG.Info(String.Format("   Reply status failed while Pinging Provider: {0}", DomainName));
                        Error = true;
                    }
                }
                catch (Exception ex)
                {
                    LOG.Info(String.Format("   Exception caught while Pinging Provider: {0}", DomainName));
                    LOG.Info(String.Format("   Exception Details: {0}", ex.ToString()));
                    Error = true;
                }
                finally
                {
                    AsyncCompleted = true;
                }    
            }
            else
            {
                Error = false;
            } 
        }

        #endregion
    }
}
 