using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeSheetIBusiness;

namespace TimeSheetMobileWeb.Models
{
    public class TaskSelectionView
    {
        public string Title { get; set; }
        public IEnumerable<ProjectInfo> Projects { get; set; }
        public bool IsInTask { get; set; }
        public RowType[] RowTypes {get; set;}
        public RowType Admin { get; set; }
        public bool IsProjectlineType { get; set; }
       
        public void PrepareRowTypes()
        {
            List<RowType> res = new List<RowType>();
            if (IsInTask)
            {
                if (ViewConfigurationTask.Default.ActualWorkA && (!ViewConfigurationTask.Default.NoTPData)) res.Add(UpdateViewBase.GetRowType(typeof(ActualWorkRow)));
                if (ViewConfigurationTask.Default.ActualWorkA && ViewConfigurationTask.Default.NoTPData) res.Add(UpdateViewBase.GetRowType(typeof(SingleValuesRow)));
                if (ViewConfigurationTask.Default.ActualOvertimeWorkA && (!ViewConfigurationTask.Default.NoTPData)) res.Add(UpdateViewBase.GetRowType(typeof(ActualOvertimeWorkRow)));

            }
            else 
            {
                if (ViewConfigurationRow.Default.ActualWorkA) res.Add(UpdateViewBase.GetRowType(typeof(ActualWorkRow)));
                if (ViewConfigurationRow.Default.ActualOvertimeWorkA) res.Add(UpdateViewBase.GetRowType(typeof(ActualOvertimeWorkRow)));
                if (ViewConfigurationRow.Default.ActualNonBillableWorkA) res.Add(UpdateViewBase.GetRowType(typeof(NonBillableActualWorkRow)));
                if (ViewConfigurationRow.Default.ActualNonBillableOvertimeWorkA) res.Add(UpdateViewBase.GetRowType(typeof(NonBillableOvertimeWorkRow)));

                Admin = UpdateViewBase.GetRowType(typeof(AdministrativeRow));
            }
            
            RowTypes = res.ToArray();
        }
    }
}