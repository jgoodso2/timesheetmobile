using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeSheetMobileWeb.Models
{
    public class ConfirmationView
    {
        public bool Success { get; set; }
        public bool IsRecall { get; set; }

        public string ErrorMessage { get; set; }

        public string ReturnUrl { get; set; }
    }
}