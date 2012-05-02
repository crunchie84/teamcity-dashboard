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

    private const string URL_PROJECTLIST = @"/httpAuth/app/rest/projects";
    private const string URL_PROJECTDETAILS = @"/httpAuth/app/rest/projects/id:{0}";


    public IEnumerable<IProject> GetActiveProjects()
    {
      XmlDocument projectsPageContent = GetPageContents(URL_PROJECTLIST);
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
      XmlDocument projectDetails = GetPageContents(string.Format(URL_PROJECTDETAILS, projectId));
      if (projectDetails == null)
        return null;

      if (projectDetails.DocumentElement.GetAttribute("archived") == "true")
        return null;//not needed

      List<IBuildConfig> buildConfigs = new List<IBuildConfig>();
      foreach (XmlElement buildType in projectDetails.SelectNodes("//buildType"))
      {
        buildConfigs.Add(new BuildConfig{
          Id = buildType.GetAttribute("id"),
          Name = buildType.GetAttribute("name"),
          Url = buildType.GetAttribute("webUrl"),
          CurrentBuildIsSuccesfull = true,//TODO implement details fetching
          PossibleBuildBreakerEmailAddress = "mark@q42.nl"//TODO IMPLEMENT 
        });
      }

      return new Project{ 
        Id = projectId, 
        Name = projectName, 
        Url = projectDetails.DocumentElement.GetAttribute("webUrl"),
        BuildConfigs = buildConfigs
      };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="relativeUrl"></param>
    /// <returns></returns>
    /// <remarks>original code on http://www.stickler.de/en/information/code-snippets/httpwebrequest-basic-authentication.aspx</remarks>
    private XmlDocument GetPageContents(string relativeUrl)
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
          string content = myStreamReader.ReadToEnd();
          XmlDocument result = new XmlDocument();
          result.LoadXml(content);
          return result;
        }
      }
    }
  }
}
