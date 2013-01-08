using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeamCityDashboard.Interfaces
{
  public interface ICodeStatistics
  {
    /// <summary>
    /// sonar key=coverage
    /// </summary>
    double CodeCoveragePercentage { get; }
    //int CodeCoveragePercentage { get; }

    /// <summary>
    /// sonar key=complexity
    /// </summary>
    int CyclomaticComplexity { get; }

    /// <summary>
    /// sonar key=ncloc
    /// </summary>
    int NonCommentingLinesOfCode { get; }
    
    /// <summary>
    /// sonar key=comment_lines_density
    /// </summary>
    double CommentLinesPercentage { get; }

    /// <summary>
    /// sonar key=comment_lines
    /// </summary>
    int CommentLines { get; }

    /// <summary>
    /// sonar key=tests
    /// </summary>
    int AmountOfUnitTests { get; }

    /// <summary>
    /// sonar key=lcom4
    /// </summary>
    //double LackOfCohesionOfMethods { get; }
  }
}
