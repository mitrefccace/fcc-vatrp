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

#define USE_LINPHONE_LOGGING

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using log4net;
using VATRP.Core.Enums;
using VATRP.Core.Events;
using VATRP.Core.Extensions;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using VATRP.Core.Model.Commands;
using VATRP.Core.Model.Utils;
using VATRP.LinphoneWrapper;
using VATRP.LinphoneWrapper.Enums;
using VATRP.LinphoneWrapper.Structs;
using System.Text.RegularExpressions;

using System.Data.SQLite;

namespace VATRP.Core.Services
{
    public partial class LinphoneService : ILinphoneService
    {
        #region Members
        private ConfigManager _configManager;
		private static readonly ILog LOG = LogManager.GetLogger(typeof(LinphoneService) );
		private readonly Preferences preferences;
		private readonly ServiceManagerBase manager;
		private IntPtr linphoneCore;
        private IntPtr linphoneFactory; // Add fjr
        private IntPtr linphoneCoreCbs; // Add fjr
		private IntPtr proxy_cfg;
		private IntPtr auth_info;
        private IntPtr carddav_auth = IntPtr.Zero;
		private IntPtr t_configPtr;
		private IntPtr vtablePtr;
		private string identity;
		Thread coreLoop;
		private string server_addr;
        private object _linphoneCoreLock = new object(); // cjm-sep17 -- trying to fix sip auto adjust TLS
        private bool _isStarting;
		private bool _isStarted;
		private bool _isStopping;
		private bool _isStopped;
        private bool _vcardSupported;
        private List<VATRPCodec> _audioCodecs = new List<VATRPCodec>();
        private List<VATRPCodec> _videoCodecs = new List<VATRPCodec>();
		private List<VATRPCall> callsList = new List<VATRPCall>();
        private object callLock = new object();
        private object messagingLock = new object();
		LinphoneCoreVTable vtable;
		LCSipTransports t_config;
        private LinphoneCardDAVStats _cardDavStats;
        private IntPtr _cardDavStatsPtr;
        private IntPtr _cardDAVFriends;
        private bool _cardDavSyncInProgress = false;

        private static ManualResetEvent regulator = new ManualResetEvent(false);
        private static Queue<LinphoneCommand> commandQueue;

		private LinphoneCoreRegistrationStateChangedCb registration_state_changed;
		private LinphoneCoreCallStateChangedCb call_state_changed;
		private LinphoneCoreGlobalStateChangedCb global_state_changed;
		private LinphoneCoreNotifyReceivedCb notify_received;
	    private LinphoneCoreCallStatsUpdatedCb call_stats_updated;
        private LinphoneCoreIsComposingReceivedCb is_composing_received;
        private LinphoneCoreMessageReceivedCb message_received;
        private LinphoneChatMessageCbsMsgStateChangedCb message_status_changed;
        private LinphoneCoreCallLogUpdatedCb call_log_updated;
        private LinphoneCoreInfoReceivedCb info_received;
        private LinphoneLogFuncCB linphone_log_received;
        private LinphoneFriendListContactCreatedCb carddav_new_contact;
        private LinphoneFriendListContactDeletedCb carddav_removed_contact;
        private LinphoneFriendListContactUpdatedCb carddav_updated_contact;
        private LinphoneFriendListSyncStateChangedCb carddav_sync_done;
        private LinphoneCoreNetworkReachableCb network_reachable;
        private LinphoneCoreCbsBuddyInfoUpdatedCb buddy_info_updated;

        private string _chatLogPath;
        private string _callLogPath;
        private string _contactsPath;

        private LinphoneRegistrationState currentRegistrationState;
        private IntPtr _linphoneAudioCodecsList = IntPtr.Zero;
        private IntPtr _linphoneVideoCodecsList = IntPtr.Zero;
        private List<IntPtr> _declinedCallsList = new List<IntPtr>();
        private IntPtr _videoMWiSubscription;
        private bool _enableLogging = false;
        // logging
        string[] placeholders = new string[] { "%d", "%s", "%lu", "%i", "%u", "%x", "%X", "%f", "%llu", "%p",
            "%10I64d", "%-9i", "%-19s", "%-19g", "%-10g", "%-20s" };
        SortedList<int, string> placeHolderItems = new SortedList<int, string>();
        private object logLock = new object();
        #endregion

		#region Delegates

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneCoreCbsBuddyInfoUpdatedCb(IntPtr lc, IntPtr lf);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void LinphoneCoreGlobalStateChangedCb(IntPtr lc, LinphoneGlobalState gstate, string message);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void LinphoneCoreRegistrationStateChangedCb(IntPtr lc, IntPtr cfg, LinphoneRegistrationState cstate, string message);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void LinphoneCoreCallStateChangedCb(IntPtr lc, IntPtr call, LinphoneCallState cstate, string message);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void LinphoneCoreNotifyPresenceReceivedCb(IntPtr lc, IntPtr lf);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void LinphoneCoreNewSubscriptionRequestedCb(IntPtr lc, IntPtr lf, string url);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void LinphoneCoreNotifyReceivedCb(IntPtr lc, IntPtr lev, string notified_event, IntPtr body);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneCoreLogFuncCb(int level, string format, IntPtr args);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneCoreCallStatsUpdatedCb(IntPtr lc, IntPtr call, IntPtr stats);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneCoreIsComposingReceivedCb(IntPtr lc, IntPtr room);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneCoreMessageReceivedCb(IntPtr lc, IntPtr room, IntPtr message);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneChatMessageCbsMsgStateChangedCb(IntPtr msg, LinphoneChatMessageState state);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneCoreCallLogUpdatedCb(IntPtr lc, IntPtr newcl);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneCoreInfoReceivedCb(IntPtr lc, IntPtr call, IntPtr msg);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneLogFuncCB(IntPtr domain, OrtpLogLevel lev, IntPtr fmt, IntPtr args);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneFriendListContactCreatedCb(IntPtr list, IntPtr lf);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneFriendListContactDeletedCb(IntPtr list, IntPtr lf);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneFriendListContactUpdatedCb(IntPtr list, IntPtr new_friend, IntPtr old_friend);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneFriendListSyncStateChangedCb(IntPtr list, LinphoneFriendListSyncStatus status, IntPtr message);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LinphoneCoreNetworkReachableCb(IntPtr lc, bool reachable);

		#endregion

		#region Events
		public delegate void GlobalStateChangedDelegate(LinphoneGlobalState state);
		public event GlobalStateChangedDelegate GlobalStateChangedEvent;

		public delegate void RegistrationStateChangedDelegate(LinphoneRegistrationState state, LinphoneReason reason);
		public event RegistrationStateChangedDelegate RegistrationStateChangedEvent;

		public delegate void CallStateChangedDelegate(VATRPCall call);
		public event CallStateChangedDelegate CallStateChangedEvent;

		public delegate void ErrorDelegate(VATRPCall call, string message);
		public event ErrorDelegate ErrorEvent;

		public delegate void NotifyReceivedDelegate(string notify_event);
		public event NotifyReceivedDelegate NotifyReceivedEvent;

        public delegate void CallStatisticsChangedDelegate(VATRPCall call);
        public event CallStatisticsChangedDelegate CallStatisticsChangedEvent;

        public delegate void IsComposingReceivedDelegate(IntPtr chatPtr, uint rttCode);
        public event IsComposingReceivedDelegate IsComposingReceivedEvent;

        public delegate void OnMessageReceivedDelegate(IntPtr chatPtr, List<IntPtr> callChatPtrList, string remote_party, VATRPChatMessage chatMessage);
        public event OnMessageReceivedDelegate OnChatMessageReceivedEvent;

        public delegate void OnMessageStatusChangedDelegate(IntPtr chatMsgPtr, LinphoneChatMessageState state);
        public event OnMessageStatusChangedDelegate OnChatMessageStatusChangedEvent;

        public delegate void OnCallLogUpdatedDelegate(IntPtr lc, IntPtr callPtr);
        public event OnCallLogUpdatedDelegate OnLinphoneCallLogUpdatedEvent;

        public delegate void MWIReceivedDelegate(MWIEventArgs args);
        public event MWIReceivedDelegate OnMWIReceivedEvent;

        public delegate void InfoReceivedDelegate(InfoEventBaseArgs args);
        public event InfoReceivedDelegate OnCameraMuteEvent;

        public delegate void NetworkReachabilityChanged(bool reachable);
        public event NetworkReachabilityChanged NetworkReachableEvent;

        public delegate void CardDavContactDelegate(CardDavContactEventArgs args);
        public delegate void CardDavSyncDelegate(CardDavSyncEventArgs args);
        public event CardDavContactDelegate CardDAVContactCreated;
        public event CardDavContactDelegate CardDAVContactUpdated;
        public event CardDavContactDelegate CardDAVContactDeleted;
        public event CardDavSyncDelegate CardDAVSyncEvent;

		#endregion

		#region Properties
        public ConfigManager FactoryConfigManager
        {
            get { return _configManager; }
        }

        public bool VCardSupported
        {
            get { return _vcardSupported; }
        }

		public Preferences LinphoneConfig
		{
			get { return preferences; }
		}

		public bool IsStarting
		{
			get
			{
				return _isStarting;
			}
		}

		public bool IsStarted
		{
			get
			{
				return _isStarted;
			}
		}

		public bool IsStopping
		{
			get
			{
				return _isStopping;
			}
		}

		public bool IsStopped
		{
			get
			{
				return _isStopped;
			}
		}

        public string ChatLogPath
        {
            get { return _chatLogPath; }
        }

        public string CallLogPath
        {
            get { return _callLogPath; }
        }

        public IntPtr LinphoneCore
        {
            get { return linphoneCore; }
        }

        public int GetActiveCallsCount
        {
            get
            {
                lock (callLock)
                {
                    return callsList.Count;
                }
            }
        }

        public string ContactsDbPath
        {
            get { return _contactsPath; }
        }

        #endregion

        #region Methods

        public LinphoneService(ServiceManagerBase manager)
		{
            this.currentRegistrationState = LinphoneRegistrationState.LinphoneRegistrationNone;
            // cjm-jul18 -- mwi
            _configManager = new ConfigManager(manager.BuildStoragePath("linphone_default_factory_config.cfg"),
                                               manager.BuildStoragePath("linphone_factory_config.cfg"));
            this.manager = manager;
            commandQueue = new Queue<LinphoneCommand>();
			preferences = new Preferences();
			_isStarting = false;
			_isStarted = false;
		    _vcardSupported = true;
		}

        public bool Start(bool enableLogs)
		{
			if (IsStarting)
				return false;

			if (IsStarted)
				return true;
            linphone_log_received = new LinphoneLogFuncCB(OnLinphoneLog);
		    try
		    {
		        if (enableLogs)
		        {
#if !USE_LINPHONE_LOGGING
		            LinphoneAPI.linphone_core_enable_logs(IntPtr.Zero);
                    LinphoneAPI.linphone_core_set_log_level_mask(OrtpLogLevel.ORTP_DEBUG);
#else
                    LinphoneAPI.linphone_core_enable_logs_with_cb(Marshal.GetFunctionPointerForDelegate(linphone_log_received));
#endif
		        }
		        else
		            LinphoneAPI.linphone_core_disable_logs();
		    }
		    catch (Exception ex)
		    {
		        LOG.Debug(ex.ToString());
		    }

            registration_state_changed = new LinphoneCoreRegistrationStateChangedCb(OnRegistrationChanged);
			call_state_changed = new LinphoneCoreCallStateChangedCb(OnCallStateChanged);
			global_state_changed = new LinphoneCoreGlobalStateChangedCb(OnGlobalStateChanged);
			notify_received = new LinphoneCoreNotifyReceivedCb(OnNotifyEventReceived);
            call_stats_updated = new LinphoneCoreCallStatsUpdatedCb(OnCallStatsUpdated);
		    is_composing_received = new LinphoneCoreIsComposingReceivedCb(OnIsComposingReceived);
            message_received = new LinphoneCoreMessageReceivedCb(OnMessageReceived);
            message_status_changed = new LinphoneChatMessageCbsMsgStateChangedCb(OnMessageStatusChanged);
            call_log_updated = new LinphoneCoreCallLogUpdatedCb(OnCallLogUpdated);
            info_received = new LinphoneCoreInfoReceivedCb(OnInfoEventReceived);
            network_reachable = new LinphoneCoreNetworkReachableCb(OnNetworkReachable);
            // cardDAV stuff
            carddav_new_contact = new LinphoneFriendListContactCreatedCb(OnCardDAVContactCreated);
            carddav_removed_contact = new LinphoneFriendListContactDeletedCb(OnCardDAVContactRemoved);
            carddav_updated_contact = new LinphoneFriendListContactUpdatedCb(OnCardDAVContactUpdated);
            carddav_sync_done = new LinphoneFriendListSyncStateChangedCb(OnCardDAVSyncChanged);
            buddy_info_updated = new LinphoneCoreCbsBuddyInfoUpdatedCb(OnBuddyInfoUpdated);

            vtable = new LinphoneCoreVTable()
            {
                global_state_changed = Marshal.GetFunctionPointerForDelegate(global_state_changed),
                registration_state_changed = Marshal.GetFunctionPointerForDelegate(registration_state_changed),
                call_state_changed = Marshal.GetFunctionPointerForDelegate(call_state_changed),
                notify_presence_received = IntPtr.Zero,
                new_subscription_requested = IntPtr.Zero,
                auth_info_requested = IntPtr.Zero,
                call_log_updated = Marshal.GetFunctionPointerForDelegate(call_log_updated),
                message_received = Marshal.GetFunctionPointerForDelegate(message_received),
                is_composing_received = Marshal.GetFunctionPointerForDelegate(is_composing_received),
                dtmf_received = IntPtr.Zero,
                refer_received = IntPtr.Zero,
                call_encryption_changed = IntPtr.Zero,
                transfer_state_changed = IntPtr.Zero,
                buddy_info_updated = Marshal.GetFunctionPointerForDelegate(buddy_info_updated),
                call_stats_updated = Marshal.GetFunctionPointerForDelegate(call_stats_updated),
                info_received = Marshal.GetFunctionPointerForDelegate(info_received),
                network_reachable = Marshal.GetFunctionPointerForDelegate(network_reachable),
                subscription_state_changed = IntPtr.Zero,
                notify_received = Marshal.GetFunctionPointerForDelegate(notify_received),
                publish_state_changed = IntPtr.Zero,
                configuring_status = IntPtr.Zero,
                display_status = IntPtr.Zero,
                display_message = IntPtr.Zero,
                display_warning = IntPtr.Zero,
                display_url = IntPtr.Zero,
                show = IntPtr.Zero,
                text_received = IntPtr.Zero,
            };

            // Working
            vtablePtr = Marshal.AllocHGlobal(Marshal.SizeOf(vtable));
            Marshal.StructureToPtr(vtable, vtablePtr, false);

            // if the second arg is set to the config path below, then it's possible 
            // that a user can log into the account with an incorrect password since
            // this file stores account information.
		    // string configPath = manager.BuildStoragePath("linphonerc.cfg");
            linphoneCore = LinphoneAPI.linphone_core_new(vtablePtr, null, _configManager.FactoryConfigPath, IntPtr.Zero);
     
            //linphoneCore = LinphoneAPI.linphone_core_new(vtablePtr, null, null, IntPtr.Zero);

            //// -- Start New DLL Integration here --//
            //string zz = VATRP.LinphoneWrapper.LinphoneAPI.linphone_core_get_version_asString();
            //Debug.Print(zz);

            ////  Old code
            ////linphoneCore = LinphoneAPI.linphone_core_new(vtablePtr, configPath, null, IntPtr.Zero);

            //// Added 4/3/2017 MITRE-fjr Test New API here
            //linphoneFactory = LinphoneAPI.linphone_factory_get();

            //// CallBack Object
            //linphoneCoreCbs = LinphoneAPI.linphone_factory_create_core_cbs(linphoneFactory);


            //linphoneCore = LinphoneAPI.linphone_factory_create_core(linphoneFactory, linphoneCoreCbs, configPath, null);
            ////linphoneCore = LinphoneAPI.linphone_factory_create_core(linphoneFactory, IntPtr.Zero, configPath, null);

            //LinphoneAPI.linphone_core_add_listener(linphoneCore, vtablePtr);


            //LinphoneAPI.linphone_core
            //linphoneCore

            //linphoneCore = LinphoneAPI.linphone_core_new(vtablePtr, configPath, null, IntPtr.Zero);

            //linphoneCore = LinphoneAPI.linphone_core_new(vtablePtr, configPath, null, IntPtr.Zero);
            //LINPHONE_DEPRECATED LinphoneCore* linphone_core_new  ( const LinphoneCoreVTable *  vtable,
            //  const char *  config_path,
            //  const char *  factory_config_path,
            //  void *  userdata
            // )


            //LinphoneCore* linphone_factory_create_core  ( const LinphoneFactory *  factory,
            //  LinphoneCoreCbs *  cbs,
            //  const char *  config_path,
            //  const char *  factory_config_path
            // )



            if (linphoneCore != IntPtr.Zero)
			{
                // cjm-sep17 -- set the IP address to avoid 401 registration error?
                //LinphoneAPI.linphone_core_set_nat_address(linphoneCore, "192.160.51.50");
                // this does not seem to be the right method :(

                // Possible that this was crashing the application
			    try
			    {
			        LinphoneAPI.libmsopenh264_init(LinphoneAPI.linphone_core_get_ms_factory(linphoneCore));
			    }
			    catch (Exception ex)
			    {
                    LOG.Debug(ex.ToString());
			    }

			    // Liz E. - this is set in the account settings now
                //LinphoneAPI.linphone_core_set_video_preset(linphoneCore, "high-fps");
				LinphoneAPI.linphone_core_enable_video_capture(linphoneCore, true);
				LinphoneAPI.linphone_core_enable_video_display(linphoneCore, true);
				LinphoneAPI.linphone_core_enable_video_preview(linphoneCore, false);
				LinphoneAPI.linphone_core_set_native_preview_window_id(linphoneCore, -1);

			    LinphoneAPI.linphone_core_set_upload_bandwidth(linphoneCore, 1500);
                LinphoneAPI.linphone_core_set_download_bandwidth(linphoneCore, 1500);

                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
			    string directory = String.Empty;
			    if (!string.IsNullOrEmpty(path))
			    {
			        directory = Path.GetDirectoryName(path);
			    }

			    if (!string.IsNullOrEmpty(directory ))
                {
                    var rootCAPath = Path.Combine(directory, "rootca.pem");
                    LinphoneAPI.linphone_core_set_root_ca(linphoneCore, rootCAPath);

                    var noVideoImagePath = Path.Combine(directory, "images");
                    noVideoImagePath = Path.Combine(noVideoImagePath, "camera_disabled.jpg");
                    LinphoneAPI.ms_static_image_set_default_image(noVideoImagePath);
                }

			    LinphoneAPI.linphone_core_verify_server_cn(linphoneCore, true);
                LinphoneAPI.linphone_core_verify_server_certificates(linphoneCore, true);

			    // load installed codecs
			    LoadAudioCodecs();
                LoadVideoCodecs();

			    IntPtr defProxyCfg = LinphoneAPI.linphone_core_get_default_proxy_config(linphoneCore);
			    if (defProxyCfg != IntPtr.Zero)
			    {
			        proxy_cfg = defProxyCfg;
                    LinphoneAPI.linphone_proxy_config_edit(proxy_cfg);
                    LinphoneAPI.linphone_proxy_config_enable_register(proxy_cfg, false);
                    LinphoneAPI.linphone_proxy_config_done(proxy_cfg);
			    }

                IntPtr coreConfig = LinphoneAPI.linphone_core_get_config(linphoneCore);
                if (coreConfig != IntPtr.Zero)
                {
                    /* cjm-jul18 -- setting the from domain so that mwi udp subscribe does not r/x "endpoint not found"
                     *
                     * I'm not sure that these API calls are working properly at the moment
                     * since the set string method does not fix the SDP contact header
                     * when the core sends and outbound SUBSCIRBE.
                     *
                     * TODO:
                     * LinphoneAPI.linphone_config_set_string(coreConfig, "sip", "contact", "sip:user@sip.linphone.org");
                     */

                    LinphoneAPI.linphone_config_set_int(coreConfig, "sip", "tcp_tls_keepalive", 1);
                    LinphoneAPI.linphone_config_set_int(coreConfig, "sip", "tcp_tls_keepalive", 1);
                    LinphoneAPI.linphone_config_set_int(coreConfig, "sip", "keepalive_period", 90000);
                    LinphoneAPI.linphone_config_set_int(coreConfig, "sip", "auto_net_state_mon", 1); // enable linphone network monitoring

                    // store contacts as vcard
                    LinphoneAPI.linphone_config_set_int(coreConfig, "misc", "store_friends", 1);

                    //********************************************************************************************************************
                    // BY MK ON DATED 1-NOV-2016 FOR SET THE HISTORY LIMIT.
                    LinphoneAPI.linphone_config_set_int(coreConfig, "misc", "history_max_size", 1000);
                    //********************************************************************************************************************

                    // VATRP-2130, prevent SIP spam
                    LinphoneAPI.linphone_config_set_int(coreConfig, "sip", "sip_random_port", 1); // force to set random ports

                    try
                    {
                        LinphoneAPI.linphone_config_sync(coreConfig);
                    }
                    catch (Exception ex)
                    {
                        LOG.Debug($"Error writing linphone config file to disk -- {ex.ToString()}");
                    }
                    
                }

			    LinphoneAPI.linphone_core_enable_keep_alive(linphoneCore, false);

				coreLoop = new Thread(LinphoneMainLoop) {IsBackground = true};
				coreLoop.Start();
			    _isStarting = false;
                _isStopping = false;
                _isStopped = false;
				_isStarted = true;
			}
            if (ServiceStarted != null)
                ServiceStarted(this, EventArgs.Empty);
			return _isStarted;
		}

