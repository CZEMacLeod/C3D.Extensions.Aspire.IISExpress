﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SWAFramework.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Env()
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Break();
            }
            ViewBag.Message = $"RemoteApp Key {ConfigurationManager.AppSettings["RemoteApp:ApiKey"]}";
            var dict = System.Environment.GetEnvironmentVariables()
                .Keys.Cast<string>()
                .ToDictionary(k => k, k => System.Environment.GetEnvironmentVariable(k));
            return View(dict);
        }
    }
}