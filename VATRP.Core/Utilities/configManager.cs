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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using log4net;

namespace VATRP.Core.Model.Utils
{
    public class ConfigManager
    {
        private ILog LOG;
        private string _defaultFactoryConfigPath;
        private string _factoryConfigPath;
        private List<string> _emergencyNumbers;
        private string _customGeolocation;

        public List<string> EmergencyNumbers
        {
            get { return _emergencyNumbers; }
        }

        /* This CustomGeolocation field is no longer in use because we have moved to a new way for providing emergency
         * geolocation data. We are now supplying a URI to an http server that will return a valid geolocation XML card.
         * For more please see LinphoneService::MakeCall and AccountSettings.xaml.cs::OnGeolocationChanged. */
        public String CustomGeolocation
        {
            get { return _customGeolocation; }
        }

        public ConfigManager(string defaultPath, string factoryPath)
        {
            LOG = LogManager.GetLogger(typeof(ConfigManager));
            _defaultFactoryConfigPath = defaultPath;
            _factoryConfigPath = factoryPath;
            _emergencyNumbers = new List<String>();
            _customGeolocation = "";
        }

        public string FactoryConfigPath
        {
            get { return _factoryConfigPath; }
        }

        #region Methods
       
        /*
         * @brief File builder for (.cfg) linphone factory configuration 
         * 
         * If the file does not exist in VATRP, then it will be created
         * 
         * @return void
         */ 
        private void BuildFactoryFile()
        {
            try
            {
                File.Create(_defaultFactoryConfigPath).Close();
                using (StreamWriter sw = File.CreateText(_defaultFactoryConfigPath))
                {
                    sw.WriteLine("[sip]\ncontact=<contact>\n");
                }
            }
            catch (Exception e)
            {
                LOG.Debug(e.ToString());
            }
        }

        /// <summary>
        /// File modifier for linphone's factory configuration 
        /// </summary>
        /// <remarks>
        /// This dynamically sets the [sip] section contact key for the
        /// username and proxy host which just signed in. 
        ///
        /// This is then passed into the linphone_create_new method and
        /// is used to set the from domain and contact address in the
        /// outbound udp subscribe to asterisk.
        /// </remarks>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>true if file modified successfully</returns>
        public bool ModifyFactoryConfig(string section, string key, string value)
        {
            try
            {
                if (!File.Exists(_defaultFactoryConfigPath))
                {
                    BuildFactoryFile();
                }

                StreamReader sr = new StreamReader(_defaultFactoryConfigPath);
                String[] rows = Regex.Split(sr.ReadToEnd(), "\r\n");
                sr.Close();

                using (StreamWriter sw = new StreamWriter(_factoryConfigPath))
                {
                    bool inSection = false;
                    for (int i = 0; i < rows.Length; i++)
                    {
                        if (rows[i].Contains(section))
                        {
                            inSection = true;
                        }
                        if (inSection && rows[i].Contains(key))
                        {
                            rows[i] = rows[i].Replace(String.Format("<{0}>", key), value);
                            inSection = false;
                        }
                        if (rows[i] != string.Empty)
                        {
                            sw.WriteLine(rows[i]);
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                LOG.Debug(e.ToString());
                return false;
            }
        }

        /**
         * @brief Load the application configuration file from the Windows App
         * ConfigurationManager.
         * 
         * @return Void.
         */
        public void LoadAppConfiguration()
        {
            ConfigurationManager.RefreshSection("appSettings");
            try
            {
                _emergencyNumbers = new List<String>(System.Configuration.ConfigurationManager.AppSettings["EmergencyContactNumbers"].Split(new Char[] { ',', ' ' }));
                _emergencyNumbers = _emergencyNumbers.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            }
            catch (System.NullReferenceException e)
            {
                LOG.Debug("Could not find configuration field EmergencyContactNumbers. " + e.ToString());
            }
            finally
            {
                if (_emergencyNumbers.Count == 0)
                {
                    _emergencyNumbers.Add("911");
                }
            }

            try
            {
                /* This CustomGeolocation field is no longer in use because we have moved to a new way for providing emergency
                 * geolocation data. We are now supplying a URI to an http server that will return a valid geolocation XML card.
                 * For more please see LinphoneService::MakeCall and AccountSettings.xaml.cs::OnGeolocationChanged. */
                List<String> latlong = new List<String>(System.Configuration.ConfigurationManager.AppSettings["CustomGeolocation"].Split(new Char[] { ',', ' ' }));
                latlong = latlong.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                _customGeolocation = string.Format("<geo:{0},{1}>", latlong.ElementAt(0), latlong.ElementAt(1));
            }
            catch (System.NullReferenceException e)
            {
                LOG.Debug("Could not find configuration field CustomGeolocation. " + e.ToString());
            }

        }

        #endregion
    }
}
