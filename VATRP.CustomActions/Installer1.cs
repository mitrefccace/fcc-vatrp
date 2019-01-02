using System;
using System.IO;
using System.Collections;
using System.ComponentModel;


namespace VATRP.Uninstall
{
    [RunInstaller(true)]
    public partial class Installer1 : System.Configuration.Install.Installer
    {
        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Uninstall(IDictionary savedState)
        {
            try
            {
                base.Uninstall(savedState);
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                appData = Path.Combine(appData, "VATRP");
                Directory.Delete(appData, true);
            }
            catch (Exception ex)
            {
                // Exit silently
            }
        }
    }
}