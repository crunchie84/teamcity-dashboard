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
    public const int CACHE_PERMANENTLY = 0;

    public T Get<T>(string cacheId) where T : class
    {
      return HttpRuntime.Cache.Get(cacheId) as T;
    }

    public T Get<T>(string cacheId, Func<T> getItemCallback, int secondsToCache) where T : class
    {
      T item = Get<T>(cacheId);
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

    /// <summary>
    /// Set item in cache, if secondsToCache <= 0 then does not expire
    /// </summary>
    /// <param name="cacheId"></param>
    /// <param name="item"></param>
    /// <param name="secondsToCache"></param>
    public void Set(string cacheId, object item, int secondsToCache)
    {
      if (item == null)
        throw new NotImplementedException("No NULL values can be cached at the moment.");

      if (secondsToCache <= CACHE_PERMANENTLY)
      {
        //never remove from cache
        HttpRuntime.Cache.Insert(cacheId, item, null, System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
      }
      else
      {
        HttpRuntime.Cache.Insert(cacheId, item, null, DateTime.Now.AddSeconds(secondsToCache), System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
      }
    }
  }
}