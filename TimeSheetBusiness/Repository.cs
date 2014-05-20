using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Web;
using System.Web.Services.Protocols;
using System.Xml;
using MVCControlsToolkit.Controller;
using SvcProject;
using TimeSheetIBusiness;
using PSLib = Microsoft.Office.Project.Server.Library;

namespace TimeSheetBusiness
{

    public class Repository : IRepository
    {
        public System.Security.Principal.WindowsIdentity AppPoolUser { get; set; }
        public SvcAdmin.AdminClient adminClient;
        public SvcQueueSystem.QueueSystemClient queueSystemClient;
        public SvcResource.ResourceClient resourceClient;
        public SvcProject.ProjectClient projectClient;
        public SvcLookupTable.LookupTableClient lookupTableClient;
        public SvcCustomFields.CustomFieldsClient customFieldsClient;
        public SvcCalendar.CalendarClient calendarClient;
        public SvcArchive.ArchiveClient archiveClient;
        public SvcStatusing.StatusingClient pwaClient;
        public SvcTimeSheet.TimeSheetClient timesheetClient;
        public SvcQueueSystem.QueueSystemClient queueClient;
        private String impersonationContextString = String.Empty; 
        public bool isImpersonated = false;
        private bool? allowTopLevel = null;
        private string currentUserName;
        private string defaultLineClass = null;
        public Repository()
        {
            if (DateTime.Today > new DateTime(2014, 12, 12)) throw new Exception("Demo Copy of Mobile Timesheet Expired");

        }

        public string GetCurrentUserId()
        {
            return resourceClient.GetCurrentUserUid().ToString();
        }

        public  string HeaderXformsKey
        {
            get
            { return "X-FORMS_BASED_AUTH_ACCEPTED"; }
        }

        public  string HeaderXformsValue
        {
            get
            { return "f"; }
        }

        public string DefaultLineClass
        {
            get
            {
                var obj = GetApplicationObject("defaultLineClass");
                if (obj != null)
                {
                    return obj.ToString();
                }
                if (defaultLineClass == string.Empty)
                {
                    defaultLineClass = GetLineClassifications().FirstOrDefault().Name;
                    CacheApplicationObject("defaultLineClass", defaultLineClass);
                }
                return defaultLineClass;
            }

        }
        public string GetUserName(string ntAccount)
        {
            
                string ntAccountCopy =  ntAccount;
                object cachedCopy = GetSessionObject(ntAccountCopy + "resName");
                if (cachedCopy != null)
                {
                    return cachedCopy.ToString();
                }
                SvcResource.ResourceDataSet rds = new SvcResource.ResourceDataSet();

                Microsoft.Office.Project.Server.Library.Filter filter = new Microsoft.Office.Project.Server.Library.Filter();
                filter.FilterTableName = rds.Resources.TableName;


                Microsoft.Office.Project.Server.Library.Filter.Field ntAccountField1 = new Microsoft.Office.Project.Server.Library.Filter.Field(rds.Resources.TableName, rds.Resources.WRES_ACCOUNTColumn.ColumnName);
                filter.Fields.Add(ntAccountField1);

                Microsoft.Office.Project.Server.Library.Filter.Field ntAccountField2 = new Microsoft.Office.Project.Server.Library.Filter.Field(rds.Resources.TableName, rds.Resources.RES_NAMEColumn.ColumnName);
                filter.Fields.Add(ntAccountField2);

                Microsoft.Office.Project.Server.Library.Filter.FieldOperator op = new Microsoft.Office.Project.Server.Library.Filter.FieldOperator(Microsoft.Office.Project.Server.Library.Filter.FieldOperationType.Equal,
                    rds.Resources.WRES_ACCOUNTColumn.ColumnName, ntAccountCopy);
                filter.Criteria = op;

                rds = resourceClient.ReadResources(filter.GetXml(), false);

                string obj = rds.Resources.Rows[0]["RES_NAME"].ToString();
                CacheSessionObject(ntAccountCopy + "resName",obj);
                return obj;
            }
        

        public bool AllowToplevel
        {
            get
            {


                if (allowTopLevel == null || !allowTopLevel.HasValue)
                {

                    allowTopLevel = (adminClient.ReadTimeSheetSettings().TimeSheetSettings.Rows[0] as SvcAdmin.TimeSheetSettingsDataSet.TimeSheetSettingsRow).WADMIN_TS_ALLOW_PROJECT_LEVEL == true;
                }

                return allowTopLevel.Value;
            }
        }

        public void ApproveTimesheet(string tUID,string mgrUID,string action)
        {
            using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
            {
                SetImpersonation(new Guid(mgrUID));
                if (action.ToUpper() == "APPROVE")
                {
                    timesheetClient.QueueReviewTimesheet(Guid.NewGuid(), new Guid(tUID), new Guid(mgrUID),
                            "Approved", SvcTimeSheet.Action.Approve);
                }
                else
                {
                    timesheetClient.QueueReviewTimesheet(Guid.NewGuid(), new Guid(tUID), new Guid(mgrUID),
                            "Rejected", SvcTimeSheet.Action.Reject);
                }
                bool res = QueueHelper.WaitForQueueJobCompletion(this, Guid.NewGuid(), (int)SvcQueueSystem.QueueMsgType.StatusApproval, queueClient);
                if (!res) throw new TimesheetUpdateException();
            }
        }

        public void RejectTimesheet(string tUID,string mgrUID)
        {
            using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
            {
                SetImpersonation(resourceClient.ReadResource(new Guid(mgrUID)).Resources[0].RES_UID);
                timesheetClient.QueueReviewTimesheet(Guid.NewGuid(), new Guid(tUID), new Guid(mgrUID),
                        "Rejected", SvcTimeSheet.Action.Reject);
                bool res = QueueHelper.WaitForQueueJobCompletion(this, Guid.NewGuid(), (int)SvcQueueSystem.QueueMsgType.StatusApproval, queueClient);
                if (!res) throw new TimesheetUpdateException();
            }
        }

        public List<TimesheetApprovalItem> GetTimesheetApprovals(string user)
        {

           List<TimesheetApprovalItem> returnValues = new List<TimesheetApprovalItem>();
            SvcTimeSheet.TimesheetListDataSet ds = new SvcTimeSheet.TimesheetListDataSet();
            using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
            {
                bool temp;
                SetImpersonation(GetResourceUidFromNtAccount(user));
                try
                {
                     ds = timesheetClient.ReadTimesheetsPendingApproval(new DateTime(1984, 1, 1), new DateTime(2049, 12, 1), null);
                }
                catch (SoapException ex)
                {
                    var sx = new PSLib.PSClientError(ex);
                }
            }

            // DO a group y user so that one PSI call is made per user for ReadResource
            var tsGroups = ds.Timesheets.GroupBy(t => t.RES_UID);
            int count=0;
            foreach (var value in tsGroups)
            {
                string resName;
                string resNTAcct="";
                TimesheetApprovalItem item = new TimesheetApprovalItem();
                item.TimesheetApprovals = new List<MyTimesheetApproval>();
                var tsList = value.ToList();
                if (value.ToList().Count > 0)
                {
                    var resource  = resourceClient.ReadResource(tsList[0].RES_UID).Resources.First();
                    item.UserName = resource.RES_NAME;
                    item.UserNTAccount = resource.WRES_ACCOUNT;
                    
                }
                else
                {
                    resNTAcct = "";
                    continue;
                }
                foreach (var ts in tsList)
                {
                    var myTimesheetApproval = new MyTimesheetApproval();
                    myTimesheetApproval.Hours = ts.IsTS_TOTAL_ACT_VALUENull() ? "0Hrs" : Math.Round(ts.TS_TOTAL_ACT_VALUE / 60000,2).ToString() + "Hrs";
                    myTimesheetApproval.Name = ts.TS_NAME;
                    myTimesheetApproval.Period = ts.WPRD_START_DATE.ToShortDateString() + "-" + ts.WPRD_FINISH_DATE.ToShortDateString();
                    myTimesheetApproval.TimesheetId = ts.TS_UID.ToString();
                    item.TimesheetApprovals.Add(myTimesheetApproval);
                }
                
                returnValues.Add(item);
            }
            
            return returnValues;
        }

        public void ApproveTasks(string[] assignments, string mgrUID, string mode)
        {
            SvcStatusing.StatusApprovalDataSet statusApprovalDs = new SvcStatusing.StatusApprovalDataSet();

            using (OperationContextScope scope = new OperationContextScope(pwaClient.InnerChannel))
            {
                SetImpersonation(new Guid(mgrUID));
                try
                {
                    statusApprovalDs = pwaClient.ReadStatusApprovalsSubmitted(false);
                    //var projectTasks = ds.StatusApprovals.Where(t => t.PROJ_UID.ToString() == projectID);
                    for (int i = 0; i < statusApprovalDs.StatusApprovals.Count; i++)
                    {
                        if (assignments.Any(t=>t == statusApprovalDs.StatusApprovals[i].ASSN_UID.ToString()))
                            statusApprovalDs.StatusApprovals[i].ASSN_TRANS_ACTION_ENUM = (int)PSLib.TaskManagement.StatusApprovalType.Accepted;
                    }
                }
                catch (SoapException ex)
                {
                    var sx = new PSLib.PSClientError(ex);
                }
                pwaClient.UpdateStatusApprovals(statusApprovalDs);
                pwaClient.QueueApplyStatusApprovals(Guid.NewGuid(), "Approved");
                bool res = QueueHelper.WaitForQueueJobCompletion(this, Guid.NewGuid(), (int)SvcQueueSystem.QueueMsgType.StatusApproval, queueClient);
                if (!res) throw new StatusUpdateException();
            }

        }
        public void ApproveProjectTasks(string projectID, string mgrUID, string mode)
        {
            SvcStatusing.StatusApprovalDataSet statusApprovalDs = new SvcStatusing.StatusApprovalDataSet();

            using (OperationContextScope scope = new OperationContextScope(pwaClient.InnerChannel))
            {
                SetImpersonation(resourceClient.ReadResource(new Guid(mgrUID)).Resources[0].RES_UID);
                try
                {
                    statusApprovalDs = pwaClient.ReadStatusApprovalsSubmitted(false);
                    //var projectTasks = ds.StatusApprovals.Where(t => t.PROJ_UID.ToString() == projectID);
                    for (int i = 0; i < statusApprovalDs.StatusApprovals.Count; i++)
                    {
                        if (statusApprovalDs.StatusApprovals[i].PROJ_UID.ToString() == projectID)
                            statusApprovalDs.StatusApprovals[i].ASSN_TRANS_ACTION_ENUM = (int)PSLib.TaskManagement.StatusApprovalType.Accepted;
                    }
                }
                catch (SoapException ex)
                {
                    var sx = new PSLib.PSClientError(ex);
                }
                pwaClient.UpdateStatusApprovals(statusApprovalDs);
                pwaClient.QueueApplyStatusApprovals(Guid.NewGuid(), "Approved");
                bool res = QueueHelper.WaitForQueueJobCompletion(this, Guid.NewGuid(), (int)SvcQueueSystem.QueueMsgType.StatusApproval, queueClient);
                if (!res) throw new StatusUpdateException();
            }
            
        }
        public List<TaskApprovalItem> GetTaskApprovals(string user)
        {

            List<TaskApprovalItem> returnValues = new List<TaskApprovalItem>();
            SvcStatusing.StatusApprovalDataSet ds = new SvcStatusing.StatusApprovalDataSet();
            using (OperationContextScope scope = new OperationContextScope(pwaClient.InnerChannel))
            {
                bool temp;
                SetImpersonation(GetResourceUidFromNtAccount(user));
                try
                {
                    ds = pwaClient.ReadStatusApprovalsSubmitted(false);
                }
                catch (SoapException ex)
                {
                    var sx = new PSLib.PSClientError(ex);
                }
            }

            // DO a group by user so that one PSI call is made per user for ReadResource
            var tsGroups = ds.StatusApprovals.GroupBy(t => t.RES_UID);
            int count = 0;
            foreach (var value in tsGroups)
            {
                string resName;
                string resNTAcct = "";
                TaskApprovalItem item = new TaskApprovalItem();
                item.TaskApprovals = new List<MyTaskApproval>();
                var tsList = value.ToList();
                if (value.ToList().Count > 0)
                {
                    var resource = resourceClient.ReadResource(tsList[0].RES_UID).Resources.First();
                    item.UserName = resource.RES_NAME;
                    item.UserNTAccount = resource.WRES_ACCOUNT;

                }
                else
                {
                    resNTAcct = "";
                    continue;
                }
                var projectGroups = tsList.GroupBy(t => t.PROJ_UID);
                foreach (var ts in projectGroups)
                {
                    var myTaskApproval = new MyTaskApproval();
                    myTaskApproval.ProjectName = ts.ElementAt(0).PROJ_NAME;
                    myTaskApproval.ProjectId = ts.Key.ToString();
                    item.TaskApprovals.Add(myTaskApproval);
                }

                returnValues.Add(item);
            }

            return returnValues;
        }

        public List<LineClass> GetAllLineClassifications()
        {
            List<LineClass> lineclasses = new List<LineClass>();
            var tsLineClassDs = adminClient.ReadLineClasses(SvcAdmin.LineClassType.All, SvcAdmin.LineClassState.Enabled).LineClasses.ToList();
            foreach (var lineclass in tsLineClassDs)
            {
                lineclasses.Add(new LineClass(lineclass.TS_LINE_CLASS_UID.ToString(), lineclass.TS_LINE_CLASS_NAME));
            }
            return lineclasses;
        }
        public List<LineClass> GetLineClassifications()
        {
            List<LineClass> lineclasses = new List<LineClass>();
            var tsLineClassDs = adminClient.ReadLineClasses(SvcAdmin.LineClassType.All, SvcAdmin.LineClassState.Enabled).LineClasses.Where(t => t.TS_LINE_CLASS_TYPE == 0)
                .OrderBy(t=>t.MOD_DATE);
            foreach (var lineclass in tsLineClassDs)
            {
                lineclasses.Add(new LineClass(lineclass.TS_LINE_CLASS_UID.ToString(), lineclass.TS_LINE_CLASS_NAME));
            }
            return lineclasses;
        }

