using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExLibris.JiraExtensions.Models
{
    public class JenkinsTestResult
    {
        public DateTime TestDate { get; set; }
        public int TestsPassed { get; set; }
        public int TestsFailed { get; set; }
        public string TestName { get; set; }
        public long BuildNumber { get; set; }
    }

    public class JenkinsTestResultViewModel
    {
        public string Title { get; set; }
        public int Refresh { get; set; }
        public List<JenkinsTestResult> TestResults;
    }

}