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
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using log4net;
using VATRP.Core.Events;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using VATRP.LinphoneWrapper.Enums;
using VATRP.LinphoneWrapper;
using System.Runtime.InteropServices;
using VATRP.Core.Extensions;
using VATRP.LinphoneWrapper.Structs;

namespace VATRP.Core.Services
{
    public class HistoryService : IHistoryService
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof (HistoryService));

        private readonly ServiceManagerBase manager;
        private bool isLoadingCalls;
        private  List<VATRPCallEvent> _allCallsEvents;
        private bool _isStarted;

        public event EventHandler<VATRPCallEventArgs> OnCallHistoryEvent;

        public HistoryService(ServiceManagerBase manager)
        {
            this.manager = manager;
        }


        #region IVATRPService

        public event EventHandler<EventArgs> ServiceStarted;
        public event EventHandler<EventArgs> ServiceStopped;

        public bool Start()
        {
            if (_isStarted)
                return true;
            if (manager.LinphoneService != null)
                manager.LinphoneService.OnLinphoneCallLogUpdatedEvent += LinphoneCallEventAdded;

            new Thread((ThreadStart)LoadLinphoneCallEvents).Start();
            if (ServiceStarted != null)
                ServiceStarted(this, EventArgs.Empty);

            _isStarted = true;
            return true;
        }

        public bool Stop()
        {
            if (!_isStarted)
                return true;
            
            if (OnCallHistoryEvent != null)
            {
                var eargs = new VATRPCallEventArgs(HistoryEventTypes.Reset);
                OnCallHistoryEvent(null, eargs);
            }

            AllCallsEvents.Clear();
            if (manager.LinphoneService != null)
                manager.LinphoneService.OnLinphoneCallLogUpdatedEvent -= LinphoneCallEventAdded;
            if (ServiceStopped != null)
                ServiceStopped(this, EventArgs.Empty);
            _isStarted = false;
            return true;
        }
        #endregion

        #region IHistoryService

        public List<VATRPCallEvent> AllCallsEvents
        {
            get { return _allCallsEvents ?? (_allCallsEvents = new List<VATRPCallEvent>()); }
        }

        public void LoadLinphoneCallEvents()
        {
            if (manager.LinphoneService.LinphoneCore == IntPtr.Zero)
                return;

           
            AllCallsEvents.Clear();
            isLoadingCalls = true;
            IntPtr callsListPtr = LinphoneAPI.linphone_core_get_call_logs(manager.LinphoneService.LinphoneCore);
            
           

            if (callsListPtr != IntPtr.Zero)
            {
                MSList curStruct;

                do
                {
                    

                    curStruct.next = IntPtr.Zero;
                    curStruct.prev = IntPtr.Zero;
                    curStruct.data = IntPtr.Zero;

                    curStruct = (MSList) Marshal.PtrToStructure(callsListPtr, typeof (MSList));
                    if (curStruct.data != IntPtr.Zero)
                    {
                        var callevent = ParseLinphoneCallLog(curStruct.data);
                        AllCallsEvents.Add(callevent);
                    }
                    callsListPtr = curStruct.next;
                } while (curStruct.next != IntPtr.Zero);

            }
            isLoadingCalls = false;
            if (OnCallHistoryEvent != null)
            {
                var eargs = new VATRPCallEventArgs(HistoryEventTypes.Load);
                OnCallHistoryEvent(null, eargs);
            }
        }

        private VATRPCallEvent ParseLinphoneCallLog(IntPtr callLogPtr)
        {
            
            LinphoneCallDir direction = LinphoneAPI.linphone_call_log_get_dir(callLogPtr);
            IntPtr tmpPtr = LinphoneAPI.linphone_call_log_get_remote_address(callLogPtr);
            if (tmpPtr == IntPtr.Zero)
                return null;

            //  4/11 MITRE-fjr Throws Exception, Added Catch
            try
            {
                tmpPtr = LinphoneAPI.linphone_address_as_string(tmpPtr);

                // 4/11 try ?? LinphoneAddress _linphoneAddress = LinphoneAPI.linphone_address_as_string(tmpPtr);
                if (tmpPtr == IntPtr.Zero)
                    return null;
            }
            catch (Exception ex)
            {
                Console.Write(ex.InnerException);
                return null;
            }

            

            var remoteParty = Marshal.PtrToStringAnsi(tmpPtr);
            LinphoneAPI.ortp_free(tmpPtr);

            string dn = "", un = "", host = "";
            int port = 0;
            VATRPCall.ParseSipAddressEx(remoteParty, out dn, out un, out host,
                out port);
            if (string.IsNullOrEmpty(un))
                return null;

            remoteParty = port==0 ? string.Format("sip:{0}@{1}", un, host) : 
                string.Format("sip:{0}@{1}:{2}", un, host, port);
            var callevent = new VATRPCallEvent("", remoteParty)
            {
                DisplayName = dn,
                Username = un
            };

            tmpPtr = LinphoneAPI.linphone_call_log_get_call_id(callLogPtr);
            if (tmpPtr != IntPtr.Zero)
                callevent.CallGuid = Marshal.PtrToStringAnsi(tmpPtr);
            callevent.StartTime =
                new DateTime(1970, 1, 1).AddSeconds(LinphoneAPI.linphone_call_log_get_start_date(callLogPtr));
            callevent.EndTime =
                callevent.StartTime.AddSeconds(
                    Convert.ToInt32(LinphoneAPI.linphone_call_log_get_duration(callLogPtr)));
            switch (LinphoneAPI.linphone_call_log_get_status(callLogPtr))
            {
                case LinphoneCallStatus.LinphoneCallSuccess:
                {
                    callevent.Status = direction == LinphoneCallDir.LinphoneCallIncoming
                        ? VATRPHistoryEvent.StatusType.Incoming
                        : VATRPHistoryEvent.StatusType.Outgoing;
                }
                    break;
                case LinphoneCallStatus.LinphoneCallAborted:
                    callevent.Status = VATRPHistoryEvent.StatusType.Failed;
                    break;
                case LinphoneCallStatus.LinphoneCallDeclined:
                    callevent.Status = VATRPHistoryEvent.StatusType.Rejected;
                    break;
                case LinphoneCallStatus.LinphoneCallMissed:
                    callevent.Status = VATRPHistoryEvent.StatusType.Missed;
                    break;
            }
            return callevent;
        }

        private void LinphoneCallEventAdded(IntPtr lc,  IntPtr callPtr)
        {
            if (callPtr == IntPtr.Zero || lc == IntPtr.Zero)
                return;

            var callEvent = ParseLinphoneCallLog(callPtr);

            if (callEvent != null)
            {
                if (OnCallHistoryEvent != null)
                {
                    var eargs = new VATRPCallEventArgs(HistoryEventTypes.Add);
                    OnCallHistoryEvent(callEvent, eargs);
                }
            }
        }

        public bool IsLoadingCalls
        {
            get { return isLoadingCalls; }
        }

       
        public void ClearCallsItems()
        {
            if (manager.LinphoneService.LinphoneCore != IntPtr.Zero)
            {
                LinphoneAPI.linphone_core_clear_call_logs(manager.LinphoneService.LinphoneCore);

                if (OnCallHistoryEvent != null)
                {
                    var eargs = new VATRPCallEventArgs(HistoryEventTypes.Reset);
                    OnCallHistoryEvent(null, eargs);
                }
            }
        }

        #endregion
    }
}
