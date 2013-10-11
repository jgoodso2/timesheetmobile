﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeSheetMobileWeb.Models;
using TimeSheetIBusiness;
using MVCControlsToolkit.Controller;

namespace TimeSheetMobileWeb.Controllers
{
    public class TimesheetController : Controller
    {
        //
        // GET: /Timesheet/
        protected IRepository repository;
        public static KeyValuePair<int, string>[] AllTimesheetSets;
        public string PWAURL { get; set; }
        public IRepository Repository { get { return repository; } }
        
        static TimesheetController()
        {
            AllTimesheetSets = new KeyValuePair<int, string>[6];
            AllTimesheetSets[0] = new KeyValuePair<int, string>(1, SiteResources.TimesheetsSets1);
            AllTimesheetSets[1] = new KeyValuePair<int, string>(2, SiteResources.TimesheetsSets2);
            AllTimesheetSets[2] = new KeyValuePair<int, string>(3, SiteResources.TimesheetsSets3);
            AllTimesheetSets[3] = new KeyValuePair<int, string>(4, SiteResources.TimesheetsSets4);
            AllTimesheetSets[4] = new KeyValuePair<int, string>(5, SiteResources.TimesheetsSets5);
            AllTimesheetSets[5] = new KeyValuePair<int, string>(6, SiteResources.TimesheetsSets6);
        }
        public TimesheetController(IRepository r)
        {
            repository = r;
           
        }
        [HttpGet()]
        public ActionResult Index(string speriod)
        {
            PeriodSelectedView period = new PeriodSelectedView();
            if (!string.IsNullOrEmpty(speriod))
            {
                string[] dataeArray = speriod.Replace("(", "").Replace(")", "").Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (dataeArray.Length > 2)
                {
                    period.SelectedPeriodStart = Convert.ToDateTime(dataeArray[1]);
                    period.SelectedPeriodStop = Convert.ToDateTime(dataeArray[2]);
                    period.SelectedPeriodId = Repository.GetPeriodID(period.SelectedPeriodStart, period.SelectedPeriodStop);
                    speriod = string.Format("({0} - {1})",  period.SelectedPeriodStart.ToShortDateString(),period.SelectedPeriodStop.ToShortDateString());
                    Session["period"] = speriod;
                }
            }
            else
            {
                if (Session["period"] != null)
                {
                    speriod = Session["period"].ToString();
                    string[] dataeArray = speriod.Replace("(", "").Replace(")", "").Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    period.SelectedPeriodStart = Convert.ToDateTime(dataeArray[0]);
                    period.SelectedPeriodStop = Convert.ToDateTime(dataeArray[1]);
                    period.SelectedPeriodId = Repository.GetPeriodID(period.SelectedPeriodStart, period.SelectedPeriodStop);
                    speriod = string.Format("({0} - {1})", period.SelectedPeriodStart.ToShortDateString(), period.SelectedPeriodStop.ToShortDateString());
                    Session["period"] = speriod;
                }
            }

            this.HttpContext.Trace.Warn("Starting Index of TimesheetController");
            ConfigurationHelper.UserConfiguration(repository, User.Identity as System.Security.Principal.WindowsIdentity);
            UpdateTimesheetsView model = new UpdateTimesheetsView();
            model.PrepareRowTypes();
            Timesheet selection = null;
            model.PeriodString = speriod;
            model.PeriodSelectionInfos = PeriodSelectionView.GetInstance(repository, User.Identity as System.Security.Principal.WindowsIdentity, out selection,TimesheetsSets.Default);
            if (period != null && period.SelectedPeriodId != null)
            {
                selection = new Timesheet();
                selection.Start = period.SelectedPeriodStart;
                selection.Stop = period.SelectedPeriodStop;
                model.PeriodSelectionInfos.TimesheetId=selection.Id = period.SelectedPeriodId;
                model.PeriodSelectionInfos.TimesheetSet = period.SelectedPeriodSet;
            }
            model.PeriodSelectionInfos.IsTask = false;
            if (selection != null)
            {
                model.PeriodSelectionInfos.TimesheetId = selection.Value;
                model.CurrentPeriodStart = selection.Start;
                model.CurrentPeriodStop = selection.Stop;
                model.Period = selection.Id;
                model.PeriodLength = Convert.ToInt32(selection.Stop.Subtract(selection.Start).TotalDays);
                int status;
                bool canDelete;
                bool canRecall;
                TimesheetHeaderInfos tInfos;
                decimal[] totals;
                model.ReceiveRows(repository.GetRows(
                    User.Identity as System.Security.Principal.WindowsIdentity,
                    ViewConfigurationRow.Default,
                    model.Period,
                    model.CurrentPeriodStart,
                    model.CurrentPeriodStop,
                    out status, out canDelete, out canRecall, out tInfos,out totals));
               
                model.Status = model.TimesheetStatusString(status);
                model.HeaderInfos = tInfos;
                model.CanDelete = canDelete;
                model.CanRecall = canRecall;
                model.Totals = totals;


            }
            this.HttpContext.Trace.Warn("Returning from Index of TimesheetController");
            return View(model);
        }

