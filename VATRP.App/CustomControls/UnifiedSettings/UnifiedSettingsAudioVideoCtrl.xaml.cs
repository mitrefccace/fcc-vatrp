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
using System.Collections.ObjectModel;
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
using com.vtcsecure.ace.windows.ViewModel;

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{

    /// <summary>
    /// Interaction logic for UnifiedSettingsAudioVideoCtrl.xaml
    /// </summary>
    public partial class UnifiedSettingsAudioVideoCtrl : BaseUnifiedSettingsPanel
    {

        public CallViewCtrl CallControl;

//        private UnifiedSettingsDeviceCtrl _cameraSelectionCtrl;
//        private UnifiedSettingsDeviceCtrl _microphoneSelectionCtrl;
//        private UnifiedSettingsDeviceCtrl _speakerSelectionCtrl;

        public ObservableCollection<VATRPDevice> CameraList { get; private set; }
        public ObservableCollection<VATRPDevice> SpeakerList { get; private set; }
        public ObservableCollection<VATRPDevice> MicrophoneList { get; private set; }

        public delegate void AdvancedSettingsHandler();
        public event AdvancedSettingsHandler UpdateAdvancedMenuHandler;

        public UnifiedSettingsAudioVideoCtrl()
        {
            InitializeComponent();
            Title = "Audio/Video";
            this.Loaded += UnifiedSettingsAudioVideoCtrl_Loaded;
/*            _cameraSelectionCtrl = new UnifiedSettingsDeviceCtrl();
            _cameraSelectionCtrl.OnDeviceSelected += HandleDeviceSelected;

            _microphoneSelectionCtrl = new UnifiedSettingsDeviceCtrl();
            _microphoneSelectionCtrl.OnDeviceSelected += HandleDeviceSelected;

            _speakerSelectionCtrl = new UnifiedSettingsDeviceCtrl();
            _speakerSelectionCtrl.OnDeviceSelected += HandleDeviceSelected;
*/
            CameraList = new ObservableCollection<VATRPDevice>();
            MicrophoneList = new ObservableCollection<VATRPDevice>();
            SpeakerList = new ObservableCollection<VATRPDevice>();

            this.DataContext = this;

        }

        // ToDo VATRP987 - Liz E. these need to be hooked into acutal settings. not sure where they live.
        private void UnifiedSettingsAudioVideoCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        public override void Initialize()
        {
            base.Initialize();
            if (App.CurrentAccount == null)
                return;
/*            List<VATRPDevice> cameraList = ServiceManager.Instance.GetAvailableCameras();
            string storedCameraId = App.CurrentAccount.SelectedCameraId;
            if (cameraList != null)
            {
                List<string> cameraNames = new List<string>();
                foreach (VATRPDevice device in cameraList)
                {
                    cameraNames.Add(device.displayName);
                    if (!string.IsNullOrEmpty(storedCameraId) && storedCameraId.Equals(device.deviceId))
                    {
                        _selectedCamera = device.displayName;
                    }
                }
                Cameras = cameraNames.ToArray();
            }
*/
            foreach (var item in PreferredVideoSizeComboBox.Items)
            {
                var tb = item as TextBlock;
                if (GetPreferredVideoSizeId(tb).Equals(App.CurrentAccount.PreferredVideoId))
                {
                    PreferredVideoSizeComboBox.SelectedItem = item;
                    break;
                }
            }

            List<VATRPDevice> availableCameras = ServiceManager.Instance.GetAvailableCameras();
            VATRPDevice selectedCamera = ServiceManager.Instance.GetSelectedCamera();
            CameraList.Clear();
            if (availableCameras == null)
            {
                return;
            }
            foreach (VATRPDevice camera in availableCameras)
            {
                CameraList.Add(camera);
                if ((selectedCamera != null) && selectedCamera.deviceId.Trim().Equals(camera.deviceId.Trim()))
                {
                    SelectCameraComboBox.SelectedItem = camera;
                }

            }

            List<VATRPDevice> availableMicrophones = ServiceManager.Instance.GetAvailableMicrophones();
            VATRPDevice selectedMicrophone = ServiceManager.Instance.GetSelectedMicrophone();
            MicrophoneList.Clear();
            foreach (VATRPDevice microphone in availableMicrophones)
            {
                MicrophoneList.Add(microphone);
                if ((selectedMicrophone != null) && selectedMicrophone.deviceId.Trim().Equals(microphone.deviceId.Trim()))
                {
                    SelectMicrophoneComboBox.SelectedItem = microphone;
                }

            }
            List<VATRPDevice> availableSpeakers = ServiceManager.Instance.GetAvailableSpeakers();
            VATRPDevice selectedSpeaker = ServiceManager.Instance.GetSelectedSpeakers();
            SpeakerList.Clear();
            foreach (VATRPDevice speaker in availableSpeakers)
            {
                SpeakerList.Add(speaker);
                if ((selectedSpeaker != null) && selectedSpeaker.deviceId.Trim().Equals(speaker.deviceId.Trim()))
                {
                    SelectSpeakerComboBox.SelectedItem = speaker;
                }

            }

            /*            string selectedCameraId = App.CurrentAccount.SelectedCameraId;
            if (string.IsNullOrEmpty(selectedCameraId))
            {
                VATRPDevice selectedCamera = ServiceManager.Instance.GetSelectedCamera();
//                SelectedCameraLabel.Content = selectedCamera.displayName;
//                SelectedCameraLabel.ToolTip = selectedCamera.displayName;
            }
            else
            {
                foreach (VATRPDevice camera in availableCameras)
                {
                    if (!string.IsNullOrEmpty(selectedCameraId) && selectedCameraId.Equals(camera.deviceId))
                    {
                        SelectCameraComboBox.SelectedItem = camera;
//                        SelectedCameraLabel.Content = camera.displayName;
//                        SelectedCameraLabel.ToolTip = camera.displayName;
                    }
                }
            }
            
            List<VATRPDevice> availableMicrophones = ServiceManager.Instance.GetAvailableMicrophones();
            string selectedMicrophoneId = App.CurrentAccount.SelectedMicrophoneId;
            if (string.IsNullOrEmpty(selectedMicrophoneId))
            {
                VATRPDevice selectedMicrophone = ServiceManager.Instance.GetSelectedMicrophone();
                SelectedMicrophoneLabel.Content = selectedMicrophone.displayName;
                SelectedMicrophoneLabel.ToolTip = selectedMicrophone.displayName;
            }
            else
            {
                foreach (VATRPDevice microphone in availableMicrophones)
                {
                    if (!string.IsNullOrEmpty(selectedMicrophoneId) && selectedMicrophoneId.Equals(microphone.deviceId))
                    {
                        SelectedMicrophoneLabel.Content = microphone.displayName;
                        SelectedMicrophoneLabel.ToolTip = microphone.displayName;
                    }
                }
            }

            List<VATRPDevice> availableSpeakers = ServiceManager.Instance.GetAvailableSpeakers();
            string selectedSpeakerId = App.CurrentAccount.SelectedSpeakerId;
            if (string.IsNullOrEmpty(selectedSpeakerId))
            {
                VATRPDevice selectedSpeaker = ServiceManager.Instance.GetSelectedSpeakers();
                SelectedSpeakerLabel.Content = selectedSpeaker.displayName;
                SelectedSpeakerLabel.ToolTip = selectedSpeaker.displayName;
            }
            else
            {
                foreach (VATRPDevice speaker in availableSpeakers)
                {
                    if (!string.IsNullOrEmpty(selectedSpeakerId) && selectedSpeakerId.Equals(speaker.deviceId))
                    {
                        SelectedSpeakerLabel.Content = speaker.displayName;
                        SelectedSpeakerLabel.ToolTip = speaker.displayName;
                    }
                }
            }
             * */
        }

        // VATRP-1200 TODO - store settings, update Linphone
        #region Device selection
/*        private void OnShowCameraOptions(object sender, RoutedEventArgs e)
        {
            List<VATRPDevice> availableCameras = ServiceManager.Instance.GetAvailableCameras();
            _cameraSelectionCtrl.deviceList = availableCameras;
            _cameraSelectionCtrl.Initialize();
            this.ContentPanel.Content = _cameraSelectionCtrl;
        }

        private void OnShowMicrophoneOptions(object sender, RoutedEventArgs e)
        {
            List<VATRPDevice> availableMicrophones = ServiceManager.Instance.GetAvailableMicrophones();
            _microphoneSelectionCtrl.deviceList = availableMicrophones;
            _microphoneSelectionCtrl.Initialize();
            this.ContentPanel.Content = _microphoneSelectionCtrl;
        }

        private void OnShowSpeakerOptions(object sender, RoutedEventArgs e)
        {
            List<VATRPDevice> availableSpeakers = ServiceManager.Instance.GetAvailableSpeakers();
            _speakerSelectionCtrl.deviceList = availableSpeakers;
            _speakerSelectionCtrl.Initialize();
            this.ContentPanel.Content = _speakerSelectionCtrl;
        }

        private void HandleDeviceSelected(VATRPDevice device)
        {
            this.ContentPanel.Content = null;
            if (device == null)
                return;

            switch (device.deviceType)
            {
                case VATRPDeviceType.CAMERA: HandleCameraSelected(device);
                    break;
                case VATRPDeviceType.MICROPHONE: HandleMicrophoneSelected(device);
                    break;
                case VATRPDeviceType.SPEAKER: HandleSpeakerSelected(device);
                    break;
                default: break;
            }
        }
*/

        private void OnSelectCamera(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null || !_initialized) return;
            Console.WriteLine("Camera Selected");

            bool updateData = false;
            VATRPDevice selectedCamera = (VATRPDevice)SelectCameraComboBox.SelectedItem;
            string selectedCameraId = App.CurrentAccount.SelectedCameraId;
            if (string.IsNullOrEmpty(selectedCameraId))
            {
                if (SelectCameraComboBox.Items.Count > 0)
                {
                    var device = SelectCameraComboBox.Items.GetItemAt(0) as VATRPDevice;
                    if (device != null)
                    {
                        updateData = true;
                        App.CurrentAccount.SelectedCameraId = device.deviceId;
                        selectedCameraId = device.deviceId;
                    }
                }
            }
            if ((selectedCamera != null && selectedCamera.deviceId != selectedCameraId) || updateData)
            {
                if (selectedCamera != null) 
                    App.CurrentAccount.SelectedCameraId = selectedCamera.deviceId;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();
            }
        }

        private void OnSelectMicrophone(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null || !_initialized) return;
            Console.WriteLine("Microphone Selected");
            bool updateData = false;
            VATRPDevice selectedMicrophone = (VATRPDevice)SelectMicrophoneComboBox.SelectedItem;
            string selectedMicrophoneId = App.CurrentAccount.SelectedMicrophoneId;

            if (string.IsNullOrEmpty(selectedMicrophoneId))
            {
                if (SelectMicrophoneComboBox.Items.Count > 0)
                {
                    var device = SelectMicrophoneComboBox.Items.GetItemAt(0) as VATRPDevice;
                    if (device != null)
                    {
                        updateData = true;
                        App.CurrentAccount.SelectedMicrophoneId = device.deviceId;
                        selectedMicrophoneId = device.deviceId;
                    }
                }
            }

            if ((selectedMicrophone != null && selectedMicrophone.deviceId != selectedMicrophoneId) || updateData )
            {
                if (selectedMicrophone != null) 
                    App.CurrentAccount.SelectedMicrophoneId = selectedMicrophone.deviceId;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();
            }
        }

        private void OnSelectSpeaker(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null || !_initialized) return;
            Console.WriteLine("Speaker Selected");
            bool updateData = false;
            VATRPDevice selectedSpeaker = (VATRPDevice)SelectSpeakerComboBox.SelectedItem;
            string selectedSpeakerId = App.CurrentAccount.SelectedSpeakerId;
            
            if (string.IsNullOrEmpty(selectedSpeakerId))
            {
                if (SelectSpeakerComboBox.Items.Count > 0)
                {
                    var device = SelectSpeakerComboBox.Items.GetItemAt(0) as VATRPDevice;
                    if (device != null)
                    {
                        updateData = true;
                        App.CurrentAccount.SelectedSpeakerId = device.deviceId;
                        selectedSpeakerId = device.deviceId;
                    }
                }
            }

            if ((selectedSpeaker != null && selectedSpeaker.deviceId != selectedSpeakerId) || updateData)
            {
                if (selectedSpeaker != null) 
                    App.CurrentAccount.SelectedSpeakerId = selectedSpeaker.deviceId;
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();
            }
        }

        #endregion

        private bool IsPreferredVideoSizeChanged()
        {
            if (App.CurrentAccount == null)
                return false;

            var tb = PreferredVideoSizeComboBox.SelectedItem as TextBlock;
            string str = GetPreferredVideoSizeId(tb);
            if ((string.IsNullOrWhiteSpace(str) && !string.IsNullOrWhiteSpace(App.CurrentAccount.PreferredVideoId)) ||
                (!string.IsNullOrWhiteSpace(str) && string.IsNullOrWhiteSpace(App.CurrentAccount.PreferredVideoId)))
                return true;
            if ((!string.IsNullOrWhiteSpace(str) && !string.IsNullOrWhiteSpace(App.CurrentAccount.PreferredVideoId)) &&
                (!str.Equals(App.CurrentAccount.PreferredVideoId)))
                return true;
            return false;
        }

        private string GetPreferredVideoSizeId(TextBlock tb)
        {
            if (tb == null)
                return string.Empty;

            var index = tb.Text.IndexOf(" (", System.StringComparison.Ordinal);
            return index != -1 ? tb.Text.Substring(0, index).Trim() : string.Empty;
        }

        private void OnPreferredVideoSize(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Preferred Video Size Clicked");
            if (App.CurrentAccount == null)
                return;

            if (!IsPreferredVideoSizeChanged())
            {
                return;
            }

            var tb = PreferredVideoSizeComboBox.SelectedItem as TextBlock;
            if (tb != null)
            {
                string str = GetPreferredVideoSizeId(tb);
                if (string.IsNullOrWhiteSpace(str))
                    return;
                
                App.CurrentAccount.PreferredVideoId = str;
            }

            ServiceManager.Instance.ApplyMediaSettingsChanges();

            if (UpdateAdvancedMenuHandler != null)
            {
                UpdateAdvancedMenuHandler();
            }

            ServiceManager.Instance.SaveAccountSettings();
        }

        public void disableElements()
        {
            PreferredVideoSizeComboBox.IsEnabled = false;
            SelectCameraComboBox.IsEnabled = false;
            SelectMicrophoneComboBox.IsEnabled = false;
            SelectSpeakerComboBox.IsEnabled = false;
        }
    }
}
