using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeleBillingUtility.ApplicationClass
{
    public class ExcelFileAC
    {
        public IFormFile File { get; set; }
        public string FolderName { get; set; }
    }
}
