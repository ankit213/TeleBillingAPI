using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
  public  class ExcelUploadClassAc
    {
    }

    public class WataniyaCSVAc
    {

        public string CallerNumber { get; set; }
        public string CallType { get; set; }
        public string Description { get; set; }
        public DateTime CallDate { get; set; }
        public decimal Duration { get; set; }
        public decimal CallAmount { get; set; }
        public decimal CallDataKB { get; set; }
        public string MessageCount { get; set; }
        public string ExtraColum { get; set; }

    }


}
