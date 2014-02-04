using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Configuration;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using ExLibris.JiraExtensions.Utilities;

namespace ExLibris.JiraExtensions.Models
{
    public class JenkinsRepository
    {

        /// <summary>
        /// Gets test results for a particular JUnit job in Jenkins
        /// </summary>
        /// <param name="jenkinsUrl">URL of the Jenkins job</param>
        /// <param name="buildsBack">Number of builds to go back</param>
        /// <returns>List of test results</returns>
        public List<JenkinsTestResult> GetTestResults(string jenkinsUrl, int buildsBack)
        {
            SimpleAES crypto = new SimpleAES();
            var testResults = new List<JenkinsTestResult>();

            CookieContainer cc = Login(ConfigurationManager.AppSettings["jenkinsUsername"], crypto.DecryptString(ConfigurationManager.AppSettings["jenkinsPassword"]));

            string url = jenkinsUrl + "lastCompletedBuild/";
            long currentBuildNumber=0;

            JenkinsTestResult tr=null;
            for (int n = 0; n < buildsBack; n++)
            {
                 tr = GetTestResult(cc, url);
                 if (n == 0)
                     currentBuildNumber = tr.BuildNumber-1;
                 else
                     currentBuildNumber = currentBuildNumber - 1;

                 if (tr != null && tr.TestsFailed + tr.TestsPassed != 0) // make sure there are test results
                     testResults.Insert(0, tr);

                 url = jenkinsUrl + currentBuildNumber + "/";
            }

            return testResults;
        }

        /// <summary>
        /// Gets test results for a particular test run
        /// </summary>
        /// <param name="cc">Cookie Container which contains the authorization cookie</param>
        /// <param name="jenkinsUrl">URL of the particular test run</param>
        /// <returns></returns>
        private JenkinsTestResult GetTestResult(CookieContainer cc, string jenkinsUrl)
        {
            JenkinsTestResult tr = new JenkinsTestResult();

            // Get test home page
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ConfigurationManager.AppSettings["jenkinsUrl"] + jenkinsUrl);
            request.CookieContainer = cc;

            string resp = null;
            HttpWebResponse response = null;
            StreamReader sr = null;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
                sr = new StreamReader(response.GetResponseStream());
                resp = sr.ReadToEnd();
            }
            catch (WebException ex)
            {
                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
                    return null;
                else
                    throw ex;
            }

            // Retrieve build number & date
            Regex exp = new Regex(@"Build #([0-9]+)\s*\(([A-Za-z0-9:, ]*)\)", RegexOptions.IgnoreCase);
            Match m = exp.Match(resp);
            if (m.Success && m.Groups.Count >= 2)
            {
                tr.BuildNumber = int.Parse(m.Groups[1].Value);
                tr.TestDate = DateTime.Parse(m.Groups[2].Value);
            }

            // Get test results
            request = (HttpWebRequest)WebRequest.Create(ConfigurationManager.AppSettings["jenkinsUrl"] + jenkinsUrl + "testReport/");
            request.CookieContainer = cc;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
                sr = new StreamReader(response.GetResponseStream());
                resp = sr.ReadToEnd();
            }
            catch (WebException ex)
            {
                // If there are no test results, return null
                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
                    return tr;
                else
                    throw ex;
            }

            // Get number of tests passed and failed
            exp = new Regex(@"\s*([0-9]*) failures\n.*\n\s*([0-9]*) tests", RegexOptions.IgnoreCase);
            m = exp.Match(resp);
            if (m.Success && m.Groups.Count >= 2)
            {
                tr.TestsFailed = int.Parse(m.Groups[1].Value);
                tr.TestsPassed = int.Parse(m.Groups[2].Value) - tr.TestsFailed;
            }


            return tr;
        }

        /// <summary>
        /// Logs into Jenkins and returns a CookieContainer with the authentication ticket
        /// </summary>
        /// <param name="userName">Jenkins Username</param>
        /// <param name="password">Jenkins Password</param>
        /// <returns>Cookie Container with the authentication ticket</returns>
        private CookieContainer Login(string userName, string password)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ConfigurationManager.AppSettings["jenkinsUrl"] + "/j_acegi_security_check");
            CookieContainer cc = new CookieContainer();
            request.CookieContainer = cc;
            request.Method = "POST";

            // Create POST data and convert it to a byte array.
            string postData = String.Format("j_username={0}&j_password={1}&remember_me=on&Submit=log+in", userName, password);
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            string resp = sr.ReadToEnd();

            return cc;

        }
    }
}