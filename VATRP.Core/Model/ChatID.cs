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

using System.ComponentModel;

namespace VATRP.Core.Model
{
    public class ChatID : ContactID
    {
        public ChatID(ChatID chatID) : base(chatID.ID, chatID.NativePtr)
        {
            this.DialogID = chatID.DialogID;
        }

        public ChatID(ContactID contactId, bool isRTT, string dialogId) : base(contactId.ID, contactId.NativePtr)
        {
            this.DialogID = dialogId;
            this.IsRttChat = isRTT;
        }

        public virtual bool Equals(ChatID other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }
            if (object.ReferenceEquals(other, this))
            {
                return true;
            }
            
            if (this.IsRttChat != other.IsRttChat)
            {
                return false;
            }

            return base.Equals((ContactID)other);
        }

        public override bool Equals(object obj)
        {
            return ((obj is ChatID) && this.Equals(obj as ChatID));
        }

        public override int GetHashCode()
        {
            return (base.GetHashCode() ^ this.DialogID.GetHashCode());
        }

        public static bool operator ==(ChatID first, ChatID second)
        {
            if (object.ReferenceEquals(first, null))
            {
                return object.ReferenceEquals(first, second);
            }
            return first.Equals(second);
        }

        public static bool operator !=(ChatID first, ChatID second)
        {
            return !(first == second);
        }

        public string DialogID { get; protected set; }

        public bool IsRttChat { get; protected set; }

    }
}

