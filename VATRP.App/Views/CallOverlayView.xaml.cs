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
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using com.vtcsecure.ace.windows.Interfaces;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;
using com.vtcsecure.ace.windows.ViewModel;
using VATRP.Core.Model;
using VATRP.LinphoneWrapper;
using VATRP.LinphoneWrapper.Enums;
using VATRP.LinphoneWrapper.Structs;

namespace com.vtcsecure.ace.windows.Views
{
    /// <summary>
    /// Interaction logic for CallOverlayView.xaml
    /// </summary>
    public partial class CallOverlayView
    {
        public bool IsAnimating { get; set; }
        public bool EndCallRequested { get; set; }
        public CallProcessingBox CallManagerView { get; set; }
        public CallOverlayView()
        {
            InitializeComponent();
        }

        private void OnEndCall(object sender, RoutedEventArgs e)
        {
            EndCallRequested = true;
            Hide();
        }

        private void ToggleInfoWindow(object sender, RoutedEventArgs e)
        {
            //CallManagerView.ToggleCallStatisticsInfo();
        }

        private void OnSwitchKeypad(object sender, RoutedEventArgs e)
        {
         //  CallManagerView.ToggleKeypadView();
        }

        private void OnMute(object sender, RoutedEventArgs e)
        {
           // CallManagerView.MuteCall();
        }

        public void ShowAnimated()
        {
            if (IsAnimating || Opacity == 1.0)
                return;

            IsAnimating = true;
            var s = (Storyboard)Resources["ShowAnim"];
            //if (s != null) 
            //    s.Begin();
        }

        public void HideAnimated()
        {
            if (IsAnimating || Opacity == 0)
                return;

            IsAnimating = true;
            //var s = (Storyboard)Resources["HideAnim"];
            //if (s != null) 
            //    s.Begin();
        }

        private void AnimationCompleted(object sender, EventArgs e)
        {
            IsAnimating = false;
        }


        private void OnVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool bShow = Convert.ToBoolean(e.NewValue);
            Debug.WriteLine("!!!!!!!!!!!!!!!! Visibility changed to " + (bShow ? "Visible" :"Hidden"));
            if (EndCallRequested && !bShow)
            {
             //   CallManagerView.EndCall();
            }
        }
    }
}
