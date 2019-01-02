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
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Input;
using log4net;
using log4net.Config;
using VATRP.Core.Enums;
using VATRP.Core.Events;
using VATRP.Core.Extensions;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using VATRP.Core.Model.Utils;
using VATRP.LinphoneWrapper;
using VATRP.LinphoneWrapper.Enums;
using VATRP.LinphoneWrapper.Structs;

namespace VATRP.Core.Services
{
    public class ChatsService : IChatService
    {
        #region Members
        private static readonly ILog LOG = LogManager.GetLogger(typeof(ChatsService));
        private bool _isStarted;
        private bool _isStopped;

        private readonly IContactsService _contactSvc;
        private readonly ILinphoneService _linphoneSvc;
        private readonly ServiceManagerBase _serviceManager;

        private Thread _msgProcessorThread;
        private bool _isRunning;
        private Queue<TextMessageEventArgs> _receiveMsgQueue = new Queue<TextMessageEventArgs>();
        private AutoResetEvent regulator = new AutoResetEvent(false);

        private ObservableCollection<VATRPChat> _chatItems;

        private ObservableCollection<VATRPContact> _contacts;

        public static readonly int InputUserDelayTypingTimeout = 3;
        public static readonly int IntervalTypingTimeout = 5;
        private const char UNLF = '\u2028'; // U - 2028
        private const char CR = '\r'; 
        private const char LF = '\n'; 
        #endregion

        #region Properties
        public event EventHandler<ConversationEventArgs> ConversationClosed;

        public event EventHandler<ConversationEventArgs> ConversationStateChanged;

        public event EventHandler<ConversationEventArgs> ConversationUnReadStateChanged;

        public event EventHandler<ConversationUpdatedEventArgs> ConversationUpdated;

        public event EventHandler<EventArgs> ContactsChanged;

        public event EventHandler<ContactEventArgs> ContactAdded;

        public event EventHandler<ContactRemovedEventArgs> ContactRemoved;

        public event EventHandler<ConversationEventArgs> NewConversationCreated;

        public event EventHandler<EventArgs> RttReceived;

        public event EventHandler<DeclineMessageArgs> ConversationDeclineMessageReceived;

        public bool IsRTTenabled { get; set; }

        public bool UpdateUnreadCounter { get; set; }

        public int ChatsCount
        {
            get
            {
                if (this._chatItems == null)
                {
                    return 0;
                }
                return this._chatItems.Count;
            }
        }

        public ObservableCollection<VATRPContact> Contacts
        {
            get
            {
                if (_contacts == null)
                    _contacts = new ObservableCollection<VATRPContact>();
                return _contacts;
            }
        }

        public ObservableCollection<VATRPChat> ChatItems
        {
            get
            {
                if (_chatItems == null)
                    _chatItems = new ObservableCollection<VATRPChat>();
                return _chatItems;
            }
            set { _chatItems = value; }
        }

        #endregion

        #region Events
        
        public event EventHandler<EventArgs> ServiceStarted;
        public event EventHandler<EventArgs> ServiceStopped;
        
        #endregion
        
        #region Methods
        public ChatsService(ServiceManagerBase mngBase)
        {

            //****************************************************************************************
            // This method will called when Chat screen is initilize.
            //*****************************************************************************************
            _isStarted = false;
            _isStopped = false;
            this._serviceManager = mngBase;
            this._contactSvc = _serviceManager.ContactService;
            this._linphoneSvc = _serviceManager.LinphoneService;
            this._contactSvc.ContactAdded += OnContactAdded;
            this._contactSvc.ContactRemoved += OnContactRemoved;
            this._contactSvc.ContactsChanged += OnContactsChanged;
            this._contactSvc.ContactsLoadCompleted += OnContactsLoadCompleted;
            this._chatItems = new ObservableCollection<VATRPChat>();

            this._linphoneSvc.IsComposingReceivedEvent += OnChatMessageComposing;
            this._linphoneSvc.OnChatMessageReceivedEvent += OnChatMessageReceived;
            this._linphoneSvc.OnChatMessageStatusChangedEvent += OnChatStatusChanged;
            IsRTTenabled = true;
            UpdateUnreadCounter = true;
        }

        public void EnqueueReceivedMsg(TextMessageEventArgs args)
        {
            //****************************************************************************************
            // Receive a Chat message.
            //*****************************************************************************************
            lock (_receiveMsgQueue)
            {
                _receiveMsgQueue.Enqueue(args);
            }
            regulator.Set();
        }

        private VATRPChat AddChat(VATRPContact contact, string dialogId, bool isRtt)
        {
            //************************************************************************************************************************************
            // Add the Contact Chat in Chat Window.
            //************************************************************************************************************************************
            VATRPChat item = null;
            if (contact != null)
            {
                item = this.FindChat(contact);
                if (item == null)
                {
                    item = this.CreateChat(contact, dialogId, isRtt);
                }
            }
            return item;
        }

