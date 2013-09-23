using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using TimeSheetIBusiness;
using MVCControlsToolkit.Controller;

namespace TimeSheetMobileWeb.Models
{
    public class RowType
    {
        public string Code { get; set; }
        public string DisplayValue { get; set; }
    }
    public class UpdateViewBase
    {
        public PeriodSelectionView PeriodSelectionInfos;
        [Display(ResourceType = typeof(SiteResources), Name = "HomeMenuPeriods")]
        public string Period { get; set; }
        public string PeriodString { get;set;}
        public DateTime CurrentPeriodStart { get; set; }
        public DateTime CurrentPeriodStop { get; set; }
        public int PeriodLength { get; set; }
        public bool Submit { get; set; }
        public RowType[] RowTypes { get; set; }
        public List<Tracker<BaseRow>> PeriodRows { get; set; }
        public String Status { get; set; }
        public bool CanDelete { get; set; }
        public bool CanRecall { get; set; }
        public TimesheetHeaderInfos HeaderInfos { get; set; }
        public string ErrorMessage { get; set; }
        public decimal[] Totals { get; set; }
        
        public static RowType GetRowType(Type rowClass)
        {
            if (rowClass == typeof(NonBillableActualWorkRow))
                return new RowType {Code = "NBAW", DisplayValue=SiteResources.NBAW };
            else if (rowClass == typeof(NonBillableOvertimeWorkRow))
                return new RowType { Code = "NBOW", DisplayValue = SiteResources.NBOW };
            else if (rowClass == typeof(ActualWorkRow))
                return new RowType { Code = "AW", DisplayValue = SiteResources.AW };
            else if (rowClass == typeof(AdministrativeRow))
                return new RowType { Code = "AD", DisplayValue = SiteResources.AD };
            else if (rowClass == typeof(SingleValuesRow))
                return new RowType { Code = "SV", DisplayValue = SiteResources.SV };
            else
                return new RowType { Code = "AOW", DisplayValue = SiteResources.AOW };
        }
        public static Type RowTypeFromCode(string code)
        {
            if (code == "NBAW") return typeof(NonBillableActualWorkRow);
            else if (code == "NBOW") return typeof(NonBillableOvertimeWorkRow);
            else if (code == "AW") return typeof(ActualWorkRow);
            else if (code == "AD") return typeof(AdministrativeRow);
            else if (code == "SV") return typeof(SingleValuesRow);
            else return typeof(ActualOvertimeWorkRow);
        }
        public string TimesheetStatusString(int status)
        {
            if (status == 0) return SiteResources.InProgress;
            else if (status == 1) return SiteResources.Submitted;
            else if (status == 2) return SiteResources.Acceptable;
            else if (status == 3) return SiteResources.Approved;
            else if (status == 4) return SiteResources.Rejected;
            else return string.Empty;

        }
        public void ReceiveRows(IEnumerable<BaseRow> rows)
        {
            List<Tracker<BaseRow>> res = new List<Tracker<BaseRow>>();
            foreach(BaseRow row in rows)
            {
                res.Add(new Tracker<BaseRow>(row));
            }
            PeriodRows = res;
        }


        public void PrepareRowTypes()
        {
            List<RowType> res= new List<RowType>();
            if (this is UpdateTasksView)
            {
                if (ViewConfigurationTask.Default.ActualWorkA && (!ViewConfigurationTask.Default.NoTPData)) res.Add(GetRowType(typeof(ActualWorkRow)));
                if (ViewConfigurationTask.Default.ActualWorkA && ViewConfigurationTask.Default.NoTPData) res.Add(GetRowType(typeof(SingleValuesRow)));
                if (ViewConfigurationTask.Default.ActualOvertimeWorkA && (!ViewConfigurationTask.Default.NoTPData)) res.Add(GetRowType(typeof(ActualOvertimeWorkRow)));

            }
            else if (this is UpdateTimesheetsView)
            {
                if (ViewConfigurationRow.Default.ActualWorkA) res.Add(GetRowType(typeof(ActualWorkRow)));
                if (ViewConfigurationRow.Default.ActualOvertimeWorkA) res.Add(GetRowType(typeof(ActualOvertimeWorkRow)));
                if (ViewConfigurationRow.Default.ActualNonBillableWorkA) res.Add(GetRowType(typeof(NonBillableActualWorkRow)));
                if (ViewConfigurationRow.Default.ActualNonBillableOvertimeWorkA) res.Add(GetRowType(typeof(NonBillableOvertimeWorkRow)));
                res.Add(GetRowType(typeof(AdministrativeRow)));
            }
            RowTypes = res.ToArray();
        }

        public CustomFieldItem GetCustomField(string assignmentId, string customFieldName)
        {
            if (PeriodRows == null || string.IsNullOrEmpty(assignmentId) || string.IsNullOrEmpty(customFieldName))
            {
                return null;
            }
            else 
            {
                 var periodRow =PeriodRows.First(m=>m.Value.AssignementId == assignmentId).Value;
                 if (periodRow.RowType == 1)
                 {
                     if((periodRow as ActualWorkRow).CustomFieldItems != null && (periodRow as ActualWorkRow).CustomFieldItems.Any(t=>t.Name == customFieldName))
                     {
                         return (periodRow as ActualWorkRow).CustomFieldItems.First(t => t.Name == customFieldName);
                     }
                 }

                 if (periodRow.RowType == 6)
                 {
                     if ((periodRow as SingleValuesRow).CustomFieldItems != null && (periodRow as SingleValuesRow).CustomFieldItems.Any(t => t.Name == customFieldName))
                     {
                         return (periodRow as SingleValuesRow).CustomFieldItems.First(t => t.Name == customFieldName);
                     }
                 }
            }
            return null;
        }
    }
}