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
using System.Windows.Controls;
using System.Windows.Input;
using log4net;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.ViewModel;
using VATRP.Core.Extensions;
using VATRP.Core.Interfaces;

namespace com.vtcsecure.ace.windows.CustomControls
{
    /// <summary>
    /// Interaction logic for DialPad.xaml
    /// </summary>
    public partial class DialPadScreen 
    {
        #region Members
        private static readonly ILog LOG = LogManager.GetLogger(typeof(DialPadScreen));
        private DialpadViewModel _viewModel;
        public event EventHandler<KeyPadEventArgs> KeypadPressed;
        private bool plusButtonHold;
        private System.Windows.Forms.Timer timerHold;
        public delegate void MakeCallRequestedDelegate(string called_address);

        public event MakeCallRequestedDelegate MakeCallRequested;
        #endregion

        public DialPadScreen()
        {

            //*****************************************************************************************************************
            // When DialPad screen Initilize.
            //*****************************************************************************************************************
            InitializeComponent();
            timerHold = new System.Windows.Forms.Timer()
            {
                Interval = 300
            };
            timerHold.Tick += TimerHoldOnTick;
        }

        public void SetViewModel(DialpadViewModel viewModel)
        {
            DataContext = viewModel;
            _viewModel = viewModel;
            //_viewModel.RemotePartyNumber = "Enter Number or User Name";

        }

        #region KeyPad

        private void buttonKeyPad_Click(object sender, RoutedEventArgs e)
        {

            //************************************* DialPad key press event ****************************************************
            // This event will called when dial button (1-9) will tapped for dial pad.
            //******************************************************************************************************************

            RemoveIfPlaceHolderTextExist();
            int oldNumberLendth = _viewModel.RemotePartyNumber.Length;
            var key = DialpadKey.DialpadKey_KeyNone;

            if (Equals(e.OriginalSource, buttonKeyPad0))  // 0 Number tapped
            {
                if (plusButtonHold)
                {
                    plusButtonHold = false;
                    return;
                }

                key = DialpadKey.DialpadKey_Key0;
            }
            else if (Equals(e.OriginalSource, buttonKeyPad1)) // 1 Number tapped
            {
                key = DialpadKey.DialpadKey_Key1;
            }
            else if (Equals(e.OriginalSource, buttonKeyPad2))  // 2 Number tapped
            {
                key = DialpadKey.DialpadKey_Key2;
            }
            else if (Equals(e.OriginalSource, buttonKeyPad3))  // 3 Number tapped
            {
                key = DialpadKey.DialpadKey_Key3;
            }
            else if (Equals(e.OriginalSource, buttonKeyPad4))  // 4 Number tapped
            {
                key = DialpadKey.DialpadKey_Key4;
            }
            else if (Equals(e.OriginalSource, buttonKeyPad5))  // 5 Number tapped
            {
                key = DialpadKey.DialpadKey_Key5;
            }
            else if (Equals(e.OriginalSource, buttonKeyPad6))  // 6 Number tapped
            {
                key = DialpadKey.DialpadKey_Key6;
            }
            else if (Equals(e.OriginalSource, buttonKeyPad7))  // 7 Number tapped
            {
                key = DialpadKey.DialpadKey_Key7;
            }
            else if (Equals(e.OriginalSource, buttonKeyPad8))  // 8 Number tapped
            {
                key = DialpadKey.DialpadKey_Key8;
            }
            else if (Equals(e.OriginalSource, buttonKeyPad9))  // 9 Number tapped
            {
                key = DialpadKey.DialpadKey_Key9;
            }
            else if (Equals(e.OriginalSource, buttonKeyPadStar))  // * Number tapped
            {
                key = DialpadKey.DialpadKey_KeyStar;
            }
            else if (Equals(e.OriginalSource, buttonKeyPadSharp))  // # Number tapped
            {
                key = DialpadKey.DialpadKey_KeyPound;
            }

            if (key != DialpadKey.DialpadKey_KeyNone)
            {
                _viewModel.RemotePartyNumber += Convert.ToChar(key);
                if (KeypadPressed != null)
                {
                    var args = new KeyPadEventArgs(key);
                    KeypadPressed(this, args);
                }
            }

        }

