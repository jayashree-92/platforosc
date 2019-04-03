<!-- This sample JSP is used to invalidate the session before logout from the SiteMinder. -->
<!-- This JSP page may be a protected resource on application server in order to invalidate -->
<!-- the protected users session object. -->

<html>
  <head>
	<title>Sample Logout</title>

  </head>

<body >
  <font color="#376633" face="Arial,Helvetica">
	 <h1>Sample logout.jsp</h1>
	 <hr>
  </font>
  <br>
  <br>

<!-- Invalidate the users session object if it exists. -->
<%
	HttpSession sess = request.getSession(false);
	if (sess != null) {
		sess.invalidate();
	}

%>


<!-- After successful session invalidate redirect to logout html page for logout from SiteMinder. -->
<%
	response.sendRedirect("/logout.html");

%>

<br>
<br>

</body>
</html>
