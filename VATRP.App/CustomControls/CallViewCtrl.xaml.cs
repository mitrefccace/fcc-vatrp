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
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.ViewModel;
using com.vtcsecure.ace.windows.Views;
using log4net;
using VATRP.Core.Model;
using Win32Api = com.vtcsecure.ace.windows.Services.Win32NativeAPI;

namespace com.vtcsecure.ace.windows.CustomControls
{
    /// <summary>
    /// Interaction logic for CallViewCtrl.xaml
    /// </summary>
    public partial class CallViewCtrl
    {

        #region Members

        private static readonly ILog LOG = LogManager.GetLogger(typeof (CallViewCtrl));
        private CallViewModel _viewModel;
        private MainControllerViewModel _parentViewModel;
        private CallViewModel _backgroundCallViewModel;
        private DispatcherTimer _mouseInactivityTimer;
        private bool _controlsHiddenByTimer = false;
        private bool _showControlsOnTimeout = false;
        private bool restoreVisibilityStates = false;
        private System.Drawing.Point _lastMousePosition;
        private bool _inactivityTimerStopped;
        private bool _isFullScreenOn;
        private object _viewLock = new object();
        #endregion

        #region Properties

        public KeyPadCtrl KeypadCtrl { get; set; }

        public MainControllerViewModel ParentViewModel
        {
            get { return _parentViewModel; }
            set { _parentViewModel = value; }
        }

        public CallViewModel BackgroundCallViewModel
        {
            get { return _backgroundCallViewModel; }
            set { _backgroundCallViewModel = value; }
        }

        #endregion

        #region Events

        public delegate void SwitchCallbarButton(bool switch_on);

        public event SwitchCallbarButton VideoOnToggled;
        public event SwitchCallbarButton FullScreenOnToggled;
        public event SwitchCallbarButton MuteOnToggled;
        public event SwitchCallbarButton SpeakerOnToggled;
        public event SwitchCallbarButton NumpadToggled;
        public event SwitchCallbarButton RttToggled;
        public event SwitchCallbarButton CallInfoToggled;
        public event EventHandler<KeyPadEventArgs> KeypadClicked;
        public event EventHandler SwitchHoldCallsRequested;
        public event EventHandler HideDeclineMessageRequested;
        private bool _mouseInControlArea = false;

        #endregion

