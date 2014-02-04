<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<html xmlns="http://www.w3.org/1999/xhtml" >
<!-- This page uses the jQuery Full Calendar plugin, http://arshaw.com/fullcalendar/ -->
<head>
    <title>Travel Calendar</title>
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
                    height: $(window).height(),
                    events: "<%=ResolveUrl("~/JiraGadgets/GetTravel") + "?projectKey=" + Request.QueryString["projectKey"]%>",
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
		    //width: 1000px;
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
<h1 style="font-family: Calibri, Verdana;">Travel Calendar</h1>
<div id="calendar" style="text-align:center"></div>
</div>
<p>&nbsp;</p>
</body>
</html>
