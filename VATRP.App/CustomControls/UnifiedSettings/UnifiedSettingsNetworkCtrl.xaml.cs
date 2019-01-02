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

using com.vtcsecure.ace.windows.Services;
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

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{
    /// <summary>
    /// Interaction logic for UnifiedSettingsNetworkCtrl.xaml
    /// </summary>
    public partial class UnifiedSettingsNetworkCtrl : BaseUnifiedSettingsPanel
    {
        public UnifiedSettingsNetworkCtrl()
        {
            InitializeComponent();
        }

        public override void Initialize()
        {
            base.Initialize();
            if (App.CurrentAccount == null)
                return;
            if (App.CurrentAccount.EnableICE && App.CurrentAccount.EnableSTUN)
                App.CurrentAccount.EnableICE = false; // normalization

            UseStunServerCheckbox.IsChecked = App.CurrentAccount.EnableSTUN;
            StunServerTextBox.Text = App.CurrentAccount.STUNAddress;
            UseIceServerCheckbox.IsChecked = App.CurrentAccount.EnableICE;

            foreach (TextBlock textBlock in MediaEncryptionComboBox.Items)
            {
                if (textBlock.Text.Equals(App.CurrentAccount.MediaEncryption))
                {
                    MediaEncryptionComboBox.SelectedItem = textBlock;
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
		
        public void OnStunServerChanged(Object sender, RoutedEventArgs args)
        {
            string newStunServer = StunServerTextBox.Text;
            // VATRP-1949: removed check for empty stun server. However - maybe we want a test here so that if the user has
            //  Stun Server checkbox enabled we prompt the user if the value does not look like a valid address?
            if (App.CurrentAccount != null)
            {
                bool updateStunServer = false;
                if (!string.IsNullOrEmpty(newStunServer) && string.IsNullOrEmpty(App.CurrentAccount.STUNAddress))
                    updateStunServer = true;
                else if (!string.IsNullOrEmpty(newStunServer) && !newStunServer.Equals(App.CurrentAccount.STUNAddress))
                    updateStunServer = true;
                if (updateStunServer)
                {
                    App.CurrentAccount.STUNAddress = newStunServer;
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
                }
            }
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

        #region NotYetSpecifiedForWindows
            private
            void OnEdgeOptimization 
            (object sender, RoutedEventArgs e)
            {
                bool enabled = EdgeOptimizationCheckbox.IsChecked ?? false;
                // Placeholder - not yet indicated for Windows

            }

        private
            void OnWifiOnly 
            (object sender, RoutedEventArgs e)
            {
                bool enabled = WifiOnlyCheckbox.IsChecked ?? false;
                // Placeholder - not yet indicated for Windows

            }

        private
            void OnRandomPort 
            (object sender, RoutedEventArgs e)
            {
                bool enabled = RandomPortCheckbox.IsChecked ?? false;
                // Placeholder - not yet indicated for Windows

            }

        public
            void OnAudioPortsChanged 
            (Object sender, RoutedEventArgs args)
            {
                string newAudioPorts = AudioPortsTextBox.Text;
                // Placeholder - not yet indicated for Windows
                //            if (string.IsNullOrEmpty(newAudioPorts))
                //            {
                //                string oldAudioPorts = App.CurrentAccount.Username;
                //                AudioPortsTextBox.Text = oldAudioPorts;
                //            }
            }

        public
            void OnVideoPortsChanged 
            (Object sender, RoutedEventArgs args)
            {
                string newVideoPorts = VideoPortsTextBox.Text;
                // Placeholder - not yet indicated for Windows

                //            if (string.IsNullOrEmpty(newVideoPorts))
                //            {
                //                string oldVideoPorts = App.CurrentAccount.Username;
                //                VideoPortsTextBox.Text = oldVideoPorts;
                //            }
            }

        private
            void OnMediaEncryptionChanged 
            (object sender, RoutedEventArgs e)
            {
                TextBlock valueTB = (TextBlock) MediaEncryptionComboBox.SelectedItem;
                string value = valueTB.Text;
                if (App.CurrentAccount != null)
                {
                    App.CurrentAccount.MediaEncryption = value;
                    // update media settings.
                    ServiceManager.Instance.ApplyMediaSettingsChanges();
                    ServiceManager.Instance.SaveAccountSettings();
                }

            }
        private
            void OnPushNotifications 
            (object sender, RoutedEventArgs e)
            {
                bool enabled = PushNotificationsCheckbox.IsChecked ?? false;
                // Placeholder - not yet indicated for Windows

            }

            #endregion

        

    }
}
