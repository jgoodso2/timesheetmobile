using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeSheetMobileWeb.Models
{
    public class TimeInputView
    {
        public DateTime CurrDate { get; set; }
        public DateTime MaxDate { get; set; }
        public DateTime MinDate { get; set; }
    }
}