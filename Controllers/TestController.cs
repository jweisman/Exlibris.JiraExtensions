using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ExLibris.JiraExtensions.Models;
using ExLibris.JiraExtensions.JiraRPC;
using System.Configuration;
using ExLibris.JiraExtensions.Utilities;
using System.Web.Script.Serialization;
using System.Net;
using System.IO;
using System.Text;

namespace ExLibris.JiraExtensions.Controllers
{
    public class TestController : Controller
    {

        JiraRPC2.JiraSoapServiceClient _jss;
        string _token;
        string _jiraUsername;
        string _jiraPassword;
        string _jiraPrefix;

        public TestController()
        {
            _jiraUsername = ConfigurationManager.AppSettings["jiraUsername"];
            SimpleAES crypto = new SimpleAES();
            _jiraPassword = crypto.DecryptString(ConfigurationManager.AppSettings["jiraPassword"]);
            _jiraPrefix = ConfigurationManager.AppSettings["jiraPrefix"];

        }

        public ActionResult Index()
        {
            TestChinese();
            return View("Error", new ErrorInfoViewModel() { ErrorMessage = String.Format("All good."), Refresh = 15 }); 
        }

        private void TestChinese()
        {
            string project = "DPS";
            string versionName = "3.2.1 Sprint II";
            _jss = new JiraRPC2.JiraSoapServiceClient();

            JavaScriptSerializer json = new JavaScriptSerializer();
            WebRequest req = WebRequest.Create(String.Format("{0}rest/api/2/search?jql=project={1}+and+fixVersion=\"{2}\"", _jiraPrefix, project, versionName));
            // req.Credentials = new NetworkCredential(_jiraUsername, _jiraPassword);
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(_jiraUsername + ":" + _jiraPassword));
            req.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
            WebResponse resp = req.GetResponse();
            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string response = sr.ReadToEnd();

            var jss = new JavaScriptSerializer();

            dynamic issues = jss.Deserialize<dynamic>(response);

            // ResolutionDate = i.resolution == null ? DateTime.MaxValue : _jss.getResolutionDateById(_token, Int64.Parse(i.id))
            List<JiraIssue> jiraIssues = new List<JiraIssue>();
            foreach (dynamic issue in issues["issues"])
            {
                //IssueKey = i.key, ResolutionDate = i.resolution == null ? DateTime.MaxValue : _jss.getResolutionDateById(_token, Int64.Parse(i.id))

                jiraIssues.Add(new JiraIssue() { IssueKey = issue["key"], 
                    ResolutionDate = issue["fields"]["resolutiondate"] == null ? 
                    DateTime.MaxValue : DateTime.Parse( issue["fields"]["resolutiondate"])});
            }


            /*
            _jss.
            _jss.login();
            _token = _jss.login(_jiraUsername, _jiraPassword);
            _jss.getIssuesFromJqlSearch(_token, String.Format("project = {0} and fixVersion = '{1}' and issueType = Bug", project, versionName), 1000).ToList();
             * 
             **/
        }

    }
}
