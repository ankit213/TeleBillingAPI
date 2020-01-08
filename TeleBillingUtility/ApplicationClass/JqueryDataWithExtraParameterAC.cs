using JqueryDataTables.ServerSide.AspNetCoreWeb.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TeleBillingUtility.ApplicationClass
{
    public class JqueryDataWithExtraParameterAC
    {
        JqueryDataWithExtraParameterAC()
        {
            Month = 0;
            Year = 0;
            ProviderId = 0;
            StatusId = 0;
            DateFrom = 0;
            DateTo = 0;
            EmployeeName = string.Empty;
            ActionId = 0;
            UserId = 0;
            BusinessUnitId = 0;
            CostCenterId = 0;

        }

        [JsonProperty("datatablesparameters")]
        public JqueryDataTablesParameters DataTablesParameters { get; set; }

        [JsonProperty("monthid")]
        public int Month { get; set; }

        [JsonProperty("yearid")]
        public int Year { get; set; }

        [JsonProperty("providerid")]
        public long ProviderId { get; set; }

        [JsonProperty("statusid")]
        public int StatusId { get; set; }

        // New Fileds for Other Report


        [JsonProperty("datefrom")]
        public int DateFrom { get; set; }

        [JsonProperty("dateto")]
        public int DateTo { get; set; }

        [JsonProperty("employeename")]
        public string EmployeeName { get; set; }

        [JsonProperty("end")]
        public DateTime? end { get; set; }

        [JsonProperty("start")]
        public DateTime? start { get; set; }

        [JsonProperty("actionid")]
        public int ActionId { get; set; }

        [JsonProperty("userid")]
        public long UserId { get; set; }

        [JsonProperty("businessunitid")]
        public long BusinessUnitId { get; set; }

        [JsonProperty("costcenterid")]
        public long CostCenterId { get; set; }

    }


    public class SearchReportAC
    {
        public SearchReportAC()
        {
            Month = 0;
            Year = 0;
            ProviderId = 0;
            StatusId = 0;
            DateFrom = 0;
            DateTo = 0;
            EmployeeName = string.Empty;
            SearchValue = string.Empty;
            ActionId = 0;
            UserId = 0;
            BusinessUnitId = 0;
            CostCenterId = 0;
            MultipleItems = new List<DrpResponseAC>();
        }

        [JsonProperty("monthid")]
        public int Month { get; set; }

        [JsonProperty("yearid")]
        public int Year { get; set; }

        [JsonProperty("providerid")]
        public long ProviderId { get; set; }

        [JsonProperty("statusid")]
        public int StatusId { get; set; }

        [JsonProperty("datefrom")]
        public int DateFrom { get; set; }

        [JsonProperty("dateto")]
        public int DateTo { get; set; }

        [JsonProperty("employeename")]
        public string EmployeeName { get; set; }

        [JsonProperty("end")]
        public DateTime? end { get; set; }

        [JsonProperty("start")]
        public DateTime? start { get; set; }

        [JsonProperty("searchvalue")]
        public string SearchValue { get; set; }

        [JsonProperty("actionid")]
        public int ActionId { get; set; }

        [JsonProperty("userid")]
        public long UserId { get; set; }

        [JsonProperty("businessunitid")]
        public long BusinessUnitId { get; set; }

        [JsonProperty("costcenterid")]
        public long CostCenterId { get; set; }

        [JsonProperty("multipleitems")]
        public List<DrpResponseAC> MultipleItems { get; set; }

    }

    public class ErrorExportAc
    {

        public ErrorExportAc()
        {
            fileGuidNo = string.Empty;
            Mode = 0;
        }

        [JsonProperty("fileguidno")]
        public string fileGuidNo { get; set; }

        [JsonProperty("mode")]
        public int Mode { get; set; }
    }
}
