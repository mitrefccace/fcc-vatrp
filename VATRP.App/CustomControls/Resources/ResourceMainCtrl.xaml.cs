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

using com.vtcsecure.ace.windows.Views;
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
using com.vtcsecure.ace.windows.Services;

namespace com.vtcsecure.ace.windows.CustomControls.Resources
{
    /// <summary>
    /// Interaction logic for ResourceMainCtrl.xaml
    /// </summary>
    public partial class ResourceMainCtrl : BaseResourcePanel
    {
        public ResourceMainCtrl()
        {
            InitializeComponent();
            this.Loaded += ControlLoaded;
            Title = "Resources";
        }

        void ControlLoaded(object sender, RoutedEventArgs e)
        {
            // populate the version labels:
            //System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            //var version = assembly.GetName().Version;

            //AceVersionLabel.Content = string.Format("Version {0}.{1}.{2}", version.Major, version.Minor, version.Build);

            //string linphoneLibraryVersion = VATRP.LinphoneWrapper.LinphoneAPI.linphone_core_get_version_asString();
            //LinphoneVersionLabel.Content = string.Format("Core Version {0}", linphoneLibraryVersion);
            AceLabel.Visibility = System.Windows.Visibility.Collapsed;
            AceVersionLabel.Visibility = System.Windows.Visibility.Collapsed;
            LinphoneLabel.Visibility = System.Windows.Visibility.Collapsed;
            LinphoneVersionLabel.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void OnTechnicalSupport(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Technical Support Clicked");
            var feedbackView = new FeedbackView();
            feedbackView.Show();
        }

        private void OnInstantFeedback(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Instant Feedback Clicked");
            var feedbackView = new FeedbackView();
            feedbackView.Show();
        }
        private void OnDeafHoh(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Deaf/Hard of Hearing Resources Clicked");
            OnContentChanging(ResourcesType.DeafHoHResourcesContent);
        }

    }
}
