using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using System.Web;
using System.Web.Caching;
using System.Web.Helpers;
using System.Web.Script.Serialization;
using Oracle.DataAccess.Client;
using System.Data;
using System.Configuration;
using System.Text.RegularExpressions;
using ExLibris.JiraExtensions.Utilities;
using ExLibris.JiraExtensions.JiraRPC;

namespace ExLibris.JiraExtensions.Models
{
    public class JiraRepository
    {
        static string _connectionString;
        static JiraSoapServiceService _jss;
        static string _token;
        static string _jiraUsername;
        static string _jiraPassword;
        static string _jiraPrefix;
        static string _jiraRestPrefix = "rest/api/latest";

        public JiraRepository()
        {
            init();
        }

        private static void init()
        {
            if (_connectionString == null)
            {
                SimpleAES crypto = new SimpleAES();
                string connectionString = ConfigurationManager.AppSettings["connectionString"];
                string pwdenc = Regex.Match(connectionString, @"Password=(\w+);").Groups[1].Value;
                string pwddec = crypto.DecryptString(pwdenc);
                _connectionString = Regex.Replace(connectionString, @"Password=\w+;", "Password=" + pwddec + ";");
            }

            if (_jiraUsername == null)
            {
                _jiraUsername = ConfigurationManager.AppSettings["jiraUsername"];
                SimpleAES crypto = new SimpleAES();
                _jiraPassword = crypto.DecryptString(ConfigurationManager.AppSettings["jiraPassword"]);
            }

            if (_jiraPrefix == null)
                _jiraPrefix = ConfigurationManager.AppSettings["jiraPrefix"];

            /*
            if (_jss == null)
            {
                _jss = new JiraSoapServiceService();
                _jss.Url = ConfigurationManager.AppSettings["jiraPrefix"] + "rpc/soap/jirasoapservice-v2?wsdl";
            }
            

            // Handle token timeouts by holding them for only 10 minutes
            if (HttpRuntime.Cache.Get("jiratoken") == null)
            {
                _token = _jss.login(_jiraUsername, _jiraPassword);
                HttpRuntime.Cache.Insert("jiratoken", _token, null, Cache.NoAbsoluteExpiration, TimeSpan.FromSeconds(10 * 60));
            }
            else
                _token = HttpRuntime.Cache.Get("jiratoken").ToString();

            */
        }

        public IEnumerable<Trip> GetTravel()
        {
            // Get travel
            List<RemoteIssue> issues = _jss.getIssuesFromJqlSearch(_token, String.Format("labels = travel and issueType = Task and (resolution=Unresolved or duedate > -30d)"), 1000).ToList();
            IEnumerable<Trip> travel = from i in issues
                                       select new Trip
                                       {
                                           IssueKey = i.key,
                                           StartDate = DateTime.Parse((i.customFieldValues.First(d => d.customfieldId == "customfield_10035")).values[0].ToString()),
                                           EndDate = Convert.ToDateTime(i.duedate).ToLocalTime(),
                                           Description = i.summary,
                                           Traveller = i.assignee,
                                           ProjectKey = i.project
                                       };

            return travel;

        }

        /// <summary>
        /// Queries Jira database for manpower / version calculations
        /// </summary>
        /// <param name="projectId">Project Key</param>
        /// <returns>Returns datatable since summary rows are calculated in Oracle (via group by rollup)</returns>
        public DataTable GetManpower(string projectId, bool allocatedWork = true)
        {
            string strsql = @"select parentversion, childversion, sum(days) days, sum(round(days/175,2)) manyears  from (
                select vp.description parentversion, vc.description childversion, vp.sequence, vc.sequence, sum(numbervalue) days
                from projectversion vp, projectversion vc, versionhierarchy vh, customfieldvalue c, jiraissue i, issuefixversionview f
                where vh.parentid = vp.id 
                and vh.childid = vc.id
                and vc.project = :projectid
                and i.id = c.issue
                and i.project = :projectid
                and i.pkey = f.pkey
                and f.versionid = vc.id
                and c.customfield=10132 
                group by vp.description, vc.description, vp.sequence, vc.sequence
                order by vp.sequence, vc.sequence )
                group by rollup (parentversion, childversion)";