        void LinphoneMainLoop()
        {
            Console.WriteLine("Main loop started");
            LOG.Debug("Main loop started");
            bool isRunning = true;
            bool dequeCommand;
            //Thread.Sleep(500); // cjm-aug17 helps with logout then back in timing problem

            while (isRunning)
            {
                regulator.WaitOne(20); // fire each 20 msec
                dequeCommand = true;
                try
                {
                    if (commandQueue.Count > 0)
                    {
                        LinphoneCommand command;
                        lock(commandQueue)
                        {
                            command = commandQueue.Peek();
                        }

                        if (command != null)
                        {
                            Console.WriteLine("Main loop started: " + command.Command);
                            switch (command.Command)
                            {
                                case LinphoneCommandType.TerminateCall:
                                    var terminateCmd = command as TerminateCallCommand;

                                    if (terminateCmd != null)
                                        LinphoneAPI.linphone_core_terminate_call(LinphoneCore, terminateCmd.CallPtr);
                                    break;
                                case LinphoneCommandType.TerminateAllCalls:
                                    LinphoneAPI.linphone_core_terminate_all_calls(linphoneCore);
                                    break;
                                case LinphoneCommandType.AcceptCall:
                                    var acceptCmd = command as AcceptCallCommand;
                                    if (acceptCmd != null)
                                        LinphoneAPI.linphone_core_accept_call_with_params(linphoneCore, //  CJM : RTT-LAST
                                            acceptCmd.CallPtr, acceptCmd.CallParamsPtr);
                                    break;
                                case LinphoneCommandType.DeclineCall:
                                    var declineCmd = command as DeclineCallCommand;
                                    if (declineCmd != null)
                                        LinphoneAPI.linphone_core_decline_call(linphoneCore, declineCmd.CallPtr,
                                            declineCmd.Reason);
                                    break;
                                case LinphoneCommandType.CreateCall:
                                {
                                        // cjm-sep17 -- just want to extract the key as an experiment so here it goes...
                                        IntPtr keyPtr = LinphoneAPI.linphone_core_get_tls_cert(linphoneCore);
                                        string key = Marshal.PtrToStringAuto(keyPtr);
                                        Console.WriteLine("============================");
                                        Console.WriteLine(key);
                                        Console.WriteLine("============================");
                                        var createCmd = command as CreateCallCommand;
                                        if (createCmd != null)
                                        {
                                            // enable rtt
                                            LinphoneAPI.linphone_call_params_enable_realtime_text(createCmd.CallParamsPtr,createCmd.EnableRtt); // CJM : RTT** the Enablertt reads _enablertt which is set from the togle button in the UI
                                            MuteCall(createCmd.MuteMicrophone);
                                            MuteSpeaker(createCmd.MuteSpeaker);

                                            IntPtr callPtr = LinphoneAPI.linphone_core_invite_with_params(linphoneCore, // CJM : RTT
                                                createCmd.Callee, createCmd.CallParamsPtr);

                                            if (callPtr == IntPtr.Zero)
                                            {
                                                if (ErrorEvent != null)
                                                    ErrorEvent(null, "Cannot create call to " + createCmd.Callee);
                                            }

                                            if (createCmd.CallParamsPtr != IntPtr.Zero)
                                                LinphoneAPI.linphone_call_params_destroy(createCmd.CallParamsPtr);
                                        }
                                }
                                    break;
                                case LinphoneCommandType.StopLinphone:
                                    isRunning = false;
                                    break;
                                case LinphoneCommandType.PauseCall:
                                    var pauseCmd = command as PauseCallCommand;
                                    if (pauseCmd != null)
                                    {
                                        dequeCommand = LinphoneAPI.linphone_call_media_in_progress(pauseCmd.CallPtr) == 0;
                                        if (dequeCommand)
                                            LinphoneAPI.linphone_core_pause_call(linphoneCore, pauseCmd.CallPtr);
                                    }
                                    break;
                                case LinphoneCommandType.ResumeCall:
                                    var resumeCmd = command as ResumeCallCommand;
                                    if (resumeCmd != null)
                                    {
                                        dequeCommand = LinphoneAPI.linphone_call_media_in_progress(resumeCmd.CallPtr) == 0;
                                        if (dequeCommand)
                                            LinphoneAPI.linphone_core_resume_call(linphoneCore, resumeCmd.CallPtr);
                                    }
                                    break;
                                case LinphoneCommandType.MuteCall:
                                    var muteCmd = command as MuteCallCommand;
                                    if (muteCmd != null)
                                    {
                                        IntPtr callPtr = LinphoneAPI.linphone_core_get_current_call(linphoneCore);
                                        dequeCommand = callPtr == IntPtr.Zero;
                                        if (callPtr != IntPtr.Zero)
                                            dequeCommand = LinphoneAPI.linphone_call_media_in_progress(callPtr) == 0;
                                        if (dequeCommand)
                                            LinphoneAPI.linphone_core_enable_mic(linphoneCore, muteCmd.MuteOn);
                                    }
                                    break;
                                case LinphoneCommandType.SendChatMessage:
                                {
                                    var msgCmd = command as SendChatMessageCommand;
                                    if (msgCmd != null)
                                    {
                                        IntPtr callbacks = LinphoneAPI.linphone_chat_message_get_callbacks(msgCmd.CallPtr);

                                        LinphoneAPI.linphone_chat_message_cbs_set_msg_state_changed(callbacks, Marshal.GetFunctionPointerForDelegate(message_status_changed));
                                        LinphoneAPI.linphone_chat_room_send_chat_message(msgCmd.ChatPtr, msgCmd.CallPtr);
                                        LinphoneAPI.linphone_chat_message_unref(msgCmd.CallPtr);
                                    }
                                    break;
                                }
                                case LinphoneCommandType.ToggleCamera:
                                {
                                    var msgCmd = command as ToggleCameraCommand;
                                    if (msgCmd != null)
                                    {
                                        IntPtr callPtr = LinphoneAPI.linphone_core_get_current_call(linphoneCore);

                                        if (msgCmd.CallPtr == callPtr)
                                        {
                                            LinphoneAPI.linphone_call_enable_camera(callPtr, msgCmd.EnableCamera);
                                        }
                                    }
                                    break;
                                }
                                default:
                                {
                                    break;
                                }
                            }
                        }

                        if (dequeCommand)
                        {
                            lock (commandQueue)
                            {
                                commandQueue.Dequeue();
                            }
                        }
                    }
                    // CJM : Login, this method needs to set the netowrk connectivity status within the linphone core library so it can be flagged UP on registration attempt
                    //lock (_linphoneCoreLock)
                    // cjm-sep17 -- this actually makes the registration calls
                    LinphoneAPI.linphone_core_iterate(linphoneCore); // roll
                }
                catch (Exception ex)
                {
                    LOG.Debug("************ Linphone Main loop exception: " + ex.Message);
                }
            }

            // cjm-sep17 -- all of this runs when the core is stopped
            LinphoneAPI.linphone_core_iterate(linphoneCore); // roll

            if (vtablePtr != IntPtr.Zero)
                Marshal.FreeHGlobal(vtablePtr);
            if (t_configPtr != IntPtr.Zero)
                Marshal.FreeHGlobal(t_configPtr);

            if (_cardDavStatsPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_cardDavStatsPtr);
               _cardDavStatsPtr = IntPtr.Zero;
            }

            //Marshal.FreeHGlobal(linphoneCore);

            try
            {
                //  Added fjr
                LinphoneAPI.linphone_core_remove_listener(linphoneCore, vtablePtr);

                //  Uncommented
                //LinphoneAPI.linphone_core_destroy(linphoneCore);

                //  Added MITRE-fjr
                //LinphoneAPI.linphone_core_unref(linphoneCore);
            }
            catch (Exception ex)
            {
                Debug.Print("Error Dereferencing");
            }


            registration_state_changed = null;
            call_state_changed = null;
            global_state_changed = null;    // Added MITRE-fjr
            notify_received = null;
            call_stats_updated = null;      //Added MITRE-fjr
            is_composing_received = null;   //  Added MITRE-fjr
            message_received = null;
            message_status_changed = null;
            call_log_updated = null;
            info_received = null;
            network_reachable = null;
            carddav_new_contact = null;
            carddav_removed_contact = null;
            carddav_updated_contact = null;
            carddav_sync_done = null;
            buddy_info_updated = null;      //  Added MITRE-fjr

            carddav_auth = IntPtr.Zero;
            linphoneCore = proxy_cfg = auth_info = t_configPtr = IntPtr.Zero;
            call_stats_updated = null;
            coreLoop = null;
            identity = null;
            server_addr = null;
            _isStarting = false;
            _isStarted = false;
            _isStopping = false;
            _isStopped = true;
            LOG.Debug("Main loop exited");
            if (ServiceStopped != null)
                ServiceStopped(this, EventArgs.Empty);
        }

        /// <summary>
        /// Method to update the [sip] contact parameter within the linphone config
        /// </summary>
        /// <param name="section">string</param>
        /// <param name="key">string</param>
        /// <param name="value">string</param>
        /// <returns>void</returns>
        public void updateLinphoneConfig(string section, string key, string value)
        {
            if (linphoneCore == IntPtr.Zero)
            {
                return;
            }

            IntPtr coreConfig = LinphoneAPI.linphone_core_get_config(linphoneCore);

            if (coreConfig != IntPtr.Zero)
            {
                try
                {
                    LinphoneAPI.linphone_config_set_string(coreConfig, section, key, value);
                    LinphoneAPI.linphone_config_sync(coreConfig);
                }
                catch (Exception ex)
                {
                    LOG.Debug($"Could not write linphone configuration file to disk -- {ex.ToString()}");
                }
                
            }
        }

