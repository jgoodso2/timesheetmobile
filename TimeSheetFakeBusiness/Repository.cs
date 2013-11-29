using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeSheetIBusiness;
using System.Security.Principal;
using MVCControlsToolkit.Controller;

namespace TimeSheetFakeBusiness
{
    public class Repository: IRepository
    {
        public System.Security.Principal.WindowsIdentity AppPoolUser { get; set; }
        public UserConfigurationInfo UserConfiguration(WindowsIdentity user, string rowField, string taskField)
        {
            return new UserConfigurationInfo { TaskViewId = null, RowViewId = "RComplete" };
        }
        public CustomFieldInfo GetCustomFieldType(Guid guid,int t, string property)
        {
            return null;
        }
        public LookupTableDisplayItem[] GetLookupTableValuesAsItems(Guid tableUid, string dataType)
        {
            return null;
        }
        public List<CustomFieldItem> GetCustomFields(List<CustomField> fields, string assignementId, DateTime start, DateTime stop)
        {
            return null;
        }
        public bool SetClientEndpointsProg(string pwaUrl)
        {
            return true;
        }
        public void ChangeUserConfiguration(WindowsIdentity user, UserConfigurationInfo conf, string rowField, string taskField)
        {
            string dummy = rowField;
        }

        public bool IsProjectlineType
        {
            get
            {
                return false;
            }
        }

