<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<IEnumerable<ExLibris.JiraExtensions.Models.ScopingVersion>>" %>
<%@ Import Namespace="ExLibris.JiraExtensions.Utilities" %>

<html>
<head>
<title>Scoping Report</title>
<style type="text/css">
body, p, td, th 
{
     font-family:Arial;
     font-size:12px;
}

td.summary { font-weight:bold }

</style>
</head>

<body>
<img src="http://il-iapps04:8181/s/587/5/_/images/exl/company_logo.gif" />
<h1>Scoping Report for <%=ViewData["projectName"] %></h1>

<table border=1 cellpadding=3 cellspacing=2>
    <tr><th>PR</th><th>Sprint</th><th>Manpower</th><th>Manpower Years</th><th>Total Days</th><th>Load %</th><th>Task Days</th><th>Task %</th><th>Story Days</th><th>Story %</th></tr>
    <% foreach( ExLibris.JiraExtensions.Models.ScopingVersion s in Model ) {     %>
        <tr>
            <td><%= s.ParentVersion %></td>
            <td><%= s.Version %></td>
            <td><%= s.Manpower %></td>
            <td><%= s.ManpowerYears %></td>
            <td><%= s.TotalDays %></td>
            <td><%= s.Load.ToString("P0") %></td>
            <td><%= s.TaskDays %></td>
            <td><%= s.TaskPercent.ToString("P0") %></td>
            <td><%= s.StoryDays %></td>
            <td><%= s.StoryPercent.ToString("P0") %></td>
        </tr>
    <% } %>
</table>
</body>
</html>