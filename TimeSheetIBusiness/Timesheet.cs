using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeSheetIBusiness
{
    public enum TimesheetsSets{Default=0, Last3=1, Next6Last3=2, Last6=3, Last12=4, CreatedProgress=5, All=6}
    public class Timesheet
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Start { get; set; }
        public DateTime Stop { get; set; }
        public string Period { get; set; }
        public string Status { get; set; }
        public string Hours { get; set; }
        public bool IsCreated { get; set; }
        public string Description
        {
            get
            {
                return string.Format("({1:d} - {2:d}) {0}", Name, Start, Stop);
            }
        }
        public string Value
        {
            get
            {
                return string.Format("{0}#{1:s}#{2:s}#{3}", Id, Start, Stop, Convert.ToInt32(Stop.Subtract(Start).TotalDays) + 1);
            }
        }
    }
}
