using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TimeSheetMobileWeb.Models
{
    [MetadataType(typeof(TimeSheetView))]
    public class TimeSheetView
    {
        public string Period { get; set; }
        public string Status { get; set; }
        public string Hours { get; set; }
        public bool IsCreated { get; set;}
    }
}