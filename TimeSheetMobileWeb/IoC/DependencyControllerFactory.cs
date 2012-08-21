using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StructureMap;


namespace TimeSheetMobileWeb.IoC
{
    public class DependencyControllerFactory: DefaultControllerFactory
    {
        protected override IController GetControllerInstance(System.Web.Routing.RequestContext requestContext, Type controllerType)
        {
            if (controllerType == null) return null;
            return ObjectFactory.GetInstance(controllerType) as Controller;
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