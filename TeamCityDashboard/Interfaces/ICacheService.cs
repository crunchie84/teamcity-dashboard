using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeamCityDashboard.Interfaces
{
  public interface ICacheService
  {
    /// <summary>
    /// Set item in cache, if secondsToCache <= 0 then does not expire
    /// </summary>
    /// <param name="cacheId"></param>
    /// <param name="item"></param>
    /// <param name="secondsToCache"></param>
    void Set(string cacheId, object item, int secondsToCache);
    T Get<T>(string cacheId) where T : class;
    T Get<T>(string cacheId, Func<T> getItemCallback, int secondsToCache) where T : class;
    void Delete(string cacheId);
  }
}
