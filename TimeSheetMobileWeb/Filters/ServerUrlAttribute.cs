using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using TimeSheetMobileWeb.Models;
using System.Configuration;

namespace TimeSheetMobileWeb.Filters
{
    public class ServerUrlAttribute: AuthorizeAttribute
    {
        private static string messageKey = "NoBaseUrlMessage";
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return PSCaseUrlHelper.FindBaseUrl();
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new HttpStatusCodeResult(400, ConfigurationManager.AppSettings[messageKey]);
        }
    }
}