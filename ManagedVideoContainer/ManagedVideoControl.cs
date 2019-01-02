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
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Input;

namespace VATRP.Linphone.VideoWrapper
{
    public class ManagedVideoControl : WindowsFormsHost
    {
        private Point lastMouseCoordinate;

        public ManagedVideoControl()
        {
            lastMouseCoordinate = new Point(0,0);
            Child = new VideoControlWrapper();
            Child.MouseEnter += (sender, args) =>
            {
                var e = new MouseEventArgs(Mouse.PrimaryDevice, (int) DateTime.Now.Ticks)
                {
                    RoutedEvent = Mouse.MouseEnterEvent
                };
                RaiseEvent(e);
            };

            Child.MouseLeave += delegate(object sender, EventArgs args)
            {
                MouseEventArgs e = new MouseEventArgs(Mouse.PrimaryDevice, (int)DateTime.Now.Ticks)
                {
                    RoutedEvent = Mouse.MouseLeaveEvent
                };
                RaiseEvent(e);
            };

            Child.MouseMove += (sender, args) =>
            {
                if (lastMouseCoordinate.X == args.X && lastMouseCoordinate.Y == args.Y)
                    return;
                lastMouseCoordinate.X = args.X;
                lastMouseCoordinate.Y = args.Y;

                MouseEventArgs e = new MouseEventArgs(Mouse.PrimaryDevice, (int)DateTime.Now.Ticks)
                {
                    RoutedEvent = Mouse.MouseMoveEvent,
                };
                RaiseEvent(e);
            };

            Child.MouseDown += delegate(object sender, System.Windows.Forms.MouseEventArgs args)
            {
                MouseButtonEventArgs e = new MouseButtonEventArgs(Mouse.PrimaryDevice, (int)DateTime.Now.Ticks,
                   MouseButton.Left)
                {
                    RoutedEvent = Mouse.MouseDownEvent
                };
                RaiseEvent(e);
            };
        }

        public Renderable RenderContent
        {
            get { return ((VideoControlWrapper)Child).RenderContent; }
            set { ((VideoControlWrapper)Child).RenderContent = value; }
        }

        public bool DrawCameraImage
        {
            get { return ((VideoControlWrapper)Child).DrawCameraImage; }
            set
            {
                ((VideoControlWrapper)Child).DrawCameraImage = value; 
                ((VideoControlWrapper)Child).Refresh();
            }
        }

        public IntPtr GetVideoControlPtr
        {
            get
            {
                return Child.Handle;
            }
        }
    }
}
