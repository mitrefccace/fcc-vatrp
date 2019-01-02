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

using com.vtcsecure.ace.windows.Utilities;
using com.vtcsecure.ace.windows.Views;
using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{
    /// <summary>
    /// Interaction logic for UnifiedSettingsSummaryCtrl.xaml
    /// </summary>
    public partial class UnifiedSettingsSummaryCtrl : BaseUnifiedSettingsPanel
    {
        public event UnifiedSettings_EnableSettings ShowSettingsUpdate;
        private TechnicalSupportSheetCtrl _viewTechnicalSupportPanel;

        public UnifiedSettingsSummaryCtrl()
        {
            InitializeComponent();
            Title = "Summary";
            Initialize();
            _viewTechnicalSupportPanel = new TechnicalSupportSheetCtrl();
        }

//        public override void ShowAdvancedOptions(bool show)
//        {
//            base.ShowAdvancedOptions(show);
//            System.Windows.Visibility visibleSetting = System.Windows.Visibility.Collapsed;
//            if (show)
//            {
//                visibleSetting = System.Windows.Visibility.Visible;
//            }
//            DebugSettingsPasswordBox.Visibility = visibleSetting;
//            ShowDebugSettingsButton.Visibility = visibleSetting;

//            SuperSettingsPasswordBox.Visibility = visibleSetting;
//            ShowAllSettingsButton.Visibility = visibleSetting;
//        }


        private void OnViewTss(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("View TSS Clicked");
//            OnContentChanging(UnifiedSettingsContentType.ViewTSS);
            ViewTSSCtrl.Initialize();
            this.ViewTSSLabel.Visibility = System.Windows.Visibility.Visible;
            this.ViewTSSScrollView.Visibility = System.Windows.Visibility.Visible;
        }
        private void OnMailTss(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Mail TSS Clicked");
            string path = TechnicalSupportInfoBuilder.CreateAndGetTechnicalSupportInfoAsTextFile(true);
            var feedbackView = new FeedbackView(path);
            feedbackView.Show();

        }
        private void OnShowAdvanced(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Show Advanced Clicked");
            string password = AdvancedSettingsPasswordBox.Password;
            if (!string.IsNullOrEmpty(password) && password.Equals("1234"))
            {
                // show advanced settings
                if (ShowSettingsUpdate != null)
                {
                    ShowSettingsUpdate(UnifiedSettings_LevelToShow.Advanced, true);
                }
            }
        }
        private void OnShowDebug(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Show Debug Clicked");
            string password = DebugSettingsPasswordBox.Password;
            if (!string.IsNullOrEmpty(password) && password.Equals("1234"))
            {
                if (ShowSettingsUpdate != null)
                {
                    ShowSettingsUpdate(UnifiedSettings_LevelToShow.Debug, true);
                }
            }
        }

        private void OnShowAllSettings(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Show Super Clicked");
            string password = this.SuperSettingsPasswordBox.Password;
            if (!string.IsNullOrEmpty(password) && password.Equals("1234"))
            {
                if (ShowSettingsUpdate != null)
                {
                    ShowSettingsUpdate(UnifiedSettings_LevelToShow.Super, true);
                }
            }
            else if (!string.IsNullOrEmpty(password) && password.Equals("1170"))
            {
                BaseUnifiedSettingsPanel.VisibilityForSuperSettingsAsPreview = System.Windows.Visibility.Visible;
                if (ShowSettingsUpdate != null)
                {
                    ShowSettingsUpdate(UnifiedSettings_LevelToShow.Super, true);
                }
            }
        }
    }
}