        private void ProcessReceivedMessages()
        {

            //****************************************************************************************
            // Service for Receive Chat message. It will run in start of application.
            //*****************************************************************************************
            var wait_time = Int32.MaxValue;

            while (_isRunning)
            {
                regulator.WaitOne(wait_time);

                TextMessageEventArgs recvMsg;
                lock (_receiveMsgQueue)
                {
                    if (_receiveMsgQueue.Count != 0)
                    {
                        recvMsg = _receiveMsgQueue.Dequeue();
                    }
                    else
                    {
                        wait_time = Int32.MaxValue;
                        continue;
                    }
                }

                if (recvMsg != null)
                {
                    if (recvMsg is ChatMessageEventArgs)
                        OnChatMessageReceived(recvMsg as ChatMessageEventArgs);
                    else if (recvMsg is MessageComposingEventArgs)
                        OnChatMessageComposing(recvMsg as MessageComposingEventArgs);
                }
                lock (_receiveMsgQueue)
                {
                    wait_time = _receiveMsgQueue.Count == 0 ? Int32.MaxValue : 1;
                }
            }
        }

        private void CheckTopPosition(VATRPChat chat)
        {
            if ((chat != null) && (this._chatItems.IndexOf(chat) > 0))
            {
                this._chatItems.ReplaceToTop<VATRPChat>(chat);
            }
        }

        public void ClearChatMsgs(ChatID chatID)
        {
            if (chatID != null)
            {
                VATRPChat chat = this.FindChat(chatID);
                if (chat != null)
                {
                    chat.Messages.Clear();
                }
            }
        }

        public void CloseAllChats()
        {
            Debug.WriteLine("asyncOperation ChatsManager.CloseAllChats");
            for (int i = 0; i < this._chatItems.Count; i++)
            {
                VATRPChat chat = this._chatItems[i];
                this.CloseChat(chat);
                i--;
            }
        }

        public void CloseChat(VATRPChat chat)
        {
            //****************************************************************************************
            // Close chat messages. When call is disconnected. that time this method also called.
            //*****************************************************************************************
            if (chat != null)
            {
                this.RemoveChat(chat);
                this.OnConversationClosed(chat);
            }
        }

        public void CloseChat(ContactID contactID)
        {
            //****************************************************************************************
            // Close chat messages. When call is disconnected. that time this method also called.
            //*****************************************************************************************
            if (contactID != null)
            {
                VATRPChat chat = this.FindChat(contactID);
                if (chat != null)
                {
                    this.CloseChat(chat);
                }
            }
        }

        public VATRPChat CreateChat(VATRPContact contact, bool isRtt)
        {
            if (contact == null)
            {
                return null;
            }
            return this.CreateChat(contact, string.Empty, isRtt);
        }

        public VATRPChat CreateChat(VATRPContact contact, string dialogId, bool isRtt)
        {

            //************************************************************************************************************************************
            // Create Chat of particular contact.
            //************************************************************************************************************************************
            if (contact == null)
            {
                return null;
            }

            VATRPChat chatRoom = new VATRPChat(contact, dialogId, isRtt);
            var loggedContact = _contactSvc.FindLoggedInContact();
            if (loggedContact != null)
                chatRoom.AddContact(loggedContact);

            lock (this._chatItems)
            {
                _chatItems.InsertToTop<VATRPChat>(chatRoom);
            }
            this.OnNewConversationCreated(chatRoom);
            return chatRoom;
        }

        public VATRPChat FindChat(VATRPContact contact)
        {
            //****************************************************************************************
            // Check the contact, contact is not null and a valid contact. This used many places like call, chat etc.
            //*****************************************************************************************
            if (contact == null)
            {
                return null;
            }
            return this.FindChat(new ContactID(contact.ID, contact.NativePtr));
        }

        private VATRPChat FindChat(IntPtr chatPtr)
        {
            if (chatPtr == IntPtr.Zero)
            {
                return null;
            }
            lock (this._chatItems)
            {
                foreach (VATRPChat chatItem in this._chatItems)
                {
                    if ((chatItem != null) && chatItem.NativePtr == chatPtr && !chatItem.IsRttChat )
                    {
                        return chatItem;
                    }
                }
            }

            return null;
        }

        private VATRPChat FindRTTChat(IntPtr chatPtr)
        {
            //****************************************************************************************
            // This method is called in both Chat and Call. When we sent a message or connect a call.
            //*****************************************************************************************
            if (chatPtr == IntPtr.Zero)
            {
                return null;
            }
            lock (this._chatItems)
            {
                foreach (VATRPChat chatItem in this._chatItems)
                {
                    if ((chatItem != null) && chatItem.NativePtr == chatPtr && chatItem.IsRttChat) // CJM : RTT
                    {
                        return chatItem;
                    }
                }
            }

            return null;
        }

        private VATRPChat FindChat(VATRPChat chat)
        {
            if (chat == null)
                return null;

            lock (this._chatItems)
            {
                foreach (VATRPChat chatItem in this._chatItems)
                {
                    if ((chatItem != null) && chatItem.ID == chat.ID)
                    {
                        return chatItem;
                    }
                }
            }

            return null;
        }

        public VATRPChat FindChat(ChatID chatID)
        {
            if (chatID == null)
                return null;

            lock (this._chatItems)
            {
                foreach (VATRPChat chatItem in this._chatItems)
                {
                    if ((chatItem != null) && chatItem == chatID)
                    {
                        return chatItem;
                    }
                }
            }

            return null;
        }

        public VATRPChat FindChat(ContactID contactID)
        {
            if (contactID == null)
                return null;

            lock (this._chatItems)
            {
                foreach (VATRPChat chatItem in this._chatItems)
                {
                    if ((chatItem != null) && chatItem.ID == contactID.ID)
                    {
                        return chatItem;
                    }
                }
            }

            return null;
        }

