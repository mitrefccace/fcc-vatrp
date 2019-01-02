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
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using log4net;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Model;
using HockeyApp;
using System.Threading;
using com.vtcsecure.ace.windows.Views;

using com.vtcsecure.ace.windows.Utilities;
using Newtonsoft.Json;
using VATRP.Core.Model.Utils;
using VATRP.Core;
using System.Net;
using System.ComponentModel;
using System.Threading.Tasks;

namespace com.vtcsecure.ace.windows
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App 
    {
        #region Members

        private static readonly log4net.ILog _log = LogManager.GetLogger(typeof (App));
        private static bool _allowDestroyWindows = false;
        private Mutex mutex;
        #endregion

        #region Properties
        public static bool AllowDestroyWindows
        {
            get { return _allowDestroyWindows; }
            set { _allowDestroyWindows = value; }
        }
        public static VATRPAccount CurrentAccount { get; set; }
        public static bool CanMakeVideoCall { get; set; }

        internal static bool AppClosing { get; set; }
        #endregion


        AppVersion[] appVersionArray;

        public App()
        {
            Mutex testmutex;
            Mutex.TryOpenExisting("Global\\84D29A79-09A3-4CBF-A12A-B15CEF971672", out testmutex);
            if (testmutex != null) //*** If testmutex is null means application instance is not running.
            {
                MessageBox.Show("Instance already running");
                Environment.Exit(0);
                return;
            }
            mutex = new Mutex(true, "Global\\84D29A79-09A3-4CBF-A12A-B15CEF971672");
        }

        protected override async void OnStartup(StartupEventArgs e)
        {

            //***********************************************************************************************************************************
            // A type that derives from Application may override OnStartup. 
            // The overridden method must call OnStartup in the base class if the Startup event needs to be raised.
            //************************************************************************************************************************************
            base.OnStartup(e);
            //CheckNewVersionAvailable();

            //  Short curcuit HockeyApp 4/12/2017 MITRE-fjr
            return;
            //main configuration of HockeySDK
            HockeyClient.Current.Configure(HOCKEYAPP_ID);
                //.UseCustomResourceManager(HockeyApp.ResourceManager) //register your own resourcemanager to override HockeySDK i18n strings
                //.RegisterCustomUnhandledExceptionLogic((eArgs) => { /* do something here */ }) // define a callback that is called after unhandled exception
                //.RegisterCustomUnobserveredTaskExceptionLogic((eArgs) => { /* do something here */ }) // define a callback that is called after unobserved task exception
                //.RegisterCustomDispatcherUnhandledExceptionLogic((args) => { }) // define a callback that is called after dispatcher unhandled exception
                //.SetApiDomain("https://your.hockeyapp.server")
                //.SetContactInfo("John Smith", "email@example.com");

            //optional should only used in debug builds. register an event-handler to get exceptions in HockeySDK code that are "swallowed" (like problems writing crashlogs etc.)
#if DEBUG
            ((HockeyClient) HockeyClient.Current).OnHockeySDKInternalException += (sender, args) =>
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            };
#endif
            try
            {

                //send crashes to the HockeyApp server
               await HockeyClient.Current.SendCrashesAsync();
            }
            catch (Exception eArgs)
            {
                if (_log != null)
                    _log.Error("HockeyApp SendCrashesAsync exception: " + eArgs.ToString());
            }

            //check for updates on the HockeyApp server
            await HockeyClient.Current.CheckForUpdatesAsync(true, () =>
            {
                if (Application.Current.MainWindow != null)
                {
                    Application.Current.MainWindow.Close();
                }
                return true;
            });
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {

            //********************************************************************************************************************
            //      A type that derives from Application may override OnStartup. 
            //      The overridden method must call OnStartup in the base class if the Startup event needs to be raised.
            //********************************************************************************************************************

            _log.Info("====================================================");
            _log.Info(String.Format("============== Starting VATRP v{0} =============",
                System.Reflection.Assembly.GetEntryAssembly().GetName().Version));
            var currentDirectory = System.IO.Directory.GetCurrentDirectory();
            try
            {
                var appDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                if (string.IsNullOrEmpty(appDirectory))
                {
                    MessageBox.Show("Current directory is null", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown(1);
                    return;
                }

                if (currentDirectory != appDirectory )
                {
                    try
                    {
                        System.IO.Directory.SetCurrentDirectory(appDirectory);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to change application directory" + Environment.NewLine + ex.Message, "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.Shutdown(1);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get application directory" + Environment.NewLine + ex.Message, "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(1);
            }

            string linphoneLibraryVersion = VATRP.LinphoneWrapper.LinphoneAPI.linphone_core_get_version_asString();
            _log.Info(String.Format("======= LinphoneLib Version v{0} =======",
                linphoneLibraryVersion));

            _log.Info("====================================================");

            CurrentAccount = null;
            AppDomain.CurrentDomain.SetData("DataDirectory",
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

            var culture = new CultureInfo("en-US");
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

            if (!ServiceManager.Instance.Initialize())
            {
                MessageBox.Show("Failed to initialize service manager");
                this.Shutdown();
            }

            ServiceManager.Instance.Start();
            var mainWnd = new MainWindow();
            this.MainWindow = mainWnd;

            //AGW - Bypass legal release always
            ServiceManager.Instance.ConfigurationService.Set(VATRP.Core.Model.Configuration.ConfSection.GENERAL,
                VATRP.Core.Model.Configuration.ConfEntry.SHOW_LEGAL_RELEASE, false);

            //VATRP.Core.Model
            if (ServiceManager.Instance.ConfigurationService.Get(VATRP.Core.Model.Configuration.ConfSection.GENERAL,
                VATRP.Core.Model.Configuration.ConfEntry.SHOW_LEGAL_RELEASE, true))
            {
                LegalReleaseWindow lrWnd = new LegalReleaseWindow();
                var dlgResult = lrWnd.ShowDialog();
                if (dlgResult == null || (bool)!dlgResult)
                {
                    ServiceManager.Instance.Stop();prop:
                    this.Shutdown();
                    return;
                }
                else
                {
                    ServiceManager.Instance.ConfigurationService.Set(VATRP.Core.Model.Configuration.ConfSection.GENERAL,
                       VATRP.Core.Model.Configuration.ConfEntry.SHOW_LEGAL_RELEASE, false);
                    ServiceManager.Instance.ConfigurationService.SaveConfig();
                }
            }

            mainWnd.InitializeMainWindow();
            mainWnd.Show();
            //CheckNewVersionAvailable();

            // This call as is uses assembly version information which means i cannot change this without commenting this method out
            //try
            //{
            //    Task task = Task.Run((Action)CheckNewVersionAvailable);
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.Message);
            //}

            //var result = AsyncContext.RunTask(CheckNewVersionAvailable).Result;
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            //************************************************************************************************************************
            // This method will called when user quit the application. "On Application Exit"
            //************************************************************************************************************************
            try
            {

           
            mutex.Dispose();
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;

            
            string appVersion = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);

            if (appVersionArray != null &&
                    appVersion != appVersionArray[0].Version)
            {

                var appDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);  
                long length = new System.IO.FileInfo(appDirectory + "\\Setup_" + appVersionArray[0].Version.ToString() + ".exe").Length;
                if (length  != appVersionArray[0].Size)
                {
                   // DownloadInstaller(appVersionArray[0].Path, appVersionArray[0].Version);
                }
                else
                {
                    //NEW VERSION ALREADY DOWNLOADED and ONLY NEED TO INSTALL

                    if (MessageBox.Show("A new version of the VATRP is available. Do you wish to install? You would be required to restart your application.", "VATRP", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        // App_OnExit(null, null);
                        InstallBuild();
                        System.Environment.Exit(1);

                    }
                }
            }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("App_OnExit: " + ex.Message);
                throw;
            }

            ServiceManager.Instance.LinphoneService.AddDBPassword();
            ServiceManager.Instance.LinphoneService.AddCallHistoryDBPassword();
            ServiceManager.Instance.LinphoneService.AddChatHistoryDBPassword();
        }

        private void InstallBuild()
        {
            var appDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);  

            if (System.IO.File.Exists(appDirectory + "\\Setup_" + appVersionArray[0].Version.ToString() + ".exe"))
            {
                Process process = Process.Start(appDirectory + "\\Setup_" + appVersionArray[0].Version.ToString() + ".exe");
                int id = process.Id;
                Process tempProc = Process.GetProcessById(id);
                //tempProc.Visible = false;
                // tempProc.WaitForExit();

                System.Environment.Exit(1);
                //tempProc.Visible = true;
            }
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (_log != null)
                _log.Error("Not handled exception: " + e.Exception.ToString() + "\n" + e.Exception.StackTrace);
        }

        private void CheckNewVersionAvailable()
        {
            try
            {
                            
            //  2/20/2017 fjr Added HTTPS support
            string response = System.Configuration.ConfigurationManager.AppSettings["useHTTPs"] == "true" ? Utilities.JsonWebRequest.MakeJsonWebRequest("https://" + ServiceManager.CDN_DOMAIN_PATH + "/version.json") : Utilities.JsonWebRequest.MakeJsonWebRequest("http://" + ServiceManager.CDN_DOMAIN_PATH + "/version.json");
            Debug.Print(response);

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;

            string appVersion= string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
            appVersionArray = JsonConvert.DeserializeObject<AppVersion[]>(response);
            var appDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (appVersion != appVersionArray[0].Version)
            {
                if (!System.IO.File.Exists(appDirectory + "\\Setup_" + appVersionArray[0].Version.ToString() + ".exe"))
                {
                    DownloadInstaller(appVersionArray[0].Path, appVersionArray[0].Version);
                }
                else
                {
                    long length = new System.IO.FileInfo(appDirectory + "\\Setup_" + appVersionArray[0].Version.ToString() + ".exe").Length;
                    if (length != appVersionArray[0].Size)
                    {
                        DownloadInstaller(appVersionArray[0].Path, appVersionArray[0].Version);
                    }
                    else
                    {
                        if (MessageBox.Show("A new version of the VATRP is available. Do you wish to install? You would be required to restart your application.", "VATRP", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                           // App_OnExit(null, null);
                            InstallBuild();
                            //System.Environment.Exit(1);

                        }
                    }
                }
            }
            else
            {
                if (System.IO.File.Exists(appDirectory + "\\Setup_" + appVersionArray[0].Version.ToString() + ".exe"))
                {
                    System.IO.File.Delete(appDirectory + "\\Setup_" + appVersionArray[0].Version.ToString() + ".exe");
                }
               
            }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Check New Ver: " + ex.Message);
                throw;
            }
        }

        private void DownloadInstaller(string path, string version){

            try
            {
                var appDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                System.Net.ServicePointManager.Expect100Continue = true;
                WebClient downloader = new WebClient();
                downloader.DownloadFileCompleted += new AsyncCompletedEventHandler(wc_DownloadFileCompleted);
                downloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                downloader.DownloadFileAsync(new Uri(path), appDirectory + "\\Setup_" + version + ".exe");
            }
            catch (Exception e)
            {
                MessageBox.Show("DownloadInstaller" + e.Message.ToString());
                throw;
            }
            
            
            //downloader.DownloadFileAsync(new Uri(path), "G:\\Setup_"+ version +".exe");
            //downloader.DownloadFileAsync(new Uri("http://192.168.5.132/mit/setup.exe"), "G:\\Setup_" + version + ".exe");
           // downloader.DownloadFileAsync(new Uri(path), System.IO.Path.GetTempPath() + "\\" + System.IO.Path.GetFileName(path));
            //var downloadHelper = new HttpDownloadHelper();
           
            //downloadHelper.DownloadCompleted += OnDownloadCompleted;
            //ThreadPool.QueueUserWorkItem((x) => downloadHelper.DownloadImage(path, "G:\\ABC1.exe"));

        }

        void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e) {

            if (e.Error != null)
            {
                Console.WriteLine("Completed" + e.Error );
                CheckNewVersionAvailable();
                return;
            }
            else
            {
                if (MessageBox.Show("A new version of the VATRP is available. Do you wish to install? You would be required to restart your application.", "VATRP", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {

                    InstallBuild();
                    System.Environment.Exit(1);

                    //App_OnExit(null, null);

                }
                Console.WriteLine("Completed");
            }
            
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.WriteLine("Download %:" + e.ProgressPercentage);
        }

        private void OnDownloadCompleted(object sender, VATRP.Core.Events.HttpDownloadEventArgs e)
        {
            Console.WriteLine("Download completed: " + e.Succeeded + " " + e.URI);
        }

    }

    public class AppVersion
    {
        public string Version { get; set; }
        public string Date { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
    }
}
