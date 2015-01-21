using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace MvcCaching.Helpers
{
    public  class CacheHelper
    {
        public  string VaryByParam { get; set; }

        public CacheHelper(string varyByParam)
        {
            this.VaryByParam = varyByParam;
        }
        public  string Generatekey(ControllerContext filterContext)
        {
            StringBuilder cacheKey = new StringBuilder();

            cacheKey.Append(filterContext.Controller.GetType().FullName);

            if (filterContext.RouteData.Values.ContainsKey("action"))
            {
                cacheKey.Append("_");
                cacheKey.Append(filterContext.RouteData.Values["action"].ToString());

            }

            List<string> varyByParam = VaryByParam.Split(';').ToList();

            if (!string.IsNullOrEmpty(VaryByParam))
            {
                foreach (KeyValuePair<string, object> pair in filterContext.RouteData.Values)
                {
                    if (VaryByParam == "*" || varyByParam.Contains(pair.Key))
                    {
                        cacheKey.Append("_");
                        cacheKey.Append(pair.Key);
                        cacheKey.Append("=");
                        cacheKey.Append(pair.Value.ToString());
                    }
                }
            }
            return cacheKey.ToString();
        }

        public  string ResolveSubstitutions(ControllerContext filterContext, string source)
        {
            if (source.IndexOf("<!-SUBSTITUTION>") == -1)
            {
                return source;
            }
            MatchEvaluator replaceCallback = new MatchEvaluator(
                matchTOHandle =>
                {
                    string tag = matchTOHandle.Value;

                    string[] parts = tag.Split(':');
                    string className = parts[1];
                    string methodName = parts[2].Replace("-->", "");

                    Type targetType = Type.GetType(className);
                    MethodInfo targetMehod = targetType.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);

                    return (string)targetMehod.Invoke(null, new object[] { filterContext });
                }
                );
            Regex templatePattern = new Regex(@"<!--SUBSTITUTION:[A-Za-z_\.]+:[A-Za-z_\.]+-->", RegexOptions.Multiline);

            return templatePattern.Replace(source, replaceCallback);

        }

    }
}