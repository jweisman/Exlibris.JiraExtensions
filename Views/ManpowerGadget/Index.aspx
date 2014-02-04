<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<ExLibris.JiraExtensions.Models.ManpowerViewModel>" %>
<%@ Import Namespace="ExLibris.JiraExtensions.Utilities" %>

<html>
<head>
<title>Manpower Report</title>
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
<h1>Manpower Report for <%=ViewData["projectName"] %></h1>
<h2>Allocated Work</h2>
<table border=1 cellpadding=3 cellspacing=2>
    <tr><th>PR</th><th>Sprint</th><th>Work Days</th><th>Man Years</th><th>Target</th><th>Target Load</th><th>Manpower</th><th>Execution Rate</th></tr>
    <% foreach( System.Data.DataRow row in Model.AllocatedWork.Rows ) {

           string cssClass = String.IsNullOrEmpty(row["childversion"].ToString()) ? "summary" : "";
     %>
        <tr>
            <td class="<%=cssClass %>"><%= Util.nvl(row["parentversion"].ToString(),"TOTAL") %></td>
            <td class="<%=cssClass %>"><%= Util.nvl(row["childversion"].ToString(), "Sub-Total") %></td>
            <td class="<%=cssClass %>"><%= row["workdays"] %></td>
            <td class="<%=cssClass %>"><%= row["workyears"] %></td>
            <td class="<%=cssClass %>"><%= row["target"] %></td>
            <td class="<%=cssClass %>"><%= Convert.ToDouble(row["target_load"].ToString()).ToString("P0") %></td>
            <td class="<%=cssClass %>"><%= row["manpower"] %></td>
            <td class="<%=cssClass %>"><%= Convert.ToDouble(row["execution_rate"].ToString()).ToString("P0") %></td>
        </tr>
    <% } %>
</table>
<h2>Non-Allocated Work</h2>
<table border=1 cellpadding=3 cellspacing=2>
    <tr><th>PR</th><th>Sprint</th><th>Work Days</th><th>Man Years</th></tr>
    <% foreach( System.Data.DataRow row in Model.NonAllocatedWork.Rows ) {

           string cssClass = String.IsNullOrEmpty(row["childversion"].ToString()) ? "summary" : "";
     %>
        <tr>
            <td class="<%=cssClass %>"><%= Util.nvl(row["parentversion"].ToString(),"TOTAL") %></td>
            <td class="<%=cssClass %>"><%= Util.nvl(row["childversion"].ToString(), "Sub-Total") %></td>
            <td class="<%=cssClass %>" style="color:Red">-<%= row["workdays"] %></td>
            <td class="<%=cssClass %>" style="color:Red">-<%= row["workyears"] %></td>
        </tr>
    <% } %>
</table>
</body>
</html>