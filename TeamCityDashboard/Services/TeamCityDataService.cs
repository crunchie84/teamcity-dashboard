using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TeamCityDashboard.Interfaces;
using System.Net;
using System.IO;
using System.Text;
using System.Xml;
using TeamCityDashboard.Models;
using System.Globalization;

namespace TeamCityDashboard.Services
{
  public class TeamCityDataService : AbstractHttpDataService
  {
    public TeamCityDataService(string baseUrl, string username, string password, ICacheService cacheService) : base(baseUrl, username, password, cacheService) { }

    /// <summary>
    /// url to retrieve list of projects in TeamCity
    /// </summary>
    private const string URL_PROJECTS_LIST = @"/httpAuth/app/rest/projects";

    /// <summary>
    /// url to retrieve details of given {0} project (buildtypes etc)
    /// </summary>
    private const string URL_PROJECT_DETAILS = @"/httpAuth/app/rest/projects/id:{0}";

    /// <summary>
    /// retrieve the first 100 builds of the given buildconfig and retrieve the status of it
    /// </summary>
    private const string URL_BUILDS_LIST = @"/httpAuth/app/rest/buildTypes/id:{0}/builds";

    /// <summary>
    /// retrieve details of the given build ({0}) and verify that the /buildType/settings/property[@name='allowExternalStatus'] == 'true'
    /// </summary>
    private const string URL_BUILD_DETAILS = @"/httpAuth/app/rest/buildTypes/id:{0}";

    /// <summary>
    /// url to retrieve the changes commited in the given {0} buildrunId
    /// </summary>
    private const string URL_BUILD_CHANGES = @"/httpAuth/app/rest/changes?build=id:{0}";

    /// <summary>
    /// Url to retrieve the details of the given {0} changeId
    /// </summary>
    private const string URL_CHANGE_DETAILS = @"/httpAuth/app/rest/changes/id:{0}";

    /// <summary>
    /// url to retrieve the emailaddress of the given {0} userId
    /// </summary>
    private const string URL_USER_EMAILADDRESS = @"/httpAuth/app/rest/users/id:{0}/email";

    /// <summary>
    /// take all failed projects with at least one build config visible AND max 15 successfull build configs
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IProject> GetActiveProjects()
    {
      var projects = getNonArchivedProjects().ToList();

      var failing = projects.Where(p => p.BuildConfigs.Any(c => !c.CurrentBuildIsSuccesfull));
      var success = projects.Where(p => p.BuildConfigs.All(c => c.CurrentBuildIsSuccesfull));

      int amountToTake = Math.Max(15, failing.Count());

      //only display the most recent 15 build projects together with the failing ones OR if we have more failing display those
      return failing.Concat(success.OrderByDescending(p => p.LastBuildDate)).Take(amountToTake);
    }

    private IEnumerable<IProject> getNonArchivedProjects()
    {
      XmlDocument projectsPageContent = GetPageContents(URL_PROJECTS_LIST);
      if (projectsPageContent == null)
        yield break;

      foreach (XmlElement el in projectsPageContent.SelectNodes("//project"))
      {
        var project = ParseProjectDetails(el.GetAttribute("id"), el.GetAttribute("name"));
        if (project == null)
          continue;
        yield return project;
      }
    }

    /// <summary>
    /// only retrieve non-archived projects etc
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectName"></param>
    /// <returns></returns>
    private IProject ParseProjectDetails(string projectId, string projectName)
    {
      //determine details, archived? buildconfigs
      XmlDocument projectDetails = CacheService.Get<XmlDocument>("project-details-" + projectId, () => { 
        return GetPageContents(string.Format(URL_PROJECT_DETAILS, projectId)); 
      }, 15 * 60);

      if (projectDetails == null)
        return null;

      if (projectDetails.DocumentElement.GetAttribute("archived") == "true")
        return null;//not needed

      List<IBuildConfig> buildConfigs = new List<IBuildConfig>();
      foreach (XmlElement buildType in projectDetails.SelectNodes("project/buildTypes/buildType"))
      {
        var buildConfigDetails = ParseBuildConfigDetails(buildType.GetAttribute("id"), buildType.GetAttribute("name"));
        if (buildConfigDetails != null)
          buildConfigs.Add(buildConfigDetails);
      }

      if (buildConfigs.Count == 0)
        return null;//do not report 'empty' projects'

      return new Project
      {
        Id = projectId,
        Name = projectName,
        Url = projectDetails.DocumentElement.GetAttribute("webUrl"),
        IconUrl = parseProjectProperty(projectDetails, "dashboard.project.logo.url"),
        SonarProjectKey = parseProjectProperty(projectDetails, "sonar.project.key"),
        BuildConfigs = buildConfigs,
        LastBuildDate = (buildConfigs.Where(b => b.CurrentBuildDate.HasValue).Max(b => b.CurrentBuildDate.Value))
      };
    }

    private static string parseProjectProperty(XmlDocument projectDetails, string propertyName)
    {
      var propertyElement = projectDetails.SelectSingleNode(string.Format("project/parameters/property[@name='{0}']/@value", propertyName));
      if (propertyElement != null)
        return propertyElement.Value;

      return null;
    }