        public ActionResult TimesheetHistory(string speriod)
        {
            this.HttpContext.Trace.Warn("Starting Index of TimesheetController");
            ConfigurationHelper.UserConfiguration(repository, User.Identity as System.Security.Principal.WindowsIdentity);
            PeriodSelectedView period = new PeriodSelectedView();
            TimeSheetHistoryView model = new TimeSheetHistoryView();
            Timesheet selection = null;
            model.PeriodSelectionInfos = PeriodSelectionView.GetInstance(repository, User.Identity as System.Security.Principal.WindowsIdentity, out selection,TimesheetsSets.Default);
            if (period != null && period.SelectedPeriodId != null)
            {
                selection = new Timesheet();
                selection.Start = period.SelectedPeriodStart;
                selection.Stop = period.SelectedPeriodStop;
            }

            model.PeriodString = speriod;
            if (selection != null)
            {
                model.CurrentPeriodStart = selection.Start;
                model.CurrentPeriodStop = selection.Stop;
                model.Period = selection.Id;
                model.PeriodLength = Convert.ToInt32(selection.Stop.Subtract(selection.Start).TotalDays);

                DateTime start, end;
                model.ReceiveRows(repository.SelectTimesheets(
                      User.Identity as System.Security.Principal.WindowsIdentity,
                     TimesheetsSets.Last3,out start,out end));
                selection.Start = start;
                selection.Stop = end;

            }
            this.HttpContext.Trace.Warn("Returning from Index of TimesheetController");
            return View("TimesheetHistory", model);
        }

