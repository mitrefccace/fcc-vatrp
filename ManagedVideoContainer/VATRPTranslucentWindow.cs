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
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace VATRP.Linphone.VideoWrapper
{
    public class VATRPTranslucentWindow
    {
        private Window _window;
        private Window _parent;
        private VATRPOverlay _container;
        double windowTopMargin;
        double windowLeftMargin;
        bool popUpBlock = false; //this boolean prevents the application from take priority when opening another appilcation. (6/1/2019 :MT)

        public VATRPTranslucentWindow(VATRPOverlay decorator)
        {
            _container = decorator;
            _window = new Window();
            
            //Make the window itself transparent, with no style.
            _window.Background = Brushes.Transparent;
            _window.AllowsTransparency = true;
            _window.WindowStyle = WindowStyle.None;

            //Hide from taskbar until it becomes a child
            _window.ShowInTaskbar = false;

            //HACK: This window and it's child controls should never have focus, as window styling of an invisible window 
            //will confuse user.
            _window.Focusable = false;
            _window.ShowActivated = false;
            _window.PreviewMouseDown += OnPreviewMouseDown;
            _window.IsVisibleChanged += OnVisibilityChanged;

        }

        private void OnVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool) e.NewValue != ShowWindow)
            {
                if (_window != null)
                {
                    try
                    {
                        var wih = new System.Windows.Interop.WindowInteropHelper(_window);
                        if (wih.Handle != IntPtr.Zero)
                        {
                            IntPtr hWnd = wih.EnsureHandle();

                            if (hWnd != IntPtr.Zero)
                            {
                                if (ShowWindow)
                                    _window.Show();
                                else
                                {
                                    _window.Hide();
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        
                    }
                }
            }
        }

        void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_parent != null) 
                _parent.Focus();
        }

        public double WindowLeftMargin
        {
            get { return windowLeftMargin; }
            set { windowLeftMargin = value; }
        }
        public double WindowTopMargin
        {
            get { return windowTopMargin; }
            set { windowTopMargin = value; }
        }

        public int OverlayWidth { get; set; }
        public int OverlayHeight { get; set; }
        public bool ShowWindow { get; set; }

        public Window TransparentWindow
        {
            get
            {
                return _window;
            }
        }
        private void parent_LocationChanged(object sender, EventArgs e)
        {
            UpdateWindow();
        }

        private Window GetParentWindow(DependencyObject o)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(o);
            if (parent == null)
            {
                FrameworkElement fe = o as FrameworkElement;
                if (fe != null)
                {
                    if (fe is Window)
                    {
                        return fe as Window;
                    }
                    else if (fe.Parent != null)
                    {
                        return GetParentWindow(fe.Parent);
                    }
                }
                throw new ApplicationException("A window parent could not be found for " + o.ToString());
            }
            else
            {
                return GetParentWindow(parent);
            }
        }

        internal void Refresh()
        {
            try
            {
                if (_window.Visibility != Visibility.Visible && ShowWindow)
                {
                    UpdateWindow();
                    _window.Show();
                    if (_parent == null)
                    {
                        _parent = GetParentWindow(_container);
                        _window.Owner = _parent;
                        _parent.LocationChanged += new EventHandler(parent_LocationChanged);
                        // The popUpBlock prevents the app from popping up in front of other windows. On Line Below, popUpBlock is set to false to allow
                        // the system to set up and open. After that the popUpBlock is set to true which prevents in popping to front. 
                        popUpBlock = false;
                    }
                }
                else
                {
                    if (!ShowWindow && _window.Visibility == Visibility.Visible)
                    {
                        _window.Hide();
                        if (_parent != null && !popUpBlock) { 
                            popUpBlock = true; 
                            _parent.Activate(); 
                        }
                    }
                }
            }
            catch (Exception )
            {
                
            }
        }

        internal void UpdateWindow()
        {
            try
            {
                Window parent = GetParentWindow(_container);

                FrameworkElement windowContent = ((parent.Content) as FrameworkElement);
                if (windowContent != null)
                {
                    windowLeftMargin = WindowLeftMargin == 0
                        ? (parent.ActualWidth - windowContent.ActualWidth)/2
                        : WindowLeftMargin;
                    windowTopMargin = (WindowTopMargin == 0)
                        ? (parent.ActualHeight - windowContent.ActualHeight)
                        : WindowTopMargin;

                    _window.Left = parent.Left + windowLeftMargin;
                    _window.Top = parent.Top + windowTopMargin;
                    _window.Width = OverlayWidth;
                    _window.Height = OverlayHeight;
                }
               
                if (ShowWindow)
                {
                    _window.Show();
                    popUpBlock = true; // Prevents the app from popping in front over windows when UpdateWindow runs
                }
                else
                {
                    _window.Hide();
                }
            }
            catch (Exception)
            {
                
            }
        }
    }
}
