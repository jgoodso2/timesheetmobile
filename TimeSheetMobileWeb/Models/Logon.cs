using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeSheetMobileWeb.Models
{
    public class Logon
    {
        
            public string UserName { get; set; }
            public string Password { get; set; }
            public string ReturnUrl { get; set; }
    }
}