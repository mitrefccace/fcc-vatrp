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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using com.vtcsecure.ace.windows.CustomControls;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Events;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using System.Windows.Data;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Views;
using Microsoft.Win32;
using VATRP.Core.Extensions;
using System.Xml;
using com.vtcsecure.ace.windows.Utilities;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;
using System.IO;
using System.Text;

namespace com.vtcsecure.ace.windows.ViewModel
{
    public class ContactsViewModel : ViewModelBase
    {

        private ICollectionView _contactsListView;
        private IContactsService _contactsService;
        private ObservableCollection<ContactViewModel> _contactsList;
        private string _eventSearchCriteria = string.Empty;
        private DialpadViewModel _dialpadViewModel;
        private double _contactPaneHeight;
        private ContactViewModel _selectedContact;
        private int _activeTab;
        private bool _importInProgress;
        private bool _exportInProgress;
        private Task importTask;
        private Task exportTask;

        public bool ImportInProgress
        {
            get { return _importInProgress; }
            set { _importInProgress = value; }
        }

        public bool ExportInProgress
        {
            get { return _exportInProgress; }
            set { _exportInProgress = value; }
        }

        public ActionCommand TabPanelImportContactsCommand
        {
            get { return new ActionCommand(ExecuteImportCommand, CanExecuteImport); } 
            
        }

        public ActionCommand TabPanelExportContactsCommand
        {
            get { return new ActionCommand(ExecuteExportCommand, CanExecuteExport); }
        }

        public ActionCommand TabPanelAddContactCommand
        {
            get { return new ActionCommand(ExecuteAddCommand, CanExecuteAdd); }
        }

        public ContactList MyContacts { get; set; }

        public ContactsViewModel()
        {
            _activeTab = 0; // All tab is active by default
            _contactsListView = CollectionViewSource.GetDefaultView(this.Contacts);
            _contactsListView.Filter = new Predicate<object>(this.FilterContactsList);
            _contactsListView.SortDescriptions.Add(new SortDescription("ContactUI", ListSortDirection.Ascending));
            _contactPaneHeight = 150;
            ImportInProgress = false;
            ExportInProgress = false;
        }
        public ContactsViewModel(IContactsService contactService, DialpadViewModel dialpadViewModel) :
            this()
        {
            _contactsService = contactService;
            _contactsService.ContactAdded += ContactAdded;
            _contactsService.ContactRemoved += ContactRemoved;
            _contactsService.ContactsChanged += ContactChanged;
            _contactsService.ContactsLoadCompleted += ContactsLoadCompleted;
            _dialpadViewModel = dialpadViewModel;
            _dialpadViewModel.PropertyChanged += OnDialpadPropertyChanged;
            ImportInProgress = false;
            ExportInProgress = false;
        }

        private void ContactsLoadCompleted(object sender, EventArgs e)
        {
            if (ServiceManager.Instance.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke((Action)(() => this.ContactsLoadCompleted(sender, e)));
                return;
            }

            LoadContacts();
            OnPropertyChanged("Contacts");
        }

        private VATRPCredential GetAuthentication()
        {
            AuthenticationView _authForm = new AuthenticationView();
            _authForm.ShowDialog();
            if (!_authForm.Proceed) { return null; }
            VATRPCredential tempCred = new VATRPCredential();
            tempCred.username = _authForm.Username;
            tempCred.password = _authForm.Password;
            return tempCred;
        }

