using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeSheetIBusiness;

namespace TimeSheetMobileWeb.Models
{
    public class LookupTableDisplayItemsView
    {
        public List<LookupTableDisplayItem> LookupTableItems { get; set; }
        public string Name { get; set; }
    }
}