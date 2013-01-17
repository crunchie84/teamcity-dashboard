using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TeamCityDashboard.Interfaces;
using System.Web.Caching;

namespace TeamCityDashboard.Services
{
  public class WebCacheService : ICacheService
  {
    public T Get<T>(string cacheId, Func<T> getItemCallback, int secondsToCache) where T : class
    {
      T item = HttpRuntime.Cache.Get(cacheId) as T;
      if (item == null)
      {
        item = getItemCallback();
        Set(cacheId, item, secondsToCache);
      }
      return item;
    }

    public void Delete(string cacheId)
    {
      HttpRuntime.Cache.Remove(cacheId);
    }

    public void Set(string cacheId, object item, int secondsToCache)
    {
      HttpRuntime.Cache.Insert(cacheId, item, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 0, secondsToCache), CacheItemPriority.Normal, null);
    }
  }
}