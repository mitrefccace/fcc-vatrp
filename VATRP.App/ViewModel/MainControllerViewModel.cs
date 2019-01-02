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
using System.Windows;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Model;
using System.Collections.ObjectModel;
using com.vtcsecure.ace.windows.CustomControls;
using log4net;
using VATRP.Core.Extensions;
using VATRP.Core.Interfaces;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class MainControllerViewModel : ViewModelBase
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof (MainControllerViewModel));
        private bool _isAccountLogged;
        private string _appTitle = "VATRP";
        private bool _isContactDocked;
        private bool _isDialpadDocked;
        private bool _isHistoryDocked;
        private bool _isSettingsDocked;
        private bool _isResourceDocked;
        private bool _isMessagingDocked;
        private bool _isCallPanelDocked;
        private bool _isMoreMenuDocked;
        private bool _isChatViewEnabled;
        private bool _isIncallFullScreen;
        private bool _offerServiceSelection;
        private bool _activateWizardPage;
        private double _dialpadHeight;

        public DialpadViewModel _dialPadViewModel; //From private to Public by MK on dated 10-NOV-2016 for access in MainWindow to remove old filter number.
        private CallHistoryViewModel _historyViewModel;
        private LocalContactViewModel _contactViewModel;
        private SimpleMessagingViewModel _simpleMessageViewModel;
        private SettingsViewModel _settingsViewModel;
        private MenuViewModel _menuViewModel;
        private ObservableCollection<CallViewModel> _callsViewModelList;
        private CallViewModel _activeCallViewModel;
        private ContactsViewModel _contactsViewModel;
        private ILinphoneService _linphoneService;
        private int _uiMissedCallsCount;
        private bool _hasUnreadMessages;
        private bool _showVideomailIndicator;
        private bool _isRttViewEnabled;

        public MainControllerViewModel()
        {
            _isAccountLogged = false;
            _isContactDocked = false;
            _isDialpadDocked = false;
            _isHistoryDocked = false;
            _isMessagingDocked = false;
            _isCallPanelDocked = false;
            _isSettingsDocked = false;
            _isResourceDocked = false;
            _offerServiceSelection = false;
            _activateWizardPage = false; 
            _dialPadViewModel = new DialpadViewModel();
            _historyViewModel = new CallHistoryViewModel(ServiceManager.Instance.HistoryService, _dialPadViewModel);
            _contactsViewModel = new ContactsViewModel(ServiceManager.Instance.ContactService, _dialPadViewModel);
            _contactViewModel = new LocalContactViewModel(ServiceManager.Instance.ContactService);
            _simpleMessageViewModel = new SimpleMessagingViewModel(ServiceManager.Instance.ChatService,
                ServiceManager.Instance.ContactService);
            _settingsViewModel = new SettingsViewModel();
            _menuViewModel = new MenuViewModel();
            _historyViewModel.MissedCallsCountChanged += OnMissedCallsCountChanged;
            _simpleMessageViewModel.UnreadMessagesCountChanged += OnUnreadMesagesCountChanged;
            _callsViewModelList = new ObservableCollection<CallViewModel>();
            _linphoneService = ServiceManager.Instance.LinphoneService;

            _dialpadHeight = 350;
        }

        private void OnMissedCallsCountChanged(object callEvent, EventArgs args)
        {
            if (_historyViewModel != null)
            {
                if (IsCallHistoryDocked)
                {
                    _historyViewModel.ResetLastMissedCallTime();
                }
                UIMissedCallsCount = _historyViewModel.UnseenMissedCallsCount;
            }
        }
		
        private void OnUnreadMesagesCountChanged(object callEvent, EventArgs args)
        {
            //****************************************************************************************
            // Check the Unread messages in chat.
            //*****************************************************************************************
            if (_simpleMessageViewModel != null)
                HasUnreadMessages = ServiceManager.Instance.ChatService.HasUnreadMessages();
        }

        #region Properties

        public bool IsAccountLogged
        {
            get { return _isAccountLogged; }
            set
            {
                _isAccountLogged = value;
                OnPropertyChanged("IsAccountLogged");
                OnPropertyChanged("IsDashboardDocked");
            }
        }

        public bool IsInCallFullScreen
        {
            get { return _isIncallFullScreen; }
            set
            {
                _isIncallFullScreen = value;
                OnPropertyChanged("IsInCallFullScreen");
                OnPropertyChanged("IsDashboardDocked");
                OnPropertyChanged("IsCallPanelBorderVisible");
            }
        }

        public bool IsDashboardDocked
        {
            get { return _isAccountLogged && !_isIncallFullScreen; }
        }

        public string AppTitle
        {
            get { return _appTitle; }
            set
            {
                _appTitle = value;
                OnPropertyChanged("AppTitle");
            }
        }

        public bool IsContactDocked
        {
            get { return _isContactDocked; }
            set
            {
                _isContactDocked = value;
                OnPropertyChanged("IsContactDocked");
            }
        }

        public bool IsDialpadDocked
        {
            get { return _isDialpadDocked; }
            set
            {
                _isDialpadDocked = value;
                OnPropertyChanged("IsDialpadDocked");
            }
        }

        public bool IsCallHistoryDocked
        {
            get { return _isHistoryDocked; }
            set
            {
                _isHistoryDocked = value;
                OnPropertyChanged("IsCallHistoryDocked");
            }
        }

        public bool IsSettingsDocked
        {
            get { return _isSettingsDocked; }
            set
            {
                _isSettingsDocked = value;
                OnPropertyChanged("IsSettingsDocked");
            }
        }

        public bool IsResourceDocked
        {
            get { return _isResourceDocked; }
            set
            {
                _isResourceDocked = value;
                OnPropertyChanged("IsResourceDocked");
            }
        }

        public bool IsMessagingDocked
        {
            get { return _isMessagingDocked; }
            set
            {
                _isMessagingDocked = value;
                OnPropertyChanged("IsMessagingDocked");
            }
        }

        public bool IsCallPanelDocked
        {
            get { return _isCallPanelDocked; }
            set
            {
                _isCallPanelDocked = value;
                OnPropertyChanged("IsCallPanelDocked");
            }
        }
		
        public bool IsMenuDocked
        {
            get { return _isMoreMenuDocked; }
            set
            {
                _isMoreMenuDocked = value;
                OnPropertyChanged("IsMenuDocked");
            }
        }
		
        public bool IsChatViewEnabled
        {
            get { return _isChatViewEnabled; }
            set
            {
                _isChatViewEnabled = value;
                OnPropertyChanged("IsChatViewEnabled");
            }
        }
		
        public bool OfferServiceSelection
        {
            get { return _offerServiceSelection; }
            set
            {
                _offerServiceSelection = value;
                OnPropertyChanged("OfferServiceSelection");
            }
        }

        public bool IsRTTViewEnabled
        {
            get { return _isRttViewEnabled; }
            set
            {
                _isRttViewEnabled = value;
                OnPropertyChanged("IsRTTViewEnabled");
            }
        }

        public bool ActivateWizardPage
        {
            get
            {
                return _activateWizardPage; 
            }
            set
            {
                _activateWizardPage = value;
                OnPropertyChanged("ActivateWizardPage");
            }
        }

        public int UIMissedCallsCount
        {
            get { return _uiMissedCallsCount; }
            set
            {
                _uiMissedCallsCount = value;
                OnPropertyChanged("UIMissedCallsCount");
            }
        }

        public bool HasUnreadMessages
        {
            get { return _hasUnreadMessages; }
            set
            {
                _hasUnreadMessages = value;
                OnPropertyChanged("HasUnreadMessages");
            }
        }

        public bool ShowVideomailIndicator
        {
            get { return _showVideomailIndicator; }
            set
            {
                _showVideomailIndicator = value;
                OnPropertyChanged("ShowVideomailIndicator");
            }
        }

        public DialpadViewModel DialpadModel
        {
            get { return _dialPadViewModel; }
        }

        public CallHistoryViewModel HistoryModel
        {
            get { return _historyViewModel; }
        }

        public ContactsViewModel ContactsModel
        {
            get { return _contactsViewModel; }
        }

        public LocalContactViewModel ContactModel
        {
            get { return _contactViewModel; }
        }

        public SimpleMessagingViewModel SipSimpleMessagingModel
        {
            get { return _simpleMessageViewModel; }
        }

        public SettingsViewModel SettingsModel
        {
            get { return _settingsViewModel; }
        }

        public MenuViewModel MoreMenuModel
        {
            get { return _menuViewModel; }
        }
        
        public CallViewModel ActiveCallModel
        {
            get { return _activeCallViewModel; }
            set
            {
                _activeCallViewModel = value;
                if (_activeCallViewModel != null)
                {
                    if (_activeCallViewModel.ActiveCall != null)
                        ServiceManager.Instance.ActiveCallPtr = _activeCallViewModel.ActiveCall.NativeCallPtr;
                    else
                        ServiceManager.Instance.ActiveCallPtr = IntPtr.Zero;
                }
                else
                    ServiceManager.Instance.ActiveCallPtr = IntPtr.Zero;
                OnPropertyChanged("ActiveCall");
            }
        }

        public ObservableCollection<CallViewModel> CallsViewModelList
        {
            get { return _callsViewModelList ?? (_callsViewModelList = new ObservableCollection<CallViewModel>()); }
            set
            {
                _callsViewModelList = value;
                OnPropertyChanged("CallsViewModelList");
            }
        }

        public double DialpadHeight
        {
            get { return _dialpadHeight; }
            set
            {
                _dialpadHeight = value; 
                OnPropertyChanged("DialpadHeight");
            }
        }

        #endregion

        #region Calls management
        
        internal CallViewModel FindCallViewModel(VATRPCall call)
        {
            if (call == null)
                return null;
            lock (CallsViewModelList)
            {
                foreach (var callVM in CallsViewModelList)
                {
                    if (callVM.Equals(call))
                        return callVM;
                }
            }
            return null;
        }

        internal void AddCalViewModel(CallViewModel callViewModel)
        {
            if (FindCallViewModel(callViewModel))
                return;

            lock (CallsViewModelList)
            {
                CallsViewModelList.Add(callViewModel);
                OnPropertyChanged("CallsViewModelList");
            }
        }

        internal int RemoveCalViewModel(CallViewModel callViewModel)
        {
            lock (CallsViewModelList)
            {
                CallsViewModelList.Remove(callViewModel);
                OnPropertyChanged("CallsViewModelList");
                return CallsViewModelList.Count;
            }
        }

        internal CallViewModel GetNextViewModel(CallViewModel skipVM)
        {
            lock (CallsViewModelList)
            {
                foreach (var callVM in CallsViewModelList)
                {
                    if (!callVM.Equals(skipVM))
                        return callVM;
                }
            }
            return null;
        }

        internal bool FindCallViewModel(CallViewModel callViewModel)
        {
            lock (CallsViewModelList)
            {
                foreach (var callVM in CallsViewModelList)
                {
                    if (callVM.Equals(callViewModel))
                        return true;
                }
            }
            return false;
        }

        internal bool TerminateCall(CallViewModel viewModel, string message)
        {

            //**********************************************************************************************
            // Terminate/End call.
            //********************************************************************************************
            lock (CallsViewModelList)
            {
                if (FindCallViewModel(viewModel))
                {
                    if (viewModel.CallState != VATRPCallState.Declined)
                    {
                        LOG.Info(String.Format("Terminating call for {0}. {1}", viewModel.CallerInfo,
                            viewModel.ActiveCall.NativeCallPtr));
                        try
                        {
                            _linphoneService.TerminateCall(viewModel.ActiveCall.NativeCallPtr, message);
                        }
                        catch (Exception ex)
                        {
                            ServiceManager.LogError("TerminateCall", ex);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        internal void AcceptCall(CallViewModel viewModel)
        {

            //**********************************************************************************************
            // Incoming Call Accepted
            //********************************************************************************************
            lock (CallsViewModelList)
            {
                if (FindCallViewModel(viewModel))
                {
                    LOG.Info(String.Format("Accepting call call for {0}. {1}", viewModel.CallerInfo,
                        viewModel.ActiveCall.NativeCallPtr));

                    viewModel.AcceptCall();
                    bool muteMicrophone = false;
                    bool muteSpeaker = false;
                    //bool enableVideo = true;
                    if (App.CurrentAccount != null)
                    {
                        muteMicrophone = App.CurrentAccount.MuteMicrophone;
                        muteSpeaker = App.CurrentAccount.MuteSpeaker;
                       // enableVideo = App.CurrentAccount.EnableVideo;
                    }
                    try
                    {
                        _linphoneService.AcceptCall(viewModel.ActiveCall.NativeCallPtr,
                            ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                                Configuration.ConfEntry.USE_RTT, true), muteMicrophone, muteSpeaker, true);
                    }
                    catch (Exception ex)
                    {
                        ServiceManager.LogError("AcceptCall", ex);
                    }
                }
            }
        }

        internal void DeclineCall(CallViewModel viewModel, string message)
        {

            //***********************************************************************************************************************************
            // Call Declined, Message will contain a message like I am in meeting which was selected when call is declined.
            //***********************************************************************************************************************************
            lock (CallsViewModelList)
            {
                if (FindCallViewModel(viewModel))
                {
                    LOG.Info(String.Format("Declining call for {0}. {1}", viewModel.CallerInfo,
                        viewModel.ActiveCall.NativeCallPtr));
                    var contactID = new ContactID(
                            string.Format("{0}@{1}", viewModel.ActiveCall.RemoteParty.Username,
                                viewModel.ActiveCall.RemoteParty.HostAddress), IntPtr.Zero);
                    var contact = ServiceManager.Instance.ContactService.FindContact(contactID);
                    if (contact == null)
                    {
                        contact = new VATRPContact(contactID)
                        {
                            Fullname = viewModel.ActiveCall.RemoteParty.Username,
                            DisplayName = viewModel.ActiveCall.RemoteParty.Username,
                            SipUsername = viewModel.ActiveCall.RemoteParty.Username,
                            RegistrationName = contactID.ID
                        };
                        ServiceManager.Instance.ContactService.AddContact(contact, string.Empty);
                    }
                    viewModel.DeclineCall(false);
                    try
                    {
                        _linphoneService.DeclineCall(viewModel.ActiveCall.NativeCallPtr);
                    }
                    catch (Exception ex)
                    {
                        ServiceManager.LogError("DeclineCall", ex);
                    }

                    if (message.NotBlank())
                    {
                        if (_simpleMessageViewModel != null)
                        {
                            _simpleMessageViewModel.SetActiveChatContact(contact, IntPtr.Zero);
                            _simpleMessageViewModel.SendSimpleMessage(string.Format("{0}{1}", VATRPChatMessage.DECLINE_PREFIX, message));
                        }
                    }
                }
            }
        }

        internal void EndAndAcceptCall(CallViewModel viewModel)
        {
            lock (CallsViewModelList)
            {
                CallViewModel nextCall = GetNextViewModel(viewModel);
                if (nextCall != null)
                {
                    TerminateCall(nextCall, "Call terminated");

                    AcceptCall(viewModel);
                }
            }
        }

        #endregion

        internal void ResumeCall(CallViewModel viewModel)
        {
            //***************************************************************************************************************
            // Resume call after Hold.
            //*****************************************************************************************************************
            lock (CallsViewModelList)
            {
                if (FindCallViewModel(viewModel))
                {
                    LOG.Info(String.Format("Resuming call for {0}. {1}", viewModel.CallerInfo,
                        viewModel.ActiveCall.NativeCallPtr));

                    viewModel.ResumeCall();
                }
            }
        }

        internal void PauseCall(CallViewModel viewModel)
        {
            lock (CallsViewModelList)
            {
                if (FindCallViewModel(viewModel))
                {
                    LOG.Info(String.Format("Pausing call for {0}. {1}", viewModel.CallerInfo,
                        viewModel.ActiveCall.NativeCallPtr));
                    viewModel.PauseCall();
                }
            }
        }

        internal bool SwitchCall(CallViewModel pausedCallViewModel, CallViewModel activeCallViewModel)
        {
            LOG.Info(String.Format("Switching call. Main call {0}. {1}. Secondary call {2} {3}",
                activeCallViewModel.CallerInfo,
                activeCallViewModel.ActiveCall.NativeCallPtr, pausedCallViewModel.CallerInfo,
                pausedCallViewModel.ActiveCall.NativeCallPtr));


           

            if (activeCallViewModel.CallState == VATRPCallState.LocalPaused && activeCallViewModel.PauseRequest)
            {
                if (pausedCallViewModel.CallState == VATRPCallState.LocalPaused && pausedCallViewModel.PauseRequest)
                    return false;
				ResumeCall(pausedCallViewModel);
            }
            else
            {

                //var textBlock =   FindChild<TextBlock>(newCallAcceptWindow.TransparentWindow, "NewCallerInfoLabel");
                //if (textBlock != null)
                //    textBlock.Text = callerInfo;
               // ShowNewCallSwapWindow(true);

                PauseCall(activeCallViewModel);
                
               
            }
            return true;
        }

        internal CallViewModel FindDeclinedCallViewModel()
        {
            lock (CallsViewModelList)
            {
                foreach (var callVM in CallsViewModelList)
                {
                    if (callVM.CallState == VATRPCallState.Declined)
                        return callVM;
                }
            }
            return null;
        }
    }
}