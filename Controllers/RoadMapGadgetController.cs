using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ExLibris.JiraExtensions.Models;
using ExLibris.JiraExtensions.JiraRPC;
using ExLibris.JiraExtensions.Utilities;
using System.Configuration;

namespace ExLibris.JiraExtensions.Controllers
{
    public class RoadMapGadgetController : Controller
    {
        static JiraSoapServiceService _jss;
        static string _token;
        static string _jiraUsername;
        static string _jiraPassword;

        public RoadMapGadgetController()
        {
            if (_jiraUsername == null)
            {
                _jiraUsername = ConfigurationManager.AppSettings["jiraUsername"];
                SimpleAES crypto = new SimpleAES();
                _jiraPassword = crypto.DecryptString(ConfigurationManager.AppSettings["jiraPassword"]);
            }
        }

        //
        // GET: /RoadMapGadget/
        public ActionResult Index(string project, string username)
        {
            _jss = new JiraSoapServiceService();
            _token = _jss.login(_jiraUsername, _jiraPassword);

            project = Util.nvl(project, "DPS");

            // Get Project List from cache and get project for specified input
            List<RemoteProject> projects = HttpRuntime.Cache.GetOrStore<List<RemoteProject>>("projects", 3600, () => _jss.getProjectsNoSchemes(_token).ToList());
            
            // Define view model
            RoadMapGadgetViewModel vm = new RoadMapGadgetViewModel();
            vm.project = projects.First(p => p.key == project);
            
            // Get data from DB (can't get from API)
            JiraRepository rc = new JiraRepository();
            vm.versions = rc.GetRoadMapVersions(vm.project.id, username);
            return View(vm);
        }
    }
}
