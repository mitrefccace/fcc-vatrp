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

namespace VATRP.Core.Services
{
    public class ProviderService : IProviderService
    {
        #region Members
        private static readonly ILog LOG = LogManager.GetLogger(typeof(ProviderService));
        private readonly ServiceManagerBase manager;
        private List<VATRPServiceProvider> providersList;
        private readonly string fileFullPath;
        private bool loading;
        private bool _isStarted;

        private bool _isStopping;
        private bool _isStopped;

        private readonly Timer deferredSaveTimer;

        private readonly XmlSerializer xmlSerializer;

        #endregion

        #region Methods

        public ProviderService(ServiceManagerBase manager)
        {
            this.manager = manager;
            this.loading = false;
            _isStarted = false;
            this.fileFullPath = this.manager.BuildStoragePath("providers.xml");

            this.deferredSaveTimer = new Timer(500) { AutoReset = false };
            this.deferredSaveTimer.Elapsed += delegate
            {
                this.manager.Dispatcher.Invoke((System.Threading.ThreadStart)delegate
                {
                    this.ImmediateSave();
                }, null);
            };
            this.xmlSerializer = new XmlSerializer(typeof(List<VATRPServiceProvider>));
        }

        private void DeferredSave()
        {
            this.deferredSaveTimer.Stop();
            this.deferredSaveTimer.Start();
        }

        private bool ImmediateSave()
        {
            lock (this.providersList)
            {
                try
                {
                    using (var writer = new StreamWriter(this.fileFullPath))
                    {
                        this.xmlSerializer.Serialize(writer, this.providersList);
                        writer.Flush();
                        writer.Close();
                    }
                    return true;
                }
                catch (IOException ioe)
                {
                    LOG.Error("Failed to save providers", ioe);
                }
                
            }
            return false;
        }
#endregion

        #region IService

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
        public event EventHandler<EventArgs> ServiceStarted;

        public event EventHandler<EventArgs> ServiceStopped;
        public bool Start()
        {
            if (IsStarting || IsStopping)
                return false;

            this.LoadProviders();
            if (ServiceStarted != null)
                ServiceStarted(this, EventArgs.Empty);
            return true;
        }

        public bool Stop()
        {
            if (IsStarting || IsStopping)
                return false;
            _isStopping = true;
            _isStopped = true;
            if (ServiceStopped != null)
                ServiceStopped(this, EventArgs.Empty);
            return true;
        }
        #endregion

        /**
         * \brief Load provider information from the provider config file.
         * 
         * If the provider config file does not exist then create a new config file
         * and populate it with the default providers.
         * 
         * @return void.
         */
        #region IProviderService
        private void LoadProviders()
        {
            this.loading = true;

            try
            {
                if (!File.Exists(this.fileFullPath))
                {
                    File.Create(this.fileFullPath).Close();
                    // create xml declaration
                    this.providersList = new List<VATRPServiceProvider>();
                    this.ImmediateSave();
                }
                using (var reader = new StreamReader(this.fileFullPath))
                {
                    try
                    {
                        this.providersList = this.xmlSerializer.Deserialize(reader) as List<VATRPServiceProvider>;
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

            VATRPServiceProvider CustomProvider = new VATRPServiceProvider();
            CustomProvider.Label = "Custom";
            CustomProvider.Address = null;
            AddProvider(CustomProvider);

            this.loading = false;
            _isStarted = true;
        }

        /**
         * \brief Add a provider to the provider list if it does not already exist.
         * 
         * @return True always.
         */
        public bool AddProvider(VATRPServiceProvider provider)
        {
            var query = from record in this.providersList where 
                            record.Label == provider.Label
                        select record;

            if (query.Any())
            {
                return true;
            }

            this.providersList.Insert(0, provider);
            this.DeferredSave();
            return true;
        }

        public bool DeleteProvider(VATRPServiceProvider provider)
        {
            if (string.IsNullOrEmpty(provider.Label))
                return false;

            var query = from record in this.providersList
                        where
                            record.Label == provider.Label 
                        select record;

            if (!query.Any())
            {
                return false;
            }

            this.providersList.Remove(provider);
            this.DeferredSave();
            return true;
        }

        public bool ContainsProvider(VATRPServiceProvider provider)
        {
            if (this.providersList == null)
                return false;

            var query = from record in this.providersList
                        where
                            record.Label == provider.Label 
                        select record;

            if (query.Any())
            {
                return true;
            }


            IEnumerable<VATRPServiceProvider> allItems = (from c in this.providersList
                                                          where c.Label == provider.Label 
                                                  select c).ToList();

            foreach (var c in allItems)
            {
                providersList.Remove(c);
            }

            this.DeferredSave();

            return false;
        }

        public int GetProvidersCount()
        {
            var VATRPproviders = this.providersList;
            if (VATRPproviders != null)
            {
                IEnumerable<VATRPServiceProvider> providers = (from c in VATRPproviders
                    select c).ToList();
                return providers.Count();
            }
            return 0;
        }

        public void ClearProvidersList()
        {
            this.providersList.Clear();
            this.DeferredSave();
        }

        public VATRPServiceProvider FindProvider(string providerLabel)
        {
            IEnumerable<VATRPServiceProvider> allproviders = (from c in this.providersList
                                                              where c.Label == providerLabel
                                                  select c).ToList();
            return allproviders.FirstOrDefault();
        }

        /**
         * @brief If there is already a provider within the providersList that matches
         * the providerLabel then return that provider, otherwise return null.
         * 
         * A provider is defined as "inside" the provider list if either the provider
         * label or the provider address match the providerLabel string.
         * 
         * @param labelOrAddress The address or label to find in the list of Providers.
         * 
         * @return the VATRPServiceProvider inside the Provider List if it exists, else
         * null.
         */
        public VATRPServiceProvider FindProviderLooseSearch(string labelOrAddress)
        {
            if (string.IsNullOrEmpty(labelOrAddress))
            {
                return null;
            }
            foreach (VATRPServiceProvider provider in providersList)
            {
                if (!string.IsNullOrEmpty(provider.Label) && provider.Label.Contains(labelOrAddress))
                {
                    return provider;
                }
                if (!string.IsNullOrEmpty(provider.Address) && provider.Address.Equals(labelOrAddress))
                {
                    return provider;
                }
            }
            return null;
        }

        public void Save()
        {
            this.DeferredSave();
        }
        #endregion


        public string[] GetProviderList()
        {
            if (this.providersList == null)
                return null;

            var providers = this.providersList.Select(c => c.Label).ToList();

            return providers.ToArray();
        }

        public List<VATRPServiceProvider> GetProviderListFullInfo()
        {
            return providersList;
        }
    }
}
