using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.IO;

namespace TimeSheetIBusiness
{
    [Serializable]
    public class ViewConfigurationBase
    {
        internal const string NoName = "No Name"; 
        // Name and Id
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        //no TimephasedData
        public bool NoTPData { get; set; }
        //assignement
        public bool WorkA { get; set; }
        public bool WorkA_Edit { get; set; }
        public string WorkA_Name { get; set; }
        public string WorkA_FullName { get; set; }
        
        public bool RegularWorkA {get; set;}
        public bool RegularWorkA_Edit { get; set; }
        public string RegularWorkA_Name { get; set; }
        public string RegularWorkA_FullName { get; set; }

        public bool OvertimeWorkA {get; set;}
        public bool OvertimeWorkA_Edit { get; set; }
        public string OvertimeWorkA_Name { get; set; }
        public string OvertimeWorkA_FullName { get; set; }

        public bool RemainingWorkA {get; set;}
        public bool RemainingWorkA_Edit { get; set; }
        public string RemainingWorkA_Name { get; set; }
        public string RemainingWorkA_FullName { get; set; }

        public bool ActualWorkA {get; set;}
        public string ActualWorkA_Name { get; set; }
        public string ActualWorkA_FullName { get; set; }

        public bool ActualOvertimeWorkA {get; set;}
        public string ActualOvertimeWorkA_Name { get; set; }
        public string ActualOvertimeWorkA_FullName { get; set; }
        
        public bool StartA {get; set;}
        public bool StartA_Edit { get; set; }
        public string StartA_Name { get; set; }
        public string StartA_FullName { get; set; }

        public bool FinishA {get; set;}
        public bool FinishA_Edit { get; set; }
        public string FinishA_Name { get; set; }
        public string FinishA_FullName { get; set; }

        public bool ActualStartA {get; set;}
        public bool ActualStartA_Edit { get; set; }
        public string ActualStartA_Name { get; set; }
        public string ActualStartA_FullName { get; set; }

        public bool ActualFinishA {get; set;}
        public bool ActualFinishA_Edit { get; set; }
        public string ActualFinishA_Name { get; set; }
        public string ActualFinishA_FullName { get; set; }

        public bool PercentWorkCompleteA {get; set;}
        public bool PercentWorkCompleteA_Edit { get; set; }
        public string PercentWorkCompleteA_Name { get; set; }
        public string PercentWorkCompleteA_FullName { get; set; }

        public bool AssignmentUnitsA { get; set; }
        public bool AssignmentUnitsA_Edit { get; set; }
        public string AssignmentUnitsA_Name { get; set; }
        public string AssignmentUnitsA_FullName { get; set; }

        public bool ConfirmedA {get; set;}
        public bool ConfirmedA_Edit { get; set; }
        public string ConfirmedA_Name { get; set; }
        public string ConfirmedA_FullName { get; set; }

        public bool CommentsA { get; set; }
        public bool CommentsA_Edit { get; set; }
        public string CommentsA_Name { get; set; }
        public string CommentsA_FullName { get; set; }

        //Tasks
        public bool WorkT { get; set; }
        public bool WorkT_Edit { get; set; }
        public string WorkT_Name { get; set; }
        public string WorkT_FullName { get; set; }

        public bool RegularWorkT { get; set; }
        public bool RegularWorkT_Edit { get; set; }
        public string RegularWorkT_Name { get; set; }
        public string RegularWorkT_FullName { get; set; }

        public bool OvertimeWorkT { get; set; }
        public bool OvertimeWorkT_Edit { get; set; }
        public string OvertimeWorkT_Name { get; set; }
        public string OvertimeWorkT_FullName { get; set; }

        public bool RemainingWorkT { get; set; }
        public bool RemainingWorkT_Edit { get; set; }
        public string RemainingWorkT_Name { get; set; }
        public string RemainingWorkT_FullName { get; set; }

        public bool RemainingOvertimeWorkT { get; set; }
        public bool RemainingOvertimeWorkT_Edit { get; set; }
        public string RemainingOvertimeWorkT_Name { get; set; }
        public string RemainingOvertimeWorkT_FullName { get; set; }

        public bool ActualWorkT { get; set; }
        public bool ActualWorkT_Edit { get; set; }
        public string ActualWorkT_Name { get; set; }
        public string ActualWorkT_FullName { get; set; }

        public bool StartT { get; set; }
        public bool StartT_Edit { get; set; }
        public string StartT_Name { get; set; }
        public string StartT_FullName { get; set; }

        public bool FinishT { get; set; }
        public bool FinishT_Edit { get; set; }
        public string FinishT_Name { get; set; }
        public string FinishT_FullName { get; set; }

        public bool ResumeT { get; set; }
        public bool ResumeT_Edit { get; set; }
        public string ResumeT_Name { get; set; }
        public string ResumeT_FullName { get; set; }

        public bool DeadlineT { get; set; }
        public bool DeadlineT_Edit { get; set; }
        public string DeadlineT_Name { get; set; }
        public string DeadlineT_FullName { get; set; }

        public bool DurationT { get; set; }
        public bool DurationT_Edit { get; set; }
        public string DurationT_Name { get; set; }
        public string DurationT_FullName { get; set; }

        public bool RemainingDurationT { get; set; }
        public bool RemainingDurationT_Edit { get; set; }
        public string RemainingDurationT_Name { get; set; }
        public string RemainingDurationT_FullName { get; set; }