        public List<VATRPChat> GetAllConversations()
        {
            if (this._chatItems == null)
            {
                return new List<VATRPChat>();
            }
            return Enumerable.ToList<VATRPChat>((IEnumerable<VATRPChat>)this._chatItems);
        }

        public VATRPChat GetChat(VATRPContact contact, bool isRtt)
        {
            //****************************************************************************************
            // Get the all Chat messages for display in Chat window.
            //*****************************************************************************************
            VATRPChat chat = this.FindChat(contact);
            if (chat == null)
            {
                chat = this.AddChat(contact, string.Empty, isRtt);
                this.Contacts.Add(contact);
                if (ContactAdded != null)
                    ContactAdded(this, new ContactEventArgs(new ContactID(contact)));
            }
            return chat;
        }

        public VATRPContact GetFirstContactWithUnreadMessages()
        {
            foreach (VATRPChat chat in this._chatItems)
            {
                if (chat.HasUnreadMsg)
                {
                    return chat.Contact;
                }
            }
            return null;
        }

        private List<VATRPChatMessage> GetLastUnreadMessages()
        {
            var list = new List<VATRPChatMessage>();
            foreach (VATRPChat chat in this._chatItems)
            {
                if (chat.HasUnreadMsg)
                {
                    int num = 0;
                    for (int j = chat.Messages.Count - 1; j >= 0; j--)
                    {
                        if (num >= chat.UnreadMsgCount)
                        {
                            break;
                        }
                        if (chat.Messages[j].Direction == MessageDirection.Incoming)
                        {
                            list.Add(chat.Messages[j]);
                            num++;
                        }
                    }
                }
            }
            list.Sort();
            var list2 = new List<VATRPChatMessage>();
            for (int i = 0; i < list.Count; i++)
            {
                if (list2.Count == 5)
                {
                    return list2;
                }
                if (list2.Count == 0)
                {
                    list2.Add(list[i]);
                }
                else
                {
                    list2.Insert(0, list[i]);
                }
            }
            return list2;
        }

        public uint GetUnreadMsgsCount()
        {
            uint num = 0;
            lock (this._chatItems)
            {
                foreach (VATRPChat chat in this._chatItems)
                {
                    if ((chat != null) && (chat.UnreadMsgCount > 0))
                    {
                        num += chat.UnreadMsgCount;
                    }
                }
            }
            return num;
        }

        public static bool IsMessageAlreadyExistCheckByMsgID(VATRPChat chat, string msgID)
        {
            if ((chat != null) && !string.IsNullOrEmpty(msgID))
            {
                foreach (VATRPChatMessage message in chat.Messages)
                {
                    if (message.ID == msgID)
                    {
                        Debug.WriteLine("ChatList_Manager:IsMessageAlreadyExistCheckByMsgID: msg exist msgId = " +
                            msgID);
                        return true;
                    }
                }
            }
            return false;
        }

        public void MarkChatAsRead(ChatID chatId)
        {
            VATRPChat chat = FindChat(chatId);
            if (chat != null)
            {
                chat.UnreadMsgCount = 0;
                OnConversationUnReadStateChanged(chat);
                if (chat.Contact != null)
                {
                    chat.Contact.UnreadMsgCount = 0;
                }
                _linphoneSvc.MarkChatAsRead(chat.NativePtr);
            }
        }

        public VATRPContact FindContact(ContactID contactID)
        {
            foreach (var contactItem in Contacts)
            {
                if (contactItem.ID == contactID.ID)
                {
                    return contactItem;
                }
            }

            return null;
        }

        public VATRPChat InsertRttChat(VATRPContact contact, IntPtr chatPtr, IntPtr callPtr)
        {

            //*****************************************************************************************************************
            // This method is called in both Chat and Call. When we sent/receive a message or connect/receive a call.
            //*****************************************************************************************************************
            VATRPChat chat = FindRTTChat(chatPtr);

            if (chat != null)
                return chat;

            chat = new VATRPChat(contact, callPtr + "_rtt", true) {NativePtr = chatPtr, CallPtr = callPtr};

            lock (this._chatItems)
            {
                this._chatItems.Add(chat);
            }

            return chat;
        }

        private bool RemoveChat(VATRPChat chat)
        {
            //******************************************************************************************************************************************************
            // Called when call is disconnected and it will close all the chats. Also remove chat when Contact is deleted from Contact list
            //******************************************************************************************************************************************************

            var retValue = false;
            if (chat == null)
            {
                return retValue;
            }

            lock (this._chatItems)
            {
                retValue = this._chatItems.Remove(chat);
            }
            return retValue;
        }

        public void UpdateRTTFontFamily(string newFont)
        {

            //************************************************************************************************************************************
            // Change the message font when Font family is changed from More==>Settings==>Text
            //************************************************************************************************************************************
            lock (this._chatItems)
            {
                foreach (VATRPChat chatItem in this._chatItems)
                {
                    if (chatItem != null)
                    {
                        chatItem.MessageFont = newFont;
                        //chatItem.MessageFontSize = 40;
                        //chatItem.Message
                    }
                }
            }
        }
        public void UpdateRTTFontSize(int newFont)
        {

            //************************************************************************************************************************************
            // Change the message font when Font family is changed from More==>Settings==>Text
            //************************************************************************************************************************************
            lock (this._chatItems)
            {
                foreach (VATRPChat chatItem in this._chatItems)
                {
                    if (chatItem != null)
                    {
                       // chatItem.MessageFont = newFont;
                        chatItem.MessageFontSize = newFont;
                        //chatItem.Message
                    }
                }
            }
        }
        public bool HasUnreadMessages()
        {

            //****************************************************************************************
            //Check if there is any unread message in chat.
            //*****************************************************************************************
            lock (this._chatItems)
            {
                foreach (VATRPChat chatItem in this._chatItems)
                {
                    if (chatItem != null)
                    {
                        if (chatItem.HasUnreadMsg)
                            return true;
                    }
                }
            }
            return false;
        }

