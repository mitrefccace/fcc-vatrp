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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Data;
using System.Windows.Threading;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Extensions;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using VATRP.Core.Events;
using Prism.Commands;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class InCallMessagingViewModel : MessagingViewModel
    {

        #region Members

        private ObservableCollection<string> _textSendModes;
        private string _selectedTextSendMode;
        private string _sendButtonTitle;
        private bool _isSendingModeRtt =  true;
        private bool rttEnabled;
        private bool toggleChecked;
        private DateTime _conversationStartTime;
        #endregion

        #region Events

        public event EventHandler<EventArgs> RttReceived;
        public DelegateCommand MuteTextCommand => new DelegateCommand(this.MuteRTT);
        
        #endregion

        public InCallMessagingViewModel() 
        {
            Init();
        }

        public InCallMessagingViewModel(IChatService chatMng, IContactsService contactsMng)
            : base(chatMng, contactsMng) 
        {
            Init();
            _chatsManager.RttReceived += OnRttReceived;
            _chatsManager.ConversationUpdated += OnChatRoomUpdated;
        }

        private void Init()
        {
            _isRunning = true;
            ToggleChecked = false;
            RTTEnabled = !ToggleChecked;

            TextSendModes.Add("Real Time Text");
            TextSendModes.Add("SIP Simple");
            _inputProcessorThread = new Thread(ProcessInputCharacters) { IsBackground = true };
            _inputProcessorThread.Start();
            //SelectedTextSendMode = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
            //    Configuration.ConfEntry.TEXT_SEND_MODE, "Real Time Text");
            _sendButtonTitle = "Send";
        }

        private void OnRttReceived(object sender, EventArgs e)
        {
            if (RttReceived != null)
            {
                RttReceived(sender, EventArgs.Empty);
            }
        }

        private void OnChatRoomUpdated(object sender, ConversationUpdatedEventArgs e)
        {

            //************************************************************************************************************************************
            // When a message is sent or received in Chat then this method will called.
            //************************************************************************************************************************************
            if (!e.Conversation.IsRttChat) return;

            if (ServiceManager.Instance.Dispatcher.Thread != Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    new EventHandler<ConversationUpdatedEventArgs>(OnChatRoomUpdated), sender, new object[] {e});
                return;
            }

            if (!e.Conversation.Equals(this.Chat))
                return;

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

        #region Methods
    
        internal void SendMessage(char key, bool isIncomplete) // CJM : RTT
       {

            //************************************************************************************************************************************
            // Seding a Chat message.
            //************************************************************************************************************************************
            if (!ServiceManager.Instance.IsRttAvailable)
                return;
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
                    var message = string.Format("{0}", key);
                    if (!message.NotBlank())
                        return;

                  _chatsManager.ComposeAndSendMessage(ServiceManager.Instance.ActiveCallPtr, Chat, key, isIncomplete); // CJM : RTT

                    if (!isIncomplete || key == '\r')
                    {
                        MessageText = string.Empty;
                    }
                });
        }

        internal void SendMessage(string message)
        {
            Dispatcher dispatcher = null;
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
                    if (!message.NotBlank())
                        return;

                    _chatsManager.ComposeAndSendMessage(Chat, message);
                });
        }

        /// <summary>
        /// Gets or sets whether or the button has been checked
        /// </summary>
        public bool ToggleChecked
        {
            get
            {
                return toggleChecked;
            } 
            set
            {
                toggleChecked = value;
                OnPropertyChanged("ToggleChecked");
            }
        }

        /// <summary>
        /// Toggles whether RTT mute is active
        /// </summary>
        private void MuteRTT()
        {
            RTTEnabled = !ToggleChecked;
        }

        /// <summary>
        /// Gets or sets whether the user can send RTT messages
        /// </summary>
        public bool RTTEnabled
        {
            get { return rttEnabled; }
            set
            {
                rttEnabled = value;
                OnPropertyChanged("RTTEnabled");
            }
        }

        public void CreateRttConversation(VATRPChat chatRoom)
        {
            if (chatRoom == null)
                return;

            if (this.Chat != null)
                _chatsManager.CloseChat(this.Chat);

            this.Chat = chatRoom;

            var contactVM = FindContactViewModel(chatRoom.Contact);
            if (contactVM == null)
            {
                contactVM = new ContactViewModel(chatRoom.Contact);
                this.Contacts.Add(contactVM);
            }

            this.ChatViewContact = contactVM;

            Chat.CharsCountInBubble = 0;
            Chat.UnreadMsgCount = 0;
            if (App.CurrentAccount != null)
            {
                Chat.MessageFont = App.CurrentAccount.RTTFontFamily;
            }

            if (App.CurrentAccount != null)
            {
                Chat.MessageFontSize = App.CurrentAccount.RTTFontSize;
            }
            ChatViewContact.IsSelected = true;

            this.MessagesListView = CollectionViewSource.GetDefaultView(this.Messages);
            this.MessagesListView.SortDescriptions.Add(new SortDescription("MessageTime",
                ListSortDirection.Ascending));

            OnPropertyChanged("Chat");
            try
            {
                if (MessagesListView != null && MessagesListView.SourceCollection != null)
                    MessagesListView.Refresh();
            }
            catch (Exception)
            {

            }

            UpdateMessagingView();
#if false
            if (Chat != null)
                Chat.InsertRttWrapupMarkers(callPtr);
#endif
        }

        public void ClearRTTConversation()
        {
            StopInputProcessor();

            Debug.WriteLine("Clear RTT Conversation for " + Chat);
            if (Chat == null)
                return;

            _chatsManager.CloseChat(Chat);
            this.Chat = null;
            
            OnPropertyChanged("Chat");
        }

        protected override bool FilterMessages(object obj)
        {
            return true;
        }

        protected override void ProcessInputCharacters(object obj)
        {
            var sb = new StringBuilder();
            int wait_time = 5;
            bool readyToDeque = true;
            while (_isRunning)
            {
                regulator.WaitOne(wait_time);

                wait_time = 10;

                if (!readyToDeque)
                    continue;

                lock (_inputTypingQueue)
                {
                    if (_inputTypingQueue.Count != 0)
                    {
                        sb.Append(_inputTypingQueue.Dequeue());
                    }
                    else
                    {
                        wait_time = Int32.MaxValue;
                        readyToDeque = true;
                        continue;
                    }
                }

                readyToDeque = false;
                for (int i = 0; i < sb.Length; i++)
                {
                    SendMessage(sb[i], sb[i] != '\r');
                }

                sb.Remove(0, sb.Length);
                readyToDeque = true;
            }
        }

        #endregion

        #region Properties

        public ObservableCollection<string> TextSendModes
        {
            get { return _textSendModes ?? (_textSendModes = new ObservableCollection<string>()); }
        }

        public bool IsSendingModeRTT
        {
            get { return _isSendingModeRtt; }
        }

        public string SelectedTextSendMode
        {
            get { return _selectedTextSendMode; }
            set
            {
                _selectedTextSendMode = value;
                OnPropertyChanged("SelectedTextSendMode");
                //_isSendingModeRtt = _selectedTextSendMode.Equals("Real Time Text");
                //SendButtonTitle = IsSendingModeRTT ? "Send (RTT)" : "Send (SIP)";
            }
        }

        public string SendButtonTitle
        {
            get { return _sendButtonTitle; }
            set
            {
                _sendButtonTitle = value;
                OnPropertyChanged("SendButtonTitle");
            }
        }

        #endregion

    }
}