using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeSheetIBusiness
{
    public class MyTaskApproval : MyApprovalBase
    {
        public string ProjectName { get; set; }
        public bool Selected { get; set; }
    }
}