using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeSheetMobileWeb.Models
{
    public class RecallDeleteView
    {
        public string RDPeriodId { get; set; }
        public DateTime RDPeriodIStart { get; set; }
        public DateTime RDPeriodIStop { get; set; }
        public bool IsRecall { get; set; }
        public bool IsTask { get; set; }

        public RecallDeleteView()
        {
            RDPeriodId = Guid.NewGuid().ToString();
        }

        public string ErrorMessage { get; set; }
    }
}