        public UserConfigurationInfo UserConfiguration(string user, string rowField, string taskField,string approvalField)
        {

            Guid defaultTimesheetViewUID = ViewConfigurationRow.ViewFieldGuid;
            Guid defaultStatusViewUID = ViewConfigurationTask.ViewFieldGuid;
            Guid defaultApprovalViewUID = ViewConfigurationApproval.ViewFieldGuid;
            if ((!string.IsNullOrWhiteSpace(rowField) && (defaultTimesheetViewUID == null || defaultTimesheetViewUID == Guid.Empty)) ||
                (!string.IsNullOrWhiteSpace(taskField) && (defaultStatusViewUID == null || defaultStatusViewUID == Guid.Empty)) ||
                (!string.IsNullOrWhiteSpace(approvalField) && (defaultApprovalViewUID == null || defaultApprovalViewUID == Guid.Empty))
                )
            {
                //this code gets the name of default views stored on the server.
                //get the list of custom fields first
                SvcCustomFields.CustomFieldDataSet cds = new SvcCustomFields.CustomFieldDataSet();
                /*I dont think we need a filter, but if we did, this is a good example
                 * http://www.epmfaq.com/ssanderlin/project-server-2007/retrieve-the-guid-of-a-custom-field-using-its-name  */
                cds = customFieldsClient.ReadCustomFields(string.Empty, false);

                if (!string.IsNullOrWhiteSpace(rowField))
                {
                    SvcCustomFields.CustomFieldDataSet.CustomFieldsRow[] timesheetviewrow;
                    timesheetviewrow = (SvcCustomFields.CustomFieldDataSet.CustomFieldsRow[])cds.CustomFields.Select("MD_PROP_NAME = '" + rowField + "'");

                    //to do//   remove dependency on these custom fields
                    if (timesheetviewrow.Length > 0)
                    {
                        ViewConfigurationRow.ViewFieldGuid = defaultTimesheetViewUID = timesheetviewrow[0].MD_PROP_UID;
                    }
                }
                if (!string.IsNullOrWhiteSpace(taskField))
                {
                    SvcCustomFields.CustomFieldDataSet.CustomFieldsRow[] statusviewrow;
                    statusviewrow = (SvcCustomFields.CustomFieldDataSet.CustomFieldsRow[])cds.CustomFields.Select("MD_PROP_NAME = '" + taskField + "'");
                    if (statusviewrow.Length > 0)
                    {
                        ViewConfigurationTask.ViewFieldGuid = defaultStatusViewUID = statusviewrow[0].MD_PROP_UID;
                    }
                }

                if (!string.IsNullOrWhiteSpace(approvalField))
                {
                    SvcCustomFields.CustomFieldDataSet.CustomFieldsRow[] approvalviewrow;
                    approvalviewrow = (SvcCustomFields.CustomFieldDataSet.CustomFieldsRow[])cds.CustomFields.Select("MD_PROP_NAME = '" + approvalField + "'");
                    if (approvalviewrow.Length > 0)
                    {
                        ViewConfigurationApproval.ViewFieldGuid = defaultApprovalViewUID = approvalviewrow[0].MD_PROP_UID;
                    }
                }

            }
            //
            string defaultTimesheetView = string.Empty;
            string defaultStatusView = string.Empty;
            string defaultApprovalView = string.Empty;
            if ((defaultTimesheetViewUID != null && defaultTimesheetViewUID != Guid.Empty) 
                || (defaultStatusViewUID != null && defaultStatusViewUID != Guid.Empty)
                || (defaultApprovalViewUID != null && defaultApprovalViewUID != Guid.Empty))
            {
                //now read the values of the custom fields.
                SvcResource.ResourceDataSet rds = new SvcResource.ResourceDataSet();
                Guid resUID = Guid.Empty;

                resUID = LoggedUser(user);
                rds = resourceClient.ReadResource(resUID);


                if (defaultTimesheetViewUID != null && defaultTimesheetViewUID != Guid.Empty)
                {
                    SvcResource.ResourceDataSet.ResourceCustomFieldsRow[] tsViewFieldsRow = null;


                    tsViewFieldsRow = (SvcResource.ResourceDataSet.ResourceCustomFieldsRow[])rds.ResourceCustomFields.Select("MD_PROP_UID = '" + defaultTimesheetViewUID + "'");

                    defaultTimesheetView = tsViewFieldsRow.Length == 0 ? null : tsViewFieldsRow[0].TEXT_VALUE;
                    if (string.IsNullOrWhiteSpace(defaultTimesheetView)) defaultTimesheetView = string.Empty;
                }
                if (defaultStatusViewUID != null && defaultStatusViewUID != Guid.Empty)
                {
                    SvcResource.ResourceDataSet.ResourceCustomFieldsRow[] statusViewFieldsRow =
                         (SvcResource.ResourceDataSet.ResourceCustomFieldsRow[])rds.ResourceCustomFields.Select("MD_PROP_UID = '" + defaultStatusViewUID + "'");
                    defaultStatusView = statusViewFieldsRow.Length == 0 ? null : statusViewFieldsRow[0].TEXT_VALUE;
                    if (string.IsNullOrWhiteSpace(defaultStatusView)) defaultStatusView = string.Empty;
                }
                if (defaultApprovalViewUID != null && defaultApprovalViewUID != Guid.Empty)
                {
                    SvcResource.ResourceDataSet.ResourceCustomFieldsRow[] approvalViewFieldsRow =
                         (SvcResource.ResourceDataSet.ResourceCustomFieldsRow[])rds.ResourceCustomFields.Select("MD_PROP_UID = '" + defaultApprovalViewUID + "'");
                    defaultApprovalView = approvalViewFieldsRow.Length == 0 ? null : approvalViewFieldsRow[0].TEXT_VALUE;
                    if (string.IsNullOrWhiteSpace(defaultApprovalView)) defaultApprovalView = string.Empty;
                }


            }
            return new UserConfigurationInfo { TaskViewId = defaultStatusView, RowViewId = defaultTimesheetView ,ApprovalViewId = defaultApprovalView};
        }

        public void ChangeUserConfiguration(string user, UserConfigurationInfo conf, string rowField, string taskField,string approvalField)
        {

            Guid defaultTimesheetViewUID = ViewConfigurationRow.ViewFieldGuid;
            Guid defaultStatusViewUID = ViewConfigurationTask.ViewFieldGuid;
            Guid defaultApprovalViewUID = ViewConfigurationApproval.ViewFieldGuid;
            if ((!string.IsNullOrWhiteSpace(rowField) && (defaultTimesheetViewUID == null || defaultTimesheetViewUID == Guid.Empty)) ||
                (!string.IsNullOrWhiteSpace(taskField) && (defaultStatusViewUID == null || defaultStatusViewUID == Guid.Empty)) ||
                (!string.IsNullOrWhiteSpace(approvalField) && (defaultApprovalViewUID == null || defaultApprovalViewUID == Guid.Empty))
                
                )
            {
                //this code gets the name of default views stored on the server.
                //get the list of custom fields first
                SvcCustomFields.CustomFieldDataSet cds = new SvcCustomFields.CustomFieldDataSet();
                /*I dont think we need a filter, but if we did, this is a good example
                 * http://www.epmfaq.com/ssanderlin/project-server-2007/retrieve-the-guid-of-a-custom-field-using-its-name  */
                cds = customFieldsClient.ReadCustomFields(string.Empty, false);

                if (!string.IsNullOrWhiteSpace(rowField))
                {
                    SvcCustomFields.CustomFieldDataSet.CustomFieldsRow[] timesheetviewrow;
                    timesheetviewrow = (SvcCustomFields.CustomFieldDataSet.CustomFieldsRow[])cds.CustomFields.Select("MD_PROP_NAME = '" + rowField + "'");
                    if (timesheetviewrow.Length > 0)
                    {
                        ViewConfigurationRow.ViewFieldGuid = defaultTimesheetViewUID = timesheetviewrow[0].MD_PROP_UID;
                    }
                }
                if (!string.IsNullOrWhiteSpace(taskField))
                {
                    SvcCustomFields.CustomFieldDataSet.CustomFieldsRow[] statusviewrow;
                    statusviewrow = (SvcCustomFields.CustomFieldDataSet.CustomFieldsRow[])cds.CustomFields.Select("MD_PROP_NAME = '" + taskField + "'");
                    if (statusviewrow.Length > 0)
                    {
                        ViewConfigurationTask.ViewFieldGuid = defaultStatusViewUID = statusviewrow[0].MD_PROP_UID;
                    }
                }

                if (!string.IsNullOrWhiteSpace(approvalField))
                {
                    SvcCustomFields.CustomFieldDataSet.CustomFieldsRow[] approvalviewrow;
                    approvalviewrow = (SvcCustomFields.CustomFieldDataSet.CustomFieldsRow[])cds.CustomFields.Select("MD_PROP_NAME = '" + approvalField + "'");
                    if (approvalviewrow.Length > 0)
                    {
                        ViewConfigurationApproval.ViewFieldGuid = defaultApprovalViewUID = approvalviewrow[0].MD_PROP_UID;
                    }
                }

            }
            if ((defaultTimesheetViewUID != null && defaultTimesheetViewUID != Guid.Empty) 
                || (defaultStatusViewUID != null && defaultStatusViewUID != Guid.Empty)
                || (defaultApprovalViewUID != null && defaultApprovalViewUID != Guid.Empty)
                )
            {
                ///////////////////
                //now read the values of the custom fields.

                SvcResource.ResourceDataSet rds = new SvcResource.ResourceDataSet();
                Guid resUID = LoggedUser(user);


                rds = resourceClient.ReadResource(resUID);

                try
                {

                    SvcResource.ResourceDataSet.ResourcesRow row = rds.Resources[0];

                    if (row.IsNull("RES_CHECKOUTBY"))  //if true, the resource can be modified
                    {
                        if (defaultTimesheetViewUID != null && defaultTimesheetViewUID != Guid.Empty && defaultApprovalViewUID != Guid.Empty)
                        {
                            SvcResource.ResourceDataSet.ResourceCustomFieldsRow[] foundrowTS = (SvcResource.ResourceDataSet.ResourceCustomFieldsRow[])rds.ResourceCustomFields.Select("MD_PROP_UID = '" + defaultTimesheetViewUID + "'");
                            if (foundrowTS.Length != 0)
                            {
                                foundrowTS[0].TEXT_VALUE = conf.RowViewId == null ? "" : conf.RowViewId;
                            }
                            else if (!string.IsNullOrWhiteSpace(conf.RowViewId))  //the user does not have a default timesheet mobile view... 
                            {
                                SvcResource.ResourceDataSet.ResourceCustomFieldsRow newrow = rds.ResourceCustomFields.NewResourceCustomFieldsRow(); //add a new row to set value of custom field.
                                newrow.RES_UID = resUID;
                                newrow.MD_PROP_UID = defaultTimesheetViewUID;
                                newrow.CUSTOM_FIELD_UID = Guid.NewGuid();
                                newrow.TEXT_VALUE = conf.RowViewId == null ? "" : conf.RowViewId;
                                rds.ResourceCustomFields.AddResourceCustomFieldsRow(newrow);

                            }
                        }
                        if (defaultStatusViewUID != null && defaultStatusViewUID != Guid.Empty)
                        {
                            SvcResource.ResourceDataSet.ResourceCustomFieldsRow[] foundrowStatus = (SvcResource.ResourceDataSet.ResourceCustomFieldsRow[])rds.ResourceCustomFields.Select("MD_PROP_UID = '" + defaultStatusViewUID + "'");
                            if (foundrowStatus.Length != 0)
                            {
                                foundrowStatus[0].TEXT_VALUE = conf.TaskViewId == null ? "" : conf.TaskViewId;
                            }
                            else if (!string.IsNullOrWhiteSpace(conf.TaskViewId))    //the user does not have a default status mobile view...
                            {
                                SvcResource.ResourceDataSet.ResourceCustomFieldsRow newrow = rds.ResourceCustomFields.NewResourceCustomFieldsRow(); //add a new row to set value of custom field.
                                newrow.RES_UID = resUID;
                                newrow.MD_PROP_UID = defaultStatusViewUID;
                                newrow.CUSTOM_FIELD_UID = Guid.NewGuid();
                                newrow.TEXT_VALUE = conf.TaskViewId == null ? "" : conf.TaskViewId;
                                rds.ResourceCustomFields.AddResourceCustomFieldsRow(newrow);

                            }

                        }

                        if (defaultApprovalViewUID != null && defaultApprovalViewUID != Guid.Empty)
                        {
                            SvcResource.ResourceDataSet.ResourceCustomFieldsRow[] foundrowStatus = (SvcResource.ResourceDataSet.ResourceCustomFieldsRow[])rds.ResourceCustomFields.Select("MD_PROP_UID = '" + defaultStatusViewUID + "'");
                            if (foundrowStatus.Length != 0)
                            {
                                foundrowStatus[0].TEXT_VALUE = conf.ApprovalViewId == null ? "" : conf.ApprovalViewId;
                            }
                            else if (!string.IsNullOrWhiteSpace(conf.ApprovalViewId))    //the user does not have a default status mobile view...
                            {
                                SvcResource.ResourceDataSet.ResourceCustomFieldsRow newrow = rds.ResourceCustomFields.NewResourceCustomFieldsRow(); //add a new row to set value of custom field.
                                newrow.RES_UID = resUID;
                                newrow.MD_PROP_UID = defaultStatusViewUID;
                                newrow.CUSTOM_FIELD_UID = Guid.NewGuid();
                                newrow.TEXT_VALUE = conf.ApprovalViewId == null ? "" : conf.ApprovalViewId;
                                rds.ResourceCustomFields.AddResourceCustomFieldsRow(newrow);

                            }

                        }
                        Guid[] resourcestoCheckout = new Guid[1];
                        resourcestoCheckout[0] = resUID;

                        resourceClient.CheckOutResources(resourcestoCheckout);
                        resourceClient.UpdateResources(rds, false, true);

                    }
                }
                catch
                {
                }
                ////////////////////

            }

        }

        protected Guid LoggedUser(string user)
        {
            bool isWindowsUser;
            return GetResourceUidFromNtAccount(user);
        }
        private SvcResource.ResourceAssignmentDataSet GetResourceAssignmentDataSet(string user)
        {

            Guid[] resourceUids = new Guid[1];

            resourceUids[0] = LoggedUser(user);

            PSLib.Filter resourceAssignmentFilter = GetResourceAssignmentFilter(resourceUids);
            string resourceAssignmentFilterXml = resourceAssignmentFilter.GetXml();
            using (OperationContextScope scope = new OperationContextScope(resourceClient.InnerChannel))
            {
                SetImpersonation(GetResourceUidFromNtAccount(user));
                return resourceClient.ReadResourceAssignments(resourceAssignmentFilterXml);
            }
        }
        private static PSLib.Filter GetResourceAssignmentFilter(Guid[] resources)
        {
            SvcResource.ResourceAssignmentDataSet resourceAssignmentDs = new SvcResource.ResourceAssignmentDataSet();
            string foo = resourceAssignmentDs.GetXmlSchema();
            PSLib.Filter resourceFilter = new PSLib.Filter();
            resourceFilter.FilterTableName = resourceAssignmentDs.ResourceAssignment.TableName;
            resourceFilter.Fields.Add(new PSLib.Filter.Field(resourceFilter.FilterTableName, resourceAssignmentDs.ResourceAssignment.RES_UIDColumn.ColumnName, PSLib.Filter.SortOrderTypeEnum.None));
            resourceFilter.Fields.Add(new PSLib.Filter.Field(resourceFilter.FilterTableName, resourceAssignmentDs.ResourceAssignment.RES_NAMEColumn.ColumnName, PSLib.Filter.SortOrderTypeEnum.None));

            resourceFilter.Fields.Add(new PSLib.Filter.Field(resourceFilter.FilterTableName, resourceAssignmentDs.ResourceAssignment.TASK_UIDColumn.ColumnName, PSLib.Filter.SortOrderTypeEnum.None));
            resourceFilter.Fields.Add(new PSLib.Filter.Field(resourceFilter.FilterTableName, resourceAssignmentDs.ResourceAssignment.TASK_NAMEColumn.ColumnName, PSLib.Filter.SortOrderTypeEnum.None));
            resourceFilter.Fields.Add(new PSLib.Filter.Field(resourceFilter.FilterTableName, resourceAssignmentDs.ResourceAssignment.ASSN_UIDColumn.ColumnName, PSLib.Filter.SortOrderTypeEnum.None));
            resourceFilter.Fields.Add(new PSLib.Filter.Field(resourceFilter.FilterTableName, resourceAssignmentDs.ResourceAssignment.PROJ_NAMEColumn.ColumnName, PSLib.Filter.SortOrderTypeEnum.None));
            resourceFilter.Fields.Add(new PSLib.Filter.Field(resourceFilter.FilterTableName, resourceAssignmentDs.ResourceAssignment.PROJ_UIDColumn.ColumnName, PSLib.Filter.SortOrderTypeEnum.None));

            List<PSLib.Filter.FieldOperator> resourceFieldOps = new List<PSLib.Filter.FieldOperator>();
            PSLib.Filter.IOperator[] fos = new PSLib.Filter.IOperator[resources.Length];
            for (int i = 0; i < resources.Length; i++)
            {
                fos[i] = new PSLib.Filter.FieldOperator(PSLib.Filter.FieldOperationType.Equal, resourceAssignmentDs.ResourceAssignment.RES_UIDColumn.ColumnName, resources[i]);
            }

            PSLib.Filter.LogicalOperator lo = new Microsoft.Office.Project.Server.Library.Filter.LogicalOperator(PSLib.Filter.LogicalOperationType.Or, fos);
            resourceFilter.Criteria = lo;
            return resourceFilter;
        }
        private TimesheetHeaderInfos GetTimesheetStatus(string user, Guid periodUID, Guid resUID, out Guid tuid, out SvcTimeSheet.TimesheetDataSet tsDS)
        {
            using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
            {
                SetImpersonation(GetResourceUidFromNtAccount(user));
                tsDS = timesheetClient.ReadTimesheetByPeriod(resUID, periodUID, SvcTimeSheet.Navigation.Current);


                if (tsDS.Headers.Rows.Count > 0)
                {
                    tuid = tsDS.Headers[0].TS_UID;
                    var rw = tsDS.Headers[0];
                    return new TimesheetHeaderInfos
                        {
                            Name = rw.TS_NAME,
                            Comments = rw.TS_COMMENTS,
                            Status = (int)rw.TS_STATUS_ENUM,
                            TotalActualWork = rw.TS_TOTAL_ACT_VALUE / 60000m,
                            TotalOverTimeWork = rw.TS_TOTAL_ACT_OVT_VALUE / 60000m,
                            TotalNonBillable = rw.TS_TOTAL_ACT_NON_BILLABLE_VALUE / 60000m,
                            TotalNonBillableOvertime = rw.TS_TOTAL_ACT_NON_BILLABLE_OVT_VALUE / 60000m,
                            TSUID = tuid,

                        };

                }
                else
                {
                    tuid = Guid.Empty;
                    return null;
                }
            }
        }

