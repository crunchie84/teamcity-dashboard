using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ITCloud.Web.Routing;
using log4net;

namespace TeamCityDashboard
{
  public class TeamCityDashboardApplication : System.Web.HttpApplication
  {
    private static readonly ILog log = LogManager.GetLogger(typeof(TeamCityDashboardApplication));

    public static void RegisterGlobalFilters(GlobalFilterCollection filters)
    {
      filters.Add(new HandleErrorAttribute());
    }

    public static void RegisterRoutes(RouteCollection routes)
    {
      routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

      // discover UrlRoute attributes on MVC controller classes in the project
      routes.DiscoverMvcControllerRoutes();

      //routes.MapRoute(
      //    "Default", // Route name
      //    "{controller}/{action}/{id}", // URL with parameters
      //    new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
      //);

    }

    protected void Application_Start()
    {
      log4net.Config.XmlConfigurator.Configure();

      log.Info("Teamcity dashboard starting...");

      string version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
      if (string.IsNullOrWhiteSpace(version))
        version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
      log.InfoFormat("Current version: '{0}'", version);

      AreaRegistration.RegisterAllAreas();
      
      RegisterGlobalFilters(GlobalFilters.Filters);
      RegisterRoutes(RouteTable.Routes);

      log.Info("Application started");
    }

    protected void Application_Error(object sender, EventArgs e)
    {
      if (log.IsErrorEnabled)
      {
        HttpApplication application = (HttpApplication)sender;
        if (application.Context == null)
          return;

        var exception = application.Context.Error;

        string logMessage = "An uncaught exception occurred";
        try
        {
          if (Request != null && Request.Url != null)
            logMessage += string.Format(" in {0} {1} (referrer={2})", Request.HttpMethod, Request.Url.PathAndQuery, Request.UrlReferrer);
        }
        catch (HttpException)
        {
          // an HttpException could occur with message: "Request is not available in this context" but then we ignore it
        }
        log.Error(logMessage, exception);
      }
    }
  }
}