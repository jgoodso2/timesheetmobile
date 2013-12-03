using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeSheetIBusiness;

namespace TimeSheetMobileWeb.Models
{
    public class MyApprovalView
    {
        public Dictionary<string,List<MyTimesheetApproval>> TimesheetApprovals { get;set;}
    }
}