using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using MVCControlsToolkit.Controller;

namespace TimeSheetIBusiness
{
    public interface IRepository
    {
        IEnumerable<ProjectInfo> UserProjects(WindowsIdentity user);
        IEnumerable<AssignementInfo> ProjectAssignements(WindowsIdentity user, string ProjectId);
        IEnumerable<Timesheet> SelectTimesheets(System.Security.Principal.WindowsIdentity user, TimesheetsSets set, out DateTime start, out DateTime end);
        List<BaseRow> GetRows(WindowsIdentity user, ViewConfigurationBase configuration, string periodId, DateTime start, DateTime stop, out int status, out bool canDelete, out bool canRecall, out TimesheetHeaderInfos tInfos,out decimal[] totals);
        BaseRow GetRowSingleValues(WindowsIdentity user, ViewConfigurationBase configuration, string periodId, DateTime start, DateTime stop, string ProgectId, string AssignementId, string lineClassID,Type RowType);
        void UpdateRows(WindowsIdentity user, ViewConfigurationBase configuration, string periodId, DateTime start, DateTime stop, IEnumerable<Tracker<BaseRow>> rows, bool submit);
        TimesheetsSets DefaultTimesheetSet {get;}
        void RecallDelete(WindowsIdentity user, string periodId, DateTime start, DateTime stop, bool isRecall);
        UserConfigurationInfo UserConfiguration(WindowsIdentity user, string rowField, string taskField);
        void ChangeUserConfiguration(WindowsIdentity user, UserConfigurationInfo conf,  string rowField, string taskField);
        bool SetClientEndpointsProg(string pwaUrl);
        string GetPeriodID(DateTime start, DateTime end);
        CustomFieldInfo GetCustomFieldType(Guid id,int type, string property);
        LookupTableDisplayItem[] GetLookupTableValuesAsItems(Guid tableUid, string dataType);

        List<LineClass> GetLineClassifications();
        WindowsIdentity AppPoolUser { get; set; }
    }
}
