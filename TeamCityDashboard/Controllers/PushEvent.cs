using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeamCityDashboard.Models
{
  public class PushEvent
  {
    /// <summary>
    ///  The SHA of the HEAD commit on the repository.
    /// </summary>
    public string EventId { get; set; }

    /// <summary>
    /// event=>actor=>login
    /// </summary>
    public string ActorUsername { get; set; }
    
    /// <summary>
    /// event=>actor=>gravatar_id
    /// </summary>    
    public string ActorGravatarId { get; set; }

    /// <summary>
    /// event=>payload=>size
    /// </summary>
    public int AmountOfCommits { get; set; }

    /// <summary>
    /// event=>repo=>name
    /// </summary>
    public string RepositoryName { get; set; }
    
    /// <summary>
    /// event=>payload=>ref
    /// </summary>    
    public string BranchName { get; set; }

    /// <summary>
    /// event=>created_at
    /// </summary>
    public DateTime Created { get; set; }
  }
}
