using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using MVCControlsToolkit.DataAnnotations;

namespace TimeSheetIBusiness
{
    public class BaseRow
    {
        public List<decimal?> DayTimes { get; set; }
        public string AssignementId { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string AssignementName { get; set; }
        public int RowType { get; set; }
        public override bool Equals(object obj)
        {
            BaseRow other = obj as BaseRow;
            if (other == null) return false;
            else return this.AssignementId == other.AssignementId && this.ProjectId == other.ProjectId && other.RowType == this.RowType;
        }
        public override int GetHashCode()
        {
            if (AssignementId == null) return 0;
            return AssignementId.GetHashCode();
        }
        public BaseRow GetCopy()
        {
            return this.MemberwiseClone() as BaseRow;
        }
    }
    public class NonBillableActualWorkRow : BaseRow
    {
        public NonBillableActualWorkRow() { RowType = 3; }
    }
    public class NonBillableOvertimeWorkRow : BaseRow
    {
        public NonBillableOvertimeWorkRow () { RowType = 4; }
    }
    public class AdministrativeRow : BaseRow
    {
        public AdministrativeRow () { RowType = 5; }
    }
    public class ActualWorkRow : BaseRow
    {
        
        public ActualWorkRow() { RowType = 1; }
        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? WorkA { get; set; }
        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? RegularWorkA { get; set; }
        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? RemainingWorkA { get; set; }
        [DateRange(DynamicMaximum = "FinishA"), Format(typeof(ModelsResources), "", "", "EmptyDate", ClientFormat = "d")]
        public DateTime? StartA { get; set; }
        [DateRange(DynamicMinimum = "StartA"), Format(typeof(ModelsResources), "", "", "EmptyDate", ClientFormat = "d")]
        public DateTime? FinishA { get; set; }
        [DateRange(DynamicMaximum = "ActualFinishA"), Format(typeof(ModelsResources), "", "", "EmptyDate", ClientFormat = "d")]
        public DateTime? ActualStartA { get; set; }
        [DateRange(DynamicMinimum = "ActualStartA"), Format(typeof(ModelsResources), "", "", "EmptyDate", ClientFormat = "d")]
        public DateTime? ActualFinishA { get; set; }

        [Range(0, 100), Format(Postfix="%")]
        public uint? PercentWorkCompleteA { get; set; }
        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? AssignmentUnitsA { get; set; }

        public bool? ConfirmedA { get; set; }

        public string CommentsA { get; set; }

        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? WorkT { get; set; }
        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? RegularWorkT { get; set; }
        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? RemainingWorkT { get; set; }
        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? ActualWorkT { get; set; }

        [DateRange(DynamicMaximum = "FinishT"), Format(typeof(ModelsResources), "", "", "EmptyDate", ClientFormat = "d")]
        public DateTime? StartT { get; set; }
        [DateRange(DynamicMinimum = "StartT"), Format(typeof(ModelsResources), "", "", "EmptyDate", ClientFormat = "d")]
        public DateTime? FinishT { get; set; }
        [DateRange(DynamicMinimum = "StartT"), Format(typeof(ModelsResources), "", "", "EmptyDate", ClientFormat = "d")]
        public DateTime? ResumeT { get; set; }
        [DateRange(DynamicMinimum = "StartT"), Format(typeof(ModelsResources), "", "", "EmptyDate", ClientFormat = "d")]
        public DateTime? DeadlineT { get; set; }
        [DynamicRange(typeof(uint), SMinimum = "0")]
        public uint? DurationT { get; set; }
        [DynamicRange(typeof(uint), SMinimum = "0")]
        public uint? RemainingDurationT { get; set; }

        public string TaskNameT { get; set; }

        [Range(0, 100), Format(Postfix = "%")]
        public uint? PercentCompleteT { get; set; }
        [Range(0, 100), Format(Postfix = "%")]
        public uint? PercentWorkCompleteT { get; set; }
        [Range(0, 100), Format(Postfix = "%")]
        public uint? PhysicalPercentCompleteT { get; set; }
    }
    public class ActualOvertimeWorkRow : BaseRow
    {
        public ActualOvertimeWorkRow() { RowType = 2; }
        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? OvertimeWorkA { get; set; }
        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? OvertimeWorkT { get; set; }
        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? RemainingOvertimeWorkT { get; set; }
    }


    public class SingleValuesRow : BaseRow
    {

        public SingleValuesRow() { RowType = 6; }
        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? WorkA { get; set; }
        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? RegularWorkA { get; set; }
        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? RemainingWorkA { get; set; }
        [DateRange(DynamicMaximum = "FinishA"), Format(typeof(ModelsResources), "", "", "EmptyDate", ClientFormat = "d")]
        public DateTime? StartA { get; set; }
        [DateRange(DynamicMinimum = "StartA"), Format(typeof(ModelsResources), "", "", "EmptyDate", ClientFormat = "d")]
        public DateTime? FinishA { get; set; }
        [DateRange(DynamicMaximum = "ActualFinishA"), Format(typeof(ModelsResources), "", "", "EmptyDate", ClientFormat = "d")]
        public DateTime? ActualStartA { get; set; }
        [DateRange(DynamicMinimum = "ActualStartA"), Format(typeof(ModelsResources), "", "", "EmptyDate", ClientFormat = "d")]
        public DateTime? ActualFinishA { get; set; }

        [Range(0, 100), Format(Postfix = "%")]
        public uint? PercentWorkCompleteA { get; set; }
        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? AssignmentUnitsA { get; set; }

        public bool? ConfirmedA { get; set; }

        public string CommentsA { get; set; }

        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? WorkT { get; set; }
        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? RegularWorkT { get; set; }
        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? RemainingWorkT { get; set; }
        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? ActualWorkT { get; set; }

        [DateRange(DynamicMaximum = "FinishT"), Format(typeof(ModelsResources), "", "", "EmptyDate", ClientFormat = "d")]
        public DateTime? StartT { get; set; }
        [DateRange(DynamicMinimum = "StartT"), Format(typeof(ModelsResources), "", "", "EmptyDate", ClientFormat = "d")]
        public DateTime? FinishT { get; set; }
        [DateRange(DynamicMinimum = "StartT"), Format(typeof(ModelsResources), "", "", "EmptyDate", ClientFormat = "d")]
        public DateTime? ResumeT { get; set; }
        [DateRange(DynamicMinimum = "StartT"), Format(typeof(ModelsResources), "", "", "EmptyDate", ClientFormat = "d")]
        public DateTime? DeadlineT { get; set; }
        [DynamicRange(typeof(uint), SMinimum = "0")]
        public uint? DurationT { get; set; }
        [DynamicRange(typeof(uint), SMinimum = "0")]
        public uint? RemainingDurationT { get; set; }

        public string TaskNameT { get; set; }

        [Range(0, 100), Format(Postfix = "%")]
        public uint? PercentCompleteT { get; set; }
        [Range(0, 100), Format(Postfix = "%")]
        public uint? PercentWorkCompleteT { get; set; }
        [Range(0, 100), Format(Postfix = "%")]
        public uint? PhysicalPercentCompleteT { get; set; }

        
        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? OvertimeWorkA { get; set; }
        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? OvertimeWorkT { get; set; }
        [DynamicRange(typeof(decimal), SMinimum = "0.0")]
        public decimal? RemainingOvertimeWorkT { get; set; }
    }
}
