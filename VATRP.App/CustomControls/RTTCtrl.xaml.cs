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
using System.Threading;
using System.Windows.Threading;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.CustomControls
{
    /// <summary>
    /// Interaction logic for RTTCtrl.xaml
    /// </summary>
    public partial class RTTCtrl : UserControl
    {
        #region Members

        private string pasteText = string.Empty;
        private InCallMessagingViewModel _viewModel;
        private readonly DispatcherTimer pasteHandlerTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMilliseconds(20),
        };

        private string msgTextNew = string.Empty;
        #endregion

        public RTTCtrl()
        {

            //***************************************************************************************************************
            // Initilize of RTTCtrl view. which was displaying on the right side of call window.
            //***************************************************************************************************************
            InitializeComponent();
            //****************COMMENTED BY MK ON DATED 24-OCT-2016 FOR DISABLE PASTE HANDLER IN RTT
            pasteHandlerTimer.Tick += OnCheckPastedText;
            DataObject.AddPastingHandler(MessageTextBox, PasteHandler);
            //***************************************************************************************
        }

        private void OnCheckPastedText(object sender, EventArgs e)
        {
            pasteHandlerTimer.Stop();
            if (!ServiceManager.Instance.IsRttAvailable || !_viewModel.IsSendingModeRTT)
            {
                if (_viewModel.MessageText == "\r")
                {
                    _viewModel.SendMessage(_viewModel.MessageText);
                }
                else
                {
                    string msgText = MessageTextBox.Text;
                    if (msgText.Length > 199)
                    {
                        _viewModel.SendMessage(msgText.Substring(0, 200));
                        _viewModel.MessageText = msgText.Remove(0, 200);
                        MessageTextBox.CaretIndex = _viewModel.MessageText.Length;
                    }
                }
            }
            else
            {
                _viewModel.EnqueueInput(pasteText);
              
               
            }
        }

        private void PasteHandler(object sender, DataObjectPastingEventArgs e)
        {
            TextBox tb = sender as TextBox;

            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                pasteText = e.DataObject.GetData(typeof(string)) as string;

                if (string.IsNullOrEmpty(pasteText))
                    return;

                if (pasteHandlerTimer.IsEnabled)
                    pasteHandlerTimer.Stop();
                pasteHandlerTimer.Start();
            }
        }

        public void SetViewModel(InCallMessagingViewModel viewModel)
        {

            //****************************************************************************************************************************
            // Set the call screen to display.
            //*****************************************************************************************************************************
            if (_viewModel != null && _viewModel.Equals(viewModel))
                return;

            if (_viewModel != null && _viewModel != viewModel)
            {
                _viewModel.ConversationUpdated -= OnConversationUpdated;
            }
            DataContext = viewModel;
            _viewModel = viewModel;
            if (_viewModel != null)
            {
                _viewModel.ConversationUpdated += OnConversationUpdated;
            }
        }

        private void OnConversationUpdated(object sender, EventArgs eventArgs)
        {
            //************************************************************************************************************************************
            // When Chat conversation is updated then it will scroll to end of chat so recent message should display.
            //************************************************************************************************************************************
            ScrollToEnd();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            
        }

       private void OnUnloaded(object sender, RoutedEventArgs e)
        {
           
        }

       private void ScrollToEnd()
       {
           //************************************************************************************************************************************
           // When Chat conversation is updated then it will scroll to end of chat so recent message should display.
           //************************************************************************************************************************************

           if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
           {
               ServiceManager.Instance.Dispatcher.BeginInvoke((Action)(ScrollToEnd));
               return;
           }

           MessageListView.SelectedIndex = MessageListView.Items.Count - 1;
           var item = MessageListView.SelectedItem as VATRPChatMessage;
           if (item != null)
           {
               MessageListView.ScrollIntoView(item);
           }
       }

        private void OnSendButtonClicked(object sender, RoutedEventArgs e)
        {
            //************************************************************************************************************************************
            // Send button clicked from Chat window, that is visible in the right side of Call.
            //************************************************************************************************************************************

            if (_viewModel != null)
            {
                if (!ServiceManager.Instance.IsRttAvailable /*|| !_viewModel.IsSendingModeRTT*/)
                {
                    _viewModel.SendMessage(_viewModel.MessageText);
                }
                else
                {
                    _viewModel.EnqueueInput("\r");
                }
            }


            //if (_viewModel != null)
            //{
            //    if (!ServiceManager.Instance.IsRttAvailable /*|| !_viewModel.IsSendingModeRTT*/)
            //    {
            //        _viewModel.SendMessage(_viewModel.MessageText);
            //    }
            //    else
            //    {
            //        _viewModel.EnqueueInput(_viewModel.MessageText); // By MK on dated 24-Oct-2016 for Disable RTT and send message to user on "Send" button click.
            //        _viewModel.EnqueueInput("\r");
            //    }
            //}
        }


        private void OnTextInput(object sender, TextCompositionEventArgs e)
        {
            if (_viewModel != null)
            {
                if (_viewModel.IsSendingModeRTT)
                    _viewModel.EnqueueInput(e.Text);
                else if (!string.IsNullOrEmpty(e.Text))
                {
                    if (e.Text == "\r")
                    {
                        _viewModel.SendMessage(_viewModel.MessageText);
                        _viewModel.MessageText = string.Empty;
                    }
                    else
                    {
                        if (_viewModel.MessageText.Length > 199)
                        {
                            _viewModel.SendMessage(_viewModel.MessageText.Substring(0, 200));
                            _viewModel.MessageText = _viewModel.MessageText.Remove(0, 200);
                            MessageTextBox.CaretIndex = _viewModel.MessageText.Length;
                        }
                    }
                }
            }
        }


        //private void OnTextInput(object sender, TextCompositionEventArgs e)
        //{
        //    //*********************************************************************************************
        //    // Code by MK for diable RTT and send message on enter
        //    //*********************************************************************************************
        //    //if (e.Text == "\r")
        //    //{
        //    //    _viewModel.EnqueueInput(_viewModel.MessageText + "\r");
        //    //    //_viewModel.SendMessage(_viewModel.MessageText + "\r");
        //    //    _viewModel.MessageText = string.Empty;

        //    //}
        //    //*******************************************************************************************************

        //    Console.WriteLine("OnTextInput MessageTextBox ***" + MessageTextBox.Text);
        //    //************************************************************************************************************************************
        //    // On Text Input in the Chat window. Chat window which is displaying in the right side of call window.
        //    //************************************************************************************************************************************
        //    if (_viewModel != null)
        //    {
        //        if (_viewModel.IsSendingModeRTT)
        //        {
        //              _viewModel.EnqueueInput(e.Text); // Commented by MK on dated 20-OCT-2016 for disable RTT message
        //        }
        //        else if (!string.IsNullOrEmpty(e.Text))
        //        {
        //            if (e.Text == "\r")
        //            {
        //                _viewModel.SendMessage(_viewModel.MessageText);
        //                _viewModel.MessageText = string.Empty;
        //            }
        //            else
        //            {
        //                if (_viewModel.MessageText.Length > 199)
        //                {
        //                    _viewModel.SendMessage(_viewModel.MessageText.Substring(0, 200));
        //                    _viewModel.MessageText = _viewModel.MessageText.Remove(0, 200);
        //                    MessageTextBox.CaretIndex = _viewModel.MessageText.Length;
        //                }
        //            }
        //        }
        //    }
        //}

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {

            //Console.WriteLine("OnPreviewKeyDown MessageTextBox ***" + MessageTextBox.Text);
           // return;
            //************************************************************************************************************************************
            // A key is pressed in the chat window which is visible in the right side of Call Window when call is running.
            //************************************************************************************************************************************
            if (MessageTextBox.CaretIndex != MessageTextBox.Text.Length)
                MessageTextBox.CaretIndex = MessageTextBox.Text.Length;

            Char inputKey = Char.MinValue;
            switch (e.Key)
            {
                case Key.None:
                    break;
                case Key.Back:
                    inputKey = '\b';
                    break;
                case Key.Tab:
                    inputKey = '\t';
                    break;
                case Key.LineFeed:
                    inputKey = '\n';
                    break;
                case Key.Clear:
                    break;
                case Key.Return:
                    inputKey = '\r';
                    break;
                case Key.Space:
                    inputKey = ' ';
                    break;
                default:
                    break;
            }

            Console.WriteLine("Message Text: " + _viewModel.MessageText);

            if (inputKey != Char.MinValue)
            {
                if (_viewModel != null)
                {
                    if (_viewModel.IsSendingModeRTT)
                    {
                        _viewModel.EnqueueInput(inputKey.ToString());
                       
                    }
                }
            }
        }

       

        

       

        
    }
        
   
}
