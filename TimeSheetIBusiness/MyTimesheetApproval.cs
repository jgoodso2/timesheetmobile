using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeSheetIBusiness
{
    public class MyTimesheetApproval : MyApprovalBase
    {
        public string Hours { get; set; }
        public string Name { get; set; }
        public string Period { get; set; }
    }
}