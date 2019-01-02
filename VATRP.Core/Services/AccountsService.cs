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
using System.IO;
using System.Linq;
using System.Timers;
using System.Xml.Serialization;
using log4net;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using VATRP.Core.Utilities;

namespace VATRP.Core.Services
{
    public class AccountService : IAccountService
    {
        #region Members
        private static readonly ILog LOG = LogManager.GetLogger(typeof (AccountService));
        private readonly ServiceManagerBase manager;
        private List<VATRPAccount> accountsList;
        private string fileFullPath;
        private bool loading;
        private bool _isStarted;
        private bool _isStopping;
        private bool _isStopped;

        private readonly Timer deferredSaveTimer;
        private readonly XmlSerializer xmlSerializer;

        private XmlProtectionHelper XmlHelper; // cjm-aug17

        #endregion

        #region Methods
        public List<VATRPAccount> CodecList
        {
            get { return accountsList; }
        }

        public AccountService(ServiceManagerBase manager)
        {
            this.manager = manager;
            this.loading = false;
            _isStarted = false;
            this.fileFullPath = this.manager.BuildStoragePath("accounts.xml");

            this.deferredSaveTimer = new Timer(1000) { AutoReset = false };
            this.deferredSaveTimer.Elapsed += delegate
            {
                this.manager.Dispatcher.Invoke((System.Threading.ThreadStart)delegate
                {
                    this.ImmediateSave();
                }, null);
            };
            this.xmlSerializer = new XmlSerializer(typeof(List<VATRPAccount>));
            this.XmlHelper = new XmlProtectionHelper("account"); // cjm-aug17
        }

        private void DeferredSave()
        {
                this.deferredSaveTimer.Stop();
            this.deferredSaveTimer.Start();
        }

        private bool ImmediateSave()
        {
            lock (this.accountsList)
            {
                LOG.Debug("Saving accounts....");
                try
                {
                    using (var writer = new StreamWriter(this.fileFullPath))
                    {
                        this.xmlSerializer.Serialize(writer, this.accountsList);
                        writer.Flush();
                        writer.Close();
                    }
                    this.XmlHelper.Encrypt(this.fileFullPath);
                    return true;
                }
                catch (IOException ioe)
                {
                    LOG.Error("Failed to save accounts", ioe);
                }
                
            }
            return false;
        }
#endregion

        #region IService

        public event EventHandler<EventArgs> ServiceStarted;

        public event EventHandler<EventArgs> ServiceStopped;

        public bool IsStopping
        {
            get { return _isStopping; }
        }

        public bool IsStopped
        {
            get { return _isStopped; }
        }
        public bool IsStarting
        {
            get
            {
                return loading;
            }
        }

        public bool IsStarted { get { return _isStarted; } }
        public bool Start()
        {
            if (IsStarting || IsStopping)
                return false;

            this.LoadAccounts();
            return true;
        }

        public bool Stop()
        {
            if (IsStarting || IsStopping)
                return false;
            _isStopping = true;
            _isStopped = true;

            // clear all empty accounts
            bool saveAccounts = false;
            do
            {
                var vatrpAccounts = this.accountsList;
                IEnumerable<VATRPAccount> allAccounts = (from c in vatrpAccounts
                                                         where c.Username == string.Empty
                                                         select c).ToList();
                VATRPAccount emptyAccount = allAccounts.FirstOrDefault();
                if (emptyAccount == null)
                    break;
                DeleteAccount(emptyAccount);
                saveAccounts = true;
            } while (true);

            if (saveAccounts)
                ImmediateSave();

            if (ServiceStopped != null)
                ServiceStopped(this, EventArgs.Empty);
            return true;
        }
        #endregion

        #region IAccountService
        private void handleFileMissing()
        {
            LOG.Debug(String.Format("{0} doesn't exist, trying to create new one", this.fileFullPath));
            File.Create(this.fileFullPath).Close();
            // create xml declaration
            this.accountsList = new List<VATRPAccount>();
            this.ImmediateSave();
        }

