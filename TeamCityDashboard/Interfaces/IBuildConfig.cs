using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeamCityDashboard.Interfaces
{
  public interface IBuildConfig
  {
    string Id { get; }
    string Name { get; }
    string Url { get; }
    DateTime? CurrentBuildDate { get; }
    bool CurrentBuildIsSuccesfull { get; }
    IEnumerable<string> PossibleBuildBreakerEmailAddresses { get; }
  }
}
