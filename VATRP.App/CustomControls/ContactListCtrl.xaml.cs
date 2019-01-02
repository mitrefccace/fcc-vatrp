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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using log4net;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.ViewModel;
using VATRP.Core.Interfaces;
using com.vtcsecure.ace.windows.Views;
using Microsoft.Win32;
using VATRP.Core.Extensions;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.CustomControls
{
    /// <summary>
    /// Interaction logic for ContactListCtrl.xaml
    /// </summary>
    public partial class ContactListCtrl : UserControl
    {
        #region Members

        private ContactsViewModel _contactsViewModel;

        #endregion

        #region Events

        public delegate void MakeCallRequestedDelegate(string called_address);

        public event MakeCallRequestedDelegate MakeCallRequested;
        private bool bEditRequest;
        private bool bDeleteRequest;
        private bool bFavoriteRequest;

        #endregion

        public ContactListCtrl()
        {
            InitializeComponent();
        }

        public ContactListCtrl(ContactsViewModel viewModel) :
            this()
        {
            SetDataContext(viewModel);
        }

        public void SetDataContext(ContactsViewModel viewModel)
        {
            _contactsViewModel = viewModel;
            DataContext = viewModel;
        }

        private void OnContactSelected(object sender, SelectionChangedEventArgs e)
        {
            //****************************************************************************************************
            // This method is called when Contact is selected from Contact list (All/Favorite). 
            // If selected contact is not null then application will connect the call to selected person. 
            //****************************************************************************************************

            if (_contactsViewModel.SelectedContact != null)
            {
                if (MakeCallRequested != null && _contactsViewModel.SelectedContact.Contact != null)
                    if (MessageBox.Show("Do you want to initiate a call?", "VATRP", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        MakeCallRequested(_contactsViewModel.SelectedContact.Contact.RegistrationName);
                        
                    }
                _contactsViewModel.SelectedContact = null;
            }
        }

        private void OnEdit(object sender, RoutedEventArgs e)
        {
            //**************************************************************************************************************************************
            //  Edit button clicked on Favorite/All Contacts list. User can change username, email, avatar from this dialog box.
            //**************************************************************************************************************************************

            if (!ServiceManager.Instance.ContactService.IsEditing())
            {
                var contact = ((ToggleButton)sender).Tag as VATRPContact;
                if (contact != null)
                {
                    ContactEditViewModel model = new ContactEditViewModel(false, contact.RegistrationName, contact.Avatar);
                    model.ContactName = contact.Fullname;
                    var contactEditView = new ContactEditView(model);
                    Nullable<bool> dialogResult = contactEditView.ShowDialog();
                    if (dialogResult != null && dialogResult.Value)
                    {
                        if (model.AvatarChanged)
                            contact.Avatar = string.Empty;
                        ServiceManager.Instance.ContactService.EditLinphoneContact(contact,
                            model.ContactName,
                            model.ContactSipAddress);
                    }
                }
            }
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {
            //*******************************************************************************************************************************************************
            // This method is called when delete button is tapped in contact list (All/Favorite). This Remove the contact from All/Favorite contact list.
            //*******************************************************************************************************************************************************

            if (!ServiceManager.Instance.ContactService.IsEditing())
            {
                var contact = ((ToggleButton)sender).Tag as VATRPContact;
                if (contact != null)
                {
                    //****************************************************************************************************
                    // If User select Yes then contact will be deleted.
                    //****************************************************************************************************
                    if (MessageBox.Show("Are you sure you want to remove/delete contact?", "VATRP", MessageBoxButton.OKCancel,
                        MessageBoxImage.Question) == MessageBoxResult.OK)
                    {
                       // ServiceManager.Instance.LinphoneService.RemoveDBPassword();
                        ServiceManager.Instance.ContactService.DeleteLinphoneContact(contact);
                    }
                }
            }
        }

        private void btnFavorite_Click(object sender, RoutedEventArgs e)
        {
            //****************************************************************************************************
            // Favorite button clicked on Favorites tab in Contacts ("Add to Favorite")
            //****************************************************************************************************
            if (!ServiceManager.Instance.ContactService.IsEditing())
            {
                var contact = ((ToggleButton)sender).Tag as VATRPContact;
                if (contact != null)
                {
                    contact.IsFavorite =
                        !contact.IsFavorite;
                    ServiceManager.Instance.ContactService.UpdateFavoriteOption(contact);
                }
            }
        }
    }
}