        public void ActivateChat(VATRPChat chat)
        {

            //********************************************************************************************************************************************************
            // Change the selected chat in chat window. Like we have 2 Chats and 1 is selected. when user select the other chat (Chat 2) then it will called.
            //********************************************************************************************************************************************************
            lock (this._chatItems)
            {
                foreach (VATRPChat chatItem in this._chatItems)
                {
                    if (chatItem != null && chatItem != chat)
                    {
                        chatItem.IsSelected = false;
                    }
                }

                if (chat != null)
                    chat.IsSelected = true;
            }
        }

        public bool ComposeAndSendMessage(IntPtr callPtr, VATRPChat chat, char key, bool inCompleteMessage)
        {

            //*****************************************************************************************************************************************************************
            // Compose and send a message in chat. This method is called only when Call is running and chat message sent using the right side chat window of call.
            //*****************************************************************************************************************************************************************
            VATRPChat chatID = this.FindRTTChat(chat.NativePtr);
            if ((chatID == null) || (chatID.Contact == null))
            {
                LOG.Warn("ComposeAndSendMessage Null Chat: for " + chat.NativePtr);
                return false;
            }

            VATRPContact loggedContact = _contactSvc.FindLoggedInContact();

            var message = chat.SearchIncompleteMessage(MessageDirection.Outgoing);
            if (message == null)
            {
                if (key == '\r')
                    return false;

                message = new VATRPChatMessage(MessageContentType.Text)
                {
                    Direction = MessageDirection.Outgoing,
                    Status = LinphoneChatMessageState.LinphoneChatMessageStateIdle,
                    IsRTTMessage = true,
                    Chat = chatID
                };
                chatID.AddMessage(message, false);
            }
            else
            {
                message.MessageTime = DateTime.Now;
            }

            var rttCode = (uint)key;
            var createBubble = false;
            if (key != '\r')
            {
                var sb = new StringBuilder(message.Content);
                if (key == '\b')
                {
                    if (sb.Length > 0)
                        sb.Remove(sb.Length - 1, 1);
                }
                else
                    sb.Append(Convert.ToChar(rttCode));
                message.Content = sb.ToString();
                chat.UpdateLastMessage(false);
            }
            else
            {
                createBubble = true;
            }

            message.IsIncompleteMessage = !createBubble;

            // send message to linphone
            var chatPtr = chat.NativePtr;
            var msgPtr = message.NativePtr;
            _linphoneSvc.SendChar(rttCode, callPtr, ref chatPtr, ref msgPtr);

            chat.NativePtr = chatPtr;
            message.NativePtr = msgPtr;
            if (message.NativePtr != IntPtr.Zero)
                message.Status = _linphoneSvc.GetMessageStatus(message.NativePtr);

            if (createBubble)
            {
                // delete empty message 
                if (!message.Content.NotBlank())
                    chatID.DeleteMessage(message);
            }
            this.OnConversationUpdated(chatID, true);
            return true;

        }

        public bool ComposeAndSendMessage(VATRPChat chat, string text)
        {

            //****************************************************************************************
            // Sending Chat message.
            //*****************************************************************************************
            VATRPChat chatID = this.FindChat(chat);
            if ((chatID == null) || (chatID.Contact == null))
            {
                return false;
            }
            var declineMessage = text.StartsWith(VATRPChatMessage.DECLINE_PREFIX);
            var message = new VATRPChatMessage(MessageContentType.Text)
            {
                Direction = MessageDirection.Outgoing,
                Status = LinphoneChatMessageState.LinphoneChatMessageStateIdle,
                MessageTime = DateTime.Now,
                IsIncompleteMessage = false,
                Chat = chatID,
                IsDeclineMessage = declineMessage
            };

            message.Content = declineMessage ? text.Substring(VATRPChatMessage.DECLINE_PREFIX.Length) : text;

            // send message to linphone
            IntPtr msgPtr = IntPtr.Zero;
            _linphoneSvc.SendChatMessage(chat, text, ref msgPtr);
            if (msgPtr != IntPtr.Zero)
                message.MessageTime = Time.ConvertUtcTimeToLocalTime(LinphoneAPI.linphone_chat_message_get_time(msgPtr));

            message.NativePtr = msgPtr;

            chat.AddMessage(message, false);
            chat.UpdateLastMessage(false);

            chat.UnreadMsgCount = 0;
            chat.Contact.UnreadMsgCount = 0;

            OnConversationUnReadStateChanged(chat);
            this.OnConversationUpdated(chat, true);

            return true;
        }

        private void SetOfflineStateToChat(VATRPChat chat)
        {
            if (chat != null)
            {
                if (chat.Contact != null)
                {
                    chat.Contact.Status = UserStatus.Offline;
                }

            }
        }

