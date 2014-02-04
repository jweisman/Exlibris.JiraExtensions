using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace ExLibris.JiraExtensions.Models
{
    public class ScopingVersion
    {
        private static XmlDocument _manpowerXml;
        public string ParentVersion { get; set; }
        public string Version { get; set; }
        private double _manpower = -1;
        public double Manpower {
            get
            {
                XmlNode sprint = _manpowerXml.SelectSingleNode("/manpower/sprint[@name='" + Version.ToString().Trim() + "']");
                if (sprint != null)
                    if (sprint.Attributes["scopingManpower"] != null)
                        _manpower = Convert.ToDouble(sprint.Attributes["scopingManpower"].Value);

                return (_manpower == -1 ? 720 : _manpower);
            }
        }
        public double ManpowerYears {
            get { return Math.Round(Manpower/216, 3); }
        }
        public double TotalDays { 
            get { return TaskDays + StoryDays; }
        }
        public double Load {
            get { return Math.Round(TotalDays / Manpower, 2); } // round(((taskdays + storydays)/manpower)*100,2)
        }
        public double TaskDays { get; set; }
        public double TaskPercent {
            get { return Math.Round((TaskDays / TotalDays), 4); } //round(taskdays/(taskdays+storydays)*100,2)
        }
        public double StoryDays { get; set; }
        public double StoryPercent {
            get { return Math.Round((StoryDays / TotalDays), 4); }
        }

        public ScopingVersion(XmlDocument manpowerXml)
        {
            _manpowerXml = manpowerXml;
        }

        public ScopingVersion()
        {
            new ScopingVersion(null);
        }
    }
}