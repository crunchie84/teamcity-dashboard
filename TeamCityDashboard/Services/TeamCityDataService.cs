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

namespace TeamCityDashboard.Services
{
  public class TeamCityDataService
  {
    private readonly string BaseUrl;
    private readonly string UserName;
    private readonly string Password;

    public TeamCityDataService(string baseUrl, string username, string password)
    {
      this.BaseUrl = baseUrl;
      this.UserName = username;
      this.Password = password;
    }

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
    /// retrieve the list of active projects which have at least one visible buildconfig
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IProject> GetActiveProjects()
    {
      XmlDocument projectsPageContent = GetPageContents(URL_PROJECTS_LIST);
      if (projectsPageContent == null)
        yield break;

      foreach (XmlElement el in projectsPageContent.SelectNodes("//project"))
      {
        var project = ParseProjectDetails(el.GetAttribute("id"), el.GetAttribute("name"));
        if (project != null)
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
      XmlDocument projectDetails = GetPageContents(string.Format(URL_PROJECT_DETAILS, projectId));
      if (projectDetails == null)
        return null;

      if (projectDetails.DocumentElement.GetAttribute("archived") == "true")
        return null;//not needed

      List<IBuildConfig> buildConfigs = new List<IBuildConfig>();
      foreach (XmlElement buildType in projectDetails.SelectNodes("project/buildTypes/buildType"))
      {
        var buildConfigDetails = ParseBuildConfigDetails(buildType.GetAttribute("id"), buildType.GetAttribute("name"));
        if(buildConfigDetails != null)
          buildConfigs.Add(buildConfigDetails);
      }

      if (buildConfigs.Count == 0)
        return null;//do not report 'empty' projects'

      return new Project
      {
        Id = projectId,
        Name = projectName,
        Url = projectDetails.DocumentElement.GetAttribute("webUrl"),
        BuildConfigs = buildConfigs
      };
    }

    private IBuildConfig ParseBuildConfigDetails(string id, string name)
    {
      //do we need to show this buildCOnfig?
      XmlDocument buildConfigDetails = GetPageContents(string.Format(URL_BUILD_DETAILS, id));
      XmlElement externalStatusEl = buildConfigDetails.SelectSingleNode("buildType/settings/property[@name='allowExternalStatus']") as XmlElement;
      if(externalStatusEl == null || externalStatusEl.GetAttribute("value") == "false")
        return null;//no external status visible

      ///retrieve details of last 100 builds and find out if the last (=first row) was succesfull or iterate untill we found the first breaker?
      XmlDocument buildResultsDoc = GetPageContents(string.Format(URL_BUILDS_LIST, id));
      XmlElement lastBuild = buildResultsDoc.DocumentElement.FirstChild as XmlElement;
      bool currentBuildSuccesfull = lastBuild != null ? lastBuild.GetAttribute("status") == "SUCCESS" : true;//default to true

      List<string> buildBreakerEmailaddress = new List<string>();
      if (!currentBuildSuccesfull)
      {
        XmlNode lastSuccessfullBuild = buildResultsDoc.DocumentElement.SelectSingleNode("build[@status='SUCCESS']");
        if (lastSuccessfullBuild == null)
        {
          buildBreakerEmailaddress.Add("unknown-too-long-ago");
        }
        else
        {
          XmlElement breakingBuild = lastSuccessfullBuild.PreviousSibling as XmlElement;
          if (breakingBuild == null)
            buildBreakerEmailaddress.Add("no-breaking-build-after-succes-should-not-happen");
          else
            buildBreakerEmailaddress = ParseBuildBreakerDetails(breakingBuild.GetAttribute("id")).Distinct().ToList();
        }
      }

      return new BuildConfig
      {
        Id = id,
        Name = name,
        Url = new Uri(string.Format("{0}/viewType?html?buildTypeId={1}", BaseUrl, id)).ToString(),
        CurrentBuildIsSuccesfull = currentBuildSuccesfull,
        PossibleBuildBreakerEmailAddresses = buildBreakerEmailaddress
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
        string userId = (changeDetailsDoc.SelectSingleNode("change/user") as XmlElement).GetAttribute("id");
        if (userId == null)
          throw new ArgumentNullException(string.Format("No userId given in changeId {0}", changeId));

        //retrieve email
        string email = GetContents(string.Format(URL_USER_EMAILADDRESS, userId));
        if (!string.IsNullOrEmpty(email))
          yield return email.ToLower().Trim();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="relativeUrl"></param>
    /// <returns></returns>
    /// <remarks>original code on http://www.stickler.de/en/information/code-snippets/httpwebrequest-basic-authentication.aspx</remarks>
    private XmlDocument GetPageContents(string relativeUrl)
    {
      XmlDocument result = new XmlDocument();
      result.LoadXml(GetContents(relativeUrl));
      return result;
    }

    private string GetContents(string relativeUrl)
    {
      try
      {
        Uri uri = new Uri(string.Format("{0}{1}", BaseUrl, relativeUrl));
        WebRequest myWebRequest = HttpWebRequest.Create(uri);

        HttpWebRequest myHttpWebRequest = (HttpWebRequest)myWebRequest;

        NetworkCredential myNetworkCredential = new NetworkCredential(UserName, Password);
        CredentialCache myCredentialCache = new CredentialCache();
        myCredentialCache.Add(uri, "Basic", myNetworkCredential);

        myHttpWebRequest.PreAuthenticate = true;
        myHttpWebRequest.Credentials = myCredentialCache;

        using (WebResponse myWebResponse = myWebRequest.GetResponse())
        {
          using (Stream responseStream = myWebResponse.GetResponseStream())
          {
            StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);
            return myStreamReader.ReadToEnd();
          }
        }
      }
      catch (Exception e)
      {
        throw new HttpException(string.Format("Error while retrieving url '{0}': {1}", relativeUrl, e.Message), e);
      }
    }
  }
}