        internal void SetStatusForMessage(VATRPContact contact, string msgId, LinphoneChatMessageState status)
        {
            if (msgId.NotBlank())
            {
                if (contact != null)
                {
                    VATRPChat chat = this.FindChat(contact);
                    if ((chat != null) && msgId.NotBlank())
                    {
                        foreach (VATRPChatMessage message in chat.Messages)
                        {
                            if (message.ID == msgId)
                            {
                                if (status > message.Status)
                                {
                                    message.Status = status;
                                }
                                break;
                            }
                        }
                    }
                }
                else
                {
                    using (IEnumerator<VATRPChat> enumerator2 = this._chatItems.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            foreach (VATRPChatMessage message2 in enumerator2.Current.Messages)
                            {
                                if (message2.ID == msgId)
                                {
                                    if (status > message2.Status)
                                    {
                                        message2.Status = status;
                                    }
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        internal void SetStatusLastMessageInActiveChat(VATRPContact contact, string msgText, LinphoneChatMessageState status)
        {
            if ((msgText != null) && (contact != null))
            {
                VATRPChat chat = this.FindChat(contact);
                if ((((chat != null) && ((chat.Messages != null) && (chat.Messages.Count != 0))) &&
                     msgText == chat.Messages[chat.Messages.Count - 1].Content) &&
                    (status > chat.Messages[chat.Messages.Count - 1].Status))
                {
                    chat.Messages[chat.Messages.Count - 1].Status = status;
                }
            }
        }

        private void UpdateContactStatusInChats(VATRPContact contact)
        {
            foreach (VATRPChat chat in this._chatItems)
            {
                for (int i = 0; i < chat.Contacts.Count; i++)
                {
                    if (contact.Equals(chat.Contacts[i]))
                    {
                        chat.Contacts[i].Status = contact.Status;
                    }
                }
            }

        }

        private void LoadLinphoneChatEvents()
        {

            //****************************************************************************************
            // Loading the Linphone chat service events
            //*****************************************************************************************
            if (_chatItems == null)
                _chatItems = new ObservableCollection<VATRPChat>();

            if (_linphoneSvc.LinphoneCore == IntPtr.Zero)
                return;

            IntPtr chatRoomPtr = LinphoneAPI.linphone_core_get_chat_rooms(_linphoneSvc.LinphoneCore);
            if (chatRoomPtr != IntPtr.Zero)
            {
                MSList msChatRoomList;

                do
                {
                    msChatRoomList.next = IntPtr.Zero;
                    msChatRoomList.prev = IntPtr.Zero;
                    msChatRoomList.data = IntPtr.Zero;

                    msChatRoomList = (MSList)Marshal.PtrToStructure(chatRoomPtr, typeof(MSList));
                    if (msChatRoomList.data != IntPtr.Zero)
                    {
                        IntPtr tmpPtr = LinphoneAPI.linphone_chat_room_get_peer_address(msChatRoomList.data);
                        if (tmpPtr != IntPtr.Zero)
                        {
                            tmpPtr = LinphoneAPI.linphone_address_as_string(tmpPtr);
                            if (tmpPtr != IntPtr.Zero)
                            {
                                var remoteParty = LinphoneAPI.PtrToStringUtf8(tmpPtr);
                                LinphoneAPI.ortp_free(tmpPtr);
                                string dn, un, host;
                                int port;
                                if (
                                    !VATRPCall.ParseSipAddressEx(remoteParty, out dn, out un,
                                        out host, out port))
                                    un = "";

                                if (un.NotBlank())
                                {
                                    var contactAddress = string.Format("{0}@{1}", un, host);
                                    var contactId = new ContactID(contactAddress, IntPtr.Zero);
                                    VATRPContact contact = _contactSvc.FindContact(contactId);
                                    if (contact == null)
                                    {
                                        contact = new VATRPContact(contactId)
                                        {
                                            DisplayName = dn,
                                            Fullname = un,
                                            SipUsername = un,
                                            RegistrationName = contactAddress
                                        };
                                        _contactSvc.AddContact(contact, string.Empty);
                                    }

                                    var chat = this.AddChat(contact, string.Empty, false);
                                    chat.NativePtr = msChatRoomList.data;
                                    LoadMessages(chat, chat.NativePtr);
                                    OnConversationUpdated(chat, true);
                                }
                            }
                        }
                    }
                    chatRoomPtr = msChatRoomList.next;
                } while (msChatRoomList.next != IntPtr.Zero);

            }
            if (ServiceStarted != null)
                ServiceStarted(this, EventArgs.Empty);
        }

        private void LoadMessages(VATRPChat chat, IntPtr chatRoomPtr)
        {
            //************************************************************************************************************************
            // Parsing and displaying old messages in chat.
            //************************************************************************************************************************
            IntPtr msgListPtr = LinphoneAPI.linphone_chat_room_get_history(chatRoomPtr, 0);
            if (msgListPtr != IntPtr.Zero)
            {
                MSList msMessagePtr;

                var loggedInContact = _contactSvc.FindLoggedInContact();
                if (loggedInContact == null)
                    return;
                do
                {
                    msMessagePtr.next = IntPtr.Zero;
                    msMessagePtr.prev = IntPtr.Zero;
                    msMessagePtr.data = IntPtr.Zero;

                    msMessagePtr = (MSList)Marshal.PtrToStructure(msgListPtr, typeof(MSList));
                    if (msMessagePtr.data != IntPtr.Zero)
                    {
                        IntPtr tmpPtr = LinphoneAPI.linphone_chat_message_get_from_address(msMessagePtr.data);
                        tmpPtr = LinphoneAPI.linphone_address_as_string(tmpPtr);
                        if (tmpPtr != IntPtr.Zero)
                        {
                            var fromParty = LinphoneAPI.PtrToStringUtf8(tmpPtr).TrimSipPrefix();
                            LinphoneAPI.ortp_free(tmpPtr);

                            tmpPtr = LinphoneAPI.linphone_chat_message_get_to_address(msMessagePtr.data);
                            if (tmpPtr != IntPtr.Zero)
                            {
                                tmpPtr = LinphoneAPI.linphone_address_as_string(tmpPtr);
                                var toParty = LinphoneAPI.PtrToStringUtf8(tmpPtr).TrimSipPrefix();
                                LinphoneAPI.ortp_free(tmpPtr);

                                string dn, un, host;
                                int port;

                                if (fromParty == loggedInContact.ID || toParty == loggedInContact.ID)
                                {
                                    bool isOutgoing = false;
                                    string remoteParty = fromParty;
                                    if (fromParty == loggedInContact.ID)
                                    {
                                        isOutgoing = true;
                                        remoteParty = toParty;
                                    }

                                    if (
                                        !VATRPCall.ParseSipAddressEx(remoteParty, out dn, out un,
                                            out host, out port))
                                        un = "";

                                    if (un.NotBlank())
                                    {
                                        var contactAddress = string.Format("{0}@{1}", un, host);
                                        var contactID = new ContactID(contactAddress, IntPtr.Zero);
                                        var contact = FindContact(contactID);

                                        if (contact == null)
                                        {
                                            contact = new VATRPContact(contactID)
                                            {
                                                DisplayName = dn,
                                                Fullname = un,
                                                SipUsername = un,
                                                RegistrationName = contactAddress
                                            };
                                            _contactSvc.AddContact(contact, "");
                                        }

                                        IntPtr msgPtr = LinphoneAPI.linphone_chat_message_get_text(msMessagePtr.data);
                                        var messageString = string.Empty;
                                        if (msgPtr != IntPtr.Zero)
                                            messageString = LinphoneAPI.PtrToStringUtf8(msgPtr);

                                        if (messageString.NotBlank())
                                        {
                                            var localTime =
                                                Time.ConvertUtcTimeToLocalTime(
                                                    LinphoneAPI.linphone_chat_message_get_time(msMessagePtr.data));
                                            var declineMessage = false;
                                            if (messageString.StartsWith(VATRPChatMessage.DECLINE_PREFIX))
                                            {
                                                messageString = messageString.Substring(VATRPChatMessage.DECLINE_PREFIX.Length);
                                                declineMessage = true;
                                            }
                                            var chatMessage = new VATRPChatMessage(MessageContentType.Text)
                                            {
                                                Direction =
                                                    isOutgoing
                                                        ? MessageDirection.Outgoing
                                                        : MessageDirection.Incoming,
                                                IsIncompleteMessage = false,
                                                MessageTime = localTime,
                                                Content = messageString,
                                                IsRTTMessage = false,
                                                IsDeclineMessage = declineMessage,
                                                IsRead = LinphoneAPI.linphone_chat_message_is_read(msMessagePtr.data) == 1,
                                                Status = LinphoneAPI.linphone_chat_message_get_state(msMessagePtr.data),
                                                Chat = chat
                                            };

                                            chat.AddMessage(chatMessage, false);
                                            chat.UpdateLastMessage(false);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    msgListPtr = msMessagePtr.next;
                } while (msMessagePtr.next != IntPtr.Zero);

            }
        }

        #endregion

        #region Event handlers
        private void OnContactsLoadCompleted(object sender, EventArgs e)
        {
            //****************************************************************************************************
            // When Contacts loading from database is completed.
            //****************************************************************************************************

            if (_contactSvc.Contacts == null)
                return;

            foreach (var contact in _contactSvc.Contacts)
            {
                if (contact.IsLoggedIn)
                    continue;
                VATRPChat chat = AddChat(contact, string.Empty, false);
                Contacts.Add(contact);
                if (ContactAdded != null)
                    ContactAdded(this, new ContactEventArgs(new ContactID(contact)));
            }
            new Thread((ThreadStart)LoadLinphoneChatEvents).Start();
        }

        private void OnContactAdded(object sender, ContactEventArgs e)
        {

            //************************************************************************************************************************************
            // On Contact added in history/chat window.
            //************************************************************************************************************************************
            VATRPContact contact = this._contactSvc.FindContact(e.Contact);
            if (contact != null)
            {
                if (contact.IsLoggedIn)
                    return;
                VATRPChat chat = AddChat(contact, string.Empty, false);
                Contacts.Add(contact);
                if (ContactAdded != null)
                    ContactAdded(this, new ContactEventArgs(new ContactID(e.Contact)));
            }
        }

        private void OnContactsChanged(object sender, EventArgs eventArgs)
        {

            //*************************************************************************************************************************************************
            // When contact is edited, contact status is changed.
            //*************************************************************************************************************************************************
            foreach (var contact in this._contactSvc.Contacts)
            {
                VATRPContact chatContact = FindContact(new ContactID(contact.ID, contact.NativePtr));
                if (chatContact == null)
                {
                    AddChat(contact, contact.ID, false);
                    Contacts.Add(contact);
                }
            }

            foreach (var chatContact in this.Contacts)
            {
                VATRPContact contact = this._contactSvc.FindContact(new ContactID(chatContact.ID, chatContact.NativePtr));
                if (contact == null)
                {
                    RemoveChat(GetChat(chatContact, false));
                    Contacts.Remove(chatContact);
                }
            }

            if (ContactsChanged != null)
                ContactsChanged(this, null);
        }

        public void OnContactRemoved(object sender, ContactRemovedEventArgs e)
        {
            //*************************************************************************************************************************************************
            // When Contact removed from Contact list.
            //*************************************************************************************************************************************************
            VATRPContact contact = this._contactSvc.FindContact(e.contactId);
            if (contact != null)
            {
                VATRPChat chat = FindChat(contact);
                if (chat != null)
                {
                    if (ConversationClosed != null)
                        ConversationClosed(this, new ConversationEventArgs(chat));
                    RemoveChat(chat);
                }
                Contacts.Remove(contact);
                if (ContactRemoved != null)
                    ContactRemoved(this, new ContactRemovedEventArgs(new ContactID(contact), true));
            }
        }

        private void OnChatMessageComposing(IntPtr chatPtr, uint rttCode)
        {
            //************************************************************************************************************************************
            // When the other user of chat is writing a message then this method will called.
            //************************************************************************************************************************************
            var args = new MessageComposingEventArgs(chatPtr, rttCode);
            EnqueueReceivedMsg(args);
        }

        private void OnChatMessageReceived(IntPtr chatPtr, List<IntPtr> callChatPtrList, string remoteUser,
            VATRPChatMessage chatMessage)
        {
            //****************************************************************************************
            // Message received in chat.
            //*****************************************************************************************
            var args = new ChatMessageEventArgs(remoteUser, chatPtr, callChatPtrList, chatMessage);
            EnqueueReceivedMsg(args);
        }

        private void OnChatMessageComposing(MessageComposingEventArgs args)
        {
            //*********************************************************************************************************************************
            // When other user/this user composing a message in chat window. This method will called on each char which user write.
            //*********************************************************************************************************************************
            if (args == null)
                return;
            System.Windows.Threading.Dispatcher dispatcher = null;
            try
            {
                dispatcher = this._serviceManager.Dispatcher;
            }
            catch (NullReferenceException)
            {
                return;
            }
            if (dispatcher != null)
                dispatcher.BeginInvoke((Action) delegate()
                {
                    VATRPChat chat = FindRTTChat(args.ChatRoomPtr);

                    if (chat == null)
                    {
                        LOG.WarnFormat("RTT Chat not found Ptr: {0} ", args.ChatRoomPtr);
                        return;
                    }

                    chat.CharsCountInBubble++;
                    var rttCodeArray = new char[2];
                    var rttCodearrayLength = 1;
                    if (chat.CharsCountInBubble == 201)
                    {
                        rttCodeArray[0] = UNLF;
                        try
                        {
                            rttCodeArray[1] = Convert.ToChar(args.RTTCode);
                        }
                        catch (Exception)
                        {
                            
                        }
                        rttCodearrayLength = 2;
                        chat.CharsCountInBubble = 0;
                    }
                    else
                    {
                        try
                        {
                            rttCodeArray[0] = Convert.ToChar(args.RTTCode);
                        }
                        catch
                        {

                        }
                    }

                    for (int i = 0; i < rttCodearrayLength; i++)
                    {
                        VATRPChatMessage message = chat.SearchIncompleteMessage(MessageDirection.Incoming);

                        if (message == null)
                        {
                            message = new VATRPChatMessage(MessageContentType.Text)
                            {
                                Direction = MessageDirection.Incoming,
                                IsIncompleteMessage = true,
                                Chat = chat,
                                IsRTTMarker = false,
                                IsRTTMessage = true
                            };

                            chat.AddMessage(message, false);
                        }

                        try
                        {
                            var sb = new StringBuilder(message.Content);
                            switch (rttCodeArray[i])
                            {
                                case UNLF: //
                                case CR:
                                case LF:
                                    message.IsIncompleteMessage = false;
                                    break;
                                case '\b':
                                    //*****************************************************
                                    // When other user write a message in compose window and remove the written text then this case will called.
                                    //****************************************************
                                    if (sb.Length > 0)
                                        sb.Remove(sb.Length - 1, 1);
                                    break;
                                default:
                                    //*****************************************************
                                    // When other user writing a char in a chat message in compose window 
                                    //****************************************************
                                    sb.Append(rttCodeArray[i]);
                                    break;
                            }
                            if (message.IsIncompleteMessage)
                                message.Content = sb.ToString();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Error in OnChatMessageComposing: " + ex.Message);
                            message.IsIncompleteMessage = false;
                        }
                        if (string.IsNullOrEmpty(message.Content))
                            chat.DeleteMessage(message);
                        else
                            chat.UpdateLastMessage(false);
                        this.OnConversationUpdated(chat, true);

                        if (this.RttReceived != null)
                        {
                            this.RttReceived(chat.CallPtr, EventArgs.Empty);
                        }
                    }
                });
        }

        private void OnChatMessageReceived(ChatMessageEventArgs args)
        {
            //****************************************************************************************
            // When chat message received, this event will called.
            //*****************************************************************************************
            if (args == null) return;

            string dn, un, host;
            int port;
            if (args.CallsChatPtrList.Contains(args.ChatPtr) /*&& RttEnabled*/)
            {
                return;
            }

            System.Windows.Threading.Dispatcher dispatcher = null;
            try
            {
               dispatcher = this._serviceManager.Dispatcher;
            }
            catch (NullReferenceException)
            {
                return;
            }

            if (dispatcher != null)
                dispatcher.BeginInvoke((Action) delegate()
                {
                    if (!VATRPCall.ParseSipAddressEx(args.Remote, out dn, out un,
                        out host, out port))
                        un = "";

                    if (!un.NotBlank())
                        return;

                    var contactAddress = string.Format("{0}@{1}", un, host);
                    var contactID = new ContactID(contactAddress, IntPtr.Zero);
                    VATRPContact contact = FindContact(contactID);

                    if (contact == null)
                    {
                        contact = new VATRPContact(contactID)
                        {
                            DisplayName = dn,
                            Fullname = dn.NotBlank() ? dn : un,
                            SipUsername = un,
                            RegistrationName = contactAddress
                        };
                        _contactSvc.AddContact(contact, "");
                        Contacts.Add(contact);
                        if (ContactAdded != null)
                            ContactAdded(this, new ContactEventArgs(new ContactID(contact)));
                    }

                    VATRPChat chat = GetChat(contact, false);
                    chat.NativePtr = args.ChatPtr;

                    if (chat.CheckMessage(args.ChatMessage))
                    {
                        chat.UnreadMsgCount++;
                        if (!chat.IsSelected)
                            contact.UnreadMsgCount++;
                        args.ChatMessage.IsRTTMessage = false;
                        args.ChatMessage.IsIncompleteMessage = false;
                        args.ChatMessage.Chat = chat;
                        if (args.ChatMessage.Content.StartsWith(VATRPChatMessage.DECLINE_PREFIX))
                        {
                            args.ChatMessage.Content = args.ChatMessage.Content.Substring(VATRPChatMessage.DECLINE_PREFIX.Length);

                            args.ChatMessage.IsDeclineMessage = true;
                            if (ConversationDeclineMessageReceived != null)
                            {
                                var header = "Call declined with message";
                                var declineArgs = new DeclineMessageArgs(header, args.ChatMessage.Content) { Sender = contact };
                                ConversationDeclineMessageReceived(this, declineArgs);
                            }
                        }


                        chat.AddMessage(args.ChatMessage, false);
                        chat.UpdateLastMessage(false);

                        OnConversationUnReadStateChanged(chat);
                        this.OnConversationUpdated(chat, true);
                    }
                });
        }

        private void OnChatStatusChanged(IntPtr chatMsgPtr, LinphoneChatMessageState state)
        {

            //****************************************************************************************
            // Chat message status change like Progress, Delivered etc
            //*****************************************************************************************
            System.Windows.Threading.Dispatcher dispatcher = null;
            try
            {
                dispatcher = this._serviceManager.Dispatcher;
            }
            catch (NullReferenceException)
            {
                return;
            }
            if (dispatcher != null)
                dispatcher.BeginInvoke((Action) delegate()
                {
                    lock (this._chatItems)
                    {
                        foreach (var chatItem in this._chatItems)
                        {
                            var chatMessage = chatItem.FindMessage(chatMsgPtr);
                            if (chatMessage != null)
                            {
                                chatMessage.Status = state;
                                return;
                            }
                        }
                    }
                });
        }
        
        private void OnConversationClosed(VATRPChat chat)
        {
            //****************************************************************************************************************
            // When conversation is closed. Also called when Call is disconnected.
            //***************************************************************************************************************
            if ((chat != null) && (this.ConversationClosed != null))
            {
                this.ConversationClosed(this, new ConversationEventArgs(chat));
            }
        }

        private void OnConversationUnReadStateChanged(VATRPChat chat)
        {
            //***********************************************************************************************************************************
            // Conversation message/ Chat message read status changed. when just message arrived/sent then its status is unread.
            // Chat message status change like Progress, Delivered etc
            //*****************************************************************************************
            //***********************************************************************************************************************************
            if ((chat != null) && (this.ConversationUnReadStateChanged != null))
            {
                this.ConversationUnReadStateChanged(this, new ConversationEventArgs(chat));
            }
        }

        public void OnConversationUpdated(VATRPChat chat, bool allowUpdate)
        {
            //****************************************************************************************
            // When conversation is updated, when new message received and status changed.
            //****************************************************************************************
            if (chat != null)
            {
                if (this.ConversationUpdated != null)
                {
                    var args = new ConversationUpdatedEventArgs(chat);
                    args.AllowToChangeUnreadMessageCounter = allowUpdate;
                    this.ConversationUpdated(this, args);
                }
            }
        }

        private void OnNewConversationCreated(VATRPChat chat)
        {
            if ((chat != null) && (this.NewConversationCreated != null))
            {
                this.NewConversationCreated(this, new ConversationEventArgs(chat));
            }
        }

        #endregion

        #region IVATRPService
        public bool Start()
        {

            //****************************************************************************************
            // Starting the Chat Service.
            //*****************************************************************************************
            if (_isStarted)
                return true;

            _isRunning = true;
            _msgProcessorThread = new Thread(ProcessReceivedMessages) { IsBackground = true };
            _msgProcessorThread.Start();

            _isStarted = true;
            _isStopped = false;
            
            if (ServiceStarted != null)
                ServiceStarted(this, EventArgs.Empty);
            return true;
        }


        public bool Stop()
        {
            if (_isStopped)
                return true;
            _isRunning = false;
            regulator.Set();
            _isStarted = false;
            _isStopped = true;

            if (ServiceStopped != null)
                ServiceStopped(this, EventArgs.Empty);

            return true;
        }
        
        #endregion
    }
}

