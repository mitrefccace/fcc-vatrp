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
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace VATRP.Core.Utilities
{
    public static class DataProtectionHelper
    {
        public static string Protect(string clearText, string optionalEntropy = null, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        {
            if (clearText == null)
            {
                return "";  // if nothing is passed in to encry, just return an empty string.
            }

            byte[] encryptedBytes = GetProtectedBytes(clearText, optionalEntropy, scope);
            return Convert.ToBase64String(encryptedBytes);
        }

        public static byte[] GetProtectedBytes(string clearText, string optionalEntropy = null, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        {
            byte[] clearBytes = Encoding.UTF8.GetBytes(clearText);
            byte[] entropyBytes = string.IsNullOrEmpty(optionalEntropy)
                ? null
                : Encoding.UTF8.GetBytes(optionalEntropy);
            byte[] encryptedBytes = ProtectedData.Protect(clearBytes, entropyBytes, scope);
            return encryptedBytes;
        }

        public static bool WriteProtectedBytesToFile(string fileName, string clearText)
        {
            byte[] encryptedBytes = GetProtectedBytes(clearText);
            try
            {
                // Open file for reading
                System.IO.FileStream fileStream = new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                // Writes a block of bytes to this stream using data from a byte array.
                fileStream.Write(encryptedBytes, 0, encryptedBytes.Length);

                // close file stream
                fileStream.Close();

                return true;
            }
            catch (Exception exception)
            {
                // Error - but this just means that we will not store the password
                Console.WriteLine("Exception caught in process: {0}", exception.ToString());
            }

            // error occured, return false
            return false;
        }


        public static string Unprotect(string encryptedText, string optionalEntropy = null, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        {
            if (encryptedText == null)
                return ""; // if there is no text, then return an empty string
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            string clearString = GetUnprotectedStringFromBytes(encryptedBytes, optionalEntropy, scope);
            return clearString;
        }

        public static string GetUnprotectedStringFromBytes(byte[] encryptedBytes, string optionalEntropy = null, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        {
            byte[] entropyBytes = string.IsNullOrEmpty(optionalEntropy)
                ? null
                : Encoding.UTF8.GetBytes(optionalEntropy);
            byte[] clearBytes = ProtectedData.Unprotect(encryptedBytes, entropyBytes, scope);
            return Encoding.UTF8.GetString(clearBytes);
        }

        public static string ReadUnprotectedBytesFromProtectedFile(string fileName)
        {
            // first read the bytes if the file exists.
            if (!File.Exists(fileName))
                return "";
            try
            {
                byte[] bytes = System.IO.File.ReadAllBytes(fileName);
                if ((bytes != null) && (bytes.Length > 0))
                {
                    return GetUnprotectedStringFromBytes(bytes);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to read the bytes from the file " + fileName);
            }
            return "";
        }
    }
}