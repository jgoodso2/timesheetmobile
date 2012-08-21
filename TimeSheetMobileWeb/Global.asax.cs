﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace TimeSheetMobileWeb
{
    // Nota: per istruzioni su come abilitare la modalità classica di IIS6 o IIS7, 
    // visitare il sito Web all'indirizzo http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Nome route
                "{controller}/{action}/{id}", // URL con parametri
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Valori predefiniti parametri
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            TimeSheetMobileWeb.IoC.DependencyControllerFactory.InitIoC();
            TimeSheetMobileWeb.Models.GlobalViewsConfiguration.Load();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}