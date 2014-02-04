using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using ExLibris.JiraExtensions.Utilities;
using ExLibris.JiraExtensions.Models;
using ExLibris.JiraExtensions.JiraRPC;
using System.Configuration;

namespace ExLibris.JiraExtensions.Controllers
{
    public class ManpowerGadgetController : Controller
    {
        static JiraSoapServiceService _jss;
        static string _token;
        static string _jiraUsername;
        static string _jiraPassword;

        public ManpowerGadgetController()
        {
            if (_jiraUsername == null)
            {
                _jiraUsername = ConfigurationManager.AppSettings["jiraUsername"];
                SimpleAES crypto = new SimpleAES();
                _jiraPassword = crypto.DecryptString(ConfigurationManager.AppSettings["jiraPassword"]);
            }
        }

        public ActionResult Index(string project)
        {
            string projectId = "";
            project = Util.nvl(project, "URM");
            _jss = new JiraSoapServiceService();
            _token = _jss.login(_jiraUsername, _jiraPassword);

            // Get Project ID
            List<RemoteProject> projects = HttpRuntime.Cache.GetOrStore<List<RemoteProject>>("projects", 3600, () => _jss.getProjectsNoSchemes(_token).ToList());
            RemoteProject proj = projects.Find(p => p.key == project);
            if (proj != null)
            {
                projectId = proj.id;
                ViewData["projectName"] = proj.key; // proj.description; // not ideal; will do a ViewModel if we have more properties
            }
            // TODO: else redirect to error
            
            JiraRepository rc = new JiraRepository();
            ManpowerViewModel vm = new ManpowerViewModel();
            vm.AllocatedWork = rc.GetManpower(projectId, true);
            vm.NonAllocatedWork = rc.GetManpower(projectId, false);

            return View(vm);
        }

        public ActionResult Scope(string project)
        {
            string projectId = "";
            project = Util.nvl(project, "URM");
            _jss = new JiraSoapServiceService();
            _token = _jss.login(_jiraUsername, _jiraPassword);

            // Get Project ID
            List<RemoteProject> projects = HttpRuntime.Cache.GetOrStore<List<RemoteProject>>("projects", 3600, () => _jss.getProjectsNoSchemes(_token).ToList());
            RemoteProject proj = projects.Find(p => p.key == project);
            if (proj != null)
            {
                projectId = proj.id;
                ViewData["projectName"] = proj.key; // proj.description; // not ideal; will do a ViewModel if we have more properties
            }
            else
            {
                return View("Error", new ErrorInfoViewModel() { ErrorMessage = String.Format("Project does not exist")}); 
            }

            JiraRepository rc = new JiraRepository();
            List<ScopingVersion> l = rc.GetScoping(projectId);

            return View(l);
        }

    }
}
