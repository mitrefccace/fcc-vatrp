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
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.ViewModel;
using VATRP.Core.Extensions;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.Views
{
    /// <summary>
    /// Interaction logic for MediaTextWindow.xaml
    /// </summary>
    public partial class MediaTextWindow
    {
        #region Members

        private SimpleMessagingViewModel _viewModel;

        public delegate void MakeCallRequestedDelegate(string called_address);
        public event MakeCallRequestedDelegate MakeCallRequested;
        #endregion

        public MediaTextWindow(SimpleMessagingViewModel vm)
            : base(VATRPWindowType.MESSAGE_VIEW)
        {
            _viewModel = vm;
            DataContext = vm;
            InitializeComponent();
            ServiceManager.Instance.ChatService.ConversationUpdated += ChatManagerOnConversationUpdated;
        }

        private void ChatManagerOnConversationUpdated(object sender, VATRP.Core.Events.ConversationUpdatedEventArgs e)
        {
            ScrollToEnd();
        }

        private void ScrollToEnd()
        {
            if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke((Action)(ScrollToEnd));
                return;
            }

            MessageListView.SelectedIndex = MessageListView.Items.Count - 1;
            MessageListView.ScrollIntoView(MessageListView.SelectedItem);
        }

        private void OnChatSelected(object sender, SelectionChangedEventArgs e)
        {
            var contactModel = ContactsList.SelectedItem as ContactViewModel;

            if (contactModel != null)
            {
                _viewModel.SetActiveChatContact(contactModel.Contact, IntPtr.Zero);
                if (contactModel.Contact != null)
                    _viewModel.ReceiverAddress = contactModel.Contact.RegistrationName;
                ScrollToEnd();
            }
        }

        private void OnSendButtonClicked(object sender, RoutedEventArgs e)
        {
            //****************************************************************************************
            // Send button pressed from chat window.
            //*****************************************************************************************
            SendSimpleMessage();
        }

        private void SendSimpleMessage()
        {
            //****************************************************************************************
            // Sending a message by Chat window.
            //*****************************************************************************************
            if (_viewModel.CheckReceiverContact())
            {
                var contactModel = ContactsList.SelectedItem as ContactViewModel;
                if (contactModel != null && contactModel.Contact != _viewModel.ChatViewContact.Contact)
                    ContactsList.SelectedItem = _viewModel.ChatViewContact;

                _viewModel.SendSimpleMessage(_viewModel.MessageText);
            }
            else
            {
                MessageBox.Show("Can't send message to " + _viewModel.ReceiverAddress, "VATRP", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Return:
                    SendSimpleMessage();
                    break;
            }
        }

        private void OnCallClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null && _viewModel.Chat != null && _viewModel.Chat.Contact != null)
                if (MakeCallRequested != null)
                    MakeCallRequested(_viewModel.Chat.Contact.RegistrationName);
        }
    }
}