        public void UpdatePrivateDataPath()
        {
            _chatLogPath = manager.BuildDataPath("chathistory.db");
            _callLogPath = manager.BuildDataPath("callhistory.db");
            _contactsPath = manager.BuildDataPath("contacts.db");
            //_contactsPath = manager.BuildDataPath("contacts.sqlite");


            if (linphoneCore == IntPtr.Zero)
                return;
            LinphoneAPI.linphone_core_set_chat_database_path(linphoneCore, _chatLogPath);
            LinphoneAPI.linphone_core_set_call_logs_database_path(linphoneCore, _callLogPath);

            try
            {
                LinphoneAPI.linphone_core_set_friends_database_path(linphoneCore, _contactsPath);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
           // AddDBPassword();

            //try
            //{
            //    //_contactsPath = manager.BuildDataPath("contacts.sqlite");
            //    var connectionString = string.Format("data source={0}", _contactsPath + ";Version=3;Password=1234;");
            //    SQLiteConnection conn = new SQLiteConnection(connectionString);
            //    conn.Open();
            //    //conn.ChangePassword("1234");
            //    conn.ChangePassword("");
            //    //// conn.ChangePassword("1234");

            //    conn.Close();
            //}
            //catch (Exception)
            //{


            //}



            //CreateSQLiteConnectionString(_contactsPath, "password");
            //CreateSQLiteConnectionString("C:\\Users\\User12\\AppData\\Roaming\\VATRP\\8445688680@24.73.117.26\\contacts123.sqlite", "mypassword");
            //LinphoneAPI.linphone_core_set_friends_database_path(linphoneCore, CreateSQLiteConnectionString("C:\\Users\\User12\\AppData\\Roaming\\VATRP\\8445688680@24.73.117.26\\contacts.sqlite", "mypassword"));
            // //dbConnection = new SQLiteConnection(connectionString)





        }


        private static string CreateSQLiteConnectionString(string sqlitePath, string password)
        {
            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = sqlitePath;
            if (password != null)
                builder.Password = password;
            builder.PageSize = 4096;
            builder.UseUTF16Encoding = true;
            string connstring = builder.ConnectionString;
            return connstring;
        }


        void SetTimeout(int miliseconds)
        {
            var timeout = new System.Timers.Timer {Interval = miliseconds, AutoReset = false};
            timeout.Elapsed += (sender, e) => DoUnregister();
            timeout.Start();
        }

        void SetTimeout(Action callback, int miliseconds)
        {
            var timeout = new System.Timers.Timer { Interval = miliseconds, AutoReset = false };
            timeout.Elapsed += (sender, e) => callback();
            timeout.Start();
        }

	    public void LockCalls()
	    {

            //**********************************************************************************************
            // When call is connected, lock the call.
            //********************************************************************************************
            Monitor.Enter(callLock);
	    }
        public void UnlockCalls()
        {
            Monitor.Exit(callLock);
        }

        public VATRPCall FindCall(IntPtr callPtr)
        {

            //**********************************************************************************************
            // Find a Call, if user is receiving a call.
            //********************************************************************************************
            foreach (var call in callsList)
            {
                if (call.NativeCallPtr == callPtr)
                    return call;
            }
            return null;
        }

        public bool CanMakeVideoCall()
        {

            //********************************************************************************************
            // This method check, user is able to make the call or does not able to make call.
            //********************************************************************************************
            if (linphoneCore == IntPtr.Zero)
            {
                if (ErrorEvent != null)
                    ErrorEvent(null, "Cannot make when Linphone Core is not working.");
                return false;
            }

            return true;
        }

        public void SendDtmfAsSipInfo(bool use_info)
        {
            if (linphoneCore == IntPtr.Zero)
            {
                if (ErrorEvent != null)
                    ErrorEvent(null, "Cannot make when Linphone Core is not working.");
                return;
            }

            var l_use_info = use_info ? 0 : 1;
            if (LinphoneAPI.linphone_core_get_use_info_for_dtmf(linphoneCore) != l_use_info)
            {
                LinphoneAPI.linphone_core_set_use_info_for_dtmf(linphoneCore, use_info);
                LOG.Info(string.Format("{0} send dtmf as SIP info", use_info ? "Enable" : "Disable"));
            }
        }

        public void SendDtmfAsTelephoneEvent(bool use_te)
        {
            if (linphoneCore == IntPtr.Zero)
            {
                if (ErrorEvent != null)
                    ErrorEvent(null, "Cannot make when Linphone Core is not working.");
                return;
            }

            var l_use_te = use_te ? 1 : 0;
            if (LinphoneAPI.linphone_core_get_use_rfc2833_for_dtmf(linphoneCore) != l_use_te)
            {
                LinphoneAPI.linphone_core_set_use_rfc2833_for_dtmf(linphoneCore, use_te);
                LOG.Info(string.Format("{0} send dtmf as RFC 2833", use_te ? "Enable" : "Disable"));
            }
        }

        public void PlayDtmf(char dtmf, int duration)
        {
            //*********************************************************************************************************************
            //Play sound when key pressed from Dialpad.
            //*********************************************************************************************************************
            if (linphoneCore == IntPtr.Zero)
                return;
            LinphoneAPI.linphone_core_play_dtmf(linphoneCore, dtmf, duration);
        }

        #endregion

        #region Registration
        public bool Register()
		{
            //****************************************************************************************
            // Registering in the application.
            //****************************************************************************************
            // cjm-sep17 -- critical section from the main loop of linphoneService which calls the iterate method
            if (linphoneCore == IntPtr.Zero)
            {
                return false;
            }

            if (currentRegistrationState == LinphoneRegistrationState.LinphoneRegistrationProgress) // CJM : Login
            {
                return false;
            }

            // cjm-aug17 -- pause to check for network connectivity -- required for our servers but not linphone
            int numAttempts = 0;
            byte isReachable = 0;
            while (numAttempts < 10 && isReachable == 0)
            {
                isReachable = LinphoneAPI.linphone_core_is_network_reachable(linphoneCore);
                numAttempts++;
                Thread.Sleep(50);
            }

            // Failed to reach the network so fail the registration process
            if (isReachable == 0)
            {
                SetTimeout(delegate
                {
                    if (RegistrationStateChangedEvent != null)
                        RegistrationStateChangedEvent(LinphoneRegistrationState.LinphoneRegistrationFailed, LinphoneReason.LinphoneReasonUnknown);
                }, 50);
                return false;
            }

            if (t_configPtr == IntPtr.Zero)
            {
                t_config = new LCSipTransports()
                {
                    udp_port = LinphoneAPI.LC_SIP_TRANSPORT_RANDOM,
                    tcp_port = LinphoneAPI.LC_SIP_TRANSPORT_RANDOM,
                    dtls_port = LinphoneAPI.LC_SIP_TRANSPORT_RANDOM,
                    tls_port = LinphoneAPI.LC_SIP_TRANSPORT_RANDOM,
                };

                t_configPtr = Marshal.AllocHGlobal(Marshal.SizeOf(t_config));
                Marshal.StructureToPtr(t_config, t_configPtr, false);
            }

            // Phone number will take higher priority over username 
            if (string.IsNullOrEmpty(preferences.DisplayName))
            {
                identity = string.Format("sip:{0}@{1}", string.IsNullOrEmpty(preferences.PhoneNumber) ? preferences.Username : preferences.PhoneNumber, preferences.ProxyHost);
            }
            else
            {
                identity = string.Format("\"{0}\" <sip:{1}@{2}>", preferences.DisplayName, string.IsNullOrEmpty(preferences.PhoneNumber) ? preferences.Username : preferences.PhoneNumber, preferences.ProxyHost);
            }

            LinphoneAPI.linphone_core_set_sip_transports(linphoneCore, t_configPtr);
            LinphoneAPI.linphone_core_set_user_agent(linphoneCore, preferences.UserAgent, preferences.Version);

            server_addr = string.Format("sip:{0}:{1};transport={2}", preferences.ProxyHost, preferences.ProxyPort, preferences.Transport.ToLower());

            LOG.Info(string.Format("Registering SIP account: {0} Server: {1}", identity, server_addr));

            if (auth_info != IntPtr.Zero)
            {
                LinphoneAPI.linphone_core_remove_auth_info(linphoneCore, auth_info);
                auth_info = IntPtr.Zero;
            }

            auth_info = LinphoneAPI.linphone_auth_info_new(preferences.Username, string.IsNullOrEmpty(preferences.AuthID) ? null : preferences.AuthID, preferences.Password, null, null, null);
            if (auth_info == IntPtr.Zero) { LOG.Debug("failed to get auth info"); }
            else
            {
                string certPath = manager.BuildStoragePath("tlsCert.pem");
                if (File.Exists(certPath))
                {
                    LinphoneAPI.linphone_auth_info_set_tls_cert_path(auth_info, certPath);
                }
                else
                {
                    LOG.Debug("Notify - File not found. Client TLS certificate for SIP not present in VATRP directory.");
                }
            }


            LinphoneAPI.linphone_core_add_auth_info(linphoneCore, auth_info);
            LinphoneAPI.linphone_core_set_primary_contact(linphoneCore, identity);

            // remove all proxy entries from linphone configuration file
		    if (proxy_cfg == IntPtr.Zero)
		    {
		        proxy_cfg = LinphoneAPI.linphone_core_create_proxy_config(linphoneCore);
		    }

            /*set localParty with user name and domain*/
            LinphoneAPI.linphone_proxy_config_edit(proxy_cfg); 

            if (!string.IsNullOrEmpty(preferences.GeolocationURI))
		    {
		        LinphoneAPI.linphone_proxy_config_set_custom_header(proxy_cfg, "Geolocation", $"<{preferences.GeolocationURI}> ;purpose=geolocation");
            }
      
            LinphoneAPI.linphone_proxy_config_set_identity(proxy_cfg, identity);
            LinphoneAPI.linphone_proxy_config_set_server_addr(proxy_cfg, server_addr);
            LinphoneAPI.linphone_proxy_config_set_avpf_mode(proxy_cfg, (LinphoneAVPFMode)LinphoneAPI.linphone_core_get_avpf_mode(LinphoneCore));
            LinphoneAPI.linphone_proxy_config_set_avpf_rr_interval(proxy_cfg, 3);

            string route = preferences.IsOutboundProxyOn ? server_addr : string.Empty;
            // use proxy as route if outbound_proxy is enabled
            LinphoneAPI.linphone_proxy_config_set_route(proxy_cfg, route);
            LinphoneAPI.linphone_proxy_config_set_expires(proxy_cfg, preferences.Expires);
            LinphoneAPI.linphone_proxy_config_enable_register(proxy_cfg, true);
            LinphoneAPI.linphone_core_add_proxy_config(linphoneCore, proxy_cfg);
            LinphoneAPI.linphone_core_set_default_proxy_config(linphoneCore, proxy_cfg);
            LinphoneAPI.linphone_proxy_config_done(proxy_cfg); 

            UpdateMediaEncryption();

            return true;
		}

		public bool Unregister(bool deferred)
		{
            // cjm-aug17
            if (proxy_cfg != IntPtr.Zero)
            {
                int isproxyRegistered = LinphoneAPI.linphone_proxy_config_is_registered(proxy_cfg); // cjm-aug17
                if (isproxyRegistered == 0)
                {
                    // remove all authorization information
                    LinphoneAPI.linphone_core_clear_all_auth_info(linphoneCore);
                    return false;
                }
            }
            else
            {
                return false;
            }
            if (deferred)
			{
                SetTimeout(3000);
            }
            else
            {
                DoUnregister();
            }

			return true;
		}

	    private void DoUnregister()
	    {
            if (linphoneCore == IntPtr.Zero)
                return;

            IntPtr proxyCfg = LinphoneAPI.linphone_core_get_default_proxy_config(LinphoneCore);
            if (proxyCfg != IntPtr.Zero && LinphoneAPI.linphone_proxy_config_is_registered(proxyCfg) == 1)
            {
                if (RegistrationStateChangedEvent != null)
                    RegistrationStateChangedEvent(LinphoneRegistrationState.LinphoneRegistrationProgress, LinphoneReason.LinphoneReasonNone); // disconnecting

                try
                {
                    LinphoneAPI.linphone_proxy_config_edit(proxyCfg);
                    LinphoneAPI.linphone_proxy_config_enable_register(proxyCfg, false);
                    LinphoneAPI.linphone_proxy_config_done(proxyCfg);
                    if (RegistrationStateChangedEvent != null)
                        RegistrationStateChangedEvent(LinphoneRegistrationState.LinphoneRegistrationCleared, LinphoneReason.LinphoneReasonNone);
                }
                catch (Exception ex)
                {
                    LOG.Error("DoUnregister: " + ex.Message);
                }
                if (t_configPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(t_configPtr);
                    t_configPtr = IntPtr.Zero;
                }
            }
			ClearProxyInformation();
	    }

        // cjm-sep17 -- trying to handle changed SIP encryption
        public void UnregisterForTLS()
        {
            LinphoneAPI.linphone_proxy_config_edit(proxy_cfg);
            LinphoneAPI.linphone_proxy_config_enable_register(proxy_cfg, false);
            LinphoneAPI.linphone_proxy_config_done(proxy_cfg);
            LinphoneAPI.linphone_core_iterate(linphoneCore);

            int port = preferences.ProxyPort;
            server_addr = string.Format("sip:{0}:{1};transport={2}", preferences.ProxyHost,
                port, preferences.Transport.ToLower());

            string route = preferences.IsOutboundProxyOn ? server_addr : string.Empty;
            LinphoneAPI.linphone_proxy_config_edit(proxy_cfg);
            LinphoneAPI.linphone_proxy_config_set_server_addr(proxy_cfg, server_addr);
            LinphoneAPI.linphone_proxy_config_set_route(proxy_cfg, route);
            LinphoneAPI.linphone_core_add_proxy_config(linphoneCore, proxy_cfg);
            LinphoneAPI.linphone_core_set_default_proxy_config(linphoneCore, proxy_cfg);
            LinphoneAPI.linphone_proxy_config_done(proxy_cfg);
        }

        public void ClearProxyInformation()
        {
            if (linphoneCore == IntPtr.Zero)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                return;
            }

            // remove all proxy entries from linphone configuration file
            LinphoneAPI.linphone_core_clear_proxy_config(linphoneCore);
            // remove all authorization information
            LinphoneAPI.linphone_core_clear_all_auth_info(linphoneCore);
            // cjm-oct17 -- implement the changes
            LinphoneAPI.linphone_core_iterate(linphoneCore);
        }
        public void ClearAccountInformation()
        {
            ClearProxyInformation();
            // clear pushnotification preference
            // clear ice_preference
            // clear stun_preference

            LinphoneAPI.linphone_core_set_stun_server(linphoneCore, null);
            LinphoneAPI.linphone_core_set_firewall_policy(linphoneCore, LinphoneFirewallPolicy.LinphonePolicyNoFirewall);

            if (RegistrationStateChangedEvent != null)
                RegistrationStateChangedEvent(LinphoneRegistrationState.LinphoneRegistrationCleared, LinphoneReason.LinphoneReasonNone);

        }
        #endregion

        #region Call

        /**
         * @brief When user select a contact to make a call or dials a number
         * this method will called.
         *
         * @return Void.
         */
        public void MakeCall(string destination, bool videoOn, bool rttEnabled, bool muteMicrophone, bool muteSpeaker, bool enableVideo, string geolocationURI, LinphonePrivacy privacyMask)
		{
            if (callsList.Count > 0)
		    {
                LOG.Warn("Cannot make call. Cause - There is active call");
		        return;
		    }

		    if (string.IsNullOrEmpty(destination))
		    {
                LOG.Warn("Cannot make call. Cause - Destination is empty");
                return;
		    }

			if (linphoneCore == IntPtr.Zero) {
				if (ErrorEvent != null)
					ErrorEvent (null, "Cannot make when Linphone Core is not working.");
				return;
			}

            IntPtr callParams = LinphoneAPI.linphone_core_create_call_params(linphoneCore, IntPtr.Zero); // just reads a config file for RTT at first
            LinphoneAPI.linphone_call_params_set_video_direction(callParams, LinphoneMediaDirection.LinphoneMediaDirectionSendRecv);
            LinphoneAPI.linphone_call_params_enable_video(callParams, enableVideo);
		    LinphoneAPI.linphone_call_params_set_audio_bandwidth_limit(callParams, 0);
            LinphoneAPI.linphone_call_params_enable_early_media_sending(callParams, true);
            LinphoneAPI.linphone_call_params_set_privacy(callParams, (int) privacyMask);

            FactoryConfigManager.LoadAppConfiguration();
            string un, host;
            int port;
            VATRPCall.ParseSipAddress(destination, out un, out host, out port);

            if (FactoryConfigManager.EmergencyNumbers.Contains(un))
            {
                LinphoneAPI.linphone_call_params_add_custom_header(callParams, "INVITE", "urn:service:sos;user=dialstring SIP/2.0");
                LinphoneAPI.linphone_call_params_add_custom_header(callParams, "To", "<urn:service:sos;user=dialstring>");
                if (geolocationURI.NotBlank())
                {
                    LinphoneAPI.linphone_call_params_add_custom_header(callParams, "Geolocation", $"<{geolocationURI}> ;purpose=geolocation");
                }
                else
                {
                    LinphoneAPI.linphone_call_params_add_custom_header(callParams, "Geolocation", "<https://example.invalid> ;purpose=rue-owner");
                }
            }

            // add rue owner call info
		    if (!string.IsNullOrEmpty(geolocationURI))
		    {
		        string callInfoUri = geolocationURI.Replace("geolocation", "mitre_contact");
		        LinphoneAPI.linphone_call_params_add_custom_header(callParams, "Call-Info", $"<{callInfoUri}> ;purpose=rue-owner");
		    }

		    var cmd = new CreateCallCommand(callParams, destination, rttEnabled, muteMicrophone, muteSpeaker);

		    lock (commandQueue)
		    {
		        commandQueue.Enqueue(cmd);
		    }
		}

		public void AcceptCall(IntPtr callPtr, bool rttEnabled, bool muteMicrophone, bool muteSpeaker, bool enableVideo)
		{

            //**********************************************************************************************
            // Accepting a call
            //********************************************************************************************
            if (linphoneCore == IntPtr.Zero)
            {
                if (ErrorEvent != null)
                    ErrorEvent(null, "Cannot receive call when Linphone Core is not working.");
                return;
            }

		    lock (callLock)
		    {
		        VATRPCall call = FindCall(callPtr);

		        if (call == null)
		        {
		            LOG.Warn("Cannot accept call. Cause - Null call");
		            return;
		        }

                IntPtr callParamsPtr = LinphoneAPI.linphone_core_create_call_params(linphoneCore, callPtr);

		        IntPtr callerParams = LinphoneAPI.linphone_call_get_remote_params(call.NativeCallPtr);

		        if (callerParams != IntPtr.Zero)
		        {
		            bool remoteRttEnabled = LinphoneAPI.linphone_call_params_realtime_text_enabled(callerParams)!=0  &&
		                                    rttEnabled;

		            LinphoneAPI.linphone_call_params_enable_realtime_text(callParamsPtr, remoteRttEnabled);
                    LinphoneAPI.linphone_call_params_enable_video(callParamsPtr, enableVideo);
		        }
                MuteCall(muteMicrophone);
                MuteSpeaker(muteSpeaker);

		        var cmd = new AcceptCallCommand(call.NativeCallPtr, callParamsPtr);
		        //	LinphoneAPI.linphone_call_params_set_record_file(callsDefaultParams, null);
		        lock (commandQueue)
		        {
		            commandQueue.Enqueue(cmd);
		        }
		    }
		}

        public void DeclineCall(IntPtr callPtr)
        {
            //****************************************************************************************
            // Decline a incoming call.
            //*****************************************************************************************
            if (linphoneCore == IntPtr.Zero)
            {
                if (ErrorEvent != null)
                    ErrorEvent(null, "Cannot terminate calls when Linphone Core is not working.");
                return;
            }

            lock (callLock)
            {
                VATRPCall call = FindCall(callPtr);

                if (call == null)
                {
                    //****************************************************************************************
                    // When call automatically closed/cancelled by Other user.
                    //*****************************************************************************************
                    LOG.Warn("Cannot decline call. Cause - Null call");
                    return;
                }
                call.CallState = VATRPCallState.Closed;

                LOG.Info("Decline Call: " + callPtr);
                LOG.Info(string.Format("Call removed from list. Call - {0}. Total calls in list: {1}", callPtr,
                    callsList.Count));
            }
            var cmd = new DeclineCallCommand(callPtr, LinphoneReason.LinphoneReasonDeclined);

            lock (commandQueue)
            {
                commandQueue.Enqueue(cmd);
            }

        }

        public bool TerminateCall(IntPtr callPtr, string message)
        {

            //***********************************************************************************************************************************************
            // Terminating a call, this method will called when user select End Call button or Cancel when call is ringing on other users screen.
            //************************************************************************************************************************************************
            if (linphoneCore == IntPtr.Zero)
            {
                if (ErrorEvent != null)
                    ErrorEvent(null, "Cannot terminate calls when Linphone Core is not working.");
                return false;
            }

            lock (callLock)
            {
                VATRPCall call = FindCall(callPtr);

                if (call == null)
                {
                    LOG.Warn("TerminateCall No such call. " + callPtr);
                    return false;
                }

                // notify call state end
                //if (LinphoneAPI.linphone_call_params_get_record_file(callsDefaultParams) != IntPtr.Zero)
                //    LinphoneAPI.linphone_call_stop_recording(call.NativeCallPtr);

                LOG.Info("Terminate Call " + callPtr);
                Debug.WriteLine("Terminate Call " + callPtr);
                call.LinphoneMessage = message;
                call.SipErrorCode = 0;
                call.CallState = VATRPCallState.Closed;
                if (CallStateChangedEvent != null)
                    CallStateChangedEvent(call);
                callsList.Remove(call);
                LOG.Info(string.Format("Terminate Call removed from list. Call - {0}. Total calls in list: {1}", callPtr,
    callsList.Count));

            }

            var cmd = new TerminateCallCommand(callPtr);

            lock (commandQueue)
            {
                commandQueue.Enqueue(cmd);
            }
            return true;
        }

        public void ResumeCall(IntPtr callPtr)
        {
            //************************************************************************************************************************
            // Resume a call when 2 calls are running and one call is disconnected or After a hold
            //************************************************************************************************************************
            if (linphoneCore == IntPtr.Zero)
            {
                if (ErrorEvent != null)
                    ErrorEvent(null, "Cannot resume calls when Linphone Core is not working.");
                return;
            }

            var cmd = new ResumeCallCommand(callPtr);

            lock (commandQueue)
            {
                commandQueue.Enqueue(cmd);
            }
        }

