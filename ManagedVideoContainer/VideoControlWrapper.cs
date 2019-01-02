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
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using VATRP.Linphone.VideoWrapper.Properties;
using Size = System.Drawing.Size;

namespace VATRP.Linphone.VideoWrapper
{
    class VideoControlWrapper : Control
    {
        private Renderable renderable;
        private System.Drawing.Bitmap _cameraBitmap;
        public VideoControlWrapper()
        {
            ClientSize = new System.Drawing.Size(1, 1);

            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(@"pack://application:,,,/VATRP.Linphone.VideoWrapper;component/Resources/camera_mute.png", UriKind.Absolute);
            src.EndInit();
            _cameraBitmap = BitmapSourceToBitmap(src);
        }

        /// <summary>
        /// For static content, use traditional windows paint approach to signaling rendering
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            RenderFrame(e);
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Size s = new Size();
            s.Width = Width;
            s.Height = Height;
            ClientSize = s;
        }

        internal Renderable RenderContent
        {
            get { return renderable; }
            set
            {
                renderable = value;
                RenderContent.OnPaint += new EventHandler(RepaintContent);
            }
        }

        internal bool DrawCameraImage { get; set; }
		
        void RepaintContent(object sender, EventArgs e)
        {
            OnPaint(null);
        }

        private void RenderFrame(PaintEventArgs e)
        {
            if (DrawCameraImage)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.Black), 0, 0, ClientSize.Width, ClientSize.Height);

                int imageSize = 300;
                Rectangle srcRect = new Rectangle(0, 0, _cameraBitmap.Width, _cameraBitmap.Height);
                Rectangle destRect = new Rectangle((ClientSize.Width - imageSize) / 2, (ClientSize.Height - imageSize) / 2,
                    imageSize, imageSize);

                if (_cameraBitmap != null)
                    e.Graphics.DrawImage(_cameraBitmap,destRect, srcRect
                        , GraphicsUnit.Pixel);
            }
        }

        private System.Drawing.Bitmap BitmapSourceToBitmap(BitmapSource srcImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(srcImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
    }
}
