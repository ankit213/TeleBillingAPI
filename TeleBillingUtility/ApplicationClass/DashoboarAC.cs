using Newtonsoft.Json;
using System.Collections.Generic;

namespace TeleBillingUtility.ApplicationClass
{
    public class DashoboarAC
    {

    }

    public class UserDashoboarAC
    {
        public UserDashoboarAC()
        {
            userMobileBills = new List<UserMobileBillAC>();
            PieChartDataList = new List<PieChartAC>();
            SkypeMocDatalist = new List<UserSkypeMocDataAC>();
            StaffEmployeeList = new List<StaffEmployeeAC>();
            userCurrentBills = new List<UserMobileBillAC>();
        }

        [JsonProperty("userid")]
        public long UserId { get; set; }

        [JsonProperty("usermobilebills")]
        public List<UserMobileBillAC> userMobileBills { get; set; }

        [JsonProperty("piechartdatalist")]
        public List<PieChartAC> PieChartDataList { get; set; }

        [JsonProperty("skypemocdatalist")]
        public List<UserSkypeMocDataAC> SkypeMocDatalist { get; set; }

        [JsonProperty("staffemployeeslist")]
        public List<StaffEmployeeAC> StaffEmployeeList { get; set; }


        [JsonProperty("usercurrentbills")]
        public List<UserMobileBillAC> userCurrentBills { get; set; }
    }



    public class UserMobileBillAC
    {

        public UserMobileBillAC()
        {
            TransTypeWiseTotalList = new List<UsertransTypeTotalAC>();
        }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("telephonenumber")]
        public string TelephoneNumber { get; set; }

        [JsonProperty("billdate")]
        public string BillDate { get; set; }

        [JsonProperty("billamount")]
        public string BillAmount { get; set; }

        [JsonProperty("packageamount")]
        public string PackageAmount { get; set; }

        [JsonProperty("deductionamount")]
        public string DeductionAmount { get; set; }

        [JsonProperty("empbillstatus")]
        public string EmpBillStatus { get; set; }

        [JsonProperty("billduedate")]
        public string BillDueDate { get; set; }

        [JsonProperty("transtypewisetotallist")]
        public List<UsertransTypeTotalAC> TransTypeWiseTotalList { get; set; }
    }
    public class UserSkypeMocDataAC
    {

        [JsonProperty("userid")]
        public long UserId { get; set; }

        [JsonProperty("servicetype")]
        public string ServiceType { get; set; }

        [JsonProperty("billdate")]
        public string BillDate { get; set; }

        [JsonProperty("totalcall")]
        public long TotalCall { get; set; }

        [JsonProperty("totalamount")]
        public decimal TotalAmount { get; set; }

    }

    public class StaffEmployeeAC
    {

        [JsonProperty("userid")]
        public long UserId { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("name")]
        public string FullName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("designation")]
        public string Designation { get; set; }

        [JsonProperty("emppfnumber")]
        public string EmpPfnumber { get; set; }

        [JsonProperty("extensionnumber")]
        public string ExtensionNumber { get; set; }


        [JsonProperty("department")]
        public string Department { get; set; }

        [JsonProperty("businessunit")]
        public string BusinessUnit { get; set; }

        [JsonProperty("costcenter")]
        public string CostCenter { get; set; }

    }

    public class UsertransTypeTotalAC
    {
        [JsonProperty("empbillid")]
        public long EmpBillId { get; set; }

        [JsonProperty("transtype")]
        public string TransType { get; set; }

        [JsonProperty("transtypetotal")]
        public decimal TranstypeTotal { get; set; }
    }

    public class PieChartAC
    {
        public PieChartAC()
        {
            dataList = new List<string>();
            datalistvalues = new List<DataListValue>();
        }

        [JsonProperty("telephonenumber")]
        public string TelephoneNumber { get; set; }


        [JsonProperty("datalist")]
        public List<string> dataList { get; set; }

        [JsonProperty("dataarray")]
        public string[] dataArray { get; set; }

        [JsonProperty("datalistvalues")]
        public List<DataListValue> datalistvalues { get; set; }

    }
    public class DataListValue
    {
        [JsonProperty("value")]
        public decimal Value { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

    }

}
