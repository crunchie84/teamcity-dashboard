using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeamCityDashboard.Interfaces
{
  public interface IBuildRun
  {
    IBuildConfig BuildConfig { get; }
    string Id { get; }
    string Name { get; }
  }
}
