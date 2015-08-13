using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVCControlsToolkit.Controller;
using System.Reflection;
using System.Globalization;
using SvcProject;


namespace TimeSheetIBusiness
{
    
    public class ChangeXml
    {
        public class Entry
        {
            public string Code;
            public int XType;
            public bool Timed;
            public bool finishDate;
            public bool isInTask;
        }
        private List<WholeLine> groups;
        private ViewConfigurationBase configuration;
        private IRepository repository;
        private DateTime start, end;
        private SvcResource.ResourceAssignmentDataSet _resAssDS;
        internal ChangeXml(SvcResource.ResourceAssignmentDataSet _resAssDS, List<WholeLine> groups, ViewConfigurationBase configuration, DateTime start, DateTime end, IRepository repsoitory)
        {
            this.groups = groups;
            this.configuration=configuration;
            this.start=start;
            this.end=end;
            this._resAssDS = _resAssDS;
            this.repository = repsoitory;
        }
        private Guid GetTaskUID(string assn_uid, SvcResource.ResourceAssignmentDataSet _resAssDS)
        {
            string expression = "ASSN_UID = '" + assn_uid + "'";
            //SvcTimeSheet.TimesheetDataSet.LinesRow[] lines = (SvcTimeSheet.TimesheetDataSet.LinesRow[])_tsDS.Lines.Select(expression);
            //DataRow[] lines = (DataRow[])

            SvcResource.ResourceAssignmentDataSet.ResourceAssignmentRow[] lines = (SvcResource.ResourceAssignmentDataSet.ResourceAssignmentRow[])_resAssDS.ResourceAssignment.Select(expression);

            return new Guid(lines[0].TASK_UID.ToString());

        }
        static ChangeXml()
        {
            PIDS = new Dictionary<string, Entry>();
            PIDS.Add("WorkA", new Entry{Code= "251658246", XType= 3});
            PIDS.Add("RegularWorkA", new Entry{Code= "251658282", XType= 3});
            PIDS.Add("RemainingWorkA", new Entry{Code= "251658248", XType= 3});
            PIDS.Add("StartA", new Entry{Code= "251658252", XType= 1});
            PIDS.Add("FinishA", new Entry { Code = "251658253", XType = 1, finishDate = true});
            PIDS.Add("ActualStartA", new Entry{Code= "251658256", XType= 1});
            PIDS.Add("ActualFinishA", new Entry { Code = "251658257", XType = 1, finishDate = true });
            PIDS.Add("PercentWorkCompleteA", new Entry{Code= "251658274", XType= 4});
            PIDS.Add("AssignmentUnitsA", new Entry{Code= "251658275", XType= 6});
            PIDS.Add("ConfirmedA", new Entry{Code= "251658295", XType= 5});
            PIDS.Add("CommentsA", new Entry{Code= "251658287", XType= 10});

            PIDS.Add("WorkT", new Entry { Code = "184549380", XType = 3, isInTask = true });
            PIDS.Add("RegularWorkT", new Entry { Code = "184549415", XType = 3, isInTask = true });
            PIDS.Add("RemainingWorkT", new Entry { Code = "184549382", XType = 3, isInTask = true });
            PIDS.Add("ActualWorkT", new Entry { Code = "184549384", XType = 3, isInTask = true });
            PIDS.Add("StartT", new Entry { Code = "184549386", XType = 1, isInTask = true });
            PIDS.Add("FinishT", new Entry { Code = "184549387", XType = 1, finishDate = true, isInTask = true });
            PIDS.Add("ResumeT", new Entry { Code = "184549389", XType = 1, isInTask = true });
            PIDS.Add("DeadlineT", new Entry { Code = "184549394", XType = 1, isInTask = true });
            PIDS.Add("DurationT", new Entry { Code = "184549405", XType = 2, isInTask = true });
            PIDS.Add("RemainingDurationT", new Entry { Code = "184549407", XType = 2, isInTask = true });
            PIDS.Add("TaskNameT", new Entry { Code = "184549403", XType = 10, isInTask = true });
            PIDS.Add("PercentCompleteT", new Entry { Code = "184549410", XType = 4, isInTask = true });
            PIDS.Add("PercentWorkCompleteT", new Entry { Code = "184549411", XType = 4, isInTask = true });
            PIDS.Add("PhysicalPercentCompleteT", new Entry { Code = "184549412", XType = 4, isInTask = true });

            PIDS.Add("OvertimeWorkA", new Entry{Code= "251658247", XType= 3});
            PIDS.Add("OvertimeWorkT", new Entry { Code = "184549381", XType = 3, isInTask = true });
            PIDS.Add("RemainingOvertimeWorkT", new Entry { Code = "184549383", XType = 3, isInTask = true });

        }
        protected static IDictionary<string, Entry> PIDS;
        protected static string level1 = "    ";
        protected static string level2 = "         ";
        public string Get(SvcCustomFields.CustomFieldDataSet customfields)
        {
            List<WholeLine> notFildered = groups;
            groups = new List<WholeLine>();
            
            foreach (var group in notFildered)
            {
                if (group.Actuals != null && group.Actuals.Count > 0 && group.Actuals[0].Values != null &&
                    (group.Actuals[0].Values.Value is AdministrativeRow || group.Actuals[0].Values.OldValue is AdministrativeRow)) continue;
                groups.Add(group);
            }
            if (groups.Count == 0) return null;
            StringBuilder sb = new StringBuilder();
            sb.Append("<Changes xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");
            
            foreach (var group in groups)
            {
                addGroup(customfields,group, sb);
            }
            sb.Append(Environment.NewLine);
            sb.Append("</Changes>");
            return sb.ToString();
        }
        private DateTime finishDate(DateTime x)
        {
            return new DateTime(x.Year, x.Month, x.Day, 23, 59, 59); 
        }
        private void addGroup(SvcCustomFields.CustomFieldDataSet customfields, WholeLine group, StringBuilder sb)
        {
            if (!group.Changed) return;
            string assignementId = group.Key.Split("_".ToCharArray())[0];
            string projectId = group.Actuals[0].Values.Value.ProjectId;
            sb.Append(Environment.NewLine);
            sb.Append(level1);
            sb.Append("<Proj ID=\""); sb.Append(projectId); sb.Append("\">");
            sb.Append(Environment.NewLine);
            sb.Append(level1);
            sb.Append("<Assn ID=\""); sb.Append(assignementId); sb.Append("\">");
            StringBuilder asb = new StringBuilder();
            foreach (ExtendedRow x in group.Actuals)
            {
                if (!x.Changed) continue;
                object source = x.Values.Value;
                if (x.TChanged && source != null)
                {
                    string code = null;
                    if (x.Values.Value is ActualWorkRow && configuration.ActualWorkA) code = "251658250";
                    else if (x.Values.Value is ActualOvertimeWorkRow && configuration.ActualOvertimeWorkA) code = "251658251";
                    if ((x.Values.Value is ActualWorkRow && configuration.ActualWorkA) || (x.Values.Value is ActualOvertimeWorkRow && configuration.ActualOvertimeWorkA))
                    {
                        DateTime day = start;
                        foreach (var tracker in x.TValues)
                        {
                            var work = tracker.Value;
                            if (!tracker.Changed)
                            {
                                day = day.AddDays(1);
                                continue;
                            }
                            sb.Append(Environment.NewLine);
                            sb.Append(level2);
                            sb.Append("<PeriodChange PID=\""); sb.Append(code); sb.Append("\" Start=\"");
                            sb.Append(day.ToString("s", CultureInfo.InvariantCulture)); sb.Append("\" End=\"");
                            sb.Append(day.AddDays(1).AddSeconds(-1).ToString("s", CultureInfo.InvariantCulture)); sb.Append("\">"); sb.Append((Convert.ToInt64(work * 60000m)).ToString(CultureInfo.InvariantCulture));
                            sb.Append("</PeriodChange>");
                            day = day.AddDays(1);
                        }
                    }
                }
                if (x.CChanged && source != null && !group.IsTopLevelTask){
                    List<string> changedPropeties = null;
                    
                        changedPropeties = new List<string>();
                        if(x.Values.Value is ActualWorkRow)
                        foreach (var key in (x.Values.Value as ActualWorkRow).CustomFieldItems) changedPropeties.Add(key.FullName);
                        if (x.Values.Value is SingleValuesRow)
                            foreach (var key in (x.Values.Value as SingleValuesRow).CustomFieldItems) changedPropeties.Add(key.FullName);
                    StringBuilder sbT = sb;
                    foreach (string property in changedPropeties)
                    {
                        CustomFieldInfo info = new CustomFieldInfo();
                        if (customfields.CustomFields.Rows.Cast<SvcCustomFields.CustomFieldDataSet.CustomFieldsRow>().Any(m => m.MD_PROP_NAME == property))
                        {
                            var csfield = customfields.CustomFields.Rows.Cast<SvcCustomFields.CustomFieldDataSet.CustomFieldsRow>().First(m => m.MD_PROP_NAME == property);
                            info = repository.GetCustomFieldType(csfield.MD_PROP_UID_SECONDARY, csfield.MD_PROP_TYPE_ENUM, property);
                        }
                        string type = info.DataType;
                        
                        string formattedValue= null;
                        if (x.Values.Value is ActualWorkRow || x.Values.Value is SingleValuesRow)
                        {
                            object value = null;
                            var csitem = x.Values.Value is ActualWorkRow ? (x.Values.Value as ActualWorkRow).CustomFieldItems.First(t => t.FullName == property)
                                : (x.Values.Value as SingleValuesRow).CustomFieldItems.First(t => t.FullName == property);

                            if (!csitem.LookupTableGuid.HasValue || csitem.LookupTableGuid.Value == Guid.Empty)
                            {
                                switch (type)
                                {
                                    case "Date": value = csitem.DateValue;
                                        if(value != null)
                                        formattedValue = ((DateTime)value).ToString("s", CultureInfo.InvariantCulture);
                                        break; //Date 
                                    case "Finishdate":
                                        value = csitem.DateValue;
                                        if (value != null)
                                        formattedValue = finishDate((DateTime)value).ToString("s", CultureInfo.InvariantCulture); break;
                                    case "Duration":
                                        value = csitem.DurationValue;
                                        if (value != null)
                                        formattedValue = (Convert.ToInt64(((uint)value) * 4800m)).ToString(CultureInfo.InvariantCulture); break; //duration
                                    case "Cost":
                                        value = csitem.CostValue;
                                        if (value != null)
                                        formattedValue = (Convert.ToInt64(((decimal)value) * 60000m)).ToString(CultureInfo.InvariantCulture); break; //work
                                    case "Flag":
                                        value = csitem.FlagValue;
                                        if (value != null)
                                        formattedValue = ((bool)(value)) ? "True" : "False"; break; //pure int
                                    case "Number":
                                        value = csitem.NumValue;
                                        if (value != null)
                                        formattedValue = ((decimal)(value)).ToString(CultureInfo.InvariantCulture); break; //pure decimal
                                    default:
                                        
                                        value = (x.Values.Value is ActualWorkRow) ? (x.Values.Value as ActualWorkRow).CustomFieldItems.First(t => t.FullName == property).TextTValue
                                            : (x.Values.Value as SingleValuesRow).CustomFieldItems.First(t => t.FullName == property).TextTValue;
                                        if (value != null)
                                        formattedValue = (string)(value); break; //pure string

                                }
                            

                            sbT.Append(Environment.NewLine);
                            sbT.Append(level2);
                            sbT.AppendFormat("<SimpleCustomFieldChange CustomFieldType=\"{0}\" CustomFieldGuid=\"{1}\" CustomFieldName=\"{2}\">{3}</SimpleCustomFieldChange>", info.DataType, info.Guid, info.Name, formattedValue);
                            }
                        else
                        {
                            sbT.Append(Environment.NewLine);
                            sbT.Append(level2);
                            sbT.AppendFormat("<LookupTableCustomFieldChange IsMultiValued=\"false\" CustomFieldType=\"{0}\" CustomFieldGuid=\"{1}\" CustomFieldName=\"{2}\"><LookupTableValue Guid=\"{3}\">{4}</LookupTableValue></LookupTableCustomFieldChange>"
                                , csitem.DataType, csitem.CustomFieldGuid, csitem.FullName, csitem.LookupID, csitem.LookupValue);
                        }
                        }
                    }
                }

                if (x.VChanged && source != null)
                {
                    List<string> changedPropeties = null;
                    if (x.Values.OldValue == null)
                    {
                        changedPropeties = new List<string>();
                        foreach (var key in PIDS.Keys) changedPropeties.Add(key);
                    }
                    else
                    {
                        changedPropeties = (x.Values as PropertyTracker<BaseRow>).ChangedProperties;
                    }
                    foreach (string property in changedPropeties)
                    {
                        PropertyInfo prop = configuration.GetType().GetProperty(property);
                        if (prop == null) continue;
                        if (!((bool)(prop.GetValue(configuration, new object[0])))) continue;
                        prop = configuration.GetType().GetProperty(property + "_Edit");
                        if (prop == null) continue;
                        if (!((bool)(prop.GetValue(configuration, new object[0])))) continue;
                        prop = source.GetType().GetProperty(property);
                        if (prop == null) continue;
                        object value = prop.GetValue(source, new object[0]);
                        if (value == null) continue;

                        string formattedValue = null;
                        Entry infos = PIDS[property];
                        StringBuilder sbT = sb;
                        if (infos.isInTask) sbT = asb;
                        switch (infos.XType)
                        {
                            case 1: formattedValue =
                                infos.finishDate ? finishDate((DateTime)value).ToString("s", CultureInfo.InvariantCulture)
                                    : ((DateTime)value).ToString("s", CultureInfo.InvariantCulture);
                                break; //Date 
                            case 2: formattedValue = (Convert.ToInt64(((uint)value) * 4800m)).ToString(CultureInfo.InvariantCulture); break; //duration
                            case 3: formattedValue = (Convert.ToInt64(((decimal)value) * 60000m)).ToString(CultureInfo.InvariantCulture); break; //work
                            case 4: formattedValue = (Convert.ToInt32(value)).ToString(CultureInfo.InvariantCulture); break; //pure int
                            case 5: formattedValue = ((bool)(value)) ? "True" : "False"; break; //pure int
                            case 6: formattedValue = ((decimal)(value)).ToString(CultureInfo.InvariantCulture); break; //pure decimal
                            default: formattedValue = (string)(value); break; //pure string

                        }
                        if (infos.Timed)
                        {
                            sbT.Append(Environment.NewLine);
                            sbT.Append(level2);
                            sbT.Append("<PeriodChange PID=\""); sbT.Append(infos.Code); sbT.Append("\" Start=\"");
                            sbT.Append(start.ToString("s", CultureInfo.InvariantCulture)); sbT.Append("\" End=\"");
                            sbT.Append(end.ToString("s", CultureInfo.InvariantCulture)); sbT.Append("\">"); sbT.Append(formattedValue);
                            sbT.Append("</PeriodChange>");
                        }
                        else
                        {
                            sbT.Append(Environment.NewLine);
                            sbT.Append(level2);
                            sbT.Append("<Change PID=\""); sbT.Append(infos.Code); sbT.Append("\" >"); sbT.Append(formattedValue); sbT.Append("</Change>");
                        }

                    }
                }
                
                
            }
            sb.Append(Environment.NewLine);
            sb.Append(level1);
            sb.Append("</Assn>");
            sb.Append(Environment.NewLine);
            if (asb.Length > 0)
            {
                sb.Append(Environment.NewLine);
                sb.Append(level1);
                sb.Append("<Task ID=\""); sb.Append(GetTaskUID(assignementId, _resAssDS)); sb.Append("\">");
                sb.Append(asb);
                sb.Append(Environment.NewLine);
                sb.Append(level1);
                sb.Append("</Task>");
                sb.Append(Environment.NewLine);
            }
            sb.Append(level1);
            sb.Append("</Proj>");
            


        }

       

    }
    
}
