using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExLibris.JiraExtensions.Models
{
    public class ReopenRateVersion
    {
        public string VersionName { get; set; }
        public string VersionId { get; set; }
        public ReopenRateGroup ReopenOnce { get; set; }
        public ReopenRateGroup ReopenTwice { get; set; }
        public ReopenRateGroup ReopenMore { get; set; }

        public ReopenRateVersion()
        {
            ReopenOnce = new ReopenRateGroup();
            ReopenTwice = new ReopenRateGroup();
            ReopenMore = new ReopenRateGroup();
        }
    }

    public class ReopenRateGroup
    {
        public int Times { get; set; }
        public List<String> IssueKeys { get; set; }

        public ReopenRateGroup()
        {
            IssueKeys = new List<string>();
        }
    }
}