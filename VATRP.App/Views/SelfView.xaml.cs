﻿#region copyright
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using com.vtcsecure.ace.windows.Model;
using com.vtcsecure.ace.windows.Services;

namespace com.vtcsecure.ace.windows.Views
{
    /// <summary>
    /// Interaction logic for SelfView.xaml
    /// </summary>
    public partial class SelfView 
    {
        public bool ResetNativePreviewHandle { get; set; }
        public SelfView() : base(VATRPWindowType.SELF_VIEW)
        {
            InitializeComponent();
        }

        private void OnVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SwitchPreviewPanel(Convert.ToBoolean(e.NewValue));
        }

        private void SwitchPreviewPanel(bool bOn)
        {
            var _linphone = ServiceManager.Instance.LinphoneService;
            if (_linphone == null) 
                return;
            if (!bOn)
            {
                if (ResetNativePreviewHandle)
                    _linphone.SetVideoPreviewWindowHandle(IntPtr.Zero, true);
            }
            else
            {
                _linphone.SetPreviewVideoSizeByName("cif");
                var source = GetWindow(this);
                if (source != null)
                {
                    var wih = new WindowInteropHelper(source);
                    if (wih.Handle != IntPtr.Zero)
                    {
                        IntPtr hWnd = wih.EnsureHandle();

                        if (hWnd != IntPtr.Zero)
                        {
                            _linphone.SetVideoPreviewWindowHandle(hWnd);
                        }
                    }
                }
                ResetNativePreviewHandle = true;
            }
        }
    }
}
