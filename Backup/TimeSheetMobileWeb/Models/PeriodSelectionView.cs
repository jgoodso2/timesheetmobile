using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeSheetIBusiness;
using System.Security.Principal;

namespace TimeSheetMobileWeb.Models
{
    public class PeriodSelectionView
    {
        public bool IsTask { get; set; }
        public KeyValuePair<int, string>[] AllTimesheetsSets { get; set; }
        public int TimesheetSet { get; set; }
        public IEnumerable<Timesheet> TimesheetsSets { get; set; }
        public string TimesheetId { get; set; }
        public static KeyValuePair<int, string>[] AllTimesheetSets;
        static PeriodSelectionView()
        {
            lock(new object())
            {
            AllTimesheetSets = new KeyValuePair<int, string>[6];
            AllTimesheetSets[0] = new KeyValuePair<int, string>(1, SiteResources.TimesheetsSets1);
            AllTimesheetSets[1] = new KeyValuePair<int, string>(2, SiteResources.TimesheetsSets2);
            AllTimesheetSets[2] = new KeyValuePair<int, string>(3, SiteResources.TimesheetsSets3);
            AllTimesheetSets[3] = new KeyValuePair<int, string>(4, SiteResources.TimesheetsSets4);
            AllTimesheetSets[4] = new KeyValuePair<int, string>(5, SiteResources.TimesheetsSets5);
            AllTimesheetSets[5] = new KeyValuePair<int, string>(6, SiteResources.TimesheetsSets6);
            }
        }
        public static Timesheet DefaultTimesheet(IEnumerable<Timesheet> choices)
        {
            if (choices == null) return null;
            long dist = long.MaxValue;
            Timesheet bestFit = null;
            DateTime Now = DateTime.Now;
            foreach (Timesheet choice in choices)
            {
                if (Now >= choice.Start && Now < choice.Stop) return choice;
                long currFit = Math.Abs(Now.Subtract(choice.Start).Ticks);
                currFit = Math.Min(currFit, Math.Abs(Now.Subtract(choice.Stop).Ticks));
                if (currFit < dist)
                {
                    dist = currFit;
                    bestFit = choice;
                }
            }
            return bestFit;
        }
        public static PeriodSelectionView GetInstance(IRepository repository, string user, out Timesheet selection,TimesheetsSets set)
        {
            PeriodSelectionView model = new PeriodSelectionView();
            model.AllTimesheetsSets = AllTimesheetSets;
            model.TimesheetSet = Convert.ToInt32(repository.DefaultTimesheetSet);
            DateTime start,end;
            model.TimesheetsSets = repository.SelectTimesheets(
                    user,
                    set, out start, out end);

            selection = new Timesheet() { Start = start, Stop = end };
            //if (selection != null )model.TimesheetId = selection.Value;
            return model;
        }

        
    }

    public class PeriodSelectedView
    {
        public int SelectedPeriodSet { get; set; }
        public string SelectedPeriodId { get; set; }
        public DateTime SelectedPeriodStart { get; set; }
        public DateTime SelectedPeriodStop { get; set; }

        
    }
       
}