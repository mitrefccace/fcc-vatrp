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
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using com.vtcsecure.ace.windows.ViewModel;
using log4net;

namespace com.vtcsecure.ace.windows.Views
{
    /// <summary>
    /// Interaction logic for AboutView.xaml
    /// </summary>
    public partial class AboutView : Window
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(AboutViewModel));

        public AboutView()
        {
            InitializeComponent();
            LoadLicense();
            DataContext = new AboutViewModel();
        }

        /// <summary>
        /// Loads the license text file and displays to view.
        /// </summary>
        /// <remarks>
        /// Dependent on a post build script placing the license.txt
        /// file into the build directory so that it can be read into
        /// the view. This will need to be in the installer as well.
        /// </remarks>
        /// <returns>void</returns>
        private void LoadLicense()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            if (!string.IsNullOrEmpty(path))
            {
                var legalReleasePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), "license.txt");
                var textRange = new TextRange(RtfContainer.Document.ContentStart,
                                              RtfContainer.Document.ContentEnd);
                try
                {
                    if (File.Exists(legalReleasePath))
                    {
                        using (var fileStream = new FileStream(legalReleasePath, FileMode.Open, FileAccess.Read))
                        {
                            textRange.Load(fileStream, DataFormats.Text);
                        }
                    }
                    else
                    {
                        throw new FileNotFoundException("license.txt");
                    }
                }
                catch (Exception ex)
                {
                    LOG.Error("Problem reading file", ex);
                    textRange.Text = "VATRP License Not Found!!";
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
