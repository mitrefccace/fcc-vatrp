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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.Model
{
    public class VATRPWindow : Window
    {
        private readonly System.Timers.Timer moveDetectionTimer;
        public bool DestroyOnClosing { get; set; }
        public  VATRPWindowType WindowType { get; private set; }
        
        protected VATRPWindow(VATRPWindowType wndType)
        {
            IsActivated = false;
            WindowType = wndType;
            DestroyOnClosing = false;
            moveDetectionTimer = new System.Timers.Timer {AutoReset = false, Interval = 1000};
            moveDetectionTimer.Elapsed += OnMoveTimerElapsed;
        }
        private void OnMoveTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new EventHandler<System.Timers.ElapsedEventArgs>(this.OnMoveTimerElapsed), sender, new object[] { e });
                return;
            }

            SaveWindowLocation();
        }

        public bool IsActivated { get; private set; }

        protected void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (moveDetectionTimer.Enabled)
                moveDetectionTimer.Stop();
            moveDetectionTimer.Start();
        }

        protected void Window_Initialized(object sender, EventArgs e)
        {
            Configuration.ConfSection section = ConfSectionFromWindow(WindowType);

            var ws = WindowState.Normal;
            try
            {
                var wsText = ServiceManager.Instance.ConfigurationService.Get(section,
                Configuration.ConfEntry.WINDOW_STATE, "Normal");

                ws = (WindowState) Enum.Parse(typeof (WindowState), wsText, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WindowInitialized: Parse State. {0} {1} ", WindowType, ex.Message);
                ws = WindowState.Normal;
            }

            this.WindowState = ws;

            try
            {
                this.Left = Convert.ToInt32(ServiceManager.Instance.ConfigurationService.Get(section,
                    Configuration.ConfEntry.WINDOW_LEFT, GetWindowDefaultCoordinates(WindowType).X.ToString()));
            }
            catch (FormatException)
            {
                this.Left = GetWindowDefaultCoordinates(WindowType).X;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WindowInitialized: Parse Left. {0} {1} ", WindowType, ex.Message);
            }

            try
            {
                this.Top = Convert.ToInt32(ServiceManager.Instance.ConfigurationService.Get(section,
                    Configuration.ConfEntry.WINDOW_TOP, GetWindowDefaultCoordinates(WindowType).Y.ToString()));
            }
            catch (FormatException fe)
            {
                this.Top = GetWindowDefaultCoordinates(WindowType).Y;
                Debug.WriteLine("WindowInitialized: Parse Top. " + fe.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WindowInitialized: Parse Top. {0} {1} ", WindowType, ex.Message);
            }

            Size defaultSize = GetWindowDefaultDimensions(WindowType);
            try
            {
                this.Width = Convert.ToDouble(ServiceManager.Instance.ConfigurationService.Get(section,
                    Configuration.ConfEntry.WINDOW_WIDTH, defaultSize.Width.ToString()));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WindowInitialized: Parse Width. {0} {1} ", WindowType, ex.Message);
            }

            try
            {
                this.Height = Convert.ToDouble(ServiceManager.Instance.ConfigurationService.Get(section,
                    Configuration.ConfEntry.WINDOW_HEIGHT, defaultSize.Height.ToString()));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WindowInitialized: Parse Height. {0} {1} ", WindowType, ex.Message);
            }
        }


        protected void Window_Activated(object sender, EventArgs e)
        {
            IsActivated = true;
        }

        protected void Window_Closing(object sender, CancelEventArgs e)
        {
            if (DestroyOnClosing)
                return;
            if (!App.AllowDestroyWindows)
            {
                switch (WindowType)
                {
                    case VATRPWindowType.CALL_VIEW:
                    case VATRPWindowType.REMOTE_VIDEO_VIEW:
                    break;
                    default:
                        Hide();
                        break;
                }
                e.Cancel = true;
            }
        }

        protected void Window_StateChanged(object sender, EventArgs e)
        {
            Configuration.ConfSection section = ConfSectionFromWindow(WindowType);
            ServiceManager.Instance.ConfigurationService.Set(section,
                Configuration.ConfEntry.WINDOW_STATE, this.WindowState.ToString());

        }

        protected void Window_LocationChanged(object sender, EventArgs e)
        {
            // save location
            if (moveDetectionTimer.Enabled)
                moveDetectionTimer.Stop();
            moveDetectionTimer.Start();
        }

        protected void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (moveDetectionTimer.Enabled)
            {
                moveDetectionTimer.Stop();
            }
            SaveWindowLocation();
        }

        private void SaveWindowLocation()
        {
            Configuration.ConfSection section = ConfSectionFromWindow(WindowType);
            ServiceManager.Instance.ConfigurationService.Set(section,
                Configuration.ConfEntry.WINDOW_LEFT, this.Left.ToString());
            ServiceManager.Instance.ConfigurationService.Set(section,
                Configuration.ConfEntry.WINDOW_TOP, this.Top.ToString());
            ServiceManager.Instance.ConfigurationService.Set(section,
                Configuration.ConfEntry.WINDOW_WIDTH, this.ActualWidth.ToString());
            ServiceManager.Instance.ConfigurationService.Set(section,
                Configuration.ConfEntry.WINDOW_HEIGHT, this.ActualHeight.ToString());
        }

        private static Configuration.ConfSection ConfSectionFromWindow(VATRPWindowType wndType)
        {
            switch (wndType)
            {
                case VATRPWindowType.CALL_VIEW:
                    return Configuration.ConfSection.CALL_WINDOW;
                case VATRPWindowType.CONTACT_VIEW:
                    return Configuration.ConfSection.CONTACT_WINDOW;
                case VATRPWindowType.DIALPAD_VIEW:
                    return Configuration.ConfSection.DIALPAD_WINDOW;
                case VATRPWindowType.MAIN_VIEW:
                    return Configuration.ConfSection.MAIN_WINDOW;
                case VATRPWindowType.MESSAGE_VIEW:
                    return Configuration.ConfSection.MESSAGE_WINDOW;
                case VATRPWindowType.RECENTS_VIEW:
                    return Configuration.ConfSection.HISTORY_WINDOW;
                case VATRPWindowType.SELF_VIEW:
                    return Configuration.ConfSection.SELF_WINDOW;
                case VATRPWindowType.SETTINGS_VIEW:
                    return Configuration.ConfSection.SETTINGS_WINDOW;
                case VATRPWindowType.REMOTE_VIDEO_VIEW:
                    return Configuration.ConfSection.REMOTE_VIDEO_VIEW;
                case VATRPWindowType.KEYPAD_VIEW:
                    return Configuration.ConfSection.KEYPAD_WINDOW;
                case VATRPWindowType.CALL_INFO_VIEW:
                    return Configuration.ConfSection.CALLINFO_WINDOW;
                case VATRPWindowType.AUTHENTICATION_VIEW:
                    return Configuration.ConfSection.AUTHENTICATION_WINDOW;

                default:
                    throw new ArgumentOutOfRangeException("wndType");
            }
        }

        private static Point GetWindowDefaultCoordinates(VATRPWindowType wndType)
        {
            switch (wndType)
            {
                case VATRPWindowType.CALL_VIEW:
                    return new Point() {X = 100, Y = 100};
                case VATRPWindowType.CONTACT_VIEW:
                    return new Point() { X = 300, Y = 100 };
                case VATRPWindowType.DIALPAD_VIEW:
                    return new Point() { X = 100, Y = 100 };
                case VATRPWindowType.MAIN_VIEW:
                {
                    int top = (int)(SystemParameters.PrimaryScreenHeight - 720)/2;
                    if (top < 0)
                        top = 0;
                    return new Point() { X = 300, Y = top };
                }
                case VATRPWindowType.MESSAGE_VIEW:
                    return new Point() { X = 300, Y = 100 };
                case VATRPWindowType.RECENTS_VIEW:
                    return new Point() { X = 500, Y = 100 };
                case VATRPWindowType.SELF_VIEW:
                    return new Point() { X = 100, Y = 400 };
                case VATRPWindowType.SETTINGS_VIEW:
                    return new Point() { X = 100, Y = 400 };
                case VATRPWindowType.REMOTE_VIDEO_VIEW:
                    return new Point() { X = 100, Y = 400 };
                case VATRPWindowType.KEYPAD_VIEW:
                    return new Point() { X = 100, Y = 400 };
                case VATRPWindowType.CALL_INFO_VIEW:
                    return new Point() { X = 200, Y = 500 };
                case VATRPWindowType.AUTHENTICATION_VIEW:
                    return new Point() { X = 200, Y = 500 };
                default:
                    throw new ArgumentOutOfRangeException("wndType");
            }
        }

        private static Size GetWindowDefaultDimensions(VATRPWindowType wndType)
        {
            switch (wndType)
            {
                case VATRPWindowType.CALL_VIEW:
                    return new Size() {Width = 300, Height = 300};
                case VATRPWindowType.CONTACT_VIEW:
                    return new Size() {Width = 300, Height = 350};
                case VATRPWindowType.DIALPAD_VIEW:
                    return new Size() {Width = 400, Height = 450};
                case VATRPWindowType.MAIN_VIEW:
                    return new Size() {Width = 300, Height = 400};
                case VATRPWindowType.MESSAGE_VIEW:
                    return new Size() {Width = 700, Height = 400};
                case VATRPWindowType.RECENTS_VIEW:
                    return new Size() {Width = 300, Height = 500};
                case VATRPWindowType.SELF_VIEW:
                    return new Size() {Width = 300, Height = 300};
                case VATRPWindowType.SETTINGS_VIEW:
                    return new Size() {Width = 400, Height = 500};
                case VATRPWindowType.REMOTE_VIDEO_VIEW:
                    return new Size() {Width = 300, Height = 300};
                case VATRPWindowType.KEYPAD_VIEW:
                    return new Size() {Width = 100, Height = 400};
                case VATRPWindowType.CALL_INFO_VIEW:
                    return new Size() {Width = 350, Height = 500};
                case VATRPWindowType.AUTHENTICATION_VIEW:
                    return new Size() { Width = 200, Height = 100 };
                default:
                    return new Size() {Width = 300, Height = 400};
            }
        }

    }
}
