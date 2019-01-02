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
    /// Interaction logic for UnifiedSettingsAudioCtrl.xaml
    /// </summary>
    public partial class UnifiedSettingsAudioCtrl : BaseUnifiedSettingsPanel
    {
        public CallViewCtrl CallControl;
        private CollectionView _codecsView;

        public UnifiedSettingsAudioCtrl()
        {
            InitializeComponent();
            this.Loaded += UnifiedSettingsAudioCtrl_Loaded;
        }

        void UnifiedSettingsAudioCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        public override void Initialize()
        {
            if (App.CurrentAccount == null)
                return;

            MuteMicrophoneCheckBox.IsChecked = App.CurrentAccount.MuteMicrophone;
            MuteSpeakerCheckBox.IsChecked = App.CurrentAccount.MuteSpeaker;

            AudioCodecsListView.ItemsSource = App.CurrentAccount.AudioCodecsList;
            _codecsView = (CollectionView)CollectionViewSource.GetDefaultView(AudioCodecsListView.ItemsSource);
            if (_codecsView != null)
            {
                _codecsView.SortDescriptions.Add(new SortDescription("Priority", ListSortDirection.Ascending));
                _codecsView.Refresh();
            }
        }

        public override void UpdateForMenuSettingChange(ACEMenuSettingsUpdateType menuSetting)
        {
            if (App.CurrentAccount == null)
                return;

            switch (menuSetting)
            {
                case ACEMenuSettingsUpdateType.MuteMicrophoneMenu: MuteMicrophoneCheckBox.IsChecked = App.CurrentAccount.MuteMicrophone;
                    break;
                case ACEMenuSettingsUpdateType.MuteSpeakerMenu: MuteSpeakerCheckBox.IsChecked = App.CurrentAccount.MuteSpeaker;
                    break;
                default:
                    break;
            }
        }

        public bool IsChanged()
        {
            if (App.CurrentAccount == null)
                return false;

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

        public override void SaveData() // called when we leave this page
        {
            if (App.CurrentAccount == null)
                return;

            foreach (var item in AudioCodecsListView.Items)
            {
                var cfgCodec = item as VATRPCodec;
                if (cfgCodec != null)
                {
                    // not working, commenting out to get the other items in
                    // TODO: this is ugly too - handling the fact that we are displaying a different set than linphone is providing.
                    //foreach (var availableAccountCodec in App.CurrentAccount.AudioCodecsList)
                    //{
                    //    if (availableAccountCodec.CodecName == cfgCodec.CodecName && availableAccountCodec.Channels == cfgCodec.Channels &&
                    //        availableAccountCodec.Rate == cfgCodec.Rate && availableAccountCodec.Status != cfgCodec.Status)
                    //    {
                    //        availableAccountCodec.Status = cfgCodec.Status;
                    //    }
                    //}
                    foreach (var accountCodec in App.CurrentAccount.AudioCodecsList)
                    {
                        if (accountCodec.CodecName == cfgCodec.CodecName && accountCodec.Channels == cfgCodec.Channels &&
                            accountCodec.Rate == cfgCodec.Rate && accountCodec.Status != cfgCodec.Status)
                        {
                            accountCodec.Status = cfgCodec.Status;
                        }
                        // not working, commenting out to get the other items in
                        // ToDO: Note: this is very ugly, but will get the job done immediately and is working over a short list.
                        //   a central settings handler like set up on Mac would isolate this logic away from the ui.
                        //if (cfgCodec.CodecName.Equals("G711"))
                       // {
                            // find and set the pcma && pcmu to match the g711
                         //   foreach (var existingAccountCodec in App.CurrentAccount.AudioCodecsList)
                           // {
                             //   if (existingAccountCodec.CodecName.ToLower().Equals("pcmu") || existingAccountCodec.CodecName.ToLower().Equals("pcma"))
                               // {
                                 //   existingAccountCodec.Status = cfgCodec.Status;
                             //   }
                           // }
                       // }
                    }
                }
            }
            ServiceManager.Instance.ApplyCodecChanges();
            ServiceManager.Instance.SaveAccountSettings();
        }

        #region Audio Settings (in call)
        private void OnMuteMicrophone(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;
            Console.WriteLine("Mute Microphone Clicked");
            bool enabled = MuteMicrophoneCheckBox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.MuteMicrophone)
            {
                App.CurrentAccount.MuteMicrophone = enabled;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();

                if ((CallControl != null) && CallControl.IsLoaded)
                {
                    CallControl.UpdateMuteSettingsIfOpen();
                }
            }
        }
        private void OnMuteSpeaker(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;
            Console.WriteLine("Mute Speaker Clicked");
            bool enabled = MuteSpeakerCheckBox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.MuteSpeaker)
            {
                App.CurrentAccount.MuteSpeaker = enabled;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();

                if ((CallControl != null) && CallControl.IsLoaded)
                {
                    CallControl.UpdateMuteSettingsIfOpen();
                }
            }
        }

        #endregion

        #region Audio Codecs

        private void AudioCodecsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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

        private void CheckBox_Click(object sender, RoutedEventArgs e)
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


    }
}
