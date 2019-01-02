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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VATRP.LinphoneWrapper.Enums;

namespace VATRP.LinphoneWrapper
{
    public static partial class LinphoneAPI
    {
        /** Inserts a new element containing data to the end of a given list
 * @param list list where data should be added. If NULL, a new list will be created.
 * @param data data to insert into the list
 * @return first element of the list
**/

        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_list_append(IntPtr list, IntPtr data);

        /** Inserts given element to the end of a given list
         * @param list list where data should be added. If NULL, a new list will be created.
         * @param new_elem element to append
         * @return first element of the list
        **/

        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_list_append_link(IntPtr list, IntPtr new_elem);

        /** Inserts a new element containing data to the start of a given list
         * @param list list where data should be added. If NULL, a new list will be created.
         * @param data data to insert into the list
         * @return first element of the list - the one which was just created.
        **/

        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_list_prepend(IntPtr list, IntPtr data);

        /** Frees all elements of a given list
         * Note that data contained in each element will not be freed. If you need to clean
         * them, consider using @ms_list_free_with_data
         * @param list object to free.
         * @return NULL
        **/

        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_list_free(IntPtr list);

        /** Concatenates second list to the end of first list
         * @param first First list
         * @param second Second list to append at the end of first list.
         * @return first element of the merged list
        **/

        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_list_concat(IntPtr first, IntPtr second);

        /** Finds and remove the first element containing the given data. Nothing is done if element is not found.
         * @param list List in which data must be removed
         * @param data Data to remove
         * @return first element of the modified list
        **/

        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_list_remove(IntPtr list, IntPtr data);

        /** Returns size of a given list
         * @param list List to measure
         * @return Size of list
        **/

        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ms_list_size(IntPtr list);

        /** Finds and remove given element in list.
         * @param list List in which element must be removed
         * @param element element to remove
         * @return first element of the modified list
        **/

        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_list_remove_link(IntPtr list, IntPtr elem);

        /** Finds first element containing data in the given list.
         * @param list List in which element must be found
         * @param data data to find
         * @return element containing data, or NULL if not found
        **/

        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_list_find(IntPtr list, IntPtr data);

        /** Returns the nth element data of the list
         * @param list List object
         * @param index data index which must be returned.
         * @return Element at the given index. NULL if index is invalid (negative or greater or equal to ms_list_size).
        **/

        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_list_nth_data(IntPtr list, int index);

        /** Returns the index of the given element
         * @param list List object
         * @param elem Element to search for.
         * @return Index of the given element. -1 if not found.
        **/

        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ms_list_position(IntPtr list, IntPtr elem);

        /** Returns the index of the first element containing data
         * @param list List object
         * @param data Data to search for.
         * @return Index of the element containing data. -1 if not found.
        **/

        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ms_list_index(IntPtr list, IntPtr data);

        /** Inserts a new element containing data before the given element
         * @param list list where data should be added. If NULL, a new list will be created.
         * @param before element parent to the one we will insert.
         * @param data data to insert into the list
         * @return first element of the modified list.
        **/

        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_list_insert(IntPtr list, IntPtr before, IntPtr data);

        /** Copies a list in another one, duplicating elements but not data
         * @param list list to copy
         * @return Newly created list
        **/

        [DllImport("mediastreamer_base.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ms_list_copy(IntPtr list);


    }
}