        public bool TaskNameT { get; set; }
        public bool TaskNameT_Edit { get; set; }
        public string TaskNameT_Name { get; set; }
        public string TaskNameT_FullName { get; set; }

        public bool PercentCompleteT { get; set; }
        public bool PercentCompleteT_Edit { get; set; }
        public string PercentCompleteT_Name { get; set; }
        public string PercentCompleteT_FullName { get; set; }

        public bool PercentWorkCompleteT { get; set; }
        public bool PercentWorkCompleteT_Edit { get; set; }
        public string PercentWorkCompleteT_Name { get; set; }
        public string PercentWorkCompleteT_FullName { get; set; }

        public bool PhysicalPercentCompleteT { get; set; }
        public bool PhysicalPercentCompleteT_Edit { get; set; }
        public string PhysicalPercentCompleteT_Name { get; set; }
        public string PhysicalPercentCompleteT_FullName { get; set; }

        public ViewConfigurationBase()
        {
            WorkA_Name = NoName;
            WorkA_FullName = NoName;
            RegularWorkA_Name = NoName;
            RegularWorkA_FullName = NoName;
            OvertimeWorkA_Name = NoName;
            OvertimeWorkA_FullName = NoName;
            RemainingWorkA_Name = NoName;
            RemainingWorkA_FullName = NoName;
            ActualWorkA_Name = NoName;
            ActualWorkA_FullName = NoName;
            ActualOvertimeWorkA_Name = NoName;
            ActualOvertimeWorkA_FullName = NoName;
            StartA_Name = NoName;
            StartA_FullName = NoName;
            FinishA_Name = NoName;
            FinishA_FullName = NoName;
            ActualStartA_Name = NoName;
            ActualStartA_FullName = NoName;
            ActualFinishA_Name = NoName;
            ActualFinishA_FullName = NoName;
            PercentWorkCompleteA_Name = NoName;
            PercentWorkCompleteA_FullName = NoName;
            AssignmentUnitsA_Name = NoName;
            AssignmentUnitsA_FullName = NoName;
            ConfirmedA_Name = NoName;
            ConfirmedA_FullName = NoName;
            CommentsA_Name = NoName;
            CommentsA_FullName = NoName;
            WorkT_Name = NoName;
            WorkT_FullName = NoName;
            RegularWorkT_Name = NoName;
            RegularWorkT_FullName = NoName;
            OvertimeWorkT_Name = NoName;
            OvertimeWorkT_FullName = NoName;
            RemainingWorkT_Name = NoName;
            RemainingOvertimeWorkT_Name = NoName;
            ActualWorkT_Name = NoName;
            ActualWorkT_FullName = NoName;
            StartT_Name = NoName;
            StartT_FullName = NoName;
            FinishT_Name = NoName;
            FinishT_FullName = NoName;
            ResumeT_Name = NoName;
            ResumeT_FullName = NoName;
            DeadlineT_Name = NoName;
            DeadlineT_FullName = NoName;
            DurationT_Name = NoName;
            DurationT_FullName = NoName;
            RemainingDurationT_Name = NoName;
            RemainingDurationT_FullName = NoName;
            TaskNameT_Name = NoName;
            TaskNameT_FullName = NoName;
            PercentCompleteT_Name = NoName;
            PercentCompleteT_FullName = NoName;
            PercentWorkCompleteT_Name = NoName;
            PercentWorkCompleteT_FullName = NoName;
            PhysicalPercentCompleteT_Name = NoName;
            PhysicalPercentCompleteT_FullName = NoName;
        }
        
    }
    [Serializable]
    public class ViewConfigurationRow: ViewConfigurationBase
    {
        public bool ActualNonBillableWorkA { get; set; }
        public string ActualNonBillableWorkA_Name { get; set; }
        public string ActualNonBillableWorkA_FullName { get; set; }

        public bool ActualNonBillableOvertimeWorkA { get; set; }
        public string ActualNonBillableOvertimeWorkA_Name { get; set; }
        public string ActualNonBillableOvertimeWorkA_FullName { get; set; }
        public string AdminDescription { get; set; }
        public static ViewConfigurationRow[] All { get; set; }
        [ThreadStatic]
        public static ViewConfigurationRow Default;
        public static string ViewFieldName;
        public static Guid ViewFieldGuid;
        public ViewConfigurationRow()
            : base()
        {
            ActualNonBillableWorkA_Name = NoName;
            ActualNonBillableWorkA_FullName = NoName;
            ActualNonBillableOvertimeWorkA_Name = NoName;
            ActualNonBillableOvertimeWorkA_FullName = NoName;
        }
        public static ViewConfigurationRow Find(string id)
        { 
            foreach (var c in All)
            {
                if (c.Id == id) return c;
            }
            return null;
        }
    }
    [Serializable]
    public class ViewConfigurationTask : ViewConfigurationBase
    {
        public static ViewConfigurationTask[] All { get; set; }
        [ThreadStatic]
        public static ViewConfigurationTask Default;
        
        public static string ViewFieldName;
        public static Guid ViewFieldGuid;
        public static ViewConfigurationTask Find(string id)
        {
            foreach (var c in All)
            {
                if (c.Id == id) return c;
            }
            return null;
        }
        public ViewConfigurationTask()
            : base()
        {

        }
    }
    
}