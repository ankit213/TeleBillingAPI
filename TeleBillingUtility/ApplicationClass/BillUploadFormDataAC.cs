using Microsoft.AspNetCore.Http;

namespace TeleBillingUtility.ApplicationClass
{
    public class BillUploadFormDataAC
    {
        public string BillUploadAc { get; set; }

        public IFormFile File { get; set; }

        public IFormFile Filecisco { get; set; }

        public IFormFile Fileavaya { get; set; }
    }
}
