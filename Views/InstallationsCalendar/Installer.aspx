<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<IEnumerable<ExLibris.JiraExtensions.Models.Installer>>" %>

<% 
    string displayName = ViewData["displayName"]==null ? "" : ViewData["displayName"] + "'s";
%>
<html xmlns="http://www.w3.org/1999/xhtml" >
<!-- This page uses the jQuery Full Calendar plugin, http://arshaw.com/fullcalendar/ -->
<head>
    <title><%=displayName%> Installation Calendar</title>
        <link href="../../Content/fullcalendar.css" rel="stylesheet" type="text/css" />
        <link href="../../Content/redmond/theme.css" rel="stylesheet" type="text/css" />
        <script type="text/javascript" src="../../Scripts/jquery-1.4.1.min.js"></script>
        <script type="text/javascript" src="../../Scripts/fullcalendar.min.js"></script>
        <script type="text/javascript" src="../../Scripts/jquery-ui-custom.js"></script>
        <script type="text/javascript">
            $(document).ready(function () {
                $('#calendar').fullCalendar({
                    theme: true,
                    defaultView: 'month',
                    editable: false,
                    events: "<%=ResolveUrl("~/InstallationsCalendar/GetInstallations")%>",
                    eventClick: function (event) {
                        if (event.url) {
                            window.open(event.url,"installation");
                            return false;
                        }
                    }
                });
            }); 
        </script>
        <style type="text/css">
        #calendar {
		    width: 1000px;
		    margin: 0 auto;
		}
		.valuedCustomer  
		{
		    border-color: red;
		    border-width: 3px;
        }
		.fc-event-title { color: white;
		       }
		       
		a.content { color: inherit}

        </style>

</head>
<body>
<div style="text-align:center"><img alt="ExLibris" src="https://jira.exlibrisgroup.com/s/en_US-vyfjb7-1988229788/6258/5/_/jira-logo-scaled.png" /><h1 style="font-family: Calibri, Verdana;"><%=displayName%> Installation Calendar</h1></div>
<%if (!String.IsNullOrEmpty(displayName))
  { %>
<div style="text-align:center">
<div id="calendar" style="text-align:center"></div>
</div>
<%} %>
<p>&nbsp;</p>
<div style="text-align:center"><%= Html.DropDownList("userName", new SelectList(Model, "userName", "displayName"), "Select Installer", new { onchange = "redirect(this.options[selectedIndex].value)" })%></div>
<script>
function redirect(dropDownValue) {
    window.location.href = "<%=ResolveUrl("~/InstallationsCalendar/Installer/")%>"+ dropDownValue;
}
</script>
</body>
</html>
