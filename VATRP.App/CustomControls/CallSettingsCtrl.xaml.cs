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
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using com.vtcsecure.ace.windows.Interfaces;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.CustomControls
{
    /// <summary>
    /// Interaction logic for CallSettingsCtrl.xaml
    /// </summary>
    public partial class CallSettingsCtrl : ISettings
    {
        #region Event
        public delegate void ResetToDefaultDelegate();
        public event ResetToDefaultDelegate ResetToDefaultEvent;
        #endregion

        public CallSettingsCtrl()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            EnableAutoAnswerBox.IsChecked = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AUTO_ANSWER, false);
            AnswerTimeoutTextBox.Text = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AUTO_ANSWER_AFTER, "2");
            EnableAVPFMode.IsChecked = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AVPF_ON, true);
            SendDtmfInfo.IsChecked = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                 Configuration.ConfEntry.DTMF_SIP_INFO, false);
            UseRTT.IsChecked = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
     Configuration.ConfEntry.USE_RTT, true);

        }

        #region ISettings

        public bool IsChanged()
        {
            var enabled = EnableAutoAnswerBox.IsChecked ?? false;
            var cfgTimeout = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AUTO_ANSWER_AFTER, 2);

            if (enabled != ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AUTO_ANSWER, false))
                return true;

            if (AnswerTimeoutTextBox.Text != cfgTimeout.ToString())
                return true;

            enabled = EnableAVPFMode.IsChecked ?? false;

            if (enabled != ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AVPF_ON, true))
                return true;

            enabled = SendDtmfInfo.IsChecked ?? false;

            if (enabled != ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.DTMF_SIP_INFO, false))
                return true;

            enabled = UseRTT.IsChecked ?? false;

            if (enabled != ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.USE_RTT, true))
                return true;
            return false;
        }

        public bool Save()
        {
            bool enabled = EnableAutoAnswerBox.IsChecked ?? false;

            if (enabled)
            {
                if (string.IsNullOrWhiteSpace(AnswerTimeoutTextBox.Text))
                {
                    MessageBox.Show("Please enter value between 0 and 60", "VATRP", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

                int val;

                if (!int.TryParse(AnswerTimeoutTextBox.Text, out val))
                    return false;

                if (val < 0 || val > 60)
                {
                    MessageBox.Show("Please enter value between 0 and 60", "VATRP", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

                ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                    Configuration.ConfEntry.AUTO_ANSWER_AFTER, val);
            }

            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AUTO_ANSWER, enabled);

            enabled = EnableAVPFMode.IsChecked ?? false;
            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AVPF_ON, enabled);

            enabled = SendDtmfInfo.IsChecked ?? false;
            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.DTMF_SIP_INFO, enabled);

            enabled = UseRTT.IsChecked ?? true;
            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.USE_RTT, enabled);

            ServiceManager.Instance.ConfigurationService.SaveConfig();
            return true;
        }

        #endregion

        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var allowed = IsTextAllowed(e.Text);
            var handled = true;
            do
            {
                if (!allowed)
                    break;
                handled = false;
            } while (false);

            e.Handled = handled;
        }

        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                var text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void OnClearData(object sender, RoutedEventArgs e)
        {
            if (
                MessageBox.Show("All user data will be cleared.\nDo you want to continue?", "VATRP",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if (ResetToDefaultEvent != null)
                    ResetToDefaultEvent();
            }
        }
    }
}
