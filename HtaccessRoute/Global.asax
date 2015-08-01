<%@ Application Language="C#" %>

<script runat="server">

    string DEFAULT_PAGE = "~/index.html";

    void Application_Start(object sender, EventArgs e) 
    {
        // Code that runs on application startup
        System.Web.Routing.RouteTable.Routes.MapPageRoute("DefaultPage", "", DEFAULT_PAGE);
        System.Web.Routing.RouteTable.Routes.Add("DefaultRoute", new System.Web.Routing.Route("{page}", RoutingHandler.LoadFromHtaccess(DEFAULT_PAGE)));
    }
    
    void Application_End(object sender, EventArgs e) 
    {
        //  Code that runs on application shutdown

    }
        
    void Application_Error(object sender, EventArgs e) 
    { 
        // Code that runs when an unhandled error occurs

    }

    void Application_BeginRequest(object sender, EventArgs e)
    {
        //HttpApplication httpApplication = (HttpApplication)sender;
    }

    void Application_EndRequest(object sender, EventArgs e)
    {
        HttpApplication httpApplication = (HttpApplication)sender;
        string physicalPath = httpApplication.Context.Request.PhysicalPath;
        if (!System.IO.File.Exists(physicalPath))
        {
            RoutingHandler routingHandler = null;

            string applicationPath = httpApplication.Server.MapPath(httpApplication.Request.ApplicationPath).TrimEnd(System.IO.Path.DirectorySeparatorChar);
            string directory = physicalPath.TrimEnd(System.IO.Path.DirectorySeparatorChar);
            do
            {
                if (System.IO.Directory.Exists(directory))
                {
                    string htaccessPath = System.IO.Path.Combine(directory, ".htaccess");
                    if (System.IO.File.Exists(htaccessPath))
                    {
                        routingHandler = RoutingHandler.LoadFromHtaccess(DEFAULT_PAGE, htaccessPath);
                        break;
                    }
                }

                if (directory == applicationPath) break;
                directory = System.IO.Path.GetDirectoryName(directory).TrimEnd(System.IO.Path.DirectorySeparatorChar);
                
            } while (!System.IO.Directory.Exists(directory));

            if (routingHandler == null && System.Web.Routing.RouteTable.Routes.Count > 0)
            {
                System.Web.Routing.Route route = (System.Web.Routing.Route)System.Web.Routing.RouteTable.Routes["DefaultRoute"];
                routingHandler = (RoutingHandler)route.RouteHandler;
            }

            if (routingHandler != null)
            {
                routingHandler.GetHttpHandler(httpApplication.Context.Request).ProcessRequest(httpApplication.Context);
            }
        }
    }

    void Session_Start(object sender, EventArgs e) 
    {
        // Code that runs when a new session is started

    }

    void Session_End(object sender, EventArgs e) 
    {
        // Code that runs when a session ends. 
        // Note: The Session_End event is raised only when the sessionstate mode
        // is set to InProc in the Web.config file. If session mode is set to StateServer 
        // or SQLServer, the event is not raised.

    }
       
</script>
