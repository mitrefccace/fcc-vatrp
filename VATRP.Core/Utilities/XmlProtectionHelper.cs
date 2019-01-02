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
using System.Xml;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;

namespace VATRP.Core.Utilities
{
    public class XmlProtectionHelper
    {
        #region Members
        private TripleDESCryptoServiceProvider _tDESkey;
        private SymmetricAlgorithm _alg;
        private XmlDocument _xmlDoc;
        private string _keyName;
        private string _applicationFileType;
        private string _applicationDataPath;
        private byte[] _entropy;
        private bool _keySaved;

        #endregion

        /**
         * @brief Constructor for decoding the encrypted VATRP configuration xml. 
         * 
         * @param appFileType the type of application file.
         */
        public XmlProtectionHelper(string appFileType)
        {
            _tDESkey = new TripleDESCryptoServiceProvider();
            _keyName = "TDESkey";
            _keySaved = false;
            _alg = _tDESkey;
            _xmlDoc = new XmlDocument();
            _xmlDoc.PreserveWhitespace = true;
            _applicationFileType = appFileType;
            _entropy = new byte[] { 9,8,7,6,5};
        }

        #region Properties 
        public TripleDESCryptoServiceProvider TDESkey
        {
            get { return _tDESkey; }
            set { _tDESkey = value; }
        }

        public XmlDocument XmlDoc
        {
            get { return _xmlDoc; }
            set { _xmlDoc = value; }
        }

        public string KeyName
        {
            get { return _keyName; }
            set { _keyName = value; }
        }

        public bool KeySaved
        {
            get { return _keySaved; }
            set { _keySaved = value; }
        }

        public SymmetricAlgorithm Alg
        {
            get { return _alg; }
            set { _alg = value; }
        }

        public byte[] Entropy
        {
            get { return _entropy; }
            set { _entropy = value; }
        }

        public string ApplicationDataPath
        {
            get
            {
                if (_applicationDataPath == null)
                {
                    String applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    _applicationDataPath = Path.Combine(applicationData, "VATRP");
                    Directory.CreateDirectory(_applicationDataPath);
                }
                return _applicationDataPath;
            }
        }

        public string ApplicationFileType
        {
            get { return _applicationFileType; }
            set { _applicationFileType = value; }
        }

        #endregion

        #region Methods

        /**
         * @brief Builds the path to the file containing the encrypted TDES key.
         * 
         * @return The encrypted TDES key.
         */
        public string GetKeyFile()
        {
            string path = ApplicationDataPath;
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    if (path.LastIndexOf(Path.PathSeparator) != (path.Length - 1))
                    {
                        path += Path.DirectorySeparatorChar;
                    }
                    path = String.Concat(path, String.Concat(ApplicationFileType, "Key.dat"));
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            return path;
        }

        // Saves the TDES Key so it can persist in an encrypted state
        private void saveKey()
        {
            FileStream FS = new FileStream(GetKeyFile(), FileMode.Create, FileAccess.Write);
            try
            {
                byte[] encryptedKey = ProtectedData.Protect(TDESkey.Key, Entropy, DataProtectionScope.CurrentUser);
                FS.Write(encryptedKey, 0, encryptedKey.Length);
                FS.Flush();          
            }
            catch (Exception ex)
            {
                throw ex;
            } 
            finally
            {
                FS.Close();
                TDESkey.Clear();
            }
        }

        // Loads the TDES key so the XML file can be Unprotected
        private void loadKey()
        {
            try
            {
                byte[] encryptedKey = File.ReadAllBytes(GetKeyFile());
                TDESkey.Key = ProtectedData.Unprotect(encryptedKey, Entropy, DataProtectionScope.CurrentUser);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Encrypts the XML file
        public void Encrypt(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("Doc");
            }
            else
            {
                XmlDoc.Load(filePath);
            }
            EncryptedXml eXml = new EncryptedXml();
            eXml.AddKeyNameMapping(KeyName, Alg);

            // Find all of the xml elements within the file
            XmlElement root = XmlDoc.DocumentElement;
            XmlNodeList nodes = root.SelectNodes("//*");

            foreach (XmlElement elementToEncrypt in nodes)
            {
                EncryptedData edElement = eXml.Encrypt(elementToEncrypt, KeyName);
                EncryptedXml.ReplaceElement(elementToEncrypt, edElement, false);
            }

            // If you do not save the file then the encryption is not saved
            XmlDoc.Save(filePath);

            // Save the key so the file can be read next time ACE is launched
            saveKey();      
        }

        public bool Decrypt(string filePath)
        {
            bool error;
            if (filePath == null)
            {
                throw new ArgumentNullException("Doc");
            }
            else
            {
                XmlDoc.Load(filePath);
            }

            if (File.Exists(GetKeyFile()))
            {
                loadKey();

                // this will execute successfully even if the file was not encrypted
                EncryptedXml exml = new EncryptedXml(XmlDoc);
                exml.AddKeyNameMapping(KeyName, Alg);
                exml.DecryptDocument();

                // If you do not save the file then the encryption is not saved
                XmlDoc.Save(filePath);
                error = false;
            }
            else
            {
                error = true;
            }
            return error;       
        }
        #endregion 
    }
}