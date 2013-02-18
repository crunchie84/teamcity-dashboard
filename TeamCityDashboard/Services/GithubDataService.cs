using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using log4net;
using Newtonsoft.Json.Linq;
using TeamCityDashboard.Interfaces;
using TeamCityDashboard.Models;

namespace TeamCityDashboard.Services
{
  public class GithubDataService
  {
    private static readonly ILog log = LogManager.GetLogger(typeof(GithubDataService));

    private readonly ICacheService cacheService;
    private readonly string oauth2token;
    private readonly string eventsurl;

    private const string API_BASE_URL = @"https://api.github.com";

    public GithubDataService(string oauth2token, string eventsurl, ICacheService cacheService)
    {
      if (cacheService == null)
        throw new ArgumentNullException("cacheService");
      if (string.IsNullOrWhiteSpace(oauth2token))
        throw new ArgumentNullException("oauth2token");
      if (string.IsNullOrWhiteSpace(eventsurl))
        throw new ArgumentNullException("eventsurl");

      this.cacheService = cacheService;
      this.oauth2token = oauth2token;
      this.eventsurl = eventsurl;
    }

    /// <summary>
    /// always returns (max) 5 recent events old -> first ordered
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TeamCityDashboard.Models.PushEvent> GetRecentEvents()
    {
      try
      {
        string response = getEventsApiContents();
        if (!string.IsNullOrWhiteSpace(response))
        {
          //parse result, re-cache it
          var latestPushEvents = parseGithubPushEventsJson(response);
          cacheService.Set("latest-pushevents", latestPushEvents, WebCacheService.CACHE_PERMANENTLY);
        }
        return getRecentPushEventsFromCache();
      }
      catch (Exception ex)
      {
        log.Error(ex);
        return Enumerable.Empty<PushEvent>();
      }
    }

    private static List<PushEvent> parseGithubPushEventsJson(string json)
    {
      JArray events = JArray.Parse(json);
      var parsedEvents = (from evt in events
                          where (string)evt["type"] == "PushEvent"
                          select parseGithubPushEvent(evt));

      log.DebugFormat("Retrieved {0} push events from github", parsedEvents.Count());

      var latestFivePushEvents = parsedEvents.OrderByDescending(pe => pe.Created).Take(5).OrderBy(pe => pe.Created).ToList();

      return latestFivePushEvents;
    }

    private static PushEvent parseGithubPushEvent(JToken evt)
    {
      string repositoryName = ((string) evt["repo"]["name"]);
      if(repositoryName.Contains('/'))
        repositoryName = repositoryName.Substring(1 + repositoryName.IndexOf('/'));

      return new PushEvent
                          {
                            RepositoryName = repositoryName,
                            BranchName = ((string)evt["payload"]["ref"]).Replace("refs/heads/", ""),
                            EventId = evt["id"].ToString(),
                            ActorUsername = (string)evt["actor"]["login"],
                            ActorGravatarId = (string)evt["actor"]["gravatar_id"],
                            AmountOfCommits = (int)evt["payload"]["size"],
                            Created = (DateTime)evt["created_at"]
                          };
    }

    private IEnumerable<PushEvent> getRecentPushEventsFromCache()
    {
      var latestPushEvents = cacheService.Get<List<PushEvent>>("latest-pushevents");
      if (latestPushEvents == null)
      {
        log.Error("We could not find pushEvents in the cache AND github did not return any. Possible error?");
        return Enumerable.Empty<PushEvent>();
      }

      return latestPushEvents;
    }

    private string lastReceivedEventsETAG;
    private string getEventsApiContents(bool ignoreCache=false)
    {
      try
      {
        Uri uri = new Uri(string.Format("{0}{1}", API_BASE_URL, eventsurl));
        HttpWebRequest myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(uri);
        myHttpWebRequest.UserAgent = "TeamCity CI Dashboard - https://github.com/crunchie84/teamcity-dashboard";
        myHttpWebRequest.Headers.Add("Authorization", "bearer " + oauth2token);

        if (!string.IsNullOrWhiteSpace(lastReceivedEventsETAG) && !ignoreCache)
          myHttpWebRequest.Headers.Add("If-None-Match", lastReceivedEventsETAG);

        using (HttpWebResponse myWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse())
        {
          if (myWebResponse.StatusCode == HttpStatusCode.OK)
          {
            //if we do not save the returned ETag we will always get the full list with latest changes instead of the real delta since we started polling.
            lastReceivedEventsETAG = myWebResponse.Headers.Get("ETag");

            using (Stream responseStream = myWebResponse.GetResponseStream())
            {
              StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);
              return myStreamReader.ReadToEnd();
            }
          }
        }
      }
      catch (HttpException ex)
      {
        if (ex.GetHttpCode() != (int)HttpStatusCode.NotModified)
          throw;
      }
      catch (WebException ex)
      {
        var response = ex.Response as HttpWebResponse;
        if(response == null || response.StatusCode != HttpStatusCode.NotModified)
          throw;
      }
      catch (Exception ex)
      {
        throw new HttpException(string.Format("Error while retrieving url '{0}': {1}", eventsurl, ex.Message), ex);
      }
      return string.Empty;
    }
  }
}