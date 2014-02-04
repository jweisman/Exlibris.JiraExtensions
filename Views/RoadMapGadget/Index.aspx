<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<ExLibris.JiraExtensions.Models.RoadMapGadgetViewModel>" %>

<% int i = 0; %>
<link rel="stylesheet" type="text/css" href="Content/jira-global-static.css" />
<link rel="stylesheet" type="text/css" href="Content/gadget.css" />
<style type="text/css">
    body
    {
        color: #333333;
        font: 12px/1.4 arial,tahoma,verdana,sans-serif;
    }
    body, .gadget .footer
    {
        background: none repeat scroll 0 0 #FFFFFF !important;
    }
    .view
    {
        padding: 0.5em 1em;
    }
    table.rm
    {
        border-collapse: collapse;
        color: #333333;
        margin-top: 0.5em;
        width: 100%;
    }
    table.rm td
    {
        vertical-align: top;
    }
    table.rm tr.head td
    {
        padding-top: 0.5em;
    }
    table.rm tr.desc td
    {
        padding-bottom: 0.5em;
    }
    table.rm tr td.icon
    {
        padding-right: 0.3em;
        width: 16px;
    }
    table.rm tr td.version
    {
        width: 70%;
    }
    table.rm tr td.relDate
    {
        white-space: nowrap;
        width: 70px;
    }
    table.rm tr td.progress, table.rm tr td.progressDesc
    {
        text-align: right;
        width: 100px;
    }
    table.rm tr td.versionDesc
    {
        font-size: 0.9em;
        width: auto;
    }
    table.rm tr.overdue
    {
        background-color: #FFFFDD;
    }
    table.rm tr.overdue td.relDate
    {
        color: #CC0000;
        font-weight: bold;
    }
    .percentageGraph
    {
        float: right;
        font-size: 0.9em;
        overflow: hidden;
        width: 100px;
    }
    .percentageGraph .barSection
    {
        border-top: 13px solid #EEEEEE;
        display: block;
        height: 0;
    }
    .percentageGraph .barSection span
    {
        display: none;
    }
    .percentageGraph .barSection.resolved
    {
        border-color: #009900;
    }
    .percentageGraph .barSection.unresolved
    {
        border-color: #CC0000;
    }
</style>
<div class="gadget default">
    <div class="view">
        <table id="road-map-content" class="rm">
            <tbody>
                <% foreach (var v in Model.versions)
                   { %>
                <tr class="roadmap <%=i % 2 == 0 ? "evenRow" : "oddRow" %> <%= v.ReleaseDate < DateTime.Now && !v.Released ? "overdue firstoverdue" : "" %>">
                    <td class="icon first">
                        <img height="16" width="16" src="<%=v.IconUrl %>" title="<%=v.Released ? "Released" : "Unreleased" %>"
                            alt="<%=v.Released ? "Released" : "Unreleased" %>">
                    </td>
                    <td class="version">
                        <a href="<%=Model.project.url %>" target="_parent">Rosetta</a> : <a href="<%=Model.project.url + "/fixforversion/" + v.VersionId %>"
                            target="_parent" title="<%=v.VersionDescription %>">
                            <%=v.VersionName %></a>
                    </td>
                    <td class="relDate">
                        <%=v.ReleaseDate.ToShortDateString() %>
                    </td>
                    <td class="progress last">
                        <% if (v.TotalTime == 0)
                           { %>
                        <span class="subtext no-issues">No work planned.</span>
                        <%}
                           else
                           { %>
                        <div class="percentageGraph">
                            <a class="barSection resolved" title="Actual Time - <%=v.PercentComplete %>% (<%=v.ActualTime %> days)"
                                href="#" style="width: <%=v.PercentComplete %>%;" target="_parent"><span>Actual Time
                                    -
                                    <%=v.PercentComplete%>%</span></a> <a class="barSection unresolved" title="Remaining Time - <%=100-v.PercentComplete %>% (<%=v.RemainingTime %> days)"
                                        href="#" style="width: <%=100-v.PercentComplete %>%;" target="_parent"><span>Remaining
                                            Time -
                                            <%=100 - v.PercentComplete%>%</span></a></div>
                        <%} %>
                    </td>
                </tr>
                <tr class="roadmap <%=i % 2 == 0 ? "evenRow" : "oddRow" %> desc <%= v.ReleaseDate < DateTime.Now && !v.Released  ? "overdue  lastOverdue" : "" %>">
                    <td class="icon first">
                    </td>
                    <td class="versionDesc" colspan="2">
                        <%=v.VersionDescription %>
                    </td>
                    <td class="progressDesc last">
                        <% if (v.TotalTime == 0)
                           { %>
                        &nbsp;
                        <%}
                           else
                           { %>
                        <span class="small nowrap">
                            <%=v.ActualTime%>
                            of
                            <%=v.TotalTime%>
                            days worked</span>
                        <%} %>
                    </td>
                </tr>
                <% i++;
               } %>
            </tbody>
        </table>
    </div>
    <div id="config" style="display: none;">
    </div>
</div>
