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
    public SonarDataService(string baseUrl, string username, string password, ICacheService cacheService) : base(baseUrl, username, password, cacheService) { }

    //http://sonar.q42.net/api/resources?resource=NegenTwee:PartnerApi&metrics=ncloc,coverage&verbose=true&includetrends=true
    private const string PROJECT_TRENDS_URL = @"/api/resources?resource={0}&metrics={1}&verbose=true&includetrends=true&format=xml";
    private const string PROJECT_METRICS_CSV = @"coverage,class_complexity,function_complexity,ncloc,comment_lines_density,comment_lines,tests";

    /// <summary>
    /// create coverage csv report
    /// 2013-01-01T00:00:00+0100
    /// </summary>
    private const string PROJECT_TIMEMACHINE_URL = @"/api/timemachine?resource={0}&metrics=coverage&fromDateTime={1}&format=csv";

    public ICodeStatistics GetProjectStatistics(string projectKey)
    {
      CodeStatistics result = CacheService.Get<CodeStatistics>("sonar-stats-" + projectKey, () => {
        var data = GetPageContents(string.Format(CultureInfo.InvariantCulture, PROJECT_TRENDS_URL, projectKey, PROJECT_METRICS_CSV));

        //TODO this code can be greatly improved - error checking etc
        var stats = new CodeStatistics
        {
          AmountOfUnitTests = (int)double.Parse(data.SelectSingleNode("resources/resource/msr[key='tests']/val").InnerText, CultureInfo.InvariantCulture),
          NonCommentingLinesOfCode = (int)double.Parse(data.SelectSingleNode("resources/resource/msr[key='ncloc']/val").InnerText, CultureInfo.InvariantCulture),
          CommentLines = (int)double.Parse(data.SelectSingleNode("resources/resource/msr[key='comment_lines']/val").InnerText, CultureInfo.InvariantCulture),
          CommentLinesPercentage = double.Parse(data.SelectSingleNode("resources/resource/msr[key='comment_lines_density']/val").InnerText, CultureInfo.InvariantCulture),
          CyclomaticComplexityClass = double.Parse(data.SelectSingleNode("resources/resource/msr[key='class_complexity']/val").InnerText, CultureInfo.InvariantCulture),
          CyclomaticComplexityFunction = double.Parse(data.SelectSingleNode("resources/resource/msr[key='function_complexity']/val").InnerText, CultureInfo.InvariantCulture)
        };

        if (data.SelectSingleNode("resources/resource/msr[key='coverage']/val") != null)
          stats.CodeCoveragePercentage = double.Parse(data.SelectSingleNode("resources/resource/msr[key='coverage']/val").InnerText, CultureInfo.InvariantCulture);

        return stats;
      }, 3600);

      return result;
    }

    public IEnumerable<KeyValuePair<DateTime, double>> GetProjectCoverage(string projectKey)
    {
      string url = string.Format(CultureInfo.InvariantCulture, PROJECT_TIMEMACHINE_URL, projectKey, DateTime.Now.AddMonths(-2).ToString("s", CultureInfo.InvariantCulture));
      string csv = GetContents(url);
      var lines = from line
                     in csv.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Skip(1)
                   let columns = line.Split(',')
                   select new KeyValuePair<DateTime, double>(parseDateTime(columns.First()), double.Parse(columns.Skip(1).First(), CultureInfo.InvariantCulture));

      return lines;
    }

    private static DateTime parseDateTime(string date)
    {
      DateTime theDate;
      if (DateTime.TryParseExact(date, @"yyyy-MM-dd\THH:mm:sszz\0\0", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out theDate))
      {
        return theDate;
      }
      return DateTime.MinValue;
    }
  }
}