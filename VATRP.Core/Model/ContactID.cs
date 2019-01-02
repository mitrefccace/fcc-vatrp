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

namespace VATRP.Core.Model
{
    public class ContactID : IEquatable<ContactID>, INotifyPropertyChanged
    {
        public string ID { get; set; }

        public IntPtr NativePtr { get; set; }
        public ContactID()
        {
            this.ID = string.Empty;
            this.NativePtr = IntPtr.Zero;
        }

        public ContactID(ContactID contactID)
        {
            if (contactID != null)
            {
                this.ID = contactID.ID;
                this.NativePtr = contactID.NativePtr;
            }
        }

        public ContactID(string contactID, IntPtr nativePtr)
        {
            this.ID = contactID;
            this.NativePtr = nativePtr;
        }

        public virtual bool Equals(ContactID other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }
            return (object.ReferenceEquals(other, this) || ((this.ID == other.ID) && (this.NativePtr == other.NativePtr)));
        }

        public override bool Equals(object obj)
        {
            return ((obj is ContactID) && this.Equals(obj as ContactID));
        }

        public override int GetHashCode()
        {
            return (base.GetHashCode() ^ this.ID.GetHashCode());
        }

        public static bool operator ==(ContactID first, ContactID second)
        {
            if (object.ReferenceEquals(first, null))
            {
                return object.ReferenceEquals(first, second);
            }
            return first.Equals(second);
        }

        public static bool operator !=(ContactID first, ContactID second) 
        {
            return !(first == second);
        }

        public override string ToString()
        {
            return (base.ToString() + ";" + this.ID);
        }

        #region NotifyPropertyChanged Interface

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}

