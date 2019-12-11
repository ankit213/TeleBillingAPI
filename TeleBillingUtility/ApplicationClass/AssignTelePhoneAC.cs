using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TeleBillingUtility.ApplicationClass
{
	public class AssignTelePhoneSP
	{
        public AssignTelePhoneSP()
        {
            AllocatePackageDetails = new List<AssignTelePhonePackageDetailAC>();

        }

        [JsonProperty("id")]
		public long Id { get; set; }

		[JsonProperty("telephonenumber")]
		public string TelephoneNumber { get; set; }

        [JsonProperty("addtionalinfo")]
        public string AdditionalInfo { get; set; }

		[JsonProperty("employeename")]
		public string EmployeeName { get; set; }

		[JsonProperty("emppfnumber")]
		public string EmpPFNumber { get; set; }

        [JsonProperty("emailid")]
        public string EmailId { get; set; }

        [JsonProperty("extensionno")]
        public string ExtensionNumber { get; set; }

        [JsonProperty("delegateuser")]
        public string DelegateUser { get; set; }

        [JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("department")]
		public string Department { get; set; }       

        [JsonProperty("assigntype")]
		public string AssignType { get; set; }

		[JsonProperty("costcenter")]
		public string CostCenter { get; set; }

		[JsonProperty("linestatus")]
		public string LineStatus { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }      

		[JsonProperty("isactive")]
		public long IsActive { get; set; }

        [JsonProperty("AllocatepackageDetails")]
        public List<AssignTelePhonePackageDetailAC> AllocatePackageDetails { get; set; }
 
	}
    public class AssignTelePhoneAC
    {
        public AssignTelePhoneAC()
        {
            AllocatePackageDetails = new List<AssignTelePhonePackageDetailAC>();

        }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("telephonenumber")]
        public string TelephoneNumber { get; set; }

        [JsonProperty("addtionalinfo")]
        public string AdditionalInfo { get; set; }

        [JsonProperty("employeename")]
        public string EmployeeName { get; set; }

        [JsonProperty("emppfnumber")]
        public string EmpPFNumber { get; set; }

        [JsonProperty("emailid")]
        public string EmailId { get; set; }

        [JsonProperty("extensionno")]
        public string ExtensionNumber { get; set; }

        [JsonProperty("delegateuser")]
        public string DelegateUser { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("department")]
        public string Department { get; set; }

        [JsonProperty("assigntype")]
        public string AssignType { get; set; }

        [JsonProperty("costcenter")]
        public string CostCenter { get; set; }

        [JsonProperty("linestatus")]
        public string LineStatus { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("isactive")]
        public bool IsActive {

            get { return (IsActive1 == 1 ? true : false); }
            set
            {
                IsActive = value;
            }
        }


        public long IsActive1 { get; set; }

        [JsonProperty("AllocatepackageDetails")]
        public List<AssignTelePhonePackageDetailAC> AllocatePackageDetails { get; set; }

    }
    public class AssignTelePhonePackageDetailAC
    {
        [JsonProperty("numberallocationid")]
        public long NumberAllocationId { get; set; }

        [JsonProperty("telephonenumber")]
        public string TelephoneNumber { get; set; }

        [JsonProperty("servicename")]
        public string ServiceName { get; set; }

        [JsonProperty("packagename")]
        public string PackageName { get; set; }

        [JsonProperty("packageamount")]
        public string PackageAmount { get; set; }

        [JsonProperty("startdate")]
        public string StartDate { get; set; }

        [JsonProperty("enddate")]
        public string EndDate { get; set; }

        [JsonProperty("packagecurrentstatus")]
        public string PackageCurrentStatus { get; set; }

        
    }
 }
