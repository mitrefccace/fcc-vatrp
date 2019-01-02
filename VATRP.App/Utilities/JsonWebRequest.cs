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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.Utilities
{
    public static class JsonWebRequest
    {
        /// <summary>
        /// GET request.
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="webRequestUrl">string</param>
        /// <returns>T</returns> 
        public static T MakeHttpJsonWebRequest<T>(string webRequestUrl)
        {
            WebResponse response = null;
            try
            {
                // create a request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(webRequestUrl);
                request.PreAuthenticate = true;
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

                try
                {
                    // deserialize json to ResourceInfo List
                    T item = JsonDeserializer.JsonDeserialize<T>(jsonResults.ToString());
                    return item;
                }
                catch (Exception ex)
                {
                    throw new JsonException(JsonExceptionType.DESERIALIZATION_FAILED, "Failed to parse json response. Details: " + ex.Message, ex);
                }
            }
            catch (Exception ex)
            {
                throw new JsonException(JsonExceptionType.CONNECTION_FAILED, "Failed to get json information. Details: " + ex.Message, ex);
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

        /// <summary>
        /// Async GET request.
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="webRequestUrl">string</param>
        /// <returns>Task<T></returns>
        public static async Task<T> MakeHttpJsonWebRequestAsync<T>(string webRequestUrl)
        {
            WebResponse response = null;
            try
            {
                // Specify TLS 1.2
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);

                // create a request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(webRequestUrl);
                request.PreAuthenticate = true;
                request.KeepAlive = false;
                request.ContentLength = 0;
                request.Method = "GET";
                request.Timeout = 30000;

                response = await request.GetResponseAsync();
                String jsonResults = "";
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    jsonResults = sr.ReadToEnd();
                    sr.Close();
                }

                try
                {
                    // deserialize json to ResourceInfo List
                    T item = JsonDeserializer.JsonDeserialize<T>(jsonResults.ToString());
                    return item;
                }
                catch (Exception ex)
                {
                    throw new JsonException(JsonExceptionType.DESERIALIZATION_FAILED, "Failed to parse json response. Details: " + ex.Message, ex);
                }
            }
            catch (Exception ex)
            {
                throw new JsonException(JsonExceptionType.CONNECTION_FAILED, "Failed to get json information. Details: " + ex.Message, ex);
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

        /// <summary>
        /// Authenticated GET request.
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="webRequestUrl">string</param>
        /// <param name="userName">string</param>
        /// <param name="password">string</param>
        /// <returns>T</returns>
        public static T MakeJsonWebRequestAuthenticated<T>(string webRequestUrl, string userName, string password)
        {
            WebResponse response = null;
            try
            {
                // TLS 1.2
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);

                // create a request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(webRequestUrl);
                request.Credentials = new NetworkCredential(userName, password);
                request.PreAuthenticate = true;
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

                try
                {
                    // deserialize json to ResourceInfo List
                    T item = JsonDeserializer.JsonDeserialize<T>(jsonResults.ToString());
                    return item;
                }
                catch (Exception ex)
                {
                    throw new JsonException(JsonExceptionType.DESERIALIZATION_FAILED,"Failed to parse json response. Details: " + ex.Message, ex);
                }
            }
            catch (Exception ex)
            {
                throw new JsonException(JsonExceptionType.CONNECTION_FAILED, "Failed to get json information. Details: " + ex.Message, ex);
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

        /// <summary>
        /// Async authenticated XML GET request. 
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="webRequestUrl">string</param>
        /// <param name="cred">VATRPCredential</param>
        /// <returns>Task<T></returns>
        public static async Task<T> MakeXmlWebRequestAuthenticatedAsync<T>(string webRequestUrl, VATRPCredential cred)
        {
            WebResponse response = null;
            try
            {
                // Specify TLS 1.2
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(webRequestUrl);
                request.Credentials = new NetworkCredential(cred.username, cred.password);
                request.PreAuthenticate = true;
                request.KeepAlive = false;
                request.ContentLength = 0;
                request.Method = "GET";
                request.Timeout = 30000;

                response = await request.GetResponseAsync();
                string xmlResults = string.Empty;
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    try
                    {
                        T item = (T)serializer.Deserialize(sr);
                        return item;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        sr.Close();
                        if (response != null)
                        {
                            response.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Async authenticated XML POST request. 
        /// </summary>
        /// <param name="webRequestUrl">string</param>
        /// <param name="cred">VATRPCredential</param>
        /// <param name="xml">string</param>
        /// <returns>void</returns>
        public static async Task MakeXmlWebPostAuthenticatedAsync(string webRequestUrl, VATRPCredential cred, string xml)
         {
            WebResponse response = null;
            try
            {
                // Specify TLS 1.2
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);

                // Build basic authentication header
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(webRequestUrl);
                request.Credentials = new NetworkCredential(cred.username, cred.password);
                request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(cred.username + ":" + cred.password));

                // Specify the rerquest properties
                request.KeepAlive = false;
                request.Method = "POST";
                request.Timeout = 30000;
                request.ContentType = "application/xml"; 

                StreamWriter sw = new StreamWriter(request.GetRequestStream());
                sw.WriteLine(xml);
                sw.Close();

                response = await request.GetResponseAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string GetTextFromXMLFile(string xmlFile)
        {
            StreamReader reader = new StreamReader(xmlFile);
            string data = reader.ReadToEnd();
            reader.Close();
            return data;
        }

        public static bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// Template http async GET request. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="webRequestUrl">string</param>
        /// <returns>Task<T></returns>
        public static async Task<T> MakeJsonWebRequestAsync<T>(string webRequestUrl)
        {
            // Specify TLS 1.2
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);

            //****************************************************************************************************************************************
            //This method call a Web Request on provide URL
            //****************************************************************************************************************************************
            WebResponse response = null;
            try
            {

                // create a request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(webRequestUrl);
                request.KeepAlive = false;
                request.ContentLength = 0;
                request.Method = "GET";
                request.Timeout = 30000;


                response = await request.GetResponseAsync();
                String jsonResults = "";
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    jsonResults = sr.ReadToEnd();
                    sr.Close();
                }

                try
                {
                    Debug.Print(jsonResults.ToString());

                    //************BY MK FOR REMOVE THE PROVIDER AND VERSION FROM JSON STRING *************************
                    int index1 = jsonResults.IndexOf("[");;
                    jsonResults = jsonResults.Remove(0, index1);
                    int index2 = jsonResults.IndexOf("]");
                    jsonResults = jsonResults.Remove(index2 + 1, jsonResults.Length - index2 - 1);

                    Debug.Print(jsonResults.ToString());

                    T item = JsonDeserializer.JsonDeserialize<T>(jsonResults.ToString());
                    return item;
                }
                catch (Exception ex)
                {
                    throw new JsonException(JsonExceptionType.DESERIALIZATION_FAILED, "Failed to parse json response. Details: " + ex.Message, ex);
                }
            }
            catch (Exception ex)
            {
                throw new JsonException(JsonExceptionType.CONNECTION_FAILED, "Failed to get json information. Details: " + ex.Message, ex);
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

        /// <summary>
        /// HTTP GET request.
        /// </summary>
        /// <param name="webRequestUrl">string</param>
        /// <returns>string</returns>
        public static string MakeJsonWebRequest(string webRequestUrl)
        {
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);

            //****************************************************************************************************************************************
            //This method call a Web Request on provide URL and RETURN A JSON RESPONSE
            // THIS METHOD IS CREATED BY MK ON DATED 13-12-2016
            //****************************************************************************************************************************************
            WebResponse response = null;
            try
            {
                // create a request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(webRequestUrl);
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

                try
                {
                    // deserialize json to ResourceInfo List
                    //T item = JsonDeserializer.JsonDeserialize<T>(jsonResults.ToString());
                    //return item;

                   return (jsonResults.ToString());
                }
                catch (Exception ex)
                {
                    throw new JsonException(JsonExceptionType.DESERIALIZATION_FAILED, "Failed to parse json response. Details: " + ex.Message, ex);
                }
            }
            catch (Exception ex)
            {
                throw new JsonException(JsonExceptionType.CONNECTION_FAILED, "Failed to get json information. Details: " + ex.Message, ex);
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
    }
}
