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

using com.vtcsecure.ace.windows.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
    /// Interaction logic for DeafHoHResourcesPanel.xaml
    /// </summary>
    public partial class DeafHoHResourcesPanel : BaseResourcePanel
    {
        private const string CDN_RESOURCE_URI = "http://cdn.vatrp.net/numbers.json";
        private List<ResourceInfo> resourceInfoList;

        public DeafHoHResourcesPanel()
        {
            InitializeComponent();
            List<ResourceInfo> resourceInfoList = new List<ResourceInfo>();

            Title = "Deaf/Hard of Hearing Resources";
            this.Loaded += DeafHoHResourcesPanel_Loaded;
        }

        private void DeafHoHResourcesPanel_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateListOfResources();
        }

        // load resources from http://cdn.vatrp.net/numbers.json
        private void PopulateListOfResources()
        {

            List<ResourceInfo> resourceList = LoadListOfResources();
            if (resourceList != null && resourceList.Count>0)
            {
                ResourceInfoListView.Items.Clear();
                foreach (ResourceInfo item in resourceList)
                {
                    ResourceInfoListView.Items.Add(item);
                }

            }         
            else if (ResourceInfoListView.Items.Count<1)
            {
                //TODO: Add default customer care number in case of no reasources found
                ResourceInfoListView.Items.Add(new ResourceInfo { name = "None Found", address = "" });
            }

        }

        private List<ResourceInfo> LoadListOfResources()
        {
            WebResponse response = null;
            try
            {
                // create a request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(CDN_RESOURCE_URI);
                request.KeepAlive = false;
                request.ContentLength = 0;
                request.Method = "GET";
                request.Timeout = 30000;

                response = request.GetResponse();
                String jsonResults = "";
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    jsonResults = sr.ReadToEnd();
                    sr.Close();
                }

                List<ResourceInfo> resourceList;
                try
                {
                    // deserialize json to ResourceInfo List
                    resourceList = JsonDeserializer.JsonDeserialize<List<ResourceInfo>>(jsonResults.ToString());
                    return resourceList;
                }
                catch (Exception ex)
                {
                    string message = "Failed to parse resource information. Details: " + ex.Message;
                    return new List<ResourceInfo>();
                }
            }
            catch (Exception ex)
            {
                string message = "Failed to get resource information. Details: " + ex.Message;
                return new List<ResourceInfo>();
            }
            finally 
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }
        }

        private void ResourceInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //var selectedItem = ResourceInfoListView.SelectedItem;
            //if (selectedItem != null)
            //{
            //    ResourceInfo resourceInfo = (ResourceInfo)selectedItem;
            //    Console.WriteLine("Resource Selected: Name=" + resourceInfo.name + " Address=" + resourceInfo.address);
            //   // OnCallResourceRequested(resourceInfo);
            //}
            //else
            //{
            //    Console.WriteLine("No selected resource available");
            //}
            //selectedItem = null;
            //ResourceInfoListView.SelectedItem = null;

         
        }

        private void ResourceInfoListView_TouchDown(object sender, TouchEventArgs e)
        {
            
        }

        private void ResourceInfoListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           
           // ResourceInfoListView.SelectedItem = null;
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null && item.IsSelected)
            {
               
                //Do your stuff
            }

           
        }

        private void ResourceInfoListView_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = ResourceInfoListView.SelectedItem;
            if (selectedItem != null)
            {
                ResourceInfo resourceInfo = (ResourceInfo)selectedItem;
                Console.WriteLine("Resource Selected: Name=" + resourceInfo.name + " Address=" + resourceInfo.address);
                OnCallResourceRequested(resourceInfo);
            }
            else
            {
                Console.WriteLine("No selected resource available");
            }
            selectedItem = null;
            ResourceInfoListView.SelectedItem = null;
        }

        //private void ResourceInfoListView_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    ResourceInfoListView.SelectedItem = null;
        //    return;
        //}

    }
}
