using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ITCloud.Web.Routing;
using TeamCityDashboard.Interfaces;
using TeamCityDashboard.Services;
using System.Configuration;

namespace TeamCityDashboard.Controllers
{
  public class DashboardController : Controller
  {
    private static TeamCityDataService TeamCityDataService = null;
    private static SonarDataService SonarDataService = null;
    private static GithubDataService GithubDataService = null;

    public DashboardController()
    {
      ICacheService cacheService = new WebCacheService();

      //singletonish ftw
      if (TeamCityDataService == null)
      {
        TeamCityDataService = new TeamCityDataService(
          ConfigurationManager.AppSettings["teamcity.baseUrl"],
          ConfigurationManager.AppSettings["teamcity.username"],
          ConfigurationManager.AppSettings["teamcity.password"],
          cacheService
        );
      }

      //singletonish ftw
      if (SonarDataService == null)
      {
        SonarDataService = new SonarDataService(
          ConfigurationManager.AppSettings["sonar.baseUrl"],
          ConfigurationManager.AppSettings["sonar.username"],
          ConfigurationManager.AppSettings["sonar.password"],
          cacheService
        );
      }

      if (GithubDataService == null)
      {
        GithubDataService = new TeamCityDashboard.Services.GithubDataService(
          (string)ConfigurationManager.AppSettings["github.oauth2token"],
          (string)ConfigurationManager.AppSettings["github.api.events.url"]
          ,cacheService
        );
      }
    }

    [UrlRoute(Name = "Data", Path = "data")]
    [HttpGet()]
    public ActionResult Data()
    {
      var projectsWithSonarDataAdded = from proj in TeamCityDataService.GetActiveProjects()
                                       select new TeamCityDashboard.Models.Project
                                       {
                                         Id = proj.Id,
                                         Name = proj.Name,
                                         BuildConfigs = proj.BuildConfigs,
                                         SonarProjectKey = proj.SonarProjectKey,
                                         Url = proj.Url,
                                         LastBuildDate = proj.LastBuildDate,
                                         IconUrl = proj.IconUrl,
                                         Statistics = string.IsNullOrWhiteSpace(proj.SonarProjectKey) ? (ICodeStatistics)null : SonarDataService.GetProjectStatistics(proj.SonarProjectKey)
                                       };

      return new JsonResult()
      {
        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
        ContentEncoding = System.Text.Encoding.UTF8,
        Data = projectsWithSonarDataAdded
      };
    }

    [UrlRoute(Name = "Github push events", Path = "pushevents")]
    [HttpGet()]
    public ActionResult PushEvents()
    {
       return new JsonResult()
      {
        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
        ContentEncoding = System.Text.Encoding.UTF8,
        Data = GithubDataService.GetRecentEvents()
      };
    }

    [UrlRoute(Name = "Home", Path = "")]
    [HttpGet()]
    public ActionResult Index()
    {
      return View();
    }
  }
}