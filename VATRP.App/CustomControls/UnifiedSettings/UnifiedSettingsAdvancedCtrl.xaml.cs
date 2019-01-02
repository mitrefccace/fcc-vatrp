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

using com.vtcsecure.ace.windows.Enums;
using com.vtcsecure.ace.windows.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{
    /// <summary>
    /// Interaction logic for UnifiedSettingsAdvancedCtrl.xaml
    /// </summary>
    public partial class UnifiedSettingsAdvancedCtrl : BaseUnifiedSettingsPanel
    {
        public CallViewCtrl CallControl;
        private CollectionView _codecsView;

        public UnifiedSettingsAdvancedCtrl()
        {
            InitializeComponent();
        }

        #region Initialization
        public override void Initialize()
        {
            base.Initialize();
            if (App.CurrentAccount != null)
            {
                // Audio
                InitializeAudio();
                // Video
                InitializeVideo();

                //Network
                InitializeNetwork();

                // cjm-sep17 -- for Advanced Menu on login page
                if (App.CurrentAccount.DisableAudioCodecs)
                {
                    this.disableElements();
                }
                App.CurrentAccount.DisableAudioCodecs = false; // return to normal state
            }

            // Debug
            foreach (TextBlock textBlock in LoggingComboBox.Items)
            {
                if (textBlock.Text.Equals(App.CurrentAccount.Logging))
                {
                    LoggingComboBox.SelectedItem = textBlock;
                }
            }

            if (LoggingComboBox.SelectedItem == null)
                LoggingComboBox.SelectedIndex = 0;

            // Ping and Call Logging cjm-aug17
            EnableTechCallLogCheckBox.IsChecked = App.CurrentAccount.EnableTechCallLog; 
            EnableProviderPingLogCheckBox.IsChecked = App.CurrentAccount.EnableProviderPingLog;
        }

        private void InitializeAudio()
        {
            AudioCodecsListView.ItemsSource = App.CurrentAccount.AudioCodecsList;
            _codecsView = (CollectionView)CollectionViewSource.GetDefaultView(AudioCodecsListView.ItemsSource);
            if (_codecsView != null)
            {
                _codecsView.SortDescriptions.Add(new SortDescription("Priority", ListSortDirection.Ascending));
                _codecsView.Refresh();
            }
        }

        private void InitializeVideo()
        {
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
                Configuration.ConfEntry.RTCP_FEEDBACK, "Implicit");

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

        private void InitializeNetwork()
        {
            if (App.CurrentAccount.EnableICE && App.CurrentAccount.EnableSTUN)
                App.CurrentAccount.EnableICE = false; // normalization

            UseStunServerCheckbox.IsChecked = App.CurrentAccount.EnableSTUN;
            StunServerTextBox.Text = App.CurrentAccount.STUNAddress; // address:port 
            UseIceServerCheckbox.IsChecked = App.CurrentAccount.EnableICE;

            foreach (TextBlock textBlock in MediaEncryptionComboBox.Items)
            {
                if (textBlock.Text.Equals(App.CurrentAccount.MediaEncryption))
                {
                    if (MediaEncryptionComboBox.SelectedItem != null)
                    {
                        MediaEncryptionComboBox.SelectedItem = textBlock;
                    }      
                }
            }

            AdaptiveRateCheckbox.IsChecked = App.CurrentAccount.EnableAdaptiveRate;

            foreach (TextBlock textBlock in AlgorithmComboBox.Items)
            {
                if (textBlock.Text.Equals(App.CurrentAccount.AdaptiveRateAlgorithm))
                {
                    AlgorithmComboBox.SelectedItem = textBlock;
                }
            }

            if (AlgorithmComboBox.SelectedItem == null)
                AlgorithmComboBox.SelectedIndex = 0;

            UploadBandwidthTextBox.Text = App.CurrentAccount.UploadBandwidth.ToString();
            DownloadBandwidthTextBox.Text = App.CurrentAccount.DownloadBandwidth.ToString();
            QoSCheckbox.IsChecked = App.CurrentAccount.EnableQualityOfService;
            IPv6Checkbox.IsChecked = App.CurrentAccount.EnableIPv6;
        }
        #endregion

        #region Save
        public override void SaveData() // called when we leave this page
        {
           
            if (App.CurrentAccount == null)
                return;

            if (!IsChanged())
            {
                return;
            }

            foreach (var item in AudioCodecsListView.Items)
            {
                var cfgCodec = item as VATRPCodec;
                if (cfgCodec != null)
                {
                    foreach (var accountCodec in App.CurrentAccount.AudioCodecsList)
                    {
                        if (accountCodec.CodecName == cfgCodec.CodecName && accountCodec.Channels == cfgCodec.Channels &&
                            accountCodec.Rate == cfgCodec.Rate && accountCodec.Status != cfgCodec.Status)
                        {
                            accountCodec.Status = cfgCodec.Status;
                        }
                    }
                }
            }

            ServiceManager.Instance.ApplyCodecChanges();
            ServiceManager.Instance.SaveAccountSettings();
        }

        public bool IsChanged()
        {
            if (App.CurrentAccount == null)
                return false;

            bool changed = false;

            if (!changed)
                changed = IsAudioChanged();
            if (!changed)
                changed = IsVideoChanged();

            return changed;
        }
        private bool IsAudioChanged()
        {
            bool changed = false;

            // check audio codecs
            foreach (var item in AudioCodecsListView.Items)
            {
                var cfgCodec = item as VATRPCodec;
                if (cfgCodec != null)
                {
                    foreach (var accountCodec in App.CurrentAccount.AudioCodecsList)
                    {
                        if (accountCodec.CodecName == cfgCodec.CodecName && accountCodec.Channels == cfgCodec.Channels &&
                            accountCodec.Rate == cfgCodec.Rate && accountCodec.Status != cfgCodec.Status)
                        {
                            changed = true;
                        }
                    }
                }
            }

            return changed;
        }
        private bool IsVideoChanged()
        {
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
        #endregion

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


        #endregion

        public override void UpdateForMenuSettingChange(ACEMenuSettingsUpdateType menuSetting)
        {
            if (App.CurrentAccount == null)
                return;

            switch (menuSetting)
            {
                case ACEMenuSettingsUpdateType.ShowSelfViewMenu: ShowSelfViewCheckBox.IsChecked = App.CurrentAccount.ShowSelfView;
                    break;
                case ACEMenuSettingsUpdateType.NetworkSettingsChanged: 
                    break;
                default:
                    break;
            }
        }


        //==================== Audio Settings
        #region Audio Codecs

        private void AudioCodecsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void AudioCodecCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var lvItem = FindAncestorOrSelf<ListViewItem>(sender as CheckBox);
            var listView = ItemsControl.ItemsControlFromItemContainer(lvItem) as ListView;
            if (listView != null)
            {
                listView.SelectedItem = null;
                var index = listView.ItemContainerGenerator.IndexFromContainer(lvItem);
                listView.SelectedIndex = index;
                SaveData();
            }
        }
        #endregion

        //==================== Video Settings
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


        private void OnDebugMode(object sender, RoutedEventArgs e)
        {
            // Placeholder - not yet indicated for Windows
        }

        private void OnClearLogs(object sender, RoutedEventArgs e)
        {
            // Placeholder - not yet indicated for Windows
        }
        private void OnSendLogs(object sender, RoutedEventArgs e)
        {
            // Placeholder - not yet indicated for Windows
        }

        private void OnPersistentNotifier(object sender, RoutedEventArgs e)
        {
            // Placeholder - not yet indicated for Windows
        }
        private void OnEnableAnimations(object sender, RoutedEventArgs e)
        {
            // Placeholder - not yet indicated for Windows
        }

        public void OnSharingServerURLChanged(Object sender, RoutedEventArgs args)
        {
            // Placeholder - not yet indicated for Windows
        }

        public void OnRemoteProvisioningChanged(Object sender, RoutedEventArgs args)
        {
            // Placeholder - not yet indicated for Windows
        }

        public void OnSIPExpireChanged(Object sender, RoutedEventArgs args)
        {
            // Placeholder - not yet indicated for Windows
        }

        public void OnFileSharingServerURLChanged(Object sender, RoutedEventArgs args)
        {
            // Placeholder - not yet indicated for Windows
        }

        private void OnLoggingChanged(object sender, SelectionChangedEventArgs e)
        {
            TextBlock valueA = (TextBlock)LoggingComboBox.SelectedItem;
            string value = valueA.Text;
            if (App.CurrentAccount != null)
            {
                App.CurrentAccount.Logging = value;
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.AdvancedSettingsChanged);
            }
        }

        private void OnEnableTechCallLog(object sender, RoutedEventArgs e)
        {
            // cjm-aug17
            //************************************************************************************************************************************
            // Call Logging setting changed from More==>Settings==>General
            //************************************************************************************************************************************
            if (App.CurrentAccount == null) { return; };
            Console.WriteLine("Enable In Call Logging Clicked");
            bool enabled = EnableTechCallLogCheckBox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.EnableTechCallLog)
            {
                App.CurrentAccount.EnableTechCallLog = enabled;
                ServiceManager.Instance.SaveAccountSettings();
            }
            return;
        }

        private void OnEnableProviderPingLog(object sender, RoutedEventArgs e)
        {
            // cjm-aug17
            //************************************************************************************************************************************
            // Provider Network Ping Logging setting changed from More==>Settings==>General
            //************************************************************************************************************************************
            if (App.CurrentAccount == null) { return; };
            Console.WriteLine("Enable Provider Ping Logging Clicked");
            bool enabled = EnableProviderPingLogCheckBox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.EnableProviderPingLog)
            {
                App.CurrentAccount.EnableProviderPingLog = enabled;
                ServiceManager.Instance.SaveAccountSettings();
            }
            return;
        }

        private void disableElements()
        {
            AudioCodecsListView.IsEnabled = false;
            VideoCodecsListView.IsEnabled = false;
            UseIceServerCheckbox.IsEnabled = false;
            UseStunServerCheckbox.IsEnabled = false;
            IPv6Checkbox.IsEnabled = false;
            MediaEncryptionComboBox.IsEnabled = false;
            UploadBandwidthTextBox.IsEnabled = false;
            DownloadBandwidthTextBox.IsEnabled = false;
            QoSCheckbox.IsEnabled = false;
            SipDscpTextBox.IsEnabled = false;
            AudioDscpTextBox.IsEnabled = false;
            VideoDscpTextBox.IsEnabled = false;
            StunServerTextBox.IsEnabled = false;
        }

        #region
        private void OnAdaptiveRateChecked(object sender, RoutedEventArgs e)
        {
            bool enabled = AdaptiveRateCheckbox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.EnableAdaptiveRate)
            {
                App.CurrentAccount.EnableAdaptiveRate = enabled;
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
            }
        }

        private void OnAdaptiveRateAlgorithmChanged(object sender, SelectionChangedEventArgs e)
        {
            TextBlock valueA = (TextBlock)AlgorithmComboBox.SelectedItem;
            string value = valueA.Text;
            if (App.CurrentAccount != null)
            {
                bool needsUpdate = false;
                if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(App.CurrentAccount.AdaptiveRateAlgorithm))
                    needsUpdate = true;
                else if (!string.IsNullOrEmpty(value) && !App.CurrentAccount.AdaptiveRateAlgorithm.Equals(value))
                    needsUpdate = true;
                // do not update if we do nto need it.
                if (needsUpdate)
                {
                    App.CurrentAccount.AdaptiveRateAlgorithm = value;
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
                }
            }
        }

        private void OnStunServerChecked(object sender, RoutedEventArgs e)
        {
            bool enabled = UseStunServerCheckbox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.EnableSTUN)
            {
                App.CurrentAccount.EnableSTUN = enabled;
                if (enabled)
                {
                    App.CurrentAccount.EnableICE = false;
                    UseIceServerCheckbox.IsChecked = App.CurrentAccount.EnableICE;
                }

                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
            }
        }

         /// <summary>
         /// Updates network information for to STUN server.
         /// </summary>
         /// <remarks>
         /// This function is called everytime the Stun Server Textbox toggles focus. 
         /// Accepts the server URI in the format address:port 
         /// rather than just the address previously.Error checks for various user conditions 
         /// such as too many colons or inputting an empty text box.
         /// </remarks>
         /// <param name="sender">The object</param>
         /// <param name="args">Event arguments</param>
         /// <returns>void</returns>
        public void OnStunServerChanged(Object sender, RoutedEventArgs args)
        {
            // VATRP-1949: removed check for empty stun server. However - maybe we want a test here so that if the user has
            //  Stun Server checkbox enabled we prompt the user if the value does not look like a valid address?
            string newStunServer = StunServerTextBox.Text;

            if (App.CurrentAccount != null)
            {
                ushort parsedPort;
                ushort port = App.CurrentAccount.DefaultSTUNPort;
                string[] stunServerComponents = newStunServer.Split(':');

                if (stunServerComponents.Length == 2 && !string.IsNullOrEmpty(stunServerComponents[1])) // user has enterd <host:port>
                {
                    if (ushort.TryParse(stunServerComponents[1], out parsedPort))
                    {
                        port = parsedPort;
                    }
                }

                // TODO - The stun server should use the sanity check 
                // below for TLD verification once we have the known
                // subset of expected inputs from the tester.
                newStunServer = string.IsNullOrEmpty(stunServerComponents[0]) ? string.Empty : $"{stunServerComponents[0]}:{port}";
                StunServerTextBox.Text = newStunServer;

                if (!newStunServer.Equals(App.CurrentAccount.STUNAddress))
                {
                    App.CurrentAccount.STUNAddress = newStunServer;
                    App.CurrentAccount.STUNPort = port;
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
                }
            }
        }

        /// <summary>
        /// Validates a proper URI.
        /// </summary>
        /// <remarks>
        /// Do some simple sanity checking to determine if the URI specified is well-formed. 
        /// If it is not then throw error. Well-formed means that the string is in valid URL format.
        /// </remarks>
        /// <param name="address">string that we are sanity checking</param>
        /// <returns>True if the URL is well formed. Well-formed URLs contain in them a TLD, such as 
        /// ".com", ".net", ".gov", ".co.uk", or ".org".
        /// </returns>

        // !TODO: update TLDs' that are checked, then uncomment it's reference in OnStunServerChanged() method
        private bool AddressURLSanityCheck(string address)
        {
            bool isValid = false;
            List<string> tlds = new List<string>(new string[] { ".com", ".net", ".gov", ".co.uk", ".org" });
            foreach (var tld in tlds)
            {
                if (address.Contains(tld))
                {
                    string trailingChars = GetAfter(address, tld);
                    if (string.IsNullOrEmpty(trailingChars))
                    {
                        isValid = true;
                    }
                    break;
                }
            }
            return isValid;
        }

        /// <summary>
        /// Checks for trailing characters after .com in a URL
        /// </summary>
        /// <param name="input">string for URL address</param>
        /// <param name="startString">string for URL ending</param>
        /// <returns>string of trailing characters</returns>
        private string GetAfter(string input, string startString)
        {
            string endString = string.Empty;
            if (input.Contains(startString))
            {
                int startPos = input.IndexOf(startString, 0) + startString.Length;
                int endPos = input.Length;
                endString = input.Substring(startPos, endPos - startPos);
            }

            return endString;
        }

        private void OnIceServerChecked(object sender, RoutedEventArgs e)
        {
            bool enabled = UseIceServerCheckbox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.EnableICE)
            {
                App.CurrentAccount.EnableICE = enabled;
                if (enabled)
                {
                    App.CurrentAccount.EnableSTUN = false;
                    UseStunServerCheckbox.IsChecked = App.CurrentAccount.EnableSTUN;
                }
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
            }
        }

        public void OnUploadBandwidthChanged(Object sender, RoutedEventArgs args)
        {
            string newBandwidth = UploadBandwidthTextBox.Text;
            if (!string.IsNullOrEmpty(newBandwidth))
            {
                int bandwidth = 0;
                if (int.TryParse(newBandwidth, out bandwidth))
                {
                    App.CurrentAccount.UploadBandwidth = bandwidth;
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
                }
            }
        }
        public void OnDownloadBandwidthChanged(Object sender, RoutedEventArgs args)
        {
            string newBandwidth = DownloadBandwidthTextBox.Text;
            if (!string.IsNullOrEmpty(newBandwidth))
            {
                int bandwidth = 0;
                if (int.TryParse(newBandwidth, out bandwidth))
                {
                    App.CurrentAccount.DownloadBandwidth = bandwidth;
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
                }
            }
        }

        private void OnQoSChecked(object sender, RoutedEventArgs e)
        {
            bool enabled = QoSCheckbox.IsChecked ?? false;

            SipDscpTextBox.IsEnabled = enabled;
            AudioDscpTextBox.IsEnabled = enabled;
            VideoDscpTextBox.IsEnabled = enabled;

            if (enabled)
            {
                SipDscpTextBox.Text = App.CurrentAccount.SipDscpValue.ToString();
                AudioDscpTextBox.Text = App.CurrentAccount.AudioDscpValue.ToString();
                VideoDscpTextBox.Text = App.CurrentAccount.VideoDscpValue.ToString();
            }
            else
            {
                SipDscpTextBox.Text = "0";
                AudioDscpTextBox.Text = "0";
                VideoDscpTextBox.Text = "0";
            }

            if (enabled != App.CurrentAccount.EnableQualityOfService)
            {
                App.CurrentAccount.EnableQualityOfService = enabled;
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
            }
        }

        private void OnSIPDscpChanged(object sender, RoutedEventArgs e)
        {
            if (!SipDscpTextBox.IsEnabled)
                return;

            string newDscp = SipDscpTextBox.Text;
            if (!string.IsNullOrEmpty(newDscp))
            {
                int val = 0;
                if (int.TryParse(newDscp, out val))
                {
                    App.CurrentAccount.SipDscpValue = val;
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
                }
            }
        }

        private void OnAudioDscpChanged(object sender, RoutedEventArgs e)
        {
            if (!AudioDscpTextBox.IsEnabled)
                return;

            string newDscp = AudioDscpTextBox.Text;
            if (!string.IsNullOrEmpty(newDscp))
            {
                int val = 0;
                if (int.TryParse(newDscp, out val))
                {
                    App.CurrentAccount.AudioDscpValue = val;
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
                }
            }
        }

        private void OnVideoDscpChanged(object sender, RoutedEventArgs e)
        {
            if (!VideoDscpTextBox.IsEnabled)
                return;

            string newDscp = VideoDscpTextBox.Text;
            if (!string.IsNullOrEmpty(newDscp))
            {
                int val = 0;
                if (int.TryParse(newDscp, out val))
                {
                    App.CurrentAccount.VideoDscpValue = val;
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
                }
            }
        }

        private void OnIPv6(object sender, RoutedEventArgs e)
        {
            bool enabled = IPv6Checkbox.IsChecked ?? false;

            if (enabled != App.CurrentAccount.EnableIPv6)
            {
                App.CurrentAccount.EnableIPv6 = enabled;
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
            }
        }

        #endregion

        #region NotYetSpecifiedForWindows
        private void OnEdgeOptimization(object sender, RoutedEventArgs e)
        {
            bool enabled = EdgeOptimizationCheckbox.IsChecked ?? false;
            // Placeholder - not yet indicated for Windows

        }

        private void OnWifiOnly(object sender, RoutedEventArgs e)
        {
            bool enabled = WifiOnlyCheckbox.IsChecked ?? false;
            // Placeholder - not yet indicated for Windows

        }

        private void OnRandomPort(object sender, RoutedEventArgs e)
        {
            bool enabled = RandomPortCheckbox.IsChecked ?? false;
            // Placeholder - not yet indicated for Windows

        }

        public void OnAudioPortsChanged(Object sender, RoutedEventArgs args)
        {
            string newAudioPorts = AudioPortsTextBox.Text;
            // Placeholder - not yet indicated for Windows
            //            if (string.IsNullOrEmpty(newAudioPorts))
            //            {
            //                string oldAudioPorts = App.CurrentAccount.Username;
            //                AudioPortsTextBox.Text = oldAudioPorts;
            //            }
        }

        public void OnVideoPortsChanged(Object sender, RoutedEventArgs args)
        {
            string newVideoPorts = VideoPortsTextBox.Text;
            // Placeholder - not yet indicated for Windows

            //            if (string.IsNullOrEmpty(newVideoPorts))
            //            {
            //                string oldVideoPorts = App.CurrentAccount.Username;
            //                VideoPortsTextBox.Text = oldVideoPorts;
            //            }
        }

        private void OnMediaEncryptionChanged(object sender, RoutedEventArgs e)
        {
            TextBlock valueTB = (TextBlock)MediaEncryptionComboBox.SelectedItem;
            string value = valueTB.Text;
            if (App.CurrentAccount != null)
            {
                App.CurrentAccount.MediaEncryption = value;
                // update media settings.
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();
            }

        }
        private void OnPushNotifications(object sender, RoutedEventArgs e)
        {
            bool enabled = PushNotificationsCheckbox.IsChecked ?? false;
            // Placeholder - not yet indicated for Windows

        }


        #endregion
    }
}
