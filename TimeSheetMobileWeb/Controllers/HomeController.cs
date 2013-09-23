using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeSheetIBusiness;

namespace TimeSheetMobile.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
       
        public ActionResult Index()
        {
            this.HttpContext.Trace.Warn("starting Index of HomeController");
            this.HttpContext.Trace.Warn("Returning from Index of HomeController");
            return View();
            
        }
    }
}
