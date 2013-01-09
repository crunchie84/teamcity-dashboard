using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using TeamCityDashboard.Interfaces;
using TeamCityDashboard.Models;

namespace TeamCityDashboard.Services
{
  public class SonarDataService : AbstractHttpDataService
  {
    public SonarDataService(string baseUrl, string username, string password) : base(baseUrl, username, password) { }

    //http://sonar.q42.net/api/resources?resource=NegenTwee:PartnerApi&metrics=ncloc,coverage&verbose=true&includetrends=true
    private const string PROJECT_TRENDS_URL = @"/api/resources?resource={0}&metrics={1}&verbose=true&includetrends=true&format=xml";
    private const string PROJECT_METRICS_CSV = @"coverage,class_complexity,function_complexity,ncloc,comment_lines_density,comment_lines,tests";

    public ICodeStatistics GetProjectStatistics(string projectKey)
    {
      var data = GetPageContents(string.Format(CultureInfo.InvariantCulture, PROJECT_TRENDS_URL, projectKey, PROJECT_METRICS_CSV));

      //TODO this code can be greatly improved - error checking etc
      return new CodeStatistics {
        AmountOfUnitTests = (int)double.Parse(data.SelectSingleNode("resources/resource/msr[key='tests']/val").InnerText, CultureInfo.InvariantCulture),
        NonCommentingLinesOfCode = (int)double.Parse(data.SelectSingleNode("resources/resource/msr[key='ncloc']/val").InnerText, CultureInfo.InvariantCulture),
        CommentLines = (int)double.Parse(data.SelectSingleNode("resources/resource/msr[key='comment_lines']/val").InnerText, CultureInfo.InvariantCulture),
        CommentLinesPercentage = double.Parse(data.SelectSingleNode("resources/resource/msr[key='comment_lines_density']/val").InnerText, CultureInfo.InvariantCulture),
        CyclomaticComplexityClass = double.Parse(data.SelectSingleNode("resources/resource/msr[key='class_complexity']/val").InnerText, CultureInfo.InvariantCulture),
        CyclomaticComplexityFunction = double.Parse(data.SelectSingleNode("resources/resource/msr[key='function_complexity']/val").InnerText, CultureInfo.InvariantCulture),
        CodeCoveragePercentage = double.Parse(data.SelectSingleNode("resources/resource/msr[key='coverage']/val").InnerText, CultureInfo.InvariantCulture)
      };
    }
  }
}