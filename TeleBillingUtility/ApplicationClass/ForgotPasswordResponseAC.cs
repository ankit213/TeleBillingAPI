namespace TeleBillingUtility.ApplicationClass
{
    public class ForgotPasswordResponseAC
    {
        public string ReceiverEmail { get; set; }
        public string ReceiverName { get; set; }
        public string ForEmployee { get; set; }
        public string ForPFNumber { get; set; }
        public string RestPasswordUrl { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
    }
}
