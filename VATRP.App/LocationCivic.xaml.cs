using com.vtcsecure.ace.windows.CustomControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace com.vtcsecure.ace.windows
{
    /// <summary>
    /// Interaction logic for LocationCivic.xaml
    /// </summary>
    public partial class LocationCivic : Window
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        LocationConfig locationConfig;
        public LocationCivic(LocationConfig location_config)
        {
            locationConfig = location_config;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }

        public void OnClose(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        public void UploadCoordinate_Click(object sender, RoutedEventArgs e)
        {
            string formatStr = "<location-info><civicAddress xmlns=\"urn:ietf:params:xml:ns:pidf:geopriv10:civicAddr\"><country>US</country><A1>{0}</A1><A3>{1}</A3><RD>{2}</RD><STS>{3}</STS><HNO>{4}</HNO><PC>{5}</PC></civicAddress></location-info>";
            string xmlContent = string.Format(formatStr, StateTextBox.Text, CityTextBox.Text, StreetNameTextBox.Text, StreetTypeTextBox.Text, StreetNumberTextBox.Text,ZIPTextBox.Text);
            locationConfig.SendXMLToServer(xmlContent);
        }
    }
}
