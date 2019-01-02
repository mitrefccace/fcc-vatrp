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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using com.vtcsecure.ace.windows.CustomControls;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Events;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using System.Windows.Data;
using com.vtcsecure.ace.windows.Model;
using VATRP.Core.Extensions;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class CallHistoryViewModel : ViewModelBase
    {
        private ICollectionView _callsListView;
        private IHistoryService _historyService;
        private IContactsService _contactService;
        private ObservableCollection<HistoryCallEventViewModel> _callHistoryList;
        private string _eventSearchCriteria = string.Empty;
        private DialpadViewModel _dialpadViewModel;
        private double _historyPaneHeight;
        private HistoryCallEventViewModel _selectedCallEvent;
        private int _activeTab;
        private int _unseenMissedCallsCount;
        public event EventHandler MissedCallsCountChanged;

        public CallHistoryViewModel()
        {
            _activeTab = 0; // All tab is active by default
            _unseenMissedCallsCount = 0;
            _callsListView = CollectionViewSource.GetDefaultView(this.Calls);
            _callsListView.SortDescriptions.Add(new SortDescription("SortDate", ListSortDirection.Descending));
            _callsListView.Filter = new Predicate<object>(this.FilterEventsList);
            _historyPaneHeight = 150;
        }
        public CallHistoryViewModel(IHistoryService historyService, DialpadViewModel dialpadViewModel):
            this()
        {
            _historyService = historyService;
            _contactService = ServiceManager.Instance.ContactService;
            _contactService.ContactAdded += OnNewContactAdded;
            _contactService.ContactsChanged += OnContactChanged;
            _contactService.ContactRemoved += OnContactRemoved;
            _historyService.OnCallHistoryEvent += CallHistoryEventChanged;
            _dialpadViewModel = dialpadViewModel;
            _dialpadViewModel.PropertyChanged += OnDialpadPropertyChanged;
        }

        private void OnContactRemoved(object sender, ContactRemovedEventArgs e)
        {
            //*************************************************************************************************************************************************
            // When Contact removed from Contact list.
            //*************************************************************************************************************************************************
            if (ServiceManager.Instance.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke((Action)(() => this.OnContactRemoved(sender, e)));
                return;
            }

            lock (this.Calls)
            {
                foreach (var call in Calls)
                {
                    if (call.CallEvent.Contact != null && call.CallEvent.Contact.ID == e.contactId.ID)
                    {
                        call.UpdateContact(null);
                        call.AllowAddContact = true;
                    }
                }
            }
            CallsListView.Refresh();
        }

        private void OnNewContactAdded(object sender, ContactEventArgs e)
        {

            //************************************************************************************************************************************
            // On New contact added in Call History and Chat.
            //************************************************************************************************************************************
            if (ServiceManager.Instance.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke((Action)(() => this.OnNewContactAdded(sender, e)));
                return;
            }

            // search and update all items
            var contact = _contactService.FindContact(new ContactID(e.Contact));
            if (contact != null && contact.IsLinphoneContact)
            {
                lock (this.Calls)
                {
                    foreach (var call in Calls)
                    {
                        if (call.CallEvent.RemoteParty.TrimSipPrefix() == contact.ID)
                        {
                            call.UpdateContact(contact);
                            call.AllowAddContact = false;
                        }
                    }
                }
                CallsListView.Refresh();
            }
        }
        private void OnContactChanged(object sender, ContactEventArgs e)
        {
            if (ServiceManager.Instance.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke((Action)(() => this.OnContactChanged(sender, e)));
                return;
            }

            // search and update all items
            var contact = _contactService.FindContact(new ContactID(e.Contact));
            if (contact != null && contact.IsLinphoneContact)
            {
                lock (this.Calls)
                {
                    foreach (var call in Calls)
                    {
                        if (call.CallEvent.RemoteParty.TrimSipPrefix() == contact.ID && call.Contact == null)
                        {
                            call.UpdateContact(contact);
                            call.AllowAddContact = false;
                        }
                    }
                    CallsListView.Refresh();
                }
            }
        }

        private void OnDialpadPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RemotePartyNumber")
            {
                EventSearchCriteria = _dialpadViewModel.RemotePartyNumber;
            }
        }

        private void CallHistoryEventChanged(object sender, VATRP.Core.Events.VATRPCallEventArgs e)
        {
            if (ServiceManager.Instance.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke((Action)(() => this.CallHistoryEventChanged(sender, e)));
                return;
            }

            var callEvent = sender as VATRPCallEvent;

            switch (e.historyEventType)
            {
                case HistoryEventTypes.Add:
                    AddNewCallEvent(callEvent, true);
                    break;
                case HistoryEventTypes.Load:
                    LoadCallEvents();
                    break;
                case HistoryEventTypes.Reset:
                    Calls.Clear();
                    _unseenMissedCallsCount = 0;
                    CallsListView.Refresh();
                    break;
                case HistoryEventTypes.Delete:
                    RemoveCallEvent(callEvent);
                    break;
            }

        }
        public void LoadCallEvents()
        {
            if (_historyService.AllCallsEvents == null)
                return;
           
           
            lock (this.Calls)
            {
                var callsItemDB = from VATRPCallEvent call in _historyService.AllCallsEvents
                    orderby call.StartTime descending
                    select call;

                foreach (var avCall in callsItemDB)
                {
                    try
                    {
                        AddNewCallEvent(avCall);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception on LoadAllCalls: " + ex.Message);
                    }
                }
            }

        }

        private void AddNewCallEvent(VATRPCallEvent callEvent, bool refreshNow = false)
        {
            if (FindCallEvent(callEvent) != null)
                return;


            long time_uts;
            var lastSeenDate = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.LAST_MISSED_CALL_DATE, string.Empty);

            if (!long.TryParse(lastSeenDate, out time_uts))
            {
                time_uts = 0;
            }

            var contact = _contactService.FindContact(new ContactID(callEvent.RemoteParty.TrimSipPrefix(), IntPtr.Zero));
            lock (this.Calls)
            {
                Calls.Add(new HistoryCallEventViewModel(callEvent, contact));
                if (callEvent.Status == VATRPHistoryEvent.StatusType.Missed)
                {
                    if (callEvent.EndTime.Ticks > time_uts)
                    {
                        _unseenMissedCallsCount++;

                        if (MissedCallsCountChanged != null)
                            MissedCallsCountChanged(callEvent, EventArgs.Empty);
                    }
                }
            }

            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
    Configuration.ConfEntry.LAST_MISSED_CALL_DATE, DateTime.UtcNow.Ticks.ToString());

            if (refreshNow)
                CallsListView.Refresh();
        }

        private object FindCallEvent(VATRPCallEvent callEvent)
        {
            lock (this.Calls)
            {
                foreach (var call in Calls)
                {
                    if (call.CallEvent == callEvent)
                    {
                        return callEvent;
                    }
                }
            }
            return null;
        }

        private void RemoveCallEvent(VATRPCallEvent callEvent)
        {
            lock (this.Calls)
            {
                foreach (var call in Calls)
                {
                    if (call.CallEvent == callEvent)
                    {
                        Calls.Remove(call);
                        if (callEvent.Status == VATRPHistoryEvent.StatusType.Missed)
                        {
                            _unseenMissedCallsCount--;
                            if (_unseenMissedCallsCount < 0)
                                _unseenMissedCallsCount = 0;
                            if (MissedCallsCountChanged != null)
                                MissedCallsCountChanged(null, EventArgs.Empty);
                        }
                        CallsListView.Refresh();
                        break;
                    }
                }
            }
        }

        public void AddNewContact(HistoryCallEventViewModel callEventViewModel)
        {
            if (!ServiceManager.Instance.ContactService.IsEditing())
            {
                var remote = callEventViewModel.CallEvent.RemoteParty.TrimSipPrefix();
                ContactEditViewModel model = new ContactEditViewModel(false, remote, string.Empty);
                model.ContactName = callEventViewModel.DisplayName;
                var contactEditView = new com.vtcsecure.ace.windows.Views.ContactEditView(model);
                var dialogResult = contactEditView.ShowDialog();
                if (dialogResult != null && dialogResult.Value)
                {
                    var contact = ServiceManager.Instance.ContactService.FindContact(new ContactID(model.ContactSipAddress, IntPtr.Zero));
                    if (contact != null && contact.Fullname == model.ContactName)
                        return;

                    ServiceManager.Instance.ContactService.AddLinphoneContact(model.ContactName, model.ContactSipUsername,
                        model.ContactSipAddress);
                }
            }
        }

        public bool FilterEventsList(object item)
        {
            var callModel = item as HistoryCallEventViewModel;
            if (callModel != null)
            {

                if (callModel.CallEvent != null && ActiveTab == 1 && callModel.CallEvent.Status != VATRPHistoryEvent.StatusType.Missed)
                    return false;
                if (callModel.Contact != null)
                {
                    if (callModel.DisplayName.ToLower().Contains(EventSearchCriteria.ToLower()))
                        return true;
                }
                return callModel.CallEvent != null && callModel.CallEvent.Username.ToLower().Contains(EventSearchCriteria.ToLower());
            }
            return true;
        }

        public ICollectionView CallsListView
        {
            get { return this._callsListView; }
            private set
            {
                if (value == this._callsListView)
                {
                    return;
                }

                this._callsListView = value;
                OnPropertyChanged("CallsListView");
            }
        }

        public ObservableCollection<HistoryCallEventViewModel> Calls
        {
            get { return _callHistoryList ?? (_callHistoryList = new ObservableCollection<HistoryCallEventViewModel>()); }
            set { _callHistoryList = value; }
        }

        public string EventSearchCriteria
        {
            get { return _eventSearchCriteria; }
            set
            {
                _eventSearchCriteria = value;
                CallsListView.Refresh();
            }
        }

        public double HistoryPaneHeight
        {
            get { return _historyPaneHeight; }
            set
            {
                _historyPaneHeight = value;
                OnPropertyChanged("HistoryPaneHeight");
            }
        }

        public HistoryCallEventViewModel SelectedCallEvent
        {
            get { return _selectedCallEvent; }
            set
            {
                _selectedCallEvent = value; 
                OnPropertyChanged("SelectedCallEvent");
            }
        }

        public int ActiveTab
        {
            get { return _activeTab; }
            set
            {
                _activeTab = value;
                CallsListView.Refresh();
                OnPropertyChanged("ActiveTab");
            }
        }

        public int UnseenMissedCallsCount
        {
            get { return _unseenMissedCallsCount; }
        }

        public void ResetLastMissedCallTime()
        {
            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
    Configuration.ConfEntry.LAST_MISSED_CALL_DATE, DateTime.UtcNow.Ticks.ToString());
            _unseenMissedCallsCount = 0;
        }
    }
}