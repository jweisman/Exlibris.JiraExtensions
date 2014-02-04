using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using ExLibris.JiraExtensions.Utilities;
using ExLibris.JiraExtensions.Models;
using System.Configuration;

namespace ExLibris.JiraExtensions.Controllers
{
    public class InstallationsCalendarController : Controller
    {

        JiraRepository rc;

        public InstallationsCalendarController()
        {
            rc = new JiraRepository();
        }

        public ActionResult Index()
        {
            return RedirectToAction("Installations", new { project = "INST" });
        }

        public string Debug()
        {
            string response = "";
            if (HttpRuntime.Cache.Get("jiratoken") == null)
            {
                response = "Null";
            }
            else
            {
                response = HttpRuntime.Cache.Get("jiratoken").ToString();
            }

            if (Request.QueryString["reset"] != null)
            {
                HttpRuntime.Cache.Remove("jiratoken");
                response += "<br/>Cleared";
            }
            return response;
        }

        public ActionResult Installations(string id)
        {
            string project = Util.nvl(id, "INST");

            // Get installations from Jira
            var installations = HttpRuntime.Cache.GetOrStore<IEnumerable<Installation>>("installations-" + project, 60,
                 () => JiraRepository.GetInstallations(project));

            // Order by startdate, then username
            installations = installations.OrderBy(i => i.startDate).ThenBy(i => i.userName);

            CalendarViewModel model = new CalendarViewModel();
            model.project = project;
            model.projectId = JiraRepository.GetJiraProjectId(project);
            model.installations = installations.ToList<Installation>();

            // Get distinct list of installers
            model.installers = JiraRepository.GetInstallers(project).ToList<Installer>();

            // Load locations xml, find installer
            XmlDocument locations = new XmlDocument();
            locations.Load(Server.MapPath("~/Utilities/locations.xml"));
            foreach (var i in model.installers)
            {
                XmlNode location = locations.SelectSingleNode("/locations/location/installers/installer[@userName='" + i.userName + "']/../..");
                if (location != null)
                {
                    i.weekend = location.SelectSingleNode("weekend").InnerText;
                    XmlNodeList nl = location.SelectNodes("holidays/year[@start <= " + DateTime.Now.Year + " and @end >= " + DateTime.Now.Year + "]/holiday");
                    foreach (XmlNode n in nl)
                    {
                        i.holidays.Add(new Holiday() { date = DateTime.Parse(n.Attributes["date"].Value), name = n.Attributes["name"].Value });
                    }

                }
                else
                    i.weekend = "Saturday,Sunday";
            }

            return View(model);

        }

        public ActionResult Installer(string id)
        {

            // **********************************
            // ** Currently supports only INST **
            // **********************************
            string project = "INST";

            var installers = JiraRepository.GetInstallers(project);

            string userName = id;
            if (String.IsNullOrEmpty(userName) && Request.Cookies["userName"] != null)
                userName = Request.Cookies["userName"].Value; 

            if (!String.IsNullOrEmpty(userName))
            {
                Installer installer = installers.First(i => i.userName == userName);
                if (installer != null)
                {
                    ViewData["displayName"] = installer.displayName;
                    // store username for GetInstallations method
                    Session["UserName"] = userName;
                    Session["Project"] = project;
                    Response.Cookies.Add(new HttpCookie("userName") { Expires = DateTime.Now.AddDays(14), Value = userName }); // TODO: add path
                }
            }

            return View(installers);
        }

        public JsonResult GetInstallations(double start, double end)
        {
            var project = Session["Project"] as string;
            project = Util.nvl(project, "INST");

            var userName = Session["UserName"] as string;
            if (string.IsNullOrEmpty(userName))
            {
                return null;
            }

            var fromDate = Util.ConvertFromUnixTimestamp(start).AddDays(-2);
            var toDate = Util.ConvertFromUnixTimestamp(end).AddDays(-2);

            // Get installations from Jira
            var installations = HttpRuntime.Cache.GetOrStore<IEnumerable<Installation>>("installations-" + project, 60,
                 () => JiraRepository.GetInstallations(project));

            var installList = from i in installations 
                              where i.userName == userName && i.startDate >= fromDate && i.endDate <= toDate
                              select new 
                                  {
                                      id = i.key,
                                      title = InstallationTitle(i),
                                      start = i.startDate.ToString("s"),
                                      end = i.endDate.ToString("s"),
                                      allDay = true,
                                      // url = (String.IsNullOrEmpty(Util.SIUrl(i.serviceIncident))) ? i.link : Util.SIUrl(i.serviceIncident),
                                      url = String.IsNullOrEmpty(Util.CaseUrl(i.cases)) ? i.link : Util.CaseUrl(i.cases[0]),
                                      className =  (ValuedCustomer(i) ? "valuedCustomer" : "")
                                  };

            var rows = installList.ToArray();
            return Json(rows, JsonRequestBehavior.AllowGet);    
        }



        #region Utilities
        private static string InstallationTitle(Installation inst)
        {
            if (inst.GetType() == typeof(CustomerInstallation))
            {
                CustomerInstallation i = (CustomerInstallation)inst;
                return String.Format("{0}: {1} {2} @ {3} [{4}]{5}",
                                            i.key, i.product, i.installTask, i.customerCode, i.serviceIncident,
                                            !String.IsNullOrEmpty(i.security) ? ", " + i.security : "");
            }
            else
            {
                return inst.summary;
            }
        }

        private static bool ValuedCustomer(Installation inst)
        {
            if (inst.GetType() == typeof(CustomerInstallation))
            {
                CustomerInstallation i = (CustomerInstallation)inst;
                return i.valuedCustomer;
            }
            return false;

        }
        #endregion


    }
}
