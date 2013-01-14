using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TeamCityDashboard.Models;

namespace TeamCityDashboard.Services
{
  public class GithubDataService
  {
    private string oauth2token;
    private string eventsurl;
    private const string API_BASE_URL = @"https://api.github.com";

    public GithubDataService(string oauth2token, string eventsurl)
    {
      this.oauth2token = oauth2token;
      this.eventsurl = eventsurl;
    }


    private string LastReceivedEventsETAG;

    public IEnumerable<TeamCityDashboard.Models.PushEvent> GetRecentEvents()
    {
      return Enumerable.Empty<PushEvent>();
    }
  }
}
