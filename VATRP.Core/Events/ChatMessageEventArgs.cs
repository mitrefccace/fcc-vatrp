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
using VATRP.Core.Model;

namespace VATRP.Core.Events
{
    public class ChatMessageEventArgs : TextMessageEventArgs, ICloneable
    {
        private List<IntPtr> _chatPtrList;
        private VATRPChatMessage _chatMessage;

        public ChatMessageEventArgs(string remote, IntPtr chatPtr, List<IntPtr> callChatPtrList, VATRPChatMessage chatMessage)
            : base(remote)
        {
            _chatPtrList = new List<IntPtr>();
            _chatPtrList.AddRange(callChatPtrList);
            this.ChatPtr = chatPtr;
            _chatMessage = new VATRPChatMessage(chatMessage.ContentType);
            _chatMessage = chatMessage;
        }

        public List<IntPtr> CallsChatPtrList
        {
            get { return _chatPtrList; }
            private set { _chatPtrList = value; }
        }

        public IntPtr ChatPtr { get; private set; }

        public VATRPChatMessage ChatMessage
        {
            get { return _chatMessage; }
            private set { _chatMessage = value; }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}

