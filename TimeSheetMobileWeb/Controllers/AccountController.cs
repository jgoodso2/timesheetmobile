using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeSheetIBusiness;
using System.Web.Security;

namespace TimeSheetMobileWeb.Controllers
{
    public class AccountController : Controller
    {
        
         protected IRepository repository;

        public IRepository Repository
        {
            get
            {
                return repository;
            }
        }

        public AccountController(IRepository r)
        {
            repository = r;
           
        }

        ActionResult Index()
        {
            return View();
        }

        
        public ActionResult Logon()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Logon(TimeSheetIBusiness.Logon model)
        {
            Session["UserName"] = model.UserName;
            Session["Password"] = model.Password;
            Repository.SetClientEndpointsProg(this.HttpContext.Items["PWAURL"].ToString());
            if (ModelState.IsValid)
            {
                if (Repository.LogonToProjectServer(model.UserName, model.Password))
                {

                    if (string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        FormsAuthentication.RedirectFromLoginPage(model.UserName, false);
                    }
                    //If ReturnUrl query string parameter is not present, 
                    //then we need to generate authentication token and redirect 
                    //the user to any page ( according to your application need). 
                    //FormsAuthentication.SetAuthCookie() 
                    //method will generate Authentication token 
                    else
                    {
                        FormsAuthentication.SetAuthCookie(model.UserName, false);
                        Redirect("Home/Index");
                    }
                   
                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

    }
}