        public void PauseCall(IntPtr callPtr)
        {
            //***************************************************************************************************************
            // When user put a call on hold.
            //************************************************************************************************************************
            if (linphoneCore == IntPtr.Zero)
            {
                if (ErrorEvent != null)
                    ErrorEvent(null, "Cannot pause calls when Linphone Core is not working.");
                return;
            }

            var cmd = new PauseCallCommand(callPtr);

            lock (commandQueue)
            {
                commandQueue.Enqueue(cmd);
            }
        }
        public bool IsCallMuted()
		{

            //**********************************************************************************************
            // Check is call is muted?
            //********************************************************************************************
            if (linphoneCore == IntPtr.Zero)
                return false;
            return LinphoneAPI.linphone_core_mic_enabled(linphoneCore) == 0;
		}
        public void MuteCall(bool muteCall)
        {
            IntPtr activeCallPtr = IntPtr.Zero;
            lock (callLock)
            {
                activeCallPtr = LinphoneAPI.linphone_core_get_current_call(linphoneCore);
                //if (activeCallPtr != IntPtr.Zero)
                //{

                //    VATRPCall call = FindCall(activeCallPtr);
                //    if (call != null
                //        /*&& (call.CallState != VATRPCallState.LocalPaused || call.CallState != VATRPCallState.LocalPausing)*/)
                //    {
                //        // probably this should be done in linphone core
                //            LinphoneAPI.linphone_core_enable_mic(linphoneCore, !muteCall);
                //    }
                //}
            }

            var cmd = new MuteCallCommand(activeCallPtr, !muteCall);
            lock (commandQueue)
            {
                commandQueue.Enqueue(cmd);
            }
        }

		public void ToggleMute()
		{
            if (linphoneCore == IntPtr.Zero)
                return;

            LinphoneAPI.linphone_core_enable_mic(linphoneCore, LinphoneAPI.linphone_core_mic_enabled(linphoneCore) == 0);
		}

        public bool IsSpeakerMuted()
        {
            if (linphoneCore == IntPtr.Zero)
                return false;
            float gainLevel = LinphoneAPI.linphone_core_get_playback_gain_db(linphoneCore);
            if (gainLevel < 0)
            {
                return true;
            }
            return false;
        }
        public void MuteSpeaker(bool muteSpeaker)
        {
            float gainLevel = 0f;
            if (muteSpeaker)
            {
                gainLevel = -1000.0f;
            }
            LinphoneAPI.linphone_core_set_playback_gain_db(linphoneCore, gainLevel);
        }

        public void ToggleVideo(bool enableVideo, IntPtr callPtr)
        {
            //**************************************************************************************
            // Toggle Video, Enable/Disable the video
            //******************************************************************************************
            if (linphoneCore == IntPtr.Zero)
                return;

            if (callPtr == IntPtr.Zero)
            {
                LOG.Error("ToggleVideo: Attempting to switch camera but the call pointer is null. Returing without modifying call.");
                return;
            }

            var cmd = new ToggleCameraCommand(callPtr, enableVideo);
            lock (commandQueue)
            {
                commandQueue.Enqueue(cmd);
            }
        }

        public void SendCameraSwtichAsInfo(IntPtr callPtr, bool muteCamera)
        {
//            if (linphoneCore == IntPtr.Zero)
//                return;

//            if (callPtr == IntPtr.Zero)
//            {
//                LOG.Error("LinphoneService.SendCameraSwtichAsInfo: Attempting to switch camera but the call pointer is null. Returing without modifying call.");
//                return;
//            }

//            LOG.Info("Send Mute Camera info. Call- " + callPtr + " Mute: " + muteCamera);
//            IntPtr im = LinphoneAPI.linphone_core_create_info_message(linphoneCore);
//            if (im == IntPtr.Zero)
//            {
//                LOG.Error("LinphoneService.SendCameraSwtichAsInfo: Failed to create info message");
//                return;
//            }

//            LinphoneAPI.linphone_info_message_add_header(im, "action", !muteCamera ? "camera_mute_on" : "camera_mute_off");
//            LinphoneAPI.linphone_call_send_info_message(callPtr, im);
//            LinphoneAPI.linphone_info_message_destroy(im);

        }

        public void SendDtmf(VATRPCall call, char dtmf)
        {
            if (call == null)
            {
                LOG.Warn("Cannot terminate call. Cause - Null call");
                return;
            }

            if ( LinphoneAPI.linphone_call_send_dtmf(call.NativeCallPtr, dtmf) != 0 )
                LOG.Error(string.Format( "Can't send dtmf {0}. Call {1}", dtmf, call.NativeCallPtr));
        }

        public void SetIncomingCallRingingTimeout(int timeout)
        {
            if (linphoneCore == IntPtr.Zero) return;
            if (LinphoneAPI.linphone_core_get_inc_timeout(linphoneCore) != timeout)
                LinphoneAPI.linphone_core_set_inc_timeout(linphoneCore, timeout);
        }

        #endregion

        #region Messaging

        public bool IsRttEnabled(IntPtr callPtr) // CJM : RTT, this is used to determine if the RTT viewmodel will be loaded since this takes place after ring and about to pickup
        {// only called if the RTT box in the ACE APP is checked
            if (callPtr == IntPtr.Zero)
                return false;

            IntPtr callerParams = LinphoneAPI.linphone_call_get_remote_params(callPtr);

            int cjm_rtt = LinphoneAPI.linphone_call_params_realtime_text_enabled(callerParams); // CJM: RTT, this is flagging it false on NTL AD but not on ACL

            return callerParams != IntPtr.Zero && cjm_rtt != 0;
        }

        public void AcceptRTTProposition(IntPtr callPtr)
        { // CJM : RTT, this wasnt even hit as a brk point when enabled
            if (linphoneCore == IntPtr.Zero)
            {
                if (ErrorEvent != null)
                    ErrorEvent(null, "Cannot receive call when Linphone Core is not working.");
                return;
            }

            lock (callLock)
            {
                VATRPCall call = FindCall(callPtr);

                if (call == null)
                {
                    LOG.Warn("Cannot accept call. Cause - Null call");
                    return;
                }

                IntPtr paramsCopy =
                    LinphoneAPI.linphone_call_params_copy(
                        LinphoneAPI.linphone_call_get_current_params(call.NativeCallPtr));
                LinphoneAPI.linphone_call_params_enable_realtime_text(paramsCopy, true);
                LinphoneAPI.linphone_core_accept_call_update(linphoneCore, call.NativeCallPtr, paramsCopy);
            }
        }

        public void SendRTTProposition(IntPtr callPtr)
        {
            if (linphoneCore == IntPtr.Zero)
            {
                if (ErrorEvent != null)
                    ErrorEvent(null, "Cannot receive call when Linphone Core is not working.");
                return;
            }

            lock (callLock)
            {
                VATRPCall call = FindCall(callPtr);

                if (call == null)
                {
                    LOG.Warn("Cannot accept call. Cause - Null call");
                    return;
                }

                IntPtr paramsCopy =
                    LinphoneAPI.linphone_call_params_copy(
                        LinphoneAPI.linphone_call_get_current_params(call.NativeCallPtr));
                LinphoneAPI.linphone_call_params_enable_realtime_text(paramsCopy, true);
                LinphoneAPI.linphone_core_update_call(linphoneCore, call.NativeCallPtr, paramsCopy);
            }
        }

        public bool SendChar(uint charCode, IntPtr callPtr, ref IntPtr chatRoomPtr, ref IntPtr chatMsgPtr)
        {
            bool retVal = false;

            lock (callLock)
            {
                VATRPCall call = FindCall(callPtr);
                if (call == null)
                    return false;

                chatRoomPtr = LinphoneAPI.linphone_call_get_chat_room(callPtr);
                //  Swings fjr 4/28/2017
                //chatRoomPtr = LinphoneAPI.linphone_chat_room_ref(callPtr);
                //chatRoomPtr = LinphoneAPI.linphone_core_get_chat_room_from_uri(callPtr);

                /*create a chat room associated to this call*/
                if (chatRoomPtr != IntPtr.Zero)
                {
                    if (chatMsgPtr == IntPtr.Zero)
                    {
                        chatMsgPtr = LinphoneAPI.linphone_chat_room_create_message(chatRoomPtr, "");
                        //  fjr Notes
                        //LinphoneChatMessageCbs *cbs = linphone_chat_message_get_callbacks(message);
                        //linphone_chat_message_get_callbacks()
                    }
                }

                if (chatMsgPtr != IntPtr.Zero)
                {
                    int retCode = 1;
                    if (charCode == '\r' || charCode == '\n')
                    {
                        OnMessageStatusChanged(chatMsgPtr,
                                LinphoneChatMessageState.LinphoneChatMessageStateDelivered);
                        if (chatMsgPtr != IntPtr.Zero)
                        {
                            LinphoneAPI.linphone_chat_room_send_chat_message(chatRoomPtr, chatMsgPtr); /*sending message*/
                        }
                        chatMsgPtr = IntPtr.Zero;
                    }
                    else
                    {
                        retCode = LinphoneAPI.linphone_chat_message_put_char(chatMsgPtr, charCode);
                    }
                    retVal = (retCode == 0);
                }
            }
            return retVal;
        }

        public bool SendChatMessage(VATRPChat chat, string message, ref IntPtr msgPtr)
        {
            if (chat == null)
                return false;

            lock (messagingLock)
            {
                IntPtr chatPtr = LinphoneAPI.linphone_core_get_chat_room_from_uri(linphoneCore, chat.Contact.ID);
                chat.NativePtr = chatPtr;

                msgPtr = LinphoneAPI.linphone_chat_room_create_message(chat.NativePtr, message);
                LinphoneAPI.linphone_chat_message_ref(msgPtr);
            }
            if (msgPtr != IntPtr.Zero)
            {
                var cmd = new SendChatMessageCommand(msgPtr, chat.NativePtr);
                lock (commandQueue)
                {
                    commandQueue.Enqueue(cmd);
                }
            }

            return true;
        }

        public LinphoneChatMessageState GetMessageStatus(IntPtr messagePtr)
        {
            return LinphoneAPI.linphone_chat_message_get_state(messagePtr);
        }

        public void MarkChatAsRead(IntPtr cr)
        {
            lock (messagingLock)
            {
                if (cr != IntPtr.Zero)
                    LinphoneAPI.linphone_chat_room_mark_as_read(cr);
            }
        }

        #endregion

        #region Video

        public bool IsCameraEnabled(IntPtr callPtr)
        {
            if (callPtr == IntPtr.Zero)
                return false;

            return LinphoneAPI.linphone_call_camera_enabled(callPtr) == 1;
        }

        public void EnableVideo(bool enable, bool automaticallyInitiate, bool automaticallyAccept)
		{
            if ((linphoneCore == null) || (linphoneCore == IntPtr.Zero))
            {
                return;
            }
            // if current account exists and we are enabling video, intialize initiate and accept vars to account. Otherwise go with previous
            //   implementation - based on enable.
            bool autoInitiate = enable;
            bool autoAccept = enable;
            if (enable) // if we are not enabling video, do not allow autoInitiate and auto Accept to be true.
            {
                autoInitiate = automaticallyInitiate;
                autoAccept = automaticallyAccept;
            }

			var t_videoPolicy = new LinphoneVideoPolicy()
			{
				automatically_initiate = autoInitiate,
				automatically_accept = autoAccept
			};

			var t_videoPolicyPtr = Marshal.AllocHGlobal(Marshal.SizeOf(t_videoPolicy));
            Marshal.StructureToPtr(t_videoPolicy, t_videoPolicyPtr, false);

            LinphoneVideoPolicy new_videoPolicy = (LinphoneVideoPolicy)Marshal.PtrToStructure(t_videoPolicyPtr, typeof(LinphoneVideoPolicy));
            Console.WriteLine("before calling core: new_videoPolicy values: new_videoPolicy.automatically_accept=" + new_videoPolicy.automatically_accept.ToString() + " new_videoPolicy.automatically_initiate=" + new_videoPolicy.automatically_initiate);

            if (t_videoPolicyPtr != IntPtr.Zero)
			{

				LinphoneAPI.linphone_core_enable_video_capture(linphoneCore, enable);
				LinphoneAPI.linphone_core_enable_video_display(linphoneCore, enable);
				LinphoneAPI.linphone_core_set_video_policy(linphoneCore, t_videoPolicyPtr);
                Console.WriteLine("linphone_core_set_video_policy sent: t_videoPolicy values: t_videoPolicy.automatically_accept=" + t_videoPolicy.automatically_accept.ToString() + " t_videoPolicy.automatically_initiate=" + t_videoPolicy.automatically_initiate);
                Marshal.FreeHGlobal(t_videoPolicyPtr);
			}

            // here - if I call linphone_core_get_video_policy it returns a policy with automatically_accept = false, automatically_initiate = true
            IntPtr testPolicyPtr = LinphoneAPI.linphone_core_get_video_policy(linphoneCore);
            t_videoPolicy = (LinphoneVideoPolicy)Marshal.PtrToStructure(testPolicyPtr, typeof(LinphoneVideoPolicy));

            Console.WriteLine("linphone_core_get_video_policy returns: t_videoPolicy values: t_videoPolicy.automatically_accept=" + t_videoPolicy.automatically_accept.ToString() + " t_videoPolicy.automatically_initiate=" + t_videoPolicy.automatically_initiate);
        }

        // Liz E: needed for unified settings
        public bool IsEchoCancellationEnabled()
        {
            if (linphoneCore == IntPtr.Zero)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                return false;
            }

