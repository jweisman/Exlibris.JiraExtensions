using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExLibris.JiraExtensions.Models
{

    public class Installation
    {
        private DateTime sDate;
        private int _duration;
        private string[] _cases;

        public string key { get; set; }
        public string summary { get; set; }
        public string link { get; set; }
        public string startDateText
        {
            set
            {
                if (value.IndexOf("+") > 0)
                    value = value.Substring(0, value.IndexOf("+") - 1);
                sDate = DateTime.Parse(value);

                // Truncate date to remove daylight savings uncertainties
                // sDate = new DateTime(sDate.Year, sDate.Month, sDate.Day);
            }
        }
        public DateTime startDate { get { return sDate; } }
        public DateTime endDate
        {
            get
            {
                if (duration <= 1)
                    return startDate;
                else
                    return startDate.AddDays(duration - 1);
            }
        }
        public int duration
        {
            get { return _duration; }
            set { _duration = value / 9; }
        }
        public string userName { get; set; }
        public string customerCode { get; set; }
        public string serviceIncident { get; set; }
        public string installerName { get; set; }
        public string caseList {
            set
            {
                if (!String.IsNullOrEmpty(value))
                    _cases = value.Split(",".ToCharArray());
            }
        }
        public string[] cases { get { return _cases;  } }
    }

    public class AlmaInstallation : Installation
    {
    }

    public class CustomerInstallation : Installation
    {
        public string product { get; set; }
        public string installTask { get; set; }
        public Boolean dateApproved { get; set; }
        public Boolean valuedCustomer { get; set; }
        public string region { get; set; }
        public string hosted { get; set; }
        public Boolean ryo { get; set; }
        public string security { get; set; }
    }
}