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
using System.Data.Linq.Mapping;
using System.IO;
using System.Threading;
using VATRP.Core.Model.Utils;
using VATRP.Core.Services;

namespace VATRP.Core.Model
{
    public class VATRPServiceProvider : INotifyPropertyChanged, IComparable<VATRPServiceProvider>
    {
        #region Members

        private string _address;
        private string _iconPath;
        private string _imagePath;
        private string _label;
        private string _imageUri;
        private string _iconUri;
        #endregion

        #region Properties

        public string Label
        {
            get { return _label; }
            set
            {
                _label = value;
                this.OnPropertyChanged("Label");
            }
        }

        public string ImagePath
        {
            get { return _imagePath; }
            set
            {
                _imagePath = value;
                this.OnPropertyChanged("ImagePath");
            }
        }

        public string IconPath
        {
            get { return _iconPath; }
            set
            {
                _iconPath = value;
                this.OnPropertyChanged("IconPath");
            }
        }

        public string IconURI
        {
            get { return _iconUri; }
            set
            {
                _iconUri = value;
                this.OnPropertyChanged("IconURI");
            }
        }

        public string ImageURI
        {
            get { return _imageUri; }
            set
            {
                _imageUri = value;
                this.OnPropertyChanged("ImageURI");
            }
        }

        public string Address
        {
            get { return _address; }
            set
            {
                _address = value;
                this.OnPropertyChanged("Address");
            }
        }

        #endregion

        #region Methods

        /* 
         * @brief Constructor 
         * 
         * Not sure what the point of _nologo is but
         * the logic surrouding it will interfere with 
         * populating / loading a cached Custom provider 
         * from the local providers.xml file so adjusting 
         * it to null since that's what we usually set it
         * to anyways.
         * 
         * @return VATRP Service Provider
         */
        public VATRPServiceProvider()
        {
            _address = null;
            _label = "_nologo";
            _imagePath = null;
            _iconPath = null;
        }

        public void LoadImage(string imgCachePath, bool icon)
        {
            if (string.IsNullOrEmpty(icon ? IconURI : ImageURI))
                return;
            var downloadHelper = new HttpDownloadHelper();
            downloadHelper.DownloadCompleted += OnDownloadCompleted;
            try
            {
                if (!Directory.Exists(imgCachePath))
                    Directory.CreateDirectory(imgCachePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create directory: " + imgCachePath);
                return;
            }

            var path = Path.Combine(imgCachePath, string.Format("{0}_{1}.png", _label, (icon ? "icon" : "img")));
            if (!icon)
                ImagePath = path;
            else
                IconPath = path;

            if (!File.Exists(icon ? IconPath : ImagePath))
            {
                ThreadPool.QueueUserWorkItem((x) => downloadHelper.DownloadImage(icon ? IconURI : ImageURI, path));
            }
        }

        private void OnDownloadCompleted(object sender, Events.HttpDownloadEventArgs e)
        {
            Console.WriteLine("Download completed: " + e.Succeeded + " " + e.URI);
        }

        #endregion

        #region INotifyPropertyChanged Interface

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region IComparable Interface

        public int CompareTo(VATRPServiceProvider other)
        {
            if (other == null)
            {
                return -1;
            }
            return this.Label.CompareTo(other.Label);
        }

        #endregion

    }
}
