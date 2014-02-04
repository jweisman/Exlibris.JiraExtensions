using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace ExLibris.JiraExtensions.Models
{
    public class RoadMapVersion
    {
        private decimal remainingTime;
        public decimal RemainingTime {
            get { return remainingTime; }
            set { remainingTime = Math.Round(value, 1); }
        }
        private decimal actualTime;
        public decimal ActualTime
        {
            get { return actualTime; }
            set { actualTime = Math.Round(value, 1); }
        }
        public decimal TotalTime
        {
            get { return RemainingTime + ActualTime; }
        }
        public decimal PercentComplete
        {
            get { return (ActualTime >= TotalTime ? 1 : Math.Round((ActualTime / TotalTime)*100)); }
        }
        public string VersionName { get; set; }
        public string VersionDescription { get; set; }
        public bool Released { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int VersionId { get; set; }
        public string IconUrl
        {
            get { return String.Format("{0}images/icons/{1}_16.gif", ConfigurationManager.AppSettings["jiraPrefix"], Released ? "package" : "box"); }
        }

    }

    public class RoadMapGadgetViewModel
    {
        public IEnumerable<RoadMapVersion> versions;
        public ExLibris.JiraExtensions.JiraRPC.RemoteProject project;
    }
}