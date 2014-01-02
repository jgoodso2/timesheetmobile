using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeSheetIBusiness
{
    public class MyApprovalBase
    {
        public string User {get;set;}
        public string ProjectId { get; set; }
        public string Approver { get; set; }
        public string UserNTAccount { get; set; }
    }
}