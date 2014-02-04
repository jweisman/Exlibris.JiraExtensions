<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<ExLibris.JiraExtensions.Models.ErrorInfoViewModel>" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Error</title>
</head>
<body>
    <div style="color:Red">
        <% if (String.IsNullOrEmpty(Model.ErrorMessage))
           {%>
            <p>An error has occurred. </p>
        <%}
           else
           { %>
        <p><%=Html.Encode(Model.ErrorMessage)%></p>
        <%} %>
    </div>
    <script>
        setTimeout( "window.location.reload( true );", <%=Model.Refresh * 60 %>*1000 );
    </script>
</body>
</html>
