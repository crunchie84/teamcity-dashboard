using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TeamCityDashboard.Interfaces;

namespace TeamCityDashboard.Models
{
  public class Project : IProject
  {
    public string Id { get; set; }
    public string Name { get; set; }
    public IEnumerable<IBuildConfig> BuildConfigs { get; set; }
    public string Url { get; set; }

    public string IconUrl { get; set; }

    // sonar specific part

    public string SonarProjectKey { get; set; }
    public ICodeStatistics Statistics{ get; set;}
  }
}