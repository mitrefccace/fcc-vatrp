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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace VATRP.Core.Extensions
{
    public static class CollectionExtensions
    {
        public static string AsString(this byte[] bytes)
        {
            StringBuilder builder = null;
            if ((bytes != null) && (bytes.Length > 0))
            {
                builder = new StringBuilder(bytes.Length);
                foreach (byte num in bytes)
                {
                    builder.Append((char) num);
                }
            }
            if ((builder != null) && (builder.Length > 0))
            {
                return builder.ToString();
            }
            return string.Empty;
        }

        public static void InsertByIndex<T>(this IList<T> itemList, T item, int index)
        {
            if (((item != null) && (itemList != null)) && (index >= 0))
            {
                if (index >= itemList.Count)
                {
                    itemList.Add(item);
                }
                else
                {
                    itemList.Insert(index, item);
                }
            }
        }

        public static void InsertToTop<T>(this IList<T> itemList, T item)
        {
            if ((item != null) && (itemList != null))
            {
                itemList.InsertByIndex<T>(item, 0);
            }
        }

        public static void ReplaceToTop<T>(this ObservableCollection<T> itemList, T element)
        {
            if ((element != null) && (itemList.Count != 0))
            {
                lock (itemList)
                {
                    int index = itemList.IndexOf(element);
                    if (index > 0)
                    {
                        itemList.Move(index, 0);
                    }
                }
            }
        }
    }
}

