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
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using System.Windows.Threading;
using com.vtcsecure.ace.windows.CustomControls;
using com.vtcsecure.ace.windows.Views;
using System.IO;
using System.Windows.Media.Imaging;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class CallViewModel : ViewModelBase, IEquatable<CallViewModel>, IEquatable<VATRPCall>
    {
        private bool _visualizeRing;
        private bool _visualizeIncoming;
        private int _ringCount;
        private bool _showInfo;
        private VATRPCallState _callState;
        private string _displayName;
        private string _remoteNumber;
        private int _callDuration;
        private int _autoAnswer;
        private ILinphoneService _linphoneService;
        private bool _hasVideo;
        private double _displayNameSize;
        private double _remotePartyTextSize;
        private double _infoTextSize;
        private VATRPCall _currentCall = null;
        private readonly System.Timers.Timer ringTimer;
        private System.Timers.Timer serviceTimer;
        private bool subscribedForStats;
        private System.Timers.Timer timerCall;
        private CallInfoViewModel _callInfoViewModel;
        private InCallMessagingViewModel _rttViewModel;
        private bool _showIncomingCallPanel;
        private bool _showOutgoingCallPanel;
        private SolidColorBrush _ringCounterBrush;
        private bool _isVideoOn;
        private bool _isMuteOn;
        private bool _isSpeakerOn;
        private bool _isNumpadOn;
        private bool _isRttOn;
        private bool _isRttEnabled;
        private bool _isInfoOn;
        private bool _isCallOnHold;
        private int _videoWidth;
        private int _videoHeight;

        private bool _savedIsVideoOn;
        private bool _savedIsMuteOn;
        private bool _savedIsSpeakerOn;
        private bool _savedIsNumpadOn;
        private bool _savedIsRttOn;
        private bool _savedIsInfoOn;
        private bool _savedIsCallHoldOn;
        private string _errorMessage;
        private ImageSource _avatar;
        public bool PauseRequest;
        public bool ResumeRequest;
        public bool AllowHideContorls;
        public Visibility CommandbarLastTimeVisibility;
        public Visibility NumpadLastTimeVisibility;
        public Visibility CallInfoLastTimeVisibility;
        public Visibility CallSwitchLastTimeVisibility;
        private bool _isFullScreenOn;
        private bool _showAvatar;
        private bool _showDeclineMenu;
        private string _declinedMessage;
        private string _declinedMessageHeader;
        private bool _showRingingTimer;
        private bool _showDeclinedMessage;
        private VATRPContact _contact;
        private object serviceLock = new object();
        private object timerCallLock = new object();
        public event CallInfoViewModel.CallQualityChangedDelegate CallQualityChangedEvent;
        public event EventHandler CallConnectingTimeout;
        public event EventHandler HideMessageWindowTimeout;
        private bool _showInfoMsg;
	
        public CallViewModel()
        {

            //*********************************************************************************************************************************************
            // Initilize of Call Screen For Incoming and Outgoing calls.
            //*********************************************************************************************************************************************
            _visualizeRing = false;
            _visualizeIncoming = false;
            _declinedMessage = string.Empty;
            _showDeclineMenu = false;
            _showRingingTimer = true;
            _showInfoMsg = false;
            _callState = VATRPCallState.None;
            _hasVideo = true;
            _displayNameSize = 30;
            _remotePartyTextSize = 25;
            _infoTextSize = 20;
            subscribedForStats = false;
            Declined = false;
            _errorMessage = String.Empty;
            AllowHideContorls = false;
            WaitForDeclineMessage = false;
            CommandbarLastTimeVisibility = Visibility.Hidden;
            NumpadLastTimeVisibility = Visibility.Hidden;
            CallInfoLastTimeVisibility = Visibility.Hidden;
            CallSwitchLastTimeVisibility = Visibility.Hidden;

            // initialize based on stored settings:
            if (App.CurrentAccount != null)
            {
                _savedIsVideoOn = App.CurrentAccount.ShowSelfView;
                _isVideoOn = App.CurrentAccount.ShowSelfView;
                _savedIsMuteOn = App.CurrentAccount.MuteMicrophone;
                _isMuteOn = App.CurrentAccount.MuteMicrophone;
                _isSpeakerOn = App.CurrentAccount.MuteSpeaker;
                _savedIsSpeakerOn = App.CurrentAccount.MuteSpeaker;
            }

            timerCall = new System.Timers.Timer
            {
                Interval = 1000,
                AutoReset = true
            };
            timerCall.Elapsed += OnUpdatecallTimer;

            ringTimer = new System.Timers.Timer
            {
                Interval = 1800,
                AutoReset = true
            };
            ringTimer.Elapsed += OnUpdateRingCounter;

            _callInfoViewModel = new CallInfoViewModel();
            _rttViewModel = new InCallMessagingViewModel(ServiceManager.Instance.ChatService, ServiceManager.Instance.ContactService);
        }

        public CallViewModel(ILinphoneService linphoneSvc, VATRPCall call) : this()
        {

            //**********************************************************************************************************************************************
            // Set the call view for Incoming and Outgoing calls Like Video, Speaker, Microphone etc
            //**********************************************************************************************************************************************
            _linphoneService = linphoneSvc;
            _currentCall = call;
            _currentCall.CallStartTime = DateTime.Now;
            _currentCall.CallEstablishTime = DateTime.MinValue;
            // initialize based on stored settings:
            if (App.CurrentAccount != null)
            {
                _savedIsVideoOn = App.CurrentAccount.ShowSelfView;
                _isVideoOn = App.CurrentAccount.ShowSelfView;
                _savedIsMuteOn = App.CurrentAccount.MuteMicrophone;
                _isMuteOn = App.CurrentAccount.MuteMicrophone;
                _isSpeakerOn = App.CurrentAccount.MuteSpeaker;
                _savedIsSpeakerOn = App.CurrentAccount.MuteSpeaker;
            }


        }

        #region Properties

        public bool Declined { get; set; }

        public bool VisualizeIncoming
        {
            get { return _visualizeIncoming; }
            set
            {
                _visualizeIncoming = value;
                OnPropertyChanged("VisualizeIncoming");
            }
        }

        public bool VisualizeRinging
        {
            get { return _visualizeRing; }
            set
            {
                _visualizeRing = value;
                OnPropertyChanged("VisualizeRinging");
            }
        }

        public SolidColorBrush RingCounterBrush
        {
            get { return _ringCounterBrush; }
            set
            {
                _ringCounterBrush = value;
                OnPropertyChanged("RingCounterBrush");
            }
        }

        public int RingCount
        {
            get { return _ringCount; }
            set
            {
                _ringCount = value;
                OnPropertyChanged("RingCount");
            }
        }

        public bool ShowInfo
        {
            get { return _showInfo; }
            set
            {
                if (_showInfo != value)
                {
                    _showInfo = value;
                    OnPropertyChanged("ShowInfo");
                }
            }
        }

        public bool ShowAvatar
        {
            get { return _showAvatar; }
            set
            {
                _showAvatar = value;
                OnPropertyChanged("ShowAvatar");
            }
        }

        public VATRPCall ActiveCall
        {
            get { return _currentCall; }

            set { _currentCall = value; }
        }

        public VATRPCallState CallState
        {
            get { return _callState; }

            set
            {
                _callState = value;
                OnPropertyChanged("CallState");
            }
        }

        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                _displayName = value;
                OnPropertyChanged("DisplayName");
                OnPropertyChanged("CallerInfo");
            }
        }

        public string RemoteNumber
        {
            get { return _remoteNumber; }
            set
            {
                _remoteNumber = value;
                OnPropertyChanged("RemoteNumber");
                OnPropertyChanged("CallerInfo");
            }
        }

        public string CallerInfo
        {
            get
            {
                if (string.IsNullOrEmpty(DisplayName))
                    return RemoteNumber;
                return DisplayName ?? string.Empty;
            }
        }

        public int CallDuration
        {
            get
            {
                if (_currentCall.CallEstablishTime == DateTime.MinValue)
                    return 0;
                return (int)(DateTime.Now - _currentCall.CallEstablishTime).TotalSeconds;
            }
        }
        public int RingDuration
        {
            get { return (int)(DateTime.Now - _currentCall.CallStartTime).TotalSeconds; }
        }

        public bool ShowRingingTimer
        {
            get { return _showRingingTimer; }
            set
            {
                _showRingingTimer = value; 
                OnPropertyChanged("ShowRingingTimer");
            }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged("ErrorMessage");
            }
        }

        public int AutoAnswer
        {

            //*************************************************************************************************************
            // Set/Get the Auto Answer value
            //**************************************************************************************************************
            get { return _autoAnswer; }
            set
            {
                _autoAnswer = value;
                OnPropertyChanged("AutoAnswer");
            }
        }

        public bool HasVideo
        {
            get { return _hasVideo; }
            set
            {
                _hasVideo = value;
                OnPropertyChanged("HasVideo");
            }
        }

        public bool ShowIncomingCallPanel
        {
            get { return _showIncomingCallPanel; }
            set
            {
                _showIncomingCallPanel = value;
                OnPropertyChanged("ShowIncomingCallPanel");
                OnPropertyChanged("ShowCallParams");
            }
        }

        public bool ShowCallParams
        {
            get { return ShowIncomingCallPanel || ShowOutgoingEndCall; }
        }

        public bool ShowOutgoingEndCall
        {
            get { return !ShowInfo && _showOutgoingCallPanel; }
            set
            {
                _showOutgoingCallPanel = value;
                OnPropertyChanged("ShowOutgoingEndCall");
                OnPropertyChanged("ShowCallParams");
            }
        }

        public bool IsVideoOn
        {
            get { return _isVideoOn; }
            set
            {
                _isVideoOn = value;
                OnPropertyChanged("IsVideoOn");
            }
        }

        public bool IsMuteOn
        {
            get { return _isMuteOn; }
            set
            {
                _isMuteOn = value;
                OnPropertyChanged("IsMuteOn");
            }
        }

        public bool IsSpeakerOn
        {
            get { return _isSpeakerOn; }
            set
            {
                _isSpeakerOn = value;
                OnPropertyChanged("IsSpeakerOn");
            }
        }

        public bool IsNumpadOn
        {
            get { return _isNumpadOn; }
            set
            {
                _isNumpadOn = value;
                OnPropertyChanged("IsNumpadOn");
            }
        }

        public bool IsRttOn
        {
            get { return _isRttOn; }
            set
            {
                _isRttOn = value;
                OnPropertyChanged("IsRttOn");
            }
        }
		
        public bool IsRTTEnabled
        {   // CJM: RTT
            get { return _isRttEnabled; }
            set
            {
                _isRttEnabled = value;
                OnPropertyChanged("IsRTTEnabled");
            }
        }
		
        public bool IsCallInfoOn
        {
            get { return _isInfoOn; }
            set
            {
                _isInfoOn = value;
                OnPropertyChanged("IsCallInfoOn");
            }
        }

        public bool IsFullScreenOn
        {
            get { return _isFullScreenOn; }
            set
            {
                _isFullScreenOn = value;
                OnPropertyChanged("IsFullScreenOn");
            }
        }
		
        public bool IsCallOnHold
        {
            get { return _isCallOnHold; }
            set
            {
                _isCallOnHold = value;
                OnPropertyChanged("IsCallOnHold");
            }
        }
        public double DisplayNameSize
        {
            get { return _displayNameSize; }
            set
            {
                _displayNameSize = value;
                OnPropertyChanged("DisplayNameSize");
            }
        }

        public double RemotePartyTextSize
        {
            get { return _remotePartyTextSize; }
            set
            {
                _remotePartyTextSize = value;
                OnPropertyChanged("RemotePartyTextSize");
            }
        }

        public double InfoTextSize
        {
            get { return _infoTextSize; }
            set
            {
                _infoTextSize = value;
                OnPropertyChanged("InfoTextSize");
            }
        }

        public int VideoWidth
        {
            get { return _videoWidth; }
            set
            {
                _videoWidth = value;
                OnPropertyChanged("VideoWidth");
            }
        }

        public int VideoHeight
        {
            get { return _videoHeight; }
            set
            {
                _videoHeight = value;
                OnPropertyChanged("VideoHeight");
            }
        }

        public CallInfoViewModel CallInfoModel
        {
            get { return _callInfoViewModel; }
        }

        public InCallMessagingViewModel RTTViewModel
        {
            get { return _rttViewModel; }
        }

        public CallInfoView CallInfoCtrl { get; set; }

        public bool SavedIsVideoOn
        {
            get { return _savedIsVideoOn; }
            set { _savedIsVideoOn = value; }
        }

        public bool SavedIsMuteOn
        {
            get { return _savedIsMuteOn; }
            set { _savedIsMuteOn = value; }
        }

        public bool SavedIsSpeakerOn
        {
            get { return _savedIsSpeakerOn; }
            set { _savedIsSpeakerOn = value; }
        }

        public bool SavedIsNumpadOn
        {
            get { return _savedIsNumpadOn; }
            set { _savedIsNumpadOn = value; }
        }

        public bool SavedIsRttOn
        {
            get { return _savedIsRttOn; }
            set { _savedIsRttOn = value; }
        }

        public bool SavedIsInfoOn
        {
            get { return _savedIsInfoOn; }
            set { _savedIsInfoOn = value; }
        }

        public bool SavedIsCallHoldOn
        {
            get { return _savedIsCallHoldOn; }
            set { _savedIsCallHoldOn = value; }
        }

        public ImageSource Avatar
        {
            get { return _avatar; }
            set
            {
                _avatar = value;
                OnPropertyChanged("Avatar");
            }
        }
        public bool ShowDeclineMenu
        {
            get { return _showDeclineMenu; }
            set
            {
                _showDeclineMenu = value;
                OnPropertyChanged("ShowDeclineMenu");
            }
        }

        public bool ShowDeclinedMessage
        {
            get { return _showDeclinedMessage && !ShowInfoMessage; }
            set
            {
                _showDeclinedMessage = value;
                OnPropertyChanged("ShowDeclinedMessage");
            }
        }

        public string DeclinedMessage
        {
            get { return _declinedMessage; }
            set
            {
                if (_declinedMessage != value)
                {
                    _declinedMessage = value;
                    OnPropertyChanged("DeclinedMessage");
                }
            }
        }

        public string DeclinedMessageHeader
        {
            get { return _declinedMessageHeader; }
            set
            {
                if (_declinedMessageHeader != value)
                {
                    _declinedMessageHeader = value;
                    OnPropertyChanged("DeclinedMessageHeader");
                }
            }
        }

        public bool ShowInfoMessage
        {
            get { return _showInfoMsg; }
            set
            {
                _showInfoMsg = value;
                OnPropertyChanged("ShowInfoMessage");
                OnPropertyChanged("ShowDeclinedMessage");
            }
        }

        public VATRPContact Contact
        {
            get { return _contact; }
            set
            {
                _contact = value;
                OnPropertyChanged("Contact");
            }
        }

        public bool WaitForDeclineMessage { get; set; }

        #endregion

        #region Methods

        private void OnUpdatecallTimer(object sender, ElapsedEventArgs e)
        {

            //******************************************************************************************************************************************
            // Update call time, Incoming or Outgoing buth timer.
            //******************************************************************************************************************************************
            if (ServiceManager.Instance.Dispatcher != null)
            {
                if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
                {
                    ServiceManager.Instance.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        new EventHandler<ElapsedEventArgs>(OnUpdatecallTimer), sender, new object[] {e});
                    return;
                }
                OnPropertyChanged("RingDuration");
                lock (timerCallLock)
                {
                    if (timerCall != null)
                    {
                        timerCall.Start();
                    }
                }
            }
        }

        private void OnUpdateRingCounter(object sender, ElapsedEventArgs e)
        {
            //*****************************************************************************************************************************
            // Update Ringer count that display on the Ringer/Incoming/Outgoing call screen.
            //*****************************************************************************************************************************
            if (ServiceManager.Instance.Dispatcher != null)
            {
                if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
                {
                    ServiceManager.Instance.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        new EventHandler<ElapsedEventArgs>(OnUpdateRingCounter), sender, new object[] {e});
                    return;
                }
                RingCount++;
                ringTimer.Start();
            }
        }

        private void OnServiceTimer(object sender, ElapsedEventArgs e)
        {

            //********************************************************************************************************************************************************
            // Call timer update
            //********************************************************************************************************************************************************
            if (ServiceManager.Instance.Dispatcher != null)
            {
                if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
                {
                    ServiceManager.Instance.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        new EventHandler<ElapsedEventArgs>(OnServiceTimer), sender, new object[] { e });
                    return;
                }

                if (CallState == VATRPCallState.Trying)
                {
                    DestroyServiceTimer();
                    if (CallConnectingTimeout != null)
                        CallConnectingTimeout(this, EventArgs.Empty);
                }
                else if (AutoAnswer > 0)
                {
                    //********************************************************************************************************************************************
                    // When AutoAnswer is yes then 
                    //********************************************************************************************************************************************
                    AutoAnswer--;

                    if (AutoAnswer == 0)
                    {
                        DestroyServiceTimer();

                        bool muteMicrophone = false;
                        bool muteSpeaker = false;
                        //bool enableVideo = true;
                        if (App.CurrentAccount != null)
                        {
                            muteMicrophone = App.CurrentAccount.MuteMicrophone;
                            muteSpeaker = App.CurrentAccount.MuteSpeaker;
                            //enableVideo = App.CurrentAccount.EnableVideo;
                        }
                        //Hide();
                        _linphoneService.AcceptCall(_currentCall.NativeCallPtr,
                            ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                                Configuration.ConfEntry.USE_RTT, true), muteMicrophone, muteSpeaker, true);
                    }
                }
            }
        }

        private void DestroyServiceTimer()
        {
            lock (serviceLock)
            {
                if (serviceTimer != null)
                {
                    serviceTimer.Stop();
                    serviceTimer.Dispose();
                    serviceTimer = null;
                }
            }
        }

        private void StopAnimation()
        {
            VisualizeRinging = false;
            VisualizeIncoming = false;
            if (ringTimer.Enabled)
                ringTimer.Stop();
        }

        internal void TerminateCall()
        {
            if (_currentCall != null)
                _linphoneService.TerminateCall(_currentCall.NativeCallPtr, "Call ended");
        }

        internal void MuteSpeaker(bool isMuted)
        {

            //************************************************************************************************************************************
            // When Mute speaker is selected over Call Window.
            //************************************************************************************************************************************
            _linphoneService.MuteSpeaker(isMuted);
            //            _linphoneService.ToggleMute();
            //            IsMuteOn = _linphoneService.IsCallMuted();

            // TM. VATRP-2219, in-call selections should not update the app settings choices
            //if (App.CurrentAccount != null)
            //{
            //    App.CurrentAccount.MuteSpeaker = isMuted;
            //    // this now needs to be able to update the unified settings control as well.
            //    ServiceManager.Instance.SaveAccountSettings();
            //}
        }

        internal bool MuteCall(bool isMuted)
        {
            //**********************************************************************************************************************************************
            // When buton is pressed on Keypad over Call window.
            //**********************************************************************************************************************************************

            if (ActiveCall != null && (ActiveCall.CallState != VATRPCallState.LocalPaused &&
                                       ActiveCall.CallState != VATRPCallState.LocalPausing))
            {
                _linphoneService.MuteCall(isMuted);
                return true;
            }
            return false;
//            _linphoneService.ToggleMute();
//            IsMuteOn = _linphoneService.IsCallMuted();

            // TM. VATRP-2219, in-call selections should not update the app settings choices
            //if (App.CurrentAccount != null)
            //{
            //    App.CurrentAccount.MuteMicrophone = isMuted;
            //    // this now needs to be able to update the unified settings control as well.
            //    ServiceManager.Instance.SaveAccountSettings();
            //}
        }

        internal void ToggleCallStatisticsInfo(bool bShow)
        {

            //************************************************************************************************************************************
            // Toggle for Call Statistics, Show or Hide Call Statistics.
            //************************************************************************************************************************************
            if (CallInfoCtrl != null)
            {
                if (!bShow)
                    CallInfoCtrl.Hide();
                else
                    CallInfoCtrl.Show();
            }
        }

        internal void LoadAvatar(string path)
        {
            if (string.IsNullOrEmpty(path) || _avatar != null) 
                return;
            try
            {
                byte[] data = File.ReadAllBytes(path);
                var source = new BitmapImage();
                source.BeginInit();
                source.StreamSource = new MemoryStream(data);
                source.EndInit();

                Avatar = source;
                // use public setter
            }
            catch (Exception ex)
            {
                    
            }
        }

        #endregion

        #region Events


        internal void OnTrying()
        {
            //********************************************************************************************************************************
            // Trying for Outgoing call.
            //********************************************************************************************************************************
            DisplayName = _currentCall.To.DisplayName;
            RemoteNumber = _currentCall.To.Username;
            AutoAnswer = 0;
            ShowIncomingCallPanel = false;
            ShowRingingTimer = false;
            ShowOutgoingEndCall = true;
            ShowAvatar = true;
            CallState = VATRPCallState.Trying;
            lock (serviceLock)
            {
                if (serviceTimer == null)
                {
                    serviceTimer = new System.Timers.Timer
                    {
                        Interval = 5000,
                        AutoReset = false
                    };
                    serviceTimer.Elapsed += OnServiceTimer;
                    serviceTimer.Start();
                }
            }
        }

        internal void OnRinging()
        {
            //*****************************************************************************************************************************************
            // When Ringing for Outgoing calls.
            //*****************************************************************************************************************************************
            DisplayName = _currentCall.To.DisplayName;
            RemoteNumber = _currentCall.To.Username;
            ShowIncomingCallPanel = false;
            ShowOutgoingEndCall = true;
            AutoAnswer = 0;
            ShowAvatar = true;
            VisualizeIncoming = false;
            DestroyServiceTimer();
            lock (timerCallLock)
            {
                if (timerCall != null)
                {
                    if (!timerCall.Enabled)
                        timerCall.Start();
                }
            }

            CallState = VATRPCallState.Ringing;
            ShowRingingTimer = true;
            if (!VisualizeRinging)
            {
                RingCounterBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xD8, 0x1C, 0x1C));
                VisualizeRinging = true;
                RingCount = 1;
                if (ringTimer != null)
                {
                    if (ringTimer.Enabled)
                        ringTimer.Stop();
                    ringTimer.Interval = 4000;
                    ringTimer.Start();
                }
            }
        }

        internal void ToggleVideo(bool videoOn)
        {
            //***********************************************************************************************************************************************
            // Toggle Video, Enable/Disaable the video
            //***********************************************************************************************************************************************
            _linphoneService.ToggleVideo(videoOn, _currentCall.NativeCallPtr);
        }

        internal void OnIncomingCall()
        {
            //*****************************************************************************************************************************************************
            //When incoming a call, set the display name, username etc.
            //*****************************************************************************************************************************************************
            AutoAnswer = 0;
            DisplayName = _currentCall.From.DisplayName;
            RemoteNumber = _currentCall.From.Username;
            ShowOutgoingEndCall = false;
            CallState = VATRPCallState.InProgress;
            ShowAvatar = true;
            ShowDeclineMenu = false;
//#if DEBUG
            bool isUserAgent = false;
            if (App.CurrentAccount != null)
            {
                isUserAgent = App.CurrentAccount.UserNeedsAgentView;
            }
            if ((ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AUTO_ANSWER, false) || isUserAgent) &&
                (ServiceManager.Instance.LinphoneService.GetActiveCallsCount == 1))
            {


                var autoAnswerDuration =
                    ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                        Configuration.ConfEntry.AUTO_ANSWER_AFTER, 2);
                if (autoAnswerDuration > 60)
                    autoAnswerDuration = 60;
                else if (autoAnswerDuration < 0)
                    autoAnswerDuration = 0;

                AutoAnswer = autoAnswerDuration;
                if (autoAnswerDuration > 0)
                    lock (serviceLock)
                    {
                        if (serviceTimer == null)
                        {
                            serviceTimer = new System.Timers.Timer
                            {
                                Interval = 1000,
                                AutoReset = true
                            };
                            serviceTimer.Elapsed += OnServiceTimer;
                        }
                        serviceTimer.Start();
                    }
            }
