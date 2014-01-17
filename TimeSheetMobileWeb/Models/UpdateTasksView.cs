using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeSheetIBusiness;

namespace TimeSheetMobileWeb.Models
{
    public class UpdateTasksView:UpdateViewBase
    {
        public UpdateTasksView(string defaultLineClass) : base(defaultLineClass)
        {
            Submit = true;
        }
        public UpdateTasksView()
            : base(string.Empty)
        {
        }
    }
}