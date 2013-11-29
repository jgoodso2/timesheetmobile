using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeSheetMobileWeb.Models;
using TimeSheetIBusiness;
using MVCControlsToolkit.Controller;

namespace TimeSheetMobileWeb.Controllers
{
    public class TasksController : Controller
    {
        //
        // GET: /Tasks/

        protected IRepository repository;
        

        public IRepository Repository
        {
            get
            {
                return repository;
            }
        }
        public TasksController(IRepository r)
        {
            repository = r;
        }
        public ActionResult Index(PeriodSelectedView period)
        {
            ConfigurationHelper.UserConfiguration(repository, User.Identity as System.Security.Principal.WindowsIdentity);
            this.HttpContext.Trace.Warn("starting Index of TasksController");
            UpdateTasksView model = new UpdateTasksView();
            model.PrepareRowTypes();
            Timesheet selection = null;
            model.PeriodSelectionInfos = PeriodSelectionView.GetInstance(repository, User.Identity as System.Security.Principal.WindowsIdentity, out selection,TimesheetsSets.All);
            if (period != null && period.SelectedPeriodId != null)
            {
                selection = new Timesheet();
                selection.Start = period.SelectedPeriodStart;
                selection.Stop = period.SelectedPeriodStop;
                model.PeriodSelectionInfos.TimesheetId = selection.Id = period.SelectedPeriodId;
                model.PeriodSelectionInfos.TimesheetSet = period.SelectedPeriodSet;
            }
            model.PeriodSelectionInfos.IsTask = true; 

            if (selection != null)
            {
                model.PeriodSelectionInfos.TimesheetId = selection.Value;
                model.CurrentPeriodStart = selection.Start;
                model.CurrentPeriodStop = selection.Stop;
                model.Period = selection.Id;
                model.PeriodLength = (int)(selection.Stop.Subtract(selection.Start).TotalDays) + 1;
                int status;
                bool canDelete;
                bool canRecall;
                TimesheetHeaderInfos tInfos;
                decimal[] totals;
                model.ReceiveRows(repository.GetRows(
                    User.Identity as System.Security.Principal.WindowsIdentity,
                    ViewConfigurationTask.Default,
                    model.Period,
                    model.CurrentPeriodStart,
                    model.CurrentPeriodStop,
                    out status, out canDelete, out canRecall, out tInfos, out totals));
                model.Status = model.TimesheetStatusString(status);

                model.CanDelete = canDelete;
                model.CanRecall = canRecall;


            }
            this.HttpContext.Trace.Warn("Returning from Index of TasksController");
            return View(model);
        }

        [HttpPost]
        public ActionResult Refresh(PeriodSelectedView period)
        {
            ConfigurationHelper.UserConfiguration(repository, User.Identity as System.Security.Principal.WindowsIdentity);
            this.HttpContext.Trace.Warn("Starting Refresh of TasksController");
            UpdateTasksView model = new UpdateTasksView();
            model.PrepareRowTypes();
            Timesheet selection = null;
            model.PeriodSelectionInfos = PeriodSelectionView.GetInstance(repository, User.Identity as System.Security.Principal.WindowsIdentity, out selection, TimesheetsSets.All);
            if (period != null && period.SelectedPeriodId != null)
            {
                selection = new Timesheet();
                selection.Start = period.SelectedPeriodStart;
                selection.Stop = period.SelectedPeriodStop;
                model.PeriodSelectionInfos.TimesheetId = selection.Id = period.SelectedPeriodId;
                model.PeriodSelectionInfos.TimesheetSet = period.SelectedPeriodSet;
            }
            model.PeriodSelectionInfos.IsTask = true;

            if (selection != null)
            {
                model.PeriodSelectionInfos.TimesheetId = selection.Value;
                model.CurrentPeriodStart = selection.Start;
                model.CurrentPeriodStop = selection.Stop;
                model.Period = selection.Id;
                model.PeriodLength = (int)(selection.Stop.Subtract(selection.Start).TotalDays) + 1;
                int status;
                bool canDelete;
                bool canRecall;
                TimesheetHeaderInfos tInfos;
                decimal[] totals;
                model.ReceiveRows(repository.GetRows(
                    User.Identity as System.Security.Principal.WindowsIdentity,
                    ViewConfigurationTask.Default,
                    model.Period,
                    model.CurrentPeriodStart,
                    model.CurrentPeriodStop,
                    out status, out canDelete, out canRecall, out tInfos,out totals));

                model.Status = model.TimesheetStatusString(status);
                model.Totals = totals;
                model.CanDelete = canDelete;
                model.CanRecall = canRecall;
                


            }
            this.HttpContext.Trace.Warn("Returning from Refresh of TasksController");
            return PartialView("Edit", model);
        }

