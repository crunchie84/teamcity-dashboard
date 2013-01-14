using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using TeamCityDashboard.Interfaces;

namespace TeamCityDashboard.Services
{
  public abstract class AbstractHttpDataService
  {
    protected readonly string BaseUrl;
    protected readonly string UserName;
    protected readonly string Password;

    protected readonly ICacheService CacheService;

    public AbstractHttpDataService(string baseUrl, string username, string password, ICacheService cacheService)
    {
      this.BaseUrl = baseUrl;
      this.UserName = username;
      this.Password = password;
      this.CacheService = cacheService;
    }

    /// <summary>
    /// Duration to cache some things which almost never change
    /// </summary>
    protected const int CACHE_DURATION = 3 * 60 * 60;//3 hours

    /// <summary>
    /// retrieve the content of given url and parse it to xmldocument. throws httpexception if raised
    /// </summary>
    /// <param name="relativeUrl"></param>
    /// <returns></returns>
    /// <remarks>original code on http://www.stickler.de/en/information/code-snippets/httpwebrequest-basic-authentication.aspx</remarks>
    protected XmlDocument GetPageContents(string relativeUrl)
    {
      XmlDocument result = new XmlDocument();
      result.LoadXml(GetContents(relativeUrl));
      return result;
    }

    /// <summary>
    /// retrieve the content of given url. throws httpexception if raised
    /// </summary>
    /// <param name="relativeUrl"></param>
    /// <returns></returns>
    protected string GetContents(string relativeUrl)
    {
      try
      {
        Uri uri = new Uri(string.Format("{0}{1}", BaseUrl, relativeUrl));
        WebRequest myWebRequest = HttpWebRequest.Create(uri);

        HttpWebRequest myHttpWebRequest = (HttpWebRequest)myWebRequest;

        NetworkCredential myNetworkCredential = new NetworkCredential(UserName, Password);
        CredentialCache myCredentialCache = new CredentialCache();
        myCredentialCache.Add(uri, "Basic", myNetworkCredential);

        myHttpWebRequest.PreAuthenticate = true;
        myHttpWebRequest.Credentials = myCredentialCache;

        using (WebResponse myWebResponse = myWebRequest.GetResponse())
        {
          using (Stream responseStream = myWebResponse.GetResponseStream())
          {
            StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);
            return myStreamReader.ReadToEnd();
          }
        }
      }
      catch (Exception e)
      {
        throw new HttpException(string.Format("Error while retrieving url '{0}': {1}", relativeUrl, e.Message), e);
      }
    }
  }
}
