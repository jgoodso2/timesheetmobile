using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeSheetIBusiness;
using System.Security.Principal;
using MVCControlsToolkit.Controller;
using System.Data;
using SvcProject;
using PSLib = Microsoft.Office.Project.Server.Library;

namespace TimeSheetBusiness
{
    
    public class Repository: IRepository
    {
        private static string bindingConfiguration = "basicHttpConf";
        public UserConfigurationInfo UserConfiguration(WindowsIdentity user, string rowField, string taskField)
        {
            string adress = ViewConfigurationBase.BaseUrl;
            using (WindowsImpersonationContext impersonatedUser = user.Impersonate())
            {
                Guid defaultTimesheetViewUID = ViewConfigurationRow.ViewFieldGuid;
                Guid defaultStatusViewUID = ViewConfigurationTask.ViewFieldGuid;
                if ((!string.IsNullOrWhiteSpace(rowField) && (defaultTimesheetViewUID == null || defaultTimesheetViewUID == Guid.Empty) ) ||
                    (!string.IsNullOrWhiteSpace(taskField) && (defaultStatusViewUID == null || defaultStatusViewUID == Guid.Empty)))
                {
                    //this code gets the name of default views stored on the server.
                    //get the list of custom fields first
                    SvcCustomFields.CustomFieldDataSet cds = new SvcCustomFields.CustomFieldDataSet();
                    SvcCustomFields.CustomFieldsClient custFieldsClient = new SvcCustomFields.CustomFieldsClient("basicHttp_CustomFields", adress);
                    /*I dont think we need a filter, but if we did, this is a good example
                     * http://www.epmfaq.com/ssanderlin/project-server-2007/retrieve-the-guid-of-a-custom-field-using-its-name  */
                    cds = custFieldsClient.ReadCustomFields(string.Empty, false);

                    if (!string.IsNullOrWhiteSpace(rowField))
                    {
                        SvcCustomFields.CustomFieldDataSet.CustomFieldsRow[] timesheetviewrow;
                        timesheetviewrow = (SvcCustomFields.CustomFieldDataSet.CustomFieldsRow[])cds.CustomFields.Select("MD_PROP_NAME = '" + rowField + "'");
                        ViewConfigurationRow.ViewFieldGuid = defaultTimesheetViewUID = timesheetviewrow[0].MD_PROP_UID;
                    }
                    if (!string.IsNullOrWhiteSpace(taskField))
                    {
                        SvcCustomFields.CustomFieldDataSet.CustomFieldsRow[] statusviewrow;
                        statusviewrow = (SvcCustomFields.CustomFieldDataSet.CustomFieldsRow[])cds.CustomFields.Select("MD_PROP_NAME = '" + taskField + "'");
                        ViewConfigurationTask.ViewFieldGuid = defaultStatusViewUID = statusviewrow[0].MD_PROP_UID;
                    }

                }
                string defaultTimesheetView = string.Empty;
                string defaultStatusView = string.Empty;
                if ((defaultTimesheetViewUID != null && defaultTimesheetViewUID != Guid.Empty) || (defaultStatusViewUID != null && defaultStatusViewUID != Guid.Empty))
                {
                    //now read the values of the custom fields.
                    SvcResource.ResourceDataSet rds = new SvcResource.ResourceDataSet();
                    SvcResource.ResourceClient resClient = new SvcResource.ResourceClient("basicHttp_Resource", adress);
                    Guid resUID = resClient.GetCurrentUserUid();
                    rds = resClient.ReadResource(resUID);
                    if (defaultTimesheetViewUID != null && defaultTimesheetViewUID != Guid.Empty)
                    {
                        SvcResource.ResourceDataSet.ResourceCustomFieldsRow[] tsViewFieldsRow =
                            (SvcResource.ResourceDataSet.ResourceCustomFieldsRow[])rds.ResourceCustomFields.Select("MD_PROP_UID = '" + defaultTimesheetViewUID + "'");
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


                }
                return new UserConfigurationInfo { TaskViewId = defaultStatusView, RowViewId = defaultTimesheetView };
            }
        }
       
        public void ChangeUserConfiguration(WindowsIdentity user, UserConfigurationInfo conf, string rowField, string taskField)
        {
            string adress = ViewConfigurationBase.BaseUrl;
            using (WindowsImpersonationContext impersonatedUser = user.Impersonate())
            {
                Guid defaultTimesheetViewUID = ViewConfigurationRow.ViewFieldGuid;
                Guid defaultStatusViewUID = ViewConfigurationTask.ViewFieldGuid;
                if ((!string.IsNullOrWhiteSpace(rowField) && (defaultTimesheetViewUID == null || defaultTimesheetViewUID == Guid.Empty) ) ||
                    (!string.IsNullOrWhiteSpace(taskField) && (defaultStatusViewUID == null || defaultStatusViewUID == Guid.Empty)))
                {
                    //this code gets the name of default views stored on the server.
                    //get the list of custom fields first
                    SvcCustomFields.CustomFieldDataSet cds = new SvcCustomFields.CustomFieldDataSet();
                    SvcCustomFields.CustomFieldsClient custFieldsClient = new SvcCustomFields.CustomFieldsClient("basicHttp_CustomFields", adress);
                    /*I dont think we need a filter, but if we did, this is a good example
                     * http://www.epmfaq.com/ssanderlin/project-server-2007/retrieve-the-guid-of-a-custom-field-using-its-name  */
                    cds = custFieldsClient.ReadCustomFields(string.Empty, false);

                    if (!string.IsNullOrWhiteSpace(rowField))
                    {
                        SvcCustomFields.CustomFieldDataSet.CustomFieldsRow[] timesheetviewrow;
                        timesheetviewrow = (SvcCustomFields.CustomFieldDataSet.CustomFieldsRow[])cds.CustomFields.Select("MD_PROP_NAME = '" + rowField + "'");
                        ViewConfigurationRow.ViewFieldGuid = defaultTimesheetViewUID = timesheetviewrow[0].MD_PROP_UID;
                    }
                    if (!string.IsNullOrWhiteSpace(taskField))
                    {
                        SvcCustomFields.CustomFieldDataSet.CustomFieldsRow[] statusviewrow;
                        statusviewrow = (SvcCustomFields.CustomFieldDataSet.CustomFieldsRow[])cds.CustomFields.Select("MD_PROP_NAME = '" + taskField + "'");
                        ViewConfigurationTask.ViewFieldGuid = defaultStatusViewUID = statusviewrow[0].MD_PROP_UID;
                    }

                }
                if ((defaultTimesheetViewUID != null && defaultTimesheetViewUID != Guid.Empty) || (defaultStatusViewUID != null && defaultStatusViewUID != Guid.Empty))
                {
                    ///////////////////
                    //now read the values of the custom fields.

                    SvcResource.ResourceDataSet rds = new SvcResource.ResourceDataSet();
                    SvcResource.ResourceClient resClient = new SvcResource.ResourceClient("basicHttp_Resource", adress);
                    Guid resUID = resClient.GetCurrentUserUid();
                    rds = resClient.ReadResource(resUID);
                    try
                    {

                        SvcResource.ResourceDataSet.ResourcesRow row = rds.Resources[0];

                        if (row.IsNull("RES_CHECKOUTBY"))  //if true, the resource can be modified
                        {
                            if (defaultTimesheetViewUID != null && defaultTimesheetViewUID != Guid.Empty)
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
                            Guid[] resourcestoCheckout = new Guid[1];
                            resourcestoCheckout[0] = resUID;
                            resClient.CheckOutResources(resourcestoCheckout);
                            resClient.UpdateResources(rds, false, true);
                        }
                    }
                    catch
                    {
                    }
                    ////////////////////

                }

            }

        }
        protected Guid LoggedUser()
        {
            string adress=ViewConfigurationBase.BaseUrl;
            SvcResource.ResourceClient rsClient = new SvcResource.ResourceClient("basicHttp_Resource", adress);
            Guid resUID = rsClient.GetCurrentUserUid();
            return resUID;
        }
        private SvcResource.ResourceAssignmentDataSet GetResourceAssignmentDataSet()
        {

            string adress = ViewConfigurationBase.BaseUrl;
            Guid[] resourceUids = new Guid[1];

            SvcResource.ResourceClient resourceClient = new SvcResource.ResourceClient("basicHttp_Resource", adress);
            resourceUids[0] = resourceClient.GetCurrentUserUid();

            PSLib.Filter resourceAssignmentFilter = GetResourceAssignmentFilter(resourceUids);
            string resourceAssignmentFilterXml = resourceAssignmentFilter.GetXml();

            return resourceClient.ReadResourceAssignments(resourceAssignmentFilterXml);
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
        private TimesheetHeaderInfos GetTimesheetStatus(Guid periodUID, Guid resUID, out Guid tuid, out SvcTimeSheet.TimesheetDataSet tsDS)
        {
            string adress = ViewConfigurationBase.BaseUrl;
            SvcTimeSheet.TimeSheetClient tsClient = new SvcTimeSheet.TimeSheetClient("basicHttp_TimeSheet", adress);
            tsDS = tsClient.ReadTimesheetByPeriod(resUID, periodUID, SvcTimeSheet.Navigation.Current);

            if (tsDS.Headers.Rows.Count > 0)
            {
                tuid=tsDS.Headers[0].TS_UID;
                var rw = tsDS.Headers[0];
                return new TimesheetHeaderInfos 
                    { Name = rw.TS_NAME, 
                      Comments = rw.TS_COMMENTS, 
                      Status = (int)rw.TS_STATUS_ENUM,
                      TotalActualWork = rw.TS_TOTAL_ACT_VALUE / 60000m,
                      TotalOverTimeWork = rw.TS_TOTAL_ACT_OVT_VALUE / 60000m,
                      TotalNonBillable = rw.TS_TOTAL_ACT_NON_BILLABLE_VALUE / 60000m,
                      TotalNonBillableOvertime = rw.TS_TOTAL_ACT_NON_BILLABLE_OVT_VALUE / 60000m
                    };
                
            }
            else
            {
                tuid = Guid.Empty;
                return null;
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
        private void createRow(WholeLine group, ref SvcTimeSheet.TimesheetDataSet _tsDS, SvcResource.ResourceAssignmentDataSet _resAssDS, SvcTimeSheet.TimesheetDataSet.LinesRow y, ViewConfigurationBase configuration, DateTime Start, DateTime Stop, string assignementId)
        {
            string adress = ViewConfigurationBase.BaseUrl;
            bool isAdmin = group.Actuals != null && group.Actuals.Count > 0 && group.Actuals[0].Values != null &&
                            ((group.Actuals[0].Values.Value != null && group.Actuals[0].Values.Value is AdministrativeRow) ||
                            (group.Actuals[0].Values.OldValue != null && group.Actuals[0].Values.OldValue is AdministrativeRow));
            if (!group.Changed) return;
            if (y == null)//creation
            {
                try
                {

                    SvcAdmin.AdminClient adminSvc = new SvcAdmin.AdminClient("basicHttp_Admin", adress);
                    SvcAdmin.TimesheetLineClassDataSet tsLineClassDs = adminSvc.ReadLineClasses(SvcAdmin.LineClassType.All, SvcAdmin.LineClassState.Enabled);

                    Guid timeSheetUID = new Guid(_tsDS.Headers.Rows[0].ItemArray[0].ToString());

                    SvcTimeSheet.TimeSheetClient tsClient = new SvcTimeSheet.TimeSheetClient("basicHttp_TimeSheet", adress);


                    SvcTimeSheet.TimesheetDataSet.LinesRow line = _tsDS.Lines.NewLinesRow();  //Create a new row for the timesheet
                    
                    line.TS_UID = timeSheetUID;  
                    line.ASSN_UID = new Guid(assignementId);  //try if this works, may be we need it when reading the rows; Francesco
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
                        line.TS_LINE_STATUS = (byte)PSLib.TimesheetEnum.LineStatus.Approved;              
                        line.TS_LINE_VALIDATION_TYPE = (byte)PSLib.TimesheetEnum.ValidationType.Verified;
                        line.TS_LINE_CLASS_UID = tsLineClassDs.LineClasses[0].TS_LINE_CLASS_UID;                    
                        line.TS_LINE_STATUS = (byte)PSLib.TimesheetEnum.LineStatus.Approved;
                        line.TS_LINE_VALIDATION_TYPE = (byte)PSLib.TimesheetEnum.ValidationType.Verified;
                        line.TS_LINE_CACHED_ASSIGN_NAME = tsLineClassDs.LineClasses[0].TS_LINE_CLASS_DESC;
                        line.TASK_UID = GetTaskUID(line.ASSN_UID, _resAssDS);
                    }

                    _tsDS.Lines.AddLinesRow(line);  //add new row to the timesheet dataset

                    Guid[] uids = new Guid[] { line.TS_LINE_UID };
                    
                    tsClient.PrepareTimesheetLine(timeSheetUID, ref _tsDS, uids);  //Validates and populates a timesheet line item and preloads actuals table in the dataset
                   
                    createActuals(_tsDS, line, Start, Stop);
                    
                }
                catch (Exception e)
                {

                    return;

                }
            }
        } 
        private void copyToRow(WholeLine group, SvcTimeSheet.TimesheetDataSet _tsDS, SvcResource.ResourceAssignmentDataSet _resAssDS, SvcTimeSheet.TimesheetDataSet.LinesRow y, ViewConfigurationBase configuration, DateTime Start, DateTime Stop, string assignementId)
        {
            if (!group.Changed) return;
            
            bool[] processed = new bool[Convert.ToInt32(Stop.Subtract(Start).TotalDays)]; 
            var allLines= y.GetActualsRows();
            if (allLines == null || allLines.Length == 0)
            {
                createActuals(_tsDS, y, Start, Stop);
                allLines = y.GetActualsRows();
            }
            if (allLines!=null)
            {
                foreach (SvcTimeSheet.TimesheetDataSet.ActualsRow day in allLines.OrderBy(m => m.TS_ACT_START_DATE))
                {
                    int i = Convert.ToInt32(day.TS_ACT_START_DATE.Subtract(Start).TotalDays);
                    if (i >= processed.Length) continue;
                    processed[i] = true;
                    copyToActualRow(group, day, i, configuration);
                }
            }
            
        }
        private void createActuals(SvcTimeSheet.TimesheetDataSet _tsDS, SvcTimeSheet.TimesheetDataSet.LinesRow y, DateTime Start, DateTime Stop)
        {
            DateTime day = Start;
            while(day<Stop)
            {
                SvcTimeSheet.TimesheetDataSet.ActualsRow actualsRow = _tsDS.Actuals.NewActualsRow();
                actualsRow.TS_LINE_UID = y.TS_LINE_UID;

                actualsRow.TS_ACT_START_DATE = day;
                actualsRow.TS_ACT_FINISH_DATE = new DateTime(day.Year, day.Month, day.Day, 23, 59, 59);
                _tsDS.Actuals.AddActualsRow(actualsRow);
                day=day.AddDays(1);
            }
        }
        private bool GetAllSingleValues(WindowsIdentity user, ViewConfigurationBase configuration, string periodId, DateTime start, DateTime stop, string projectId, string assignementId, ActualWorkRow ar, ActualOvertimeWorkRow aor, SingleValuesRow sv=null)
        {
            string adress = ViewConfigurationBase.BaseUrl;
            SvcStatusing.StatusingClient proxy = new SvcStatusing.StatusingClient("basicHttp_Statusing", adress);
            //SvcStatusing.StatusingDataSet res = proxy.ReadStatusForResource(LoggedUser(), new Guid(assignementId), start, stop);
            SvcStatusing.StatusingDataSet res = proxy.ReadStatus(new Guid(assignementId), start, stop);
            bool result = false;
            if (res.Assignments.Count > 0)
            {
                result = true;
                var sa=res.Assignments[0];
                
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
            if (res.Tasks.Count > 0)
            {
                result = true;
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

                }
                if (aor != null)
                {
                    aor.AssignementId = assignementId;
                    if (configuration.OvertimeWorkT && !st.IsTASK_OVT_WORKNull()) aor.OvertimeWorkT = st.TASK_OVT_WORK / 60000m;
                    if (configuration.RemainingOvertimeWorkT && !st.IsTASK_REM_OVT_WORKNull()) aor.RemainingOvertimeWorkT = st.TASK_REM_OVT_WORK / 60000m;
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
                }
                
            }
            return result;
        }

        public TimesheetsSets DefaultTimesheetSet { get { return TimesheetsSets.Last3; } }
        public IEnumerable<ProjectInfo> UserProjects(System.Security.Principal.WindowsIdentity user)
        {
            List<ProjectInfo> res = new List<ProjectInfo>();
            using (WindowsImpersonationContext impersonatedUser = user.Impersonate())
            {
                SvcResource.ResourceAssignmentDataSet resourceAssignmentDS = GetResourceAssignmentDataSet();
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
            }
            return res;
        }

        public IEnumerable<AssignementInfo> ProjectAssignements(System.Security.Principal.WindowsIdentity user, string ProjectId)
        {
            string adress = ViewConfigurationBase.BaseUrl;
            List<AssignementInfo> res = new List<AssignementInfo>();
            if (string.IsNullOrWhiteSpace(ProjectId))
            {
                return res;
            }
            if (ProjectId == "-1")
            {
                using (WindowsImpersonationContext impersonatedUser = user.Impersonate())
                {
                    SvcAdmin.TimesheetLineClassDataSet tslineclassDS = new SvcAdmin.TimesheetLineClassDataSet();
                    SvcAdmin.AdminClient admClient = new SvcAdmin.AdminClient("basicHttp_Admin", adress);
                    tslineclassDS = admClient.ReadLineClasses(SvcAdmin.LineClassType.AllNonProject, SvcAdmin.LineClassState.Enabled);
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
            }
            else
            {
                using (WindowsImpersonationContext impersonatedUser = user.Impersonate())
                {
                    SvcResource.ResourceAssignmentDataSet resourceAssignmentDS = GetResourceAssignmentDataSet();
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
                }
            }
            return res;
        }

        public IEnumerable<Timesheet> SelectTimesheets(System.Security.Principal.WindowsIdentity user, TimesheetsSets set)
        {
            string adress = ViewConfigurationBase.BaseUrl;
            using (WindowsImpersonationContext impersonatedUser = user.Impersonate())
            {
                
                int selection= 32; //all timesheets all the deleted ones
                DateTime Start=new DateTime(1984, 1, 1);
                DateTime End=new DateTime(2049, 12, 1);
                if (set == TimesheetsSets.Default) set = DefaultTimesheetSet;
                switch(set)
                {
                    case TimesheetsSets.CreatedProgress: selection=1;
                        break;
                    case TimesheetsSets.Last3:
                        Start=DateTime.Today.AddMonths(-3);
                        End=DateTime.Today;
                        break;
                    case TimesheetsSets.Last6:
                        Start=DateTime.Today.AddMonths(-6);
                        End=DateTime.Today;
                        break;
                    case TimesheetsSets.Next6Last3:
                        Start=DateTime.Today.AddMonths(-3);
                        End=DateTime.Today.AddMonths(+6);
                        break;
                    default: selection = 32; break; //all existing

                }

                SvcTimeSheet.TimeSheetClient client = new SvcTimeSheet.TimeSheetClient("basicHttp_TimeSheet", adress);
                SvcTimeSheet.TimesheetListDataSet res = client.ReadTimesheetList(LoggedUser(), Start, End, selection);
                List<Timesheet> fres = new List<Timesheet>();
                foreach (var t in res.Timesheets)
                {
                    fres.Add(new Timesheet{Name=t.WPRD_NAME, Id=t.WPRD_UID.ToString(), Start=t.WPRD_START_DATE, Stop=t.WPRD_FINISH_DATE});
                }
                return fres;
            }
        }
        public List<BaseRow> GetRows(WindowsIdentity user, ViewConfigurationBase configuration, string periodId, DateTime start, DateTime stop, out int status, out bool canDelete, out bool canRecall, out TimesheetHeaderInfos tInfos)
        {
            using (WindowsImpersonationContext impersonatedUser = user.Impersonate())
            {
                string adress = ViewConfigurationBase.BaseUrl;
                Guid ruid = LoggedUser();
                Guid periodUID = new Guid(periodId);
                Guid tuid;
                tInfos = null;
                int dayCount = Convert.ToInt32(stop.Subtract(start).TotalDays);
                if (configuration is ViewConfigurationTask)
                {
                    ActualWorkRow actual = null;
                    ActualOvertimeWorkRow overtime = null;
                    SingleValuesRow onlySingleValues = null;
                    decimal?[] actualArray = null;
                    decimal?[] overtimeArray = null;
                    var tres = new List<BaseRow>();
                    SvcStatusing.StatusingClient proxy = new SvcStatusing.StatusingClient("basicHttp_Statusing", adress);
                        /// Reading Assignements //////
                        /// 


                        //SvcStatusing.StatusingDataSet ds = proxy.ReadStatusForResource(ruid, Guid.Empty, start, stop);
                        SvcStatusing.StatusingDataSet ds = proxy.ReadStatus(Guid.Empty, start, stop);
                        foreach(var row in ds.Assignments){
                            if (configuration.NoTPData)
                            {
                                onlySingleValues = new SingleValuesRow();
                                onlySingleValues.ProjectId = row.PROJ_UID.ToString();
                                onlySingleValues.ProjectName = row.PROJ_NAME;
                                onlySingleValues.AssignementId = row.ASSN_UID.ToString();
                                onlySingleValues.AssignementName = row.TASK_NAME;
                                onlySingleValues.DayTimes = new List<decimal?>();
                            }
                            else
                            {
                                if (configuration.ActualWorkA)
                                {
                                    actual = new ActualWorkRow();
                                    actualArray = new decimal?[dayCount];
                                    actual.ProjectId = row.PROJ_UID.ToString();
                                    actual.ProjectName = row.PROJ_NAME;
                                    actual.AssignementId = row.ASSN_UID.ToString();
                                    actual.AssignementName = row.TASK_NAME;
                                    actual.DayTimes = new List<decimal?>();

                                }
                                if (configuration.ActualOvertimeWorkA)
                                {
                                    overtime = new ActualOvertimeWorkRow();
                                    overtimeArray = new decimal?[dayCount];
                                    overtime.ProjectId = row.PROJ_UID.ToString();
                                    overtime.ProjectName = row.PROJ_NAME;
                                    overtime.AssignementId = row.ASSN_UID.ToString();
                                    overtime.AssignementName = row.TASK_NAME;
                                    overtime.DayTimes = new List<decimal?>();

                                }
                                try
                                {
                                    SvcStatusing.StatusingTimephasedActualsDataSet tData =
                                    proxy.ReadStatusTimephasedData(row.PROJ_UID, row.ASSN_UID, start, stop, 1440);
                                    foreach (var actuals in tData.AssignmentTimephasedData)
                                    {
                                        int i = Convert.ToInt32(actuals.TimeByDay.Subtract(start).TotalDays);
                                        if (i >= dayCount) continue;
                                        if (actual != null && !actuals.IsAssignmentActualWorkNull())
                                        {
                                            actualArray[i] = actuals.AssignmentActualWork / 60000m;
                                        }
                                        if (overtime != null && !actuals.IsAssignmentOvertimeWorkNull())
                                        {
                                            overtimeArray[i] = actuals.AssignmentOvertimeWork / 60000;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    continue;
                                }
                            }
                            if (actual != null) actual.DayTimes = actualArray.ToList();
                            if (overtime != null) overtime.DayTimes = overtimeArray.ToList();
                            if (actual != null) tres.Add(actual);
                            if (overtime != null) tres.Add(overtime);
                            if (onlySingleValues != null) tres.Add(onlySingleValues);
                            GetAllSingleValues(user, configuration, periodId, start, stop, row.PROJ_UID.ToString(), row.ASSN_UID.ToString(), actual, overtime, onlySingleValues);
                        }
                        status = -1;
                        canDelete = false;
                        canRecall = false;
                        return tres;
                }
                SvcTimeSheet.TimesheetDataSet tsDs;
                tInfos = GetTimesheetStatus(periodUID, ruid, out tuid, out tsDs);
                if (tInfos == null) status = -1;
                else status = tInfos.Status.Value;
                if (status == -1)
                {
                    if (configuration is ViewConfigurationRow)
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
                        SvcTimeSheet.TimeSheetClient tsClient = new SvcTimeSheet.TimeSheetClient("basicHttp_TimeSheet", adress);
                        tsClient.CreateTimesheet(tsDs, SvcTimeSheet.PreloadType.Default);  //default load type is to use the server settings
                        GetTimesheetAction(status, out canDelete, out canRecall);
                    }
                    else
                    {
                        canRecall = false;
                        canDelete = false;
                    }
                    return new List<BaseRow>();

                }
                
                    GetTimesheetAction(status, out canDelete, out canRecall);
                    var res = new List<BaseRow>(); 
                    foreach (var row in tsDs.Lines)
                    {
                        ActualWorkRow actual = null;
                        ActualOvertimeWorkRow overtime = null;
                        NonBillableActualWorkRow nonbillable = null;
                        NonBillableOvertimeWorkRow nonbillableovertime = null;
                        AdministrativeRow admin=null;
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
                        bool actualNZ = false;
                        bool overtimeNZ = false;
                        bool nonbillableNZ = false;
                        bool nonbillableovertimeNZ = false;
                        foreach (var actuals in row.GetActualsRows())
                        {

                            int i = Convert.ToInt32(actuals.TS_ACT_START_DATE.Subtract(start).TotalDays);
                            if (i >= dayCount) continue;
                            if (nonbillable != null && !actuals.IsTS_ACT_NON_BILLABLE_VALUENull())
                            {
                                nonbillableArray[i]=actuals.TS_ACT_NON_BILLABLE_VALUE / 60000m;
                                if (nonbillableArray[i].Value != 0m) nonbillableNZ = true;
                            }
                            if (nonbillableovertime != null && !actuals.IsTS_ACT_NON_BILLABLE_OVT_VALUENull())
                            {
                                nonbillableovertimeArray[i]=actuals.TS_ACT_NON_BILLABLE_OVT_VALUE / 60000m;
                                if (nonbillableovertimeArray[i].Value != 0m) nonbillableovertimeNZ = true;
                            }
                            if (actual != null && !actuals.IsTS_ACT_VALUENull())
                            {
                                actualArray[i] = actuals.TS_ACT_VALUE / 60000m;
                                if (actualArray[i].Value != 0m) actualNZ= true;
                            }
                            if (overtime != null && !actuals.IsTS_ACT_OVT_VALUENull())
                            {
                                overtimeArray[i] = actuals.TS_ACT_OVT_VALUE / 60000m;
                                if (overtimeArray[i].Value != 0m) overtimeNZ = true;
                            }
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
                        if ((configuration is ViewConfigurationTask) || actual != null || overtime  != null || nonbillable != null || nonbillableovertime != null)
                            result = GetAllSingleValues(user, configuration, periodId, start, stop, row.PROJ_UID.ToString(), row.ASSN_UID.ToString(), actual, overtime);
                        if (actual != null)
                        {
                            if (result) res.Add(actual);
                            else
                            {
                                admin.DayTimes = actual.DayTimes;
                                res.Add(admin);
                            }
                        }
                        if (overtime != null && result) res.Add(overtime);
                        if (nonbillable != null && result) res.Add(nonbillable);
                        if (nonbillableovertime != null && result) res.Add(nonbillableovertime);

                    }
                    return res;
                
            }

        }
        public BaseRow GetRowSingleValues(WindowsIdentity user, ViewConfigurationBase configuration, string periodId, DateTime start, DateTime stop, string projectId, string assignementId, Type RowType)
        {
            
            BaseRow res=null;
            using (WindowsImpersonationContext impersonatedUser = user.Impersonate())
            {
                if (RowType == typeof(ActualWorkRow))
                {
                    res = new ActualWorkRow();
                    GetAllSingleValues(user, configuration, periodId, start, stop, projectId, assignementId, res as ActualWorkRow, null);
                }
                else if (RowType == typeof(ActualOvertimeWorkRow))
                {
                    res = new ActualOvertimeWorkRow();
                    GetAllSingleValues(user, configuration, periodId, start, stop, projectId, assignementId, null, res as ActualOvertimeWorkRow);
                }
                else if (RowType == typeof(SingleValuesRow))
                {
                    res = new SingleValuesRow();
                    GetAllSingleValues(user, configuration, periodId, start, stop, projectId, assignementId, null, null, res as SingleValuesRow);
                }
                else if (RowType == typeof(AdministrativeRow))
                {
                    res = new AdministrativeRow();
                }
                else if (RowType == typeof(NonBillableActualWorkRow))
                {
                    res = new NonBillableActualWorkRow();
                }
                else
                {
                    res = new NonBillableOvertimeWorkRow();
                }
            }
            if (res != null)
            {
                res.AssignementId = assignementId;
                res.ProjectId=projectId;
            }
            return res;
        
        }
        public void UpdateRows(WindowsIdentity user, ViewConfigurationBase configuration, string periodId, DateTime start, DateTime stop, IEnumerable<Tracker<BaseRow>> rows, bool submit)
        {
            string adress = ViewConfigurationBase.BaseUrl;
            if (rows == null) return;
            using (WindowsImpersonationContext impersonatedUser = user.Impersonate())
            {
                var crows = rows.GroupBy(m => m.Value.AssignementId);
                Guid ruid = LoggedUser();
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
                SvcResource.ResourceAssignmentDataSet _resAssDS = null;
                ///dataset processing
                
                Guid periodUID = new Guid(periodId);
                Guid tuid;
                if (configuration is ViewConfigurationRow && ((!noChange) || submit))
                {
                    
                    SvcTimeSheet.TimesheetDataSet tsDs;
                    TimesheetHeaderInfos tInfos=GetTimesheetStatus(periodUID, ruid, out tuid, out tsDs);
                    if (tInfos == null) return;
                    int status = tInfos.Status.Value;
                    
                    if (noChange)
                    {
                        try
                        {
                            Guid jobGuid=Guid.NewGuid();
                            SvcTimeSheet.TimeSheetClient tsclient = new SvcTimeSheet.TimeSheetClient("basicHttp_TimeSheet", adress);
                            var tsGuid = (Guid)(tsDs.Headers.Rows[0].ItemArray[0]);
                            tsclient.QueueSubmitTimesheet(jobGuid, tsGuid, (Guid)tsDs.Headers.Rows[0].ItemArray[8], BusisnessResources.ApprovalComment);
                            bool res = QueueHelper.WaitForQueueJobCompletion(jobGuid, (int)SvcQueueSystem.QueueMsgType.TimesheetSubmit);
                            if (!res) throw new TimesheetSubmitException();
                        }
                        catch { throw new TimesheetSubmitException(); }
                    }
                    else
                    {
                        foreach (var row in tsDs.Lines)
                        {
                            string assignementId = row.ASSN_UID.ToString();
                            WholeLine group = null;
                            bool res = dict.TryGetValue(assignementId, out group);
                            if (!res) continue;
                            group.Processed = true;
                        }
                        
                        foreach (var group in list)
                        {
                            if (group.Processed || !group.Changed) continue;
                            if (_resAssDS == null) _resAssDS = GetResourceAssignmentDataSet();
                            createRow(group, ref tsDs, _resAssDS, null, configuration, start, stop, group.Key);
                            group.Processed = true;
                        }
                        List<SvcTimeSheet.TimesheetDataSet.LinesRow> rowsToDelete = new List<SvcTimeSheet.TimesheetDataSet.LinesRow>();
                        foreach (var row in tsDs.Lines)
                        {
                            string assignementId = row.ASSN_UID != null ? row.ASSN_UID.ToString() : row.TS_LINE_CLASS_UID.ToString();  //John; this line sets the assignmentUID?
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
                            var actuals=row.GetActualsRows();
                            foreach (var ac in actuals)
                            {
                                ac.Delete();
                            }
                            row.Delete();
                            
                        }
                        SvcTimeSheet.TimeSheetClient tsclient = new SvcTimeSheet.TimeSheetClient("basicHttp_TimeSheet", adress);
                            var tsGuid = (Guid)(tsDs.Headers.Rows[0].ItemArray[0]);
                            try
                            {
                                Guid jobGuid = Guid.NewGuid();
                                tsclient.QueueUpdateTimesheet(jobGuid,
                                     tsGuid,
                                    (SvcTimeSheet.TimesheetDataSet)tsDs.GetChanges());  //Saves the specified timesheet data to the Published database
                                bool res = QueueHelper.WaitForQueueJobCompletion(jobGuid, (int)SvcQueueSystem.QueueMsgType.TimesheetUpdate);
                                if (!res) throw new TimesheetUpdateException();
                            }
                            catch { throw new TimesheetUpdateException(); }
                            if (submit)
                            {
                                try
                                {
                                    Guid jobGuid= Guid.NewGuid();
                                    tsclient.QueueSubmitTimesheet(jobGuid, tsGuid, (Guid)tsDs.Headers.Rows[0].ItemArray[8], BusisnessResources.ApprovalComment);
                                    bool res = QueueHelper.WaitForQueueJobCompletion(jobGuid, (int)SvcQueueSystem.QueueMsgType.TimesheetSubmit);
                                    if (!res) throw new TimesheetSubmitException();
                                }
                                catch { throw new TimesheetSubmitException(); }
                            }
                        
                        
                    }
                }
                ///////

                ////xml Processing //////


                SvcStatusing.StatusingClient statusingClient = new SvcStatusing.StatusingClient("basicHttp_Statusing", adress);
                if (!noChange) 
                {
                    try
                    {
                        if (_resAssDS == null) _resAssDS = GetResourceAssignmentDataSet();

                        string xml = new ChangeXml(_resAssDS, list, configuration, start, stop).Get();

                        if (xml != null) statusingClient.UpdateStatus(xml);
                    }
                    catch { throw new StatusUpdateException(); }
                }
                if (submit) 
                {
                    List<Guid> changedAssignements= new List<Guid>();
                    foreach(var g in list)
                    {
                        if (g.Actuals != null && g.Actuals.Count > 0 && g.Actuals[0].Values != null &&
                            (g.Actuals[0].Values.Value is AdministrativeRow || g.Actuals[0].Values.OldValue is AdministrativeRow)) continue;
                        changedAssignements.Add(new Guid(g.Key));
                    }
                    try
                    {
                        if (changedAssignements.Count > 0) statusingClient.SubmitStatus(changedAssignements.ToArray(), BusisnessResources.StausApprovalComment);
                    }
                    catch { throw new StatusSubmitException(); }
                }
                
                /////everything ok...confirmchanges//////
                foreach (Tracker<BaseRow> tracker in rows)
                {
                    tracker.Confirm();
                }
            }
        }
        public void RecallDelete(WindowsIdentity user, string periodId, DateTime start, DateTime stop, bool isRecall)
        {
            string adress = ViewConfigurationBase.BaseUrl;
            using (WindowsImpersonationContext impersonatedUser = user.Impersonate())
            {
                Guid ruid = LoggedUser();
                Guid periodUID = new Guid(periodId);
                Guid tuid;
                SvcTimeSheet.TimesheetDataSet tsDs;
                TimesheetHeaderInfos tInfos= GetTimesheetStatus(periodUID, ruid, out tuid, out tsDs);
                int status = -1;
                if (tInfos != null) status = tInfos.Status.Value;
                bool canDelete;
                bool canRecall;
                GetTimesheetAction(status, out canDelete, out canRecall);
                if (isRecall && canRecall)
                {
                    try
                    {
                        SvcTimeSheet.TimeSheetClient tsClient = new SvcTimeSheet.TimeSheetClient("basicHttp_TimeSheet", adress);
                        Guid jobUID = Guid.NewGuid();
                        tsClient.QueueRecallTimesheet(jobUID, tuid);
                        bool res = QueueHelper.WaitForQueueJobCompletion(jobUID, (int)SvcQueueSystem.QueueMsgType.TimesheetRecall);
                        

                    }
                    catch 
                    {
                        
                    }
                }
                else if ((!isRecall) && canDelete)
                {
                    try
                    {
                        SvcTimeSheet.TimeSheetClient tsClient = new SvcTimeSheet.TimeSheetClient("basicHttp_TimeSheet", adress);
                        Guid jobUID = Guid.NewGuid();
                        tsClient.QueueDeleteTimesheet(jobUID, tuid);
                        bool res = QueueHelper.WaitForQueueJobCompletion(jobUID, (int)SvcQueueSystem.QueueMsgType.TimesheetDelete);
                    }
                    catch
                    {

                    }
                }
            }
            
        }
        protected void GetTimesheetAction(int status, out bool canDelete, out bool canRecall)
        {
            canDelete = (status == 0) || (status == 4) || (status == 2);
            canRecall = (status == 1) || (status == 3) || (status == 2);
            
        }
    }
}
