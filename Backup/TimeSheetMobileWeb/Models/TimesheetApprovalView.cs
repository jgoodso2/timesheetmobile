using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeSheetMobileWeb.Models
{
    public class TimesheetApprovalView
    {
        public string TSUID { get; set; }
        public string MGRUID { get; set; }
        public string ErrorMessage { get; set; }

        public bool Success { get; set; }
    }
}