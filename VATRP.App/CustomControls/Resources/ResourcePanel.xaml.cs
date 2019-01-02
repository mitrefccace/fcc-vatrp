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

namespace com.vtcsecure.ace.windows.CustomControls.Resources
{
    /// <summary>
    /// Interaction logic for ResourcePanel.xaml
    /// </summary>
    public partial class ResourcePanel : UserControl
    {
        public Resources_CallResource CallResourceRequested;

        private ResourceMainCtrl _mainPanel;
        private DeafHoHResourcesPanel _deafHohPanel;

        private BaseResourcePanel _currentContent;
        private List<BaseResourcePanel> _previousContent;

        private List<BaseResourcePanel> _allPanels;

        public ResourcePanel()
        {
            InitializeComponent();
            _previousContent = new List<BaseResourcePanel>();
            _allPanels = new List<BaseResourcePanel>();

            _mainPanel = new ResourceMainCtrl();
            InitializePanelAndEvents(_mainPanel);

            _deafHohPanel = new DeafHoHResourcesPanel();
            InitializePanelAndEvents(_deafHohPanel);
            _deafHohPanel.CallResourceRequested += HandleCallResourceRequested;

            ScrollViewer.SetVerticalScrollBarVisibility(scrollViewer, ScrollBarVisibility.Hidden);

            _currentContent = _mainPanel;
            UpdateContentInUI();
        }

        private void InitializePanelAndEvents(BaseResourcePanel panel)
        {
            if (panel == null)
                return;

            if (!_allPanels.Contains(panel))
            {
                _allPanels.Add(panel);
            }

            panel.ContentChanging += HandleContentChanging;
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
            //_currentContent.Initialize();
            this.ContentPanel.Content = _currentContent;
            this.TitleLabel.Content = _currentContent.Title;

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
                //_currentContent.SaveData();

                // set the new content in this panel
                _currentContent = _previousContent[_previousContent.Count - 1];
                // pop the panel that is now the current panel off the previous stack
                _previousContent.Remove(_currentContent);

                UpdateContentInUI();
            }
        }

        #region panel navigation
        private void HandleContentChanging(ResourcesType newContentType)
        {
            switch (newContentType)
            {
                case ResourcesType.MainContent: // clear previous, set to main
                    //_currentContent.SaveData();
                    _previousContent.Clear();
                    _currentContent = _mainPanel;
                    UpdateContentInUI();
                    break;
                case ResourcesType.DeafHoHResourcesContent: MoveToContentPanel(_deafHohPanel);
                    break;
                default: break;
            }
        }

        private void MoveToContentPanel(BaseResourcePanel newPanel)
        {
//            _currentContent.SaveData();

            _previousContent.Add(_currentContent);
            _currentContent = newPanel;
            UpdateContentInUI();
        }
        #endregion

        private void HandleCallResourceRequested(ResourceInfo resourceInfo)
        {
            if (CallResourceRequested != null)
            {
                CallResourceRequested(resourceInfo);
            }
        }

        public void ActivateDeafHohResource()
        {
            HandleContentChanging(ResourcesType.DeafHoHResourcesContent);
        }

    }
}
