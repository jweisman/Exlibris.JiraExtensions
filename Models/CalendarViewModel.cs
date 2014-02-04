using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExLibris.JiraExtensions.Models
{
    public class CalendarViewModel
    {
        public string project { get; set; }
        public string projectId { get; set; }
        public List<Installer> installers { get; set; }
        public List<Installation> installations { get; set; }
    }
}