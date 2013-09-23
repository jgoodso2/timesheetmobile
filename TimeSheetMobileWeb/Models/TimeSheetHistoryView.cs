using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using TimeSheetIBusiness;
using MVCControlsToolkit.Controller;

namespace TimeSheetMobileWeb.Models
{
    public class TimeSheetHistoryView
    {
        public List<TimeSheetView> TimeSheets { get; set; }
        public PeriodSelectionView PeriodSelectionInfos { get; set; }
        [Display(ResourceType = typeof(SiteResources), Name = "HomeMenuPeriods")]
        public string Period { get; set; }
        public string PeriodString { get; set; }
        public DateTime CurrentPeriodStart { get; set; }
        public DateTime CurrentPeriodStop { get; set; }
        public int PeriodLength { get; set; }
        public List<Tracker<TimeSheetView>> PeriodTimesheets { get; set; }
        

        public void ReceiveRows(IEnumerable<Timesheet> rows)
        {
            List<Tracker<TimeSheetView>> res = new List<Tracker<TimeSheetView>>();
            foreach (Timesheet row in rows)
            {
                TimeSheetView timesheet = new TimeSheetView() { Hours = row.Hours, IsCreated = row.IsCreated, Period = row.Period, Status = row.Status };
                res.Add(new Tracker<TimeSheetView>(timesheet));
            }
            PeriodTimesheets = res;
        }
    }
}