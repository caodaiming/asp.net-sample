using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcCaching.Helpers;

namespace MvcCaching.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        [Helpers.OutputCache(Duration = 10, VaryByParam = "*", CachePolicy = CachePolicy.Server)]
        public ActionResult Index()
        {
            ViewData["Title"] = "Home Page";
            ViewData["Message"] = "Welcome to ASP.NET MVC!";

            return View();
        }

    }
}
