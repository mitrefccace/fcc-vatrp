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
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Data;
using System.Windows.Threading;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Events;
using VATRP.Core.Extensions;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using System.Text;
using VATRP.Core.Enums;


namespace com.vtcsecure.ace.windows.ViewModel
{
    public class SimpleMessagingViewModel : MessagingViewModel
    {

        #region Members

        private string _receiverAddress;
        private string _contactSearchCriteria;
        private ICollectionView contactsListView;
        
        #endregion

        #region Events

        public event EventHandler UnreadMessagesCountChanged;
        public event EventHandler<DeclineMessageArgs> DeclineMessageReceived;
        #endregion

        public SimpleMessagingViewModel()
        {
            Init();
        }

        public SimpleMessagingViewModel(IChatService chatMng, IContactsService contactsMng)
            : base(chatMng, contactsMng)
        {
            Init();
            _chatsManager.ConversationUnReadStateChanged += OnUnreadStateChanged;
            _chatsManager.ConversationDeclineMessageReceived += OnDeclineMessageReceived;
            _chatsManager.ConversationUpdated += OnChatRoomUpdated;

        }

        private void OnDeclineMessageReceived(object sender, DeclineMessageArgs args)
        {
            if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new EventHandler<DeclineMessageArgs>(OnDeclineMessageReceived) , sender, new object[] { args });
                return;
            }

