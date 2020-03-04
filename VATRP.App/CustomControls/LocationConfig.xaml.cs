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
    /// Interaction logic for LocationConfig.xaml
    /// </summary>
    public partial class LocationConfig : Window
    {
        LocationGPS _locationGPSWindow;
        LocationCivic _locationCivicWindow;

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public string GeolocationPostURI;
        public LocationConfig()
        {
            GeolocationPostURI = "";
            _locationGPSWindow = new LocationGPS(this);
            _locationCivicWindow = new LocationCivic(this);
            InitializeComponent();
        }

        public void GPS_Click(object sender, RoutedEventArgs e)
        {
            _locationGPSWindow.Show();
        }

        public void Civic_Click(object sender, RoutedEventArgs e)
        {
            _locationCivicWindow.Show();
        }

        public void FromFile_Click(object sender, RoutedEventArgs e)
        {
            string xmlContents = "";
            try
            {
                var openDlg = new OpenFileDialog()
                {
                    CheckFileExists = true,
                    CheckPathExists = true,
                    Filter = "Xml files (*.xml)|*.xml",
                    FilterIndex = 0,
                    ShowReadOnly = false,
                };

                if (openDlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;

                using (StreamReader sr = new StreamReader(openDlg.FileName))
                {
                    xmlContents = sr.ReadToEnd();
                    sr.Close();
                }
                SendXMLToServer(xmlContents);
            }
            catch (Exception ex)
            {
                string caption = "Load failed";
                MessageBoxButton button = MessageBoxButton.OK;
                System.Windows.MessageBox.Show(ex.Message, caption, button, MessageBoxImage.Error);
                return;
            }
        }

        private string getGeolocationURI()
        {
            return GeolocationURI.Text;
        }
        public void SendXMLToServer(string xmlContents)
        {
            VATRPCredential credentials = GetAuthentication();
            Task.Run(async () =>
            {
                try
                {
                    await PostLocationToServer(xmlContents, credentials, GeolocationPostURI);
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    await Dispatcher.BeginInvoke((Action)(() => Mouse.OverrideCursor = null));
                }
            });
        }

        private VATRP.Core.Model.VATRPCredential GetAuthentication()
        {
            AuthenticationView _authForm = new AuthenticationView();
            _authForm.ShowDialog();
            if (!_authForm.Proceed) { return null; }
            VATRPCredential tempCred = new VATRPCredential();
            tempCred.username = _authForm.Username;
            tempCred.password = _authForm.Password;
            return tempCred;
        }

        private async Task PostLocationToServer(string location, VATRPCredential credentials, string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                string msg = "No geolocation uri is set";
                string caption = "Load failed";
                MessageBoxButton button = MessageBoxButton.OK;
                System.Windows.MessageBox.Show(msg, caption, button, MessageBoxImage.Error);
            }
            try
            {
                await Utilities.JsonWebRequest.MakeXmlWebPostAuthenticatedAsync(uri, credentials, location);
            }
            catch (Utilities.JsonException ex)
            {
                string msg = String.Format("Failed to post location file to the web server: {0}.", uri);
                string caption = "Load failed";
                MessageBoxButton button = MessageBoxButton.OK;
                System.Windows.MessageBox.Show(msg, caption, button, MessageBoxImage.Error);
            }
        }

        void LocationConfig_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }


        public void OnClose(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void GeolocationURI_LostFocus(object sender, RoutedEventArgs e)
        {
            GeolocationPostURI = GeolocationURI.Text; 
        }
    }
}
