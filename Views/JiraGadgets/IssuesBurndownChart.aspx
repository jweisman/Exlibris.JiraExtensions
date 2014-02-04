<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<ExLibris.JiraExtensions.Models.IssuesBurndownChartViewModel>" %>

<%
    bool wallboard = (Request.UrlReferrer != null && Request.UrlReferrer.ToString().ToLower().Contains("wallboard")) || Request.QueryString["wallboard"] != null;
    var sprint = Model.Sprint;
 %>
<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
    <title><%=Model.Title %></title>
    <script src="http://code.jquery.com/jquery-latest.js"></script>
    <script type="text/javascript" src="https://www.google.com/jsapi"></script>
    <script type="text/javascript">
      google.load("visualization", "1", {packages:["corechart"]});
      google.setOnLoadCallback(drawChart);
      function drawChart() {
        var data = new google.visualization.DataTable();
        data.addColumn('string', 'Date');
        data.addColumn('number', 'Baseline');
        data.addColumn('number', 'Remaining');
        data.addRows(<%=sprint.Days.Count + 2 %>);
        data.setValue(0, 0, '');
        data.setValue(0, 1, <%=sprint.NumberOfIssues %>);
        data.setValue(0, 2, <%=sprint.NumberOfIssues %>);

           <%for ( int n=1; n<=sprint.Days.Count; n++)
            { 
                var r = sprint.Days[n-1];
                Response.Write("\n// r.Day=" + r.Day + "; DateTime.Now.Date=" + DateTime.Now.Date + "\n");
                Response.Write(String.Format("data.setValue({0}, 0, '{1}');", n, r.Day.ToString("d MMM")));
                Response.Write(String.Format("data.setValue({0}, 1, {1});", n, sprint.NumberOfIssues - (sprint.NumberOfIssues/sprint.Days.Count) * n));
                if (r.Day < DateTime.Now.Date)
                    Response.Write(String.Format("data.setValue({0}, 2, {1});", n, sprint.NumberOfIssues - r.ResolvedIssues));
            }
            %>

        data.setValue(<%=sprint.Days.Count + 1 %>, 0, '');
        data.setValue(<%=sprint.Days.Count + 1 %>, 1, 0);
        //data.setValue(0, 2, <%=sprint.NumberOfIssues %>);

        var chart = new google.visualization.LineChart(document.getElementById('chart_div'));
        chart.draw(data, { width: $(window).width(), height:$(window).height(), title: '<%=Model.Title %>', 
                colors: ['grey', 'red'], legend: 'none', chartArea: { left: 50, top: 50 },
                titleTextStyle: { color: 'grey', fontSize: 24 }
                <%=wallboard ?  @", hAxis: { textStyle: { color: 'white' }, titleTextStyle: { color: 'white' } }, 
                                  vAxis: { textStyle: { color: 'white' }, titleTextStyle: { color: 'white'} }, 
                                  backgroundColor: 'black'" : ""%>
            });

         setTimeout( "<%=wallboard && !Request.Url.ToString().ToLower().Contains("wallboard") ? "window.location.href='" + Request.Url.ToString() + "&wallboard=true';" : "window.location.reload( true );"%>", <%=Model.Refresh * 60 %>*1000 );


        }
    </script>

</head>
<body>
    <div id="chart_div"></div>
</body>
</html>
