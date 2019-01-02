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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VATRP.Core.Model;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.Enums;
using System.ComponentModel;

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{
    /// <summary>
    /// Interaction logic for UnifiedSettingsVideoCtrl.xaml
    /// </summary>
    public partial class UnifiedSettingsVideoCtrl : BaseUnifiedSettingsPanel
    {
        private CollectionView _codecsView;
        public UnifiedSettingsVideoCtrl()
        {
            InitializeComponent();
            this.Loaded += UnifiedSettingsVideoCtrl_Loaded;
        }


        void UnifiedSettingsVideoCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        public override void Initialize()
        {
            base.Initialize();

            if (App.CurrentAccount == null)
                return;
            AutomaticallyStartCheckBox.IsChecked = App.CurrentAccount.VideoAutomaticallyStart;
            AutomaticallyAcceptCheckBox.IsChecked = App.CurrentAccount.VideoAutomaticallyAccept;

            ShowSelfViewCheckBox.IsChecked = App.CurrentAccount.ShowSelfView;

            string accountPreset = App.CurrentAccount.VideoPreset;
            if (string.IsNullOrWhiteSpace(accountPreset))
            {
                accountPreset = "default";
            }
            foreach (var item in VideoPresetComboBox.Items)
            {
                var tb = item as TextBlock;
                string itemString = tb.Text;
                if (itemString.Equals(accountPreset))
                {
                    VideoPresetComboBox.SelectedItem = item;
                    break;
                }
            }

            float preferredFPS = App.CurrentAccount.PreferredFPS;
            PreferredFPSTextBox.Text = Convert.ToString(preferredFPS);

            VideoCodecsListView.ItemsSource = App.CurrentAccount.VideoCodecsList;
            _codecsView = (CollectionView)CollectionViewSource.GetDefaultView(VideoCodecsListView.ItemsSource);
            if (_codecsView != null)
            {
                _codecsView.SortDescriptions.Add(new SortDescription("Priority", ListSortDirection.Ascending));
                _codecsView.Refresh();
            }
            /*
            New option name: RTCP feedback
Option Location: Advanced > Video
Options: Implicit, Explicit, Off
Default: "Implicit"

AVPF shall be off by default and  "rtcp_fb_implicit_rtcp_fb" on =1 call that combination "RTCP feedback (AVPF)"   (Implicit)
and then a setting with both AVPF off and  "rtcp_fb_implicit_rtcp_fb" off , and call that  setting  "RTCP feedback (AVPF)"   (Off)
And a setting with both AVPF on and  "rtcp_fb_implicit_rtcp_fb" on , and call that  setting  "RTCP feedback (AVPF)"   (Explicit).
             * The function can be activated/deactivated via an integer parameter called "rtcp_fb_implicit_rtcp_fb" inside "rtp" section. Values are 0 or 1.
This parameter can be set dynamically also via the traditional wrappers to get/set parameters (lp_config_set_int for C API).
Default value = 1
             * */
            string rtcpFeedbackString = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.RTCP_FEEDBACK, "Off");

            foreach (var item in RtcpFeedbackComboBox.Items)
            {
                var tb = item as TextBlock;
                if (tb.Text.Equals(rtcpFeedbackString))
                {
                    RtcpFeedbackComboBox.SelectedItem = item;
                    break;
                }
            }

        }

        public override void UpdateForMenuSettingChange(ACEMenuSettingsUpdateType menuSetting)
        {
            if (App.CurrentAccount == null)
                return;

            switch (menuSetting)
            {
                case ACEMenuSettingsUpdateType.ShowSelfViewMenu: ShowSelfViewCheckBox.IsChecked = App.CurrentAccount.ShowSelfView;
                    break;
                default:
                    break;
            }
        }

        public bool IsChanged()
        {
            if (App.CurrentAccount == null)
                return false;

            if (IsVideoPresetChanged())
            {
                return true;
            }

            // video codecs
            foreach (var item in VideoCodecsListView.Items)
            {
                var cfgCodec = item as VATRPCodec;
                if (cfgCodec != null)
                {
                    foreach (var accountCodec in App.CurrentAccount.VideoCodecsList)
                    {
                        if (accountCodec.CodecName == cfgCodec.CodecName && accountCodec.Channels == cfgCodec.Channels &&
                            accountCodec.Rate == cfgCodec.Rate)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool IsVideoPresetChanged()
        {
            if (App.CurrentAccount == null)
                return false;

            var videoPresetText = VideoPresetComboBox.SelectedItem as TextBlock;
            string videoPresetString = GetVideoPresetId(videoPresetText);
            if ((string.IsNullOrWhiteSpace(videoPresetString) && !string.IsNullOrWhiteSpace(App.CurrentAccount.VideoPreset)) ||
                (!string.IsNullOrWhiteSpace(videoPresetString) && string.IsNullOrWhiteSpace(App.CurrentAccount.VideoPreset)))
                return true;
            if ((!string.IsNullOrWhiteSpace(videoPresetString) && !string.IsNullOrWhiteSpace(App.CurrentAccount.VideoPreset)) &&
                (!videoPresetString.Equals(App.CurrentAccount.VideoPreset)))
                return true;

            return false;
        }


        private bool IsRtcpFeedbackChanged()
        {
            var rtcpFeedbackTextBlock = RtcpFeedbackComboBox.SelectedItem as TextBlock;
            string rtcpFeedbackString = rtcpFeedbackTextBlock.Text;
            string oldRtcpFeedbackString = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.RTCP_FEEDBACK, "Off");

            if ((string.IsNullOrWhiteSpace(rtcpFeedbackString) && !string.IsNullOrWhiteSpace(oldRtcpFeedbackString)) ||
                (!string.IsNullOrWhiteSpace(rtcpFeedbackString) && string.IsNullOrWhiteSpace(oldRtcpFeedbackString)))
                return true;
            if ((!string.IsNullOrWhiteSpace(rtcpFeedbackString) && !string.IsNullOrWhiteSpace(oldRtcpFeedbackString)) &&
                (!rtcpFeedbackString.Equals(oldRtcpFeedbackString)))
                return true;

            return false;
        }


        #region HelperMethods
        private string GetVideoPresetId(TextBlock tb)
        {
            if (tb == null)
                return string.Empty;

            string value = tb.Text.Trim();
            if (value.Equals("default"))
            {
                return null;
            }
            return value;
        }

        #endregion

        #region General Video Settings
        private void OnAutomaticallyStart(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Automatically Start Video Clicked");
            if (App.CurrentAccount == null)
                return;
            bool enable = AutomaticallyStartCheckBox.IsChecked ?? false;
            if (enable != App.CurrentAccount.VideoAutomaticallyStart)
            {
                App.CurrentAccount.VideoAutomaticallyStart = enable;
                ServiceManager.Instance.SaveAccountSettings();

                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.VideoPolicyChanged);
            }
        }

        private void OnAutomaticallyAccept(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Automatically Accept Video Clicked");
            if (App.CurrentAccount == null)
                return;
            bool enable = AutomaticallyAcceptCheckBox.IsChecked ?? false;
            if (enable != App.CurrentAccount.VideoAutomaticallyAccept)
            {
                App.CurrentAccount.VideoAutomaticallyAccept = enable;
                ServiceManager.Instance.SaveAccountSettings();

                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.VideoPolicyChanged);
            }
        }
        private void OnShowSelfView(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Show Self View Clicked");
            if (App.CurrentAccount == null)
                return;
            bool enable = this.ShowSelfViewCheckBox.IsChecked ?? true;
            if (enable != App.CurrentAccount.ShowSelfView)
            {
                App.CurrentAccount.ShowSelfView = enable;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();

                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.ShowSelfViewChanged);
            }
        }

        public void OnPreferredFPS(Object sender, RoutedEventArgs args)
        {
            if ((App.CurrentAccount == null) || !this.IsVisible)
                return;
            // then get and set the preferred fps.
            string stringValue = PreferredFPSTextBox.Text;
            if (!string.IsNullOrEmpty(stringValue))
            {
                float floatValue = float.Parse(stringValue);
                App.CurrentAccount.PreferredFPS = floatValue;
                // set the preferred fps in linphone
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();
            }
        }

        private void OnVideoPreset(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Video Preset Clicked");
            if (App.CurrentAccount == null)
                return;
            if (!IsVideoPresetChanged())
                return;

            var tb = VideoPresetComboBox.SelectedItem as TextBlock;
            if (tb != null)
            {
                var str = tb.Text;
                if (string.IsNullOrWhiteSpace(str))
                    return;
                if (str.Equals("default"))
                {
                    str = null;
                }
                
                App.CurrentAccount.VideoPreset = str;
            }
            ServiceManager.Instance.ApplyMediaSettingsChanges();
            ServiceManager.Instance.SaveAccountSettings();
        }

        #endregion

        #region videoCodecs
        public static T FindAncestorOrSelf<T>(DependencyObject obj)
        where T : DependencyObject
        {
            while (obj != null)
            {
                T objTest = obj as T;

                if (objTest != null)
                    return objTest;

                obj = VisualTreeHelper.GetParent(obj);
            }
            return null;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var lvItem = FindAncestorOrSelf<ListViewItem>(sender as CheckBox);
            var listView = ItemsControl.ItemsControlFromItemContainer(lvItem) as ListView;
            if (listView != null)
            {
                listView.SelectedItem = null;
                var index = listView.ItemContainerGenerator.IndexFromContainer(lvItem);
                listView.SelectedIndex = index;
                SaveVideoCodecsSettings();
            }
        }

        private void SaveVideoCodecsSettings()
        {
            if (App.CurrentAccount == null)
                return;

            foreach (var item in VideoCodecsListView.Items)
            {
                var cfgCodec = item as VATRPCodec;
                if (cfgCodec != null)
                {
                    foreach (var accountCodec in App.CurrentAccount.VideoCodecsList)
                    {
                        if (accountCodec.CodecName == cfgCodec.CodecName && accountCodec.Channels == cfgCodec.Channels &&
                            accountCodec.Rate == cfgCodec.Rate)
                        {
                            accountCodec.Status = cfgCodec.Status;
                        }
                    }
                }
            }
            ServiceManager.Instance.ApplyCodecChanges();
            ServiceManager.Instance.SaveAccountSettings();

        }

        private void VideoCodecsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        #endregion

        #region RtcpFeedback
        private void OnRtcpFeedback(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("RTCP Feedback Selected");
            if (App.CurrentAccount == null)
                return;
            if (!IsRtcpFeedbackChanged())
                return;

            var tb = RtcpFeedbackComboBox.SelectedItem as TextBlock;
            if (tb != null)
            {
                var str = tb.Text;
                if (string.IsNullOrWhiteSpace(str))
                    return;

                ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                     Configuration.ConfEntry.RTCP_FEEDBACK, str);
                if (str.Equals("Explicit"))
                {
                    ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                         Configuration.ConfEntry.AVPF_ON, true);
                }
                else
                {
                    ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                         Configuration.ConfEntry.AVPF_ON, false);
                }
                ServiceManager.Instance.ConfigurationService.SaveConfig();
                // RTCP Feedback and AVPF are related
                ServiceManager.Instance.ApplyAVPFChanges();
            }
        }
        #endregion
    }
}