        public List<LineClass> GetLineClassifications()
        {
            List<LineClass> lineclasses = new List<LineClass>();
           
            return lineclasses;
        }
        public List<BaseRow> GetRows(WindowsIdentity user, ViewConfigurationBase configuration, string periodId, DateTime start, DateTime stop, out int status, out bool canDelete, out bool canRecall, out TimesheetHeaderInfos tInfos,out decimal[] totals)
        {
            tInfos = null;
            totals = null;
            List<BaseRow> res = new List<BaseRow>();
            if (periodId == "asss-qwesat-w345-swer") status = 0;
            else status = 1;
            GetTimesheetAction(status, out canDelete, out canRecall);
            List<decimal?> standards = new List<decimal?> { 2.4m, 3.2m, 5, 5, 4, 6, 3 };
            if (configuration is ViewConfigurationRow)
            {
                ViewConfigurationRow conf = configuration as ViewConfigurationRow;
                if (conf.ActualWorkA) res.Add(new ActualWorkRow { RemainingWorkA = 1, PercentWorkCompleteA = 50, StartA=DateTime.Today.AddMonths(-3), DayTimes=standards,
                                                                  ProjectId = "qwqw-sdfc-asdf-qwqw",
                                                                  AssignementId = "axsd-s23ed-23sd-zxc",
                                                                  AssignementName = "Ass1",
                                                                  ProjectName = "Proj1"
                });
                if (conf.ActualOvertimeWorkA) res.Add(new ActualOvertimeWorkRow { OvertimeWorkA = 3, OvertimeWorkT = 10, RemainingOvertimeWorkT = 5, DayTimes = standards,
                                                                  ProjectId = "qwqw-sdfc-asdf-qwqw",
                                                                  AssignementId = "axsd-s23ed-23sd-zxc",
                                                                  AssignementName = "Ass1",
                                                                  ProjectName = "Proj1" });
                if (conf.ActualNonBillableWorkA) res.Add(new NonBillableActualWorkRow { DayTimes = standards, 
                                                                  ProjectId = "qwqw-sdfc-asdf-qwqw",
                                                                  AssignementId = "axsd-s23ed-23sd-zxc",
                                                                  AssignementName = "Ass1",
                                                                  ProjectName = "Proj1" });
                if (conf.ActualNonBillableOvertimeWorkA) res.Add(new NonBillableOvertimeWorkRow { DayTimes = standards,
                                                                  ProjectId = "qwqw-sdfc-asdf-qwqw",
                                                                  AssignementId = "axsd-s23ed-23sd-zxc",
                                                                  AssignementName = "Ass1",
                                                                  ProjectName = "Proj1" });
            }
            else
            {
                ViewConfigurationTask conf = configuration as ViewConfigurationTask;
                
                if (conf.ActualWorkA && (!conf.NoTPData)) res.Add(new ActualWorkRow { RemainingWorkA = 1, PercentWorkCompleteA = 50, DayTimes = standards, StartT = DateTime.Today.AddMonths(-3), FinishT = DateTime.Today.AddMonths(3),
                                                                  ProjectId = "qwqw-sdfc-asdf-qwqw",
                                                                  AssignementId = "axsd-s23ed-23sd-zxc",
                                                                  AssignementName = "Ass1",
                                                                  ProjectName = "Proj1" });
                if (conf.ActualWorkA && conf.NoTPData) res.Add(new SingleValuesRow
                {
                    RemainingWorkA = 1,
                    PercentWorkCompleteA = 50,
                    DayTimes = standards,
                    StartT = DateTime.Today.AddMonths(-3),
                    FinishT = DateTime.Today.AddMonths(3),
                    ProjectId = "qwqw-sdfc-asdf-qwqw",
                    AssignementId = "axsd-s23ed-23sd-zxc",
                    AssignementName = "Ass1",
                    ProjectName = "Proj1"
                });
                if (conf.ActualOvertimeWorkA && (!conf.NoTPData)) res.Add(new ActualOvertimeWorkRow { OvertimeWorkA = 3, OvertimeWorkT = 10, RemainingOvertimeWorkT = 5, DayTimes = standards,
                                                                  ProjectId = "qwqw-sdfc-asdf-qwqw",
                                                                  AssignementId = "axsd-s23ed-23sd-zxc",
                                                                  AssignementName = "Ass1",
                                                                  ProjectName = "Proj1" });
            }
            return res;
        }
        public BaseRow GetRowSingleValues(WindowsIdentity user, ViewConfigurationBase configuration, string periodId, DateTime start, DateTime stop, string ProgectId, string AssignementId,string assignmentName,string lineClassID, Type RowType)
        {
            List<decimal?> standards = new List<decimal?> { 2.1m, 4.5m, 5, 5, 4, 6, 3 };
            if (RowType == typeof(ActualWorkRow))
            {
                if (configuration is ViewConfigurationTask) return new ActualWorkRow
                {
                    DayTimes = standards,
                    RemainingWorkA = 1,
                    PercentWorkCompleteA = 50,
                    StartT = DateTime.Today.AddMonths(-3),
                    FinishT = DateTime.Today.AddMonths(3),
                    ProjectId = "nnnn-sdfc-asdf-qwqw",
                    AssignementId = "nnnn-s23ed-23sd-zxc",
                    AssignementName = "AssNew1",
                    ProjectName = "ProjNew1"
                };
                else return new ActualWorkRow {
                    DayTimes = standards,
                    RemainingWorkA = 1, PercentWorkCompleteA = 50,
                                                                  ProjectId = "nnnn-sdfc-asdf-qwqw",
                                                                  AssignementId = "nnnn-s23ed-23sd-zxc",
                                                                  AssignementName = "AssNew1",
                                                                  ProjectName = "ProjNew1" };
            }
            else if (RowType == typeof(ActualOvertimeWorkRow))
            {
                return new ActualOvertimeWorkRow {
                    DayTimes = standards,
                    OvertimeWorkA=3, OvertimeWorkT=10, RemainingOvertimeWorkT=5,
                                                                  ProjectId = "nnnn-sdfc-asdf-qwqw",
                                                                  AssignementId = "nnnn-s23ed-23sd-zxc",
                                                                  AssignementName = "AssNew1",
                                                                  ProjectName = "ProjNew1"  };
            }
            else
            {
                return new BaseRow{
                                                                  DayTimes=standards,
                                                                  ProjectId = "nnnn-sdfc-asdf-qwqw",
                                                                  AssignementId = "nnnn-s23ed-23sd-zxc",
                                                                  AssignementName = "AssNew1",
                                                                  ProjectName = "ProjNew1" };
            }
        }
        public void UpdateRows(WindowsIdentity user, ViewConfigurationBase configuration, string periodId, DateTime start, DateTime stop, IEnumerable<Tracker<BaseRow>> rows, bool submit)
        {
            if (rows == null) return;
            var crows = rows.GroupBy(m => m.Value.AssignementId);
            IDictionary<string, WholeLine> dict =
                new Dictionary<string, WholeLine>();
            List<WholeLine> list =
                new List<WholeLine>();
            
            foreach (var group in crows)
            {
                WholeLine wLine = new WholeLine(group);
                dict.Add(group.Key, wLine);
                list.Add(wLine);
            }
            bool noChange = true;
            foreach (var x in list)
            {
                if (x.Changed) noChange = false;
            }
            if (noChange) return;
            //string xml = new ChangeXml(list, configuration, start, stop, ).Get();
            foreach(Tracker<BaseRow> tracker in rows)
            {
                tracker.Confirm();
            }
        }
        public void RecallDelete(WindowsIdentity user, string periodId, DateTime start, DateTime stop, bool isRecall)
        {
        }
        public TimesheetsSets DefaultTimesheetSet {get {return TimesheetsSets.Last3;}}