        private SvcTimeSheet.TimesheetListDataSet GetTimesheetStatus(string user, Guid resUID, DateTime startDate, DateTime endDate, int select)
        {
            using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
            {
                SetImpersonation(GetResourceUidFromNtAccount(user));
                return timesheetClient.ReadTimesheetList(resUID, startDate, endDate, select);
            }

        }
        private void copyToActualRow(WholeLine group, SvcTimeSheet.TimesheetDataSet.ActualsRow day, int i, ViewConfigurationBase configuration)
        {
            ViewConfigurationRow extConf = configuration as ViewConfigurationRow;
            foreach (ExtendedRow z in group.Actuals)
            {
                if (!z.Changed) continue;
                if (z.Values.Value is ActualWorkRow)
                {
                    ActualWorkRow x = z.Values.Value as ActualWorkRow;
                    if (configuration.ActualWorkA && x.DayTimes != null && x.DayTimes[i].HasValue)
                    {
                        day.TS_ACT_VALUE = x.DayTimes[i].Value * 60000m;
                    }
                }
                else if (z.Values.Value is ActualOvertimeWorkRow)
                {
                    ActualOvertimeWorkRow x = z.Values.Value as ActualOvertimeWorkRow;
                    if (configuration.ActualOvertimeWorkA && x.DayTimes != null && x.DayTimes[i].HasValue)
                    {
                        day.TS_ACT_OVT_VALUE = x.DayTimes[i].Value * 60000m;
                    }
                }
                else if (z.Values.Value is NonBillableActualWorkRow)
                {
                    NonBillableActualWorkRow x = z.Values.Value as NonBillableActualWorkRow;
                    if (extConf != null && extConf.ActualNonBillableWorkA && x.DayTimes != null && x.DayTimes[i].HasValue)
                    {
                        day.TS_ACT_NON_BILLABLE_VALUE = x.DayTimes[i].Value * 60000m;
                    }
                }
                else if (z.Values.Value is NonBillableOvertimeWorkRow)
                {
                    NonBillableOvertimeWorkRow x = z.Values.Value as NonBillableOvertimeWorkRow;
                    if (extConf != null && extConf.ActualNonBillableOvertimeWorkA && x.DayTimes != null && x.DayTimes[i].HasValue)
                    {
                        day.TS_ACT_NON_BILLABLE_OVT_VALUE = x.DayTimes[i].Value * 60000m;
                    }
                }
                else if (z.Values.Value is AdministrativeRow)
                {
                    AdministrativeRow x = z.Values.Value as AdministrativeRow;
                    if (extConf != null && extConf.ActualWorkA && x.DayTimes != null && x.DayTimes[i].HasValue)
                    {
                        day.TS_ACT_VALUE = x.DayTimes[i].Value * 60000m;
                    }
                }

            }
        }
        private Guid GetTaskUID(Guid assn_uid, SvcResource.ResourceAssignmentDataSet _resAssDS)
        {
            string expression = "ASSN_UID = '" + assn_uid + "'";
            //SvcTimeSheet.TimesheetDataSet.LinesRow[] lines = (SvcTimeSheet.TimesheetDataSet.LinesRow[])_tsDS.Lines.Select(expression);
            //DataRow[] lines = (DataRow[])

            SvcResource.ResourceAssignmentDataSet.ResourceAssignmentRow[] lines = (SvcResource.ResourceAssignmentDataSet.ResourceAssignmentRow[])_resAssDS.ResourceAssignment.Select(expression);

            return new Guid(lines[0].TASK_UID.ToString());

        }