            var newArgs = new DeclineMessageArgs(args.MessageHeader, args.DeclineMessage) {Sender = args.Sender};
            if (DeclineMessageReceived != null) 
                DeclineMessageReceived(sender, newArgs);
        }

        private void OnUnreadStateChanged(object sender, VATRP.Core.Events.ConversationEventArgs e)
        {
            if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new EventHandler<VATRP.Core.Events.ConversationEventArgs>(OnUnreadStateChanged), sender, new object[] { e });
                return;
            }

            ChangeUnreadCounter();
        }

        private void Init()
        {
            _isRunning = true;
            _inputProcessorThread = new Thread(ProcessInputCharacters) { IsBackground = true };
            _inputProcessorThread.Start();

            _contactSearchCriteria = string.Empty;
            _receiverAddress = string.Empty;
            LoadContacts();
            this.ContactsListView = CollectionViewSource.GetDefaultView(this.Contacts);  
            this.ContactsListView.SortDescriptions.Add(new SortDescription("LastUnreadMessageTime", ListSortDirection.Descending));
            this.ContactsListView.SortDescriptions.Add(new SortDescription("ContactUI", ListSortDirection.Ascending));
            this.ContactsListView.Filter = new Predicate<object>(this.FilterContactsList);
        }


        #region Methods

        private void OnChatRoomUpdated(object sender, ConversationUpdatedEventArgs e)
        {
            //****************************************************************************************
            // When chat receive a new message then this method called for update chat room.
            //*****************************************************************************************
            if (e.Conversation.IsRttChat) return;

            if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new EventHandler<ConversationUpdatedEventArgs>(OnChatRoomUpdated), sender, new object[] { e });
                return;
            }

            if (ChatViewContact != null && ChatViewContact.Contact == e.Conversation.Contact)
            {
                try
                {
                    if (MessagesListView != null && MessagesListView.SourceCollection != null)
                        MessagesListView.Refresh();
                }
                catch (Exception)
                {

                }

                RaiseConversationChanged();
            }
            else
            {
                foreach (var contactVM in this.Contacts)
                {
                    if (contactVM.Contact == e.Conversation.Contact)
                    {
                        contactVM.LastUnreadMessageTime = e.Conversation.LastUnreadMessageTime;
                    }
                }
            }
            RefreshContactsList();
        }

        protected override bool FilterMessages(object obj)
        {
            var message = obj as VATRPChatMessage;

            if (message != null)
            {
                if (message.Direction == MessageDirection.Incoming)
                    return !message.IsIncompleteMessage && !message.IsRTTStartMarker && !message.IsRTTEndMarker;
                return !message.IsRTTMarker;
            }

            return false;
        }

        protected override void ChangeUnreadCounter()
        {
            if (UnreadMessagesCountChanged != null)
                UnreadMessagesCountChanged(this, EventArgs.Empty);
        }

        protected override void RefreshContactsList()
        {

            //****************************************************************************************
            // Refresh the contact list of Chat.
            //*****************************************************************************************
            try
            {
                if (ContactsListView != null && ContactsListView.SourceCollection != null)
                    ContactsListView.Refresh();
            }
            catch (Exception)
            {

            }
            OnPropertyChanged("ContactsListView");
        }

        protected override void ProcessInputCharacters(object obj)
        {
            var sendBuffer = new StringBuilder();
            var wait_time = Int32.MaxValue;

            while (_isRunning)
            {
                regulator.WaitOne(wait_time);

                lock (_inputTypingQueue)
                {
                    if (_inputTypingQueue.Count != 0)
                    {
                        sendBuffer.Append(_inputTypingQueue.Dequeue());
                    }
                    else
                    {
                        wait_time = Int32.MaxValue;
                        continue;
                    }
                }

                SendMessage(sendBuffer.ToString());

                sendBuffer.Remove(0, sendBuffer.Length);
                lock (_inputTypingQueue)
                {
                    wait_time = _inputTypingQueue.Count == 0 ? Int32.MaxValue : 1;
                }
            }
        }

        internal bool SendSimpleMessage(string message)
        {

            //**********************************************************************************************
            // Sending a message to caller when call was declined or just a message in chat window.
            //********************************************************************************************
            if (!message.NotBlank() || (Chat == null && string.IsNullOrEmpty(ReceiverAddress)))
                return false;
            EnqueueInput(message);
            MessageText = string.Empty;
            return true;
        }

        private void SendMessage(string message)
        {
            Dispatcher dispatcher;
            try
            {
                dispatcher = ServiceManager.Instance.Dispatcher;
            }
            catch (NullReferenceException)
            {
                return;
            }

            if (dispatcher != null)
                dispatcher.BeginInvoke((Action) delegate()
                {
                    _chatsManager.ComposeAndSendMessage(Chat, message);
                });
        }

        public bool FilterContactsList(object item)
        {
            var contactVM = item as ContactViewModel;
            if (contactVM != null)
                return contactVM.Contact != null && contactVM.Contact.Fullname.Contains(ContactSearchCriteria);
            return true;
        }
		
   
        #endregion

        #region Properties

        public string ReceiverAddress
        {
            get { return _receiverAddress; }
            set
            {
                _receiverAddress = value;
                OnPropertyChanged("ReceiverAddress");
                OnPropertyChanged("ShowReceiverHint");
            }
        }

        public bool ShowReceiverHint
        {
            get { return !ReceiverAddress.NotBlank(); }
        }


        public bool ShowSearchHint
        {
            get { return !ContactSearchCriteria.NotBlank(); }
        }


        public string ContactSearchCriteria
        {
            get { return _contactSearchCriteria; }
            set
            {
                if (_contactSearchCriteria != value)
                {
                    _contactSearchCriteria = value;
                    OnPropertyChanged("ContactSearchCriteria");
                    OnPropertyChanged("ShowSearchHint");
                    try
                    {
                        if (ContactsListView != null && this.ContactsListView.SourceCollection != null)
                            ContactsListView.Refresh();
                    }
                    catch (Exception)
                    {
                        
                    }
                }
            }
        }

        public ICollectionView ContactsListView
        {
            get { return this.contactsListView; }
            private set
            {
                if (value == this.contactsListView)
                {
                    return;
                }

                this.contactsListView = value;
                OnPropertyChanged("ContactsListView");
            }
        }

        #endregion

        internal bool CheckReceiverContact()
        {
            //****************************************************************************************
            // Check Chat message receiver contact. it should not be null.
            //*****************************************************************************************
            var receiver = string.Empty;
            if (ReceiverAddress != null)
            {
                receiver = ReceiverAddress.Trim();
            }

            if (ChatViewContact != null && receiver == ChatViewContact.Contact.RegistrationName)
                return true;

            VATRPContact contact = _chatsManager.FindContact(new ContactID(receiver, IntPtr.Zero));
            if (contact == null)
            {
                string un, host, dn;
                int port;
                if (!VATRPCall.ParseSipAddress(receiver, out un,
                    out host, out port))
                    un = "";

                if (!un.NotBlank())
                    return false;

                if (string.IsNullOrEmpty(host))
                    host = App.CurrentAccount.ProxyHostname;
                var contactAddress = string.Format("{0}@{1}", un, host);
                var contactID = new ContactID(contactAddress, IntPtr.Zero);

                contact = new VATRPContact(contactID)
                {
                    DisplayName = un,
                    Fullname = un,
                    SipUsername = un,
                    RegistrationName = contactAddress
                };
                _contactsManager.AddContact(contact, "");
            }

            SetActiveChatContact(contact, IntPtr.Zero);
            if ( ReceiverAddress != contact.RegistrationName )
                ReceiverAddress = contact.RegistrationName;

            return true;
        }

        internal void ShowUnreadMessageInfo(bool updUnreadCounter)
        {
            _chatsManager.UpdateUnreadCounter = updUnreadCounter;
            if (Chat == null)
                return;
            Chat.UpdateUnreadCounter = updUnreadCounter;
        }
    }
}