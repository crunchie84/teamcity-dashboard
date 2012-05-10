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
    private TeamCityDataService DataService;

    public DashboardController()
    {
      this.DataService = new TeamCityDataService(
        ConfigurationManager.AppSettings["teamcity.baseUrl"],
        ConfigurationManager.AppSettings["teamcity.username"],
        ConfigurationManager.AppSettings["teamcity.password"]
      );
    }

    [UrlRoute(Name = "Data", Path = "data")]
    [HttpGet()]
    public ActionResult Data()
    {
      return new JsonResult()
      {
        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
        ContentEncoding = System.Text.Encoding.UTF8,
        Data = DataService.GetActiveProjects()
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
