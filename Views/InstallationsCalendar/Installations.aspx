<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<ExLibris.JiraExtensions.Models.CalendarViewModel>" %>
<%@ Import Namespace="ExLibris.JiraExtensions.Utilities" %>

<%
    
    DateTime startDate = DateTime.Now;
    DateTime endDate = startDate.AddMonths(9);
    startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day);
    endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day);
    int currentMonth=0;
    int currentInstallation =0;
    int freeDay = 0;
    bool weekday = false;
           
    // initialize installation list- find first relevant installation
    ExLibris.JiraExtensions.Models.Installation install;
    install = Model.installations[currentInstallation];
    while (install.startDate < startDate)
    {
        // if installation is still open, add it to installer's list
        if (Model.installations[currentInstallation].endDate >= startDate)
        {
            Model.installers.Find(i => i.userName == Model.installations[currentInstallation].userName).openInstallations.Add(install);
        }
        currentInstallation++;
        install = Model.installations[currentInstallation];
    }
    
     %>

<html>
<head>
    <title>Installations Availability</title>
    <script type="text/javascript" src="../Scripts/jquery-1.4.1.min.js"></script>
    <link rel="stylesheet" type="text/css" href="<%=Url.Content("~/Content/calendar.css") %>" />
    <style>
        .selected {background-color: red};
    </style>
</head>
<body style="font-family: Arial; font-size: 10pt;">
    <img src="http://il-iapps04:8181/s/522/3/_/images/exl/company_logo.gif" /><h1>ExLibris Installations Availability</h1>
    <table border="0">
    <tr>
    <td>Instructions:<br />
    <i>Double-click cell to create new installation<br />Alt-n to find next open slot</i></td>
    <td>Key:<br />
    <span style='background-color:#FFCC00'>Preferred Customer</span><br />
    <span style='background-color:#66CC00'>Internal Customer</span>
    </td>
    </tr>
    </table>
    <p>&nbsp;</p>
    <table>
        <tr>
           <th>&nbsp;</th>
            <th>&nbsp;</th>
            <% foreach (var i in Model.installers) {%>
                <th><%=i.displayName %></th>
                <%} %>
        </tr>
        <%  for (DateTime date = startDate; date <= endDate; date=date.AddDays(1)) { %>
                <tr>
            <!--<% if (currentMonth != date.Month) { %>
                <td rowspan="<%=DateTime.DaysInMonth(date.Year, date.Month) - date.Day + 1 %>" class="monthyr"><%=date.ToString("MMM") %></td>
                <% currentMonth = date.Month; %>
            <%} %>-->
            <td><%=date.ToString("MMM") %></td>
            <td><%=date.Day + " (" + date.ToString("ddd") + ")"%></td>
            <% for (var i = 0; i < Model.installers.Count; i ++ )
               {
                   // If the installer has an open installation, or if the next install belongs to this installer, then write the installation
                   if (Model.installers[i].openInstallations.Count > 0 || 
                       (install != null && install.userName == Model.installers[i].userName 
                            && (install.startDate <= date && install.endDate >= date) ))
                   {
                       Response.Write("<td class='busy'");
                       Response.Write(Html.NewInstallLink(date, Model.installers[i].userName,Model.projectId)+">");
                       // Write name if saturday
                       if (date.DayOfWeek == DayOfWeek.Saturday)
                        {
                            Response.Write(Model.installers[i].displayName + "<br/>");
                        }
                       
                       // Include open installations
                       for (var x = 0; x < Model.installers[i].openInstallations.Count; x++) 
                       {
                           var openInstall = Model.installers[i].openInstallations[x];

                           Response.Write(Html.InstallHtml(openInstall));

                           if (openInstall.startDate.AddDays(openInstall.duration - 1) <= date)
                           {
                               Model.installers[i].openInstallations.RemoveAt(x);
                               x = x - 1; // go back one because we just removed
                           }
                       }
                       
                       // Write today's installations
                       while (install != null && install.startDate <= date 
                           && install.userName == Model.installers[i].userName)
                       {
                           Response.Write(Html.InstallHtml(install));
                           // if installation lasts several days, save
                           if (install.startDate.AddDays(install.duration - 1) > date)
                               Model.installers[i].openInstallations.Add(install);
                           
                           currentInstallation++;
                           // if there are more installations, move forward
                           if (Model.installations.Count > currentInstallation)
                               install = Model.installations[currentInstallation];
                           else // no more installations
                           {
                               install = null;
                               break;
                           }
                       }
                       Response.Write("</td>");
                       
                   }
                   else // no installations today for the installer
                   {
                       Response.Write("<td ");
                       weekday = false;
                       if (Model.installers[i].weekend.IndexOf(date.DayOfWeek.ToString()) >= 0)
                           Response.Write(" class='weekend'");
                       else if (Model.installers[i].holidays.Any(h => h.date == date))
                           Response.Write(" class='weekend'");
                       else
                           weekday = true;
                       Response.Write(Html.NewInstallLink(date, Model.installers[i].userName, Model.projectId) + ">");
                       if (weekday)
                       {
                           Response.Write(String.Format("<a name='free{0}'/>", freeDay + 1));
                           freeDay++;
                       }
                       if (date.DayOfWeek == DayOfWeek.Saturday)
                            Response.Write(Model.installers[i].displayName + "<br/>");
                       if (Model.installers[i].holidays.Any(h => h.date == date))
                           Response.Write(String.Format("<span style='font-weight: bold; color: purple'>{0}</span>", Model.installers[i].holidays.Single(h => h.date == date).name));
                       Response.Write("&nbsp;</td>");
                   }
                   %>
            <%} %>
            </tr>
        <% } %>

    </table>
<script type="text/javascript">
var nextOpen = 1;
var isAlt = false;
$(document).keyup(function (e) {
    if (e.which == 18) isAlt = false;
}).keydown(function (e) {
    if (e.which == 18) isAlt = true;
    if (e.which == 78 && isAlt == true) {
        prevOpen = nextOpen - 1;
        $("a[name=free" + prevOpen + "]").parent().removeClass("selected");
        window.location.hash = 'free' + nextOpen;
        $("a[name=free" + nextOpen + "]").parent().addClass("selected");
        nextOpen++;
        return false;
    }
}); 

</script>
</body>
</html>