        public CallViewCtrl()
        {
           

            //****************************************************************************************************************
            // This method is called When Call screen initilize.
            //****************************************************************************************************************
            InitializeComponent();
            DataContext = _viewModel;
            ctrlOverlay.CommandOverlayWidth = 660;
            ctrlOverlay.CommandOverlayHeight = 160;

            ctrlOverlay.NumpadOverlayWidth = 229;
            ctrlOverlay.NumpadOverlayHeight = 305;

            ctrlOverlay.CallInfoOverlayWidth = 660;
            ctrlOverlay.CallInfoOverlayHeight = 200;

            ctrlOverlay.NewCallAcceptOverlayWidth = 370;
            ctrlOverlay.NewCallAcceptOverlayHeight = 160;

            ctrlOverlay.CallsSwitchOverlayWidth = 190;
            ctrlOverlay.CallsSwitchOverlayHeight = 200;

            ctrlOverlay.OnHoldOverlayWidth = 660;
            ctrlOverlay.OnHoldOverlayHeight = 400;

            ctrlOverlay.QualityIndicatorOverlayHeight = 30;
            ctrlOverlay.QualityIndicatorOverlayWidth = 54;

            ctrlOverlay.EncryptionIndicatorOverlayHeight = 19;
            ctrlOverlay.EncryptionIndicatorOverlayWidth = 24;

            ctrlOverlay.InfoMsgOverlayHeight = 180;
            ctrlOverlay.InfoMsgOverlayWidth = 670;

            _mouseInactivityTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3),
            };
            _mouseInactivityTimer.Tick += OnMouseInactivityTimer;
        }

        public CallViewCtrl(MainControllerViewModel parentVM) : this()
        {
            _parentViewModel = parentVM;
        }

        public void SetCallViewModel(CallViewModel viewModel)
        {
            //**************************************************************************************************************
            // Set Call Screen for display Incoming and Outgoing calls
            //**************************************************************************************************************
            lock (_viewLock)
            {
                if (_viewModel == viewModel)
                    return;
                DataContext = viewModel;
                _viewModel = viewModel;
            }
            UpdateControls();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        internal void EndCall(bool bRunning)
        {
            //********************************************************************************************************************
            // End the call which was running.
            //********************************************************************************************************************
            if (_parentViewModel != null)
            {
                if (!_parentViewModel.TerminateCall(bRunning ? _viewModel : _backgroundCallViewModel, "Call terminated"))
                {
                    HideDeclineMessage(bRunning ? _viewModel : _backgroundCallViewModel);
                }
            }
        }

        internal void MuteSpeaker(bool isMuted)
        {

            //******************************************************************************************************************************
            // When call Speaker Mute button is pressed over call window.
            //******************************************************************************************************************************
            if (_viewModel.ActiveCall != null)
                _viewModel.MuteSpeaker(isMuted);
        }

        internal bool MuteCall(bool isMuted)
        {
            //*********************************************************************************************************************
            // When call Mute button is pressed over call window.
            //*********************************************************************************************************************
            if (_viewModel.ActiveCall != null)
                return _viewModel.MuteCall(isMuted);
            return false;
        }

        private void OnEndCall(object sender, RoutedEventArgs e)
        {
            //********************************************************************************************************************
            // When End Call button is pressed over call window.
            //********************************************************************************************************************
            EndCall(true);
        }

        private void OnEndPaused(object sender, RoutedEventArgs e)
        {
            //***************************************************************************************************************
            // End the one call when 2 calls are running (One is on hold and one is active).
            //***************************************************************************************************************
            EndCall(false);
        }

        public void AcceptCall(object sender, RoutedEventArgs e)
        {

            //**********************************************************************************************
            // When call Accept button is pressed over call window.
            //********************************************************************************************
            if (_parentViewModel != null)
                _parentViewModel.AcceptCall(_viewModel);
        }

        private void DeclineCall(object sender, RoutedEventArgs e)
        {
            //**********************************************************************************************
            // When call Decline button is pressed over call window.
            //********************************************************************************************
            if (_parentViewModel != null)
                _parentViewModel.DeclineCall(_viewModel, string.Empty);
        }

        #region Call Statistics Info

        private void OnToggleInfo(object sender, RoutedEventArgs e)
        {

            //**********************************************************************************************
            // When Info button is pressed over call window.
            //**********************************************************************************************
            if (CallInfoToggled != null)
                CallInfoToggled(BtnInfo.IsChecked ?? false);
            if (_viewModel != null)
                _viewModel.ToggleCallStatisticsInfo(BtnInfo.IsChecked ?? false);
            SaveStates();
        }

        #endregion


        internal void AddVideoControl()
        {

            //****************************************************************************************************************
            // Add video window when call is accepted.
            //****************************************************************************************************************
            try
            {
                ServiceManager.Instance.LinphoneService.SetVideoCallWindowHandle(ctrlVideo.GetVideoControlPtr);
                ctrlVideo.Visibility = Visibility.Visible;
                //ctrlVideo.
            }
            catch (Exception ex)
            {
                ServiceManager.LogError("Main OnCallStateChanged", ex);
            }
        }

        private void OnToggleFullScreen(object sender, RoutedEventArgs e)
        {

            //***************************************************************************************************************************************
            // When Full screen button is pressed over call window.
            //***************************************************************************************************************************************
            if (FullScreenOnToggled != null)
                FullScreenOnToggled(BtnFullScreen.IsChecked ?? false);
            SaveStates();
        }

        private void OnToggleVideo(object sender, RoutedEventArgs e)
        {
            //****************************************************************************************************************************************
            // Toggle for Video On/Off during call.
            //****************************************************************************************************************************************
            if (VideoOnToggled != null)
                VideoOnToggled(BtnVideoOn.IsChecked ?? false);
            if (_viewModel != null)
                _viewModel.ToggleVideo(!BtnVideoOn.IsChecked ?? false);
            SaveStates();
        }

        private void OnToggleSpeaker(object sender, RoutedEventArgs e)
        {
            //*************************************************************************************************************************************
            // Speaker Toggle Mute/Unmute Over call window.
            //*************************************************************************************************************************************
            if (SpeakerOnToggled != null)
                SpeakerOnToggled(BtnSpeaker.IsChecked ?? false);
            SaveStates();
            MuteSpeaker(BtnSpeaker.IsChecked ?? false);
        }

        private void OnToggleRTT(object sender, RoutedEventArgs e)
        {   // CJM: RTT
            //************************************************************************************************************************************
            // When Chat button is pressed from Call window overlay. This will display chat window in the right side of Call Window.
            //************************************************************************************************************************************
            if (!_viewModel.IsRTTEnabled)
            {
                _inactivityTimerStopped = true;
                _mouseInactivityTimer.Stop();
                MessageBox.Show("RTT has been disabled for this call", "VATRP", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                RestartInactivityDetectionTimer();
                BtnRTT.IsChecked = false;
                return;
            }

            if (RttToggled != null)
                RttToggled(BtnRTT.IsChecked ?? false);
            SaveStates();
        }

        private void OnToggleKeypad(object sender, RoutedEventArgs e)
        {

            //**********************************************************************************************************************************************
            // When Dialpaad button is pressed over call window.
            //**********************************************************************************************************************************************
            if (NumpadToggled != null)
                NumpadToggled(BtnNumpad.IsChecked ?? false);
            ctrlOverlay.ShowNumpadWindow(BtnNumpad.IsChecked ?? false);
            SaveStates();
        }

        private void OnMute(object sender, RoutedEventArgs e)
        {
            //**********************************************************************************************************************************************
            // Mute Mic button pressed over Call Window.
            //**********************************************************************************************************************************************
            bool isChecked = BtnMuteOn.IsChecked ?? false;
            if (!MuteCall(isChecked))
            {
                BtnMuteOn.IsChecked = !isChecked;
                return;
            }
            if (MuteOnToggled != null)
                MuteOnToggled(BtnMuteOn.IsChecked ?? false);
            SaveStates();
        }

        private void buttonKeyPad(object sender, RoutedEventArgs e)
        {
            //**************************************************************************************************************************************
            // When buton is pressed from Keypad over Call window.
            //**************************************************************************************************************************************
            if (KeypadClicked != null)
            {
                var btnKey = e.OriginalSource as Button;
                if (btnKey != null)
                {
                    if (Equals(e.OriginalSource, buttonKeyPadStar))
                    {
                        KeypadClicked(this, new KeyPadEventArgs(DialpadKey.DialpadKey_KeyStar));
                    }
                    else if (Equals(e.OriginalSource, buttonKeyPadPound))
                    {
                        KeypadClicked(this, new KeyPadEventArgs(DialpadKey.DialpadKey_KeyPound));
                    }
                    else
                    {
                        char key;
                        if (char.TryParse(btnKey.Tag.ToString(), out key))
                            KeypadClicked(this, new KeyPadEventArgs((DialpadKey) key));
                        else
                        {
                            KeypadClicked(this, new KeyPadEventArgs(DialpadKey.DialpadKey_Key0));
                        }
                    }
                }
            }
        }

        public void HoldAndAcceptCall(object sender, RoutedEventArgs e)
        {
            //**********************************************************************************************************************************
            // Hold the current call and Accept the other call when a call is already running.
            //**********************************************************************************************************************************
            if (_parentViewModel != null)
                _parentViewModel.AcceptCall(_viewModel);
        }

        private void AcceptAndEndCall(object sender, RoutedEventArgs e)
        {

            //*************************************************************************************************************************************
            // End the active call and Accept the other call when a call is already running.
            //*************************************************************************************************************************************
            SaveStates();
            if (_parentViewModel != null)
                _parentViewModel.EndAndAcceptCall(_viewModel);
        }

        private void SaveStates()
        {

            //**************************************************************************************************************************************
            // Save last state of call window.
            //**************************************************************************************************************************************
            if (_viewModel != null)
            {
                _viewModel.SavedIsVideoOn = BtnVideoOn.IsChecked ?? false;
                _viewModel.SavedIsMuteOn = BtnMuteOn.IsChecked ?? false;
                _viewModel.SavedIsSpeakerOn = BtnSpeaker.IsChecked ?? false;
                _viewModel.SavedIsNumpadOn = BtnNumpad.IsChecked ?? false;
                _viewModel.SavedIsRttOn = BtnRTT.IsChecked ?? false;
                _viewModel.SavedIsInfoOn = BtnInfo.IsChecked ?? false;
                _viewModel.SavedIsCallHoldOn = BtnHold.IsChecked ?? false;
            }
        }

        private void LoadStates()
        {

            //************************************************************************************************************************************
            // Load last call saved state of Call window.
            //************************************************************************************************************************************
            if (_viewModel != null)
            {
                _viewModel.IsVideoOn = _viewModel.SavedIsVideoOn;
                _viewModel.IsMuteOn = _viewModel.SavedIsMuteOn;
                _viewModel.IsSpeakerOn = _viewModel.SavedIsSpeakerOn;
                _viewModel.IsNumpadOn = _viewModel.SavedIsNumpadOn;
                _viewModel.IsRttOn = _viewModel.SavedIsRttOn && _viewModel.IsRTTEnabled;
                _viewModel.IsCallInfoOn = _viewModel.SavedIsInfoOn;
                _viewModel.IsCallOnHold = _viewModel.SavedIsCallHoldOn;
                _viewModel.IsFullScreenOn = _parentViewModel != null && _parentViewModel.IsInCallFullScreen;
            }
        }

        internal void UpdateControls()
        {
            if (_viewModel != null)
            {
                LoadStates();
                // do not force this to false. make sure that the call is muted if this setting is 
//                _viewModel.IsMuteOn = false;
                BtnMuteOn.IsChecked = _viewModel.IsMuteOn;
                BtnVideoOn.IsChecked = _viewModel.IsVideoOn;
                BtnSpeaker.IsChecked = _viewModel.IsSpeakerOn;
                BtnNumpad.IsChecked = _viewModel.IsNumpadOn;
                BtnRTT.IsChecked = _viewModel.IsRttOn;
                BtnInfo.IsChecked = _viewModel.IsCallInfoOn;
                BtnHold.IsChecked = _viewModel.IsCallOnHold;

                _viewModel.ToggleCallStatisticsInfo(BtnInfo.IsChecked ?? false);
            }

            if (RttToggled != null)
                RttToggled(BtnRTT.IsChecked ?? false);
            ctrlOverlay.ShowNumpadWindow(BtnNumpad.IsChecked ?? false);

            bool rttEnabled = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.USE_RTT, true);
            EnableRTTButton(rttEnabled); // CJM : RTT
            UpdateVideoSettingsIfOpen();
        }



        private void OnDeclineCall(object sender, RoutedEventArgs e)
        {
            if (_parentViewModel != null)
                _parentViewModel.DeclineCall(_viewModel, string.Empty);
        }

        private void SwitchCall(object sender, RoutedEventArgs e)
        {
            bool value = false;

            if (_parentViewModel != null)
            {
                value = _parentViewModel.SwitchCall(_backgroundCallViewModel, _viewModel);
                if (!value)
                {
                    if (SwitchHoldCallsRequested != null)
                        SwitchHoldCallsRequested(this, EventArgs.Empty);
                    //this.CallSwapLabel.Text = "Switching the call to user Mitesh";
                }

                if (value)
                {
                    //var textBlock = FindChild<TextBlock>(callInfoWindow.TransparentWindow, "CallDurationLabel");
                    //if (textBlock != null)
                    //{
                    //    textBlock.Text = str;
                    //}

                }
                    //this.CallSwapLabel.Text = "Switching the call to user Mitesh";
                    
                //}
                //if (!_parentViewModel.SwitchCall(_backgroundCallViewModel, _viewModel))
                //{
                //    if (SwitchHoldCallsRequested != null)
                //        SwitchHoldCallsRequested(this, EventArgs.Empty);
                //}
            }
        }

        private void OnToggleHold(object sender, RoutedEventArgs e)
        {
            //*****************************************************************************************************************************************
            // When call Hold/Pause button is pressed over call window.
            //*****************************************************************************************************************************************
            if (_parentViewModel != null)
            {
                if (_viewModel.CallState == VATRPCallState.RemotePaused)
                {
                    BtnHold.IsChecked = false;
                    SaveStates();
                }
                else
                {
                    SaveStates();
                    if (BtnHold.IsChecked ?? false)
                    {
                        _viewModel.PauseRequest = true;
                        _parentViewModel.PauseCall(_viewModel);
                    }
                    else
                    {
                        _viewModel.PauseRequest = false;
                        _viewModel.ResumeRequest = true;
                        _parentViewModel.ResumeCall(_viewModel);
                    }
                }
            }
        }

        public bool IsRTTViewShown()
        {
            return this.BtnRTT.IsChecked ?? false;
        }

        public void UpdateRTTToggle(bool enable)
        {
            if ((BtnRTT.IsChecked ?? false) != enable)
            {
                BtnRTT.IsChecked = enable;
            }
        }

        public void EnableRTTButton(bool enable)
        {
            BtnRTT.IsEnabled = enable;

            if ((BtnRTT.IsChecked ?? false))
            {
                BtnRTT.IsChecked = false;
                if (RttToggled != null)
                    RttToggled(false);
            }
        }

        public void UpdateMuteSettingsIfOpen()
        {

            //****************************************************************************************************************
            // Update Mice and Speaker Mute setting.
            //****************************************************************************************************************
            if (App.CurrentAccount != null)
            {
                this.BtnMuteOn.IsChecked = App.CurrentAccount.MuteMicrophone;
                this.BtnSpeaker.IsChecked = App.CurrentAccount.MuteSpeaker;

                if (_viewModel != null)
                {
                    _viewModel.IsMuteOn = BtnMuteOn.IsChecked ?? false;
                    _viewModel.IsSpeakerOn = BtnSpeaker.IsChecked ?? false;
                }
            }
        }

        public void UpdateVideoSettingsIfOpen()
        {

            //***************************************************************************************************************************************
            // Update video setting, Enable/Disable the video option.
            //**************************************************************************************************************************************
            if (_viewModel != null && _viewModel.ActiveCall != null)
            {

                var isVideoEnabled = ServiceManager.Instance.LinphoneService.IsVideoEnabled(_viewModel.ActiveCall);
                this.BtnVideoOn.IsEnabled = isVideoEnabled;
                this.BtnVideoOn.IsChecked = !isVideoEnabled ||
                                            !ServiceManager.Instance.LinphoneService.IsCameraEnabled(
                                                _viewModel.ActiveCall.NativeCallPtr);
            }
            else
            {
                //***********************************************************************************************************************************
                // If call is cancelled/Disconnected
                //***********************************************************************************************************************************
                this.BtnVideoOn.IsChecked = true;
                this.BtnVideoOn.IsEnabled = false;
            }
        }

        private void RestoreControlsVisibility()
        {

            //**************************************************************************************************************************************
            // Set the Call window controls visiblity when mouse hover
            //**************************************************************************************************************************************
            if (!restoreVisibilityStates)
                return;

            if (_viewModel == null || _viewModel.CallState == VATRPCallState.Closed)
            {
                HideOverlayControls();
                return;
            }

            if (_viewModel != null)
            {
                if (_viewModel.CallInfoLastTimeVisibility == Visibility.Visible)
                {
                    ctrlOverlay.ShowCallInfoWindow(true);
                }

                if (_viewModel.CommandbarLastTimeVisibility == Visibility.Visible)
                {
                    ctrlOverlay.ShowCommandBar(true);
                }

                if (_viewModel.CallSwitchLastTimeVisibility == Visibility.Visible)
                {
                    ctrlOverlay.ShowCallsSwitchWindow(true);
                }

                if (_viewModel.NumpadLastTimeVisibility == Visibility.Visible)
                {
                    ctrlOverlay.ShowNumpadWindow(true);
                }
            }
            restoreVisibilityStates = false;
        }

        private void HideOverlayControls()
        {

            //***************************************************************************************************************************************
            // Hide overlay window which was display over call window.
            //***************************************************************************************************************************************
            if (_viewModel == null || restoreVisibilityStates)
                return;

            var wndObject = ctrlOverlay.CallInfoWindow;
            if (wndObject != null)
            {
                _viewModel.CallInfoLastTimeVisibility = wndObject.ShowWindow
                    ? Visibility.Visible
                    : Visibility.Hidden;
                if (wndObject.ShowWindow)
                {
                    _viewModel.CallInfoLastTimeVisibility = Visibility.Visible;
                    ctrlOverlay.ShowCallInfoWindow(false);
                }
            }

            wndObject = ctrlOverlay.CommandBarWindow;
            if (wndObject != null)
            {
                _viewModel.CommandbarLastTimeVisibility = wndObject.ShowWindow
                    ? Visibility.Visible
                    : Visibility.Hidden;
                if (wndObject.ShowWindow)
                {
                    _viewModel.CommandbarLastTimeVisibility = Visibility.Visible;
                    ctrlOverlay.ShowCommandBar(false);
                }
            }

            wndObject = ctrlOverlay.CallsSwitchWindow;
            if (wndObject != null)
            {
                _viewModel.CallSwitchLastTimeVisibility = wndObject.ShowWindow
                    ? Visibility.Visible
                    : Visibility.Hidden;
                if (wndObject.ShowWindow)
                {
                    _viewModel.CallSwitchLastTimeVisibility = Visibility.Visible;
                    ctrlOverlay.ShowCallsSwitchWindow(false);
                }
            }

            wndObject = ctrlOverlay.NumpadWindow;
            if (wndObject != null)
            {
                _viewModel.NumpadLastTimeVisibility = wndObject.ShowWindow
                    ? Visibility.Visible
                    : Visibility.Hidden;
                if (wndObject.ShowWindow)
                {
                    _viewModel.NumpadLastTimeVisibility = Visibility.Visible;
                    ctrlOverlay.ShowNumpadWindow(false);
                }
            }

            restoreVisibilityStates = true;
        }

        private void CtrlVideo_OnMouseMove(object sender, MouseEventArgs e)
        {

            //*************************************************************************************************************************************
            // When Mouse is moved over call window.
            //*************************************************************************************************************************************
            lock (_viewLock)
            {
                if (_viewModel == null || !_viewModel.AllowHideContorls)
                    return;
                if (_viewModel.CallState == VATRPCallState.Closed)
                    return;
                Win32Api.POINT mousePositionInControl;
                Win32Api.GetCursorPos(out mousePositionInControl);

                if (_lastMousePosition.X != mousePositionInControl.X ||
                    _lastMousePosition.Y != mousePositionInControl.Y)
                {
                    _mouseInControlArea = false;
                    RestoreControlsVisibility();
                    RestartInactivityDetectionTimer();
                }
                _lastMousePosition = mousePositionInControl;
            }
        }
        
        private void CtrlVideo_OnMouseEnter(object sender, MouseEventArgs e)
        {
            //*****************************************************************************************************************************************
            // When Mouse is moved over call window.
            //*****************************************************************************************************************************************
            lock (_viewLock)
            {
                if (_viewModel == null || !_viewModel.AllowHideContorls)
                    return;

                if (_viewModel == null || _viewModel.CallState == VATRPCallState.Closed)
                    return;
                Win32Api.POINT mousePositionInControl;
                Win32Api.GetCursorPos(out mousePositionInControl);

                if (_lastMousePosition.X == mousePositionInControl.X &&
                    _lastMousePosition.Y == mousePositionInControl.Y)
                {
                    //Debug.WriteLine("Unchanged coordinates. Should be skipped. Control area: " + _mouseInControlArea);
                    if (restoreVisibilityStates)
                    {
                        if (_mouseInactivityTimer.IsEnabled)
                        {
                            _inactivityTimerStopped = true;
                            _mouseInactivityTimer.Stop();
                        }
                    }
                    return;
                }

                _lastMousePosition = mousePositionInControl;
                if (!_mouseInControlArea)
                    RestoreControlsVisibility();
                RestartInactivityDetectionTimer();
            }
        }

        private void CtrlVideo_OnMouseLeave(object sender, MouseEventArgs e)
        {

            //***************************************************************************************************************************************
            // When mouse leave the Video call window
            //**************************************************************************************************************************************
            lock (_viewLock)
            {
                if (_viewModel == null || !_viewModel.AllowHideContorls)
                    return;

                Point mousePosition = Mouse.GetPosition(this);

                //Debug.WriteLine("MouseLeave: X = {0}, Y={1}", mousePositionInControl.X, mousePositionInControl.Y); 
                if (mousePosition.X > 0 && mousePosition.Y > 0)
                {
                    //Debug.WriteLine("we are in control area, ");
                    _mouseInControlArea = true;
                }

                Win32Api.POINT mousePositionInControl;
                Win32Api.GetCursorPos(out mousePositionInControl);
                _lastMousePosition = mousePositionInControl;
                RestartInactivityDetectionTimer();
            }
        }

        private void CtrlVieo_MouseDown(object sender, MouseEventArgs e)
        {
            
        }

        private void OnMouseInactivityTimer(object sender, EventArgs e)
        {

            //****************************************************************************************************************************************
            // When Mouse is not moving (Inactive) then this method will called.
            //****************************************************************************************************************************************
            _mouseInactivityTimer.Stop();
            lock (_viewLock)
            {
                if (_viewModel == null || _viewModel.CallState == VATRPCallState.Closed)
                    return;
                if (_inactivityTimerStopped)
                {
                    Debug.WriteLine("Inactivity timer stopped. Do not process ");
                    return;
                }

                if (!restoreVisibilityStates)
                {
                    Win32Api.POINT mousePositionInControl;
                    Win32Api.GetCursorPos(out mousePositionInControl);
                    _lastMousePosition = mousePositionInControl;
                    HideOverlayControls();
                }
                else
                {
                    RestoreControlsVisibility();
                }
            }
        }

        public void RestartInactivityDetectionTimer()
        {
//            Debug.WriteLine("Restart detection timer");
            if (_mouseInactivityTimer != null)
            {
                if (_mouseInactivityTimer.IsEnabled)
                {
                    _inactivityTimerStopped = true;
                    _mouseInactivityTimer.Stop();
                }

                _mouseInactivityTimer.Start();
                _inactivityTimerStopped = false;
            }
        }
        
        public void CheckRttButton()
        {
            _viewModel.IsRttOn = true;
            BtnRTT.IsChecked = _viewModel.IsRttOn;
            _viewModel.SavedIsRttOn = BtnRTT.IsChecked ?? false;
        }

        internal void ExitFullScreen()
        {
            BtnFullScreen.IsChecked = false;
            OnToggleFullScreen(this, null);
        }

        private void OnSendDeclineMessage(object sender, RoutedEventArgs e)
        {

            //*************************************************************************************************************************
            // When Call declined and Decline Message is selected.
            //*************************************************************************************************************************
            if (_viewModel != null)
                _viewModel.ShowDeclineMenu = false;

            var menuItem = sender as Button;
            if (menuItem != null)
            {
                if (_parentViewModel != null)
                    _parentViewModel.DeclineCall(_viewModel, menuItem.Tag as string ?? string.Empty);
            }
        }

        private void ToggleDeclineMenu(object sender, RoutedEventArgs e)
        {

            //**********************************************************************************************************************
            // When call Decline button is Popup pressed over call window. It will show the messages for decline a call.
            //**********************************************************************************************************************
            if (_viewModel != null)
            {
                _viewModel.ShowDeclineMenu = !_viewModel.ShowDeclineMenu;
                ArrowBtn.Focus();
            }
        }

        private void OnControlClicked(object sender, MouseButtonEventArgs e)
        {

            //**********************************************************************************************
            // Anywhere clicked on call window.
            //********************************************************************************************
            HideDeclineMessage(sender);
        }

        private void HideDeclineMessage(object sender)
        {
            if (HideDeclineMessageRequested != null) 
                HideDeclineMessageRequested(sender, EventArgs.Empty);
        }
    }
}