        public bool IsAdminproject(string projectName)
        {
            bool retVal = false;

            var ds = adminClient.ReadLineClasses(SvcAdmin.LineClassType.All, new SvcAdmin.LineClassState());
            string[] subprojects = projectName.Split(",".ToCharArray());
            foreach (string name in subprojects)
            {
                if (ds.LineClasses.Any(m => m.TS_LINE_CLASS_NAME == name))
                    retVal = true;
                break;
            }

            return retVal;
        }
        private void createRow(string user, WholeLine group, ref SvcTimeSheet.TimesheetDataSet _tsDS, SvcResource.ResourceAssignmentDataSet _resAssDS, SvcTimeSheet.TimesheetDataSet.LinesRow y, ViewConfigurationBase configuration, DateTime Start, DateTime Stop, string assignementId, string projectId, string projectName)
        {

            //if(string.IsNullOrEmpty(assignementId))
            //{
            //    var projectDataSet  = projectClient.ReadProject(new Guid(projectId),
            //}

            bool isAdmin = group.Actuals != null && group.Actuals.Count > 0 && group.Actuals[0].Values != null &&
                            ((group.Actuals[0].Values.Value != null && group.Actuals[0].Values.Value is AdministrativeRow) ||
                            (group.Actuals[0].Values.OldValue != null && group.Actuals[0].Values.OldValue is AdministrativeRow));
            if (!group.Changed) return;
            if (y == null)//creation
            {
                try
                {
                    SvcAdmin.TimesheetLineClassDataSet tsLineClassDs;

                    tsLineClassDs = new SvcAdmin.TimesheetLineClassDataSet();
                    tsLineClassDs = adminClient.ReadLineClasses(SvcAdmin.LineClassType.All, SvcAdmin.LineClassState.Enabled);


                    Guid timeSheetUID = new Guid(_tsDS.Headers[0].TS_UID.ToString());



                    SvcTimeSheet.TimesheetDataSet.LinesRow line = _tsDS.Lines.NewLinesRow();  //Create a new row for the timesheet

                    line.TS_UID = timeSheetUID;
                    line.ASSN_UID = new Guid(assignementId);

                    //try if this works, may be we need it when reading the rows; Francesco
                    line.TS_LINE_UID = Guid.NewGuid();
                    line.TS_LINE_COMMENT = BusisnessResources.InitLineComment;


                    if (isAdmin)
                    {
                        line.TS_LINE_CLASS_UID = new Guid(assignementId);
                        line.TS_LINE_STATUS = (byte)PSLib.TimesheetEnum.LineStatus.NotApplicable;
                        line.TS_LINE_VALIDATION_TYPE = (byte)PSLib.TimesheetEnum.ValidationType.Unverified;
                        SvcAdmin.TimesheetLineClassDataSet.LineClassesRow foundTSClassRow;
                        foundTSClassRow = tsLineClassDs.LineClasses.FindByTS_LINE_CLASS_UID(new Guid(assignementId));
                        line.TS_LINE_CACHED_ASSIGN_NAME = foundTSClassRow.TS_LINE_CLASS_NAME;
                    }
                    else
                    {

                        if (LoggedUser(user) == GetTimesheetMgrUID(user))
                        {
                            line.TS_LINE_STATUS = (byte)PSLib.TimesheetEnum.LineStatus.Approved;
                        }
                        else
                        {
                            line.TS_LINE_STATUS = (byte)PSLib.TimesheetEnum.LineStatus.PendingApproval;
                        }
                        line.TS_LINE_VALIDATION_TYPE = (byte)PSLib.TimesheetEnum.ValidationType.Verified;
                        line.TS_LINE_CLASS_UID = new Guid(group.Actuals[0].Values.Value.LineClass.Id);
                        line.TS_LINE_VALIDATION_TYPE = (byte)PSLib.TimesheetEnum.ValidationType.Verified;
                        line.TS_LINE_CACHED_ASSIGN_NAME = tsLineClassDs.LineClasses[0].TS_LINE_CLASS_DESC;


                        if (!(_resAssDS.ResourceAssignment.Any(t => t.ASSN_UID == line.ASSN_UID)))
                        {
                            line.TS_LINE_VALIDATION_TYPE = (int)Microsoft.Office.Project.Server.Library.TimesheetEnum.ValidationType.ProjectLevel;
                            line.TASK_UID = Guid.NewGuid();
                            line.PROJ_UID = new Guid(projectId);
                            line.TS_LINE_CACHED_PROJ_NAME = projectName;
                            line.TS_LINE_CACHED_ASSIGN_NAME = "Top Level";
                            group.IsTopLevelTask = true;
                        }
                        else
                        {
                            line.TASK_UID = GetTaskUID(line.ASSN_UID, _resAssDS);
                        }

                    }

                    _tsDS.Lines.AddLinesRow(line);  //add new row to the timesheet dataset

                    Guid[] uids = new Guid[] { line.TS_LINE_UID };

                    using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
                    {
                        SetImpersonation(GetResourceUidFromNtAccount(user));
                        timesheetClient.PrepareTimesheetLine(timeSheetUID, ref _tsDS, uids);  //Validates and populates a timesheet line item and preloads actuals table in the dataset
                    }

                    createActuals(_tsDS, line, Start, Stop);


                }
                catch (Exception e)
                {
                    HttpContext.Current.Trace.Warn("An exception occured in CreateRow and error = " + e.Message);
                    return;

                }
            }
        }
        private void copyToRow(WholeLine group, SvcTimeSheet.TimesheetDataSet _tsDS, SvcResource.ResourceAssignmentDataSet _resAssDS, SvcTimeSheet.TimesheetDataSet.LinesRow lineRow, ViewConfigurationBase configuration, DateTime Start, DateTime Stop, string assignementId)
        {
            if (!group.Changed) return;

            bool[] processed = new bool[Convert.ToInt32((Stop.Date - Start.Date).TotalDays) + 1];
            var allLines = lineRow.GetActualsRows();
            if (allLines == null || allLines.Length == 0)
            {
                createActuals(_tsDS, lineRow, Start, Stop);
                allLines = lineRow.GetActualsRows();
            }
            else
            {
                foreach (SvcTimeSheet.TimesheetDataSet.ActualsRow day in allLines.OrderBy(m => m.TS_ACT_START_DATE))
                {
                    HttpContext.Current.Trace.Warn("Start Date = " + day.TS_ACT_START_DATE);
                    HttpContext.Current.Trace.Warn("End Date = " + day.TS_ACT_FINISH_DATE);
                }
            }
            if (allLines != null)
            {
                int i = 0;
                foreach (SvcTimeSheet.TimesheetDataSet.ActualsRow day in allLines.OrderBy(m => m.TS_ACT_START_DATE))
                {
                    if (i >= processed.Length)
                    {
                        continue;
                    }
                    processed[i] = true;
                    copyToActualRow(group, day, i, configuration);
                    i++;
                }
            }

        }
        private void createActuals(SvcTimeSheet.TimesheetDataSet _tsDS, SvcTimeSheet.TimesheetDataSet.LinesRow lineRow, DateTime Start, DateTime Stop)
        {
            DateTime day = Start;
            while (day <= Stop)
            {
                SvcTimeSheet.TimesheetDataSet.ActualsRow actualsRow = _tsDS.Actuals.NewActualsRow();
                actualsRow.TS_LINE_UID = lineRow.TS_LINE_UID;

                actualsRow.TS_ACT_START_DATE = day;
                HttpContext.Current.Trace.Warn("Actual Start Date is " + day); 
                actualsRow.TS_ACT_FINISH_DATE = day.AddDays(1);
                HttpContext.Current.Trace.Warn("Actual End Date is " + day.AddDays(1)); 
                _tsDS.Actuals.AddActualsRow(actualsRow);
                day = day.AddDays(1);
            }
        }
        private bool GetAllSingleValues(List<LineClass> classses, string currentLineClassId, SvcTimeSheet.TimesheetDataSet timesheetDataSet
            , SvcCustomFields.CustomFieldDataSet customDataSet, string user, ViewConfigurationBase configuration, string periodId, DateTime start, DateTime stop
            , string projectId, string assignementId, ActualWorkRow ar, ActualOvertimeWorkRow aor, SingleValuesRow sv = null)
        {
            //SvcStatusing.StatusingDataSet res = proxy.ReadStatusForResource(LoggedUser(), new Guid(assignementId), start, stop);

            if (string.IsNullOrEmpty(assignementId))
            {
                return true;
            }
            bool isWindowsUser;
            var Resuid = GetResourceUidFromNtAccount(user);
            SvcStatusing.StatusingDataSet res;
            using (OperationContextScope scope = new OperationContextScope(pwaClient.InnerChannel))
            {
                SetImpersonation(GetResourceUidFromNtAccount(user));
                res = pwaClient.ReadStatus(new Guid(assignementId), start, stop);
            }
            var customfieldValues = res.AssnCustomFields.Where(t => t.ASSN_UID == new Guid(assignementId)).ToList();

            bool result = false;
            if (res.Assignments.Count > 0)
            {
                result = true;
                var sa = res.Assignments[0];

                if (ar != null)
                {

                    ar.AssignementId = assignementId;
                    if (configuration.WorkA && !sa.IsASSN_WORKNull()) ar.WorkA = sa.ASSN_WORK / 60000m;
                    if (configuration.RegularWorkA && !sa.IsASSN_REG_WORKNull()) ar.RegularWorkA = sa.ASSN_REG_WORK / 60000m;
                    if (configuration.RemainingWorkA && !sa.IsASSN_REM_WORKNull()) ar.RemainingWorkA = sa.ASSN_REM_WORK / 60000m;
                    if (configuration.FinishA && !sa.IsASSN_FINISH_DATENull()) ar.FinishA = sa.ASSN_FINISH_DATE;
                    if (configuration.StartA && !sa.IsASSN_START_DATENull()) ar.StartA = sa.ASSN_START_DATE;
                    if (configuration.ActualFinishA && !sa.IsASSN_ACT_FINISHNull()) ar.ActualFinishA = sa.ASSN_ACT_FINISH;
                    if (configuration.ActualStartA && !sa.IsASSN_ACT_STARTNull()) ar.ActualStartA = sa.ASSN_ACT_START;

                    if (configuration.PercentWorkCompleteA && !sa.IsASSN_PCT_WORK_COMPLETENull()) ar.PercentWorkCompleteA = (uint)sa.ASSN_PCT_WORK_COMPLETE;
                    if (configuration.AssignmentUnitsA && !sa.IsASSN_UNITSNull()) ar.AssignmentUnitsA = sa.ASSN_UNITS;
                    if (configuration.ConfirmedA && !sa.IsASSN_IS_CONFIRMEDNull()) ar.ConfirmedA = sa.ASSN_IS_CONFIRMED;
                    if (configuration.CommentsA && !sa.IsWASSN_COMMENTSNull()) ar.CommentsA = sa.WASSN_COMMENTS;


                }
                if (aor != null)
                {
                    aor.AssignementId = assignementId;
                    if (configuration.OvertimeWorkA && !sa.IsASSN_OVT_WORKNull()) aor.OvertimeWorkA = sa.ASSN_OVT_WORK / 60000m;
                }
                if (sv != null)
                {
                    sv.AssignementId = assignementId;
                    if (configuration.WorkA && !sa.IsASSN_WORKNull()) sv.WorkA = sa.ASSN_WORK / 60000m;
                    if (configuration.RegularWorkA && !sa.IsASSN_REG_WORKNull()) sv.RegularWorkA = sa.ASSN_REG_WORK / 60000m;
                    if (configuration.RemainingWorkA && !sa.IsASSN_REM_WORKNull()) sv.RemainingWorkA = sa.ASSN_REM_WORK / 60000m;
                    if (configuration.FinishA && !sa.IsASSN_FINISH_DATENull()) sv.FinishA = sa.ASSN_FINISH_DATE;
                    if (configuration.StartA && !sa.IsASSN_START_DATENull()) sv.StartA = sa.ASSN_START_DATE;
                    if (configuration.ActualFinishA && !sa.IsASSN_ACT_FINISHNull()) sv.ActualFinishA = sa.ASSN_ACT_FINISH;
                    if (configuration.ActualStartA && !sa.IsASSN_ACT_STARTNull()) sv.ActualStartA = sa.ASSN_ACT_START;

                    if (configuration.PercentWorkCompleteA && !sa.IsASSN_PCT_WORK_COMPLETENull()) sv.PercentWorkCompleteA = (uint)sa.ASSN_PCT_WORK_COMPLETE;
                    if (configuration.AssignmentUnitsA && !sa.IsASSN_UNITSNull()) sv.AssignmentUnitsA = sa.ASSN_UNITS;
                    if (configuration.ConfirmedA && !sa.IsASSN_IS_CONFIRMEDNull()) sv.ConfirmedA = sa.ASSN_IS_CONFIRMED;
                    if (configuration.CommentsA && !sa.IsWASSN_COMMENTSNull()) sv.CommentsA = sa.WASSN_COMMENTS;
                    if (configuration.OvertimeWorkA && !sa.IsASSN_OVT_WORKNull()) sv.OvertimeWorkA = sa.ASSN_OVT_WORK / 60000m;


                }
            }
            else
            {
                ar.AssignementId = assignementId;
                if (configuration.CustomFields != null)
                {
                    ar.CustomFieldItems = GetCustomFields(user, configuration.CustomFields, assignementId, start, stop, customDataSet);

                }

                if (!string.IsNullOrEmpty(currentLineClassId))
                {
                    ar.LineClass = new LineClass(currentLineClassId, classses.First(t => t.Id == currentLineClassId).Name);
                }
                else
                {
                    if (timesheetDataSet.Lines.Any(t => t.ASSN_UID.ToString() == assignementId && !t.IsTS_LINE_ACT_SUM_VALUENull() && (ar.DayTimes.Sum() * 60000) == t.TS_LINE_ACT_SUM_VALUE))
                    {
                        var line = timesheetDataSet.Lines.First(t => t.ASSN_UID.ToString() == ar.AssignementId && !t.IsTS_LINE_ACT_SUM_VALUENull() && (ar.DayTimes.Sum() * 60000) == t.TS_LINE_ACT_SUM_VALUE);
                        if (classses.Any(t => t.Id == line.TS_LINE_CLASS_UID.ToString()))
                        {
                            var lineClass = classses.First(t => t.Id == line.TS_LINE_CLASS_UID.ToString());
                            ar.LineClass = new LineClass(lineClass.Id, lineClass.Name);
                        }
                        else
                        {
                            ar.LineClass = GetLineClassifications().First(t => t.Name == "Standard");
                        }

                    }
                    else
                    {
                        ar.LineClass = GetLineClassifications().First(t => t.Name == "Standard");
                    }
                }

            }
            if (res.Tasks.Count > 0)
            {
                result = true;
                var customds = customDataSet.CustomFields;
                var st = res.Tasks[0];
                if (ar != null)
                {
                    ar.AssignementId = assignementId;
                    if (configuration.WorkT && !st.IsTASK_WORKNull()) ar.WorkT = st.TASK_WORK / 60000m;
                    if (configuration.RegularWorkT && !st.IsTASK_REG_WORKNull()) ar.RegularWorkT = st.TASK_REG_WORK / 60000m;
                    if (configuration.RemainingWorkT && !st.IsTASK_REM_WORKNull()) ar.RemainingWorkT = st.TASK_REM_WORK / 60000m;
                    if (configuration.ActualWorkT && !st.IsTASK_ACT_WORKNull()) ar.ActualWorkT = st.TASK_ACT_WORK / 60000m;
                    if (configuration.StartT && !st.IsTASK_START_DATENull()) ar.StartT = st.TASK_START_DATE;
                    if (configuration.FinishT && !st.IsTASK_FINISH_DATENull()) ar.FinishT = st.TASK_FINISH_DATE;
                    if (configuration.ResumeT && !st.IsTASK_RESUME_DATENull()) ar.ResumeT = st.TASK_RESUME_DATE;
                    if (configuration.DeadlineT && !st.IsTASK_DEADLINENull()) ar.DeadlineT = st.TASK_DEADLINE;
                    if (configuration.DurationT && !st.IsTASK_DURNull()) ar.DurationT = (uint)(st.TASK_DUR / 4800m);
                    if (configuration.RemainingDurationT && !st.IsTASK_REM_DURNull()) ar.RemainingDurationT = (uint)(st.TASK_REM_DUR / 4800m);
                    if (configuration.TaskNameT && !st.IsTASK_NAMENull()) ar.TaskNameT = st.TASK_NAME;
                    if (configuration.PercentCompleteT && !st.IsTASK_PCT_COMPNull()) ar.PercentCompleteT = (uint)st.TASK_PCT_COMP;
                    if (configuration.PercentWorkCompleteT && !st.IsTASK_PCT_WORK_COMPNull()) ar.PercentWorkCompleteT = (uint)st.TASK_PCT_WORK_COMP;
                    if (configuration.PhysicalPercentCompleteT && !st.IsTASK_PHY_PCT_COMPNull()) ar.PhysicalPercentCompleteT = (uint)st.TASK_PHY_PCT_COMP;

                    if (configuration.CustomFields != null)
                    {
                        ar.CustomFieldItems = GetCustomFields(user, configuration.CustomFields, assignementId, start, stop, customDataSet);
                    }
                    if (!string.IsNullOrEmpty(currentLineClassId))
                    {
                        ar.LineClass = new LineClass(currentLineClassId, classses.First(t => t.Id == currentLineClassId).Name);
                    }
                    else
                    {
                        if (timesheetDataSet.Lines.Any(t => t.ASSN_UID.ToString() == assignementId && !t.IsTS_LINE_ACT_SUM_VALUENull() && (ar.DayTimes.Sum() * 60000) == t.TS_LINE_ACT_SUM_VALUE))
                        {
                            var line = timesheetDataSet.Lines.First(t => t.ASSN_UID.ToString() == ar.AssignementId && !t.IsTS_LINE_ACT_SUM_VALUENull() &&  (ar.DayTimes.Sum() * 60000) == t.TS_LINE_ACT_SUM_VALUE);
                            if (classses.Any(t => t.Id == line.TS_LINE_CLASS_UID.ToString()))
                            {
                                var lineClass = classses.First(t => t.Id == line.TS_LINE_CLASS_UID.ToString());
                                ar.LineClass = new LineClass(lineClass.Id, lineClass.Name);
                            }
                            else
                            {
                                ar.LineClass = GetLineClassifications().First(t => t.Name == "Standard");
                            }

                        }
                        else
                        {
                            ar.LineClass = GetLineClassifications().First(t => t.Name == "Standard");
                        }
                    }

                }
                if (aor != null)
                {
                    aor.AssignementId = assignementId;
                    if (configuration.OvertimeWorkT && !st.IsTASK_OVT_WORKNull()) aor.OvertimeWorkT = st.TASK_OVT_WORK / 60000m;
                    if (configuration.RemainingOvertimeWorkT && !st.IsTASK_REM_OVT_WORKNull()) aor.RemainingOvertimeWorkT = st.TASK_REM_OVT_WORK / 60000m;
                    if (!string.IsNullOrEmpty(currentLineClassId))
                    {
                        aor.LineClass = new LineClass(currentLineClassId, classses.First(t => t.Id == currentLineClassId).Name);
                    }
                    else
                    {
                        if (timesheetDataSet.Lines.Any(t => t.ASSN_UID.ToString() == assignementId &&  !t.IsTS_LINE_ACT_SUM_VALUENull() &&(aor.DayTimes.Sum() * 60000) == t.TS_LINE_ACT_SUM_VALUE))
                        {
                            var line = timesheetDataSet.Lines.First(t => t.ASSN_UID.ToString() == aor.AssignementId && !t.IsTS_LINE_ACT_SUM_VALUENull() && (aor.DayTimes.Sum() * 60000) == t.TS_LINE_ACT_SUM_VALUE);
                            if (classses.Any(t => t.Id == line.TS_LINE_CLASS_UID.ToString()))
                            {
                                var lineClass = classses.First(t => t.Id == line.TS_LINE_CLASS_UID.ToString());
                                aor.LineClass = new LineClass(lineClass.Id, lineClass.Name);
                            }
                            else
                            {
                                aor.LineClass = GetLineClassifications().First(t => t.Name == "Standard");
                            }

                        }
                        else
                        {
                            aor.LineClass = GetLineClassifications().First(t => t.Name == "Standard");
                        }
                    }
                }
                if (sv != null)
                {
                    sv.AssignementId = assignementId;
                    if (configuration.WorkT && !st.IsTASK_WORKNull()) sv.WorkT = st.TASK_WORK / 60000m;
                    if (configuration.RegularWorkT && !st.IsTASK_REG_WORKNull()) sv.RegularWorkT = st.TASK_REG_WORK / 60000m;
                    if (configuration.RemainingWorkT && !st.IsTASK_REM_WORKNull()) sv.RemainingWorkT = st.TASK_REM_WORK / 60000m;
                    if (configuration.ActualWorkT && !st.IsTASK_ACT_WORKNull()) sv.ActualWorkT = st.TASK_ACT_WORK / 60000m;
                    if (configuration.StartT && !st.IsTASK_START_DATENull()) sv.StartT = st.TASK_START_DATE;
                    if (configuration.FinishT && !st.IsTASK_FINISH_DATENull()) sv.FinishT = st.TASK_FINISH_DATE;
                    if (configuration.ResumeT && !st.IsTASK_RESUME_DATENull()) sv.ResumeT = st.TASK_RESUME_DATE;
                    if (configuration.DeadlineT && !st.IsTASK_DEADLINENull()) sv.DeadlineT = st.TASK_DEADLINE;
                    if (configuration.DurationT && !st.IsTASK_DURNull()) sv.DurationT = (uint)(st.TASK_DUR / 4800m);
                    if (configuration.RemainingDurationT && !st.IsTASK_REM_DURNull()) sv.RemainingDurationT = (uint)(st.TASK_REM_DUR / 4800m);
                    if (configuration.TaskNameT && !st.IsTASK_NAMENull()) sv.TaskNameT = st.TASK_NAME;
                    if (configuration.PercentCompleteT && !st.IsTASK_PCT_COMPNull()) sv.PercentCompleteT = (uint)st.TASK_PCT_COMP;
                    if (configuration.PercentWorkCompleteT && !st.IsTASK_PCT_WORK_COMPNull()) sv.PercentWorkCompleteT = (uint)st.TASK_PCT_WORK_COMP;
                    if (configuration.PhysicalPercentCompleteT && !st.IsTASK_PHY_PCT_COMPNull()) sv.PhysicalPercentCompleteT = (uint)st.TASK_PHY_PCT_COMP;
                    if (configuration.OvertimeWorkT && !st.IsTASK_OVT_WORKNull()) sv.OvertimeWorkT = st.TASK_OVT_WORK / 60000m;
                    if (configuration.RemainingOvertimeWorkT && !st.IsTASK_REM_OVT_WORKNull()) sv.RemainingOvertimeWorkT = st.TASK_REM_OVT_WORK / 60000m;
                    if (configuration.CustomFields != null)
                    {

                        sv.CustomFieldItems = GetCustomFields(user, configuration.CustomFields, assignementId, start, stop, customDataSet);

                    }
                    if (!string.IsNullOrEmpty(currentLineClassId))
                    {
                        sv.LineClass = new LineClass(currentLineClassId, classses.First(t => t.Id == currentLineClassId).Name);
                    }
                    else
                    {
                        if (timesheetDataSet.Lines.Any(t => t.ASSN_UID.ToString() == assignementId && !t.IsTS_LINE_ACT_SUM_VALUENull() && (sv.DayTimes.Sum() * 60000) == t.TS_LINE_ACT_SUM_VALUE))
                        {
                            var line = timesheetDataSet.Lines.First(t => t.ASSN_UID.ToString() == sv.AssignementId && !t.IsTS_LINE_ACT_SUM_VALUENull() && (sv.DayTimes.Sum() * 60000) == t.TS_LINE_ACT_SUM_VALUE);
                            if (classses.Any(t => t.Id == line.TS_LINE_CLASS_UID.ToString()))
                            {
                                var lineClass = classses.First(t => t.Id == line.TS_LINE_CLASS_UID.ToString());
                                sv.LineClass = new LineClass(lineClass.Id, lineClass.Name);
                            }

                        }
                        else
                        {
                            sv.LineClass = GetLineClassifications().First(t => t.Name == "Standard");
                        }
                    }
                }

            }
            return result;
        }
        public List<CustomFieldItem> GetCustomFields(string user, List<CustomField> fields, string assignementId, DateTime start, DateTime stop, SvcCustomFields.CustomFieldDataSet customFieldDataSet = null)
        {
            List<CustomFieldItem> values = new List<CustomFieldItem>();
            var customds = customFieldDataSet.CustomFields;
            bool isWindowsUser;
            var Resuid = GetResourceUidFromNtAccount(user);
            SvcStatusing.StatusingDataSet res;
            using (OperationContextScope scope = new OperationContextScope(pwaClient.InnerChannel))
            {
                SetImpersonation(GetResourceUidFromNtAccount(user));
                res = pwaClient.ReadStatus(new Guid(assignementId), start, stop);
            }
            var customfieldValues = res.AssnCustomFields.Where(t => t.ASSN_UID == new Guid(assignementId)).ToList();
            foreach (CustomField field in fields)
            {

                var id = customds.First(m => m.MD_PROP_NAME == field.FullName).MD_PROP_UID_SECONDARY;
                if (customfieldValues.Any(t => !t.IsMD_PROP_UIDNull() && t.MD_PROP_UID == id))
                {
                    var customfield = customfieldValues.First(t => !t.IsMD_PROP_UIDNull() && t.MD_PROP_UID == id);
                    CustomFieldItem item = new CustomFieldItem();
                    switch (customfield.FIELD_TYPE_ENUM)
                    {
                        case 4: item.DataType = "Date";
                            if (!customfield.IsDATE_VALUENull())
                                item.DateValue = customfield.DATE_VALUE;
                            break;
                        case 9: item.DataType = "Cost";
                            if (!customfield.IsNUM_VALUENull())
                                item.CostValue = customfield.NUM_VALUE;
                            break;
                        case 6: item.DataType = "Duration";
                            if (!customfield.IsDUR_VALUENull())
                                item.DurationValue = customfield.DUR_VALUE;
                            break;
                        case 27: item.DataType = "Finishdate";
                            if (!customfield.IsDATE_VALUENull())
                                item.DateValue = customfield.DATE_VALUE;
                            break;
                        case 17: item.DataType = "Flag";
                            if (!customfield.IsFLAG_VALUENull())
                                item.FlagValue = customfield.FLAG_VALUE;
                            break;
                        case 15: item.DataType = "Number";
                            if (!customfield.IsNUM_VALUENull())
                                item.NumValue = customfield.NUM_VALUE;
                            break;
                        case 21: item.DataType = "Text";
                            if (!customfield.IsTEXT_VALUENull())
                                item.TextTValue = customfield.TEXT_VALUE;
                            break;
                    }
                    if (!customfield.IsCODE_VALUENull())
                    {
                        SvcLookupTable.LookupTableDataSet lookups;

                        lookups = lookupTableClient.ReadLookupTables("", false, System.Globalization.CultureInfo.InvariantCulture.LCID);
                        IEnumerable<SvcLookupTable.LookupTableDataSet.LookupTableTreesRow> lookupRows = lookups.LookupTableTrees.Where(t => t.LT_STRUCT_UID == customfield.CODE_VALUE);
                        string value = "";
                        foreach (SvcLookupTable.LookupTableDataSet.LookupTableTreesRow lookupRow in lookupRows)
                        {
                            switch ((Microsoft.Office.Project.Server.Library.PSDataType)customfield.FIELD_TYPE_ENUM)
                            {
                                case PSLib.PSDataType.DATE:
                                    if (!lookupRow.IsLT_VALUE_DATENull())
                                        value += lookupRow.LT_VALUE_DATE.ToShortDateString() + ",";
                                    break;
                                case PSLib.PSDataType.COST:
                                case PSLib.PSDataType.NUMBER:
                                    if (!lookupRow.IsLT_VALUE_NUMNull())
                                        value += lookupRow.LT_VALUE_NUM.ToString() + ",";
                                    break;
                                case PSLib.PSDataType.DURATION:
                                    if (!lookupRow.IsLT_VALUE_DURNull())
                                        value += lookupRow.LT_VALUE_DUR.ToString() + ",";
                                    break;
                                case PSLib.PSDataType.STRING:
                                    if (!lookupRow.IsLT_VALUE_TEXTNull())
                                        value += lookupRow.LT_VALUE_TEXT + ",";

                                    break;
                            }
                            item.LookupTableGuid = lookupRow.LT_UID;
                            item.LookupID = lookupRow.LT_STRUCT_UID;
                        }
                        item.LookupValue = value.Trim(',');
                        var customDs = customFieldDataSet;
                        var csfield = customds.First(t => t.MD_PROP_NAME == field.FullName);
                        item.CustomFieldGuid = csfield.MD_PROP_UID_SECONDARY;
                        if (item.LookupTableGuid.HasValue)
                            item.LookupTableItems = GetLookupTableValuesAsItems(item.LookupTableGuid.Value, item.DataType).ToList();
                    }
                    else
                    {
                        var customDs = customFieldDataSet;
                        var csfield = customds.First(t => t.MD_PROP_NAME == field.FullName);
                        if (!csfield.IsMD_LOOKUP_TABLE_UIDNull())
                        {
                            string value = null;
                            item.LookupValue = value;
                            item.LookupTableGuid = csfield.MD_LOOKUP_TABLE_UID;
                            item.CustomFieldGuid = csfield.MD_PROP_UID_SECONDARY;
                            item.LookupTableItems = GetLookupTableValuesAsItems(csfield.MD_LOOKUP_TABLE_UID,
                                item.DataType).ToList();
                        }
                    }
                    item.Name = field.Name;
                    item.FullName = field.FullName;
                    values.Add(item);
                }
                else
                {
                    CustomFieldItem item = new CustomFieldItem();
                    item.Name = field.Name;
                    item.FullName = field.FullName;
                    var customDs = customFieldDataSet;
                    var csfield = customds.First(t => t.MD_PROP_NAME == field.FullName);
                    switch (csfield.MD_PROP_TYPE_ENUM)
                    {
                        case 4: item.DataType = "Date";
                            break;
                        case 9: item.DataType = "Cost";
                            break;
                        case 6: item.DataType = "Duration";
                            break;
                        case 27: item.DataType = "Finishdate";
                            break;
                        case 17: item.DataType = "Flag";
                            break;
                        case 15: item.DataType = "Number";
                            break;
                        case 21: item.DataType = "Text";
                            break;

                    }
                    if (!csfield.IsMD_LOOKUP_TABLE_UIDNull())
                    {
                        string value = null;
                        item.LookupValue = value;
                        item.LookupTableGuid = csfield.MD_LOOKUP_TABLE_UID;
                        item.CustomFieldGuid = csfield.MD_PROP_UID_SECONDARY;
                        item.LookupTableItems = GetLookupTableValuesAsItems(csfield.MD_LOOKUP_TABLE_UID,
                            item.DataType).ToList();
                    }
                    item.DateValue = null;
                    values.Add(item);
                }


            }
            return values;
        }

