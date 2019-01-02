using com.vtcsecure.ace.windows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{
    /// <summary>
    /// Interaction logic for EmbeddedSelfView.xaml
    /// </summary>
    public partial class EmbeddedSelfView : UserControl
    {
        public bool ResetNativePreviewHandle { get; set; }

        public EmbeddedSelfView()
        {
            InitializeComponent();
        }

        public void OnVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SwitchPreviewPanel(Convert.ToBoolean(e.NewValue));
        }

        private void SwitchPreviewPanel(bool bOn)
        {
            var _linphone = ServiceManager.Instance.LinphoneService;
            if (_linphone == null) 
                return;
            if (!bOn)
            {
                if (ResetNativePreviewHandle)
                    _linphone.SetVideoPreviewWindowHandle(IntPtr.Zero, true);
                ctrlVideo.DrawCameraImage = false;
            }
            else
            {
                _linphone.SetPreviewVideoSizeByName("cif");
                ServiceManager.Instance.LinphoneService.SetVideoPreviewWindowHandle(ctrlVideo.GetVideoControlPtr);
                ctrlVideo.DrawCameraImage = true;
                ctrlVideo.Visibility = System.Windows.Visibility.Visible;
                

/*                HwndSource source = (HwndSource)HwndSource.FromVisual(ContentPanel);
                if (source != null)
                {
                    IntPtr hWnd = source.Handle;
                    if (hWnd != IntPtr.Zero)
                    {
                        _linphone.SetVideoPreviewWindowHandle(hWnd);
                    }

                }
                ResetNativePreviewHandle = true;
 * */
            }
        }

    }
}