        [HttpPost]
        public ActionResult TimesheetHistoryRefresh(string speriod)
        {
            this.HttpContext.Trace.Warn("Starting Index of TimesheetController");
            ConfigurationHelper.UserConfiguration(repository, User.Identity as System.Security.Principal.WindowsIdentity);
            PeriodSelectedView period = new PeriodSelectedView();
            TimeSheetHistoryView model = new TimeSheetHistoryView();
            Timesheet selection = null;
            int selctedset = Convert.ToInt32(speriod);
            model.PeriodSelectionInfos = PeriodSelectionView.GetInstance(repository, User.Identity as System.Security.Principal.WindowsIdentity, out selection,TimesheetsSets.Default);
            if (period != null && period.SelectedPeriodId != null)
            {
                selection = new Timesheet();
                selection.Start = period.SelectedPeriodStart;
                selection.Stop = period.SelectedPeriodStop;
            }

            model.PeriodString = speriod;
            if (selection != null)
            {
                model.CurrentPeriodStart = selection.Start;
                model.CurrentPeriodStop = selection.Stop;
                model.Period = selection.Id;
                model.PeriodLength = Convert.ToInt32(selection.Stop.Subtract(selection.Start).TotalDays);

                DateTime start, end;
                model.ReceiveRows(repository.SelectTimesheets(
                      User.Identity as System.Security.Principal.WindowsIdentity,
                     (TimesheetsSets) selctedset, out start, out end));
                selection.Start = start;
                selection.Stop = end;

            }
            this.HttpContext.Trace.Warn("Returning from Index of TimesheetController");
            return PartialView("TSGridTemplate", model);
        }
        [HttpPost]
        public ActionResult Refresh(string speriod)
        {
            ConfigurationHelper.UserConfiguration(repository, User.Identity as System.Security.Principal.WindowsIdentity);
            PeriodSelectedView pmodel = new PeriodSelectedView();
            if (!string.IsNullOrEmpty(speriod))
            {
                string[] dataeArray = speriod.Replace("(", "").Replace(")", "").Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (dataeArray.Length > 2)
                {
                    pmodel.SelectedPeriodStart = Convert.ToDateTime(dataeArray[0]);
                    pmodel.SelectedPeriodStop = Convert.ToDateTime(dataeArray[1]);
                    pmodel.SelectedPeriodId = Repository.GetPeriodID(pmodel.SelectedPeriodStart, pmodel.SelectedPeriodStop);
                    speriod = string.Format("({0} - {1})",  pmodel.SelectedPeriodStart.ToShortDateString(), pmodel.SelectedPeriodStop.ToShortDateString());
                    Session["period"] = speriod;
                }
            }
            else
            {
                if (Session["period"] != null)
                {
                    speriod = Session["period"].ToString();
                    string[] dataeArray = speriod.Replace("(", "").Replace(")", "").Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    pmodel.SelectedPeriodStart = Convert.ToDateTime(dataeArray[0]);
                    pmodel.SelectedPeriodStop = Convert.ToDateTime(dataeArray[1]);
                    pmodel.SelectedPeriodId = Repository.GetPeriodID(pmodel.SelectedPeriodStart, pmodel.SelectedPeriodStop);
                    speriod = string.Format("({0} - {1})", pmodel.SelectedPeriodStart.ToShortDateString(), pmodel.SelectedPeriodStop.ToShortDateString());
                    Session["period"] = speriod;
                }
            }
            this.HttpContext.Trace.Warn("Starting Refresh of TimesheetController");
            UpdateTimesheetsView model = new UpdateTimesheetsView();
            model.CurrentPeriodStart = pmodel.SelectedPeriodStart;
            model.CurrentPeriodStop = pmodel.SelectedPeriodStop;
            model.Period = pmodel.SelectedPeriodId;
            int status;
            bool canDelete;
            bool canRecall;
            TimesheetHeaderInfos tInfos;
            decimal[] totals;
            model.ReceiveRows(repository.GetRows(
                User.Identity as System.Security.Principal.WindowsIdentity,
                ViewConfigurationRow.Default,
                model.Period,
                model.CurrentPeriodStart,
                model.CurrentPeriodStop,
                out status, out canDelete, out canRecall, out tInfos,out totals));
            
            model.Totals = totals;
            model.HeaderInfos = tInfos;
            model.Status = model.TimesheetStatusString(status);
            model.CanDelete = canDelete;
            model.CanRecall = canRecall;
            this.HttpContext.Trace.Warn("Returning from Refresh of TimesheetController");
            return PartialView("Edit", model);
        }
        [ChildActionOnly]
        public ActionResult TaskSelection()
        {
            this.HttpContext.Trace.Warn("Starting TaskSelection of TimesheetController");
            TaskSelectionView model = new TaskSelectionView();
            model.Title = SiteResources.HomeMenuAddRow;
            model.Projects = new List<ProjectInfo>() { new ProjectInfo { Id = "-1", Name = ViewConfigurationRow.Default.AdminDescription } }.Concat(repository.UserProjects(User.Identity as System.Security.Principal.WindowsIdentity));
            model.IsInTask = false;
            model.PrepareRowTypes();
            this.HttpContext.Trace.Warn("Returning from TaskSelection of TimesheetController");
            return PartialView(model);
        }