    /// <summary>
    /// Retrieve the build configdetails with the given ID. return NULL if the given config is not visible in the widget interface
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    private IBuildConfig ParseBuildConfigDetails(string id, string name)
    {
      //do we need to show this buildCOnfig?
      bool isVisibleExternally = CacheService.Get<ProjectVisible>("build-visible-widgetinterface-" + id, () => IsBuildVisibleOnExternalWidgetInterface(id), CACHE_DURATION).Visible;
      if (!isVisibleExternally)
        return null;

      ///retrieve details of last 100 builds and find out if the last (=first row) was succesfull or iterate untill we found the first breaker?
      XmlDocument buildResultsDoc = GetPageContents(string.Format(URL_BUILDS_LIST, id));
      XmlElement lastBuild = buildResultsDoc.DocumentElement.FirstChild as XmlElement;


      DateTime? currentBuildDate = null;
      bool currentBuildSuccesfull = true;
      List<string> buildBreakerEmailaddress = new List<string>();

      if (lastBuild != null)
      {
        currentBuildSuccesfull = lastBuild.GetAttribute("status") == "SUCCESS";//we default to true

        //try to parse last date
        string buildDate = lastBuild.GetAttribute("startDate");
        DateTime theDate;

        //TeamCity date format => 20121101T134409+0100
        if (DateTime.TryParseExact(buildDate, @"yyyyMMdd\THHmmsszz\0\0", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out theDate))
        {
          currentBuildDate = theDate;
        }

        //if the last build was not successfull iterate back untill we found one which was successfull so we know who might have broke it.
        if (!currentBuildSuccesfull)
        {
          XmlNode lastSuccessfullBuild = buildResultsDoc.DocumentElement.SelectSingleNode("build[@status='SUCCESS']");
          if (lastSuccessfullBuild != null)
          {
            XmlElement breakingBuild = lastSuccessfullBuild.PreviousSibling as XmlElement;
            if (breakingBuild == null)
              buildBreakerEmailaddress.Add("no-breaking-build-after-succes-should-not-happen");
            else
              buildBreakerEmailaddress = CacheService.Get<IEnumerable<string>>(
                  "buildbreakers-build-" + breakingBuild.GetAttribute("id"),
                  () => ParseBuildBreakerDetails(breakingBuild.GetAttribute("id")),
                  CACHE_DURATION
                ).Distinct().ToList();
          }
          else
          {
            //IF NO previous pages with older builds available then we can assume this is the first build and it broke. show image of that one.
            //TODO we could iterate older builds to find the breaker via above logic
          }
        }
      }

      return new BuildConfig
      {
        Id = id,
        Name = name,
        Url = new Uri(string.Format("{0}/viewType.html?buildTypeId={1}&tab=buildTypeStatusDiv", BaseUrl, id)).ToString(),
        CurrentBuildIsSuccesfull = currentBuildSuccesfull,
        CurrentBuildDate = currentBuildDate,
        PossibleBuildBreakerEmailAddresses = buildBreakerEmailaddress
      };
    }

    private ProjectVisible IsBuildVisibleOnExternalWidgetInterface(string id)
    {
      XmlDocument buildConfigDetails = GetPageContents(string.Format(URL_BUILD_DETAILS, id));
      XmlElement externalStatusEl = buildConfigDetails.SelectSingleNode("buildType/settings/property[@name='allowExternalStatus']") as XmlElement;
      return new ProjectVisible
      {
        Visible = externalStatusEl != null && externalStatusEl.GetAttribute("value") == "true"//no external status visible
      };
    }

    /// <summary>
    /// retrieve the emailaddress of the user who changed something in the given buildId because he most likely broke it
    /// </summary>
    /// <param name="buildId"></param>
    /// <returns></returns>
    private IEnumerable<string> ParseBuildBreakerDetails(string buildId)
    {
      //retrieve changes
      XmlDocument buildChangesDoc = GetPageContents(string.Format(URL_BUILD_CHANGES, buildId));
      foreach (XmlElement el in buildChangesDoc.SelectNodes("//change"))
      {
        //retrieve change details
        string changeId = el.GetAttribute("id");
        if (string.IsNullOrEmpty(changeId))
          throw new ArgumentNullException(string.Format("@id of change within buildId {0} should not be NULL", buildId));

        //retrieve userid who changed something//details
        XmlDocument changeDetailsDoc = GetPageContents(string.Format(URL_CHANGE_DETAILS, changeId));
        XmlElement userDetails = (changeDetailsDoc.SelectSingleNode("change/user") as XmlElement);
        if (userDetails == null)
          continue;//sometimes a change is not linked to a user who commited it.. ?

        string userId = userDetails.GetAttribute("id");
        if (userId == null)
          throw new ArgumentNullException(string.Format("No userId given in changeId {0}", changeId));

        //retrieve email
        string email = CacheService.Get<string>("user-email-" + userId, () => GetUserEmailAddress(userId), CACHE_DURATION);
        if (!string.IsNullOrEmpty(email))
          yield return email.ToLower().Trim();
      }
    }

    private string GetUserEmailAddress(string userId)
    {
      return GetContents(string.Format(URL_USER_EMAILADDRESS, userId));
    }
  }
}
