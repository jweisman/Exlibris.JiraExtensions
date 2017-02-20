using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using System.Xml.Linq;
using System.Net;
using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ExLibris.JiraExtensions.Models;

namespace ExLibris.JiraExtensions.Utilities
{
    public static class Util
    {
        #region Generic Utilities
        public static XDocument TransformXML(XDocument doc, string xslPath)
        {
            XDocument newdoc = new XDocument();

            using (XmlWriter writer = newdoc.CreateWriter())
            {
                // Load the style sheet.
                XslCompiledTransform xslt = new XslCompiledTransform();
                xslt.Load(XmlReader.Create(xslPath));

                // Execute the transform and output the results to a writer.
                xslt.Transform(doc.CreateReader(), writer);
            }

            return newdoc;
            /*
            XslCompiledTransform transform = new XslCompiledTransform();
            using (XmlReader reader = XmlReader.Create(xslPath))
            {
                transform.Load(reader);
            }
            StringWriter results = new StringWriter();
            transform.Transform(doc, null, results);
            var newdoc = new XmlDocument();
            newdoc.LoadXml(results.ToString());
            return newdoc;
             * */
        }

        public static string nvl(object val, string valifnull)
        {
            if (val == null )
                return valifnull;

            String str = val.ToString();
            if (String.IsNullOrEmpty(str))
                return valifnull;
            else
                return str;
        }


        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = DateTime.SpecifyKind(new DateTime(1970, 1, 1, 0, 0, 0, 0), DateTimeKind.Utc);
            return origin.AddSeconds(timestamp).ToLocalTime();
        }

        public static JObject GetJson(string url)
        {
            string result;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                result = reader.ReadToEnd();
            }
            JObject o = JObject.Parse(result);
            return o;
        }

        public static string SItoRecordId(string si)
        {
            /*
             0x0040000000030B67 = 16384-199527
             0x004000 ="00"+ hex(16384)
                +
                00
                +
             0x00030B67="000"+hex(199527)
            */
            string recordId = "";
            if (!String.IsNullOrEmpty(si))
            {
                int si1, si2;
                string[] siparts = si.Split("-".ToCharArray());
                if (siparts.Length == 1) // one part SI number
                {
                    if (Int32.TryParse(siparts[0], out si1))
                    {
                        recordId = String.Format("0x{0}", si1.ToString("X").PadLeft(16, Convert.ToChar("0")));
                    }
                }
                else if (siparts.Length == 2) // two part SI number
                {
                    if (Int32.TryParse(siparts[0], out si1) && Int32.TryParse(siparts[1], out si2))
                    {
                        recordId = String.Format("0x{0}{1}", si1.ToString("X").PadLeft(6, Convert.ToChar("0")),
                                si2.ToString("X").PadLeft(10, Convert.ToChar("0")));
                    }
                }
            }

            return recordId;
        }

        public static string SIUrl(string si)
        {
            string hexsi = SItoRecordId(si);
            if (!String.IsNullOrEmpty(hexsi))
                return ConfigurationManager.AppSettings["pivotalUrl"] + hexsi;
            else
                return "";
        }

        /// <summary>
        /// Returns case URL
        /// </summary>
        /// <param name="caseNumber"></param>
        /// <returns></returns>
        public static string CaseUrl(string caseNumber)
        {
            if (!String.IsNullOrEmpty(caseNumber))
                return ConfigurationManager.AppSettings["SFUrl"] + caseNumber;
            else
                return "";
        }

        /// <summary>
        /// Returns case URL for the first case in the list
        /// </summary>
        /// <param name="caseNumber"></param>
        /// <returns></returns>
        public static string CaseUrl(string[] caseNumber)
        {
            if (caseNumber != null)
                return CaseUrl(caseNumber[0]);
            else
                return "";
        }

        #endregion


    }
}