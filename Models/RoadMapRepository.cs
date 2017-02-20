using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExLibris.JiraExtensions.Models
{
    public class RoadMapRepository
    {
        public IEnumerable<RoadMapVersion> GetRoadMapVersions()
        {
            List<RoadMapVersion> l = new List<RoadMapVersion>();

            RoadMapVersion m1 = new RoadMapVersion();
            m1.RemainingTime = 85.5625;
            m1.ActualTime = 0.875;
            m1.VersionName = "2.1.1";
            m1.VersionDescription = "Version 2.1 SP 1";
            m1.ReleaseDate = DateTime.Parse("01-Jan-11");
            m1.VersionId = 10041;
            m1.Released = false;

            RoadMapVersion m2 = new RoadMapVersion();
            m2.RemainingTime = 33.75;
            m2.ActualTime = 7.27083333333333;
            m2.VersionName = "2.1 QA1";
            m2.VersionDescription = "QA Build before NLB release";
            m2.ReleaseDate = DateTime.Parse("30-Jun-10");
            m2.VersionId = 10042;
            m2.Released = false;

            RoadMapVersion m3 = new RoadMapVersion();
            m3.RemainingTime = 153.1875;
            m3.ActualTime = 2.8125;
            m3.VersionName = "2.1 QA2";
            m3.VersionDescription = "QA Build before NLNZ release";
            m3.ReleaseDate = DateTime.Parse("19-Aug-10");
            m3.VersionId = 10065;
            m3.Released = false;

            RoadMapVersion m4 = new RoadMapVersion();
            m4.RemainingTime = 240;
            m4.ActualTime = 279;
            m4.VersionName = "2.1 QA0";
            m4.VersionDescription = "First QA release";
            m4.ReleaseDate = DateTime.Parse("01-Jun-10");
            m4.VersionId = 10020;
            m4.Released = true;

            l.Add(m4);
            l.Add(m2);
            l.Add(m3);
            l.Add(m1);
            return l;
        }

    }
}