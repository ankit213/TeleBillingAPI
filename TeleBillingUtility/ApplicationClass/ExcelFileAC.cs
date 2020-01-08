using Microsoft.AspNetCore.Http;

namespace TeleBillingUtility.ApplicationClass
{
    public class ExcelFileAC
    {
        public IFormFile File { get; set; }
        public string FolderName { get; set; }
    }
}
