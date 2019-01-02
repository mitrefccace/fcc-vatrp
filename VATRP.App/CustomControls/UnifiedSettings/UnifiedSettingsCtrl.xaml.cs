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
    /// Interaction logic for UnifiedSettingsCtrl.xaml
    /// </summary>
    public partial class UnifiedSettingsCtrl : UserControl
    {
        public UnifiedSettings_AccountChange AccountChangeRequested;

        private CallViewCtrl _callControl;

        private UnifiedSettingsMainCtrl _mainPanel;
        private UnifiedSettingsGeneralCtrl _generalPanel;
        private UnifiedSettingsAudioVideoCtrl _audioVideoPanel;
        private UnifiedSettingsThemeCtrl _themePanel;
        private UnifiedSettingsTextCtrl _textPanel;
        private UnifiedSettingsSummaryCtrl _summaryPanel;

        private UnifiedSettingsAudioCtrl _audioSettingsPanel;
        private UnifiedSettingsVideoCtrl _videoSettingsPanel;
        private UnifiedSettingsCallCtrl _callSettingsPanel;
        private UnifiedSettingsNetworkCtrl _networkSettingsPanel;
        private UnifiedSettingsAdvancedCtrl _advancedSettingsPanel;

        private TechnicalSupportSheetCtrl _viewTechnicalSupportPanel;

        private BaseUnifiedSettingsPanel _currentContent;
        private List<BaseUnifiedSettingsPanel> _previousContent;

        private List<BaseUnifiedSettingsPanel> _allPanels;

        public UnifiedSettingsCtrl()
        {
            InitializeComponent();
            _previousContent = new List<BaseUnifiedSettingsPanel>();
            _allPanels = new List<BaseUnifiedSettingsPanel>();

            BaseUnifiedSettingsPanel.EnableAdvancedSettings = false;
            BaseUnifiedSettingsPanel.EnabledDebugSettings = false;
            BaseUnifiedSettingsPanel.EnableSuperSettings = false;

            _mainPanel = new UnifiedSettingsMainCtrl();
            _mainPanel.ShowSettingsUpdate += HandleShowSettingsUpdate;
            InitializePanelAndEvents(_mainPanel);

            _generalPanel = new UnifiedSettingsGeneralCtrl();
            InitializePanelAndEvents(_generalPanel);

            _audioVideoPanel = new UnifiedSettingsAudioVideoCtrl();
            InitializePanelAndEvents(_audioVideoPanel);

            _themePanel = new UnifiedSettingsThemeCtrl();
            InitializePanelAndEvents(_themePanel);

            _textPanel = new UnifiedSettingsTextCtrl();
            InitializePanelAndEvents(_textPanel);

            _summaryPanel = new UnifiedSettingsSummaryCtrl();
            _summaryPanel.ShowSettingsUpdate += HandleShowSettingsUpdate;
            InitializePanelAndEvents(_summaryPanel);

            _audioSettingsPanel = new UnifiedSettingsAudioCtrl();
            InitializePanelAndEvents(_audioSettingsPanel);

            _videoSettingsPanel = new UnifiedSettingsVideoCtrl();
            InitializePanelAndEvents(_videoSettingsPanel);

            _callSettingsPanel = new UnifiedSettingsCallCtrl();
            InitializePanelAndEvents(_callSettingsPanel);

            _networkSettingsPanel = new UnifiedSettingsNetworkCtrl();
            InitializePanelAndEvents(_networkSettingsPanel);

            _advancedSettingsPanel = new UnifiedSettingsAdvancedCtrl();
            InitializePanelAndEvents(_advancedSettingsPanel);

            _viewTechnicalSupportPanel = new TechnicalSupportSheetCtrl();
            InitializePanelAndEvents(_viewTechnicalSupportPanel);

            _currentContent = _mainPanel;
#if DEBUG
            HandleShowSettingsUpdate(UnifiedSettings_LevelToShow.Normal, true);
#else
            HandleShowSettingsUpdate(UnifiedSettings_LevelToShow.Normal, true);
#endif
            UpdateContentInUI();
        }

        private void InitializePanelAndEvents(BaseUnifiedSettingsPanel panel)
        {
            if (panel == null)
                return;
            
            if (!_allPanels.Contains(panel))
            {
                _allPanels.Add(panel);
            }

            panel.ContentChanging += HandleContentChanging;
            panel.AccountChangeRequested += HandleAccountChangeRequested;
        }

        public void Initialize()
        {
            _mainPanel.Initialize();
            // the other panels are initialized when they are shown
        }

        public void SetCallControl(CallViewCtrl callControl)
        {
            _callControl = callControl;
            _textPanel.CallControl = _callControl;
            _audioSettingsPanel.CallControl = _callControl;
            _audioVideoPanel.CallControl = _callControl;
        }

        private void UpdateContentInUI()
        {
            // in case current content is not set, revert to main panel to 'restart'
            if (_currentContent == null)
            {
                Console.WriteLine("UnifiedSettings: Navigation error - _currentContent is null");
                _currentContent = _mainPanel;
                _previousContent.Clear();
                this.ContentPanel.Content = _mainPanel;
            }
            _currentContent.Initialize();
            this.ContentPanel.Content = _currentContent;
            this.TitleLabel.Content = _currentContent.Title;
            this.MaxWidth = 300;

            if (_currentContent == _mainPanel)
            {
                BackLabel.Content = "";
            }
            else
            {
                BackLabel.Content = "< Back";
            }
        }

        private void OnBack(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Back Clicked");
            if (_currentContent == _mainPanel)
            {
                // ignore
                return;
            }
            if (_previousContent.Count > 0)
            {
                // beacuse this comes from the main control not from within the content, 
                //   make sure that the current data is saved before changing content
                _currentContent.SaveData();

                // set the new content in this panel
                _currentContent = _previousContent[_previousContent.Count - 1];
                // pop the panel that is now the current panel off the previous stack
                _previousContent.Remove(_currentContent);

                UpdateContentInUI();
            }
        }

        private void OnAbout(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("About Clicked");

            UpdateContentInUI();
        }

        #region ShowSettingsLevel
        public void HandleShowSettingsUpdate(UnifiedSettings_LevelToShow settingsType, bool show)
        {
            switch (settingsType)
            {
                case UnifiedSettings_LevelToShow.Advanced: BaseUnifiedSettingsPanel.EnableAdvancedSettings = show;
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

        #region panel navigation
        private void HandleContentChanging(UnifiedSettingsContentType newContentType)
        {
            switch (newContentType)
            {
                case UnifiedSettingsContentType.MainContent: // clear previous, set to main
                    _currentContent.SaveData();
                    _previousContent.Clear();
                    _currentContent = _mainPanel;
                    UpdateContentInUI();
                    break;
                case UnifiedSettingsContentType.GeneralContent: MoveToContentPanel(_generalPanel);
                    break;
                case UnifiedSettingsContentType.AudioVideoContent: MoveToContentPanel(_audioVideoPanel);
                    break;
                case UnifiedSettingsContentType.ThemeContent: MoveToContentPanel(_themePanel);
                    break;
                case UnifiedSettingsContentType.TextContent: MoveToContentPanel(_textPanel);
                    break;
                case UnifiedSettingsContentType.SummaryContent: MoveToContentPanel(_summaryPanel);
                    break;
                case UnifiedSettingsContentType.AudioSettingsContent: MoveToContentPanel(_audioSettingsPanel);
                    break;
                case UnifiedSettingsContentType.VideoSettingsContent: MoveToContentPanel(_videoSettingsPanel);
                    break;
                case UnifiedSettingsContentType.CallSettingsContent: MoveToContentPanel(_callSettingsPanel);
                    break;
                case UnifiedSettingsContentType.NetworkSettingsContent: MoveToContentPanel(_networkSettingsPanel);
                    break;
                case UnifiedSettingsContentType.AdvancedSettingsContent: MoveToContentPanel(_advancedSettingsPanel);
                    break;

                case UnifiedSettingsContentType.ViewTSS: MoveToContentPanel(_viewTechnicalSupportPanel);
                    break;
                default: break;
            }
        }

        private void MoveToContentPanel(BaseUnifiedSettingsPanel newPanel)
        {
            _currentContent.SaveData();

            _previousContent.Add(_currentContent);
            _currentContent = newPanel;
            UpdateContentInUI();
        }
        #endregion

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
            if (_videoSettingsPanel.IsLoaded)
            {
                _videoSettingsPanel.UpdateForMenuSettingChange(menuSetting);
            }
            if (_audioVideoPanel.IsLoaded)
            {
                _audioVideoPanel.UpdateForMenuSettingChange(menuSetting);
            }
        }
        private void UpdateAudioSettingsIfOpen(ACEMenuSettingsUpdateType menuSetting)
        {
            if (_audioSettingsPanel.IsLoaded)
            {
                _audioSettingsPanel.UpdateForMenuSettingChange(menuSetting);
            }
            if (_audioVideoPanel.IsLoaded)
            {
                _audioVideoPanel.UpdateForMenuSettingChange(menuSetting);
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

    }
}
