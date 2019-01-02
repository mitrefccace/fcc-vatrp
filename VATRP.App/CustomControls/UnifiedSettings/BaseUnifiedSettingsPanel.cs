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
using com.vtcsecure.ace.windows.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;


namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{
    public enum UnifiedSettings_LevelToShow
    {
        Normal, 
        Advanced,
        Debug,
        Super
    }

    public delegate void UnifiedSettings_EnableSettings(UnifiedSettings_LevelToShow settingsType, bool show);
    public delegate void UnifiedSettings_ContentChanging(UnifiedSettingsContentType contentType);
    public delegate void UnifiedSettings_AccountChange(ACEMenuSettingsUpdateType changeType);


    public class BaseUnifiedSettingsPanel : UserControl
    {
        public static bool EnableAdvancedSettings;
        public static bool EnabledDebugSettings;
        public static bool EnableSuperSettings;

        // VATRP-1170: Show items that have been implemented but are not specified for windows.
        public static System.Windows.Visibility VisibilityForSuperSettingsAsPreview = System.Windows.Visibility.Visible;

        private MainControllerViewModel _parentViewModel;

        protected bool _initialized;
        internal bool IsDirty { get; set; }

        public string Title { get; set; }
        // Call When Panel Content needs to change
        public event UnifiedSettings_ContentChanging ContentChanging;
        public event UnifiedSettings_AccountChange AccountChangeRequested;

        public BaseUnifiedSettingsPanel()
        {
            this.IsVisibleChanged += OnVisibilityChanged;
        }

        private void OnVisibilityChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool) e.NewValue)
            {
                if (IsDirty)
                {
                    Initialize();
                }
            }
        }

        public void ShowSettings(UnifiedSettings_LevelToShow settingsType, bool show)
        {
            switch (settingsType)
            {
                case UnifiedSettings_LevelToShow.Advanced: ShowAdvancedOptions(show);
                    break;
                case UnifiedSettings_LevelToShow.Debug: ShowDebugOptions(show);
                    break;
                case UnifiedSettings_LevelToShow.Super: ShowSuperOptions(show);
                    break;
                default:   // show normal
                    ShowNormalOptions();
                    break;
            }
        }

        public void ShowNormalOptions()
        {
            ShowAdvancedOptions(false);
            ShowDebugOptions(false);
            ShowSuperOptions(false);
        }

        public virtual void ShowDebugOptions(bool show)
        {
        }

        public virtual void ShowAdvancedOptions(bool show)
        {
        }

        public virtual void ShowSuperOptions(bool show)
        {
            ShowDebugOptions(show);
            ShowAdvancedOptions(show);
        }

        public virtual void AddAccountChangedMethod(UnifiedSettings_AccountChange accountChangedHandler)
        {
            this.AccountChangeRequested += accountChangedHandler;
        }
        // Invoke the Content Changed event
        public virtual void OnContentChanging(UnifiedSettingsContentType contentType)
        {
            if (ContentChanging != null)
            {
                ContentChanging(contentType);
            }
        }

        // Invoke the Account Change Requested event
        public virtual void OnAccountChangeRequested(ACEMenuSettingsUpdateType changeType)
        {
            if (AccountChangeRequested != null)
            {
                AccountChangeRequested(changeType);
            }
        }

        public virtual void Initialize()
        {
            _initialized = true;
            IsDirty = false;
            //ShowNormalOptions();
        }
		
        public virtual void SaveData()
        {
        }

        public virtual void UpdateForMenuSettingChange(ACEMenuSettingsUpdateType menuSetting)
        {
        }

    }
}
