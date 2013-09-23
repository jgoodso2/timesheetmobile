using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeSheetMobileWeb.Models
{
    public class ViewSelectionView
    {
        public string TaskUpdatorViewId { get; set; }
        public string TimesheetViewId { get; set; }
        public bool IsTask { get; set; }
    }
}