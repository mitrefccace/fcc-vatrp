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
using System.CodeDom;
using System.Collections.ObjectModel;
using System.Windows;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Model;
using Prism.Commands;
using System.Windows.Controls;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class DialpadViewModel : ViewModelBase
    {
        private string _remotePartyNumber = string.Empty;
        private bool _allowAudioCall = false;
        private bool _allowVideoCall = false;
        private int _remotePartyDigitLimit = 1000;
        private VATRPCallState _callState;
        private ObservableCollection<ProviderViewModel> _providers;
        private ProviderViewModel _selectedProvider;
        private int _selectedProviderIndex;
        public DelegateCommand ChangeProviderCommand => new DelegateCommand(this.ChangeDialAroundProvider);

        public DialpadViewModel()
        {
            _callState = VATRPCallState.Closed;
            ServiceManager.Instance.ProviderService.ServiceStarted += OnProvidersListLoaded;
        }

        public void UpdateProvider()
        {
            OnProvidersListLoaded(this, EventArgs.Empty);
        }

        private void OnProvidersListLoaded(object sender, EventArgs args)
        {

            //**************************************************************************************************
            // When provider list loading is completed.
            //*************************************************************************************************
            Providers.Clear();
            var providersList = ServiceManager.Instance.ProviderService.GetProviderListFullInfo();
            providersList.Sort((a, b) => a.Label.CompareTo(b.Label));
            var selectedprovider = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL, Configuration.ConfEntry.CURRENT_PROVIDER, "");

            var provider = ServiceManager.Instance.ProviderService.FindProviderLooseSearch(selectedprovider);

            VATRPServiceProvider emptyProvider = new VATRPServiceProvider();
            emptyProvider.Label = "None";
            Providers.Add(new ProviderViewModel(emptyProvider));
            foreach (var s in providersList)
            {
                if (s.Address == "_nologo" || s.Label == "_nologo")
                    continue; //If provider logo is not available then continue with next provider.
                if (s.Address == null)
                    continue; //If the provider doesn't have an address, then we can't use it for dial-around
                var providerModel = new ProviderViewModel(s);
                Providers.Add(providerModel);

                if (provider == null && s.Address == App.CurrentAccount.ProxyHostname)
                {
                    selectedprovider = s.Label;
                    provider = s;
                }

                if (s.Label == selectedprovider)
                    SelectedProvider = providerModel;
            }

            if (_selectedProvider == null)
                if (Providers != null && Providers.Count > 0)
                {
                    _selectedProvider = Providers[0];
                }
            SelectedProviderIndex = 0;
        }

        private void ChangeDialAroundProvider()
        {
            if(SelectedProviderIndex > -1)
            {
                ProviderViewModel selectedProvider = Providers[SelectedProviderIndex];
                App.CurrentAccount.DialAroundProviderAddress = selectedProvider?.Provider.Address;
            }
        }

        #region Properties

        public String RemotePartyNumber
        {
            get { return _remotePartyNumber; }
            set
            {

                if (_remotePartyNumber == value)
                    return;

                if (_remotePartyNumber.Length > _remotePartyDigitLimit)
                    return;

                _remotePartyNumber = value;
                AllowAudioCall = !string.IsNullOrWhiteSpace(_remotePartyNumber);
                AllowVideoCall = !string.IsNullOrWhiteSpace(_remotePartyNumber);
                OnPropertyChanged("RemotePartyNumber");
            }
        }

        public bool AllowAudioCall
        {
            get { return _allowAudioCall; }
            set
            {
                _allowAudioCall = value;
                OnPropertyChanged("AllowAudioCall");
            }
        }
        public bool AllowVideoCall
        {
            get { return _allowVideoCall && App.CanMakeVideoCall; }
            set
            {
                _allowVideoCall = value;
                OnPropertyChanged("AllowVideoCall");
            }
        }

        public VATRPCallState CallState
        {
            get { return _callState; }
            set
            {
                _callState = value;
                OnPropertyChanged("CallState");
            }
        }

        public ObservableCollection<ProviderViewModel> Providers
        {
            get { return _providers ?? (_providers = new ObservableCollection<ProviderViewModel>()); }
            set
            {
                _providers = value;
                OnPropertyChanged("Providers");
            }
        }

        public ProviderViewModel SelectedProvider
        {
            get { return _selectedProvider; }
            set
            {
                _selectedProvider = value;
                ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                    Configuration.ConfEntry.CURRENT_PROVIDER, value.Provider.Label);
                OnPropertyChanged("SelectedProvider");
            }
        }

        public int SelectedProviderIndex
        {
            get { return _selectedProviderIndex; }
            set
            {
                _selectedProviderIndex = value;
                OnPropertyChanged("SelectedIndex");
            }
        }
        #endregion

    }
}