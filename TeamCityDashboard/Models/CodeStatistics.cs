using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TeamCityDashboard.Interfaces;

namespace TeamCityDashboard.Models
{
  public class CodeStatistics : ICodeStatistics
  {
    public double CodeCoveragePercentage { get; set; }
    public double CyclomaticComplexityClass { get; set; }
    public double CyclomaticComplexityFunction { get; set; }
    public int NonCommentingLinesOfCode { get; set; }
    public double CommentLinesPercentage { get; set; }
    public int CommentLines { get; set; }
    public int AmountOfUnitTests { get; set; }
  }
}