        /// <summary>
        /// Method called when the import button clicked. Prompts
        /// the user if they want to import the contacts from a 
        /// remote location or local one and then calls the 
        /// relevant methods to complete the request.
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>void</returns>
        private void ExecuteImportCommand(object obj)
        {
            if (ServiceManager.Instance.LinphoneService.VCardSupported && !ImportInProgress)
            {
                ImportInProgress = true;
                string msg = "To download your contacts from the Provider, please select Yes. Select No to import from local file system.";
                string caption = "Import Contacts";
                MessageBoxButton button = MessageBoxButton.YesNoCancel;
                MessageBoxResult result = MessageBox.Show(msg, caption, button, MessageBoxImage.Question);
                Dispatcher Dispatcher = Dispatcher.CurrentDispatcher;

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        string uri = App.CurrentAccount.ContactsURI;
                        if (uri == string.Empty)
                        {
                            msg = "Valid URI required to import contacts. Please go to the Settings, Account menu to input a valid URI.";
                            caption = "Import Contacts";
                            button = MessageBoxButton.OK;
                            MessageBox.Show(msg, caption, button, MessageBoxImage.Error);
                            ImportInProgress = false;
                            break;
                        }

                        importTask = Task.Run(async () =>
                        {
                            try
                            {
                                await Dispatcher.BeginInvoke((Action)(() => Mouse.OverrideCursor = Cursors.AppStarting));
                                XmlDocument xDoc = new XmlDocument();
                                xDoc = await JsonWebRequest.MakeXmlWebRequestAsync<XmlDocument>(uri);
                                var recordsImported = ServiceManager.Instance.ContactService.ImportVcardFromXdoc(xDoc);
                            }
                            catch (Exception ex)
                            {
                                try
                                {

                                    System.Net.HttpWebResponse response = ((ex as System.Net.WebException).Response) as System.Net.HttpWebResponse;
                                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                    {
                                        await Dispatcher.BeginInvoke((Action)(() => Mouse.OverrideCursor = Cursors.AppStarting));
                                        VATRPCredential creds = App.CurrentAccount.configuration.FindCredential("contacts", null);
                                        if (creds == null)
                                        {
                                            await Dispatcher.BeginInvoke((Action)delegate ()
                                            {
                                                creds = GetAuthentication();
                                            });
                                        }
                                        XmlDocument xDoc = new XmlDocument();
                                        xDoc = await JsonWebRequest.MakeXmlWebRequestAuthenticatedAsync<XmlDocument>(uri, creds);
                                        var recordsImported = ServiceManager.Instance.ContactService.ImportVcardFromXdoc(xDoc);
                                    }
                                    else
                                    {
                                        throw;
                                    }
                                }
                                catch
                                {
                                    await Dispatcher.BeginInvoke((Action)delegate ()
                                    {
                                        msg = ex.Message;
                                        caption = "Download failed";
                                        button = MessageBoxButton.OK;
                                        MessageBox.Show(msg, caption, button, MessageBoxImage.Error);
                                    });
                                }
                            }
                            finally
                            {
                                await Dispatcher.BeginInvoke((Action)(() => Mouse.OverrideCursor = null));
                                ImportInProgress = false;
                            }
                        });
                        break;
                    case MessageBoxResult.No:
                        importTask = Task.Run((Action)delegate ()
                        {
                            try
                            {
                                Dispatcher.BeginInvoke((Action)(() => Mouse.OverrideCursor = Cursors.AppStarting));
                                ExecuteLocalImport();
                            }
                            catch (Exception ex)
                            {
                                Dispatcher.BeginInvoke((Action)delegate ()
                                {
                                    msg = "Contacts failed to load.";
                                    caption = "Import Error";
                                    button = MessageBoxButton.OK;
                                    MessageBox.Show(msg, caption, button, MessageBoxImage.Error);
                                });
                            }
                            finally
                            {
                                Dispatcher.BeginInvoke((Action)(() => Mouse.OverrideCursor = null));
                                ImportInProgress = false;
                            }
                        });
                        break;
                    case MessageBoxResult.Cancel:
                        ImportInProgress = false;
                        break;
                }
            }
        }

        /// <summary>
        /// Method to import contacts from local machine. Prompts
        /// user to select a contacts file from the computer. Then
        /// it parses the file & stores the contacts.
        /// </summary>
        /// <returns>void</returns>
        private void ExecuteLocalImport()
        {
            var openDlg = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,

                Filter = "vCard Files (*.VCF, *.vcard)|*.VCF;*vcard | xCard Files (*.xml, *.xml)|*.xml;*xml",
                FilterIndex = 0,

                ShowReadOnly = false,
            };

            if (openDlg.ShowDialog() != true)
                return;

