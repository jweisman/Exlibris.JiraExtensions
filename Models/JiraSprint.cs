using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExLibris.JiraExtensions.Models
{
    public class JiraSprint
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Name { get; set; }
        public long Id { get; set; }
        public int NumberOfIssues { get; set; }
        public List<JiraSprintDay> Days { get; set; }

        public JiraSprint()
        {
            Days = new List<JiraSprintDay>();
        }

    }

    public class JiraSprintDay
    {
        public DateTime Day { get; set; }
        public long ResolvedIssues { get; set; }
    }

    public class JiraIssue
    {
        private DateTime resolutionDate;

        public DateTime ResolutionDate { 
            get { return resolutionDate; }
            set {
                if (value == null) resolutionDate = DateTime.MaxValue;
                else resolutionDate = value;
            } 
        }
        public string IssueKey { get; set; }
    }

    public class IssuesBurndownChartViewModel
    {
        public string Title { get; set; }
        public int Refresh { get; set; }
        public JiraSprint Sprint { get; set; }
        
    }
}