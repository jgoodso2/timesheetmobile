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

        

        [ChildActionOnly]
        [HttpGet]
        public ActionResult UpdateSummary()
        {
            return PartialView("UpdateSummary");
        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            Session["CurrentUser"] = repository.GetUserName((User.Identity as System.Security.Principal.WindowsIdentity).Name);
        }
        public ActionResult Index(string projectId,string user)
        {
            ConfigurationHelper.UserConfiguration(repository, (User.Identity as System.Security.Principal.WindowsIdentity).Name);
            this.HttpContext.Trace.Warn("starting Index of TasksController");
            UpdateTasksView model = new UpdateTasksView(repository.DefaultLineClass);
            model.PrepareRowTypes();
            Timesheet selection = null;
            model.PeriodSelectionInfos = PeriodSelectionView.GetInstance(repository, (User.Identity as System.Security.Principal.WindowsIdentity).Name, out selection,TimesheetsSets.All);
           
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
                string timesheetUser;
                timesheetUser = Session["user"] == null ? (User.Identity as System.Security.Principal.WindowsIdentity).Name : Session["user"].ToString();
                if (!string.IsNullOrEmpty(user))
                {
                    model.ReceiveRows(repository.GetSubmittedRows(projectId, (User.Identity as System.Security.Principal.WindowsIdentity).Name,user.Replace("i:0#.w|",""), ViewConfigurationApproval.Default));
                    model.ApprovalMode = true;
                }
                else
                {
                    model.ReceiveRows(repository.GetRows(
                   timesheetUser,
                   ViewConfigurationTask.Default,
                   model.Period,
                   model.CurrentPeriodStart,
                   model.CurrentPeriodStop,
                   out status, out canDelete, out canRecall, out tInfos, out totals));
                }
              


            }
            this.HttpContext.Trace.Warn("Returning from Index of TasksController");
            return View(model);
        }

        [HttpPost]
        public ActionResult Refresh(PeriodSelectedView period)
        {
            ConfigurationHelper.UserConfiguration(repository, (User.Identity as System.Security.Principal.WindowsIdentity).Name);
            this.HttpContext.Trace.Warn("Starting Refresh of TasksController");
            UpdateTasksView model = new UpdateTasksView(repository.DefaultLineClass);
            model.PrepareRowTypes();
            Timesheet selection = null;
            model.PeriodSelectionInfos = PeriodSelectionView.GetInstance(repository, (User.Identity as System.Security.Principal.WindowsIdentity).Name, out selection, TimesheetsSets.All);
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
                string timesheetUser;
                timesheetUser = Session["user"] == null ? (User.Identity as System.Security.Principal.WindowsIdentity).Name
                    : Session["user"].ToString();
                model.ReceiveRows(repository.GetRows(
                    timesheetUser,
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
            model.Projects = repository.UserProjects((User.Identity as System.Security.Principal.WindowsIdentity).Name);
            model.IsInTask = true;
            model.PrepareRowTypes();
            this.HttpContext.Trace.Warn("Returning from TaskSelection of TasksController");
            return PartialView(model);
        }
        public string ProjectTasks(string projectId)
        {
            this.HttpContext.Trace.Warn("Starting ProjectTasks of TasksController");
              var assignments = repository.ProjectAssignements(
                    (User.Identity as System.Security.Principal.WindowsIdentity).Name,
                    projectId);
            string returnValue = "<select id='assignments' class='dynamictasks'>";
            returnValue += "<option value='' name=''>" + SiteResources.AssignementPrompt + "</option>";
            foreach(var assignment in assignments)
            {
                returnValue += "<option value='" + assignment.Id + "'>" + assignment.Name + "</option>";
            }
            returnValue += "</select>";
            this.HttpContext.Trace.Warn("Returning from ProjectTasks of TasksController");
           return returnValue;
        }
        
        [ChildActionOnly]
        [HttpGet]
        public ActionResult RecallDelete()
        {
            return PartialView(new RecallDeleteView() { IsTask = true});
        }

        [HttpPost]
        public ActionResult Edit(UpdateTasksView model)
        {
            ConfigurationHelper.UserConfiguration(repository, (User.Identity as System.Security.Principal.WindowsIdentity).Name);
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
                model.PeriodSelectionInfos = PeriodSelectionView.GetInstance(repository, (User.Identity as System.Security.Principal.WindowsIdentity).Name, out selection, TimesheetsSets.All);
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
                    ErrorHandlingHelpers.UpdateRows(model.ApprovalMode, repository, model,
                        (User.Identity as System.Security.Principal.WindowsIdentity).Name,
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
                     string timesheetUser;
                     timesheetUser = Session["user"] == null ? (User.Identity as System.Security.Principal.WindowsIdentity).Name : Session["user"].ToString();
                    model.ReceiveRows(repository.GetRows(
                        timesheetUser,
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
            ConfigurationHelper.UserConfiguration(repository, (User.Identity as System.Security.Principal.WindowsIdentity).Name);
            this.HttpContext.Trace.Warn("Starting RowSingleValues of TasksController");
            BaseRow res = repository.GetRowSingleValues(
                (User.Identity as System.Security.Principal.WindowsIdentity).Name,
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

        [HttpPost]
        public ActionResult ApproveSelectedTasks(string[] assignments,string mode)
        {
            string errorMessage="";
            bool success;
            foreach (var row in assignments)
                {
                    
                    try
                    {
                        repository.ApproveTasks(assignments, repository.GetResourceUidFromNtAccount((User.Identity as System.Security.Principal.WindowsIdentity).Name).ToString(), mode);
                        
                            errorMessage = SiteResources.ApprovalSuccessful;
                            success = true;
                       
                    }

                    catch (Exception ex)
                    {
                        errorMessage = SiteResources.ApprovalError;
                        success = false;
                        break;
                    }
                }
            return Json(new ConfirmationView
            {
                Success = true,
                ErrorMessage = errorMessage
                ,
                ReturnUrl = HttpContext.Request.Url.AbsoluteUri.ToString()
            });
            }
        

    }
}
