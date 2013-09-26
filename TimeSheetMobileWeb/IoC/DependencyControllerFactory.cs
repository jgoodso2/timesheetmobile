using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StructureMap;
using TimeSheetMobileWeb.Controllers;


namespace TimeSheetMobileWeb.IoC
{
    public class DependencyControllerFactory: DefaultControllerFactory
    {
        protected override IController GetControllerInstance(System.Web.Routing.RequestContext requestContext, Type controllerType)
        {
            if (controllerType == null) return null;
            Controller controller = ObjectFactory.GetInstance(controllerType) as Controller;
            
            if (controllerType == typeof(TimesheetController))
            {
                TimesheetController timsheetController = controller as TimesheetController;
                timsheetController.Repository.SetClientEndpointsProg(HttpContext.Current.Items["PWAURL"].ToString());
                timsheetController.Repository.AppPoolUser = (System.Security.Principal.WindowsIdentity)HttpContext.Current.Application["AppUser"];
            }

            if (controllerType == typeof(TasksController))
            {
                TasksController timsheetController = controller as TasksController;
                timsheetController.Repository.SetClientEndpointsProg(HttpContext.Current.Items["PWAURL"].ToString());
                timsheetController.Repository.AppPoolUser = (System.Security.Principal.WindowsIdentity)HttpContext.Current.Application["AppUser"];
            }
            return controller;
        }
        public static void InitIoC(){
            ControllerBuilder.Current.SetControllerFactory(new DependencyControllerFactory());
            ObjectFactory.Initialize(x =>
            {
                // Tell StructureMap to look for configuration
                // from the App.config file
                // The default is false
                x.PullConfigurationFromAppConfig = true;
            });
        }
    }
}