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
    public class ReopenRateGadgetController : Controller
    {

        static JiraSoapServiceService _jss;
        static string _token;
        static string _jiraUsername;
        static string _jiraPassword;

        public ReopenRateGadgetController()
        {
            if (_jiraUsername == null)
            {
                _jiraUsername = ConfigurationManager.AppSettings["jiraUsername"];
                SimpleAES crypto = new SimpleAES();
                _jiraPassword = crypto.DecryptString(ConfigurationManager.AppSettings["jiraPassword"]);
            }
        }

        //
        // GET: /ReopenRate/
        public ActionResult Index(string project)
        {
            _jss = new JiraSoapServiceService();
            _token = _jss.login(_jiraUsername, _jiraPassword );
            string projectId;

            project = Util.nvl(project, "DPS");

            // Get Project ID
            List<RemoteProject> projects = HttpRuntime.Cache.GetOrStore<List<RemoteProject>>("projects", 3600, () => _jss.getProjectsNoSchemes(_token).ToList());
            projectId = projects.Find(p => p.key == project).id;

            // Get Versions, limit to ones we care about
            List<RemoteVersion> versions = HttpRuntime.Cache.GetOrStore<List<RemoteVersion>>("versions-"+project, 3600, () => _jss.getVersions(_token, project).ToList());
            versions = versions.Where(v => !v.archived && (!v.released || v.releaseDate > DateTime.Now.AddDays(-90))).OrderBy(v => v.sequence).ToList();

            // Populate version list
            var rlist = new List<ReopenRateVersion>();
            foreach (var v in versions)
            {
                ReopenRateVersion r = new ReopenRateVersion() { VersionId = v.id, VersionName = v.name };
                rlist.Add(r);
            }
            rlist.Add(new ReopenRateVersion() { VersionId = "", VersionName = "None" });

            // Enrich version list with data from DB (not available in API)
            JiraRepository rc = new JiraRepository();
            rc.GetReopenRates(projectId, ref rlist);

            return View(rlist);
        }

    }
}
