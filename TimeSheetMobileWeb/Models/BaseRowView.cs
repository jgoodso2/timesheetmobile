using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeSheetIBusiness;

namespace TimeSheetMobileWeb.Models
{
    public class BaseRowView: BaseRow
    {
        public bool TaskRow { get; set; }
        public string Title { get; set; }
        public int PeriodLength;
        public string Status { get; set; }

        public bool ApprovalMode { get; set; }

        public BaseRowView()
            : base()
        {
            Status = "";
        }
    }
}