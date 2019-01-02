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
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using com.vtcsecure.ace.windows.ViewModel;

namespace com.vtcsecure.ace.windows.CustomControls
{
    /// <summary>
    /// Interaction logic for MoreMenuCtrl.xaml
    /// </summary>
    public partial class MoreMenuCtrl : UserControl
    {
        private MenuViewModel _viewModel;

        public event EventHandler ResourceClicked;
        public event EventHandler SettingsClicked;
        public event EventHandler SelfViewClicked;
        public event EventHandler VideoMailClicked;

        public MoreMenuCtrl()
        {
            InitializeComponent();
        }
        public void SetDataContext(MenuViewModel viewModel)
        {

            //****************************************************************************************
            // Setting the More Menu view.
            //*****************************************************************************************
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void OnResourcesClicked(object sender, RoutedEventArgs e)
        {

            //************************************************************************************************************************************
            // On Resources item clicked from More==>Settings==>Resources
            //************************************************************************************************************************************
            if (ResourceClicked != null) 
                ResourceClicked(this, EventArgs.Empty);
        }

        private void OnSettingsClicked(object sender, RoutedEventArgs e)
        {
            //*************************************************************************************************************************************
            // Settings menu item clicked from "More" menu item.
            //*************************************************************************************************************************************
            if (SettingsClicked != null)
                SettingsClicked(this, EventArgs.Empty);
        }

        private void OnVmClicked(object sender, RoutedEventArgs e)
        {
            //************************************************************************************************************************************
            // On VideoMail clicked from  More==>Settings==>VideoMail
            //************************************************************************************************************************************
            if (VideoMailClicked != null)
                VideoMailClicked(this, EventArgs.Empty);
        }

        private void OnSelfViewClicked(object sender, RoutedEventArgs e)
        {

            //************************************************************************************************************************************
            // On Self-Preview clicked from  More==>Settings==>Self-Preview
            //************************************************************************************************************************************
            if (SelfViewClicked != null)
                SelfViewClicked(this, EventArgs.Empty);
        }
    }
}
