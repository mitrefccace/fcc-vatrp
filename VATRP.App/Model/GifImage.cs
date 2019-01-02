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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows;

namespace com.vtcsecure.ace.windows.CustomControls
{
    class GifImage : Image
    {
        private bool _isInitialized;
        private GifBitmapDecoder _gifDecoder;
        private Int32Animation _animation;

        public int FrameIndex
        {
            get { return (int)GetValue(FrameIndexProperty); }
            set { SetValue(FrameIndexProperty, value); }
        }

        private void Initialize()
        {
            _gifDecoder = new GifBitmapDecoder(new Uri("pack://application:,,," + this.GifSource), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            _animation = new Int32Animation(0, _gifDecoder.Frames.Count - 1, new Duration(new TimeSpan(0, 0, 0, _gifDecoder.Frames.Count / 10, (int)((_gifDecoder.Frames.Count / 10.0 - _gifDecoder.Frames.Count / 10) * 5000))));
            _animation.RepeatBehavior = RepeatBehavior.Forever;
            this.Source = _gifDecoder.Frames[0];

            _isInitialized = true;
        }

        static GifImage()
        {
            VisibilityProperty.OverrideMetadata(typeof(GifImage),
                new FrameworkPropertyMetadata(VisibilityPropertyChanged));
        }

        private static void VisibilityPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Visibility)e.NewValue == Visibility.Visible)
            {
                ((GifImage)sender).StartAnimation();
            }
            else
            {
                ((GifImage)sender).StopAnimation();
            }
        }

        public static readonly DependencyProperty FrameIndexProperty =
            DependencyProperty.Register("FrameIndex", typeof(int), typeof(GifImage), new UIPropertyMetadata(0, new PropertyChangedCallback(ChangingFrameIndex)));

        static void ChangingFrameIndex(DependencyObject obj, DependencyPropertyChangedEventArgs ev)
        {
            var gifImage = obj as GifImage;
            gifImage.Source = gifImage._gifDecoder.Frames[(int)ev.NewValue];
        }

        /// <summary>
        /// Defines whether the animation starts on it's own
        /// </summary>
        public bool AutoStart
        {
            get { return (bool)GetValue(AutoStartProperty); }
            set { SetValue(AutoStartProperty, value); }
        }

        public static readonly DependencyProperty AutoStartProperty =
            DependencyProperty.Register("AutoStart", typeof(bool), typeof(GifImage), new UIPropertyMetadata(false, AutoStartPropertyChanged));

        private static void AutoStartPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                (sender as GifImage).StartAnimation();
        }

        public string GifSource
        {
            get { return (string)GetValue(GifSourceProperty); }
            set { SetValue(GifSourceProperty, value); }
        }

        public static readonly DependencyProperty GifSourceProperty =
            DependencyProperty.Register("GifSource", typeof(string), typeof(GifImage), new UIPropertyMetadata(string.Empty, GifSourcePropertyChanged));

        private static void GifSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as GifImage).Initialize();
        }

        /// <summary>
        /// Starts the animation
        /// </summary>
        public void StartAnimation()
        {
            if (!_isInitialized)
                this.Initialize();

            BeginAnimation(FrameIndexProperty, _animation);
        }

        /// <summary>
        /// Stops the animation
        /// </summary>
        public void StopAnimation()
        {
            BeginAnimation(FrameIndexProperty, null);
        }
    }
}
