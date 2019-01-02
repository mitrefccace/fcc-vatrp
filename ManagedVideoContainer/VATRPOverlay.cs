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
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace VATRP.Linphone.VideoWrapper
{
    public class VATRPOverlay : Decorator
    {
        private VATRPTranslucentWindow commandBarWindow;
        private VATRPTranslucentWindow numpadWindow;
        private VATRPTranslucentWindow callInfoWindow;
        private VATRPTranslucentWindow callsSwitchWindow;
        private VATRPTranslucentWindow newCallAcceptWindow;
        private VATRPTranslucentWindow onHoldWindow;
        private VATRPTranslucentWindow qualityIndicatoWindow;
        private VATRPTranslucentWindow encryptionIndicatoWindow;
        private VATRPTranslucentWindow infoMsgWindow;
        private System.Timers.Timer _timerCall;
        private int _foregroundCallDuration = 0;
        private int _backgroundCallDuration = 0;
        private QualityIndicator _lastQuality = QualityIndicator.Unknown;
        private MediaEncryptionIndicator _lastEncryption = MediaEncryptionIndicator.None;

        private Boolean hideSwapLabel = false;
        private int durationHideLabel = 0;

        public VATRPOverlay()
        {
            commandBarWindow = new VATRPTranslucentWindow(this);
            numpadWindow = new VATRPTranslucentWindow(this);
            callInfoWindow = new VATRPTranslucentWindow(this);
            callsSwitchWindow = new VATRPTranslucentWindow(this);
            newCallAcceptWindow = new VATRPTranslucentWindow(this);
            onHoldWindow = new VATRPTranslucentWindow(this);
            qualityIndicatoWindow = new VATRPTranslucentWindow(this);
            encryptionIndicatoWindow = new VATRPTranslucentWindow(this);
            infoMsgWindow = new VATRPTranslucentWindow(this);

            //  Commented out 4/5/2017 MITRE-fjr
            //_timerCall = new System.Timers.Timer
            //{
            //    Interval = 1000,
            //    AutoReset = true
            //};
            //_timerCall.Elapsed += OnUpdatecallTimer;
        }

        #region Command Bar window

        public double CommandWindowLeftMargin
        {
            get
            {
                if (commandBarWindow != null)
                    return commandBarWindow.WindowLeftMargin;
                return 0;
            }
            set
            {
                if (commandBarWindow != null)
                    commandBarWindow.WindowLeftMargin = value;
            }
        }

        public double CommandWindowTopMargin
        {
            get
            {
                if (commandBarWindow != null)
                    return commandBarWindow.WindowTopMargin;
                return 0;
            }
            set
            {
                if (commandBarWindow != null)
                    commandBarWindow.WindowTopMargin = value;
            }
        }

        public int CommandOverlayWidth
        {
            get
            {
                if (commandBarWindow != null)
                    return commandBarWindow.OverlayWidth;
                return 0;
            }
            set
            {
                if (commandBarWindow != null)
                    commandBarWindow.OverlayWidth = value;
            }
        }

        public int CommandOverlayHeight
        {
            get
            {
                if (commandBarWindow != null)
                    return commandBarWindow.OverlayHeight;
                return 0;
            }
            set
            {
                if (commandBarWindow != null)
                    commandBarWindow.OverlayHeight = value;
            }
        }

        public void ShowCommandBar(bool bshow)
        {
            commandBarWindow.ShowWindow = bshow;
            commandBarWindow.Refresh();
            if (bshow)
                commandBarWindow.UpdateWindow();
        }

        public object OverlayCommandbarChild
        {
            get
            {
                if (commandBarWindow != null && commandBarWindow.TransparentWindow != null)
                {
                    return commandBarWindow.TransparentWindow.Content;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (commandBarWindow != null && commandBarWindow.TransparentWindow != null)
                {
                    commandBarWindow.TransparentWindow.Content = value;
                }
            }
        }

        #endregion

        #region Numpad window

        public double NumpadWindowLeftMargin
        {
            get
            {
                if (numpadWindow != null)
                    return numpadWindow.WindowLeftMargin;
                return 0;
            }
            set
            {
                if (numpadWindow != null)
                    numpadWindow.WindowLeftMargin = value;
            }
        }

        public double NumpadWindowTopMargin
        {
            get
            {
                if (numpadWindow != null)
                    return numpadWindow.WindowTopMargin;
                return 0;
            }
            set
            {
                if (numpadWindow != null)
                    numpadWindow.WindowTopMargin = value;
            }
        }

        public int NumpadOverlayWidth
        {
            get
            {
                if (numpadWindow != null)
                    return numpadWindow.OverlayWidth;
                return 0;
            }
            set
            {
                if (numpadWindow != null)
                    numpadWindow.OverlayWidth = value;
            }
        }

        public int NumpadOverlayHeight
        {
            get
            {
                if (numpadWindow != null)
                    return numpadWindow.OverlayHeight;
                return 0;
            }
            set
            {
                if (numpadWindow != null)
                    numpadWindow.OverlayHeight = value;
            }
        }

        public void ShowNumpadWindow(bool bshow)
        {
            numpadWindow.ShowWindow = bshow;
            numpadWindow.Refresh();
            if (bshow)
                numpadWindow.UpdateWindow();
        }

        public object OverlayNumpadChild
        {
            get
            {
                if (numpadWindow != null && numpadWindow.TransparentWindow != null)
                {
                    return numpadWindow.TransparentWindow.Content;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (numpadWindow != null && numpadWindow.TransparentWindow != null)
                {
                    numpadWindow.TransparentWindow.Content = value;
                }
            }
        }

        #endregion

        #region CallInfo window

        public double CallInfoWindowLeftMargin
        {
            get
            {
                if (callInfoWindow != null)
                    return callInfoWindow.WindowLeftMargin;
                return 0;
            }
            set
            {
                if (callInfoWindow != null)
                    callInfoWindow.WindowLeftMargin = value;
            }
        }

        public double CallInfoWindowTopMargin
        {
            get
            {
                if (callInfoWindow != null)
                    return callInfoWindow.WindowTopMargin;
                return 0;
            }
            set
            {
                if (callInfoWindow != null)
                    callInfoWindow.WindowTopMargin = value;
            }
        }

        public int CallInfoOverlayWidth
        {
            get
            {
                if (callInfoWindow != null)
                    return callInfoWindow.OverlayWidth;
                return 0;
            }
            set
            {
                if (callInfoWindow != null)
                    callInfoWindow.OverlayWidth = value;
            }
        }

        public int CallInfoOverlayHeight
        {
            get
            {
                if (callInfoWindow != null)
                    return callInfoWindow.OverlayHeight;
                return 0;
            }
            set
            {
                if (callInfoWindow != null)
                    callInfoWindow.OverlayHeight = value;
            }
        }

        public int ForegroundCallDuration
        {
            get { return _foregroundCallDuration; }
            set { _foregroundCallDuration = value; }
        }

        public void SetCallerInfo(string callerInfo)
        {
            var textBlock =
                FindChild<TextBlock>(callInfoWindow.TransparentWindow, "CallerInfoLabel");


            
            if (textBlock != null)
            {
                if (textBlock.Text != null && textBlock.Text != "Test Call" && textBlock.Text != "")
                {

                    if (textBlock.Text != callerInfo)
                    {
                        //if(_linphoneService.GetActiveCallsCount == 2)
                        SetCallSwap(textBlock.Text, callerInfo);
                    }
                    else
                    {
                        var textBlock1 =
              FindChild<TextBlock>(callInfoWindow.TransparentWindow, "CallSwapLabel");
                        if (textBlock1 != null)
                        {
                            textBlock1.Text = "";
                            textBlock1.Visibility = Visibility.Hidden;
                        }
                    }
                    
                   
                }
                textBlock.Text = callerInfo;

            }
                

            //if (textBlock != null)
            //    textBlock.Text = "Hello " + callerInfo;



        }

        public void SetCallState(string callState)
        {
            var textBlock =
                FindChild<TextBlock>(callInfoWindow.TransparentWindow, "CallStateLabel");
            if (textBlock != null)
            {
                textBlock.Text = callState;
                ShowOnHoldWindow(callState.Equals("On Hold"));
            }
        }

        public void StartCallTimer(int duration)
        {
            //  Added 4/5/2017 MITRE-fjr
            //  Moved the Timer creation here
            _timerCall = new System.Timers.Timer
            {
                Interval = 1000,
                AutoReset = true
            };
            _timerCall.Elapsed += OnUpdatecallTimer;
            // End Add

            _foregroundCallDuration = duration;
            if (_timerCall != null && !_timerCall.Enabled)
                _timerCall.Start();
            UpdateCallDuration();
        }

        public void StopCallTimer()
        {
            _foregroundCallDuration = 0;
            if (_timerCall != null && _timerCall.Enabled)
            {
                //  Added 4/5/2017 MITRE-fjr
                _timerCall.Enabled = false;
                _timerCall.Stop();
                //  Added 4/5/2017 MITRE-fjr
                _timerCall.Dispose();
                                
                var textBlock =
                FindChild<TextBlock>(callInfoWindow.TransparentWindow, "CallerInfoLabel");
                if (textBlock != null)
                {
                    textBlock.Text = "";
                }
            }
        }


        private void SetCallSwap(string oldCall, string newCall)
        {
            var textBlock =
               FindChild<TextBlock>(callInfoWindow.TransparentWindow, "CallSwapLabel");
            if (textBlock != null)
            {
                textBlock.Visibility = Visibility.Visible;
                textBlock.Text = "Call Swapped from " + oldCall +  " to " +  newCall;
                Console.WriteLine(_foregroundCallDuration);
                hideSwapLabel = true;
                durationHideLabel = _foregroundCallDuration + 10;
            }
        }
        private void OnUpdatecallTimer(object sender, ElapsedEventArgs e)
        {

            //************************************************************************************************************************************
            // Update Call timer (Duration of Call)
            //************************************************************************************************************************************
            if (callInfoWindow.TransparentWindow.Dispatcher != null)
            {
                if (callInfoWindow.TransparentWindow.Dispatcher.Thread != Thread.CurrentThread)
                {
                    callInfoWindow.TransparentWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        new EventHandler<ElapsedEventArgs>(OnUpdatecallTimer), sender, new object[] {e});
                    return;
                }

                _foregroundCallDuration++;
                UpdateCallDuration();

                if (callsSwitchWindow.ShowWindow)
                {
                    _backgroundCallDuration++;
                    UpdatebackgroundCallDuration();
                }

                //  Commented out 4/5/2017 MITRE-fjr
                //  This seemed to be causing multiple resets 
                //_timerCall.Start();
            }
        }

        private void UpdateCallDuration()
        {
            var str = string.Empty;

            Console.WriteLine("UpdateCallDuration: " + _foregroundCallDuration);

            if (_foregroundCallDuration > 3599)
                str = string.Format("{0:D2}:{1:D2}:{2:D2}", _foregroundCallDuration/3600,
                    (_foregroundCallDuration/60)%60, _foregroundCallDuration%60);
            else
                str = string.Format("{0:D2}:{1:D2}", _foregroundCallDuration/60, _foregroundCallDuration%60);
            var textBlock =
                FindChild<TextBlock>(callInfoWindow.TransparentWindow, "CallDurationLabel");
            if (textBlock != null)
            {
                textBlock.Text =   str;
            }

            if (ForegroundCallDuration > durationHideLabel)
            {
                var textBlock1 =
               FindChild<TextBlock>(callInfoWindow.TransparentWindow, "CallSwapLabel");
                if (textBlock1 != null && hideSwapLabel)
                {
                    textBlock1.Visibility = Visibility.Hidden;
                    hideSwapLabel = false;


                }
            }
        }

        public void ShowCallInfoWindow(bool bshow)
        {
            callInfoWindow.ShowWindow = bshow;
            callInfoWindow.Refresh();
            if (bshow)
                callInfoWindow.UpdateWindow();
        }

        public object OverlayCallInfoChild
        {
            get
            {
                if (callInfoWindow != null && callInfoWindow.TransparentWindow != null)
                {
                    return callInfoWindow.TransparentWindow.Content;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (callInfoWindow != null && callInfoWindow.TransparentWindow != null)
                {
                    callInfoWindow.TransparentWindow.Content = value;
                }
            }
        }

        #endregion

        #region onHoldWindow

        public double OnHoldWindowLeftMargin
        {
            get
            {
                if (onHoldWindow != null)
                    return onHoldWindow.WindowLeftMargin;
                return 0;
            }
            set
            {
                if (onHoldWindow != null)
                    onHoldWindow.WindowLeftMargin = value;
            }
        }

        public double OnHoldWindowTopMargin
        {
            get
            {
                if (onHoldWindow != null)
                    return onHoldWindow.WindowTopMargin;
                return 0;
            }
            set
            {
                if (onHoldWindow != null)
                    onHoldWindow.WindowTopMargin = value;
            }
        }

        public int OnHoldOverlayWidth
        {
            get
            {
                if (onHoldWindow != null)
                    return onHoldWindow.OverlayWidth;
                return 0;
            }
            set
            {
                if (onHoldWindow != null)
                    onHoldWindow.OverlayWidth = value;
            }
        }

        public int OnHoldOverlayHeight
        {
            get
            {
                if (onHoldWindow != null)
                    return onHoldWindow.OverlayHeight;
                return 0;
            }
            set
            {
                if (onHoldWindow != null)
                    onHoldWindow.OverlayHeight = value;
            }
        }

        public void ShowOnHoldWindow(bool bshow)
        {
            onHoldWindow.ShowWindow = bshow;
            onHoldWindow.Refresh();
            if (bshow)
            {
                onHoldWindow.UpdateWindow();
            }
        }

        public object OverlayOnHoldChild
        {
            get
            {
                if (onHoldWindow != null && onHoldWindow.TransparentWindow != null)
                {
                    return onHoldWindow.TransparentWindow.Content;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (onHoldWindow != null && onHoldWindow.TransparentWindow != null)
                {
                    onHoldWindow.TransparentWindow.Content = value;
                }
            }
        }

        #endregion

        #region CallsSwitch window

        public double CallsSwitchWindowLeftMargin
        {
            get
            {
                if (callsSwitchWindow != null)
                    return callsSwitchWindow.WindowLeftMargin;
                return 0;
            }
            set
            {
                if (callsSwitchWindow != null)
                    callsSwitchWindow.WindowLeftMargin = value;
            }
        }

        public double CallsSwitchWindowTopMargin
        {
            get
            {
                if (callsSwitchWindow != null)
                    return callsSwitchWindow.WindowTopMargin;
                return 0;
            }
            set
            {
                if (callsSwitchWindow != null)
                    callsSwitchWindow.WindowTopMargin = value;
            }
        }

        public int CallsSwitchOverlayWidth
        {
            get
            {
                if (callsSwitchWindow != null)
                    return callsSwitchWindow.OverlayWidth;
                return 0;
            }
            set
            {
                if (callsSwitchWindow != null)
                    callsSwitchWindow.OverlayWidth = value;
            }
        }

        public int CallsSwitchOverlayHeight
        {
            get
            {
                if (callsSwitchWindow != null)
                    return callsSwitchWindow.OverlayHeight;
                return 0;
            }
            set
            {
                if (callsSwitchWindow != null)
                    callsSwitchWindow.OverlayHeight = value;
            }
        }

        public int BackgroundCallDuration
        {
            get { return _backgroundCallDuration; }
            set { _backgroundCallDuration = value; }
        }

        private void UpdatebackgroundCallDuration()
        {
            var textBlock =
                FindChild<TextBlock>(callsSwitchWindow.TransparentWindow, "PausedCallDurationLabel");
            if (textBlock != null)
            {
                var str = string.Empty;

                if (_backgroundCallDuration > 3599)
                    str = string.Format("{0:D2}:{1:D2}:{2:D2}", _backgroundCallDuration/3600,
                        (_backgroundCallDuration/60)%60, _backgroundCallDuration%60);
                else
                    str = string.Format("{0:D2}:{1:D2}", _backgroundCallDuration/60, _backgroundCallDuration%60);
                textBlock.Text = str;
            }
        }

        public void SetPausedCallerInfo(string callerInfo)
        {
            var textBlock =
                FindChild<TextBlock>(callsSwitchWindow.TransparentWindow, "PausedCallerInfoLabel");
            if (textBlock != null)
                textBlock.Text = callerInfo;
        }

        public void SetPausedCallState(string callState)
        {
            var textBlock =
                FindChild<TextBlock>(callsSwitchWindow.TransparentWindow, "PausedCallStateLabel");
            if (textBlock != null)
                textBlock.Text = callState;
        }

        public void StartPausedCallTimer(int duration)
        {
            _backgroundCallDuration = duration;
            UpdatebackgroundCallDuration();
        }

        public void StopPausedCallTimer()
        {
            _backgroundCallDuration = 0;
        }

        public void ShowCallsSwitchWindow(bool bshow)
        {
            callsSwitchWindow.ShowWindow = bshow;
            callsSwitchWindow.Refresh();
            callsSwitchWindow.UpdateWindow();
        }

        public object OverlayCallsSwitchChild
        {
            get
            {
                if (callsSwitchWindow != null && callsSwitchWindow.TransparentWindow != null)
                {
                    return callsSwitchWindow.TransparentWindow.Content;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (callsSwitchWindow != null && callsSwitchWindow.TransparentWindow != null)
                {
                    callsSwitchWindow.TransparentWindow.Content = value;
                }
            }
        }

        #endregion

        #region New Call window

        public double NewCallAcceptWindowLeftMargin
        {
            get
            {
                if (newCallAcceptWindow != null)
                    return newCallAcceptWindow.WindowLeftMargin;
                return 0;
            }
            set
            {
                if (newCallAcceptWindow != null)
                    newCallAcceptWindow.WindowLeftMargin = value;
            }
        }

        public double NewCallAcceptWindowTopMargin
        {
            get
            {
                if (newCallAcceptWindow != null)
                    return newCallAcceptWindow.WindowTopMargin;
                return 0;
            }
            set
            {
                if (newCallAcceptWindow != null)
                    newCallAcceptWindow.WindowTopMargin = value;
            }
        }

        public int NewCallAcceptOverlayWidth
        {
            get
            {
                if (newCallAcceptWindow != null)
                    return newCallAcceptWindow.OverlayWidth;
                return 0;
            }
            set
            {
                if (newCallAcceptWindow != null)
                    newCallAcceptWindow.OverlayWidth = value;
            }
        }

        public int NewCallAcceptOverlayHeight
        {
            get
            {
                if (newCallAcceptWindow != null)
                    return newCallAcceptWindow.OverlayHeight;
                return 0;
            }
            set
            {
                if (newCallAcceptWindow != null)
                    newCallAcceptWindow.OverlayHeight = value;
            }
        }

        public void SetNewCallerInfo(string callerInfo)
        {
            var textBlock =
                FindChild<TextBlock>(newCallAcceptWindow.TransparentWindow, "NewCallerInfoLabel");
            if (textBlock != null)
                textBlock.Text = callerInfo;
        }

        public void ShowNewCallAcceptWindow(bool bshow)
        {
            newCallAcceptWindow.ShowWindow = bshow;
            newCallAcceptWindow.Refresh();
            if (bshow)
                newCallAcceptWindow.UpdateWindow();
        }


        public object OverlayNewCallSwap
        {
            get
            {
                return null;
            }
            set
            {

            }
        }
        public object OverlayNewCallAcceptChild
        {
            get
            {
                if (newCallAcceptWindow != null && newCallAcceptWindow.TransparentWindow != null)
                {
                    return newCallAcceptWindow.TransparentWindow.Content;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (newCallAcceptWindow != null && newCallAcceptWindow.TransparentWindow != null)
                {
                    newCallAcceptWindow.TransparentWindow.Content = value;
                }
            }
        }

        public VATRPTranslucentWindow CommandBarWindow
        {
            get { return commandBarWindow; }
        }

        public VATRPTranslucentWindow NumpadWindow
        {
            get { return numpadWindow; }
        }

        public VATRPTranslucentWindow CallInfoWindow
        {
            get { return callInfoWindow; }
        }

        public VATRPTranslucentWindow CallsSwitchWindow
        {
            get { return callsSwitchWindow; }
        }

        public VATRPTranslucentWindow NewCallAcceptWindow
        {
            get { return newCallAcceptWindow; }
        }

        public VATRPTranslucentWindow OnHoldWindow
        {
            get { return onHoldWindow; }
        }

        #endregion

        #region Quality indicator window

        public double QualityIndicatorWindowLeftMargin
        {
            get
            {
                if (qualityIndicatoWindow != null)
                    return qualityIndicatoWindow.WindowLeftMargin;
                return 0;
            }
            set
            {
                if (qualityIndicatoWindow != null)
                    qualityIndicatoWindow.WindowLeftMargin = value;
            }
        }

        public double QualityIndicatorWindowTopMargin
        {
            get
            {
                if (qualityIndicatoWindow != null)
                    return qualityIndicatoWindow.WindowTopMargin;
                return 0;
            }
            set
            {
                if (qualityIndicatoWindow != null)
                    qualityIndicatoWindow.WindowTopMargin = value;
            }
        }

        public int QualityIndicatorOverlayWidth
        {
            get
            {
                if (qualityIndicatoWindow != null)
                    return qualityIndicatoWindow.OverlayWidth;
                return 0;
            }
            set
            {
                if (qualityIndicatoWindow != null)
                    qualityIndicatoWindow.OverlayWidth = value;
            }
        }

        public int QualityIndicatorOverlayHeight
        {
            get
            {
                if (qualityIndicatoWindow != null)
                    return qualityIndicatoWindow.OverlayHeight;
                return 0;
            }
            set
            {
                if (qualityIndicatoWindow != null)
                    qualityIndicatoWindow.OverlayHeight = value;
            }
        }

        public void UpdateQualityIndicator(QualityIndicator currentQuality)
        {
            if (_lastQuality != currentQuality)
            {
                var currentIndicator = "MediumIndicator";
                var lastIndicator = "MediumIndicator";
                switch (_lastQuality)
                {
                    case QualityIndicator.ToBad:
                        lastIndicator = "BadIndicator";
                        break;
                    case QualityIndicator.VeryPoor:
                    case QualityIndicator.Poor:
                        lastIndicator = "PoorIndicator";
                        break;
                    case QualityIndicator.Medium:
                        lastIndicator = "MediumIndicator";
                        break;
                    case QualityIndicator.Good:
                        lastIndicator = "GoodIndicator";
                        break;
                    default:
                        lastIndicator = "None";
                        break;
                }

                switch (currentQuality)
                {
                    case QualityIndicator.ToBad:
                        currentIndicator = "BadIndicator";
                        break;
                    case QualityIndicator.VeryPoor:
                    case QualityIndicator.Poor:
                        currentIndicator = "PoorIndicator";
                        break;
                    case QualityIndicator.Medium:
                        currentIndicator = "MediumIndicator";
                        break;
                    case QualityIndicator.Good:
                        currentIndicator = "GoodIndicator";
                        break;
                    default:
                        return;
                }

                _lastQuality = currentQuality;
                var container =
                    FindChild<Grid>(qualityIndicatoWindow.TransparentWindow, "QualityContainer");
                if (container != null)
                {
                    var image =
                        FindChild<Image>(container, lastIndicator);
                    if (image != null)
                    {
                        image.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        image =
                            FindChild<Image>(container, "BadIndicator");

                        if (image != null)
                            image.Visibility = Visibility.Collapsed;
                        image =
                            FindChild<Image>(container, "PoorIndicator");
                        if (image != null)
                            image.Visibility = Visibility.Collapsed;
                        image =
                            FindChild<Image>(container, "MediumIndicator");
                        if (image != null)
                            image.Visibility = Visibility.Collapsed;
                        image =
                            FindChild<Image>(container, "GoodIndicator");
                        if (image != null)
                            image.Visibility = Visibility.Collapsed;
                    }

                    image =
                        FindChild<Image>(container, currentIndicator);
                    if (image != null)
                        image.Visibility = Visibility.Visible;
                }
            }
            qualityIndicatoWindow.Refresh();
            qualityIndicatoWindow.UpdateWindow();
        }

        public void ShowQualityIndicatorWindow(bool bshow)
        {
            qualityIndicatoWindow.ShowWindow = bshow;
            qualityIndicatoWindow.Refresh();
            if (bshow)
            {
                qualityIndicatoWindow.UpdateWindow();
            }
            else
            {
                _lastQuality = QualityIndicator.Unknown;
            }
        }

        public object OverlayQualityIndicatorChild
        {
            get
            {
                if (qualityIndicatoWindow != null && qualityIndicatoWindow.TransparentWindow != null)
                {
                    return qualityIndicatoWindow.TransparentWindow.Content;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (qualityIndicatoWindow != null && qualityIndicatoWindow.TransparentWindow != null)
                {
                    qualityIndicatoWindow.TransparentWindow.Content = value;
                }
            }
        }

        public VATRPTranslucentWindow QualityIndicatorWindow
        {
            get { return onHoldWindow; }
        }

        #endregion

        #region Media encryption indicator window

        public double EncryptionIndicatorWindowLeftMargin
        {
            get
            {
                if (encryptionIndicatoWindow != null)
                    return encryptionIndicatoWindow.WindowLeftMargin;
                return 0;
            }
            set
            {
                if (encryptionIndicatoWindow != null)
                    encryptionIndicatoWindow.WindowLeftMargin = value;
            }
        }

        public double EncryptionIndicatorWindowTopMargin
        {
            get
            {
                if (encryptionIndicatoWindow != null)
                    return encryptionIndicatoWindow.WindowTopMargin;
                return 0;
            }
            set
            {
                if (encryptionIndicatoWindow != null)
                    encryptionIndicatoWindow.WindowTopMargin = value;
            }
        }

        public int EncryptionIndicatorOverlayWidth
        {
            get
            {
                if (encryptionIndicatoWindow != null)
                    return encryptionIndicatoWindow.OverlayWidth;
                return 0;
            }
            set
            {
                if (encryptionIndicatoWindow != null)
                    encryptionIndicatoWindow.OverlayWidth = value;
            }
        }

        public int EncryptionIndicatorOverlayHeight
        {
            get
            {
                if (encryptionIndicatoWindow != null)
                    return encryptionIndicatoWindow.OverlayHeight;
                return 0;
            }
            set
            {
                if (encryptionIndicatoWindow != null)
                    encryptionIndicatoWindow.OverlayHeight = value;
            }
        }

        public void UpdateEncryptionIndicator(MediaEncryptionIndicator currentEncryption)
        {
            if (_lastEncryption != currentEncryption)
            {
                var currentIndicator = "EncryptionOff";
                var lastIndicator = "EncryptionOff";
                switch (_lastEncryption)
                {
                    case MediaEncryptionIndicator.On:
                        lastIndicator = "EncryptionOn";
                        break;
                    case MediaEncryptionIndicator.Off:
                        lastIndicator = "EncryptionOff";
                        break;
                    default:
                        lastIndicator = "None";
                        break;
                }

                switch (currentEncryption)
                {
                    case MediaEncryptionIndicator.On:
                        currentIndicator = "EncryptionOn";
                        break;
                    case MediaEncryptionIndicator.Off:
                        currentIndicator = "EncryptionOff";
                        break;
                    default:
                        return;
                }

                _lastEncryption = currentEncryption;
                var container =
                    FindChild<Grid>(encryptionIndicatoWindow.TransparentWindow, "MediaEncryptionContainer");
                if (container != null)
                {
                    var image =
                        FindChild<Image>(container, lastIndicator);
                    if (image != null)
                    {
                        image.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        image =
                            FindChild<Image>(container, "EncryptionOn");

                        if (image != null)
                            image.Visibility = Visibility.Collapsed;
                        image =
                            FindChild<Image>(container, "EncryptionOff");
                        if (image != null)
                            image.Visibility = Visibility.Collapsed;
                    }

                    image =
                        FindChild<Image>(container, currentIndicator);
                    if (image != null)
                        image.Visibility = Visibility.Visible;
                }
            }
            encryptionIndicatoWindow.Refresh();
            encryptionIndicatoWindow.UpdateWindow();
        }

        public void ShowEncryptionIndicatorWindow(bool bshow)
        {
            encryptionIndicatoWindow.ShowWindow = bshow;
            encryptionIndicatoWindow.Refresh();
            if (bshow)
            {
                encryptionIndicatoWindow.UpdateWindow();
            }
            else
            {
                _lastEncryption = MediaEncryptionIndicator.None;
            }
        }

        public object OverlayEncryptionIndicatorChild
        {
            get
            {
                if (encryptionIndicatoWindow != null && encryptionIndicatoWindow.TransparentWindow != null)
                {
                    return encryptionIndicatoWindow.TransparentWindow.Content;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (encryptionIndicatoWindow != null && encryptionIndicatoWindow.TransparentWindow != null)
                {
                    encryptionIndicatoWindow.TransparentWindow.Content = value;
                }
            }
        }

        public VATRPTranslucentWindow EncryptionIndicatorWindow
        {
            get { return onHoldWindow; }
        }

        #endregion
        
        #region Decline message window

        public double InfoMsgWindowLeftMargin
        {
            get
            {
                if (infoMsgWindow != null)
                    return infoMsgWindow.WindowLeftMargin;
                return 0;
            }
            set
            {
                if (infoMsgWindow != null)
                    infoMsgWindow.WindowLeftMargin = value;
            }
        }

        public double InfoMsgWindowTopMargin
        {
            get
            {
                if (infoMsgWindow != null)
                    return infoMsgWindow.WindowTopMargin;
                return 0;
            }
            set
            {
                if (infoMsgWindow != null)
                    infoMsgWindow.WindowTopMargin = value;
            }
        }

        public int InfoMsgOverlayWidth
        {
            get
            {
                if (infoMsgWindow != null)
                    return infoMsgWindow.OverlayWidth;
                return 0;
            }
            set
            {
                if (infoMsgWindow != null)
                    infoMsgWindow.OverlayWidth = value;
            }
        }

        public int InfoMsgOverlayHeight
        {
            get
            {
                if (infoMsgWindow != null)
                    return infoMsgWindow.OverlayHeight;
                return 0;
            }
            set
            {
                if (infoMsgWindow != null)
                    infoMsgWindow.OverlayHeight = value;
            }
        }

        public void UpdateInfoMsg(string header, string info)
        {
                var container =
                    FindChild<Grid>(infoMsgWindow.TransparentWindow, "InfoMsgContainer");
                if (container != null)
                {
                    var textBlock = FindChild<TextBlock>(container, "MsgHeader");
                    if (textBlock != null)
                        textBlock.Text = header;

                    textBlock = FindChild<TextBlock>(container, "Info");
                    if (textBlock != null)
                        textBlock.Text = info;
                }
            infoMsgWindow.Refresh();
            infoMsgWindow.UpdateWindow();
        }

        public void ShowInfoMsgWindow(bool bshow)
        {
            infoMsgWindow.ShowWindow = bshow;
            infoMsgWindow.Refresh();
            if (bshow)
            {
                infoMsgWindow.UpdateWindow();
            }
        }

        public object OverlayInfoMsgChild
        {
            get
            {
                if (infoMsgWindow != null && infoMsgWindow.TransparentWindow != null)
                {
                    return infoMsgWindow.TransparentWindow.Content;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (infoMsgWindow != null && infoMsgWindow.TransparentWindow != null)
                {
                    infoMsgWindow.TransparentWindow.Content = value;
                }
            }
        }

        public VATRPTranslucentWindow InfoMsgWindow
        {
            get { return onHoldWindow; }
        }

        #endregion
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            commandBarWindow.UpdateWindow();
            numpadWindow.UpdateWindow();
            callInfoWindow.UpdateWindow();
            callsSwitchWindow.UpdateWindow();
            newCallAcceptWindow.UpdateWindow();
            qualityIndicatoWindow.Refresh();
            encryptionIndicatoWindow.Refresh();
            infoMsgWindow.Refresh();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            Refresh();
        }

        public void Refresh()
        {
            commandBarWindow.Refresh();
            numpadWindow.Refresh();
            callInfoWindow.Refresh();
            callsSwitchWindow.Refresh();
            newCallAcceptWindow.Refresh();
            qualityIndicatoWindow.Refresh();
            encryptionIndicatoWindow.Refresh();
            infoMsgWindow.Refresh();
        }

        private static T FindChild<T>(DependencyObject parent, string childName)
            where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T) child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T) child;
                    break;
                }
            }

            return foundChild;
        }
    }
}