            OracleConnection cn = new OracleConnection(_connectionString);
            OracleCommand cmd = new OracleCommand(strsql, cn);
            OracleParameter param;
            param = cmd.Parameters.Add(":projectId", OracleDbType.Int16);
            param.Value = projectId;

            // Load locations xml, find installer
            XmlDocument manpowerXml = new XmlDocument();
            manpowerXml.Load(HttpContext.Current.Server.MapPath("~/Utilities/manpower.xml"));

            double manpower = 0;
            double target = 0;

            try
            {
                cmd.Connection.Open();
                OracleDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                DataTable table = new DataTable();
                table.Load(rdr);

                // Add extra columns
                table.Columns.Add("manpower");
                table.Columns.Add("target");
                table.Columns.Add("execution_rate");
                table.Columns.Add("target_load");

                // scratch columns to handle allocated work; ugly workaround to handle allocated work requirement
                table.Columns.Add("workdays");
                table.Columns.Add("workyears");

                double manpowerSum = 0;
                double manpowerGrandSum = 0;
                double targetSum = 0;
                double targetGrandSum = 0;
                double workDaysToDeduct = 0;
                double manYearsToDeduct = 0;

                foreach (DataRow row in table.Rows)
                {
                    // Get manpower for version
                    XmlNode sprint = manpowerXml.SelectSingleNode("/manpower/sprint[@name='" + row["childversion"].ToString().Trim() + "']");
                    if (sprint != null)
                    {
                        manpower = Convert.ToDouble(sprint.Attributes["manpower"].Value);
                        target = Convert.ToDouble(sprint.Attributes["target"].Value);
                        manpowerSum += manpower;
                        manpowerGrandSum += manpower;
                        targetSum += target;
                        targetGrandSum += target;

                        // scratch columns to handle allocated work
                        row["workdays"] = row["days"];
                        row["workyears"] = row["manyears"];

                        // Deduct non allocated work
                        if (allocatedWork && manpower == 0)
                        {
                            // remove allocated work
                            workDaysToDeduct += Convert.ToDouble(row["days"].ToString());
                            manYearsToDeduct += Convert.ToDouble(row["manyears"].ToString());
                        }
                    }
                    else if (String.IsNullOrEmpty(row["parentversion"].ToString())) // grand summary row
                    {
                        manpower = manpowerGrandSum;
                        target = targetGrandSum;

                        // scratch columns to handle allocated work
                        row["workdays"] = Convert.ToDouble(row["days"].ToString()) - workDaysToDeduct;
                        row["workyears"] = Convert.ToDouble(row["manyears"].ToString()) - manYearsToDeduct;
                    }
                    else // sprint summary row
                    {
                        manpower = manpowerSum;
                        manpowerSum = 0;
                        target = targetSum;
                        targetSum = 0;

                        // scratch columns to handle allocated work
                        row["workdays"] = row["days"];
                        row["workyears"] = row["manyears"];
                    }

                    // If table is for allocated work then display. Otherwise, remove row
                    if ((allocatedWork && manpower > 0) || (!allocatedWork && manpower == 0))
                    {
                        // Set values in data table
                        row["manpower"] = manpower;
                        row["target"] = target;
                        row["execution_rate"] = Math.Round(Convert.ToDouble(row["workyears"].ToString()) / manpower, 2);
                        row["target_load"] = Math.Round(Convert.ToDouble(row["workyears"].ToString()) / target, 2);
                    }
                    else
                    {
                        row.Delete();
                    }
                }
                table.AcceptChanges();
                return table;
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                    cn.Close();
            }
        }


