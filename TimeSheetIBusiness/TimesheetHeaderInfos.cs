using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVCControlsToolkit.DataAnnotations;

namespace TimeSheetIBusiness
{
    public class TimesheetHeaderInfos
    {
        [Format(Postfix="", Prefix="", NullDisplayText="", ClientFormat="n")]
        public decimal? TotalActualWork { get; set; }
        [Format(Postfix = "", Prefix = "", NullDisplayText = "", ClientFormat = "n")]
        public decimal? TotalOverTimeWork { get; set; }
        [Format(Postfix = "", Prefix = "", NullDisplayText = "", ClientFormat = "n")]
        public decimal? TotalNonBillable { get; set; }
        [Format(Postfix = "", Prefix = "", NullDisplayText = "", ClientFormat = "n")]
        public decimal? TotalNonBillableOvertime { get; set; }
        public string Name { get; set; }
        public string Comments { get; set; }
        public int? Status { get; set; }


        public Guid TSUID { get; set; }
    }
}
