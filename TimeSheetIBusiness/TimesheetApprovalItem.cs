using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeSheetIBusiness
{
    public class TimesheetApprovalItem
    {
        public string UserNTAccount { get; set; }
        public string UserName { get; set; }
        public List<MyTimesheetApproval> TimesheetApprovals { get; set; }
        public TimesheetApprovalItem()
        {
            TimesheetApprovals = new List<MyTimesheetApproval>();
        }

    }

    public class TaskApprovalItem
    {
        public string UserNTAccount { get; set; }
        public string UserName { get; set; }
        public List<MyTaskApproval> TaskApprovals { get; set; }
        public TaskApprovalItem()
        {
            TaskApprovals = new List<MyTaskApproval>();
        }

    }
}
