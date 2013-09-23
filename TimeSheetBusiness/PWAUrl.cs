using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace TimeSheetBusiness
{
    static class PWAUrl
    {
        public static string GetCurrentPwaURL()
        {
            return HttpContext.Current.Items["PWAURL"].ToString();
        }
    }
}
