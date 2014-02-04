using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExLibris.JiraExtensions.Models;
using System.Web.Mvc;
using System.Configuration;

namespace ExLibris.JiraExtensions.Utilities
{
    public static class HtmlExtensions
    {

        public static string InstallHtml(this HtmlHelper helper, Installation inst)
        {
            string html = "";
            string backgroundColor = "";

            if (inst.GetType() == typeof(CustomerInstallation))
            {
                CustomerInstallation installation = (CustomerInstallation)inst;
                if (installation.valuedCustomer)
                    backgroundColor = "FFCC00";
                else if (installation.region == "Internal")
                    backgroundColor = "66CC00";
                html = String.Format("<span style='font-style:{0}; background-color:#{10}'><a href='{1}'>{2}</a>: {3} {4} @ {5} [{6}]{7}{8}{9}</span><br/><hr/>",
                    installation.dateApproved ? "normal" : "italic", installation.link, installation.key, installation.product,
                    installation.installTask, installation.customerCode, SILink(installation.serviceIncident, installation.cases),
                    !String.IsNullOrEmpty(installation.hosted) ? ", " + installation.hosted : "",
                    (installation.ryo) ? ", RYO" : "",
                    !String.IsNullOrEmpty(installation.security) ? ", " + installation.security : "",
                    backgroundColor);
            }
            else
            {
                Installation installation = inst;
                html = String.Format("<span style='font-style:normal; background-color:#white'><a href='{0}'>{1}</a>: {3} @ {2}</span><br/><hr/>",
                    installation.link, installation.key, installation.customerCode, SILink(installation.serviceIncident, installation.cases ));

            }

            return html;
        }

        public static string NewInstallLink(this HtmlHelper helper, DateTime date, string user, string projectId)
        {
            string html;
            html = String.Format(" ondblclick=\"window.open('{0}secure/CreateIssueDetails!init.jspa?pid={3}&issuetype=9&priority=3&assignee={1}&customfield_10035={2}', 'newInstall')\"",
                ConfigurationManager.AppSettings["jiraPrefix"], user, date.ToString("d'/'MMM'/'yyyy"), projectId);
            return html;
        }

        private static string SILink(string si, string[] cases)
        {
            string silink = "";
            if (!String.IsNullOrEmpty(si))
            {
                string siurl = Util.SIUrl(si);
                if (!String.IsNullOrEmpty(siurl))
                    silink = String.Format("<a target='_blank' href='{0}'>{1}</a>", siurl, si);
                else
                    silink = si;
            }

            if (cases != null)
            {
                foreach (string c in cases)
                {
                    if (silink != "") silink += " ";
                    silink += String.Format("<a target='_blank' href='{0}'>{1}</a>", Util.CaseUrl(c), c);
                }
            }

            return silink;
        }

    }
}