        public LookupTableDisplayItem[] GetLookupTableValuesAsItems(Guid tableUid, string dataType)
        {
            SvcLookupTable.LookupTableDataSet ds = new SvcLookupTable.LookupTableDataSet();
            var obj = GetApplicationObject("LookupTableItems");
            if (obj != null)
            {
                return (LookupTableDisplayItem[])obj;
            }

            try
            {
                ds = lookupTableClient.ReadLookupTablesByUids(new Guid[] { tableUid }, false, -1);
            }

            catch (SoapException ex)
            {
                throw (ex);
            }
            catch (WebException ex)
            {
                throw (ex);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            LookupTableDisplayItem[] items = new LookupTableDisplayItem[ds.LookupTableTrees.Count];

            for (int i = 0; i < ds.LookupTableTrees.Count; i++)
            {
                // The display text varies based on type. 
                // For some datatypes, the description is in the text field.
                items[i] = new LookupTableDisplayItem(
                            ds.LookupTableTrees[i].LT_STRUCT_UID,
                            (ds.LookupTableTrees[i].IsLT_VALUE_DESCNull()
                                ? ds.LookupTableTrees[i].LT_VALUE_TEXT
                                : ds.LookupTableTrees[i].LT_VALUE_DESC),
                            dataType.ToString(),
                            BoxMeUp(ds.LookupTableTrees[i], dataType));
            }
            CacheApplicationObject("LookupTableItems", items);
            return items;
        }

        private object BoxMeUp(SvcLookupTable.LookupTableDataSet.LookupTableTreesRow row,
                            string dataType)
        {
            switch (dataType)
            {
                case "Flag":
                    throw new Exception("Yes/No is not a valid lookup table datatype.");
                case "Cost":
                    return (object)row.LT_VALUE_NUM;
                case "Date":
                    return (object)row.LT_VALUE_DATE;
                case "Duration":
                    return (object)row.LT_VALUE_DUR;
                case "Number":
                    return (object)row.LT_VALUE_NUM;
                case "Text":
                    return (object)row.LT_VALUE_TEXT;
                default:
                    throw new Exception("Invalid type was specified for a lookup table.");
            }
        }


        public TimesheetsSets DefaultTimesheetSet { get { return TimesheetsSets.Last3; } }
        public IEnumerable<ProjectInfo> UserProjects(string user)
        {
            List<ProjectInfo> res = new List<ProjectInfo>();

            SvcResource.ResourceAssignmentDataSet resourceAssignmentDS = GetResourceAssignmentDataSet(user);
            DataTable projects = (DataTable)resourceAssignmentDS.ResourceAssignment.DefaultView.ToTable(true, "PROJ_UID", "PROJ_NAME");
            foreach (DataRow row in projects.Rows)
            {
                res.Add(
                    new ProjectInfo()
                    {
                        Id = row["PROJ_UID"].ToString(),
                        Name = row["PROJ_NAME"].ToString()
                    });
            }
            return res;
        }

        public IEnumerable<AssignementInfo> ProjectAssignements(string user, string ProjectId)
        {
            List<AssignementInfo> res = new List<AssignementInfo>();
            if (string.IsNullOrWhiteSpace(ProjectId))
            {
                return res;
            }
            if (ProjectId == "-1")
            {

                SvcAdmin.TimesheetLineClassDataSet tslineclassDS = new SvcAdmin.TimesheetLineClassDataSet();
                tslineclassDS = adminClient.ReadLineClasses(SvcAdmin.LineClassType.AllNonProject, SvcAdmin.LineClassState.Enabled);

                foreach (var x in tslineclassDS.LineClasses)
                {
                    res.Add(
                        new AssignementInfo()
                        {
                            Id = x.TS_LINE_CLASS_UID.ToString(),
                            Name = x.TS_LINE_CLASS_NAME
                        });
                }
            }
            else
            {

                SvcResource.ResourceAssignmentDataSet resourceAssignmentDS = GetResourceAssignmentDataSet(user);
                DataTable assignmentsDT = (DataTable)resourceAssignmentDS.ResourceAssignment.DefaultView.ToTable(true, "TASK_NAME", "ASSN_UID", "PROJ_UID");
                DataView view = new DataView();
                view.Table = assignmentsDT;

                view.RowFilter = "PROJ_UID = '" + ProjectId + "'";

                foreach (DataRowView row in view)
                {
                    res.Add(
                        new AssignementInfo()
                        {
                            Id = row["ASSN_UID"].ToString(),
                            Name = row["TASK_NAME"].ToString()
                        });
                }
                if (AllowToplevel)
                {
                    var projectDataSet = projectClient.ReadProject(new Guid(ProjectId), DataStoreEnum.PublishedStore);


                    var tasks = projectDataSet.Task;
                    if (tasks.Rows.Count > 0)
                    {
                        if (tasks.Any(t => t.TASK_IS_SUMMARY == true))
                        {
                            var summaryTask = tasks.First(t => t.TASK_IS_SUMMARY == true);
                            if (projectDataSet.Assignment.Any(t => t.TASK_UID == summaryTask.TASK_UID))
                            {
                                res.Add(
                                    new AssignementInfo()
                                    {
                                        Id = projectDataSet.Assignment.First(t => t.TASK_UID == summaryTask.TASK_UID).ASSN_UID.ToString(),
                                        Name = summaryTask.TASK_NAME,
                                        IsProjectLineType = true
                                    });

                            }
                            else
                            {
                                res.Add(
                                    new AssignementInfo()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        Name = "Top Level",
                                        IsProjectLineType = true
                                    });
                            }
                        }
                    }
                }
            }
            return res;
        }

        public IEnumerable<Timesheet> SelectTimesheets(string user, TimesheetsSets set, out DateTime start, out DateTime end)
        {


            int selection = 32; //all timesheets all the deleted ones
            DateTime Start = new DateTime(1984, 1, 1);
            DateTime End = new DateTime(2049, 12, 1);
            DateTime startFrom = DateTime.Today;
            DateTime EndWith = DateTime.Today;
            if (set == TimesheetsSets.Default) set = DefaultTimesheetSet;

            SvcAdmin.TimePeriodDataSet.TimePeriodsRow period = null;
            using (OperationContextScope scope = new OperationContextScope(adminClient.InnerChannel))
            {
                SetImpersonation(GetResourceUidFromNtAccount(AppPoolUser.Name));
                var periods = adminClient.ReadPeriods(SvcAdmin.PeriodState.All);
                period = periods.TimePeriods[0];
                if (periods.TimePeriods.Any(t => !t.IsWPRD_START_DATENull() && !t.IsWPRD_FINISH_DATENull()
                     && DateTime.Today >= t.WPRD_START_DATE && DateTime.Today <= t.WPRD_FINISH_DATE))
                {
                    var currentPeriod = periods.TimePeriods.First(t => !t.IsWPRD_START_DATENull() && !t.IsWPRD_FINISH_DATENull()
                         && DateTime.Today >= t.WPRD_START_DATE && DateTime.Today <= t.WPRD_FINISH_DATE);
                    startFrom = currentPeriod.WPRD_START_DATE;
                    EndWith = currentPeriod.WPRD_FINISH_DATE;
                }


            }

            switch (set)
            {
                case TimesheetsSets.CreatedProgress: selection = 1;
                    break;
                case TimesheetsSets.Last3:
                    Start = startFrom.AddMonths(-3);
                    End = EndWith;
                    break;
                case TimesheetsSets.Last6:
                    Start = startFrom.AddMonths(-6);
                    End = EndWith;
                    break;
                case TimesheetsSets.Next6Last3:
                    Start = startFrom.AddMonths(-3);
                    End = EndWith.AddMonths(+6);
                    break;
                default: selection = 32; break; //all existing

            }
            bool isWindowsUser;
            Guid resUID = GetResourceUidFromNtAccount(user);
            SvcTimeSheet.TimesheetListDataSet res;
            using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
            {
                SetImpersonation(GetResourceUidFromNtAccount(user));
                res = timesheetClient.ReadTimesheetList(resUID, new DateTime(1984, 1, 1), new DateTime(2049, 12, 1), selection);
                HttpContext.Current.Trace.Warn("list Count = " + res.Timesheets.Count().ToString());

            }
            List<Timesheet> fres = new List<Timesheet>();

            List<SvcTimeSheet.TimesheetListDataSet.TimesheetsRow> filter = res.Timesheets.
                Where(t => (t.IsWPRD_START_DATENull() || t.IsWPRD_FINISH_DATENull()) || (t.WPRD_START_DATE >= Start && t.WPRD_FINISH_DATE <= End)

                )
                                                                                                    .OrderByDescending(t => t.WPRD_START_DATE).ToList();

            if (filter.Count() > 0 && (filter.ToList()[0].WPRD_FINISH_DATE < DateTime.Today))
            {
                /*var current = res.Timesheets.OrderBy(t => t.WPRD_FINISH_DATE).
                First(t => (t.IsWPRD_START_DATENull() || t.IsWPRD_FINISH_DATENull()) || (t.WPRD_FINISH_DATE > DateTime.Today));
                if (!filter.Any(t => (t.WPRD_START_DATE == current.WPRD_START_DATE) && (t.WPRD_FINISH_DATE == current.WPRD_FINISH_DATE)))
                    filter.Add(current);*/
                filter = filter.OrderByDescending(t => t.WPRD_START_DATE).ToList();
            }
            int totalCount = filter.Count;
            foreach (var t in filter)
            {
                fres.Add(new Timesheet
                {
                    Period = totalCount.ToString() + " (" + t.WPRD_START_DATE.ToShortDateString() + " - " + t.WPRD_FINISH_DATE.ToShortDateString() + ")"
                    ,
                    Start = t.WPRD_START_DATE
                    ,
                    Stop = t.WPRD_FINISH_DATE
                    ,
                    Name = t.WPRD_NAME,
                    Id = t.WPRD_UID.ToString()
                    ,
                    Status = (t.IsTS_STATUS_ENUMNull() ? "Not created" : Enum.GetName(typeof(Microsoft.Office.Project.Server.Library.TimesheetEnum.Status), t.TS_STATUS_ENUM))
                    ,
                    Hours = !t.IsTS_GRAND_TOTAL_ACT_VALUENull() && t.TS_GRAND_TOTAL_ACT_VALUE != 0 ? Math.Round((t.TS_GRAND_TOTAL_ACT_VALUE / 60000m), 2).ToString() + "h" : @"00.00h"
                    ,
                    IsCreated = (!t.IsTS_STATUS_ENUMNull() && t.TS_STATUS_ENUM != (byte)Microsoft.Office.Project.Server.Library.TimesheetEnum.Status.PendingSubmit ? true : false)
                });
                totalCount--;
            }
            start = Start;
            end = End;
            return fres;
        }


        private object GetSessionObject(string key)
        {
            if (HttpContext.Current.Items.Contains(key))
            {
                return HttpContext.Current.Items[key];
            }

            if (HttpContext.Current.Session[key] != null)
            {
                return HttpContext.Current.Items[key];
            }

            return null;
        }

        private void CacheSessionObject(string key,object Value)
        {
            HttpContext.Current.Items[key] = Value;
            HttpContext.Current.Session[key] = Value;
        }

        private object GetApplicationObject(string key)
        {
            lock (new object())
            {
                return HttpContext.Current.Application[key];
            }
        }

        private void CacheApplicationObject(string key,Object Value)
        {
            lock (new object())
            {
                HttpContext.Current.Application[key] = Value;
            }
        }
        
