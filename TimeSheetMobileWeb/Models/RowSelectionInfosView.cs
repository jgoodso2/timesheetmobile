using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeSheetMobileWeb.Models
{
    public class RowSelectionInfosView
    {
        public string RequiredPeriodId {get;set;}
        public DateTime RequiredPeriodIStart {get;set;}
        public DateTime RequiredPeriodIStop {get;set;} 
        public string RequiredProgectId {get;set;}
        public string RequiredAssignementId { get; set; }
        public string RequiredProjectName { get; set; }
        public string RequiredAssignementName { get; set; }
        public string RequiredRowType { get; set; }
        public string RequiredLineClassId { get; set; }
        public string RequiredLineClassName { get; set; }
    }
}