        #endregion
        
        private void VideoCallClick(object sender, RoutedEventArgs e)
        {
            //******************************** Call button clicked *************************************************
            // This event will called when Call button clicked
            //******************************************************************************************************

            if (string.IsNullOrWhiteSpace(_viewModel.RemotePartyNumber)) // If entered number is blank than it will show error
            {
                MessageBox.Show("Destination address is empty", "VATRP", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MakeCallRequested != null) 
                MakeCallRequested(_viewModel.RemotePartyNumber);
        }

        private void OnBackSpaceClicked(object sender, MouseButtonEventArgs e)
        {
            //******************************** Backspace button clicked *******************************************************************************************
            // This event will called when backspace button is tapped and it will remove the digit that is enter in last. 
            // It remove only when there is more than 0 characters or numbers.
            //*****************************************************************************************************************************************************

            //RemoveIfPlaceHolderTextExist();
            if (!String.IsNullOrEmpty(_viewModel.RemotePartyNumber))
            {
                _viewModel.RemotePartyNumber = _viewModel.RemotePartyNumber.Substring(0,
                    _viewModel.RemotePartyNumber.Length - 1);

                if (_viewModel.RemotePartyNumber == "")
                {
                    this.RemotePartyNumberPlaceHolder.Visibility = Visibility.Visible;
                }

            }
            else
            {
                this.RemotePartyNumberPlaceHolder.Visibility = Visibility.Visible;
            }


           
        }

        private void OnPlusPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (timerHold != null)
                timerHold.Enabled = false;
        }

        private void TimerHoldOnTick(object sender, EventArgs e)
        {
            timerHold.Stop();
            _viewModel.RemotePartyNumber += "+";
            plusButtonHold = true;
        }

        private void OnPlusPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (timerHold != null)
                timerHold.Enabled = true;
        }

        private void OnDialpadPreviewKeyup(object sender, KeyEventArgs e)
        {
            //***********************************************************************************************************************************************
            // When DialPad is visible over Call Window and any key pressed from keyboard then this event will called. Key may be numeric or char.
            //***********************************************************************************************************************************************

            if (_viewModel.RemotePartyNumber.Length > 0)
            {
                RemoveIfPlaceHolderTextExist();
            }
            else
            {
                this.RemotePartyNumberPlaceHolder.Visibility = Visibility.Visible;

            }

            if (e.Key != Key.Enter)
                return;
            try
            {
                var remote = _viewModel.RemotePartyNumber.Trim();
                if (remote.NotBlank())
                {
                    if (App.CurrentAccount != null)
                    {
                        remote = string.Format("sip:{0}@{1}", remote, App.CurrentAccount.ProxyHostname);
                        if (MediaActionHandler.MakeVideoCall(remote))
                            _viewModel.RemotePartyNumber = "";
                        e.Handled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ServiceManager.LogError("OnDialString", ex);
            }
        }

        private void OnDialpadPreviewKeydown(object sender, KeyEventArgs e)
       {
            //**********************************************************************************************
            // This method will called when any Keypress in Dialpad from System keyboard.
            //**********************************************************************************************
          
            

            if (e.Key == Key.Enter)
                e.Handled = true; // prevent further processing
        }


        /// <summary>
        /// *****************************************************************
        /// ADDED THESE METHODS BY MK ON DATED 10-NOV-2016 FOR HIDE/SHOW REMOTE PARTY NUMBER
        /// </summary>
        public void RemoveIfPlaceHolderTextExist()
        {

            this.RemotePartyNumberPlaceHolder.Visibility = Visibility.Hidden;
            
        }

        public void ShowPlaceHolderText()
        {

            this.RemotePartyNumberPlaceHolder.Visibility = Visibility.Visible;

        }


        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_viewModel.RemotePartyNumber == string.Empty)
            {
                this.RemotePartyNumberPlaceHolder.Visibility = Visibility.Visible;
            }
        }
        //**************************************************************************
    }
}