        public Guid GetResourceUidFromNtAccount(String ntAccount)
        {
            string ntAccountCopy =  ntAccount;
            object cachedCopy = GetSessionObject(ntAccountCopy + "resId");
            if (cachedCopy != null)
            {
                return (Guid)cachedCopy;
            }
            SvcResource.ResourceDataSet rds = new SvcResource.ResourceDataSet();

            Microsoft.Office.Project.Server.Library.Filter filter = new Microsoft.Office.Project.Server.Library.Filter();
            filter.FilterTableName = rds.Resources.TableName;


            Microsoft.Office.Project.Server.Library.Filter.Field ntAccountField1 = new Microsoft.Office.Project.Server.Library.Filter.Field(rds.Resources.TableName, rds.Resources.WRES_ACCOUNTColumn.ColumnName);
            filter.Fields.Add(ntAccountField1);

            Microsoft.Office.Project.Server.Library.Filter.Field ntAccountField2 = new Microsoft.Office.Project.Server.Library.Filter.Field(rds.Resources.TableName, rds.Resources.RES_IS_WINDOWS_USERColumn.ColumnName);
            filter.Fields.Add(ntAccountField2);

            Microsoft.Office.Project.Server.Library.Filter.FieldOperator op = new Microsoft.Office.Project.Server.Library.Filter.FieldOperator(Microsoft.Office.Project.Server.Library.Filter.FieldOperationType.Equal,
                rds.Resources.WRES_ACCOUNTColumn.ColumnName, ntAccountCopy);
            filter.Criteria = op;



            rds = resourceClient.ReadResources(filter.GetXml(), false);

            var obj = (Guid)rds.Resources.Rows[0]["RES_UID"];
            CacheSessionObject(ntAccountCopy + "resId", obj);
            return obj;
        }

        public Guid GetTimesheetMgrUID(String ntAccount)
        {
            string ntAccountCopy =  ntAccount;
            SvcResource.ResourceDataSet rds = new SvcResource.ResourceDataSet();

            Microsoft.Office.Project.Server.Library.Filter filter = new Microsoft.Office.Project.Server.Library.Filter();
            filter.FilterTableName = rds.Resources.TableName;


            Microsoft.Office.Project.Server.Library.Filter.Field ntAccountField1 = new Microsoft.Office.Project.Server.Library.Filter.Field(rds.Resources.TableName, rds.Resources.WRES_ACCOUNTColumn.ColumnName);
            filter.Fields.Add(ntAccountField1);

            Microsoft.Office.Project.Server.Library.Filter.Field ntAccountField2 = new Microsoft.Office.Project.Server.Library.Filter.Field(rds.Resources.TableName, rds.Resources.RES_TIMESHEET_MGR_UIDColumn.ColumnName);
            filter.Fields.Add(ntAccountField2);

            Microsoft.Office.Project.Server.Library.Filter.FieldOperator op = new Microsoft.Office.Project.Server.Library.Filter.FieldOperator(Microsoft.Office.Project.Server.Library.Filter.FieldOperationType.Equal,
                rds.Resources.WRES_ACCOUNTColumn.ColumnName, ntAccountCopy);
            filter.Criteria = op;


            rds = resourceClient.ReadResources(filter.GetXml(), false);
            if (rds.Resources[0].IsRES_TIMESHEET_MGR_UIDNull())
            {
                return rds.Resources[0].RES_UID;
            }
            var obj = rds.Resources[0].RES_TIMESHEET_MGR_UID;
            return obj;
        }
        public void SetImpersonation( Guid resourceGuid)
        {
            Guid trackingGuid = Guid.NewGuid();

            HttpContext.Current.Trace.Warn("Resource Name = " + resourceClient.ReadResource(resourceGuid).Resources[0].RES_NAME);
            
            bool isWindowsUser = true;
            Guid siteId = Guid.Empty;           // Project Web App site ID.
            CultureInfo languageCulture = null; // The language culture is not used.
            CultureInfo localeCulture = null;   // The locale culture is not used.
            WCFHelpers.WcfHelpers.SetImpersonationContext(isWindowsUser,
                resourceClient.ReadResource(resourceGuid).Resources[0].RES_NAME, resourceGuid, trackingGuid, siteId,
                                               languageCulture, localeCulture);
            WCFHelpers.WcfHelpers.UseCorrectHeaders(true);
        }

        public  void UseCorrectHeaders(bool isImpersonated)
        {
            if (isImpersonated)
            {
                // Use WebOperationContext in the HTTP channel, not the OperationContext.
                WebOperationContext.Current.OutgoingRequest.Headers.Remove("PjAuth");
                WebOperationContext.Current.OutgoingRequest.Headers.Add("PjAuth", impersonationContextString);
            }


            UseWindowsAuthOnMultiAuthHeader();

        }

        public  void UseWindowsAuthOnMultiAuthHeader()
        {
            WebOperationContext.Current.OutgoingRequest.Headers.Remove(HeaderXformsKey);
            WebOperationContext.Current.OutgoingRequest.Headers.Add(HeaderXformsKey, HeaderXformsValue);
        }

       

        // Set the impersonation context for calls to the PSI on behalf of the impersonated user.
        public  void SetImpersonationContext(bool isWindowsUser, String userNTAccount,
                                                   Guid userGuid, Guid trackingGuid, Guid siteId,
                                                   CultureInfo languageCulture, CultureInfo localeCulture)
        {
            GetImpersonationContext(isWindowsUser, userNTAccount, userGuid,
                                                                  trackingGuid, siteId,
                                                                  languageCulture, localeCulture);
        }

        // Get the impersonation context.
        private  String GetImpersonationContext(bool isWindowsUser, String userNTAccount,
                                                      Guid userGuid, Guid trackingGuid, Guid siteId,
                                                      CultureInfo languageCulture, CultureInfo localeCulture)
        {
            PSLib.PSContextInfo contextInfo = new PSLib.PSContextInfo(isWindowsUser, userNTAccount, userGuid,
                                                                      trackingGuid, siteId,
                                                                      languageCulture, localeCulture);
            String contextInfoString = PSLib.PSContextInfo.SerializeToString(contextInfo);
            return contextInfoString;
        }

        // Clear the impersonation context.
        public  void ClearImpersonationContext()
        {
            impersonationContextString = string.Empty;
        }

        public SvcCustomFields.CustomFieldDataSet GetCustomFields(ViewConfigurationBase configuration)
        {
            if (configuration.CustomFields == null || configuration.CustomFields.Count <= 0)
            {
                return new SvcCustomFields.CustomFieldDataSet();
            }
            SvcCustomFields.CustomFieldDataSet cfDataSet = new SvcCustomFields.CustomFieldDataSet();
            string tableName = cfDataSet.CustomFields.TableName;
            string nameColumn =
                cfDataSet.CustomFields.MD_PROP_NAMEColumn.ColumnName;
            string uidsecndaryColumnName = cfDataSet.CustomFields.MD_PROP_UID_SECONDARYColumn.ColumnName;
            string uidColumnName = cfDataSet.CustomFields.MD_PROP_UIDColumn.ColumnName;
            string typeColumnName = cfDataSet.CustomFields.MD_PROP_TYPE_ENUMColumn.ColumnName;
            string lookuptableuidName = cfDataSet.CustomFields.MD_LOOKUP_TABLE_UIDColumn.ColumnName;
            PSLib.Filter.FieldOperationType equal =
                          PSLib.Filter.FieldOperationType.Equal;
            PSLib.Filter cfFilter = new PSLib.Filter();
            cfFilter.FilterTableName = tableName;
            cfFilter.Fields.Add(new PSLib.Filter.Field(tableName, nameColumn, PSLib.Filter.SortOrderTypeEnum.None));
            cfFilter.Fields.Add(new PSLib.Filter.Field(tableName, uidColumnName, PSLib.Filter.SortOrderTypeEnum.None));
            cfFilter.Fields.Add(new PSLib.Filter.Field(tableName, uidsecndaryColumnName, PSLib.Filter.SortOrderTypeEnum.None));
            cfFilter.Fields.Add(new PSLib.Filter.Field(tableName, typeColumnName, PSLib.Filter.SortOrderTypeEnum.None));
            cfFilter.Fields.Add(new PSLib.Filter.Field(tableName, lookuptableuidName, PSLib.Filter.SortOrderTypeEnum.None));
            List<PSLib.Filter.IOperator> operands = new List<PSLib.Filter.IOperator>();
            foreach (var configField in configuration.CustomFields)
            {
                operands.Add(new PSLib.Filter.FieldOperator(PSLib.Filter.FieldOperationType.Equal, nameColumn, configField.FullName));
            }

            cfFilter.Criteria = new PSLib.Filter.LogicalOperator(PSLib.Filter.LogicalOperationType.Or, operands.ToArray());
            return customFieldsClient.ReadCustomFields(cfFilter.GetXml(), false);

        }

        public List<BaseRow> GetRows(string user, ViewConfigurationBase configuration, string periodId, DateTime start, DateTime stop, out int status, out bool canDelete, out bool canRecall, out TimesheetHeaderInfos tInfos, out decimal[] totals)
        {

            bool iscreate = false;
            bool prepopulateForHours = false;
            Guid ruid = LoggedUser(user);
            SvcCustomFields.CustomFieldDataSet customfieldDataSet = GetCustomFields(configuration);
            List<LineClass> lineClasses = GetAllLineClassifications();
            Guid periodUID = Guid.Empty;
            if (!string.IsNullOrEmpty(periodId))
            {
                periodUID = new Guid(periodId);
            }
            Guid tuid;
            SvcTimeSheet.TimesheetDataSet timesheetDS;
            if (configuration is ViewConfigurationTask)
            {
                var tasks = GetTasks(lineClasses, customfieldDataSet, configuration, periodId, user, start, stop);
                status = -1;
                canDelete = false;
                canRecall = false;
                totals = null;
                tInfos = new TimesheetHeaderInfos();
                return tasks;
            }
            timesheetDS = GetTimesheet(user, ruid, periodUID);
            tInfos = null;
            int dayCount = Convert.ToInt32((stop.Date - start.Date).TotalDays) + 1;

            SvcTimeSheet.TimesheetDataSet tsDs;
            tInfos = GetTimesheetStatus(user, periodUID, ruid, out tuid, out tsDs);
            if (tInfos == null) status = -1;
            else status = tInfos.Status.Value;
            if (status == -1)
            {

                if (configuration is ViewConfigurationRow)
                {
                    CreateTimesheet(user, ref status, ref iscreate, ruid, periodUID, ref tuid, ref timesheetDS, ref tsDs, out canDelete, out canRecall);
                }
                else
                {
                    canRecall = false;
                    canDelete = false;

                }
            }
        prepopulate: if (status == 0 && prepopulateForHours)
            {


                using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
                {
                    SetImpersonation(GetResourceUidFromNtAccount(user)); tsDs = timesheetClient.ReadTimesheet(tuid);
                    if (tsDs.Lines.Count < 1)
                    {
                        timesheetClient.PrepareTimesheetLine(tuid, ref tsDs, new Guid[0] { });
                    }
                }
                iscreate = true;
                prepopulateForHours = true;
            }
            GetTimesheetAction(status, out canDelete, out canRecall);
            var res = new List<BaseRow>();
            decimal[] alltotalsarray = null;
            alltotalsarray = new decimal[dayCount];
            ReadTimePhasedData(user, configuration, periodId, start, stop, iscreate, customfieldDataSet, lineClasses, timesheetDS, dayCount, tsDs, res, alltotalsarray);
            totals = alltotalsarray;
            if (status == 0)
            {
                if (alltotalsarray.Sum() == 0 && status == 0 && !prepopulateForHours)
                {
                    prepopulateForHours = true;
                    goto prepopulate;
                }
            }
            return res.OrderBy(t => t.RowType).ToList();
        }
        public List<BaseRow> GetSubmittedRows(string projectId,string approver,string user, ViewConfigurationBase configuration)
        {
            if (configuration is ViewConfigurationApproval)
            {
                ActualWorkRow actual = null;
                ActualOvertimeWorkRow overtime = null;
                SingleValuesRow onlySingleValues = null;
                var tres = new List<BaseRow>();
                SvcStatusing.StatusApprovalDataSet ds = new SvcStatusing.StatusApprovalDataSet();
                bool isuser;
                var resName = GetUserName(user);
                using (OperationContextScope scope = new OperationContextScope(pwaClient.InnerChannel))
                {
                    SetImpersonation(GetResourceUidFromNtAccount(approver));
                    ds = pwaClient.ReadStatusApprovalsSubmitted(false);
                }

                var projectTasks = ds.StatusApprovals.Where(t => t.PROJ_UID.ToString() == projectId && t.RES_NAME == resName);
                var lineclassifications = GetLineClassifications();
                var customFieldDataSet = GetCustomFields(configuration);
                foreach (var row in projectTasks)
                {
                    if (configuration.NoTPData)
                    {
                        onlySingleValues = new SingleValuesRow();
                        BuildBaseRow(onlySingleValues, row);
                    }
                    else
                    {
                        if (configuration.ActualWorkA)
                        {
                            actual = new ActualWorkRow();
                            BuildBaseRow(actual, row);
                        }
                    }
                    if (actual != null) tres.Add(actual);
                    if (onlySingleValues != null) tres.Add(onlySingleValues);
                    GetAllSingleValues(lineclassifications, null, new SvcTimeSheet.TimesheetDataSet(), customFieldDataSet, user, configuration, ""
                        , DateTime.MinValue, DateTime.MaxValue, row.PROJ_UID.ToString(), row.ASSN_UID.ToString(), actual, overtime, onlySingleValues);
                }
                return tres;

            }
            else
            {
                return new List<BaseRow>();
            }
        }

