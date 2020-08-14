using com.vtcsecure.ace.windows.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.CustomControls
{
    /// <summary>
    /// Interaction logic for LocationGPS.xaml
    /// </summary>
    public partial class LocationGPS : Window
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private LocationConfig locationConfig;
        public LocationGPS(LocationConfig location_config)
        {
            locationConfig = location_config;
            InitializeComponent();
        }

        public void UploadCoordinate_Click(object sender, RoutedEventArgs e)
        {
            string xCoord = XCoordTextBox.Text;
            string yCoord = YCoordTextBox.Text;
            string epsgNumber = EPSGTextBox.Text;
            double convertDouble;
            int convertInt;
            bool isDouble = double.TryParse(xCoord, out convertDouble);
            if (!isDouble)
            {
                not_numeric_message("X Coordinate");
                return;
            }
            isDouble = double.TryParse(yCoord, out convertDouble);
            if (!isDouble)
            {
                not_numeric_message("Y Coordinate");
                return;
            }
            bool isInt = int.TryParse(epsgNumber, out convertInt);
            if (!isInt)
            {
                not_numeric_message("EPSG");
                return;
            }


            string xmlContent = string.Format("<location-info><Point srsName=\"urn:ogc:def:crs:EPSG::{2}\"><pos>{0} {1}</pos></Point></location-info>", xCoord, yCoord, epsgNumber);
            locationConfig.SendXMLToServer(xmlContent);
        }

        private void not_numeric_message(string field)
        {
            string msg = String.Format("{0} is not numeric. Please set it to a number", field);
            string caption = "Upload failed";
            MessageBoxButton button = MessageBoxButton.OK;
            System.Windows.MessageBox.Show(msg, caption, button, MessageBoxImage.Error);
        }

        void LocationGPS_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }


        public void OnClose(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
