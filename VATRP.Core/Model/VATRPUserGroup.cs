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
using System.IO;
using System.Net;
using System.Reflection;

namespace VATRP.Core.Model
{
    public sealed class VATRPUserGroup : INotifyPropertyChanged, IEquatable<VATRPUserGroup>, IComparable, IComparable<VATRPUserGroup>
    {
        private string _name;
        private int _id;
        private int _id_forUI;
        private string _interests;
        private bool _isFavorite;
        private string _description;

        public VATRPUserGroup()
        {
            this._id = -1;
            this._id_forUI = -1;
        }

        public VATRPUserGroup(int groupID)
        {
            this._id = groupID;
            this._id_forUI = -1;
        }

        public int ID
        {
            get
            {
                return this._id;
            }
            set
            {
                this._id = value;
                OnPropertyChanged("ID");
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
                OnPropertyChanged("Name");
            }
        }

        public string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                this._description = value;
                OnPropertyChanged("Description");
            }
        }

        public bool IsFavorite
        {
            get
            {
                return this._isFavorite;
            }
            set
            {
                if (this._isFavorite != value)
                {
                    this._isFavorite = value;
                    OnPropertyChanged("IsFavorite");
                }
            }
        }

        public string Interests
        {
            get { return _interests; }
            set
            {
                _interests = value;
                OnPropertyChanged("Interests");
            }
        }

        public int CompareTo(VATRPUserGroup other)
        {
            return ID.CompareTo(other.ID);
        }

        public bool Equals(VATRPUserGroup other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }
            return (object.ReferenceEquals(this, other) || (ID == other.ID));
        }

        public override bool Equals(object obj)
        {
            return ((obj is VATRPUserGroup) && this.Equals(obj as VATRPUserGroup));
        }


        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }


        public static bool operator ==(VATRPUserGroup one, VATRPUserGroup two)
        {
            return (object.ReferenceEquals(one, two) || one.Equals(two));
        }

        public static bool operator !=(VATRPUserGroup one, VATRPUserGroup two)
        {
            return !(one == two);
        }

        int IComparable.CompareTo(object obj)
        {
            if (!(obj is VATRPUserGroup))
            {
                throw new ArgumentException("Argument is not a GroupElement", "obj");
            }
            return this.CompareTo((VATRPUserGroup)obj);
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        
        public void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

    }
}

