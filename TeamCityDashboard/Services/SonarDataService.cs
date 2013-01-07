using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TeamCityDashboard.Interfaces;

namespace TeamCityDashboard.Services
{
  public class SonarDataService : AbstractHttpDataService
  {
    public SonarDataService(string baseUrl, string username, string password) : base(baseUrl, username, password) { }

    //http://sonar.q42.net/api/resources?resource=NegenTwee:PartnerApi&metrics=ncloc,coverage&verbose=true&includetrends=true
    private const string PROJECT_TRENDS_URL = @"";

  }
}