        public string GetPeriodID(DateTime start, DateTime end)
        {
            throw new Exception();
        }
        public IEnumerable<Timesheet> SelectTimesheets(System.Security.Principal.WindowsIdentity user, TimesheetsSets set, out DateTime start, out DateTime end)
        {
            start = DateTime.MinValue;
            end = DateTime.MaxValue;
            if (set == TimesheetsSets.Default) set = DefaultTimesheetSet;
            List<Timesheet> res = new List<Timesheet>();
            //if (set == TimesheetsSets.Last3 ) return res;
            res.Add(
                    new Timesheet
                    {
                        Id = "asss-qwesat-w344-swer",
                        Name = "Contoso 1",
                        Start = DateTime.Today.AddMonths(-2),
                        Stop = DateTime.Today.AddMonths(-2).AddDays(6)
                    }
            );
            res.Add(
                    new Timesheet
                    {
                        Id = "avvv-qwesat-w345-swer",
                        Name = "Contoso 2",
                        Start = DateTime.Today.AddMonths(-1),
                        Stop = DateTime.Today.AddMonths(-1).AddDays(6)
                    }
            );
            res.Add(
                    new Timesheet
                    {
                        Id = "annn-qwesat-w346-swer",
                        Name = "Contoso 3",
                        Start = DateTime.Today.AddDays(-1),
                        Stop = DateTime.Today.AddDays(-1).AddDays(6)
                    }
            );
            return res;
        }
        protected void GetTimesheetAction(int status, out bool canDelete, out bool canRecall)
        {
            canDelete = (status == 0) || (status == 4) || (status == 2);
            canRecall = (status == 1) || (status == 3) || (status == 2);
        }
        public IEnumerable<ProjectInfo> UserProjects(System.Security.Principal.WindowsIdentity user)
        {
            List<ProjectInfo> res = new List<ProjectInfo>();
            res.Add(new ProjectInfo
            {
                Id = "asdf-qwert-w345-swer",
                Name = "Project1"
            });
            res.Add(new ProjectInfo
            {
                Id = "zxcv-fghjt-w456-zxcv",
                Name = "Project2"
            });
            return res;
        }

        public IEnumerable<AssignementInfo> ProjectAssignements(System.Security.Principal.WindowsIdentity user, string ProjectId)
        {
            List<AssignementInfo> res = new List<AssignementInfo>();
            if (string.IsNullOrWhiteSpace(ProjectId))
            {
                return res;
            }
            else if (ProjectId == "asdf-qwert-w345-swer")
            {
                res.Add(new AssignementInfo
                {
                    Id = "asdf-2345-w345-swer",
                    Name = "Assignement1"
                });
                res.Add(new AssignementInfo
                {
                    Id = "xder-fghjt-w456-zxcv",
                    Name = "Assignement2"
                });
            }
            else
            {
                res.Add(new AssignementInfo
                {
                    Id = "zxde-1aw3-xsde-swer",
                    Name = "Assignement3"
                });
                res.Add(new AssignementInfo
                {
                    Id = "cdsa-azxs-2asd-cdrs",
                    Name = "Assignement4"
                });
            }
            return res;
        }


        public bool IsAdminproject(string projectName)
        {
            throw new NotImplementedException();
        }
    }
}
