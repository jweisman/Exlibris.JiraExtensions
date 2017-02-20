using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using ExLibris.JiraExtensions.Utilities;
using ExLibris.JiraExtensions.Models;
using System.Configuration;

namespace ExLibris.JiraExtensions.Controllers
{
    public class InstallationsCalendarController : Controller
    {
        //
        // GET: /Calendar/

        public ActionResult Index()
        {
            // Get installations from Jira
            var installations = HttpRuntime.Cache.GetOrStore<IEnumerable<Installation>>("installations",60,
                 () => Util.GetInstallations());

            // Order by startdate, then username
            installations = installations.OrderBy(i => i.startDate).ThenBy(i => i.userName);

            CalendarViewModel model = new CalendarViewModel();
            model.installations = installations.ToList<Installation>();

            // Get distinct list of installers
            model.installers = HttpRuntime.Cache.GetOrStore<IEnumerable<Installer>>("installers", 900,
                 () => Util.GetInstallers()).ToList<Installer>();

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
           
            var installers = HttpRuntime.Cache.GetOrStore<IEnumerable<Installer>>("installers", 900,
                 () => Util.GetInstallers());

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
                    Response.Cookies.Add(new HttpCookie("userName") { Expires = DateTime.Now.AddDays(14), Value = userName }); // TODO: add path
                }
            }

            return View(installers);
        }

        public JsonResult GetInstallations(double start, double end)
        {

            var userName = Session["UserName"] as string;
            if (string.IsNullOrEmpty(userName))
            {
                return null;
            }

            var fromDate = Util.ConvertFromUnixTimestamp(start).AddDays(-2);
            var toDate = Util.ConvertFromUnixTimestamp(end).AddDays(-2);

            // Get installations from Jira
            var installations = HttpRuntime.Cache.GetOrStore<IEnumerable<Installation>>("installations", 60,
                 () => Util.GetInstallations());

            var installList = from i in installations
                              where i.userName == userName && i.startDate >= fromDate && i.endDate <= toDate
                              select new
                                  {
                                      id = i.key,
                                      title = String.Format("{0}: {1} {2} @ {3} [{4}]{5}",
                                            i.key, i.product, i.installTask, i.customerCode, i.serviceIncident, 
                                            !String.IsNullOrEmpty(i.environmentNotes) ? ", " + i.environmentNotes : ""),
                                      start = i.startDate.ToString("s"),
                                      end = i.endDate.ToString("s"),
                                      allDay = true,
                                      url = i.link
                                  };

            var rows = installList.ToArray();
            return Json(rows, JsonRequestBehavior.AllowGet);    
        }


    }
}
