using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using ExLibris.JiraExtensions.JiraRPC;
using ExLibris.JiraExtensions.Models;
using ExLibris.JiraExtensions.Utilities;

namespace ExLibris.JiraExtensions.Controllers
{
    public class JiraGadgetsController : Controller
    {

        static JiraSoapServiceService _jss;
        static string _token;
        static string _jiraUsername;
        static string _jiraPassword;

        public JiraGadgetsController()
        {
            if (_jiraUsername == null)
            {
                _jiraUsername = ConfigurationManager.AppSettings["jiraUsername"];
                SimpleAES crypto = new SimpleAES();
                _jiraPassword = crypto.DecryptString(ConfigurationManager.AppSettings["jiraPassword"]);
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TravelCalendar(string projectKey)
        {
            if (String.IsNullOrEmpty(projectKey))
                return View("Error", new ErrorInfoViewModel() { ErrorMessage = "Please select a project" });

            return View();
        }

        public JsonResult GetTravel(string projectKey)
        {
            JiraRepository rc = new JiraRepository();

            //var travel = HttpRuntime.Cache.GetOrStore<IEnumerable<Trip>>("travel", 3600, () => rc.GetTravel());
            var travel = rc.GetTravel();

            var travelList = from t in travel
                             where t.ProjectKey == projectKey
                             select new
                             {
                                 id = t.IssueKey,
                                 start = t.StartDate.ToString("s"),
                                 end = t.EndDate.ToString("s"),
                                 allDay = true,
                                 title = String.Format("{0} ({1})", t.Description, t.Traveller)
                             };

            var rows = travelList.ToArray();
            return Json(rows, JsonRequestBehavior.AllowGet);    

        }

        /// <summary>
        /// Returns a google chart of issues burndown for a specified version
        /// </summary>
        /// <param name="projectKey">Project Key, eg. DPS</param>
        /// <param name="version">Name of the desired version</param>
        /// <param name="refresh">Refresh parameter for chart- options are a number of minutes, or "now" to remove the cache</param>
        /// <returns></returns>
        public ActionResult IssuesBurndownChart(string projectKey, string version, string refresh)
        {
            try
            {
                int i;
                _jss = new JiraSoapServiceService();
                _token = _jss.login(_jiraUsername, _jiraPassword);

                string versionId="";

                if (String.IsNullOrEmpty(projectKey))
                    return View("Error", new ErrorInfoViewModel() { ErrorMessage = "Please select a project" });

                // Agile or Regular?
                if (!int.TryParse(projectKey, out i))
                {
                    // Get relevant version
                    List<RemoteVersion> versions = HttpRuntime.Cache.GetOrStore<List<RemoteVersion>>("versions-" + projectKey, 3600, () => _jss.getVersions(_token, projectKey).ToList());
                    try
                    {
                        versionId = versions.Find(v => v.name == version).id;
                    }
                    catch (NullReferenceException)
                    {
                        return View("Error", new ErrorInfoViewModel() { ErrorMessage = String.Format("Version does not exist in project {0}.", projectKey) });
                    }
                }

                // Handle refresh - options are now or a value in minutes
                int refreshMinutes;
                handleRefreshParameter(Request.QueryString["refresh"], "sprint-" + version, out refreshMinutes);

                // Populate View Model parameters
                JiraRepository rc = new JiraRepository();
                IssuesBurndownChartViewModel vm = new IssuesBurndownChartViewModel();
                vm.Title = String.Format("Issues Burndown Chart for {0}", version);
                vm.Refresh = refreshMinutes;
                vm.Sprint = HttpRuntime.Cache.GetOrStore<JiraSprint>("sprint-" + version, 60 * 45, () => String.IsNullOrEmpty(versionId) ? 
                    rc.GetBugsForAgileSprint(projectKey, version) : rc.GetBugsForSprint(projectKey, versionId, version));

                return View(vm);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorInfoViewModel() { ErrorMessage = String.Format("An unexpected error has occurred ({0}).", ex.Message), Refresh = 15 }); 
            }
        }

        /// <summary>
        /// Return a google chart of JUnit test results extracted from Jenkins results
        /// </summary>
        /// <param name="jenkinsUrl">URL in Jenkins of the desired job</param>
        /// <param name="numOfBuilds">Number of builds to go back- default is 7</param>
        /// <param name="title">Title of chart</param>
        /// <param name="refresh">Refresh parameter for chart- options are a number of minutes, or "now" to remove the cache</param>
        /// <returns></returns>
        public ActionResult JenkinsTestResults(string jenkinsUrl, string numOfBuilds, string title, string refresh)
        {
            // Validate Jenkins URL
            if (String.IsNullOrEmpty(jenkinsUrl))
                return View("Error", new ErrorInfoViewModel() { ErrorMessage = "Please specify a Jenkins Url." });

            numOfBuilds = Util.nvl(numOfBuilds, "7");

            // Handle refresh - options are now or a value in minutes
            int refreshMinutes;
            handleRefreshParameter(Request.QueryString["refresh"], jenkinsUrl, out refreshMinutes);

            try
            {
                // Populate View Model parameters
                JenkinsRepository jr = new JenkinsRepository();
                JenkinsTestResultViewModel vm = new JenkinsTestResultViewModel();
                vm.Title = title;
                vm.Refresh = refreshMinutes;
                vm.TestResults = HttpRuntime.Cache.GetOrStore<List<JenkinsTestResult>>(jenkinsUrl, 60 * 45, () => jr.GetTestResults(jenkinsUrl, int.Parse(numOfBuilds)));

                return View(vm);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorInfoViewModel() { ErrorMessage = String.Format("An unexpected error has occurred ({0}).", ex.Message), Refresh = 15 }); 
            }

        }

        # region Helpers

        /// <summary>
        /// Handles refresh parameter. Options are "now" or a value in minutes
        /// </summary>
        /// <param name="refresh">Value passed in from querystring</param>
        /// <param name="cacheKey">Key of cache item to remove if refreshing</param>
        /// <param name="refreshMinutes">Out parameter for minutes</param>
        private void handleRefreshParameter(string refresh, string cacheKey, out int refreshMinutes)
        {
            refreshMinutes = 1000;
            if (!String.IsNullOrEmpty(refresh))
            {
                if (refresh.ToLower() == "now")
                    HttpRuntime.Cache.Remove(cacheKey);
                else if (!String.IsNullOrEmpty(refresh))
                    int.TryParse(refresh, out refreshMinutes);
            }
        }

        #endregion
    }
}