            return LinphoneAPI.linphone_core_echo_cancellation_enabled(linphoneCore) == 1;
        }

        // Liz E: needed for unified settings
        public void EnableEchoCancellation(bool enable)
        {
            if (linphoneCore == IntPtr.Zero)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                return;
            }

            LinphoneAPI.linphone_core_enable_echo_cancellation(linphoneCore, enable);
        }

        // Liz E: needed for unified settings
        public bool IsSelfViewEnabled()
        {
            if (linphoneCore == IntPtr.Zero)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                return false;
            }

            return LinphoneAPI.linphone_core_self_view_enabled(linphoneCore) == 1;
        }

        // Liz E: needed for unified settings
        public void EnableSelfView(bool enable)
        {
            if (linphoneCore == IntPtr.Zero)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                return;
            }

            LinphoneAPI.linphone_core_enable_self_view(linphoneCore, enable);
        }

		public void SwitchSelfVideo()
		{
            if (linphoneCore == IntPtr.Zero)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                return;
            }

			bool isSelfViewEnabled = LinphoneAPI.linphone_core_self_view_enabled(linphoneCore) == 1;
			LinphoneAPI.linphone_core_enable_self_view(linphoneCore, !isSelfViewEnabled);
		}
        public bool SetPreviewVideoSizeByName(string name)
        {
            string[] supportedResolutions = { "1080p", "720p", "svga", "4cif", "vga", "cif", "qvga", "qcif" };

            if (!supportedResolutions.Contains(name.ToLower()))
                return false;

            LinphoneAPI.linphone_core_set_preview_video_size_by_name(linphoneCore, name.ToLower());
            return true;
        }

		public void SetVideoPreviewWindowHandle(IntPtr hWnd, bool reset = false)
		{
            if (linphoneCore ==IntPtr.Zero)
                return;
            LinphoneAPI.linphone_core_enable_video_preview(linphoneCore, !reset);
		    if (reset)
		    {
		        LinphoneAPI.linphone_core_set_native_preview_window_id(linphoneCore, -1);
		    }
		    else
		    {
		        LinphoneAPI.linphone_core_set_native_preview_window_id(linphoneCore, hWnd.ToInt64());
		    }
		}

		public void SetPreviewVideoSize(MSVideoSize w, MSVideoSize h)
		{
			var t_videoSize = new MSVideoSizeDef()
			{
				height = Convert.ToInt32(h),
				width = Convert.ToInt32(w)
			};

			var t_videoSizePtr = Marshal.AllocHGlobal(Marshal.SizeOf(t_videoSize));
			if (t_videoSizePtr != IntPtr.Zero)
			{
				LinphoneAPI.linphone_core_set_preview_video_size(linphoneCore, t_videoSizePtr);
				Marshal.FreeHGlobal(t_videoSizePtr);
			}
		}

		public void SetVideoCallWindowHandle(IntPtr hWnd, bool reset = false)
		{
			if (reset)
			{
				LinphoneAPI.linphone_core_set_native_video_window_id(linphoneCore, -1);
			}
			else
			{
				LinphoneAPI.linphone_core_set_native_video_window_id(linphoneCore, hWnd.ToInt64());
			}
		}

		public bool IsVideoEnabled(VATRPCall call)
		{
            if (linphoneCore == IntPtr.Zero)
                return false;
            IntPtr curCallPtr = LinphoneAPI.linphone_core_get_current_call(linphoneCore);
		    if (curCallPtr == IntPtr.Zero)
		        return false;
            var linphoneCallParams = LinphoneAPI.linphone_call_get_current_params(curCallPtr);
            var videoCodecName = string.Empty;
            if (linphoneCallParams != IntPtr.Zero)
                videoCodecName = GetUsedVideoCodec(linphoneCallParams);
			return !string.IsNullOrEmpty(videoCodecName);
		}

        public void UpdateMediaSettings(VATRPAccount account)
	    {
            if (linphoneCore == IntPtr.Zero) return;

	        if (account == null)
	        {
                LOG.Error("Account is null");
	            return;
	        }

            MuteCall(account.MuteMicrophone);
            MuteSpeaker(account.MuteSpeaker);

            EnableEchoCancellation(account.EchoCancel);

            EnableSelfView(account.ShowSelfView);
            // Liz E. - note: get_video_preset is not available in liphoneAPI. Null is an accepted value
            //    for Linphone API as default.
            LOG.Info("Set preferred video size by name: " + account.VideoPreset);
            LinphoneAPI.linphone_core_set_video_preset(linphoneCore, account.VideoPreset);
            LinphoneAPI.linphone_core_set_preferred_framerate(linphoneCore, account.PreferredFPS);

            IntPtr namePtr = LinphoneAPI.linphone_core_get_preferred_video_size_name(linphoneCore);
            MSVideoSize preferredVideoSize = LinphoneAPI.linphone_core_get_preferred_video_size(linphoneCore);
            if (namePtr != IntPtr.Zero)
            {
                string name = Marshal.PtrToStringAnsi(namePtr);
                if (!string.IsNullOrWhiteSpace(account.PreferredVideoId) && account.PreferredVideoId != name)
                {
                    LOG.Info("Set preferred video size by name: " + account.PreferredVideoId);
                    LinphoneAPI.linphone_core_set_preferred_video_size_by_name(linphoneCore, account.PreferredVideoId);
                    MSVideoSize preferredVideoSizeAfterChange = LinphoneAPI.linphone_core_get_preferred_video_size(linphoneCore);

                    int bandwidth = 512;
                    switch (account.PreferredVideoId)
                    {
                        case "720p":
                            bandwidth = 2000;
                            break;
                        case "svga":
                            bandwidth = 2000;
                            break;
                        case "vga":
                            bandwidth = 1500;
                            break;
                        case "cif":
                            bandwidth = 660;
                            break;
                        case "qvga":
                            bandwidth = 410;
                            break;
                        case "qcif":
                            bandwidth = 256;
                            break;
                    }

                    account.DownloadBandwidth = bandwidth;
                    LinphoneAPI.linphone_core_set_download_bandwidth(linphoneCore, bandwidth);

                    account.UploadBandwidth = bandwidth;
                    LinphoneAPI.linphone_core_set_upload_bandwidth(linphoneCore, bandwidth);
                }
            }

            UpdateMediaEncryption();
	    }

        private void UpdateMediaEncryption()
        {
            LinphoneMediaEncryption lme = LinphoneAPI.linphone_core_get_media_encryption(linphoneCore);

            if (lme == LinphoneConfig.MediaEncryption)
                return;

            int retVal = LinphoneAPI.linphone_core_set_media_encryption(linphoneCore, LinphoneConfig.MediaEncryption);
            if (retVal == 0)
            {
                LOG.Info("Media encryption set to " + LinphoneConfig.MediaEncryption.ToString());
            }
            else
            {
                LOG.Error("Failed to update Linphone media encryption");
            }

            string certPath = manager.BuildStoragePath("dtlsCert.pem");

            if (File.Exists(certPath))
            {
                LinphoneAPI.linphone_core_set_user_certificates_path(linphoneCore, Environment.ExpandEnvironmentVariables(certPath));
            }
            else
            {
                LOG.Debug("Notify - File not found. Client certificate for SRTP not present in VATRP directory.");
            }
        }

        #endregion

		#region Codecs

        public bool UpdateNativeCodecs(VATRPAccount account, CodecType codecType)
	    {
            var retValue = true;

            if (linphoneCore == IntPtr.Zero)
				throw new Exception("Linphone not initialized");

	        if (account == null)
	            throw new ArgumentNullException("Account is not defined");

            var cfgCodecs = codecType == CodecType.Video ? account.VideoCodecsList : account.AudioCodecsList;
            var linphoneCodecs = codecType == CodecType.Video ? _videoCodecs : _audioCodecs;
            var tmpCodecs = new List<VATRPCodec>();
            foreach (var cfgCodec in cfgCodecs)
            {
                // find cfgCodec in linphone codec list
                var pt = LinphoneAPI.linphone_core_find_payload_type(linphoneCore, cfgCodec.CodecName, LinphoneAPI.LINPHONE_FIND_PAYLOAD_IGNORE_RATE,
                    cfgCodec.Channels);
                if (pt == IntPtr.Zero)
                {
                    LOG.Warn(string.Format("Codec not found: {0} , Channels: {1} ", cfgCodec.CodecName,
                        cfgCodec.Channels));
                    tmpCodecs.Add(cfgCodec);
                }
            }

            foreach (var codec in linphoneCodecs)
            {
                if (!cfgCodecs.Contains(codec))
                {
                    LOG.Info(string.Format("Adding codec into configuration: {0} , Channels: {1} ", codec.CodecName, codec.Channels));
                    cfgCodecs.Add(codec);
                }
            }

            foreach (var codec in tmpCodecs)
            {
                LOG.Info(string.Format("Removing Codec from configuration: {0} , Channels: {1} ", codec.CodecName, codec.Channels));
                cfgCodecs.Remove(codec);
            }

            foreach (var codec in linphoneCodecs)
            {
                for (int i = 0; i < cfgCodecs.Count; i++)
                {
                    if (cfgCodecs[i].CodecName == codec.CodecName && cfgCodecs[i].Rate == codec.Rate && cfgCodecs[i].Channels == codec.Channels)
                    {
                        cfgCodecs[i].Priority = codec.Priority;
                        cfgCodecs[i].Status = codec.Status;
                    }
                }
            }

            return retValue;
	    }

        public bool UpdateCodecsAccessibility(VATRPAccount account, CodecType codecType)
        {
            var retValue = true;

            if (linphoneCore == IntPtr.Zero)
                throw new Exception("Linphone not initialized");

            if (account == null)
                throw new ArgumentNullException("Account is not defined");

            var cfgCodecs = codecType == CodecType.Video ? account.VideoCodecsList : account.AudioCodecsList;
            var linphoneCodecs = codecType == CodecType.Video ? _videoCodecs : _audioCodecs;

            foreach (var cfgCodec in cfgCodecs)
            {
                var payloadPtr = LinphoneAPI.linphone_core_find_payload_type(linphoneCore, cfgCodec.CodecName, cfgCodec.IPBitRate,
                    cfgCodec.Channels);
                if (payloadPtr == IntPtr.Zero)
                    continue;
                if (cfgCodec.Status == ( LinphoneAPI.linphone_core_payload_type_enabled(linphoneCore, payloadPtr) == 1))
                    continue;
                LinphoneAPI.linphone_core_enable_payload_type(linphoneCore, payloadPtr, cfgCodec.Status);
            }

            return retValue;
        }

	    public void FillCodecsList(VATRPAccount account, CodecType codecType)
	    {
            if (account == null)
                throw new ArgumentNullException("Account is not defined");
            var cfgCodecs = codecType == CodecType.Video ? account.VideoCodecsList : account.AudioCodecsList;
            var linphoneCodecs = codecType == CodecType.Video ? _videoCodecs : _audioCodecs;
            cfgCodecs.Clear();
            cfgCodecs.AddRange(linphoneCodecs);
	    }

        public void configureFmtpCodec()
        {
            var h263PtPtr = LinphoneAPI.linphone_core_find_payload_type(linphoneCore, "H263", 90000, -1);
            setFmtpSetting(h263PtPtr, "CIF=1;QCIF=1", "CIF=1;QCIF=1");
            var h264PtPtr = LinphoneAPI.linphone_core_find_payload_type(linphoneCore, "H264", 90000, -1);
            // cjm-may18 -- adjusting the fmtp attribute for outbound dialing to correspond with Asterisk res_pjsi_session.c patch
            setFmtpSetting(h264PtPtr, null, "level-asymmetry-allowed=1;packetization-mode=1;profile-level-id=42001f");
            //setFmtpSetting(h264PtPtr, null, "packetization-mode=1;profile-level-id=42801F");
        }
        private void setFmtpSetting(IntPtr ptPtr, string sendFmtp, string recvFmtp)
        {
            if (ptPtr != IntPtr.Zero)
            {
                var payload = (PayloadType)Marshal.PtrToStructure(ptPtr, typeof(PayloadType));
                if (recvFmtp != null)
                     payload.recv_fmtp = recvFmtp;
                if (sendFmtp != null)
                    payload.send_fmtp = sendFmtp;
                Marshal.StructureToPtr(payload, ptPtr, false);
                LinphoneAPI.linphone_core_payload_type_enabled(linphoneCore, ptPtr);
            }

        }
		private void LoadAudioCodecs()
		{
            if (linphoneCore == IntPtr.Zero)
                throw new Exception("Linphone not initialized");
            _audioCodecs.Clear();

            // remove
		    string[] primaryCodecs = {"G722", "PCMU", "PCMA", "speex", "speex"};
            int[] rate = { LinphoneAPI.LINPHONE_FIND_PAYLOAD_IGNORE_RATE, LinphoneAPI.LINPHONE_FIND_PAYLOAD_IGNORE_RATE, LinphoneAPI.LINPHONE_FIND_PAYLOAD_IGNORE_RATE, 28000, 8000 };

            _linphoneAudioCodecsList = IntPtr.Zero;
		    int i;
		    for (i = 0; i < primaryCodecs.Length; i++)
		    {
		        var pt = LinphoneAPI.linphone_core_find_payload_type(linphoneCore, primaryCodecs[i],
                    rate[i],
		            LinphoneAPI.LINPHONE_FIND_PAYLOAD_IGNORE_CHANNELS);

		        if (pt != IntPtr.Zero)
		        {
		            _linphoneAudioCodecsList = LinphoneAPI.ms_list_append(_linphoneAudioCodecsList, pt);
		        }
		    }

            IntPtr audioCodecListPtr = LinphoneAPI.linphone_core_get_audio_codecs(linphoneCore);

            MSList curStruct;
            do
            {
                curStruct.next = IntPtr.Zero;
                curStruct.prev = IntPtr.Zero;
                curStruct.data = IntPtr.Zero;
                curStruct = (MSList)Marshal.PtrToStructure(audioCodecListPtr, typeof(MSList));
                if (curStruct.data != IntPtr.Zero)
                {
                    var payload = (PayloadType)Marshal.PtrToStructure(curStruct.data, typeof(PayloadType));

                    bool found = false;
                    for (i = 0; i < primaryCodecs.Length; i++)
                    {
                        if (primaryCodecs[i] == payload.mime_type)
                        {
                            if (rate[i] == LinphoneAPI.LINPHONE_FIND_PAYLOAD_IGNORE_RATE || payload.clock_rate == rate[i])
                            {
                                found = true;
                                break;
                            }
                        }
                    }

                    if (!found)
                    {
                        _linphoneAudioCodecsList = LinphoneAPI.ms_list_append(_linphoneAudioCodecsList, curStruct.data);
                    }
                }
                audioCodecListPtr = curStruct.next;
            } while (curStruct.next != IntPtr.Zero);

            LinphoneAPI.linphone_core_set_audio_codecs(linphoneCore, _linphoneAudioCodecsList);

            //  enable codecs
            audioCodecListPtr = LinphoneAPI.linphone_core_get_audio_codecs(linphoneCore);
            do
            {
                curStruct.next = IntPtr.Zero;
                curStruct.prev = IntPtr.Zero;
                curStruct.data = IntPtr.Zero;
                curStruct = (MSList)Marshal.PtrToStructure(audioCodecListPtr, typeof(MSList));
                if (curStruct.data != IntPtr.Zero)
                {
                    var payload = (PayloadType)Marshal.PtrToStructure(curStruct.data, typeof(PayloadType));

                    bool enable = false;
                    for (i = 0; i < primaryCodecs.Length; i++)
                    {
                        if (primaryCodecs[i] == payload.mime_type)
                        {
                            if (rate[i] == LinphoneAPI.LINPHONE_FIND_PAYLOAD_IGNORE_RATE || payload.clock_rate == rate[i])
                            {
                                enable = true;
                                break;
                            }
                        }
                    }
                    IntPtr payloadPtr = LinphoneAPI.linphone_core_find_payload_type(linphoneCore, payload.mime_type, payload.clock_rate, payload.channels);
                    LinphoneAPI.linphone_core_enable_payload_type(linphoneCore, payloadPtr, enable);
                }
                audioCodecListPtr = curStruct.next;
            } while (curStruct.next != IntPtr.Zero);


            audioCodecListPtr = LinphoneAPI.linphone_core_get_audio_codecs(linphoneCore);
            int index = 1;
            do
            {
                curStruct.next = IntPtr.Zero;
                curStruct.prev = IntPtr.Zero;
                curStruct.data = IntPtr.Zero;
                curStruct = (MSList)Marshal.PtrToStructure(audioCodecListPtr, typeof(MSList));
                if (curStruct.data != IntPtr.Zero)
                {
                    var payload = (PayloadType)Marshal.PtrToStructure(curStruct.data, typeof(PayloadType));
                    var codec = new VATRPCodec
                    {
                        Priority = index++,
                        Purpose = CodecType.Audio,
                        CodecName = payload.mime_type,
                        Rate = payload.normal_bitrate,
                        IPBitRate = payload.clock_rate,
                        Channels = payload.channels,
                        ReceivingFormat = payload.recv_fmtp,
                        SendingFormat = payload.send_fmtp,
                        Status = LinphoneAPI.linphone_core_payload_type_enabled(linphoneCore, curStruct.data) == 1,
                        IsUsable = LinphoneAPI.linphone_core_check_payload_type_usability(linphoneCore, curStruct.data) == 1
                    };
                    _audioCodecs.Add(codec);
                }
                audioCodecListPtr = curStruct.next;
            } while (curStruct.next != IntPtr.Zero);
		}

        private void LoadVideoCodecs()
        {
            if (linphoneCore == IntPtr.Zero)
                throw new Exception("Linphone not initialized");
            _videoCodecs.Clear();

            // remove
            string[] primaryCodecs = { "H264", "H263", "VP8" };

            _linphoneVideoCodecsList = IntPtr.Zero;
            int i;
            for (i = 0; i < primaryCodecs.Length; i++)
            {
                var pt = LinphoneAPI.linphone_core_find_payload_type(linphoneCore, primaryCodecs[i],
                    LinphoneAPI.LINPHONE_FIND_PAYLOAD_IGNORE_RATE,
                    LinphoneAPI.LINPHONE_FIND_PAYLOAD_IGNORE_CHANNELS);

                if (pt != IntPtr.Zero)
                {
                    _linphoneVideoCodecsList = LinphoneAPI.ms_list_append(_linphoneVideoCodecsList, pt);
                }
            }

            IntPtr videoCodecListPtr = LinphoneAPI.linphone_core_get_video_codecs(linphoneCore);

            MSList curStruct;
            do
            {
                curStruct.next = IntPtr.Zero;
                curStruct.prev = IntPtr.Zero;
                curStruct.data = IntPtr.Zero;
                curStruct = (MSList)Marshal.PtrToStructure(videoCodecListPtr, typeof(MSList));
                if (curStruct.data != IntPtr.Zero)
                {
                    var payload = (PayloadType)Marshal.PtrToStructure(curStruct.data, typeof(PayloadType));

                    bool found = false;
                    for (i = 0; i < primaryCodecs.Length; i++)
                    {
                        if (primaryCodecs[i] == payload.mime_type)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        _linphoneVideoCodecsList = LinphoneAPI.ms_list_append(_linphoneVideoCodecsList, curStruct.data);
                    }
                }
                videoCodecListPtr = curStruct.next;
            } while (curStruct.next != IntPtr.Zero);

            LinphoneAPI.linphone_core_set_video_codecs(linphoneCore, _linphoneVideoCodecsList);

            //  enable codecs
            videoCodecListPtr = LinphoneAPI.linphone_core_get_video_codecs(linphoneCore);
            do
            {
                curStruct.next = IntPtr.Zero;
                curStruct.prev = IntPtr.Zero;
                curStruct.data = IntPtr.Zero;
                curStruct = (MSList)Marshal.PtrToStructure(videoCodecListPtr, typeof(MSList));
                if (curStruct.data != IntPtr.Zero)
                {
                    var payload = (PayloadType)Marshal.PtrToStructure(curStruct.data, typeof(PayloadType));

                    bool enable = false;
                    for (i = 0; i < primaryCodecs.Length; i++)
                    {
                        if (primaryCodecs[i] == payload.mime_type)
                        {
                            enable = true;
                            break;
                        }
                    }
                    IntPtr payloadPtr = LinphoneAPI.linphone_core_find_payload_type(linphoneCore, payload.mime_type, payload.clock_rate, payload.channels);
                    LinphoneAPI.linphone_core_enable_payload_type(linphoneCore, payloadPtr, enable);
                }
                videoCodecListPtr = curStruct.next;
            } while (curStruct.next != IntPtr.Zero);


            videoCodecListPtr = LinphoneAPI.linphone_core_get_video_codecs(linphoneCore);
            int index = 1;
            do
            {
                curStruct.next = IntPtr.Zero;
                curStruct.prev = IntPtr.Zero;
                curStruct.data = IntPtr.Zero;
                curStruct = (MSList)Marshal.PtrToStructure(videoCodecListPtr, typeof(MSList));
                if (curStruct.data != IntPtr.Zero)
                {
                    var payload = (PayloadType)Marshal.PtrToStructure(curStruct.data, typeof(PayloadType));
                    var codec = new VATRPCodec
                    {
                        Priority = index++,
                        Purpose = CodecType.Video,
                        CodecName = payload.mime_type,
                        Rate = payload.normal_bitrate,
                        IPBitRate = payload.clock_rate,
                        Channels = payload.channels,
                        ReceivingFormat = payload.recv_fmtp,
                        SendingFormat = payload.send_fmtp,
                        Status = LinphoneAPI.linphone_core_payload_type_enabled(linphoneCore, curStruct.data) == 1,
                        IsUsable = LinphoneAPI.linphone_core_check_payload_type_usability(linphoneCore, curStruct.data) == 1
                    };
                    _videoCodecs.Add(codec);
                }
                videoCodecListPtr = curStruct.next;
            } while (curStruct.next != IntPtr.Zero);
        }

		#endregion

        #region Networking

        public bool UpdateNetworkingParameters(VATRPAccount account)
        {
            if (linphoneCore == IntPtr.Zero)
                throw new Exception("Linphone not initialized");

            if (account == null)
            {
                LOG.Error("UpdateNetworkingParameters: Account is NULL");
                return false;
            }

            var ip6Enabled = LinphoneAPI.linphone_core_ipv6_enabled(linphoneCore) == 1;
            if (ip6Enabled != account.EnableIPv6)
            {
                LinphoneAPI.linphone_core_enable_ipv6(linphoneCore, account.EnableIPv6);
            }
            LOG.Info(string.Format("UpdateNetworkingParameters: IPv6 is {0}", account.EnableIPv6 ? "enabled" : "disabled"));

            var address = string.Format(account.STUNAddress);
            LinphoneAPI.linphone_core_set_stun_server(linphoneCore, address);
            if (account.EnableSTUN || account.EnableICE)
            {
                if (account.EnableSTUN)
                {
                    LinphoneAPI.linphone_core_set_firewall_policy(linphoneCore,
                        LinphoneFirewallPolicy.LinphonePolicyUseStun);
                    LOG.Info("UpdateNetworkingParameters: Enable STUN. " + address);
                }
                else
                {
                    LinphoneAPI.linphone_core_set_firewall_policy(linphoneCore,
                        LinphoneFirewallPolicy.LinphonePolicyUseIce);
                    LOG.Info("UpdateNetworkingParameters: Enable ICE. " + address);
                }
            }
            else
            {
                LinphoneAPI.linphone_core_set_firewall_policy(linphoneCore,
                    LinphoneFirewallPolicy.LinphonePolicyNoFirewall);
                LOG.Info("UpdateNetworkingParameters: No Firewall. Stun server is " + address);
            }

            // TODO, Disable adaptive rate algorithm, since it caused bad video
            LinphoneAPI.linphone_core_set_adaptive_rate_algorithm(linphoneCore, account.AdaptiveRateAlgorithm);
            LinphoneAPI.linphone_core_enable_adaptive_rate_control(linphoneCore, account.EnableAdaptiveRate);
            LinphoneAPI.linphone_core_set_upload_bandwidth(linphoneCore, account.UploadBandwidth);
            LinphoneAPI.linphone_core_set_download_bandwidth(linphoneCore, account.DownloadBandwidth);

            // quality of service
            LinphoneAPI.linphone_core_set_sip_dscp(linphoneCore, account.EnableQualityOfService ? account.SipDscpValue : 0);
            LinphoneAPI.linphone_core_set_audio_dscp(linphoneCore, account.EnableQualityOfService ? account.AudioDscpValue : 0);
            LinphoneAPI.linphone_core_set_video_dscp(linphoneCore, account.EnableQualityOfService ? account.VideoDscpValue : 0);
            return false;
        }

        public bool UpdateAdvancedParameters(VATRPAccount account)
        {
            if (account.Logging == "Verbose")
            {
                LinphoneAPI.linphone_core_set_log_level_mask(OrtpLogLevel.ORTP_MESSAGE);
                _enableLogging = true;
                LOG.Info("Setting Linphone logging level to DEBUG");
            }
            else
            {
                LinphoneAPI.linphone_core_set_log_level_mask(OrtpLogLevel.ORTP_FATAL);
                _enableLogging = false;
                LOG.Info("Setting Linphone logging level to OFF");
            }
            return true;
        }

        public void SetAVPFMode(LinphoneAVPFMode mode, LinphoneRTCPMode rtcpMode)
	    {
            if (linphoneCore == IntPtr.Zero)
                return;

            LOG.Info("AVPF mode changed to " + mode);
            LinphoneAPI.linphone_core_set_avpf_mode(linphoneCore, mode);

	        if (proxy_cfg != IntPtr.Zero)
	        {
                LinphoneAPI.linphone_proxy_config_set_avpf_mode(proxy_cfg, mode);
                LinphoneAPI.linphone_proxy_config_set_avpf_rr_interval(proxy_cfg, 3);
                LinphoneAPI.linphone_core_set_avpf_rr_interval(linphoneCore, 3);
	        }

	        IntPtr coreConfig = LinphoneAPI.linphone_core_get_config(linphoneCore);
            if (coreConfig != IntPtr.Zero)
            {
                LOG.Info("RTCP mode changing to " + rtcpMode);
                LinphoneAPI.linphone_config_set_int(coreConfig, "rtp", "rtcp_xr_enabled", 0);
                LinphoneAPI.linphone_config_set_int(coreConfig, "rtp", "rtcp_xr_voip_metrics_enabled", 0);
                LinphoneAPI.linphone_config_set_int(coreConfig, "rtp", "rtcp_xr_stat_summary_enabled", 0);
                LinphoneAPI.linphone_config_set_int(coreConfig, "rtp", "rtcp_fb_implicit_rtcp_fb", (int)rtcpMode);
            }
        }

        public int GetAVPFMode()
        {
            if (linphoneCore == IntPtr.Zero)
                return (int) LinphoneAVPFMode.LinphoneAVPFDefault;

            return LinphoneAPI.linphone_core_get_avpf_mode(linphoneCore);
        }

        public void SetRTCPFeedback(string settingValue)
        {
            if (settingValue.Equals("Off"))
            {
                SetAVPFMode(LinphoneAVPFMode.LinphoneAVPFDisabled, LinphoneRTCPMode.LinphoneRTCPDisabled);
            }
            else if (settingValue.Equals("Implicit"))
            {
                SetAVPFMode(LinphoneAVPFMode.LinphoneAVPFDisabled, LinphoneRTCPMode.LinphoneRTCPEnabled);
            }
            else if (settingValue.Equals("Explicit"))
            {
                SetAVPFMode(LinphoneAVPFMode.LinphoneAVPFEnabled, LinphoneRTCPMode.LinphoneRTCPEnabled);
            }
        }

        #endregion

		#region Events

        private void OnLinphoneLog(IntPtr domain, OrtpLogLevel lev, IntPtr fmt, IntPtr args)
        {
            //**************************************************************************************************
            // Login in Linphone
            //*************************************************************************************************

            if (fmt == IntPtr.Zero || !_enableLogging)
                return;
            var format  = Marshal.PtrToStringAnsi(fmt);
            if (string.IsNullOrEmpty(format))
            {
                return;
            }
            lock (logLock)
            {
                try
                {
                    foreach (var formatter in placeholders)
                    {
                        int pos = format.IndexOf(formatter, 0, StringComparison.InvariantCulture);
                        while (pos != -1)
                        {
                            placeHolderItems[pos] = formatter;
                            pos = format.IndexOf(formatter, pos + 1, StringComparison.InvariantCulture);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LOG.Error("Error on formatting: " + format);
                    return;
                }

                if (placeHolderItems.Count == 0)
                {
                    LOG.Info(format);
                    return;
                }

                try
                {
                    var argsArray = new IntPtr[placeHolderItems.Count];

                    Marshal.Copy(args, argsArray, 0, placeHolderItems.Count);
                    var logOutput = new StringBuilder(format);
                    var formattedString = string.Empty;
                    int offset = 0;
                    for (int i = 0; i < placeHolderItems.Count; i++)
                    {
                        if (i >= argsArray.Length)
                            continue;
                        switch (placeHolderItems.Values[i])
                        {
                            case "%s":
                                formattedString = Marshal.PtrToStringAnsi(argsArray[i]);
                                break;
                            case "%d":
                            case "%lu":
                            case "%i":
                            case "%u":
                            case "%llu":
                            case "%f":
                            case "%p":
                                formattedString = argsArray[i].ToString();
                                break;
                            case "%x":
                            case "%X":
                                formattedString = argsArray[i].ToString("X");
                                break;
                            case "%10I64d":
                                formattedString = argsArray[i].ToInt64().ToString().PadLeft(10);
                                break;
                            case "%-9i":
                                formattedString = argsArray[i].ToString().PadLeft(9, '-');
                                break;
                            case "%-20s":
                            case "%-19s":
                                formattedString = Marshal.PtrToStringAnsi(argsArray[i]);
                                if (formattedString != null)
                                    formattedString = formattedString.PadLeft(19, '-');
                                break;
                            case "%-19g":
                                formattedString = argsArray[i].ToString().PadLeft(19, '-');
                                break;
                            case "%-10g":
                                formattedString = argsArray[i].ToString().PadLeft(10, '-');
                                break;
                            case "%3.1f":
                            case "%5.1f":
                                formattedString = argsArray[i].ToString("N1");
                                break;

                            default:
                                formattedString = string.Empty;
                                break;
                        }
                        if (formattedString != null)
                        {
                            logOutput.Remove(placeHolderItems.Keys[i] + offset, placeHolderItems.Values[i].Length);
                            if (formattedString.Length > 0)
                                logOutput.Insert(placeHolderItems.Keys[i] + offset, formattedString);

                            // update offset
                            offset += formattedString.Length - placeHolderItems.Values[i].Length;
                        }
                    }

                    LOG.Info(logOutput);
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    placeHolderItems.Clear();
                }
            }
        }

        void OnBuddyInfoUpdated(IntPtr lc, IntPtr friend)
        {
            Debug.Write("Stop!");
        }

        // this is a linphone dll callback so cstate is coming from within based on the sal_register of proxy config
		void OnRegistrationChanged (IntPtr lc, IntPtr cfg, LinphoneRegistrationState cstate, string message)
		{
            //****************************************************************************************************
            // This method will called when Registration state is changed.
            //****************************************************************************************************
			if (linphoneCore == IntPtr.Zero) return;
            // Liz E. - I think that here - if the registration state has not actually changed, just return
            if (currentRegistrationState == cstate) // CJM: Login, if you fail to login, and it fails again before this point, none of the brk points
            {
//                LOG.Info("LinphoneService.OnRegistrationChanged called - but there is no change. Do nothing.");
                return;
            }
		    var erroeReason = LinphoneAPI.linphone_error_info_get_reason(cfg);

            LOG.Info("LinphoneService.OnRegistrationChanged called. Call State was:" + currentRegistrationState.ToString() + " call state changing to " + cstate.ToString());
            if (cfg == proxy_cfg)
		    {
                var reason = LinphoneAPI.linphone_proxy_config_get_error(cfg);
                currentRegistrationState = cstate;
		        if (RegistrationStateChangedEvent != null)
		            RegistrationStateChangedEvent(cstate, reason);
		        switch (cstate)
		        {
		            case LinphoneRegistrationState.LinphoneRegistrationOk:
		                LinphoneAPI.linphone_core_enable_keep_alive(linphoneCore, true);
		                break;
		            case LinphoneRegistrationState.LinphoneRegistrationFailed:
		            case LinphoneRegistrationState.LinphoneRegistrationCleared:
		                LinphoneAPI.linphone_core_enable_keep_alive(linphoneCore, false);
		                break;
		        }
		    }
        }

		void OnGlobalStateChanged(IntPtr lc, LinphoneGlobalState gstate, string message)
		{
			if (linphoneCore == IntPtr.Zero) return;

			if (GlobalStateChangedEvent != null)
				GlobalStateChangedEvent(gstate);
		}
		private void OnCallStateChanged(IntPtr lc, IntPtr callPtr, LinphoneCallState cstate, string message)
		{

            //***********************************************************************************************************************
            // This method is called when user try to connect a call and call status changed like (Trying,Calling, Incoming ) etc.
            //***********************************************************************************************************************

			if (linphoneCore == IntPtr.Zero) return;

			LOG.Info(string.Format( "OnCallStateChanged: State - {0}, CallPtr - {1}, Message: {2}", cstate, callPtr, message));

			var newstate = VATRPCallState.None;
			var direction = LinphoneCallDir.LinphoneCallIncoming;
			string remoteParty = "";
			IntPtr addressStringPtr;
		    bool removeCall = false;
			// detecting direction, state and source-destination data by state
			switch (cstate)
			{
				case LinphoneCallState.LinphoneCallIncomingReceived:
				case LinphoneCallState.LinphoneCallIncomingEarlyMedia:
					newstate = cstate == LinphoneCallState.LinphoneCallIncomingReceived
						? VATRPCallState.InProgress
						: VATRPCallState.EarlyMedia;
					addressStringPtr = LinphoneAPI.linphone_call_get_remote_address_as_string(callPtr);
			        if (addressStringPtr != IntPtr.Zero)
			        {
			            identity = Marshal.PtrToStringAnsi(addressStringPtr);
                        LinphoneAPI.ortp_free(addressStringPtr);
			        }
					remoteParty = identity;
					break;

                case LinphoneCallState.LinphoneCallOutgoingEarlyMedia:
				case LinphoneCallState.LinphoneCallConnected:
					newstate = VATRPCallState.Connected;
					break;
				case LinphoneCallState.LinphoneCallStreamsRunning:
                    newstate = VATRPCallState.StreamsRunning;
					break;
				case LinphoneCallState.LinphoneCallPausedByRemote:
                    newstate = VATRPCallState.RemotePaused;
			        break;
				case LinphoneCallState.LinphoneCallPausing:
                    newstate = VATRPCallState.LocalPausing;
					break;
                case LinphoneCallState.LinphoneCallPaused:
                    newstate = VATRPCallState.LocalPaused;
                    break;
                case LinphoneCallState.LinphoneCallResuming:
                    newstate = VATRPCallState.LocalResuming;
			        break;
				case LinphoneCallState.LinphoneCallOutgoingInit:
				case LinphoneCallState.LinphoneCallOutgoingProgress:
				case LinphoneCallState.LinphoneCallOutgoingRinging:
                    newstate = cstate != LinphoneCallState.LinphoneCallOutgoingRinging
						? VATRPCallState.Trying
						: VATRPCallState.Ringing;
					direction = LinphoneCallDir.LinphoneCallOutgoing;
					addressStringPtr = LinphoneAPI.linphone_call_get_remote_address_as_string(callPtr);
			        if (addressStringPtr != IntPtr.Zero)
			        {
			            remoteParty = Marshal.PtrToStringAnsi(addressStringPtr);
                        LinphoneAPI.ortp_free(addressStringPtr);
			        }
					break;

				case LinphoneCallState.LinphoneCallError:
                    string linphoneLibraryVersion = VATRP.LinphoneWrapper.LinphoneAPI.linphone_core_get_version_asString();
                    LOG.Info("OnCallStateChanged: CallState=LinphoneCallError .LinphoneLib Version: " + linphoneLibraryVersion);
					newstate = VATRPCallState.Error;
			        removeCall = true;
					break;

				case LinphoneCallState.LinphoneCallEnd:
					newstate = VATRPCallState.Closed;
                    removeCall = true;
					break;
				case LinphoneCallState.LinphoneCallReleased:
			        if ((_declinedCallsList != null) && _declinedCallsList.Contains(callPtr))
			        {
                        LOG.Info("   trying to remove callPtr from declinedCallList");
			            _declinedCallsList.Remove(callPtr);
                    }
                    LOG.Info("   calling linphone_call_unref");
                    try
                    {
                        LinphoneAPI.linphone_call_unref(callPtr);
                        LOG.Info("   passed unref");
                    }
                    catch (Exception ex)
                    {
                        LOG.Error("LinphoneService.OnCallStateChanged: Exception occured while calling linphone_call_unref. Details: " + ex.Message);
                    }
			        return;
			}

		    lock (callLock)
		    {
		        VATRPCall call = FindCall(callPtr);

		        if (call == null)
		        {
		            if (_declinedCallsList.Contains(callPtr))
		                return;

		            if (GetActiveCallsCount > 1)
		            {
                        callPtr = LinphoneAPI.linphone_call_ref(callPtr);
                        var cmd = new DeclineCallCommand(callPtr, LinphoneReason.LinphoneReasonBusy);
		                commandQueue.Enqueue(cmd);
                        _declinedCallsList.Add(callPtr);
		                return;
		            }

		            if (!removeCall)
		            {
                        //*******************************************************************************************************************************
                        // When call disconnected like Missed call. or Connect a call/Incoming call. Direction is incoming/outgoing etc
                        //*******************************************************************************************************************************
		                LOG.Info("Call not found. Adding new call into list. ID - " + callPtr + " Calls count: " +
		                         callsList.Count);
                        callPtr = LinphoneAPI.linphone_call_ref(callPtr);
		                call = new VATRPCall(callPtr) {CallState = newstate, CallDirection = direction};
		                CallParams from = direction == LinphoneCallDir.LinphoneCallIncoming ? call.From : call.To;
		                CallParams to = direction == LinphoneCallDir.LinphoneCallIncoming ? call.To : call.From;

		                if (
		                    !VATRPCall.ParseSipAddressEx(remoteParty, out from.DisplayName, out from.Username,
		                        out from.HostAddress,
		                        out from.HostPort))
		                    from.Username = "Unknown user";

		                if (
		                    !VATRPCall.ParseSipAddressEx(remoteParty, out to.DisplayName, out to.Username, out to.HostAddress,
		                        out to.HostPort))
		                    to.Username = "Unknown user";

		                IntPtr chatPtr = LinphoneAPI.linphone_call_get_chat_room(callPtr);

		                if (chatPtr != IntPtr.Zero)
		                {
		                    VATRPContact contact;
		                    var contactAddress = string.Empty;
		                    if (direction == LinphoneCallDir.LinphoneCallIncoming)
		                    {
                                //**********************************************************************************************
                                // Incoming call
                                //********************************************************************************************
		                        contactAddress = string.Format("{0}@{1}", from.Username, from.HostAddress);
		                        contact = new VATRPContact(new ContactID(contactAddress, chatPtr))
		                        {
		                            DisplayName = from.DisplayName,
		                            Fullname = from.Username,
		                            SipUsername = from.Username
		                        };
		                    }
		                    else
		                    {
                                //**********************************************************************************************
                                // When call is Outgoing
                                //********************************************************************************************
		                        contactAddress = string.Format("{0}@{1}", to.Username, to.HostAddress);
		                        contact = new VATRPContact(new ContactID(contactAddress, chatPtr))
		                        {
		                            DisplayName = to.DisplayName,
		                            Fullname = to.Username,
		                            SipUsername = to.Username
		                        };
		                    }
		                    contact.RegistrationName = contactAddress;
                            call.ChatRoom = manager.ChatService.InsertRttChat(contact, chatPtr, callPtr);
		                    var loggedContact = manager.ContactService.FindLoggedInContact();
		                    if (loggedContact != null)
		                        call.ChatRoom.AddContact(loggedContact);
		                }

		                callsList.Add(call);
		            }
		        }

		        if (call != null)
		        {
                    //**********************************************************************************************
                    // When call is not null. Incoming and Outgoing both calls.
                    //********************************************************************************************
		            call.LinphoneMessage =  message;
		            call.CallState = newstate;

                    if (call.CallState == VATRPCallState.Error || call.CallState == VATRPCallState.Closed)
		            {
		                IntPtr errorReason = LinphoneAPI.linphone_call_get_error_info(callPtr);
		                if (errorReason != IntPtr.Zero)
		                {
                            call.SipErrorCode = LinphoneAPI.linphone_error_info_get_protocol_code(errorReason);
		                }
		            }
                    if (CallStateChangedEvent != null)
                        CallStateChangedEvent(call);
		            if (removeCall)
		            {
		                callsList.Remove(call);
		                LOG.Info(string.Format("Call removed from list. Call - {0}. Total calls in list: {1}", callPtr,
		                    callsList.Count));
		            }
		        }
		    }
		}

		private void OnNotifyEventReceived(IntPtr lc, IntPtr eventPtr, string notified_event, IntPtr bodyPtr)
		{
			if (linphoneCore == IntPtr.Zero) return;

		    if (bodyPtr != IntPtr.Zero)
		    {
		        IntPtr subTypePtr = LinphoneAPI.linphone_content_get_subtype(bodyPtr);
		        if (subTypePtr != IntPtr.Zero)
		        {
		            if (Marshal.PtrToStringAnsi(subTypePtr) == "simple-message-summary")
		            {
                        IntPtr contentPtr = LinphoneAPI.linphone_content_get_string_buffer(bodyPtr);

                        if (contentPtr != IntPtr.Zero)
                        {
                            string content = Marshal.PtrToStringAnsi(contentPtr);
                            int messageCount = extractMessageCount(content);

                            if (OnMWIReceivedEvent != null)
                                OnMWIReceivedEvent(new MWIEventArgs(messageCount));
                            return;
                        }
                    }   
		        }
		    }

		    if (NotifyReceivedEvent != null)
				NotifyReceivedEvent(notified_event);
		}
		
        /// <summary>
        /// Method to find the number of waiting messages 
        /// </summary>
        /// <param name="messageSummaryBody"></param>
        /// <returns>int number of messages</returns>
        private int extractMessageCount(string messageSummaryBody)
        {
            int result;
            const string ValidPostiveNumberRegex = @"([0-9,.]+)";

            bool success = Int32.TryParse(Regex.Match(messageSummaryBody, ValidPostiveNumberRegex).Value, out result);

            if (!success)
            {
                result = 0;
            }

            return result;
        }

        private void OnInfoEventReceived(IntPtr lc, IntPtr callPtr, IntPtr msgPtr)
		{
            //if (linphoneCore == IntPtr.Zero)
            //    return;

            //if (msgPtr != IntPtr.Zero)
            //{
            //    lock (callLock)
            //    {
            //        VATRPCall call = FindCall(callPtr);

            //        if (call != null)
            //        {
            //            IntPtr valuePtr = LinphoneAPI.linphone_info_message_get_header(msgPtr, "action");
            //            if (valuePtr != IntPtr.Zero)
            //            {
            //                string val = Marshal.PtrToStringAnsi(valuePtr);
            //                if (val == "camera_mute_on" || val == "camera_mute_off"  || val == "isCameraMuted")
            //                    if (OnCameraMuteEvent != null)
            //                        OnCameraMuteEvent(new CameraMuteEventArgs(call, val == "camera_mute_off"));
            //            }
            //        }
            //    }
            //}
		}

        private void OnNetworkReachable(IntPtr lc, bool reachable)
        {
            if (NetworkReachableEvent != null)
                NetworkReachableEvent(reachable);
        }

        private void OnCallStatsUpdated(IntPtr lc, IntPtr callPtr, IntPtr statsPtr)
        {
            if (linphoneCore == IntPtr.Zero) return;

            lock (callLock)
            {
                VATRPCall call = FindCall(callPtr);
                if (call != null)
                {
                    if (CallStatisticsChangedEvent != null)
                        CallStatisticsChangedEvent(call);
                }
            }
        }
        private void OnIsComposingReceived(IntPtr lc, IntPtr chatPtr)
        {
            if (linphoneCore == IntPtr.Zero) return;

            IntPtr callPtr = IntPtr.Zero;

            lock (callLock)
            {
                foreach (var call in callsList)
                {
                    if (LinphoneAPI.linphone_call_get_chat_room(call.NativeCallPtr) == chatPtr)
                    {
                        callPtr = call.NativeCallPtr;
                        break;
                    }
                }
            }

            uint rttCode = 0;
            lock (messagingLock)
            {
                rttCode = LinphoneAPI.linphone_chat_room_get_char(chatPtr);

            }

            if (rttCode == 0)
                return;
            if (IsComposingReceivedEvent != null)
                IsComposingReceivedEvent(chatPtr, rttCode);

        }

        private void OnMessageReceived(IntPtr lc, IntPtr roomPtr, IntPtr message)
        {
            if (linphoneCore == IntPtr.Zero) return;

            var callChatRoomPtrList = new List<IntPtr>();

            if (LinphoneAPI.linphone_core_in_call(linphoneCore) != 0)
            {
                lock (callLock)
                {
                    callChatRoomPtrList.AddRange(callsList.Select(call => LinphoneAPI.linphone_call_get_chat_room(call.NativeCallPtr)));
                }
            }

            lock (messagingLock)
            {
                var from = string.Empty;
                var to = string.Empty;

                ////  Added 3/28 MITRE-fjr
                if (message != IntPtr.Zero)
                {
                    return;
                }

                IntPtr addressPtr = LinphoneAPI.linphone_chat_message_get_from_address(message);
                if (addressPtr != IntPtr.Zero)
                {
                    IntPtr addressStringPtr = LinphoneAPI.linphone_address_as_string(addressPtr);
                    if (addressStringPtr != IntPtr.Zero)
                    {
                        from = Marshal.PtrToStringAnsi(addressStringPtr);
                        LinphoneAPI.ortp_free(addressStringPtr);
                    }
                }

                addressPtr = LinphoneAPI.linphone_chat_message_get_to_address(message);
                if (addressPtr != IntPtr.Zero)
                {
                    IntPtr addressStringPtr = LinphoneAPI.linphone_address_as_string(addressPtr);
                    if (addressStringPtr != IntPtr.Zero)
                    {
                        to = Marshal.PtrToStringAnsi(addressStringPtr);
                        LinphoneAPI.ortp_free(addressStringPtr);
                    }
                }

                IntPtr msgPtr = LinphoneAPI.linphone_chat_message_get_text(message);
                var messageString = string.Empty;
                if (msgPtr != IntPtr.Zero)
                    messageString = Marshal.PtrToStringAnsi(msgPtr);

                var localTime = Time.ConvertUtcTimeToLocalTime(LinphoneAPI.linphone_chat_message_get_time(message));
                var chatMessage = new VATRPChatMessage(MessageContentType.Text)
                {
                    Direction = LinphoneAPI.linphone_chat_message_is_outgoing(message) == 1? MessageDirection.Outgoing : MessageDirection.Incoming,
                    IsIncompleteMessage = false,
                    MessageTime = localTime,
                    Content = messageString,
                    IsRTTMessage = false,
                    IsRead = LinphoneAPI.linphone_chat_message_is_read(message) == 1
                };

                if (OnChatMessageReceivedEvent != null)
                    OnChatMessageReceivedEvent(roomPtr, callChatRoomPtrList, from, chatMessage);
            }
        }

        private void OnMessageStatusChanged(IntPtr msgPtr, LinphoneChatMessageState state)
        {
            if (linphoneCore == IntPtr.Zero) return;

            lock (messagingLock)
            {
                if (OnChatMessageStatusChangedEvent != null)
                    OnChatMessageStatusChangedEvent(msgPtr, state);
            }
        }

        private void OnCallLogUpdated(IntPtr lc, IntPtr newcl)
        {
            if (OnLinphoneCallLogUpdatedEvent != null)
                OnLinphoneCallLogUpdatedEvent(lc, newcl);
        }

	    #endregion

        #region Info
        public IntPtr GetCallParams(IntPtr callPtr)
        {
            lock (callLock)
            {
                var call = FindCall(callPtr);
                if (call == null)
                    return IntPtr.Zero;
                return LinphoneAPI.linphone_call_get_current_params(call.NativeCallPtr);
            }
        }
        public string GetUsedAudioCodec(IntPtr callParams)
        {
            if (linphoneCore == IntPtr.Zero)
                return string.Empty;

            IntPtr payloadPtr = LinphoneAPI.linphone_call_params_get_used_audio_codec(callParams);
            if (payloadPtr != IntPtr.Zero)
            {
                var payload = (PayloadType)Marshal.PtrToStructure(payloadPtr, typeof(PayloadType));
                return payload.mime_type;
            }
            return string.Empty;
        }

        public string GetUsedVideoCodec(IntPtr callParams)
        {
            if (linphoneCore == IntPtr.Zero)
                return string.Empty;

            IntPtr payloadPtr = LinphoneAPI.linphone_call_params_get_used_video_codec(callParams);
            if (payloadPtr != IntPtr.Zero)
            {
                var payload = (PayloadType)Marshal.PtrToStructure(payloadPtr, typeof(PayloadType));
                return payload.mime_type;
            }
            return string.Empty;
        }

	    public MSVideoSizeDef GetVideoSize(IntPtr curparams, bool sending)
	    {
	        MSVideoSizeDef msVideoSize = sending
	            ? LinphoneAPI.linphone_call_params_get_sent_video_size(curparams)
	            : LinphoneAPI.linphone_call_params_get_received_video_size(curparams);

	        return msVideoSize;
	    }

	    public float GetFrameRate(IntPtr curparams, bool sending)
        {
            if (linphoneCore == IntPtr.Zero)
                return 0;

            return sending
                ? LinphoneAPI.linphone_call_params_get_sent_framerate(curparams)
                : LinphoneAPI.linphone_call_params_get_received_framerate(curparams);
        }

        public LinphoneMediaEncryption GetMediaEncryption(IntPtr curparams)
        {
            if (linphoneCore == IntPtr.Zero)
                return LinphoneMediaEncryption.LinphoneMediaEncryptionNone;

            return (LinphoneMediaEncryption)LinphoneAPI.linphone_call_params_get_media_encryption(curparams);
        }

	    public void GetCallAudioStats(IntPtr callPtr, ref LinphoneCallStats stat)
	    {
	        lock (callLock)
	        {
	            var call = FindCall(callPtr);

	            if (call != null)
	            {
	                IntPtr statsPtr = LinphoneAPI.linphone_call_get_audio_stats(call.NativeCallPtr);

	                if (statsPtr != IntPtr.Zero)
	                {
                        stat.download_bandwidth = LinphoneAPI.linphone_call_stats_get_download_bandwidth(statsPtr);
                        stat.upload_bandwidth = LinphoneAPI.linphone_call_stats_get_upload_bandwidth(statsPtr);
                        stat.ice_state = LinphoneAPI.linphone_call_stats_get_ice_state(statsPtr);
                        stat.upnp_state = LinphoneAPI.linphone_call_stats_get_upnp_state(statsPtr);
                        stat.total_late_packets = LinphoneAPI.linphone_call_stats_get_late_packets_cumulative_number(statsPtr, callPtr);
                        stat.rtp_stats = LinphoneAPI.linphone_call_stats_get_rtp_stats(statsPtr);
	                }
	            }
	        }
        }

        public void GetCallVideoStats(IntPtr callPtr, ref LinphoneCallStats stat)
        {
            lock (callLock)
            {
                var call = FindCall(callPtr);

                if (call != null)
                {
                    IntPtr statsPtr = LinphoneAPI.linphone_call_get_video_stats(call.NativeCallPtr);

                    if (statsPtr != IntPtr.Zero)
                    {
                        stat.download_bandwidth = LinphoneAPI.linphone_call_stats_get_download_bandwidth(statsPtr);
                        stat.upload_bandwidth = LinphoneAPI.linphone_call_stats_get_upload_bandwidth(statsPtr);
                        stat.ice_state = LinphoneAPI.linphone_call_stats_get_ice_state(statsPtr);
                        stat.upnp_state = LinphoneAPI.linphone_call_stats_get_upnp_state(statsPtr);
                        stat.total_late_packets = LinphoneAPI.linphone_call_stats_get_late_packets_cumulative_number(statsPtr, callPtr);
                        stat.rtp_stats = LinphoneAPI.linphone_call_stats_get_rtp_stats(statsPtr);
                    }
                }
            }
        }

        public void GetUsedPorts(out int sipPort, out int rtpPort)
        {
            sipPort = LinphoneAPI.linphone_core_get_sip_port(linphoneCore);
            rtpPort = LinphoneAPI.linphone_core_get_audio_port(linphoneCore);
        }

	    #endregion

        #region Chat History

        public int GetHistorySize(string username)
        {
            var address = string.Format("sip:{1}@{2}", username, preferences.ProxyHost);
            IntPtr friendAddressPtr = LinphoneAPI.linphone_core_create_address(linphoneCore, address);
            if (friendAddressPtr == IntPtr.Zero)
                return 0;

            IntPtr chatRoomPtr = LinphoneAPI.linphone_core_get_chat_room(linphoneCore, friendAddressPtr);
            if (chatRoomPtr == IntPtr.Zero)
                return 0 ;

            return LinphoneAPI.linphone_chat_room_get_history_size(chatRoomPtr);
        }

        public void LoadChatRoom(VATRPChat chat)
        {
            var address = string.Format("sip:{1}@{2}",  chat.Contact.ID, preferences.ProxyHost);
            IntPtr friendAddressPtr = LinphoneAPI.linphone_core_create_address(linphoneCore, address);
            if (friendAddressPtr == IntPtr.Zero)
                return;

            IntPtr chatRoomPtr = LinphoneAPI.linphone_core_get_chat_room(linphoneCore, friendAddressPtr);
            if (chatRoomPtr == IntPtr.Zero)
                return;
            IntPtr historyListPtr = LinphoneAPI.linphone_chat_room_get_history(chatRoomPtr, 100); // load all messages

            MSList curStruct;
            do
            {
                curStruct.next = IntPtr.Zero;
                curStruct.prev = IntPtr.Zero;
                curStruct.data = IntPtr.Zero;
                curStruct = (MSList)Marshal.PtrToStructure(historyListPtr, typeof(MSList));
                if (curStruct.data != IntPtr.Zero)
                {
                    IntPtr msgPtr = LinphoneAPI.linphone_chat_message_get_text(curStruct.data);
                    var messageString = string.Empty;
                    if (msgPtr != IntPtr.Zero)
                        messageString = Marshal.PtrToStringAnsi(msgPtr);

                    if (!string.IsNullOrEmpty(messageString) && messageString.Length > 1)
                    {
                        var localTime =
                            Time.ConvertUtcTimeToLocalTime(LinphoneAPI.linphone_chat_message_get_time(curStruct.data));

                        var chatMessage = new VATRPChatMessage(MessageContentType.Text)
                        {
                            Direction =
                                LinphoneAPI.linphone_chat_message_is_outgoing(curStruct.data) == 1
                                    ? MessageDirection.Outgoing
                                    : MessageDirection.Incoming,
                            IsIncompleteMessage = false,
                            MessageTime = localTime,
                            Content = messageString,
                            IsRead = LinphoneAPI.linphone_chat_message_is_read(curStruct.data) == 1,
                            IsRTTMessage = false,
                            IsRTTStartMarker = false,
                            IsRTTEndMarker = false,
                            Chat = chat
                        };
                        chat.Messages.Add(chatMessage);
                    }
                }
                historyListPtr = curStruct.next;
            } while (curStruct.next != IntPtr.Zero);
        }

        #endregion

        #region Subscriptions

        public bool SubscribeForVideoMWI(string newVideoMailUri)
        {
            if (linphoneCore != IntPtr.Zero)
            {
                try
                {
                    IntPtr mwiAddressPtr = LinphoneAPI.linphone_core_create_address(linphoneCore, newVideoMailUri);
                    if (_videoMWiSubscription != IntPtr.Zero)
                    {
                        LinphoneAPI.linphone_event_terminate(_videoMWiSubscription);
                        _videoMWiSubscription = IntPtr.Zero;
                    }

                    if (mwiAddressPtr != IntPtr.Zero)
                    {
                        _videoMWiSubscription = LinphoneAPI.linphone_core_subscribe(linphoneCore, mwiAddressPtr,
                            "message-summary", 1800, IntPtr.Zero);
                    }
                }
                catch (Exception ex)
                {
                    LOG.Debug($"Error subscribing to MWI -- {ex.ToString()}");
                }
            }

            return _videoMWiSubscription != IntPtr.Zero;
        }

        #endregion

        #region CardDAV

        public void RemoveCardDAVAuthInfo()
        {
            if (linphoneCore == IntPtr.Zero)
                return;
            carddav_auth = LinphoneAPI.linphone_core_find_auth_info(linphoneCore, LinphoneConfig.CardDavRealm, LinphoneConfig.CardDavUser, LinphoneConfig.CardDavDomain);
            if (carddav_auth != IntPtr.Zero)
            {
                LinphoneAPI.linphone_core_remove_auth_info(linphoneCore, carddav_auth);
            }
        }

        private string GetMd5Hash(string user, string password, string realm)
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(string.Format("{0}:{1}:{2}", user, realm, password));
            byte[] hash = md5.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("X2"));

            return sb.ToString().ToLower();
        }

        public void CardDAVSync()
        {
            if (linphoneCore == IntPtr.Zero || string.IsNullOrEmpty(LinphoneConfig.CardDavServer) )
                return;

            if (_cardDavSyncInProgress)
                return;

            LinphoneConfig.CardDavDomain = "localhost";
            LinphoneConfig.CardDavUser = "admin";
            LinphoneConfig.CardDavPass = "admin";
            LinphoneConfig.CardDavRealm = "SabreDAV";
            LinphoneConfig.CardDavServer = "http://localhost/groupwareserver.php/addressbooks/admin/default";
            carddav_auth = LinphoneAPI.linphone_core_find_auth_info(linphoneCore, LinphoneConfig.CardDavRealm, LinphoneConfig.CardDavUser, LinphoneConfig.CardDavDomain);
            if (carddav_auth != IntPtr.Zero)
            {
                LinphoneAPI.linphone_core_remove_auth_info(linphoneCore, carddav_auth);
            }

            //var hash = GetMd5Hash(LinphoneConfig.CardDavUser, LinphoneConfig.CardDavPass, LinphoneConfig.CardDavRealm);
            carddav_auth = LinphoneAPI.linphone_auth_info_new(LinphoneConfig.CardDavUser, null, LinphoneConfig.CardDavPass, null, LinphoneConfig.CardDavRealm,
                    LinphoneConfig.CardDavDomain);

            if (carddav_auth == IntPtr.Zero)
            {
                LOG.Debug("Failed to create cardDAV info");
                return;
            }
            LinphoneAPI.linphone_core_add_auth_info(linphoneCore, carddav_auth);
            //LinphoneAPI.linphone_friend_create_vcard()
            _cardDAVFriends = LinphoneAPI.linphone_core_get_default_friend_list(linphoneCore);
            if (_cardDAVFriends == IntPtr.Zero)
                return;
            _cardDavStats = new LinphoneCardDAVStats();

            if (_cardDavStatsPtr != IntPtr.Zero)
                Marshal.FreeHGlobal(_cardDavStatsPtr);

            _cardDavStatsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_cardDavStats));
            Marshal.StructureToPtr(_cardDavStats, _cardDavStatsPtr, false);

            LinphoneAPI.linphone_friend_list_set_uri(_cardDAVFriends, LinphoneConfig.CardDavServer);

            var cardDavCallbacksPtr = LinphoneAPI.linphone_friend_list_get_callbacks(_cardDAVFriends);

            if (cardDavCallbacksPtr != IntPtr.Zero)
            {
                LinphoneAPI.linphone_friend_list_cbs_set_user_data(cardDavCallbacksPtr, _cardDavStatsPtr);

                LinphoneAPI.linphone_friend_list_cbs_set_sync_status_changed(cardDavCallbacksPtr,
                    Marshal.GetFunctionPointerForDelegate(carddav_sync_done));
                LinphoneAPI.linphone_friend_list_cbs_set_contact_created(cardDavCallbacksPtr,
                    Marshal.GetFunctionPointerForDelegate(carddav_new_contact));
                LinphoneAPI.linphone_friend_list_cbs_set_contact_deleted(cardDavCallbacksPtr,
                    Marshal.GetFunctionPointerForDelegate(carddav_removed_contact));
                LinphoneAPI.linphone_friend_list_cbs_set_contact_updated(cardDavCallbacksPtr,
                    Marshal.GetFunctionPointerForDelegate(carddav_updated_contact));
            }
            _cardDavSyncInProgress = true;
            LinphoneAPI.linphone_friend_list_synchronize_friends_from_server(_cardDAVFriends);

        }

        private void OnCardDAVSyncChanged(IntPtr list, LinphoneFriendListSyncStatus status, IntPtr msgPtr)
        {
            var message = string.Empty;
            if (msgPtr != IntPtr.Zero)
                message = Marshal.PtrToStringAnsi(msgPtr);

            LOG.Info(string.Format("##OnSync {0} Msg: {1}. ", status, message));
            _cardDavSyncInProgress = status == LinphoneFriendListSyncStatus.LinphoneFriendListSyncStarted;
        }

        private void OnCardDAVContactUpdated(IntPtr list, IntPtr newFriend, IntPtr oldFriend)
        {
            var neweTag = "";
            IntPtr tmpPtr = LinphoneAPI.linphone_friend_get_ref_key(newFriend);
            if (tmpPtr != IntPtr.Zero)
            {
                neweTag = Marshal.PtrToStringAnsi(tmpPtr);
            }



            var oldeTag = "";
            tmpPtr = LinphoneAPI.linphone_friend_get_ref_key(oldFriend);
            if (tmpPtr != IntPtr.Zero)
            {
                neweTag = Marshal.PtrToStringAnsi(tmpPtr);
            }
            LOG.Info(string.Format("### OnCardDAVContactUpdated New eTag: {0} Old: ETag: {1}", neweTag, oldeTag));

            if (CardDAVContactUpdated != null)
            {
                var args = new CardDavContactEventArgs(CardDavContactEventArgs.CardDavAction.Update)
                {
                    FriendListPtr = list,
                    NewContactPtr = newFriend,
                    ChangedContactPtr = oldFriend
                };
                CardDAVContactUpdated(args);
            }
        }

        private void OnCardDAVContactRemoved(IntPtr list, IntPtr lf)
        {
            var refKey = "";
            IntPtr tmpPtr = LinphoneAPI.linphone_friend_get_ref_key(lf);
            if (tmpPtr != IntPtr.Zero)
            {
                refKey = Marshal.PtrToStringAnsi(tmpPtr);
            }

            tmpPtr = LinphoneAPI.linphone_friend_get_ref_key(lf);
            if (tmpPtr != IntPtr.Zero)
            {
                refKey = Marshal.PtrToStringAnsi(tmpPtr);
            }
            LOG.Info(string.Format( "### OnCardDAVContactRemoved RefKey: {0}", refKey));
            if (CardDAVContactDeleted != null)
            {
                var args = new CardDavContactEventArgs(CardDavContactEventArgs.CardDavAction.Delete)
                {
                    FriendListPtr = list,
                    ChangedContactPtr = lf
                };
                CardDAVContactDeleted(args);
            }
        }

        private void OnCardDAVContactCreated(IntPtr list, IntPtr lf)
        {
            var eTag = "";
            IntPtr tmpPtr = LinphoneAPI.linphone_friend_get_ref_key(lf);
            if (tmpPtr != IntPtr.Zero)
            {
                eTag = Marshal.PtrToStringAnsi(tmpPtr);
            }
            LOG.Info(string.Format("### OnCardDAVContactCreated eTag: {0}", eTag));
            if (CardDAVContactCreated != null)
            {
                var args = new CardDavContactEventArgs(CardDavContactEventArgs.CardDavAction.Create)
                {
                    FriendListPtr = list,
                    NewContactPtr = lf
                };
                CardDAVContactCreated(args);
            }
        }

        #endregion

        #region IVATRPInterface

        public event EventHandler<EventArgs> ServiceStarted;

        public event EventHandler<EventArgs> ServiceStopped;
        public bool Start()
        {
            return Start(true);
        }

        public bool Stop()
        {
            if (IsStarting || IsStopping)
                return false;

            if (IsStopped)
                return true;
            _isStopping = true;

            var cmd = new LinphoneCommand(LinphoneCommandType.TerminateAllCalls);
            lock (commandQueue)
            {
                commandQueue.Enqueue(cmd);
            }

            DoUnregister(); // cjm-aug17 this is done in unregister isnt it?!?

            cmd = new LinphoneCommand(LinphoneCommandType.StopLinphone);
            lock (commandQueue)
            {
                commandQueue.Enqueue(cmd);
            }
            return true;
        }
        #endregion

        #region Devices
        // VATRP-1200 TODO
        public List<VATRPDevice> GetAvailableCameras()
        {
            //linphone_core_get_video_devices
            List<VATRPDevice> cameraList = new List<VATRPDevice>();
            IntPtr videoDevicesPtr;
            if (linphoneCore != IntPtr.Zero)
            {
                videoDevicesPtr = LinphoneAPI.linphone_core_get_video_devices(linphoneCore);
            }
            else
            {
                return null;
            }
            if (videoDevicesPtr != IntPtr.Zero)
            {
                IntPtr current;
                var offset = 0;
                while ((current = Marshal.ReadIntPtr(videoDevicesPtr, offset)) != IntPtr.Zero)
                {
                    string device = LinphoneAPI.PtrToStringUtf8(current);
                    VATRPDevice newDevice = new VATRPDevice(device, VATRPDeviceType.CAMERA);
                    cameraList.Add(newDevice);
                    offset += IntPtr.Size;
                }
            }
            return cameraList;
        }

        public void SetCamera(string deviceId)
        {
            if (!string.IsNullOrEmpty(deviceId))
            {
                LinphoneAPI.linphone_core_set_video_device(linphoneCore, deviceId);
            }
        }

        public VATRPDevice GetSelectedCamera()
        {
            IntPtr deviceIdPtr = IntPtr.Zero;
            if (linphoneCore != IntPtr.Zero)
            {
                deviceIdPtr = LinphoneAPI.linphone_core_get_video_device(linphoneCore);
            }

            if (deviceIdPtr != IntPtr.Zero)
            {
                string deviceId = LinphoneAPI.PtrToStringUtf8(deviceIdPtr);
                VATRPDevice device = new VATRPDevice(deviceId, VATRPDeviceType.CAMERA);
                return device;
            }
            return null;
        }


        // VATRP-1200 TODO
        public List<VATRPDevice> GetAvailableMicrophones()
        {
            //linphone_core_get_sound_devices
            // filter with linphone_core_sound_device_can_capture
            if (linphoneCore == IntPtr.Zero)
            {
                return null;
            }
            List<VATRPDevice> microphoneList = new List<VATRPDevice>();
            IntPtr soundDevicesPtr = LinphoneAPI.linphone_core_get_sound_devices(linphoneCore);
            if (soundDevicesPtr != IntPtr.Zero)
            {
                IntPtr current;
                var offset = 0;
                while ((current = Marshal.ReadIntPtr(soundDevicesPtr, offset)) != IntPtr.Zero)
                {
                    string device = LinphoneAPI.PtrToStringUtf8(current);
                    if (LinphoneAPI.linphone_core_sound_device_can_capture(linphoneCore, device) == 1)
                    {
                        VATRPDevice newDevice = new VATRPDevice(device, VATRPDeviceType.MICROPHONE);
                        microphoneList.Add(newDevice);
                    }
                    offset += IntPtr.Size;
                }
            }
            return microphoneList;
        }

        public void SetCaptureDevice(string deviceId)
        {
            if (!string.IsNullOrEmpty(deviceId))
            {
                LinphoneAPI.linphone_core_set_capture_device(linphoneCore, deviceId);
            }
        }

        public VATRPDevice GetSelectedMicrophone()
        {
            if (linphoneCore == IntPtr.Zero)
            {
                return null;
            }
            IntPtr deviceIdPtr = LinphoneAPI.linphone_core_get_capture_device(linphoneCore);
            if (deviceIdPtr != IntPtr.Zero)
            {
                string deviceId = LinphoneAPI.PtrToStringUtf8(deviceIdPtr);
                VATRPDevice device = new VATRPDevice(deviceId, VATRPDeviceType.MICROPHONE);
                return device;
            }
            return null;
        }


        // VATRP-1200 TODO
        public List<VATRPDevice> GetAvailableSpeakers()
        {
            //linphone_core_get_sound_devices
            // filter with linphone_core_sound_device_can_playback
            if (linphoneCore == IntPtr.Zero)
            {
                return null;
            }
            List<VATRPDevice> speakerList = new List<VATRPDevice>();
            IntPtr soundDevicesPtr = LinphoneAPI.linphone_core_get_sound_devices(linphoneCore);
            if (soundDevicesPtr != IntPtr.Zero)
            {
                IntPtr current;
                var offset = 0;
                while ((current = Marshal.ReadIntPtr(soundDevicesPtr, offset)) != IntPtr.Zero)
                {
                    string device = LinphoneAPI.PtrToStringUtf8(current);
                    if (LinphoneAPI.linphone_core_sound_device_can_playback(linphoneCore, device) == 1)
                    {
                        VATRPDevice newDevice = new VATRPDevice(device, VATRPDeviceType.SPEAKER);
                        speakerList.Add(newDevice);
                    }
                    offset += IntPtr.Size;
                }
            }
            return speakerList;
        }

        public void SetSpeakers(string deviceId)
        {
            if (!string.IsNullOrEmpty(deviceId))
            {
                LinphoneAPI.linphone_core_set_playback_device(linphoneCore, deviceId);
            }
        }

        public VATRPDevice GetSelectedSpeakers()
        {
            if (linphoneCore == IntPtr.Zero)
            {
                return null;
            }
            IntPtr deviceIdPtr = LinphoneAPI.linphone_core_get_playback_device(linphoneCore);
            if (deviceIdPtr != IntPtr.Zero)
            {
                string deviceId = LinphoneAPI.PtrToStringUtf8(deviceIdPtr);
                VATRPDevice device = new VATRPDevice(deviceId, VATRPDeviceType.SPEAKER);
                return device;
            }
            return null;
        }

        #endregion

        #region Provide linphone settings for technical support sheet
        public string GetTechnicalSupportInfo()
        {
            StringBuilder configString = new StringBuilder();
            if ((linphoneCore != null) && IsStarted)
            {
                // items to add: enabled video codecs, enabled audio codecs, preferred video resolution, preferred bandwidth
                bool adaptiveRateEnabled = LinphoneAPI.linphone_core_adaptive_rate_control_enabled(linphoneCore) == 1;
                configString.AppendLine("Adaptive Rate Enabled: " + adaptiveRateEnabled.ToString());
                IntPtr strPtr = LinphoneAPI.linphone_core_get_adaptive_rate_algorithm(linphoneCore);
                var algorithm = string.Empty;
                if (strPtr != IntPtr.Zero)
                    algorithm = Marshal.PtrToStringAnsi(strPtr);
                configString.AppendLine("Adaptive Rate Algorithm: " + algorithm);
                int min_port = -1;
                int max_port = -1;
                LinphoneAPI.linphone_core_get_video_port_range(linphoneCore, ref min_port, ref max_port);
                configString.AppendLine("Video Port Range: " + min_port + "-" + max_port);
                LinphoneAPI.linphone_core_get_audio_port_range(linphoneCore, ref min_port, ref max_port);
                configString.AppendLine("Audio Port Range: " + min_port + "-" + max_port);

                configString.AppendLine("---Linphone Selected Devices---");
                VATRPDevice microphone = GetSelectedMicrophone();
                bool micEnabled = LinphoneAPI.linphone_core_mic_enabled(linphoneCore) == 1;
               // bool micMuted = IsCallMuted();
                if (microphone != null)
                {
                    configString.AppendLine("Microphone: " + microphone.displayName);
                    configString.AppendLine("    Id: " + microphone.deviceId);
                }
                else
                {
                    configString.AppendLine("Microphone: name is null");
                }
                configString.AppendLine("    Enabled:" + micEnabled.ToString());
                configString.AppendLine("    Muted:" + IsCallMuted());
                int recordingLevel = LinphoneAPI.linphone_core_get_rec_level(linphoneCore);
                configString.AppendLine("    Mic Recording Level: " + recordingLevel.ToString());
                float micGain = LinphoneAPI.linphone_core_get_mic_gain_db(linphoneCore);
                configString.AppendLine("    Mic Gain: " + micGain.ToString());
                VATRPDevice speaker = GetSelectedSpeakers();
                if (speaker != null)
                {
                    configString.AppendLine("Speaker: " + speaker.displayName);
                    configString.AppendLine("    Id: " + speaker.deviceId);
                }
                else
                {
                    configString.AppendLine("Speaker: name is null");
                }
                configString.AppendLine("    Muted:" + IsSpeakerMuted());

                VATRPDevice camera = GetSelectedCamera();
                if (camera != null)
                {
                    configString.AppendLine("Camera: " + camera.displayName);
                    configString.AppendLine("    Id: " + camera.deviceId);
                }
                else
                {
                    configString.AppendLine("Camera: name is null");
                }

            }
            return configString.ToString();
        }

        public string GetTechnicalSupportInfo(IntPtr callPtr)
        {
            StringBuilder configString = new StringBuilder();
            if (callPtr != IntPtr.Zero)
            {


            }
            return configString.ToString();
        }
        #endregion


        public void RemoveDBPassword()
        {
            try
            {
                _contactsPath = manager.BuildDataPath("contacts.db");
                //string conn = @"Data Source=database.s3db;Password=Mypass;";
                string conn = string.Format("data source={0}", _contactsPath + ";Password=1234;");
                SQLiteConnection connection = new SQLiteConnection(conn);
                connection.Open();
                //Some code
                connection.ChangePassword(""); //connection.ChangePassword("");
                connection.Close();

                //UpdatePrivateDataPath();
            }
            //if it is the first time sets the password in the database
            catch
            {
                _contactsPath = manager.BuildDataPath("contacts.db");
                string conn = string.Format("data source={0}", _contactsPath + ";");
                SQLiteConnection connection = new SQLiteConnection(conn);
                connection.Open();
                //Some code
                connection.ChangePassword("");
                connection.Close();

                //UpdatePrivateDataPath();
            }
        }
        public void AddDBPassword()
        {
            //if the database has already password
            try
            {
                _contactsPath = manager.BuildDataPath("contacts.db");
                //string conn = @"Data Source=database.s3db;Password=Mypass;";
                string conn = string.Format("data source={0}", _contactsPath + ";Password=1234;");
                SQLiteConnection connection = new SQLiteConnection(conn);
                connection.Open();
                //Some code
                connection.ChangePassword("1234"); //connection.ChangePassword("");
                connection.Close();
            }
            //if it is the first time sets the password in the database
            catch
            {
                _contactsPath = manager.BuildDataPath("contacts.db");
                string conn = string.Format("data source={0}", _contactsPath + ";");
                SQLiteConnection connection = new SQLiteConnection(conn);
                connection.Open();
                //Some code
                connection.ChangePassword("1234");
                connection.Close();
            }
        }

        public void AddCallHistoryDBPassword()
        {
            try
            {
                _contactsPath = manager.BuildDataPath("callhistory.db");
                string conn = string.Format("data source={0}", _contactsPath + ";Password=1234;");
                SQLiteConnection connection = new SQLiteConnection(conn);
                connection.Open();
                connection.ChangePassword("1234");
                connection.Close();
            }
            //if it is the first time sets the password in the database
            catch
            {
                _contactsPath = manager.BuildDataPath("callhistory.db");
                string conn = string.Format("data source={0}", _contactsPath + ";");
                SQLiteConnection connection = new SQLiteConnection(conn);
                connection.Open();
                //Some code
                connection.ChangePassword("1234");
                connection.Close();
            }
        }
        public void RemoveCallHistoryDBPassword()
        {
            try
            {
                _contactsPath = manager.BuildDataPath("callhistory.db");
                string conn = string.Format("data source={0}", _contactsPath + ";Password=1234;");
                SQLiteConnection connection = new SQLiteConnection(conn);
                connection.Open();
                //Some code
                connection.ChangePassword("");
                connection.Close();
            }
            //if it is the first time sets the password in the database
            catch
            {
                _contactsPath = manager.BuildDataPath("callhistory.db");
                string conn = string.Format("data source={0}", _contactsPath + ";");
                SQLiteConnection connection = new SQLiteConnection(conn);
                connection.Open();
                //Some code
                connection.ChangePassword("");
                connection.Close();
            }
        }
        public void AddChatHistoryDBPassword()
        {
            try
            {
                _contactsPath = manager.BuildDataPath("chathistory.db");
                string conn = string.Format("data source={0}", _contactsPath + ";Password=1234;");
                SQLiteConnection connection = new SQLiteConnection(conn);
                connection.Open();
                connection.ChangePassword("1234");
                connection.Close();
            }
            //if it is the first time sets the password in the database
            catch
            {
                _contactsPath = manager.BuildDataPath("chathistory.db");
                string conn = string.Format("data source={0}", _contactsPath + ";");
                SQLiteConnection connection = new SQLiteConnection(conn);
                connection.Open();
                //Some code
                connection.ChangePassword("1234");
                connection.Close();
            }
        }
        public void RemoveChatHistoryDBPassword()
        {
            try
            {
                _contactsPath = manager.BuildDataPath("chathistory.db");
                string conn = string.Format("data source={0}", _contactsPath + ";Password=1234;");
                SQLiteConnection connection = new SQLiteConnection(conn);
                connection.Open();
                //Some code
                connection.ChangePassword("");
                connection.Close();
            }
            //if it is the first time sets the password in the database
            catch
            {
                _contactsPath = manager.BuildDataPath("chathistory.db");
                string conn = string.Format("data source={0}", _contactsPath + ";");
                SQLiteConnection connection = new SQLiteConnection(conn);
                connection.Open();
                //Some code
                connection.ChangePassword("");
                connection.Close();
            }
        }

    }
}
