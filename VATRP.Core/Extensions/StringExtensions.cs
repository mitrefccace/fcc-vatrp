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
using System.Diagnostics;
using System.Text;

namespace VATRP.Core.Extensions
{
    public static class StringExtensions
    {
        public static DateTime FromMilliseconds(this string str)
        {
            DateTime minValue = DateTime.MinValue;
            try
            {
                if (str.IsValid())
                {
                    long num2 = Convert.ToInt64(str) * 10000;
                    minValue = new DateTime(num2, (DateTimeKind) DateTimeKind.Utc);
                }
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
            return minValue;
        }

        public static bool IsValid(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        public static bool NotBlank(this string str)
        {
            return (!string.IsNullOrEmpty(str) && (str.Length > 0));
        }

        public static bool ToBool(this string str)
        {
            bool flag = false;
            if (str.NotBlank())
            {
                flag = (str.CompareTo("true") == 0) || (str.CompareTo(bool.TrueString) == 0);
            }
            return flag;
        }

        public static byte[] ToByteArray(this string str)
        {
            if (str.NotBlank())
            {
                try
                {
                    byte[] buffer2 = new byte[str.Length];
                    for (int i = 0; i < str.Length; i++)
                    {
                        buffer2[i] = (byte) str[i];
                    }
                    return buffer2;
                }
                catch
                {
                }
            }
            return null;
        }

        public static T ToEnum<T>(this string str)
        {
            Type type = typeof(T);
            if (str.NotBlank() && Enum.IsDefined(type, str))
            {
                return (T) Enum.Parse(type, str, false);
            }
            return default(T);
        }

        public static int ToInt32(this string str)
        {
            int num = 0;
            try
            {
                if (str.NotBlank())
                {
                    num = Convert.ToInt32(str);
                }
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
            return num;
        }

        public static string UppercaseWords(this string str)
        {
            char[] chArray = str.ToLower().ToCharArray();
            if ((chArray.Length >= 1) && char.IsLower(chArray[0]))
            {
                chArray[0] = char.ToUpper(chArray[0]);
            }
            for (int i = 1; i < chArray.Length; i++)
            {
                if ((chArray[i - 1] == ' ') && char.IsLower(chArray[i]))
                {
                    chArray[i] = char.ToUpper(chArray[i]);
                }
            }
            return (string) new string(chArray);
        }

        public static string TrimSipPrefix(this string str)
        {
            if (str.IndexOf("sip:") == 0)
            {
                str = str.Remove(0, 4);
            }
            return str;
        }
    }
}

