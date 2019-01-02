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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using com.vtcsecure.ace.windows.Services;
using VATRP.LinphoneWrapper;

namespace com.vtcsecure.ace.windows.Views
{
    /// <summary>
    /// Interaction logic for LegalReleaseWindow.xaml
    /// </summary>
    public partial class LegalReleaseWindow : Window
    {
        public LegalReleaseWindow()
        {
            InitializeComponent();
        }

        private void LoadCompleted(object sender, RoutedEventArgs e)
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            if (!string.IsNullOrEmpty(path))
            {
                var legalReleasePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), "legal_release.txt");

                try
                {
                    if (File.Exists(legalReleasePath))
                    {
                        var textRange = new TextRange(RtfContainer.Document.ContentStart,
                            RtfContainer.Document.ContentEnd);
                        using (var fileStream = new FileStream(legalReleasePath, FileMode.Open, FileAccess.Read))
                        {
                            textRange.Load(fileStream, DataFormats.Text);
                        }
                    }
                    else
                        throw new FileNotFoundException("legal_release.txt");
                }
                catch (Exception ex)
                {
                    ServiceManager.LogError("Load Legal release", ex);
                }
            }
        }


        private void OnAcceptTerms(object sender, RoutedEventArgs e)
        {
            this.BtnAccept.IsEnabled = true;
        }

        private void AcceptAgreement(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void DeclineAgreement(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
         
//            var test = VisualTreeHelper.GetChild(RtfContainer, 0);
//            var scroll = (ScrollViewer)((Border)VisualTreeHelper.GetChild(RtfContainer, 0)).Child;
//            if (scroll != null)
//            {
//                scroll.ScrollChanged += OnScrollChanged;
//            }
//            else
//            {
//                BtnAccept.IsEnabled = true;
//            }
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
//            var scroll = (sender as ScrollViewer);
//            if (scroll != null) 
//                BtnAccept.IsEnabled = (scroll.VerticalOffset == scroll.ScrollableHeight);
        }

        private void RtfContainer_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