            if (ServiceManager.Instance.LinphoneService.VCardSupported)
            {

                // System.Windows.Forms.MessageBox.Show("ExecuteImportCommand Path" + openDlg.FileName);
                var recordsImported = ServiceManager.Instance.ContactService.ImportVCards(openDlg.FileName);
            }
            else
            {
                var cardReader = new vCardReader(openDlg.FileName);

                string un, host;
                int port;

                foreach (var card in cardReader.vCards)
                {
                    var remoteParty = card.Title.TrimSipPrefix();
                    var contact =
                        ServiceManager.Instance.ContactService.FindContact(new ContactID(remoteParty, IntPtr.Zero));
                    if (contact != null && contact.Fullname == card.FormattedName)
                    {
                        continue;
                    }
                    VATRPCall.ParseSipAddress(remoteParty, out un, out host, out port);
                    if ((App.CurrentAccount != null && App.CurrentAccount.ProxyHostname != host) ||
                        App.CurrentAccount == null)
                    {
                        un = remoteParty;
                    }
                    ServiceManager.Instance.ContactService.AddLinphoneContact(card.FormattedName, un,
                        remoteParty);
                }
            }
        }

        private bool CanExecuteImport(object arg)
        {
            return true;
        }

        /// <summary>
        /// Method called when the export button clicked. Prompts
        /// the user if they want to export the contacts to a 
        /// remote location or local one and then calls the 
        /// relevant methods to complete the request.
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>void</returns>
        private void ExecuteExportCommand(object obj)
        {
            //**************************************************************************************************************
            // Exporting Vcard contact from contact list.
            //**************************************************************************************************************
            if (ServiceManager.Instance.LinphoneService.VCardSupported && !ExportInProgress)
            {
                ExportInProgress = true;
                string msg = "To export your contacts to the Provider, please select Yes. Select No to export to local file system.";
                string caption = "Export Contacts";
                MessageBoxButton button = MessageBoxButton.YesNoCancel;
                MessageBoxResult result = MessageBox.Show(msg, caption, button, MessageBoxImage.Question);
                Dispatcher Dispatcher = Dispatcher.CurrentDispatcher;

                switch (result)
                {
                    // Remote Export
                    case MessageBoxResult.Yes:
                        string uri = App.CurrentAccount.ContactsURI;
                        if (uri == string.Empty)
                        {
                            msg = "Valid URI required to export contacts. Please go to the Settings, Account menu to input a valid URI.";
                            caption = "Import Contacts";
                            button = MessageBoxButton.OK;
                            MessageBox.Show(msg, caption, button, MessageBoxImage.Error);
                            ExportInProgress = false;
                            break;
                        }
                        VATRPCredential contactsCredential = App.CurrentAccount.configuration.FindCredential("contacts", null) ?? GetAuthentication();
                        if (contactsCredential == null) { ExportInProgress = false; return; }
                        exportTask = Task.Run(async () =>
                        {
                            try
                            {
                                await Dispatcher.BeginInvoke((Action)(() => Mouse.OverrideCursor = Cursors.AppStarting));
                                await ExecuteRemoteExport(uri, contactsCredential);
                                await Dispatcher.BeginInvoke((Action)delegate ()
                                {
                                    msg = "Upload Successful.";
                                    caption = "Export Status";
                                    button = MessageBoxButton.OK;
                                    MessageBox.Show(msg, caption, button, MessageBoxImage.Information);
                                });
                            }
                            catch(Exception ex)
                            {
                                await Dispatcher.BeginInvoke((Action)delegate ()
                                {
                                    msg = "Upload failed.";
                                    caption = "Export Error";
                                    button = MessageBoxButton.OK;
                                    MessageBox.Show(msg, caption, button, MessageBoxImage.Error);
                                });
                            }
                            finally
                            {
                                await Dispatcher.BeginInvoke((Action)(() => Mouse.OverrideCursor = null));
                                ExportInProgress = false;
                            }
                        });
                        break;

                    // Local Export
                    case MessageBoxResult.No:
                        exportTask = Task.Run((Action)delegate ()
                        {
                            try
                            {
                                Dispatcher.BeginInvoke((Action)(() => Mouse.OverrideCursor = Cursors.AppStarting));
                                ExecuteLocalExport();
                            }
                            catch (Exception ex)
                            {
                                Dispatcher.BeginInvoke((Action)delegate ()
                                {
                                    msg = "Contacts export failed.";
                                    caption = "Export Error";
                                    button = MessageBoxButton.OK;
                                    MessageBox.Show(msg, caption, button, MessageBoxImage.Error);
                                });
                            }
                            finally
                            {
                                Dispatcher.BeginInvoke((Action)(() => Mouse.OverrideCursor = null));
                                ExportInProgress = false;
                            }
                        });
                        break;
                    case MessageBoxResult.Cancel:
                        ExportInProgress = false;
                        break;
                }
            }
            
        }

        private bool CanExecuteExport(object arg)
        {
            return ServiceManager.Instance.ContactService.Contacts.Any(contact => contact.IsLinphoneContact);
        }

        /// <summary>
        /// Method to export contacts to remote uri. Generates the 
        /// xml format for the contacts and provides that to the 
        /// method used to generate the POST request.
        /// </summary>
        /// <param name="uri">string</param>
        /// <param name="cred">VATRPCredential</param>
        /// <returns>void</returns>
        private async Task ExecuteRemoteExport(string uri, VATRPCredential cred)
        {
            var cardWriter = new vCardWriter();
            this.MyContacts = new ContactList();
            this.MyContacts.VCards = new List<vCard>();

            foreach (var contactVM in this.Contacts)
            {
                var card = new vCard()
                {
                    GivenName = contactVM.Contact.Fullname,
                    FormattedName = contactVM.Contact.Fullname,
                    Title = contactVM.Contact.RegistrationName
                };

                card.Telephone.Uri = contactVM.Contact.RegistrationName;

                this.MyContacts.VCards.Add(card);
            }

            // Add the namespaces
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            ns.Add("xsd", "http://www.w3.org/2001/XMLSchema");
            this.MyContacts.Namespaces = ns;

            // Serialize contacts into xml string
            if (this.MyContacts.VCards != null)
            {
                XmlAttributes atts = new XmlAttributes();
                atts.Xmlns = true;

                XmlAttributeOverrides xover = new XmlAttributeOverrides();
                xover.Add(typeof(List<vCard>), "Namespaces", atts);

                XmlSerializer xsSubmit = new XmlSerializer(typeof(List<vCard>), xover);

                var xml = "";

                //XmlWriterSettings settings = new XmlWriterSettings();
                //settings.OmitXmlDeclaration = true; // is this necessary?

                using (var sww = new StringWriter())
                {
                    using (XmlWriter writer = XmlWriter.Create(sww))
                    {
                        xsSubmit.Serialize(writer, this.MyContacts.VCards);
                        xml = sww.ToString(); // the final xml output
                        xml = xml.Replace("ArrayOfVcard", "vcards"); // replace incorrect tag with correct one
                        xml = xml.Replace("utf-16", "utf-8"); // TODO - fix this with formatting 
                    }
                }

                try
                {
                    await JsonWebRequest.MakeXmlWebPostAuthenticatedAsync(uri, cred, xml);
                }
                catch (JsonException ex)
                {
                    Debug.WriteLine($"ERROR -- Failed to POST contacts to {uri}.");
                }
            }
        }

        /// <summary>
        /// Method to export contacts to local machine. Creates
        /// the file the contacts will be saved to and calls methods
        /// that generate the xml formatting for the exported contacts.
        /// </summary>
        /// <returns>void</returns>
        private void ExecuteLocalExport()
        {
            var saveDlg = new SaveFileDialog()
            {
                CheckPathExists = true,
                OverwritePrompt = true,
                FileName = "ace_contacts",
                Filter = "VCF files (*.vcf) | *.vcf| xCard files (*.xml) | *.xml",
                FilterIndex = 0,
            };

            if (saveDlg.ShowDialog() != true)
                return;

            if (ServiceManager.Instance.LinphoneService.VCardSupported)
            {
                //  Added 2/21/207 fjr, save as XML
                if (saveDlg.FileName.EndsWith(".xml"))
                {
                    var cardWriter = new vCardWriter();
                    var vCards = new List<vCard>();

                    foreach (var contactVM in this.Contacts)
                    {
                        var card = new vCard()
                        {
                            GivenName = contactVM.Contact.Fullname,
                            FormattedName = contactVM.Contact.Fullname,
                            Title = contactVM.Contact.RegistrationName
                        };
                        vCards.Add(card);
                    }
                    cardWriter.WriteCardsAsXML(saveDlg.FileName, vCards);
                }
                else
                {
                    ServiceManager.Instance.ContactService.ExportVCards(saveDlg.FileName);
                }
            }
            else
            {
                var cardWriter = new vCardWriter();
                var vCards = new List<vCard>();

                foreach (var contactVM in this.Contacts)
                {
                    var card = new vCard()
                    {
                        GivenName = contactVM.Contact.Fullname,
                        FormattedName = contactVM.Contact.Fullname,
                        Title = contactVM.Contact.RegistrationName
                    };
                    vCards.Add(card);
                }
                cardWriter.WriteCards(saveDlg.FileName, vCards);
            }
        }

        private void ExecuteAddCommand(object obj)
        {
            if (!ServiceManager.Instance.ContactService.IsEditing())
            {
                ContactEditViewModel model = new ContactEditViewModel(true, string.Empty, string.Empty);
                var contactEditView = new ContactEditView(model);
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

        private bool CanExecuteAdd(object arg)
        {
            return true;
        }

        
        private void OnDialpadPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RemotePartyNumber")
            {
                EventSearchCriteria = _dialpadViewModel.RemotePartyNumber;
            }
        }
        
        private void ContactChanged(object sender, ContactEventArgs e)
        {
            if (ServiceManager.Instance.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke((Action)(() => this.ContactChanged(sender, e)));
                return;
            }

            if (ContactsListView != null) 
                ContactsListView.Refresh();
        }

        private void ContactAdded(object sender, ContactEventArgs e)
        {

            //************************************************************************************************************************************
            // Contact added in Contact History and Chat window.
            //************************************************************************************************************************************
            if (ServiceManager.Instance.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke((Action)(() => this.ContactAdded(sender, e)));
                return;
            }

            var contact = _contactsService.FindContact(e.Contact);
            if (contact != null)
            {
                AddContact(contact, true);
            }
        }

        private void OnContactPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsFavorite" && ActiveTab == 1)
            {
                ContactsListView.Refresh();
            }
        }

        private void ContactRemoved(object sender, ContactRemovedEventArgs e)
        {
            //*************************************************************************************************************************************************
            // When Contact removed from Contact list.
            //*************************************************************************************************************************************************
            if (ServiceManager.Instance.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                ServiceManager.Instance.Dispatcher.BeginInvoke((Action)(() => this.ContactRemoved(sender, e)));
                return;
            }

            RemoveContactModel(e.contactId);
        }

        public void LoadContacts()
        {
            if (_contactsService.Contacts == null)
                return;

            lock (this.Contacts)
            {
                //  Changed 5/5/2017 MITRE-fjr Tried changing to ToList()
                foreach (var c in _contactsService.Contacts.ToList())
                {
                    try
                    {
                        AddContact(c);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception on LoadContacts: " + ex.Message);
                    }
                }
            }

        }

        private void AddContact(VATRPContact contact, bool refreshNow = false)
        {
            if (!contact.SipUsername.NotBlank() || !contact.IsLinphoneContact)
                return;

            if (FindContact(contact) != null)
                return;

            lock (this.Contacts)
            {
                contact.PropertyChanged += OnContactPropertyChanged;
                Contacts.Add(new ContactViewModel(contact));
            }

            if (refreshNow)
                ContactsListView.Refresh();
        }

        private object FindContact(VATRPContact contact)
        {
            lock (this.Contacts)
            {
                foreach (var c in Contacts)
                {
                    if (c.Contact == contact)
                    {
                        return contact;
                    }
                }
            }
            return null;
        }

        private void RemoveContactModel(ContactID contactID)
        {

            //*************************************************************************************************************************************************
            // Remove contact and update contact list.
            //*************************************************************************************************************************************************
            lock (this.Contacts)
            {
                foreach (var contact in Contacts)
                {
                    if (contact.Contact == contactID)
                    {
                        contact.PropertyChanged -= OnContactPropertyChanged;
                        Contacts.Remove(contact);
                        ContactsListView.Refresh();
                        break;
                    }
                }
            }
        }

        public bool FilterContactsList(object item)
        {
            var contactModel = item as ContactViewModel;
            if (contactModel != null)
            {
                if (contactModel.Contact != null && ActiveTab == 1 && !contactModel.Contact.IsFavorite)
                    return false;

                if (contactModel.Contact != null)
                {
                    if (contactModel.Contact.Fullname.ToLower().Contains(EventSearchCriteria.ToLower()))
                        return true;
                    return contactModel.Contact.ContactAddress_ForUI.ToLower().Contains(EventSearchCriteria.ToLower());
                }
            }
            return true;
        }

        public ICollectionView ContactsListView
        {
            get { return this._contactsListView; }
            private set
            {
                if (value == this._contactsListView)
                {
                    return;
                }

                this._contactsListView = value;
                OnPropertyChanged("CallsListView");
            }
        }

        public ObservableCollection<ContactViewModel> Contacts
        {
            get { return _contactsList ?? (_contactsList = new ObservableCollection<ContactViewModel>()); }
            set { _contactsList = value; }
        }

        public string EventSearchCriteria
        {
            get { return _eventSearchCriteria; }
            set
            {
                _eventSearchCriteria = value;
                ContactsListView.Refresh();
            }
        }

        public double ContactPaneHeight
        {
            get { return _contactPaneHeight; }
            set
            {
                _contactPaneHeight = value;
                OnPropertyChanged("HistoryPaneHeight");
            }
        }

        public ContactViewModel SelectedContact
        {
            get { return _selectedContact; }
            set
            {
                _selectedContact = value;
                OnPropertyChanged("SelectedContact");
            }
        }
        public int ActiveTab
        {
            get { return _activeTab; }
            set
            {
                _activeTab = value;
                ContactsListView.Refresh();
                OnPropertyChanged("ActiveTab");
            }
        }
    }
}