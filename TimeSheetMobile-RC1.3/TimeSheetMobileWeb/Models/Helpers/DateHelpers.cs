using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeSheetMobileWeb.Models
{
    public static class DateHelpers
    {
        public static string DateRowLabel(DateTime start, int i)
        {
            start = start.AddDays(i);
            return start.ToString("ddd")+" ";
        }
    }
}