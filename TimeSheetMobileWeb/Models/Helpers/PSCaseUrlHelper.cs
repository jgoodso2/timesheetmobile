using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using TimeSheetIBusiness;

namespace TimeSheetMobileWeb.Models
{
    public static class PSCaseUrlHelper
    {
        private static string urlKey = "PWAURL";
        private static string cookiename = "_PS_BAse_Url";
        private static string urlKeyConf="BaseUrl";
        private static string urllocKeyConf="BaseUrlWsLocation";
        public static bool FindBaseUrl(){
            string res = ConfigurationManager.AppSettings[urlKeyConf ];
            if (!string.IsNullOrEmpty(res))
            {
                ViewConfigurationTask.BaseUrl = ViewConfigurationRow.BaseUrl = res + ConfigurationManager.AppSettings[urllocKeyConf];
               return true;
            }
            res = HttpContext.Current.Request.QueryString[urlKey];
            if (!string.IsNullOrEmpty(res))
            {
                res = res + ConfigurationManager.AppSettings[urllocKeyConf];
                ViewConfigurationBase.BaseUrl =  res;
                HttpCookie newCookie = null;
                if (newCookie == null) newCookie = new HttpCookie(cookiename);
                newCookie.Value = res;
                newCookie.Expires = DateTime.Now.AddYears(1);
                HttpContext.Current.Response.Cookies.Add(newCookie);
                return true;
            }
            HttpCookie cookie = HttpContext.Current.Request.Cookies[cookiename];
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value)){
                  ViewConfigurationBase.BaseUrl =  cookie.Value;
                  cookie.Expires = DateTime.Now.AddYears(1);
                  HttpContext.Current.Response.Cookies.Add(cookie);
                  return true;
            }
            return false;
        }
    }
}