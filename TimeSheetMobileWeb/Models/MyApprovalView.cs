using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeSheetIBusiness;

namespace TimeSheetMobileWeb.Models
{
    public class MyApprovalView
    {
        //TODO : think this constructor is redandant . Verify and remove it later
        public MyApprovalView()
        {
            IsTask = false;
        }
        public List<TimesheetApprovalItem> TimesheetApprovals
        {
            get;set;
        }

        public List<TaskApprovalItem> TaskApprovals
        {
            get;
            set;
        }

        public bool IsTask { get; set; }
        public string NextMgr {get;set;}

        public string ErrorMessage { get; set; }

        public bool Success { get; set; }
    }
}