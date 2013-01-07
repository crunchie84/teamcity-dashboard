using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeamCityDashboard.Interfaces
{
  public interface IProject
  {
    string Id { get; }
    string Name { get; }
    string Url { get; }
    string SonarProjectKey { get; }
    IEnumerable<IBuildConfig> BuildConfigs { get; }
  }
}
