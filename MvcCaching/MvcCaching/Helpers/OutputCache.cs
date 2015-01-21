using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace MvcCaching.Helpers
{
    public enum CachePolicy
    {
        NoCache = 0,
        Client = 1,
        Server = 2,
        ClientAndServer = 3
    }

    public class OutputCache : ActionFilterAttribute
    {
        #region public properties

        public int Duration { get; set; }

        public string VaryByParam { get; set; }

        public CachePolicy CachePolicy { get; set; }


        #endregion

        #region private properties

        private Cache cache { get; set; }

        private bool cacheHit { get; set; }

        private HttpContext existingContext { get; set; }

        private StringWriter writer { get; set; }
        #endregion

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (CachePolicy == CachePolicy.Client || CachePolicy == Helpers.CachePolicy.ClientAndServer)
            {
                if (Duration < 0) return;

                HttpCachePolicyBase cache = filterContext.HttpContext.Response.Cache;
                TimeSpan cacheduration = TimeSpan.FromSeconds(Duration);

                cache.SetCacheability(HttpCacheability.Public);
                cache.SetExpires(DateTime.Now.Add(cacheduration));
                cache.SetMaxAge(cacheduration);
                cache.AppendCacheExtension("must-revalidate,proxy-revalidate");

            }
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {

            if (CachePolicy == Helpers.CachePolicy.Server || CachePolicy == Helpers.CachePolicy.ClientAndServer)
            {
                cache = filterContext.HttpContext.Cache;

                object cacheData = cache.Get(new CacheHelper(VaryByParam).Generatekey(filterContext));

                if (cacheData != null)
                {
                    cacheHit = true;

                    cacheData = new CacheHelper(VaryByParam).ResolveSubstitutions(filterContext, (string)cacheData);

                    filterContext.HttpContext.Response.Write(cacheData);

                    filterContext.Cancel = true;
                }
                else
                {
                    existingContext = System.Web.HttpContext.Current;
                    writer = new StringWriter();
                    HttpResponse response = new HttpResponse(writer);
                    HttpContext context = new HttpContext(existingContext.Request, response)
                    {
                        User = existingContext.User
                    };

                    foreach (var key in existingContext.Items.Keys)
                    {
                        context.Items[key] = existingContext.Items[key];
                    }

                    System.Web.HttpContext.Current = context;
                }
            }
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (CachePolicy == Helpers.CachePolicy.Client || CachePolicy == Helpers.CachePolicy.ClientAndServer)
            {
                if (!cacheHit)
                {
                    string output = writer.ToString();

                    System.Web.HttpContext.Current = existingContext;
                    output = new CacheHelper(VaryByParam).ResolveSubstitutions(filterContext, output);

                    existingContext.Response.Write(output);

                    cache.Add(
                        new CacheHelper(VaryByParam).Generatekey(filterContext),
                        writer.ToString(),
                        null,
                        DateTime.Now.AddSeconds(Duration),
                        Cache.NoSlidingExpiration,
                        CacheItemPriority.Normal,
                        null
                        );
                }
            }
        }
    }
}