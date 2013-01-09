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

    public DashboardController()
    {
      //singletonish ftw
      if (TeamCityDataService == null)
      {
        TeamCityDataService = new TeamCityDataService(
          ConfigurationManager.AppSettings["teamcity.baseUrl"],
          ConfigurationManager.AppSettings["teamcity.username"],
          ConfigurationManager.AppSettings["teamcity.password"]
        );
      }

      //singletonish ftw
      if (SonarDataService == null)
      {
        SonarDataService = new SonarDataService(
          ConfigurationManager.AppSettings["sonar.baseUrl"],
          ConfigurationManager.AppSettings["sonar.username"],
          ConfigurationManager.AppSettings["sonar.password"]
        );
      }

    }

    [UrlRoute(Name = "Data", Path = "data")]
    [HttpGet()]
    public ActionResult Data()
    {

      var projectsWithSonarDataAdded = from proj in TeamCityDataService.GetActiveProjects() select new TeamCityDashboard.Models.Project{
        Id = proj.Id,
        Name = proj.Name,
        BuildConfigs = proj.BuildConfigs,
        SonarProjectKey = proj.SonarProjectKey,
        Url = proj.Url,
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

    [UrlRoute(Name = "Home", Path = "")]
    [HttpGet()]
    public ActionResult Index()
    {
      return View();
    }

  }
}