        private void BuildBaseRow(BaseRow onlySingleValues, SvcStatusing.StatusApprovalDataSet.StatusApprovalsRow row)
        {
            onlySingleValues.ProjectId = row.PROJ_UID.ToString();
            onlySingleValues.ProjectName = row.PROJ_NAME;
            onlySingleValues.AssignementId = row.ASSN_UID.ToString();
            onlySingleValues.AssignementName = row.TASK_NAME;
            onlySingleValues.DayTimes = new List<decimal?>();
        }
        private void ReadTimePhasedData(string user, ViewConfigurationBase configuration, string periodId, DateTime start, DateTime stop, bool iscreate, SvcCustomFields.CustomFieldDataSet customfieldDataSet, List<LineClass> lineClasses, SvcTimeSheet.TimesheetDataSet timesheetDS, int dayCount, SvcTimeSheet.TimesheetDataSet tsDs, List<BaseRow> res, decimal[] alltotalsarray)
        {
            
            foreach (var row in tsDs.Lines)
            {
                ActualWorkRow actual = null;
                ActualOvertimeWorkRow overtime = null;
                NonBillableActualWorkRow nonbillable = null;
                NonBillableOvertimeWorkRow nonbillableovertime = null;
                AdministrativeRow admin = null;
                decimal?[] actualArray = null;
                decimal?[] overtimeArray = null;
                decimal?[] nonbillableArray = null;
                decimal?[] nonbillableovertimeArray = null;
                if (configuration.ActualWorkA)
                {

                    actual = new ActualWorkRow();
                    actualArray = new decimal?[dayCount];
                    actual.ProjectId = row.PROJ_UID.ToString();
                    actual.ProjectName = row.TS_LINE_CACHED_PROJ_NAME;
                    actual.AssignementId = row.ASSN_UID.ToString();
                    actual.AssignementName = row.TS_LINE_CACHED_ASSIGN_NAME;
                    actual.DayTimes = new List<decimal?>();
                    admin = new AdministrativeRow();
                    admin.ProjectId = actual.ProjectId;
                    admin.ProjectName = actual.ProjectName;
                    admin.AssignementId = actual.AssignementId;
                    admin.AssignementName = actual.AssignementName;
                    admin.DayTimes = new List<decimal?>();

                }
                if (configuration.ActualOvertimeWorkA)
                {
                    overtime = new ActualOvertimeWorkRow();
                    overtimeArray = new decimal?[dayCount];
                    overtime.ProjectId = row.PROJ_UID.ToString();
                    overtime.ProjectName = row.TS_LINE_CACHED_PROJ_NAME;
                    overtime.AssignementId = row.ASSN_UID.ToString();
                    overtime.AssignementName = row.TS_LINE_CACHED_ASSIGN_NAME;
                    overtime.DayTimes = new List<decimal?>();

                }
                ViewConfigurationRow configurationRow = configuration as ViewConfigurationRow;
                if (configurationRow != null && configurationRow.ActualNonBillableWorkA)
                {
                    nonbillable = new NonBillableActualWorkRow();
                    nonbillableArray = new decimal?[dayCount];
                    nonbillable.ProjectId = row.PROJ_UID.ToString();
                    nonbillable.ProjectName = row.TS_LINE_CACHED_PROJ_NAME;
                    nonbillable.AssignementId = row.ASSN_UID.ToString();
                    nonbillable.AssignementName = row.TS_LINE_CACHED_ASSIGN_NAME;
                    nonbillable.DayTimes = new List<decimal?>();

                }
                if (configurationRow != null && configurationRow.ActualNonBillableOvertimeWorkA)
                {
                    nonbillableovertime = new NonBillableOvertimeWorkRow();
                    nonbillableovertimeArray = new decimal?[dayCount];
                    nonbillableovertime.ProjectId = row.PROJ_UID.ToString();
                    nonbillableovertime.ProjectName = row.TS_LINE_CACHED_PROJ_NAME;
                    nonbillableovertime.AssignementId = row.ASSN_UID.ToString();
                    nonbillableovertime.AssignementName = row.TS_LINE_CACHED_ASSIGN_NAME;
                    nonbillableovertime.DayTimes = new List<decimal?>();

                }
                bool actualNZ = iscreate;
                bool overtimeNZ = false;
                bool nonbillableNZ = iscreate;
                bool nonbillableovertimeNZ = false;

                int i = 0;
                foreach (var actuals in row.GetActualsRows())
                {

                    if (i >= dayCount) continue;
                    if (nonbillable != null && !actuals.IsTS_ACT_NON_BILLABLE_VALUENull())
                    {
                        nonbillableArray[i] = actuals.TS_ACT_NON_BILLABLE_VALUE / 60000m;
                        if (nonbillableArray[i].Value != 0m) nonbillableNZ = true;
                    }
                    if (nonbillableovertime != null && !actuals.IsTS_ACT_NON_BILLABLE_OVT_VALUENull())
                    {
                        nonbillableovertimeArray[i] = actuals.TS_ACT_NON_BILLABLE_OVT_VALUE / 60000m;
                        if (nonbillableovertimeArray[i].Value != 0m) nonbillableovertimeNZ = true;
                    }
                    if (actual != null && !actuals.IsTS_ACT_VALUENull())
                    {
                        actualArray[i] = actuals.TS_ACT_VALUE / 60000m;
                        alltotalsarray[i] += actualArray[i].Value;
                        if (actualArray[i].Value != 0m) actualNZ = true;
                    }
                    if (overtime != null && !actuals.IsTS_ACT_OVT_VALUENull())
                    {
                        overtimeArray[i] = actuals.TS_ACT_OVT_VALUE / 60000m;
                        alltotalsarray[i] += overtimeArray[i].Value;
                        if (overtimeArray[i].Value != 0m) overtimeNZ = true;
                    }
                    i++;
                }

                if (actual != null && ((configuration is ViewConfigurationTask) || actualNZ)) actual.DayTimes = actualArray.ToList();
                else actual = null;
                if (overtime != null && ((configuration is ViewConfigurationTask) || overtimeNZ)) overtime.DayTimes = overtimeArray.ToList();
                else overtime = null;
                if (nonbillable != null && ((configuration is ViewConfigurationTask) || nonbillableNZ)) nonbillable.DayTimes = nonbillableArray.ToList();
                else nonbillable = null;
                if (nonbillableovertime != null && ((configuration is ViewConfigurationTask) || nonbillableovertimeNZ)) nonbillableovertime.DayTimes = nonbillableovertimeArray.ToList();
                else nonbillableovertime = null;
                bool result = false;
                if ((configuration is ViewConfigurationTask) || actual != null || overtime != null || nonbillable != null || nonbillableovertime != null)
                    result = GetAllSingleValues(lineClasses, null, timesheetDS, customfieldDataSet, user, configuration, periodId, start, stop, row.PROJ_UID.ToString()
                        , row.ASSN_UID.ToString(), actual, overtime);
                if (actual != null)
                {
                    if (result || actual.AssignementName == "Top Level") res.Add(actual);
                    else
                    {
                        admin.DayTimes = actual.DayTimes;
                        admin.LineClass = actual.LineClass;
                        res.Add(admin);
                    }
                }
                if (overtime != null && result) res.Add(overtime);
                if (nonbillable != null && result) res.Add(nonbillable);
                if (nonbillableovertime != null && result) res.Add(nonbillableovertime);


            }
        }

        private void CreateTimesheet(string user, ref int status, ref bool iscreate, Guid ruid, Guid periodUID, ref Guid tuid, ref SvcTimeSheet.TimesheetDataSet timesheetDS, ref SvcTimeSheet.TimesheetDataSet tsDs, out bool canDelete, out bool canRecall)
        {
            tsDs = new SvcTimeSheet.TimesheetDataSet();
            SvcTimeSheet.TimesheetDataSet.HeadersRow headersRow = tsDs.Headers.NewHeadersRow();
            headersRow.RES_UID = ruid;  // cant be null.
            tuid = Guid.NewGuid();
            headersRow.TS_UID = tuid;
            headersRow.WPRD_UID = periodUID;
            headersRow.TS_NAME = BusisnessResources.InitTimesheetName;
            headersRow.TS_COMMENTS = BusisnessResources.InitTimesheetComment;
            headersRow.TS_ENTRY_MODE_ENUM = (byte)PSLib.TimesheetEnum.EntryMode.Daily;
            tsDs.Headers.AddHeadersRow(headersRow);
            status = 0;

            using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
            {
                SetImpersonation(GetResourceUidFromNtAccount(user));
                timesheetClient.CreateTimesheet(tsDs, SvcTimeSheet.PreloadType.Default);
            }
            iscreate = true;
            using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
            {
                SetImpersonation(GetResourceUidFromNtAccount(user));
                tsDs = timesheetClient.ReadTimesheet(tuid); //calling ReadTimesheet to pre populate with default server settings
                timesheetDS = tsDs;
            }
            GetTimesheetAction(status, out canDelete, out canRecall);
        }

        private SvcTimeSheet.TimesheetDataSet GetTimesheet(string user, Guid ruid, Guid periodUID)
        {
            using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
            {
                SetImpersonation(GetResourceUidFromNtAccount(user));
                return timesheetClient.ReadTimesheetByPeriod(ruid, periodUID, SvcTimeSheet.Navigation.Current);
            }
        }

        private List<BaseRow> GetTasks(List<LineClass> lineClasses
            , SvcCustomFields.CustomFieldDataSet customDataSet, ViewConfigurationBase configuration, string periodId, string user, DateTime start, DateTime stop)
        {
            ActualWorkRow actual = null;
            ActualOvertimeWorkRow overtime = null;
            SingleValuesRow onlySingleValues = null;
            var tres = new List<BaseRow>();

            /// Reading Assignements //////
            /// 
            bool isWindowsUser;
            var resUid = GetResourceUidFromNtAccount(user);
            SvcStatusing.StatusingDataSet ds = GetAssignments(user, start, stop);

            foreach (var row in ds.Assignments)
            {
                if (configuration.NoTPData)
                {
                    onlySingleValues = new SingleValuesRow();
                    BuildBaseRow(onlySingleValues, row);
                }
                else
                {
                    if (configuration.ActualWorkA)
                    {
                        actual = new ActualWorkRow();
                        BuildBaseRow(actual, row);
                    }
                }
                if (actual != null) tres.Add(actual);
                if (onlySingleValues != null) tres.Add(onlySingleValues);
                GetAllSingleValues(lineClasses, null, new SvcTimeSheet.TimesheetDataSet(), customDataSet, user, configuration, periodId
                    , start, stop, row.PROJ_UID.ToString(), row.ASSN_UID.ToString(), actual, overtime, onlySingleValues);
            }
            return tres;
        }

        private void BuildBaseRow(BaseRow onlySingleValues, SvcStatusing.StatusingDataSet.AssignmentsRow row)
        {
            onlySingleValues.ProjectId = row.PROJ_UID.ToString();
            onlySingleValues.ProjectName = row.PROJ_NAME;
            onlySingleValues.AssignementId = row.ASSN_UID.ToString();
            onlySingleValues.AssignementName = row.TASK_NAME;
            onlySingleValues.DayTimes = new List<decimal?>();
        }

        private SvcStatusing.StatusingDataSet GetAssignments(string user, DateTime start, DateTime stop)
        {
            using (OperationContextScope scope = new OperationContextScope(pwaClient.InnerChannel))
            {
                SetImpersonation(GetResourceUidFromNtAccount(user));
                return pwaClient.ReadStatus(Guid.Empty, start, stop);
            }
        }


        public string GetPeriodID(DateTime start, DateTime end)
        {
            string periodID = "";


            periodID = adminClient.ReadPeriods(SvcAdmin.PeriodState.All).TimePeriods.Single(t => t.WPRD_START_DATE.Date == start.Date && t.WPRD_FINISH_DATE.Date == end.Date).WPRD_UID.ToString();
            return periodID;
        }
        public List<Timesheet> GetTimesheets(string user, string periodId, DateTime start, DateTime stop)
        {

            Guid ruid = LoggedUser(user);
            Guid periodUID = new Guid(periodId);

            int dayCount = Convert.ToInt32((stop.Date - start.Date).TotalDays) + 1;

            SvcTimeSheet.TimesheetListDataSet tsDs;
            tsDs = GetTimesheetStatus(user, ruid, start, stop, (int)Microsoft.Office.Project.Server.Library.TimesheetEnum.ListSelect.AllPeriods);

            var res = new List<Timesheet>();
            foreach (var ts in tsDs.Timesheets.Rows)
            {
                SvcTimeSheet.TimesheetListDataSet.TimesheetsRow row = ts as SvcTimeSheet.TimesheetListDataSet.TimesheetsRow;
                Timesheet timeSheet = new Timesheet() { Id = row.TS_UID.ToString(), Name = row.TS_NAME, Start = row.WPRD_START_DATE, Stop = row.WPRD_FINISH_DATE };
                res.Add(timeSheet);
            }

            return res;

        }

        public BaseRow GetRowSingleValues(string user, ViewConfigurationBase configuration, string periodId, DateTime start, DateTime stop, string projectId, string assignementId, string assignmentName, string lineClassID, Type RowType)
        {

            BaseRow res = null;
            var ruid = LoggedUser(user);
            var lineClasses = GetAllLineClassifications();
            SvcTimeSheet.TimesheetDataSet timesheetDS = new SvcTimeSheet.TimesheetDataSet();
            Guid periodUID;
            if (Guid.TryParse(periodId, out periodUID))
            {
                using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
                {
                    SetImpersonation(GetResourceUidFromNtAccount(user));
                    timesheetDS = timesheetClient.ReadTimesheetByPeriod(ruid, periodUID, SvcTimeSheet.Navigation.Current);
                }
            }

            SvcCustomFields.CustomFieldDataSet customFieldDataSet = GetCustomFields(configuration);
            if (RowType == typeof(ActualWorkRow))
            {
                res = new ActualWorkRow();
                GetAllSingleValues(lineClasses, lineClassID, timesheetDS, customFieldDataSet, user, configuration, periodId, start, stop, projectId, assignementId, res as ActualWorkRow, null);
            }
            else if (RowType == typeof(ActualOvertimeWorkRow))
            {
                res = new ActualOvertimeWorkRow();
                GetAllSingleValues(lineClasses, lineClassID, timesheetDS, customFieldDataSet, user, configuration, periodId, start, stop, projectId, assignementId, null, res as ActualOvertimeWorkRow);
            }
            else if (RowType == typeof(SingleValuesRow))
            {
                res = new SingleValuesRow();
                GetAllSingleValues(lineClasses, lineClassID, timesheetDS, customFieldDataSet, user, configuration, periodId, start, stop, projectId, assignementId, null, null);
            }
            else if (RowType == typeof(AdministrativeRow))
            {
                res = new AdministrativeRow();
                res.LineClass = GetAllLineClassifications().First(t => t.Name == assignmentName);
            }
            else if (RowType == typeof(NonBillableActualWorkRow))
            {

                res = new NonBillableActualWorkRow();
                res.LineClass = GetAllLineClassifications().First(t => t.Name == assignmentName);
            }
            else
            {
                res = new NonBillableOvertimeWorkRow();
                res.LineClass = GetAllLineClassifications().First(t => t.Name == assignmentName);
            }
            if (res != null)
            {
                res.AssignementId = assignementId;
                res.ProjectId = projectId;
            }
            return res;

        }
        public void UpdateRows(bool isApprovalMode, string user, ViewConfigurationBase configuration, string periodId, DateTime start, DateTime stop, IEnumerable<Tracker<BaseRow>> rows, bool submit)
        {
            var customfields = GetCustomFields(configuration);
            if (rows == null) return;

            var crows = rows.GroupBy(m => m.Value.LineClass == null ? m.Value.AssignementId : m.Value.AssignementId + "_" + m.Value.LineClass.Id);
            Guid ruid = LoggedUser(user);
            IDictionary<string, WholeLine> dict =
                new Dictionary<string, WholeLine>();
            List<WholeLine> list =
                new List<WholeLine>();


            bool noChange = BuildWholeLineGroups(crows, dict, list);
            SvcResource.ResourceAssignmentDataSet _resAssDS = null;
            SvcStatusing.StatusingClient statusingClient = pwaClient;
            if (!noChange)
            {
                _resAssDS = UpdateStatus(user, configuration, start, stop, customfields, list, _resAssDS, statusingClient);
            }
            if (submit)
            {
                SendStatus(user, list, statusingClient);
            }
            ///dataset processing
            if (!string.IsNullOrEmpty(periodId))
            {
                Guid periodUID = new Guid(periodId);
                Guid tuid;
                if (configuration is ViewConfigurationRow && ((!noChange) || submit))
                {

                    SvcTimeSheet.TimesheetDataSet tsDs;
                    TimesheetHeaderInfos tInfos = GetTimesheetStatus(user, periodUID, ruid, out tuid, out tsDs);
                    if (tInfos == null) return;
                    int status = tInfos.Status.Value;

                    if (noChange)
                    {
                        SubmitTimesheet(user, tsDs);
                    }
                    else
                    {
                        ProcessGroups(user, configuration, start, stop, dict, list, ref _resAssDS, ref tsDs);
                        List<SvcTimeSheet.TimesheetDataSet.LinesRow> rowsToDelete = new List<SvcTimeSheet.TimesheetDataSet.LinesRow>();
                        ProcessTimeLines(configuration, start, stop, dict, tsDs, rowsToDelete);
                        var tsGuid = (Guid)(tsDs.Headers[0].TS_UID);
                        if (isApprovalMode)
                        {
                            SaveTimesheetForUser(user, tsDs, tsGuid);
                        }
                        else
                        {
                            SaveTimesheet(user, tsDs, tsGuid);
                        }
                        if (submit)
                        {
                            SubmitTimesheet(user, tsDs);
                        }


                    }
                }
            }

            /////everything ok...confirmchanges//////
            foreach (Tracker<BaseRow> tracker in rows)
            {
                tracker.Confirm();
            }
        }