//#endif
            lock (timerCallLock)
            {
                if (timerCall != null)
                {
                    if (!timerCall.Enabled)
                        timerCall.Start();
                }
            }

            VisualizeIncoming = true;
            if (!VisualizeRinging)
            {
                RingCounterBrush = new SolidColorBrush(Colors.White);
                VisualizeRinging = true;
                RingCount = 1;
                if (ringTimer != null)
                {
                    if (ringTimer.Enabled)
                        ringTimer.Stop();
                    ringTimer.Interval = 1800;
                    ringTimer.Start();
                }
            }
        }

        internal void OnEarlyMedia()
        {

        }

        internal void OnConnected()
        {
            DestroyServiceTimer();

            lock (timerCallLock)
            {
                if (timerCall != null)
                {
                    timerCall.Stop();
                    timerCall.Dispose();
                }
                timerCall = null;
            }
            AllowHideContorls = true;
            StopAnimation();

            if (_currentCall.CallEstablishTime == DateTime.MinValue)
                _currentCall.CallEstablishTime = DateTime.Now;

            CallState = VATRPCallState.Connected;
            ShowIncomingCallPanel = false;
            ShowDeclineMenu = false;
            IsMuteOn = _linphoneService.IsCallMuted();
            ShowInfo = true;
            ShowAvatar = false;
        }

        internal void OnStreamRunning()
        {
            CallState = VATRPCallState.StreamsRunning;
            AllowHideContorls = true;
            SubscribeCallStatistics();
        }

        internal void OnResumed()
        {
            CallState = VATRPCallState.LocalResumed;
        }

        internal void OnRemotePaused()
        {
            CallState = VATRPCallState.RemotePaused;
        }

        internal void OnLocalPaused()
        {
            CallState = VATRPCallState.LocalPaused;
        }

        internal void OnClosed(ref bool isError, string errorMessage, int errorCode, bool isDeclined)
        {

            //*************************************************************************************************************************************
            // On disconnect of call. Closing Call window. Call may be disconnected from both side. On both cases this method will called.
            //*************************************************************************************************************************************
            DestroyServiceTimer();
            ShowIncomingCallPanel = false;
            ShowInfo = false ;

            var errString = string.Empty;
            var sipErrCodeStr = string.Empty;
            isError = true;
            switch (errorCode)
            {
                case 200:
                    isError = false;
                    break;
                case 301:
                    errString = Properties.Resources.SIP_301;
                    break;
                case 400:
                    errString = Properties.Resources.SIP_400;
                    break;
                case 404:
                    errString = Properties.Resources.SIP_404;
                    break;
                case 406:
                    errString = Properties.Resources.SIP_406;
                    break;
                case 408:
                    errString = Properties.Resources.SIP_408;
                    break;
                case 480:
                    errString = Properties.Resources.SIP_480;
                    break;
                case 484:
                    errString = Properties.Resources.SIP_484;
                    break;
                case 486:
                    errString = Properties.Resources.SIP_486;
                    break;
                case 488:
                    errString = Properties.Resources.SIP_488;
                    break;
                case 502:
                    errString = Properties.Resources.SIP_502;
                    break;
                case 603:
                    isError = false;
                    errString = Properties.Resources.SIP_603;
                    break;
                case 604:
                    errString = Properties.Resources.SIP_604;
                    break;
                case 501:
                    errString = Properties.Resources.SIP_501;
                    break;
                case 504:
                    errString = Properties.Resources.SIP_504;
                    break;
                case 494:
                    errString = Properties.Resources.SIP_494;
                    break;
                default:
                    sipErrCodeStr = " ";
                    if (string.Compare(errorMessage, "BadCredentials", StringComparison.InvariantCultureIgnoreCase) == 0)
                        errString = Properties.Resources.ERR_BadCredentials;
                    else if (
                        string.Compare(errorMessage, "DoNotDisturb", StringComparison.InvariantCultureIgnoreCase) ==
                        0)
                        errString = Properties.Resources.ERR_DoNotDisturb;
                    else if (
                        string.Compare(errorMessage, "NoResponse", StringComparison.InvariantCultureIgnoreCase) ==
                        0)
                        errString = Properties.Resources.ERR_NoResponse;
                    else if (
                        string.Compare(errorMessage, "Unknown", StringComparison.InvariantCultureIgnoreCase) ==
                        0)
                        errString = Properties.Resources.ERR_Unknown;
                    else if (
                        string.Compare(errorMessage, "IOError", StringComparison.InvariantCultureIgnoreCase) ==
                        0)
                        errString = Properties.Resources.ERR_IOError;
                    else if (
                        string.Compare(errorMessage, "NotAnswered", StringComparison.InvariantCultureIgnoreCase) ==
                        0)
                        errString = Properties.Resources.ERR_NotAnswered;
                    else if (
                        string.Compare(errorMessage, "NotReachable", StringComparison.InvariantCultureIgnoreCase) ==
                    0)
                        errString = Properties.Resources.ERR_NotReachable;
                    else if ( (
                        string.Compare(errorMessage, "Call terminated", StringComparison.InvariantCultureIgnoreCase) ==
                            0) || (
                        string.Compare(errorMessage, "Call ended", StringComparison.InvariantCultureIgnoreCase) ==
                            0))
                            isError = false;
                    else 
                    {
                        errString = Properties.Resources.ERR_Generic;
                        sipErrCodeStr = string.Format(" (SIP: {0})", errorCode);
                    }
                    break;
            }

            if (string.IsNullOrEmpty(sipErrCodeStr))
                sipErrCodeStr = string.Format(" (SIP: {0})", errorCode);
            ErrorMessage = string.Format("{0}{1}", errString, sipErrCodeStr);

            ShowAvatar = isDeclined;

            if (isError)
                CallState = VATRPCallState.Error;
            else if (isDeclined)
                CallState = VATRPCallState.Declined;
            else
                CallState = VATRPCallState.Closed;
            ShowOutgoingEndCall = isError || isDeclined;
            ShowRingingTimer = !isError && !isDeclined;

            AllowHideContorls = false;
            StopAnimation();

            UnsubscribeCallStaistics();

            _rttViewModel.ClearRTTConversation();

            lock (timerCallLock)
            {
                if (timerCall != null)
                {
                    timerCall.Stop();
                    timerCall.Dispose();
                }
                timerCall = null;
            }
        }

        #endregion

        internal void AcceptCall()
        {
            //****************************************************************************************
            // When Incoming call Accepted.
            //*****************************************************************************************
//#if DEBUG
            AutoAnswer = 0;
            DestroyServiceTimer();
//#endif
            StopAnimation();

            //Hide();

            ShowIncomingCallPanel = false;
            IsMuteOn = false;


        }

        internal void DeclineCall(bool declineOnTimeout)
        {
            //****************************************************************************************
            // Incoming Call Declined
            //*****************************************************************************************
//#if DEBUG
            AutoAnswer = 0;
            DestroyServiceTimer();
//#endif
            SetTimeout(delegate
            {
                if (_currentCall != null)
                {
                    try
                    {
                        _linphoneService.DeclineCall(_currentCall.NativeCallPtr);
                    }
                    catch (Exception ex)
                    {
                        ServiceManager.LogError("DeclineCall", ex);
                    }
                }
            }, 30);
        }

        internal void PauseCall()
        {

            //********************************************************************************************
            // Pause a running call/ Put call On hold
            //********************************************************************************************
            SetTimeout(delegate
            {
                if (_currentCall != null)
                {
                    try
                    {
                        _linphoneService.PauseCall(_currentCall.NativeCallPtr);
                    }
                    catch (Exception ex)
                    {
                        ServiceManager.LogError("PauseCall", ex);
                    }
                }
            }, 30);
        }

        internal void ResumeCall()
        {

            //*****************************************************************************************************************
            // Resume call after Hold.
            //*****************************************************************************************************************
            SetTimeout(delegate
            {
                if (_currentCall != null)
                {
                    try
                    {
                        _linphoneService.ResumeCall(_currentCall.NativeCallPtr);
                    }
                    catch (Exception ex)
                    {
                        ServiceManager.LogError("ResumeCall", ex);
                    }
                }
            }, 30);
        }

        public void SubscribeCallStatistics()
        {

            //*************************************************************************************************************
            // When Call started this method will called for Call Statistics. Update the Info related to Call Statics
            //*************************************************************************************************************
            if (subscribedForStats)
                return;
            subscribedForStats = true;
            CallInfoCtrl.SetViewModel(_callInfoViewModel);
            ServiceManager.Instance.LinphoneService.CallStatisticsChangedEvent +=
                _callInfoViewModel.OnCallStatisticsChanged;

            _callInfoViewModel.CallQualityChangedEvent += OnCalQualityChanged;
        }

        public void UnsubscribeCallStaistics()
        {
            if (!subscribedForStats)
                return;
            subscribedForStats = false;
            ServiceManager.Instance.LinphoneService.CallStatisticsChangedEvent -=
                _callInfoViewModel.OnCallStatisticsChanged;
            _callInfoViewModel.CallQualityChangedEvent -= OnCalQualityChanged;
        }

        private void OnCalQualityChanged(VATRP.Linphone.VideoWrapper.QualityIndicator callQuality)
        {
            if (CallQualityChangedEvent != null)
                CallQualityChangedEvent(callQuality);
        }

        internal void SwitchSelfVideo()
        {
            _linphoneService.SwitchSelfVideo();
        }

        internal void HoldAndAcceptCall()
        {
//#if DEBUG
            AutoAnswer = 0;
            DestroyServiceTimer();
//#endif
            StopAnimation();

            IsMuteOn = false;

            if (_currentCall != null)
            {
                try
                {
                    bool muteMicrophone = false;
                    bool muteSpeaker = false;
                    //bool enableVideo = true;
                    if (App.CurrentAccount != null)
                    {
                        muteMicrophone = App.CurrentAccount.MuteMicrophone;
                        muteSpeaker = App.CurrentAccount.MuteSpeaker;
                        //enableVideo = App.CurrentAccount.EnableVideo;
                    }
                    _linphoneService.AcceptCall(_currentCall.NativeCallPtr,
                        ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                            Configuration.ConfEntry.USE_RTT, true), muteMicrophone, muteSpeaker, true);
                }
                catch (Exception ex)
                {
                    ServiceManager.LogError("AcceptCall", ex);
                }
            }
        }

        private void SetTimeout(Action callback, int miliseconds)
        {
            var timeout = new System.Timers.Timer {Interval = miliseconds, AutoReset = false};
            timeout.Elapsed += (sender, e) => callback();
            timeout.Start();
        }

        public bool Equals(CallViewModel other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }
            if (ReferenceEquals(other, this))
            {
                return true;
            }

            if (ActiveCall == null || other.ActiveCall == null)
                return false;

            return ActiveCall.Equals(other.ActiveCall);
        }

        public bool Equals(VATRPCall other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (ActiveCall == null)
                return false;

            return ActiveCall.Equals(other);
        }

        internal void DeferredHideMessageControl()
        {
            SetTimeout(delegate
            {
                if (ShowInfoMessage)
                {
                    if (HideMessageWindowTimeout != null) 
                        HideMessageWindowTimeout(this, EventArgs.Empty);
                }
            }, 5000);
        }
    }
}