        [ChildActionOnly]
        public ActionResult TaskSelection()
        {
            this.HttpContext.Trace.Warn("Starting TaskSelection of TasksController");
            TaskSelectionView model = new TaskSelectionView();
            model.Title = SiteResources.HomeMenuAddTask;
            model.Projects = repository.UserProjects(User.Identity as System.Security.Principal.WindowsIdentity);
            model.IsInTask = true;
            model.PrepareRowTypes();
            this.HttpContext.Trace.Warn("Returning from TaskSelection of TasksController");
            return PartialView(model);
        }
        [System.Web.Mvc.OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ProjectTasks(string projectId)
        {
            this.HttpContext.Trace.Warn("Starting ProjectTasks of TasksController");
            var res = MVCControlsToolkit.Controls.ChoiceListHelper.Create(repository.ProjectAssignements(
                    User.Identity as System.Security.Principal.WindowsIdentity,
                    projectId),
                                m => m.Id,
                                m => m.Name,null).PrepareForJson();
            this.HttpContext.Trace.Warn("Returning from ProjectTasks of TasksController");
            return Json(
                res,
                JsonRequestBehavior.AllowGet);
        }
        

        [HttpPost]
        public ActionResult Edit(UpdateTasksView model)
        {
            ConfigurationHelper.UserConfiguration(repository, User.Identity as System.Security.Principal.WindowsIdentity);
            this.HttpContext.Trace.Warn("Starting Edit of TasksController");
            if (ModelState.IsValid)
            {
                ModelState.Clear();
                var toUpdate = new List<Tracker<BaseRow>>();
                if (model.PeriodRows != null)
                {
                    foreach (var x in model.PeriodRows)
                    {
                        if (x.Value != null) toUpdate.Add(x);
                    }
                }
                model.PeriodRows = toUpdate;
                Timesheet selection = null;
                model.PeriodSelectionInfos = PeriodSelectionView.GetInstance(repository, User.Identity as System.Security.Principal.WindowsIdentity, out selection, TimesheetsSets.All);
               if (selection != null)
               {
                   model.PeriodSelectionInfos.TimesheetId = selection.Value;
                   model.CurrentPeriodStart = selection.Start;
                   model.CurrentPeriodStop = selection.Stop;
                   model.Period = selection.Id;
                   model.PeriodLength = (int)(selection.Stop.Subtract(selection.Start).TotalDays) + 1;
               }

                try
                {
                    ErrorHandlingHelpers.UpdateRows(repository, model,
                        User.Identity as System.Security.Principal.WindowsIdentity,
                        ViewConfigurationTask.Default,
                        model.Period,
                        model.CurrentPeriodStart,
                        model.CurrentPeriodStop,
                        model.PeriodRows, model.Submit);
                    int status;
                    bool canDelete;
                    bool canRecall;
                    TimesheetHeaderInfos tInfos;
                    decimal[] totals;
                    model.ReceiveRows(repository.GetRows(
                        User.Identity as System.Security.Principal.WindowsIdentity,
                        ViewConfigurationTask.Default,
                        model.Period,
                        model.CurrentPeriodStart,
                        model.CurrentPeriodStop,
                        out status, out canDelete, out canRecall, out tInfos,out totals));
                    model.Status = model.TimesheetStatusString(status);
                    model.Totals = totals;
                    model.CanDelete = canDelete;
                    model.CanRecall = canRecall;

                }
                catch
                {
                    this.HttpContext.Trace.Warn("Error Returning from Edit of TasksController");
                    return PartialView(model);
                }
            }
            this.HttpContext.Trace.Warn("Returning from Edit of TasksController");
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult RowSingleValues(RowSelectionInfosView selection)
        {
            ConfigurationHelper.UserConfiguration(repository, User.Identity as System.Security.Principal.WindowsIdentity);
            this.HttpContext.Trace.Warn("Starting RowSingleValues of TasksController");
            BaseRow res = repository.GetRowSingleValues(
                User.Identity as System.Security.Principal.WindowsIdentity,
                ViewConfigurationTask.Default,
                selection.RequiredProgectId,
                selection.RequiredPeriodIStart,
                selection.RequiredPeriodIStop,
                selection.RequiredProgectId,
                selection.RequiredAssignementId,
                selection.RequiredAssignementName,
                selection.RequiredLineClassId,
                UpdateViewBase.RowTypeFromCode(selection.RequiredRowType));
            res.ProjectId = selection.RequiredProgectId;
            res.AssignementId = selection.RequiredAssignementId;
            res.AssignementName = selection.RequiredAssignementName;
            res.ProjectName = selection.RequiredProjectName;
            if (res.DayTimes == null)
            {
                res.DayTimes = new List<decimal?>();
                for (int i = 0; i < Convert.ToInt32(selection.RequiredPeriodIStop.Subtract(selection.RequiredPeriodIStart).TotalDays + 1); i++)
                {
                    res.DayTimes.Add(0);
                }
            }
            this.HttpContext.Trace.Warn("Returning from RowSingleValues of TasksController");
            return Json(res);
        }
    }
}
