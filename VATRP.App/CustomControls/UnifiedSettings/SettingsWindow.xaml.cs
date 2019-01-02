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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private UnifiedSettings_AccountChange AccountChangeRequested;
        private CallViewCtrl _callControl;
        private BaseUnifiedSettingsPanel _currentContent;
        private BaseUnifiedSettingsPanel _previousContent;
        private List<BaseUnifiedSettingsPanel> _allPanels;

        public delegate void VideoMailHandler();
        public event VideoMailHandler updateVideomail; 

        public SettingsWindow(CallViewCtrl callControl, UnifiedSettings_AccountChange accountChangeRequestedMethod)
        {

            //************************************************************************************************************************************
            // Initilize of setting screen More==>Settings. Called only once when application runs first time.
            //************************************************************************************************************************************
            InitializeComponent();
            
            AccountChangeRequested += accountChangeRequestedMethod;

            _allPanels = new List<BaseUnifiedSettingsPanel>();

            BaseUnifiedSettingsPanel.EnableAdvancedSettings = false;
            BaseUnifiedSettingsPanel.EnabledDebugSettings = false;
            BaseUnifiedSettingsPanel.EnableSuperSettings = false;

            AccountSettings.ShowSettingsUpdate += HandleShowSettingsUpdate;
            AccountSettings.updateVideomail += OnUpdateVideomail;
            AccountSettings.ShiftFocus += focusCloseButton;
            InitializePanelAndEvents(AccountSettings);

            InitializePanelAndEvents(GeneralSettings);

            InitializePanelAndEvents(AudioVideoSettings);

            InitializePanelAndEvents(ThemeSettings);

            InitializePanelAndEvents(TextSettings);

            SummarySettings.ShowSettingsUpdate += HandleShowSettingsUpdate;
            InitializePanelAndEvents(SummarySettings);

//            InitializePanelAndEvents(AudioSettings);

//            InitializePanelAndEvents(VideoSettings);

//            InitializePanelAndEvents(CallSettings);

//            InitializePanelAndEvents(NetworkSettings);

            InitializePanelAndEvents(AdvancedSettings);

//            InitializePanelAndEvents(_viewTechnicalSupportPanel);

            _currentContent = GeneralSettings;
#if DEBUG
            HandleShowSettingsUpdate(UnifiedSettings_LevelToShow.Normal, true);
#else
            HandleShowSettingsUpdate(UnifiedSettings_LevelToShow.Normal, true);
#endif
            SetCallControl(callControl);

            this.Loaded += SettingsWindow_Loaded;

            AudioVideoSettings.UpdateAdvancedMenuHandler += UpdateAdvancedMenuBitrate;
        }

        /// <summary>
        /// Callback to update the advanced menu bandwidth text edit fields
        /// </summary>
        /// <returns>void</returns>
        private void UpdateAdvancedMenuBitrate()
        {
            AdvancedSettings.Initialize();
        }

        void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }

        private void SetCallControl(CallViewCtrl callControl)
        {
            _callControl = callControl;
            TextSettings.CallControl = _callControl;
//            AudioSettings.CallControl = _callControl;
            GeneralSettings.CallControl = _callControl;
        }

        private void InitializePanelAndEvents(BaseUnifiedSettingsPanel panel)
        {
            if (panel == null)
                return;

            if (!_allPanels.Contains(panel))
            {
                _allPanels.Add(panel);
            }
//            panel.ContentChanging += HandleContentChanging;
            panel.AddAccountChangedMethod(HandleAccountChangeRequested);
//            panel.AccountChangeRequested += HandleAccountChangeRequested;
        }

        #region ShowSettingsLevel
        public void HandleShowSettingsUpdate(UnifiedSettings_LevelToShow settingsType, bool show)
        {
            switch (settingsType)
            {
                case UnifiedSettings_LevelToShow.Advanced: BaseUnifiedSettingsPanel.EnableAdvancedSettings = show;
                    if (show)
                    {
                        AdvancedTab.Visibility = System.Windows.Visibility.Visible;
                    }
                    break;
                case UnifiedSettings_LevelToShow.Debug: BaseUnifiedSettingsPanel.EnabledDebugSettings = show;
                    break;
                case UnifiedSettings_LevelToShow.Normal: BaseUnifiedSettingsPanel.EnableAdvancedSettings = false;
                    BaseUnifiedSettingsPanel.EnabledDebugSettings = false;
                    BaseUnifiedSettingsPanel.EnableSuperSettings = false;
                    break;
                case UnifiedSettings_LevelToShow.Super: BaseUnifiedSettingsPanel.EnableSuperSettings = show;
                    break;
                default:
                    break;
            }
            foreach (BaseUnifiedSettingsPanel panel in _allPanels)
            {
                panel.ShowSettings(settingsType, show);
            }
        }
        #endregion

        // cjm-sep17 method to close settings window when logout requested
        public void OnCloseFromLogOut()
        {
            this.OnClose(null, null);
        }

        private void OnClose(object sender, RoutedEventArgs e)
        {

            //************************************************************************************************************************************
            // Close setting view/screen.
            //************************************************************************************************************************************
            Console.WriteLine("Close Clicked");
            _currentContent.SaveData();
            this.Hide();       
        }

        private void SetHidden()
        {
            _currentContent.SaveData();
            this.Hide();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentContent.SaveData();
            if (GeneralTab.IsSelected)
            {
                _currentContent = GeneralSettings;
            }
            else if (AudioVideoTab.IsSelected)
            {
                _currentContent = AudioVideoSettings;
            }
            else if (TextTab.IsSelected)
            {
                _currentContent = TextSettings;
            }
            else if (ThemeTab.IsSelected)
            {
                _currentContent = ThemeSettings;
            }
            else if (SummaryTab.IsSelected)
            {
                _currentContent = SummarySettings;
            }
            else if (AccountTab.IsSelected)
            {
                _currentContent = AccountSettings;
            }
            else if (MediaTab.IsSelected)
            {
//                _currentContent = MediaSettings;
            }
            else if (TestingTab.IsSelected)
            {
//                _currentContent = TestingSettings;
            }
            else if (AdvancedTab.IsSelected)
            {
                _currentContent = AdvancedSettings;
            }
        }


        #region respondToMenuChange
        public void RespondToMenuUpdate(ACEMenuSettingsUpdateType menuSetting)
        {
            switch (menuSetting)
            {
                case ACEMenuSettingsUpdateType.MuteMicrophoneMenu: UpdateAudioSettingsIfOpen(menuSetting);
                    break;
                case ACEMenuSettingsUpdateType.MuteSpeakerMenu: UpdateAudioSettingsIfOpen(menuSetting);
                    break;
                case ACEMenuSettingsUpdateType.ShowSelfViewMenu: UpdateVideoSettingsIfOpen(menuSetting);
                    break;
                default:
                    break;
            }
        }

        private void UpdateVideoSettingsIfOpen(ACEMenuSettingsUpdateType menuSetting)
        {
            if (GeneralSettings.IsLoaded)
            {
                GeneralSettings.UpdateForMenuSettingChange(menuSetting);
            }

            if (AdvancedSettings.IsLoaded)
            {
                AdvancedSettings.UpdateForMenuSettingChange(menuSetting);
            }

            if (AudioVideoSettings.IsLoaded)
            {
                AudioVideoSettings.UpdateForMenuSettingChange(menuSetting); 
            }
        }

        private void UpdateAudioSettingsIfOpen(ACEMenuSettingsUpdateType menuSetting)
        {
            if (AdvancedSettings.IsLoaded)
            {
                AdvancedSettings.UpdateForMenuSettingChange(menuSetting);
            }

            if (GeneralSettings.IsLoaded)
            {
                GeneralSettings.UpdateForMenuSettingChange(menuSetting);
            }
        }
        #endregion

        #region respondToRegistrationChange
        private void HandleAccountChangeRequested(ACEMenuSettingsUpdateType changeType)
        {
            if (AccountChangeRequested != null)
            {
                AccountChangeRequested(changeType);
            }
            // ToDo - this handle updates in the UI of the settings, if needed
        }

        #endregion

        private void OnVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            //************************************************************************************************************************************
            // Visibility changed of Setting screen, Setting screen get show or hide then this event will called.
            //************************************************************************************************************************************
            var isVisible = (bool) e.NewValue;
            if (!isVisible && (_currentContent != null))
            {
                _currentContent.IsDirty = true;
            }
        }

        /// <summary>
        /// Callback to update video mail count indicator 
        /// </summary>
        private void OnUpdateVideomail()
        {
            if (updateVideomail != null)
            {
                updateVideomail();
            }
        }

        /// <summary>
        /// Callback to set the focus to the close button when hitting enter
        /// </summary>
        private void focusCloseButton()
        {
            SettingsCloseButton.Focus();
        }
    }
}
