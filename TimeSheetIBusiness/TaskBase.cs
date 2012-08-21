using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeSheetIBusiness
{
    public class TaskBase
    {
        public string ProjectId { get; set; }
        public string AssignementId { get; set; }
    }
    
    public class TaskWorkUnit
    {
        public DateTime Day { get; set; }
        public uint Hours { get; set; }
    }
    public class TaskTimes
    {
        public List<TaskWorkUnit> WorkUnits { get; set; }
        public uint RemainigTime { get; set; }
    }
    public class TaskInfos : TaskBase
    {
        
    }
}
