using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeamCityDashboard.Interfaces
{
  public interface ICacheService
  {
    void Set(string cacheId, object item, int secondsToCache);
    T Get<T>(string cacheId, Func<T> getItemCallback, int secondsToCache) where T : class;
    void Delete(string cacheId);
  }
}
