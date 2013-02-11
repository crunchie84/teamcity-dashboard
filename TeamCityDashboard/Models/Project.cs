using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TeamCityDashboard.Interfaces;

namespace TeamCityDashboard.Models
{
  [System.Diagnostics.DebuggerDisplay("Project {Id} - {Name}")]
  public class Project : IProject
  {
    public string Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public string IconUrl { get; set; }
    public DateTime? LastBuildDate { get; set; }
    public IEnumerable<IBuildConfig> BuildConfigs { get; set; }

    // sonar specific part

    public string SonarProjectKey { get; set; }
    public ICodeStatistics Statistics{ get; set;}

    public object[][] CoverageGraph { get; set; }
  }
}