        [System.Web.Mvc.OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult Timesheets(int? iset)
        {
            this.HttpContext.Trace.Warn("Starting Timesheets of TimesheetController");
            TimesheetsSets set = TimesheetsSets.Default;
            if (iset.HasValue) set = (TimesheetsSets)iset.Value;
            DateTime start, end;
            var res = MVCControlsToolkit.Controls.ChoiceListHelper.Create(repository.SelectTimesheets(
                    User.Identity as System.Security.Principal.WindowsIdentity,
                    set,out start,out end).OrderByDescending(m => m.Start),
                                m => m.Value,
                                m => m.Description).PrepareForJson();
            this.HttpContext.Trace.Warn("Returning from Timesheets of TimesheetController");
            return Json(
                res,
                JsonRequestBehavior.AllowGet);
        }

        [ChildActionOnly]
        [HttpGet]
        public ActionResult RecallDelete()
        {
            return PartialView();
        }


        
        [HttpPost]
        public ActionResult Edit(UpdateTimesheetsView model)
        {
            ConfigurationHelper.UserConfiguration(repository, User.Identity as System.Security.Principal.WindowsIdentity);
            this.HttpContext.Trace.Warn("Starting Edit of TimesheetController");
            var erors = DebugHelper.ModelStateErrors(ModelState);
            if (ModelState.IsValid)
            {
                ModelState.Clear();
                var toUpdate = new List<Tracker<BaseRow>>();
                if (model.PeriodRows != null)
                {
                    foreach(var x in model.PeriodRows)
                    {
                        if (x.Value != null) toUpdate.Add(x);
                        else if (x.OldValue != null) 
                        {
                            x.Value = x.OldValue.GetCopy();
                            x.Value.DayTimes = new List<decimal?>();
                            x.Changed = true;
                            foreach (var y in x.OldValue.DayTimes) x.Value.DayTimes.Add(0m);
                            toUpdate.Add(x);
                        }
                    }
                }
                model.PeriodRows = toUpdate;
                
                try
                {
                    
                    ErrorHandlingHelpers.UpdateRows(repository, model,
                        User.Identity as System.Security.Principal.WindowsIdentity,
                        ViewConfigurationRow.Default,
                        model.Period,
                        model.CurrentPeriodStart,
                        model.CurrentPeriodStop,
                        model.PeriodRows, 
                        model.Submit);
                    int status;
                    bool canDelete;
                    bool canRecall;
                    TimesheetHeaderInfos tInfos;
                    decimal[] totals;
                    model.ReceiveRows(repository.GetRows(
                        User.Identity as System.Security.Principal.WindowsIdentity,
                        ViewConfigurationRow.Default,
                        model.Period,
                        model.CurrentPeriodStart,
                        model.CurrentPeriodStop,
                        out status, out canDelete, out canRecall, out tInfos,out totals));
                   
                    model.HeaderInfos = tInfos;
                    model.Status = model.TimesheetStatusString(status);
                    model.CanDelete = canDelete;
                    model.CanRecall = canRecall;
                    model.Totals = totals;
                }
                catch
                {
                    this.HttpContext.Trace.Warn("Error returning from Edit of TimesheetController");
                    return PartialView(model);
                }
            }
            this.HttpContext.Trace.Warn("Returning from Edit of TimesheetController");
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult RowSingleValues(RowSelectionInfosView selection)
        {
            ConfigurationHelper.UserConfiguration(repository, User.Identity as System.Security.Principal.WindowsIdentity);
            this.HttpContext.Trace.Warn("Starting RowSingleValues of TimesheetController");
            BaseRow res = repository.GetRowSingleValues(
                User.Identity as System.Security.Principal.WindowsIdentity,
                ViewConfigurationRow.Default,
                selection.RequiredProgectId,
                selection.RequiredPeriodIStart,
                selection.RequiredPeriodIStop,
                selection.RequiredProgectId,
                selection.RequiredAssignementId,
                UpdateViewBase.RowTypeFromCode(selection.RequiredRowType));
            res.ProjectId = selection.RequiredProgectId;
            res.AssignementId = selection.RequiredAssignementId;
            res.AssignementName = selection.RequiredAssignementName;
            res.ProjectName = selection.RequiredProjectName;
            if (res.DayTimes == null)
            {
                res.DayTimes = new List<decimal?>();
                for (int i = 0; i < Convert.ToInt32(selection.RequiredPeriodIStop.Subtract(selection.RequiredPeriodIStart).TotalDays); i++)
                {
                    res.DayTimes.Add(0);
                }
            }
            
            
            this.HttpContext.Trace.Warn("Returning from RowSingleValues of TimesheetController");    
            return Json(res);
        }

        [HttpPost]
        public ActionResult CustomFields(IList<CustomFieldItem> selection)
        {

            if (selection != null)
            {
                CustomFieldsView model = new CustomFieldsView() { CustomFieldItems = selection.ToList() };
                return PartialView("CustomFieldDetail", model);
            }
            return PartialView();
        }
        [HttpPost]
        public ActionResult RecallDelete(RecallDeleteView model)
        {

            this.HttpContext.Trace.Warn("Starting RecallDelete of TimesheetController");
            repository.RecallDelete(
                    User.Identity as System.Security.Principal.WindowsIdentity,
                    model.RDPeriodId,
                    model.RDPeriodIStart,
                    model.RDPeriodIStop,
                    model.IsRecall
                    );
            this.HttpContext.Trace.Warn("Returning from RecallDelete of TimesheetController");
            return Json(new ConfirmationView { Success = true, IsRecall=model.IsRecall });
        }
        
    }
}
