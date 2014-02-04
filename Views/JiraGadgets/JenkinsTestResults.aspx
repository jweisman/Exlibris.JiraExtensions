<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<ExLibris.JiraExtensions.Models.JenkinsTestResultViewModel>" %>

<%
    bool wallboard = (Request.UrlReferrer != null && Request.UrlReferrer.ToString().ToLower().Contains("wallboard")) || Request.QueryString["wallboard"] != null;
 %>
<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
    <title>Jenkins Test Results for <%=Model.Title %></title>
    <script src="http://code.jquery.com/jquery-latest.js"></script>
    <script type="text/javascript" src="https://www.google.com/jsapi"></script>
    <script type="text/javascript">
        google.load("visualization", "1", { packages: ["corechart"] });
        google.setOnLoadCallback(drawChart);

        function drawChart() {
            var data = new google.visualization.DataTable();
            data.addColumn('string', 'Date');
            data.addColumn('number', 'Fail');
            data.addColumn('number', 'Pass');
            data.addRows([
            <%
            // Add default if there is only one test result
            if (Model.TestResults.Count == 1)
            {
                Response.Write("['New', 0, 0],");
            }
            
            for ( int n=0; n<Model.TestResults.Count; n++)
            { 
                var r = Model.TestResults[n];
                if (n!=0)
                    Response.Write(",");
                Response.Write(String.Format("['{0}', {1}, {2}]", String.Format("{0} (#{1})", r.TestDate.ToString("d MMM"), r.BuildNumber), r.TestsFailed, r.TestsPassed));
            }
            %>
        ]);

            var chart = new google.visualization.AreaChart(document.getElementById('chart_div'));

            chart.draw(data, { width: $(window).width(), height:$(window).height(), title: '<%=Model.Title %>', isStacked: true,
                colors: ['red', 'green'], legend: 'none', chartArea: { left: 50, top: 50 },
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