        public IEnumerable<RoadMapVersion> GetRoadMapVersions(string projectId, string userName)
        {
            string strSQL = @"select sum(remaining), sum(actual), vname, description, versionid, released, releasedate from (
                select pkey, project, remaining, actual, vt.vname, v.sequence, description, versionid, released, releasedate from (
                select i.pkey, nvl(f2.vname, f1.vname) vname, nvl(f2.versionid, f1.versionid) versionid, round(timeestimate/28800) remaining, round(timespent/28800) actual, assignee
                from issuefixversionview f1, issuefixversionview f2, issueparentview p, jiraissue i
                where i.pkey = f1.pkey
                {0}
                and i.pkey = p.child_key (+)
                and p.parent_key = f2.pkey(+)
                ) vt, projectversion v
                where vt.versionid = v.id(+) and project = :projectId 
                )  
                where released is null
                group by vname, description,versionid, sequence, released, releasedate
                order by sequence";
            strSQL = String.Format(strSQL, (!String.IsNullOrEmpty(userName) ? "and i.assignee = :userName" : ""));

            OracleConnection cn = new OracleConnection(_connectionString);
            OracleCommand cmd = new OracleCommand(strSQL, cn);

            if (!String.IsNullOrEmpty(userName))
            {
                cmd.Parameters.Add(new OracleParameter() { ParameterName = ":userName", OracleDbType = OracleDbType.Varchar2, Value = userName });
            }
            cmd.Parameters.Add(new OracleParameter() { ParameterName = ":projectId", OracleDbType = OracleDbType.Int16, Value = projectId });
            cn.Open();

