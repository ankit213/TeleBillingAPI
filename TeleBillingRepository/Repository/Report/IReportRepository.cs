﻿using JqueryDataTables.ServerSide.AspNetCoreWeb.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingRepository.Repository.Report
{
   public  interface IReportRepository
    {
        /// <summary>
        /// GetOperatorCallLogSearchReportList
        /// </summary>
        /// <param name="param">JqueryDataWithExtraParameterAC Data as param</param>
        /// <param name="userId"></param>
        /// <returns></returns>
        JqueryDataTablesPagedResults<OperatorLogReportAC> GetOperatorCallLogSearchReportList(JqueryDataWithExtraParameterAC param, long userId);

        /// <summary>
        /// ExportOperatorLogReportList pass serach parameters
        /// </summary>
        /// <param name="param"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// 
        List<OperatorLogReportAC> ExportOperatorLogReportList(SearchReportAC param, long userId);

        /// <summary>
        /// GetProviderBillSearchReportList
        /// </summary>
        /// <param name="param">JqueryDataWithExtraParameterAC Data as param</param>
        /// <param name="userId"></param>
        /// <returns></returns>
        JqueryDataTablesPagedResults<ProviderBillReportAC> GetProviderBillSearchReportList(JqueryDataWithExtraParameterAC param, long userId);
         /// <summary>
        /// ExportProviderBillReportList
        /// </summary>
        /// <param name="param"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<ProviderBillReportAC> ExportProviderBillReportList(SearchReportAC param, long userId);

        /// <summary>
        /// GetAuditActivitySearchReportList
        /// </summary>
        /// <param name="param"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        JqueryDataTablesPagedResults<AuditLogReportAC> GetAuditActivitySearchReportList(JqueryDataWithExtraParameterAC param, long userId);

        /// <summary>
        /// ExportAuditLogReportList
        /// </summary>
        /// <param name="param"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<AuditLogReportAC> ExportAuditLogReportList(SearchReportAC param, long userId);

        /// <summary>
        /// GetReimbursementBillSearchReportList
        /// </summary>
        /// <param name="param"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        JqueryDataTablesPagedResults<ReimbursementBillReportAC> GetReimbursementBillSearchReportList(JqueryDataWithExtraParameterAC param, long userId);
      
        /// <summary>
        /// ExportReimbursementBillReportList
        /// </summary>
        /// <param name="param"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<ReimbursementBillReportAC> ExportReimbursementBillReportList(SearchReportAC param, long userId);

        /// <summary>
        /// GetAccountBillSearchReportList
        /// </summary>
        /// <param name="param"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        JqueryDataTablesPagedResults<AccountBillReportAC> GetAccountBillSearchReportList(JqueryDataWithExtraParameterAC param, long userId);

        /// <summary>
        /// ExportAccountBillReportList
        /// </summary>
        /// <param name="param"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<AccountBillReportAC> ExportAccountBillReportList(SearchReportAC param, long userId);

        /// <summary>
        /// ViewAccountBillDetails
        /// </summary>
        /// <param name="empBillId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<AccountBillDetailsAC> ViewAccountBillDetails(long empBillId, long userId);

		/// <summary>
		/// This function used for get account memo report
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		JqueryDataTablesPagedResults<MemoReportListAC> GetAccountMemoReport(JqueryDataWithExtraParameterAC param);

		/// <summary>
		/// This method used for export the memo report list
		/// </summary>
		/// <param name="searchReportAC"></param>
		/// <returns></returns>
		List<MemoReportListAC> ExportAccountMemoReportList(SearchReportAC searchReportAC);

		/// <summary>
		/// This function used for get view memo bill detais list. 
		/// </summary>
		/// <param name="memoId"></param>
		/// <returns></returns>
		List<MemoBillDetailAC> ViewMemoBillsDetail(long memoId);
		
		/// <summary>
		/// This method used for get transfrreddeactive report
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		JqueryDataTablesPagedResults<TransferDeActivatedReportAC> GetTransferredDeactivatedReport(JqueryDataTablesParameters param);

		/// <summary>
		/// This method used for export transfrreddeactive report
		/// </summary>
		/// <param name="searchReportAC"></param>
		/// <returns></returns>
		List<TransferDeActivatedReportAC> ExportTransferredDeactivatedList(SearchReportAC searchReportAC);

		/// <summary>
		/// This method used for get vendor wise package detail report. 
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		JqueryDataTablesPagedResults<VendorWisePackageDetailsAC> VendorWisePackageDetailReport(JqueryDataTablesParameters param);
		
		/// <summary>
		/// This method used for export vendor package detail
		/// </summary>
		/// <param name="searchReportAC"></param>
		/// <returns></returns>
		List<PackageDetailListAC> ExportVendorWisePackageDetailList(SearchReportAC searchReportAC);
	}
}