        private void SaveTimesheetForUser(string user, SvcTimeSheet.TimesheetDataSet tsDs, Guid tsGuid)
        {
            try
            {
                Guid jobGuid = Guid.NewGuid();
                using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
                {
                    SetImpersonation(resourceClient.ReadResource(tsDs.Headers[0].RES_TIMESHEET_MGR_UID).Resources[0].RES_UID);
                    var temp = tsDs.GetChanges();
                    timesheetClient.QueueUpdateTimesheet(jobGuid,
                         tsGuid,
                        (SvcTimeSheet.TimesheetDataSet)tsDs.GetChanges());  //Saves the specified timesheet data to the Published database
                }
                bool res = QueueHelper.WaitForQueueJobCompletion(this, jobGuid, (int)SvcQueueSystem.QueueMsgType.TimesheetUpdate, queueClient);
                if (!res) throw new TimesheetUpdateException();
            }
            catch (TimesheetUpdateException tex) { throw new TimesheetUpdateException(); }
        }
        private void SaveTimesheet(string user, SvcTimeSheet.TimesheetDataSet tsDs, Guid tsGuid)
        {
            
            try
            {
                Guid jobGuid = Guid.NewGuid();
                HttpContext.Current.Trace.Warn("User = " + user);
                HttpContext.Current.Trace.Warn("tsGuid = " + tsGuid.ToString());

                using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
                {
                    SetImpersonation(GetResourceUidFromNtAccount(user));
                    var temp = tsDs.GetChanges();
                    timesheetClient.QueueUpdateTimesheet(jobGuid,
                         tsGuid,
                        (SvcTimeSheet.TimesheetDataSet)tsDs);  //Saves the specified timesheet data to the Published database
                }
                bool res = QueueHelper.WaitForQueueJobCompletion(this, jobGuid, (int)SvcQueueSystem.QueueMsgType.TimesheetUpdate, queueClient);
                if (!res) throw new TimesheetUpdateException();
            }
            catch(TimesheetUpdateException tex) { throw new TimesheetUpdateException(); }
        }

        private void ProcessTimeLines(ViewConfigurationBase configuration, DateTime start, DateTime stop, IDictionary<string, WholeLine> dict, SvcTimeSheet.TimesheetDataSet tsDs, List<SvcTimeSheet.TimesheetDataSet.LinesRow> rowsToDelete)
        {
            //Microsoft.Office.Project.Server.Library.TimesheetEnum.Status
            foreach (var row in tsDs.Lines)
            {
                string assignementId = row.ASSN_UID != null ? row.ASSN_UID.ToString() + "_" + row.TS_LINE_CLASS_UID.ToString() : row.TS_LINE_CLASS_UID.ToString();  //John; this line sets the assignmentUID?
                WholeLine group = null;
                bool res = dict.TryGetValue(assignementId, out group);
                if (!res) continue;
                copyToRow(group, tsDs, null, row, configuration, start, stop, assignementId);
                group.Processed = true;
                var cActuals = row.GetActualsRows();
                bool canDelete = true;
                if (cActuals != null)
                {

                    foreach (var act in cActuals)
                    {
                        if ((!act.IsTS_ACT_VALUENull() && act.TS_ACT_VALUE != 0m) ||
                            (!act.IsTS_ACT_PLAN_VALUENull() && act.TS_ACT_PLAN_VALUE != 0m) ||
                            (!act.IsTS_ACT_OVT_VALUENull() && act.TS_ACT_OVT_VALUE != 0m) ||
                            (!act.IsTS_ACT_NON_BILLABLE_VALUENull() && act.TS_ACT_NON_BILLABLE_VALUE != 0m) ||
                            (!act.IsTS_ACT_NON_BILLABLE_OVT_VALUENull() && act.TS_ACT_NON_BILLABLE_OVT_VALUE != 0m)
                            ) canDelete = false;
                    }
                }
                if (canDelete) rowsToDelete.Add(row);

            }
            foreach (var row in rowsToDelete)
            {
                var actuals = row.GetActualsRows();
                foreach (var ac in actuals)
                {
                    ac.Delete();
                }
                row.Delete();

            }
        }

        private void ProcessGroups(string user, ViewConfigurationBase configuration, DateTime start, DateTime stop, IDictionary<string, WholeLine> dict, List<WholeLine> list, ref SvcResource.ResourceAssignmentDataSet _resAssDS, ref SvcTimeSheet.TimesheetDataSet tsDs)
        {
            foreach (var row in tsDs.Lines)
            {
                string assignementId = row.ASSN_UID.ToString();

                string lineClassID = row.TS_LINE_CLASS_UID.ToString();
                WholeLine group = null;
                bool res = dict.TryGetValue(assignementId + "_" + lineClassID, out group);
                if (!res) continue;
                group.ProjectId = row.PROJ_UID.ToString();
                group.Processed = true;
            }

            foreach (var group in list)
            {

                if (group.Processed || !group.Changed) continue;
                if (_resAssDS == null) _resAssDS = GetResourceAssignmentDataSet(user);
                createRow(user, group, ref tsDs, _resAssDS, null, configuration, start, stop, group.Key.Split("_".ToCharArray())[0], group.ProjectId, group.ProjectName);
                group.Processed = true;
            }
        }

        private void SubmitTimesheet(string user, SvcTimeSheet.TimesheetDataSet tsDs)
        {
            try
            {
                Guid jobGuid = Guid.NewGuid();
                var tsGuid = (Guid)(tsDs.Headers[0].TS_UID);
                using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
                {
                    SetImpersonation(GetResourceUidFromNtAccount(user));
                    timesheetClient.QueueSubmitTimesheet(jobGuid, tsGuid, GetTimesheetMgrUID(user), BusisnessResources.ApprovalComment);
                }
                bool res = QueueHelper.WaitForQueueJobCompletion(this, jobGuid, (int)SvcQueueSystem.QueueMsgType.TimesheetSubmit, queueClient);
                if (!res) throw new TimesheetSubmitException();
            }
            catch { throw new TimesheetSubmitException(); }
        }

        private void SendStatus(string user, List<WholeLine> list, SvcStatusing.StatusingClient statusingClient)
        {
            List<Guid> changedAssignements = new List<Guid>();
            foreach (var g in list)
            {
                if (g.Actuals != null && g.Actuals.Count > 0 && g.Actuals[0].Values != null &&
                    (g.Actuals[0].Values.Value is AdministrativeRow || g.Actuals[0].Values.OldValue is AdministrativeRow)) continue;
                changedAssignements.Add(new Guid(g.Key.Split("_".ToCharArray())[0]));
            }
            try
            {
                bool isWindowsUser;
                var resID = GetResourceUidFromNtAccount(user);
                if (changedAssignements.Count > 0)
                {
                    using (OperationContextScope scope = new OperationContextScope(statusingClient.InnerChannel))
                    {
                        SetImpersonation(GetResourceUidFromNtAccount(user));
                        statusingClient.SubmitStatus(changedAssignements.ToArray(), BusisnessResources.StausApprovalComment);
                    }
                }
            }
            catch { throw new StatusSubmitException(); }
        }

        private SvcResource.ResourceAssignmentDataSet UpdateStatus(string user, ViewConfigurationBase configuration, DateTime start, DateTime stop, SvcCustomFields.CustomFieldDataSet customfields, List<WholeLine> list, SvcResource.ResourceAssignmentDataSet _resAssDS, SvcStatusing.StatusingClient statusingClient)
        {
            try
            {

                if (_resAssDS == null) _resAssDS = GetResourceAssignmentDataSet(user);
                // weed out top level tasks since they are not intended for statusing
                var statuslist = list.Where(t => (t.IsTopLevelTask == false) && (t.Changed == true)).ToList();
                statuslist = statuslist.Where(t => (_resAssDS.ResourceAssignment.Any(s => s.ASSN_UID == new Guid(t.Key.Split("_".ToCharArray())[0])))).ToList();
                  string xml = new ChangeXml(_resAssDS, statuslist, configuration, start, stop, this).Get(customfields);
                using (OperationContextScope scope = new OperationContextScope(statusingClient.InnerChannel))
                {
                    SetImpersonation(GetResourceUidFromNtAccount(user));
                    if (xml != null) statusingClient.UpdateStatus(xml);
                }
            }
            catch(StatusUpdateException sex) { throw new StatusUpdateException(); }
            return _resAssDS;
        }

        private static bool BuildWholeLineGroups(IEnumerable<IGrouping<string, Tracker<BaseRow>>> crows, IDictionary<string, WholeLine> dict, List<WholeLine> list)
        {
            foreach (var group in crows)
            {
                WholeLine wLine = new WholeLine(group);
                wLine.ProjectId = group.First().Value.ProjectId;
                wLine.ProjectName = group.First().Value.ProjectName;
                if (group.Key != null)
                {
                    dict.Add(group.Key, wLine);
                }
                list.Add(wLine);
            }
            bool noChange = true;
            foreach (var x in list)
            {
                if (x.Changed) noChange = false;
            }
            return noChange;
        }
        public void RecallDelete(string user, string periodId, DateTime start, DateTime stop, bool isRecall)
        {

            Guid ruid = LoggedUser(user);
            Guid periodUID = new Guid(periodId);
            Guid tuid;
            SvcTimeSheet.TimesheetDataSet tsDs;
            TimesheetHeaderInfos tInfos = GetTimesheetStatus(user, periodUID, ruid, out tuid, out tsDs);
            int status = -1;
            if (tInfos != null) status = tInfos.Status.Value;
            bool canDelete;
            bool canRecall;
            GetTimesheetAction(status, out canDelete, out canRecall);
            if (isRecall && canRecall)
            {
                try
                {
                    Guid jobUID = Guid.NewGuid();
                    using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
                    {
                        SetImpersonation(GetResourceUidFromNtAccount(user));
                        timesheetClient.QueueRecallTimesheet(jobUID, tuid);
                    }
                    bool res = QueueHelper.WaitForQueueJobCompletion(this, jobUID, (int)SvcQueueSystem.QueueMsgType.TimesheetRecall, queueClient);


                }
                catch
                {

                }
            }
            else if ((!isRecall) && canDelete)
            {
                try
                {
                    Guid jobUID = Guid.NewGuid();
                    using (OperationContextScope scope = new OperationContextScope(timesheetClient.InnerChannel))
                    {
                        SetImpersonation(GetResourceUidFromNtAccount(user));
                        timesheetClient.QueueDeleteTimesheet(jobUID, tuid);
                    }
                    bool res = QueueHelper.WaitForQueueJobCompletion(this, jobUID, (int)SvcQueueSystem.QueueMsgType.TimesheetDelete, queueClient);
                }
                catch
                {

                }
            }
        }

        protected void GetTimesheetAction(int status, out bool canDelete, out bool canRecall)
        {
            canDelete = (status == 0) || (status == 4) || (status == 2);
            canRecall = (status == 1) || (status == 3) || (status == 2);

        }

        public CustomFieldInfo GetCustomFieldType(Guid id, int type, string property)
        {


            //var customfied = customfields.First(m => m.MD_PROP_NAME == property);
            CustomFieldInfo cfinfo = new CustomFieldInfo() { Guid = id.ToString(), Name = property };
            switch (type)
            {
                case 4: cfinfo.DataType = "Date";
                    break;
                case 9: cfinfo.DataType = "Cost";
                    break;
                case 6: cfinfo.DataType = "Duration";
                    break;
                case 27: cfinfo.DataType = "Finishdate";
                    break;
                case 17: cfinfo.DataType = "Flag";
                    break;
                case 15: cfinfo.DataType = "Number";
                    break;
                case 21: cfinfo.DataType = "Text";
                    break;
            }
            return cfinfo;


        }

        // Set the PSI client endpoints programmatically; don't use app.config.
        public bool SetClientEndpointsProg(string pwaUrl)
        {
            const int MAXSIZE = int.MaxValue;
            const string SVC_ROUTER = "/_vti_bin/PSI/ProjectServer.svc";

            bool isHttps = pwaUrl.ToLower().StartsWith("https");
            bool result = true;
            BasicHttpBinding binding = null;

            try
            {
                if (isHttps)
                {
                    // Create a binding for HTTPS.TimesheetL
                    binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
                }
                else
                {
                    // Create a binding for HTTP.
                    binding = new BasicHttpBinding(BasicHttpSecurityMode.TransportCredentialOnly);
                }

                binding.Name = "basicHttpConf";
                binding.MessageEncoding = WSMessageEncoding.Text;

                binding.CloseTimeout = new TimeSpan(00, 05, 00);
                binding.OpenTimeout = new TimeSpan(00, 05, 00);
                binding.ReceiveTimeout = new TimeSpan(00, 05, 00);
                binding.SendTimeout = new TimeSpan(00, 05, 00);
                binding.TextEncoding = System.Text.Encoding.UTF8;

                // If the TransferMode is buffered, the MaxBufferSize and 
                // MaxReceived MessageSize must be the same value.
                binding.TransferMode = TransferMode.Buffered;
                binding.MaxBufferSize = MAXSIZE;
                binding.MaxReceivedMessageSize = MAXSIZE;
                binding.MaxBufferPoolSize = MAXSIZE;


                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
                binding.GetType().GetProperty("ReaderQuotas").SetValue(binding, XmlDictionaryReaderQuotas.Max, null);
                // The endpoint address is the ProjectServer.svc router for all public PSI calls.
                EndpointAddress address = new EndpointAddress(pwaUrl + SVC_ROUTER);



                adminClient = new SvcAdmin.AdminClient(binding, address);
                adminClient.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel
                    = TokenImpersonationLevel.Impersonation;
                adminClient.ChannelFactory.Credentials.Windows.AllowNtlm = true;


                projectClient = new SvcProject.ProjectClient(binding, address);
                projectClient.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel
                    = TokenImpersonationLevel.Impersonation;
                projectClient.ChannelFactory.Credentials.Windows.AllowNtlm = true;

                queueSystemClient = new SvcQueueSystem.QueueSystemClient(binding, address);
                queueSystemClient.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel
                    = TokenImpersonationLevel.Impersonation;
                queueSystemClient.ChannelFactory.Credentials.Windows.AllowNtlm = true;

                resourceClient = new SvcResource.ResourceClient(binding, address);
                resourceClient.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel
                    = TokenImpersonationLevel.Impersonation;
                resourceClient.ChannelFactory.Credentials.Windows.AllowNtlm = true;

                lookupTableClient = new SvcLookupTable.LookupTableClient(binding, address);
                lookupTableClient.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel
                    = TokenImpersonationLevel.Impersonation;
                lookupTableClient.ChannelFactory.Credentials.Windows.AllowNtlm = true;


                customFieldsClient = new SvcCustomFields.CustomFieldsClient(binding, address);
                customFieldsClient.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel
                    = TokenImpersonationLevel.Impersonation;
                customFieldsClient.ChannelFactory.Credentials.Windows.AllowNtlm = true;

                calendarClient = new SvcCalendar.CalendarClient(binding, address);
                calendarClient.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel
                    = TokenImpersonationLevel.Impersonation;
                calendarClient.ChannelFactory.Credentials.Windows.AllowNtlm = true;

                archiveClient = new SvcArchive.ArchiveClient(binding, address);
                archiveClient.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel
                    = TokenImpersonationLevel.Impersonation;
                archiveClient.ChannelFactory.Credentials.Windows.AllowNtlm = true;

                pwaClient = new SvcStatusing.StatusingClient(binding, address);
                pwaClient.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel
                    = TokenImpersonationLevel.Impersonation;
                pwaClient.ChannelFactory.Credentials.Windows.AllowNtlm = true;

                timesheetClient = new SvcTimeSheet.TimeSheetClient(binding, address);
                timesheetClient.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel
                    = TokenImpersonationLevel.Impersonation;
                timesheetClient.ChannelFactory.Credentials.Windows.AllowNtlm = true;

                queueClient = new SvcQueueSystem.QueueSystemClient(binding, address);
                queueClient.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel
                    = TokenImpersonationLevel.Impersonation;
                queueClient.ChannelFactory.Credentials.Windows.AllowNtlm = true;
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }


    }
}
