using System;
using System.Web;

namespace TimeSheetMobileWeb
{
    public class PwaUrlModule : IHttpModule
    {
        /// <summary>
        /// You will need to configure this module in the web.config file of your
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpModule Members

        public void Dispose()
        {
            //clean-up code here.
        }

        public void Init(HttpApplication context)
        {
            // Below is an example of how you can handle LogRequest event and provide 
            // custom logging implementation for it
            //context.LogRequest += new EventHandler(OnLogRequest);
            context.BeginRequest += new EventHandler(context_BeginRequest);
        }

       

        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication app = sender as HttpApplication;
            if (!(app.Context.Request.QueryString["PWAURL"] == null || app.Context.Request.QueryString["PWAURL"].ToString() == string.Empty))
            {
                 HttpCookie cookie = new HttpCookie("PWAURL", HttpUtility.UrlDecode(app.Context.Request.QueryString["PWAURL"].ToString()));
                 cookie.Expires = DateTime.Now.AddYears(1);
                 app.Context.Response.Cookies.Add(cookie);
                app.Context.Items["PWAURL"] = HttpUtility.UrlDecode(app.Context.Request.QueryString["PWAURL"].ToString());
            }
            else
            {
                if (app.Context.Request.Cookies["PWAURL"] == null || app.Context.Request.Cookies["PWAURL"].ToString() == string.Empty)
                {
                    throw new ArgumentException("PWA Url cookie not found");
                }
                else
                {
                    //This is for the duration of the request and has to be set on every begin request
                    app.Context.Items["PWAURL"] = app.Context.Request.Cookies["PWAURL"].Value;
                }
            }  
        }

        #endregion

        public void OnLogRequest(Object source, EventArgs e)
        {
            //custom logging logic can go here
        }

        public void OnAuthenticateRequest(Object sender, EventArgs e)
        {
           
        }
    }
}