            List<RoadMapVersion> l = new List<RoadMapVersion>();
            RoadMapVersion v;
            using (OracleDataReader rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    v = new RoadMapVersion();
                    v.RemainingTime = rdr.IsDBNull(0) ? 0 : rdr.GetDecimal(0);
                    v.ActualTime = rdr.IsDBNull(1) ? 0 : rdr.GetDecimal(1);
                    v.VersionName = rdr.GetString(2);
                    v.VersionDescription = rdr.IsDBNull(3) ? "" : rdr.GetString(3);
                    v.VersionId = (Int32)rdr.GetDecimal(4);
                    v.Released = rdr.IsDBNull(5) ? false : true;
                    v.ReleaseDate = rdr.IsDBNull(6) ? DateTime.MinValue : rdr.GetDateTime(6); // DateTime.Parse("01-Jan-11");

                    l.Add(v);
                    // TODO: Change to better code?
                    // http://damieng.com/blog/2009/09/22/when-an-object-relational-mapper-is-too-much-datareader-too-little
                }
            }
            cn.Close();
            return l;
        }

        public void GetReopenRates(string projectId, ref List<ReopenRateVersion> list)
        {
            string strSQL;
            strSQL = @"SELECT i.pkey, count(*) cnt, V.VERSIONID, V.VNAME
                FROM changeitem, changegroup, jiraissue i, ISSUEFIXVERSIONVIEW v
                WHERE changeitem.field = 'status'
                    and v.pkey = i.pkey
                    and changegroup.issueid = i.id
                    AND changeitem.groupid = changegroup.id
                    AND changegroup.issueid = i.id
                    AND i.project = :projectId
                    and to_char(changeitem.oldstring) = 'Resolved'
                    and to_char(changeitem.newstring) = 'Reopened'
                Group By i.pkey, v.versionid, v.vname
                ";

            ReopenRateVersion v = null;
            OracleConnection cn = new OracleConnection(_connectionString);
            OracleCommand cmd = new OracleCommand(strSQL, cn);
            OracleParameter param;
            param = cmd.Parameters.Add(":projectId", OracleDbType.Int16);
            param.Value = projectId;
            cn.Open();

            using (OracleDataReader rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    string versionId = rdr.IsDBNull(2) ? "" : rdr.GetDecimal(2).ToString();
                    v = list.Find(i => i.VersionId == versionId);
                    if (v != null)
                    {
                        int cnt = Convert.ToInt16(rdr.GetDecimal(1));
                        if (cnt == 1)
                        {
                            v.ReopenOnce.Times++;
                            v.ReopenOnce.IssueKeys.Add(rdr.GetOracleString(0).ToString());
                        }
                        else if (cnt == 2)
                        {
                            v.ReopenTwice.Times++;
                            v.ReopenTwice.IssueKeys.Add(rdr.GetOracleString(0).ToString());
                        }
                        else if (cnt > 2)
                        {
                            v.ReopenMore.Times++;
                            v.ReopenMore.IssueKeys.Add(rdr.GetOracleString(0).ToString());
                        }
                    }
                }
            }
            cn.Close();
        }

        public static IEnumerable<Installation> GetInstallations(string project)
        {

            // Static method so init class
            init();

            if (String.IsNullOrEmpty(project))
                throw new Exception("No project specified");

            string query = HttpUtility.UrlEncode(String.Format("project = {0} and issueType = Installation and resolution = Unresolved and \"Start Date\" >= -14d", project));
            string fields = "key,resolutiondate,summary,components,customfield_10035,customfield_10036,assignee,customfield_10033,customfield_10021,customfield_10050,customfield_10070,customfield_10140,customfield_10091,customfield_10060,customfield_14800";
            dynamic resp = JiraRestCall("/search?maxResults=1000&fields=" + fields + "&jql=" + query);

            IEnumerable<Installation> installations;

            switch (project)
            {
                case "INST":
                    // Load valuable customers
                    List<string> valuableCustomers = System.IO.File.ReadAllLines(HttpContext.Current.Server.MapPath("~/Utilities/mvc.txt")).ToList();
                    installations = JiraRestIssuesToInstallationList(resp["issues"], valuableCustomers);

                    return installations;
                case "URM":
                    // Create object list
                    /*
                    installations = from i in issues
                                    select new Installation
                                    {
                                        key = i.key,
                                        summary = i.summary,
                                        link = String.Format("{0}{1}/{2}", _jiraPrefix, "browse", i.key),
                                        startDateText = GetCustomFieldValue(i.customFieldValues, "customfield_10035"),
                                        duration = (int)decimal.Parse(Util.nvl(GetCustomFieldValue(i.customFieldValues, "customfield_10036"), "1")),
                                        userName = i.assignee,
                                        customerCode = i.summary,
                                        serviceIncident = GetCustomFieldValue(i.customFieldValues, "customfield_10021"),
                                        installerName = ""
                                    };
                    */
                    return null;

            }
            return null;

        }

        public static IEnumerable<Installer> GetInstallers(string project)
        {
            // Static method so init class
            init();

            var installations = HttpRuntime.Cache.GetOrStore<IEnumerable<Installation>>("installations-" + project, 60,
                 () => GetInstallations(project));

            var installers = from c in installations
                             select new Installer
                             {
                                 userName = c.userName,
                                 displayName = c.displayName
                             };

            installers = installers.Distinct(new InstallerComparer()).OrderBy(i => i.userName);

            return installers;
        }

        public JiraSprint GetBugsForAgileSprint(string boardId, string sprintName)
        {
            dynamic resp;
            JiraSprint sprint = new JiraSprint();
            sprint.Name = sprintName;

            // Get Sprint Information
            resp = JiraRestCall(
                String.Format("rest/greenhopper/1.0/xboard/work/allData?rapidViewId={1}",
                boardId));

            foreach (dynamic sp in resp["sprintsData"]["sprints"])
            {
                if (sp["name"] == sprintName)
                {
                    sprint.StartDate = DateTime.Parse(sp["startDate"]);
                    sprint.EndDate = DateTime.Parse(sp["endDate"]);
                    sprint.Id = sp["id"];
                    break;
                }
            }

            if (sprint.StartDate == null)
                throw new Exception("Sprint not found");


            // Get issues for this version via REST
            resp = JiraRestCall(
                String.Format("rest/api/2/search?maxResults=100&jql=sprint={1}+and+issueType=Bug",
                sprint.Id));

            List<JiraIssue> jiraIssues = JiraRestIssuesToList(resp["issues"]);

            sprint.NumberOfIssues = jiraIssues.Count();

            // Can't find how to get non-working days out of the sprint API
            List<DateTime> nonWorkingDays = new DateTime[0].ToList();

            // Build Sprint Days
            sprint.Days = BuildSprintDayList(sprint.StartDate, sprint.EndDate, nonWorkingDays, jiraIssues);

            return sprint;
        }


        public JiraSprint GetBugsForSprint(string project, string versionId, string versionName)
        {

            JiraSprint sprint = new JiraSprint();
            sprint.Id = Int32.Parse(versionId);
            sprint.Name = versionName;

            // Get start and end dates from the GH configuration; DB only
            string sql = @"SELECT propertyvalue
                             FROM propertytext
                             WHERE ID = (
                             SELECT ID
                             FROM propertyentry
                             WHERE ENTITY_NAME = 'GreenHopper'
                             AND ENTITY_ID = (select project from projectversion where id = :versionId)
                             AND PROPERTY_KEY = 'CONFIGURATION' )";

            OracleConnection cn = new OracleConnection(_connectionString);
            OracleCommand cmd = new OracleCommand(sql, cn);

            OracleParameter param;
            param = cmd.Parameters.Add(":versionId", OracleDbType.Int16);
            param.Value = versionId;
            cn.Open();

            string ghXmlText = cmd.ExecuteScalar().ToString();

            XmlDocument ghXml = new XmlDocument();
            ghXml.LoadXml(ghXmlText);

            // Calculate start date
            XmlNode node;
            node = ghXml.SelectSingleNode(String.Format("/map/entry[string='BURNDOWN_START_DATE_{0}']", versionId));
            sprint.StartDate = Util.ConvertFromUnixTimestamp(Convert.ToDouble(node.SelectSingleNode("long").InnerText) / 1000);

            // Calculate end date
            node = ghXml.SelectSingleNode(String.Format("/map/entry[string='BURNDOWN_END_DATE_{0}']", versionId));
            sprint.EndDate = Util.ConvertFromUnixTimestamp(Convert.ToDouble(node.SelectSingleNode("long").InnerText) / 1000);

            #region broken
            /* Problem with rendering Chinese characters (such as those in DPS-11590). Replace with REST API */
            /*
            // Get issues for this version
            List<RemoteIssue> issues = _jss.getIssuesFromJqlSearch(_token, String.Format("project = {0} and fixVersion = '{1}' and issueType = Bug", project, versionName), 1000).ToList();
            IEnumerable<JiraIssue> jiraIssues = from i in issues
                                                select new JiraIssue
                                                {
                                                    IssueKey = i.key,
                                                    ResolutionDate = i.resolution == null ? DateTime.MaxValue : _jss.getResolutionDateById(_token, Int64.Parse(i.id))
                                                };
            */
            #endregion

            // Get issues for this version via REST
            dynamic resp = JiraRestCall(
                String.Format("rest/api/2/search?maxResults=100&jql=project={1}+and+fixVersion=\"{2}\"+and+issueType=Bug", 
                project, versionName));

            List<JiraIssue> jiraIssues = JiraRestIssuesToList(resp["issues"]);

            sprint.NumberOfIssues = jiraIssues.Count();

            // Calculate non working days
            node = ghXml.SelectSingleNode("/map/entry[string='NWDAYS']");
            string[] nwdays = node.SelectNodes("string")[1].InnerText.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            List<DateTime> nonWorkingDays = Array.ConvertAll<string, DateTime>(nwdays, delegate(string s) { return DateTime.ParseExact(s, "yyyy-M-d", System.Globalization.CultureInfo.CurrentCulture); }).ToList();

            // Build Sprint Days
            sprint.Days = BuildSprintDayList(sprint.StartDate, sprint.EndDate, nonWorkingDays, jiraIssues);

            return sprint;
        }

        /// <summary>
        /// Queries Jira database for scoping calculations
        /// </summary>
        /// <param name="projectId">Project Key</param>
        /// <returns>List of Scoping Versions</returns>
        public List<ScopingVersion> GetScoping(string projectId)
        {
            string strsql = @"SELECT v.vname,
                    sum(decode(i.issuetype,3, numbervalue,0)) taskdays,
                    sum(decode(i.issuetype,7, numbervalue,0)) storydays
                    FROM customfieldvalue c,
                    jiraissue i,
                    issuefixversionview v
                    WHERE i.id          = c.issue
                    AND i.project     = :projectid
                    AND i.id        = v.issueid
                    AND c.customfield =10132
                    and nvl(v.RELEASED,'false') <> 'true'
                    /* remove all migration tasks from calculation */
                    and i.id not in (select id from nodeassociation a, jiraissue i
                                    where a.association_type='IssueComponent'
                                    and a.source_node_id = i.id
                                    and a.sink_node_id in (
                                            select id from component where project = 10040 and instr(lower(cname),'migration') > 0) 
                                      )
                    GROUP BY v.vname, v.SEQUENCE
                    ORDER BY v.sequence";

            OracleConnection cn = new OracleConnection(_connectionString);
            OracleCommand cmd = new OracleCommand(strsql, cn);
            OracleParameter param;
            param = cmd.Parameters.Add(":projectId", OracleDbType.Int16);
            param.Value = projectId;

            List<ScopingVersion> l = new List<ScopingVersion>();
            ScopingVersion s;

            try
            {
                // Load manpower xml
                XmlDocument manpowerXml = new XmlDocument();
                manpowerXml.Load(HttpContext.Current.Server.MapPath("~/Utilities/manpower.xml"));

                cn.Open();

                using (OracleDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        s = new ScopingVersion(manpowerXml);
                        //s.ParentVersion = rdr.GetString(0);
                        s.Version = rdr.IsDBNull(0) ? "No Version" : rdr.GetString(0);
                        s.TaskDays = (Double)rdr.GetDecimal(1);
                        s.StoryDays = (Double)rdr.GetDecimal(2);
                        l.Add(s);

                        // TODO: Change to better code?
                        // http://damieng.com/blog/2009/09/22/when-an-object-relational-mapper-is-too-much-datareader-too-little
                    }
                }
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                    cn.Close();
            }


            return l;
        }

        public static string GetJiraProjectId(string project)
        {
            // Get Project ID
            List<RemoteProject> projects = HttpRuntime.Cache.GetOrStore<List<RemoteProject>>("projects", 3600, () => _jss.getProjectsNoSchemes(_token).ToList());
            RemoteProject proj = projects.Find(p => p.key == project);
            if (proj != null)
            {
                return proj.id;
            }
            return null;
        }

        #region Utilities

        private static string GetCustomFieldValue(RemoteCustomFieldValue[] customFieldValues, string fieldName)
        {
            foreach (RemoteCustomFieldValue cf in customFieldValues)
            {
                if (cf.customfieldId == fieldName)
                    return cf.values[0];
            }
            return "";
        }

        /// <summary>
        /// Builds a sprint day list based on the provided parameters
        /// </summary>
        /// <param name="startDate">Start day of the sprint</param>
        /// <param name="endDate">End day of the sprint</param>
        /// <param name="nonWorkingDays">List of non working days</param>
        /// <param name="jiraIssues">List of Jira Issues</param>
        /// <returns>List of sprint days</returns>
        private static List<JiraSprintDay> BuildSprintDayList(DateTime startDate, DateTime endDate, List<DateTime> nonWorkingDays, List<JiraIssue> jiraIssues)
        {

            List<JiraSprintDay> days = new List<JiraSprintDay>();

            // Loop through days and if <> weekend and if not in non-working-days, calculate number of issues resolved
            for (DateTime d = startDate; d <= endDate; d = d.AddDays(1))
            {
                // if not weekend and not a non working day
                if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday && !nonWorkingDays.Exists(n => n == d))
                {
                    JiraSprintDay day = new JiraSprintDay();
                    day.Day = d;
                    day.ResolvedIssues = d.Date > DateTime.Now.Date ? 0 : jiraIssues.Count(i => i.ResolutionDate.Date <= d);
                    days.Add(day);
                }
            }

            return days;
        }

        private static String GetRestField(dynamic fields, String field, bool combo = false)
        {

            dynamic value;
            if (fields.TryGetValue(field, out value))
            {
                if (combo)
                {
                    if (value != null) value = value["value"];
                }
                return Util.nvl(value, "");
            }
            else
            {
                return "";
            }
        }
        
        /// <summary>
        /// Converts a Jira REST issue list to a list of Installations Issues
        /// </summary>
        /// <param name="issues">JSON issue list obtained as the result of a Jira REST call</param>
        /// <returns>List of Installations</returns>
        private static List<Installation> JiraRestIssuesToInstallationList(dynamic issues, List<String> valuableCustomers)
        {
            List<Installation> installations = new List<Installation>();

            foreach (dynamic issue in issues)
            {
                installations.Add(new CustomerInstallation()
                {
                    key = issue["key"],
                    summary = GetRestField(issue["fields"], "summary"),
                    link = String.Format("{0}{1}/{2}", _jiraPrefix, "browse", issue["key"]),
                    startDateText = GetRestField(issue["fields"],"customfield_10035"),
                    duration = (int)decimal.Parse(Util.nvl(GetRestField(issue["fields"],"customfield_10036"), "1")),
                    userName = issue["fields"]["assignee"]["name"],
                    displayName = issue["fields"]["assignee"]["displayName"],
                    product = issue["fields"]["components"].Length > 0 ? issue["fields"]["components"][0]["name"] : "",
                    customerCode = issue["fields"]["summary"],
                    installTask = GetRestField(issue["fields"],"customfield_10033", true),
                    serviceIncident = GetRestField(issue["fields"],"customfield_10021"),
                    installerName = "",
                    dateApproved = GetRestField(issue["fields"],"customfield_10050", true) == "Yes",
                    valuedCustomer = valuableCustomers.Any(v => v == issue["fields"]["summary"]),
                    region = GetRestField(issue["fields"],"customfield_10070", true),
                    hosted = GetRestField(issue["fields"],"customfield_10140", true),
                    ryo = GetRestField(issue["fields"],"customfield_10091").Contains("RYO"),
                    security = GetRestField(issue["fields"],"customfield_10060"),
                    caseList = GetRestField(issue["fields"],"customfield_14800")

                });
            }

            return installations;
        }

        /// <summary>
        /// Converts a Jira REST issue list to a list of Jira Issues
        /// </summary>
        /// <param name="issues">JSON issue list obtained as the result of a Jira REST call</param>
        /// <returns>List of JiraIssues</JiraIssue></returns>
        private static List<JiraIssue> JiraRestIssuesToList(dynamic issues)
        {
            List<JiraIssue> jiraIssues = new List<JiraIssue>();

            foreach (dynamic issue in issues)
            {
                jiraIssues.Add(new JiraIssue()
                {
                    IssueKey = issue["key"],
                    ResolutionDate = issue["fields"]["resolutiondate"] == null ?
                    DateTime.MaxValue : DateTime.Parse(issue["fields"]["resolutiondate"])
                });
            }

            return jiraIssues;
        }

        /// <summary>
        /// Performs a REST API call with the provided URL
        /// </summary>
        /// <param name="uri">URL for the REST call</param>
        /// <returns>JSON response from the server</returns>
        private static dynamic JiraRestCall(string uri)
        {
            if (!uri.StartsWith("rest")) uri = _jiraRestPrefix + uri;
            uri = _jiraPrefix + uri;
            System.Web.Script.Serialization.JavaScriptSerializer json = new System.Web.Script.Serialization.JavaScriptSerializer();
            WebRequest req = WebRequest.Create(uri);
            // NetworkCredentials don't work because Jira doesn't do a 401 challenge
            // req.Credentials = new NetworkCredential(_jiraUsername, _jiraPassword);
            string credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(_jiraUsername + ":" + _jiraPassword));
            req.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
            WebResponse resp = req.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            string response = sr.ReadToEnd();

            return json.Deserialize<dynamic>(response);
        }

        #endregion


        #region mocks
        /*
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
        */
        #endregion

    }
}