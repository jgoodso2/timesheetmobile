using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeSheetIBusiness;

namespace TimeSheetMobileWeb.Models
{
    
    public class UpdateTimesheetsView:UpdateViewBase
    {
        public string CurrentUserGuid { get; set; }
        public UpdateTimesheetsView(string defaultLineClass)
            : base(defaultLineClass)
        {
            
        }
        public UpdateTimesheetsView()
            : base(string.Empty)
        {
            
        }
    }
}