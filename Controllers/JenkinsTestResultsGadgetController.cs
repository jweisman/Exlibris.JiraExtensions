using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using ExLibris.JiraExtensions.Models;
using ExLibris.JiraExtensions.Utilities;

namespace ExLibris.JiraExtensions.Controllers
{
    public class JenkinsTestResultsGadgetController : Controller
    {
        //
        // GET: /JenkinsTestResultsGadget/

        public ActionResult Index(string jenkinsUrl, string numOfBuilds, string title, string refresh)
        {
            JenkinsRepository jr = new JenkinsRepository();
            JenkinsTestResultViewModel vm = new JenkinsTestResultViewModel();

            // Validate Jenkins URL
            if (String.IsNullOrEmpty(jenkinsUrl))
                return View("Error", new ErrorInfoViewModel() { ErrorMessage = "Please specify a Jenkins Url." });

            numOfBuilds = Util.nvl(numOfBuilds, "7");

            // Handle refresh - options are now or a value in minutes
            int refreshMinutes = 100000000;
            if (!String.IsNullOrEmpty(Request.QueryString["refresh"]))
            {
                if (Request.QueryString["refresh"].ToLower() == "now")
                    HttpRuntime.Cache.Remove(jenkinsUrl);
                else if (!String.IsNullOrEmpty(Request.QueryString["refresh"]))
                    int.TryParse(Request.QueryString["refresh"], out refreshMinutes);
            }

            // Populate View Model parameters
            vm.Title = title;
            vm.Refresh = refreshMinutes;
            vm.TestResults = HttpRuntime.Cache.GetOrStore<List<JenkinsTestResult>>(jenkinsUrl, 60*45, () => jr.GetTestResults(jenkinsUrl, int.Parse(numOfBuilds)));

            return View(vm);

        }

    }
}
