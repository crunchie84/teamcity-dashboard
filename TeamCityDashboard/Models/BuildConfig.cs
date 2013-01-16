using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TeamCityDashboard.Interfaces;

namespace TeamCityDashboard.Models
{
  [System.Diagnostics.DebuggerDisplay("BuildConfig - {Id} - {Name} - CurrentBuildDate {CurrentBuildDate}")]
  public class BuildConfig : IBuildConfig
  {
    public string Id { get; set; }
    public string Name { get; set; }
    public bool CurrentBuildIsSuccesfull { get; set; }
    public string Url { get; set; }
    public IEnumerable<string> PossibleBuildBreakerEmailAddresses { get; set; }
    public DateTime? CurrentBuildDate { get; set; }
  }
}