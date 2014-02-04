<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<List<ExLibris.JiraExtensions.Models.ReopenRateVersion>>" %>
<%@ Import Namespace="ExLibris.JiraExtensions.Utilities" %>
<%@ Import Namespace="Newtonsoft.Json.Linq" %>


<%
    string data="t:";
    string titles="0:";
    string imageUrl = "http://chart.apis.google.com/chart?cht=bvs&chs=550x275&chco=FFFF33,FF9933,CC0033&chd={0}&chxt=x,y&chxs=0,ff0000,12,0,lt|1,0000ff,10,1,lt&chxl={1}&chbh=a,20&chds=0,15&chxr=1,0,15&chdl=Once|Twice|2%2b+Times&chma=20,20,20,20|80,20&chxs=1,000000|0,000000&chm=N,333333,0,,10,,c|N,ffffff,1,,10,,c|N,ffffff,2,,10,,c";
    string d1="", d2="", d3="";
    string map="";

    foreach (var v in Model)
    {
        d1 += (v.ReopenOnce.Times == 0 ? -1 : v.ReopenOnce.Times) + ","; d2 += v.ReopenTwice.Times + ","; d3 += v.ReopenMore.Times + ",";
        titles += "|" + Server.HtmlEncode(v.VersionName);
    }

    data += d1.TrimEnd(new char[] {','}) + "|" + d2.TrimEnd(new char[] {','}) + "|" + d3.TrimEnd(new char[] {','});

    imageUrl = String.Format(imageUrl, data, titles);

    JObject mapJson = Util.GetJson(imageUrl + "&chof=json");
    foreach (JObject m in mapJson["chartshape"].Children()) 
    {
        if (m.Property("name").Value.ToString().StartsWith("\"bar")) 
        {
            var v = Model[Convert.ToInt32(m.Property("name").Value.ToString().Substring(6, 1))];
            string url="";
            switch (m.Property("name").Value.ToString().Substring(4, 1))
            {
                case "0":
                    url = String.Join(",", v.ReopenOnce.IssueKeys);
                    break;
                case "1":
                    url = String.Join(",", v.ReopenTwice.IssueKeys);
                    break;
                case "2":
                    url = String.Join(",", v.ReopenMore.IssueKeys);
                    break;
            }
            url = "http://il-iapps04:8181/secure/IssueNavigator.jspa?reset=true&jqlQuery=issuekey+in+%28" + url + "%29";
            map +=  String.Format("<area name={0} shape={1} coords={2} href='{3}' target='_blank' title=''>",
                m.Property("name").Value.ToString(), m.Property("type").Value.ToString(), String.Join(",",m.Property("coords").Value), url);
            
            
        }
    }
    
    
    
    
     %>


<div>
<img alt="chart" src="<%=imageUrl %>" usemap="#reopenMap" border="0"/>
</div>

<map name="reopenMap">
<%=map %>
</map>