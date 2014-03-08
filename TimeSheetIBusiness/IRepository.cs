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
        IEnumerable<ProjectInfo> UserProjects(string user);
        IEnumerable<AssignementInfo> ProjectAssignements(string user, string ProjectId);
        IEnumerable<Timesheet> SelectTimesheets(string user, TimesheetsSets set, out DateTime start, out DateTime end);
        List<BaseRow> GetRows(string user, ViewConfigurationBase configuration, string periodId, DateTime start, DateTime stop, out int status, out bool canDelete, out bool canRecall, out TimesheetHeaderInfos tInfos,out decimal[] totals);
        BaseRow GetRowSingleValues(string user, ViewConfigurationBase configuration, string periodId, DateTime start, DateTime stop, string ProgectId, string AssignementId, string assignmentName, string lineClassID, Type RowType);
        void UpdateRows(bool isApprovalMode, string user, ViewConfigurationBase configuration, string periodId, DateTime start, DateTime stop, IEnumerable<Tracker<BaseRow>> rows, bool submit);
        TimesheetsSets DefaultTimesheetSet {get;}
        void RecallDelete(string user, string periodId, DateTime start, DateTime stop, bool isRecall);
        UserConfigurationInfo UserConfiguration(string user, string rowField, string taskField, string approvalFieldID);
        void ChangeUserConfiguration(string user, UserConfigurationInfo conf, string rowField, string taskField, string approvalFieldID);
        bool SetClientEndpointsProg(string pwaUrl);
        string GetPeriodID(DateTime start, DateTime end);
        CustomFieldInfo GetCustomFieldType(Guid id,int type, string property);
        LookupTableDisplayItem[] GetLookupTableValuesAsItems(Guid tableUid, string dataType);

        List<LineClass> GetLineClassifications();
        WindowsIdentity AppPoolUser { get; set; }
        string GetUserName(string name);
        List<TimesheetApprovalItem> GetTimesheetApprovals(string user);
        void ApproveTimesheet(string tUID, string mgrUID,string action);
        void RejectTimesheet(string tUID, string mgrUID);
        List<TaskApprovalItem> GetTaskApprovals(string user);
        string DefaultLineClass { get; }

        string GetCurrentUserId();
        Guid GetResourceUidFromNtAccount(String ntAccount);

        void ApproveProjectTasks(string projectID, string mgrUID, string mode);
        void ApproveTasks(string[] assnid, string mgrUID, string mode);
        List<BaseRow> GetSubmittedRows(string projectId, string approver,string user, ViewConfigurationBase configuration);
    }
}