        private void LoadAccounts()
        {
            this.loading = true;
            LOG.Debug(string.Format("Loading accounts from {0}", this.fileFullPath));

            try
            {
                if (!File.Exists(this.fileFullPath))
                {
                    handleFileMissing();
                }
                // cjm-sep17 -- must unprotect the xml file before reading it
                bool decrypteError = this.XmlHelper.Decrypt(this.fileFullPath);
                if (decrypteError)
                {
                    handleFileMissing();
                }
                using (StreamReader reader = new StreamReader(this.fileFullPath))
                {
                    try
                    {
                        this.accountsList = this.xmlSerializer.Deserialize(reader) as List<VATRPAccount>;
                    }
                    catch (InvalidOperationException ie)
                    {
                        LOG.Error("Failed to load history", ie);

                        reader.Close();
                        File.Delete(this.fileFullPath);
                    }
                }
            }
            catch (Exception e)
            {
                LOG.Error("Failed to load history", e);
                _isStarted = false;
                loading = false;
            }

            this.loading = false;
            _isStarted = true;

            if (ServiceStarted != null)
                ServiceStarted(this, EventArgs.Empty);
        }

        // cjm-sep17 -- need a method to get proper list of accounts
        private void loadProperAccount()
        {
            try
            {
                // cjm-sep17 -- must unprotect the xml file before reading it
                bool decrypteError = this.XmlHelper.Decrypt(this.fileFullPath);
                if (decrypteError)
                {
                    handleFileMissing();
                }
                using (StreamReader reader = new StreamReader(this.fileFullPath))
                {
                    try
                    {
                        this.accountsList = this.xmlSerializer.Deserialize(reader) as List<VATRPAccount>;
                    }
                    catch (InvalidOperationException ie)
                    {
                        LOG.Error("Failed to load history", ie);
                        reader.Close();
                        File.Delete(this.fileFullPath);
                    }
                }
            }
            catch (Exception e)
            {
                LOG.Error("Failed to load history", e);
            }
        }

        public bool AddAccount(VATRPAccount account)
        {
            if (account.AccountType == VATRPAccountType.Unknown)
                return false;

            this.loadProperAccount(); // cjm-sep17 -- trying to persist the changes to the list
            if (this.accountsList != null)
            {
                if (this.accountsList.Exists(x => x.AccountID == account.AccountID && x.AccountType == account.AccountType))
                {
                    return true;
                }
            }
            else
            {
                accountsList = new List<VATRPAccount>();
            }
            this.accountsList.Insert(0, account);
            this.DeferredSave();
            return true;
        }

        public bool DeleteAccount(VATRPAccount account)
        {
            if (string.IsNullOrEmpty(account.AccountID))
                return false;

            var query = from record in this.accountsList
                        where
                            record.AccountID == account.AccountID 
                        select record;

            if (!query.Any())
            {
                return false;
            }

            this.accountsList.Remove(account);
            this.DeferredSave();
            return true;
        }

        public bool ContainsAccount(VATRPAccount account)
        {
            var query = from record in this.accountsList
                        where
                            record.Username == account.Username &&
                            record.AccountType == account.AccountType
                        select record;

            if (query.Any())
            {
                return true;
            }


            IEnumerable<VATRPAccount> allItems = (from c in this.accountsList
                                                  where c.Username == account.Username && 
                                                  c.AccountType == account.AccountType
                                                  select c).ToList();

            foreach (var c in allItems)
            {
                accountsList.Remove(c);
            }

            this.DeferredSave();

            return false;
        }

        public int GetAccountsCount()
        {
            var VATRPAccounts = this.accountsList;
            if (VATRPAccounts != null)
            {
                IEnumerable<VATRPAccount> accounts = (from c in VATRPAccounts
                    select c).ToList();
                return accounts.Count();
            }
            return 0;
        }

        public void ClearAccounts()
        {
            this.accountsList.Clear();
            this.DeferredSave();
        }

        public VATRPAccount FindAccount(string accountUID)
        {
            var vatrpAccounts = this.accountsList;
            if (vatrpAccounts == null)
                return null;
            IEnumerable<VATRPAccount> allAccounts = (from c in vatrpAccounts
                where c.AccountID == accountUID
                select c).ToList();
            return allAccounts.FirstOrDefault();
        }

        public VATRPAccount FindAccount(string username, string hostname)
        {
            this.loadProperAccount(); // cjm-sep17 -- trying to persist the changes to the list
            if (this.accountsList == null)
            {
                return null;
            }
            else
            {
                return this.accountsList.Find(x => x.Username == username && x.ProxyHostname == hostname);
            }
        }

        public void Save()
        {
            this.DeferredSave();
        }
        #endregion

    }
}
