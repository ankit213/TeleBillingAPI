using AutoMapper;
using JqueryDataTables.ServerSide.AspNetCoreWeb.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TeleBillingRepository.Service.Constants;
using TeleBillingRepository.Service.LogMangement;
using TeleBillingUtility.ApplicationClass;
using TeleBillingUtility.Helpers;
using TeleBillingUtility.Helpers.CommonFunction;
using TeleBillingUtility.Helpers.Enums;
using TeleBillingUtility.Models;

namespace TeleBillingRepository.Repository.Report
{
	public class ReportRepository : IReportRepository
	{
		#region "Private Variable(s)"
		private readonly telebilling_v01Context _dbTeleBilling_V01Context;
		private readonly ILogManagement _iLogManagement;
		private readonly IStringConstant _iStringConstant;
		private IMapper _mapper;
		private readonly DAL _objDal = new DAL();
		private readonly DALMySql _objDalmysql = new DALMySql();
		#endregion

		#region "Constructor"
		public ReportRepository(telebilling_v01Context dbTeleBilling_V01Context, IMapper mapper, IStringConstant iStringConstant,
			ILogManagement iLogManagement)
		{
			_dbTeleBilling_V01Context = dbTeleBilling_V01Context;
			_iStringConstant = iStringConstant;
			_iLogManagement = iLogManagement;
			_mapper = mapper;
		}
		#endregion

		#region "Public Method(s)"


		public JqueryDataTablesPagedResults<OperatorLogReportAC> GetOperatorCallLogSearchReportList(JqueryDataWithExtraParameterAC param, long userId)
		{

			JqueryDataTablesPagedResults<OperatorLogReportAC> _searchData = new JqueryDataTablesPagedResults<OperatorLogReportAC>();
			int totalsize = 0;
			List<OperatorLogReportAC> _searchDataList = new List<OperatorLogReportAC>();

			try
			{
				int skip = (param.DataTablesParameters.Start / param.DataTablesParameters.Length) * param.DataTablesParameters.Length;
				int take = param.DataTablesParameters.Length;
				int? sortColumnNumber = null;
				string sortType = string.Empty;
				if (param.DataTablesParameters.Order.Length > 0)
				{
					sortColumnNumber = param.DataTablesParameters.Order[0].Column;
					sortType = param.DataTablesParameters.Order[0].Dir.ToString();
				}

				SortedList sl = new SortedList();
				sl.Add("SkipRecord", skip);
				sl.Add("Length", take);
				sl.Add("SearchValue", param.DataTablesParameters.Search.Value);

				sl.Add("SMonth", param.Month);
				sl.Add("SYear", param.Year);
				sl.Add("EmployeeName", param.EmployeeName);

				if (param.start != null)
				{
					DateTime StartDate = new DateTime();
					StartDate = param.start ?? System.DateTime.Now;
					DateTime newStartDate = StartDate.AddDays(1);
					int dateFrom = Convert.ToInt32(newStartDate.ToString("yyyyMMdd"));
					sl.Add("DateFrom", dateFrom);
				}
				else
				{
					sl.Add("DateFrom", 0);
				}

				if (param.end != null)
				{
					DateTime EndDate = new DateTime();
					EndDate = param.end ?? System.DateTime.Now;
					DateTime newEndDate = EndDate.AddDays(1);
					int Dateto = Convert.ToInt32(newEndDate.ToString("yyyyMMdd"));
					sl.Add("DateTo", Dateto);
				}
				else
				{
					sl.Add("DateTo", 0);
				}

				DataSet ds = _objDalmysql.GetDataSet("usp_GetCallLogReportWithPagging", sl);
				if (ds != null)
				{
					if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
					{
						_searchDataList = _objDal.ConvertDataTableToGenericList<OperatorLogReportAC>(ds.Tables[0]).ToList();
					}
					if (ds.Tables[1].Rows.Count > 0 && ds.Tables[1] != null)
					{
						totalsize = Convert.ToInt16(ds.Tables[1].Rows[0]["TotalSize"]);
					}
				}



				return new JqueryDataTablesPagedResults<OperatorLogReportAC>
				{
					Items = _searchDataList,
					TotalSize = totalsize
				};

			}
			catch (Exception e)
			{
				return null;
			}

		}

		public List<OperatorLogReportAC> ExportOperatorLogReportList(SearchReportAC param, long userId)
		{
			List<OperatorLogReportAC> _searchDataList = new List<OperatorLogReportAC>();
			try
			{
				SortedList sl = new SortedList();

				sl.Add("SearchValue", param.SearchValue);
				sl.Add("SMonth", param.Month);
				sl.Add("SYear", param.Year);

				if (param.start != null)
				{
					DateTime StartDate = new DateTime();
					StartDate = param.start ?? System.DateTime.Now;
					DateTime newStartDate = StartDate.AddDays(1);
					int dateFrom = Convert.ToInt32(newStartDate.ToString("yyyyMMdd"));
					sl.Add("DateFrom", dateFrom);
				}
				else
				{
					sl.Add("DateFrom", 0);
				}

				if (param.end != null)
				{
					DateTime EndDate = new DateTime();
					EndDate = param.end ?? System.DateTime.Now;
					DateTime newEndDate = EndDate.AddDays(1);
					int Dateto = Convert.ToInt32(newEndDate.ToString("yyyyMMdd"));
					sl.Add("DateTo", Dateto);
				}
				else
				{
					sl.Add("DateTo", 0);
				}

				DataSet ds = _objDalmysql.GetDataSet("usp_GetCallLogReportExport", sl);
				if (ds != null)
				{
					if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
					{
						_searchDataList = _objDal.ConvertDataTableToGenericList<OperatorLogReportAC>(ds.Tables[0]).ToList();
					}
				}

				return _searchDataList;

			}
			catch (Exception e)
			{
				return new List<OperatorLogReportAC>();
			}

		}


		public JqueryDataTablesPagedResults<ProviderBillReportAC> GetProviderBillSearchReportList(JqueryDataWithExtraParameterAC param, long userId)
		{

			JqueryDataTablesPagedResults<ProviderBillReportAC> _searchData = new JqueryDataTablesPagedResults<ProviderBillReportAC>();
			int totalsize = 0;
			List<ProviderBillReportAC> _searchDataList = new List<ProviderBillReportAC>();

			try
			{
				int skip = (param.DataTablesParameters.Start / param.DataTablesParameters.Length) * param.DataTablesParameters.Length;
				int take = param.DataTablesParameters.Length;
				int? sortColumnNumber = null;
				string sortType = string.Empty;
				if (param.DataTablesParameters.Order.Length > 0)
				{
					sortColumnNumber = param.DataTablesParameters.Order[0].Column;
					sortType = param.DataTablesParameters.Order[0].Dir.ToString();
				}

				SortedList sl = new SortedList();
				sl.Add("SkipRecord", skip);
				sl.Add("Length", take);
				sl.Add("SearchValue", param.DataTablesParameters.Search.Value);
				sl.Add("SMonth", param.Month);
				sl.Add("SYear", param.Year);
				sl.Add("Status", 0);

				DataSet ds = _objDalmysql.GetDataSet("usp_GetMainBillStatusReportWithPaging", sl);
				if (ds != null)
				{
					if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
					{
						_searchDataList = _objDal.ConvertDataTableToGenericList<ProviderBillReportAC>(ds.Tables[0]).ToList();
					}
					if (ds.Tables[1].Rows.Count > 0 && ds.Tables[1] != null)
					{
						totalsize = Convert.ToInt16(ds.Tables[1].Rows[0]["TotalSize"]);
					}
				}
				return new JqueryDataTablesPagedResults<ProviderBillReportAC>
				{
					Items = _searchDataList,
					TotalSize = totalsize
				};

			}
			catch (Exception e)
			{
				return null;
			}

		}

		public List<ProviderBillReportAC> ExportProviderBillReportList(SearchReportAC param, long userId)
		{
			List<ProviderBillReportAC> _searchDataList = new List<ProviderBillReportAC>();
			try
			{
				SortedList sl = new SortedList();

				sl.Add("SearchValue", param.SearchValue);
				sl.Add("SMonth", param.Month);
				sl.Add("SYear", param.Year);
				sl.Add("Status", 0);


				DataSet ds = _objDalmysql.GetDataSet("usp_GetMainBillStatusReportExport", sl);
				if (ds != null)
				{
					if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
					{
						_searchDataList = _objDal.ConvertDataTableToGenericList<ProviderBillReportAC>(ds.Tables[0]).ToList();
					}
				}

				return _searchDataList;

			}
			catch (Exception e)
			{
				return new List<ProviderBillReportAC>();
			}

		}

		public JqueryDataTablesPagedResults<AuditLogReportAC> GetAuditActivitySearchReportList(JqueryDataWithExtraParameterAC param, long userId)
		{

			JqueryDataTablesPagedResults<AuditLogReportAC> _searchData = new JqueryDataTablesPagedResults<AuditLogReportAC>();
			int totalsize = 0;
			List<AuditLogReportAC> _searchDataList = new List<AuditLogReportAC>();

			try
			{
				int skip = (param.DataTablesParameters.Start / param.DataTablesParameters.Length) * param.DataTablesParameters.Length;
				int take = param.DataTablesParameters.Length;
				int? sortColumnNumber = null;
				string sortType = string.Empty;
				if (param.DataTablesParameters.Order.Length > 0)
				{
					sortColumnNumber = param.DataTablesParameters.Order[0].Column;
					sortType = param.DataTablesParameters.Order[0].Dir.ToString();
				}

				SortedList sl = new SortedList();
				sl.Add("SkipRecord", skip);
				sl.Add("Length", take);
				sl.Add("SearchValue", param.DataTablesParameters.Search.Value);
				sl.Add("ActionId", Convert.ToInt32(param.ActionId));
				sl.Add("UserId", 0);

				if (param.start != null)
				{
					DateTime StartDate = new DateTime();
					StartDate = param.start ?? System.DateTime.Now;
					DateTime newStartDate = StartDate.AddDays(1);
					int dateFrom = Convert.ToInt32(newStartDate.ToString("yyyyMMdd"));
					sl.Add("DateFrom", dateFrom);
				}
				else
				{
					sl.Add("DateFrom", 0);
				}

				if (param.end != null)
				{
					DateTime EndDate = new DateTime();
					EndDate = param.end ?? System.DateTime.Now;
					DateTime newEndDate = EndDate.AddDays(1);
					int Dateto = Convert.ToInt32(newEndDate.ToString("yyyyMMdd"));
					sl.Add("DateTo", Dateto);
				}
				else
				{
					sl.Add("DateTo", 0);
				}

				DataSet ds = _objDalmysql.GetDataSet("usp_GetAuditLogReportWithPaging", sl);
				if (ds != null)
				{
					if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
					{
						_searchDataList = _objDal.ConvertDataTableToGenericList<AuditLogReportAC>(ds.Tables[0]).ToList();
					}
					if (ds.Tables[1].Rows.Count > 0 && ds.Tables[1] != null)
					{
						totalsize = Convert.ToInt16(ds.Tables[1].Rows[0]["TotalSize"]);
					}
				}



				return new JqueryDataTablesPagedResults<AuditLogReportAC>
				{
					Items = _searchDataList,
					TotalSize = totalsize
				};

			}
			catch (Exception e)
			{
				return null;
			}

		}

		public List<AuditLogReportAC> ExportAuditLogReportList(SearchReportAC param, long userId)
		{
			List<AuditLogReportAC> _searchDataList = new List<AuditLogReportAC>();
			try
			{
				SortedList sl = new SortedList();

				sl.Add("SearchValue", param.SearchValue);
				sl.Add("ActionId", Convert.ToInt32(param.ActionId));
				sl.Add("UserId", 0);

				if (param.start != null)
				{
					DateTime StartDate = new DateTime();
					StartDate = param.start ?? System.DateTime.Now;
					DateTime newStartDate = StartDate.AddDays(1);
					int dateFrom = Convert.ToInt32(newStartDate.ToString("yyyyMMdd"));
					sl.Add("DateFrom", dateFrom);
				}
				else
				{
					sl.Add("DateFrom", 0);
				}

				if (param.end != null)
				{
					DateTime EndDate = new DateTime();
					EndDate = param.end ?? System.DateTime.Now;
					DateTime newEndDate = EndDate.AddDays(1);
					int Dateto = Convert.ToInt32(newEndDate.ToString("yyyyMMdd"));
					sl.Add("DateTo", Dateto);
				}
				else
				{
					sl.Add("DateTo", 0);
				}

				DataSet ds = _objDalmysql.GetDataSet("usp_GetAuditReportExport", sl);
				if (ds != null)
				{
					if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
					{
						_searchDataList = _objDal.ConvertDataTableToGenericList<AuditLogReportAC>(ds.Tables[0]).ToList();
					}
				}

				return _searchDataList;

			}
			catch (Exception e)
			{
				return new List<AuditLogReportAC>();
			}

		}

		public JqueryDataTablesPagedResults<ReimbursementBillReportAC> GetReimbursementBillSearchReportList(JqueryDataWithExtraParameterAC param, long userId)
		{

			JqueryDataTablesPagedResults<ReimbursementBillReportAC> _searchData = new JqueryDataTablesPagedResults<ReimbursementBillReportAC>();
			int totalsize = 0;
			List<ReimbursementBillReportAC> _searchDataList = new List<ReimbursementBillReportAC>();

			try
			{
				int skip = (param.DataTablesParameters.Start / param.DataTablesParameters.Length) * param.DataTablesParameters.Length;
				int take = param.DataTablesParameters.Length;
				int? sortColumnNumber = null;
				string sortType = string.Empty;
				if (param.DataTablesParameters.Order.Length > 0)
				{
					sortColumnNumber = param.DataTablesParameters.Order[0].Column;
					sortType = param.DataTablesParameters.Order[0].Dir.ToString();
				}

				SortedList sl = new SortedList();
				sl.Add("SkipRecord", skip);
				sl.Add("Length", take);
				sl.Add("SearchValue", param.DataTablesParameters.Search.Value);
				sl.Add("SMonth", param.Month);
				sl.Add("SYear", param.Year);
				sl.Add("Status", 0);

				DataSet ds = _objDalmysql.GetDataSet("usp_GetReimbursementReportWithPaging", sl);
				if (ds != null)
				{
					if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
					{
						_searchDataList = _objDal.ConvertDataTableToGenericList<ReimbursementBillReportAC>(ds.Tables[0]).ToList();
					}
					if (ds.Tables[1].Rows.Count > 0 && ds.Tables[1] != null)
					{
						totalsize = Convert.ToInt16(ds.Tables[1].Rows[0]["TotalSize"]);
					}
				}
				return new JqueryDataTablesPagedResults<ReimbursementBillReportAC>
				{
					Items = _searchDataList,
					TotalSize = totalsize
				};

			}
			catch (Exception e)
			{
				return null;
			}

		}

		public List<ReimbursementBillReportAC> ExportReimbursementBillReportList(SearchReportAC param, long userId)
		{
			List<ReimbursementBillReportAC> _searchDataList = new List<ReimbursementBillReportAC>();
			try
			{
				SortedList sl = new SortedList();

				sl.Add("SearchValue", param.SearchValue);
				sl.Add("SMonth", param.Month);
				sl.Add("SYear", param.Year);
				sl.Add("Status", 0);


				DataSet ds = _objDalmysql.GetDataSet("usp_GetReimbursementBillsExport", sl);
				if (ds != null)
				{
					if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
					{
						_searchDataList = _objDal.ConvertDataTableToGenericList<ReimbursementBillReportAC>(ds.Tables[0]).ToList();
					}
				}

				return _searchDataList;

			}
			catch (Exception e)
			{
				return new List<ReimbursementBillReportAC>();
			}

		}

		public JqueryDataTablesPagedResults<AccountBillReportAC> GetAccountBillSearchReportList(JqueryDataWithExtraParameterAC param, long userId)
		{

			JqueryDataTablesPagedResults<AccountBillReportAC> _searchData = new JqueryDataTablesPagedResults<AccountBillReportAC>();
			int totalsize = 0;
			List<AccountBillReportAC> _searchDataList = new List<AccountBillReportAC>();

			try
			{
				int skip = (param.DataTablesParameters.Start / param.DataTablesParameters.Length) * param.DataTablesParameters.Length;
				int take = param.DataTablesParameters.Length;
				int? sortColumnNumber = null;
				string sortType = string.Empty;
				if (param.DataTablesParameters.Order.Length > 0)
				{
					sortColumnNumber = param.DataTablesParameters.Order[0].Column;
					sortType = param.DataTablesParameters.Order[0].Dir.ToString();
				}

				SortedList sl = new SortedList();
				sl.Add("SkipRecord", skip);
				sl.Add("Length", take);
				sl.Add("SearchValue", param.DataTablesParameters.Search.Value);
				sl.Add("SMonth", param.Month);
				sl.Add("SYear", param.Year);

				sl.Add("ProviderId", param.ProviderId);
				sl.Add("UserId", param.UserId);
				sl.Add("BusinessUnitId", param.BusinessUnitId);
				sl.Add("CostCenterId", param.CostCenterId);

				DataSet ds = _objDalmysql.GetDataSet("usp_GetEmpBillListReportWithPagging", sl);
				if (ds != null)
				{
					if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
					{
						_searchDataList = _objDal.ConvertDataTableToGenericList<AccountBillReportAC>(ds.Tables[0]).ToList();
					}
					if (ds.Tables[1].Rows.Count > 0 && ds.Tables[1] != null)
					{
						totalsize = Convert.ToInt16(ds.Tables[1].Rows[0]["TotalSize"]);
					}
				}
				return new JqueryDataTablesPagedResults<AccountBillReportAC>
				{
					Items = _searchDataList,
					TotalSize = totalsize
				};

			}
			catch (Exception e)
			{
				return null;
			}

		}

		public List<AccountBillReportAC> ExportAccountBillReportList(SearchReportAC param, long userId)
		{
			List<AccountBillReportAC> _searchDataList = new List<AccountBillReportAC>();
			try
			{
				SortedList sl = new SortedList();
				sl.Add("SearchValue", param.SearchValue);
				sl.Add("SMonth", param.Month);
				sl.Add("SYear", param.Year);

				sl.Add("ProviderId", param.ProviderId);
				sl.Add("UserId", param.UserId);
				sl.Add("BusinessUnitId", param.BusinessUnitId);
				sl.Add("CostCenterId", param.CostCenterId);


				DataSet ds = _objDalmysql.GetDataSet("usp_GetEmpBillListExport", sl);
				if (ds != null)
				{
					if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
					{
						_searchDataList = _objDal.ConvertDataTableToGenericList<AccountBillReportAC>(ds.Tables[0]).ToList();
					}
				}

				return _searchDataList;

			}
			catch (Exception e)
			{
				return new List<AccountBillReportAC>();
			}

		}

		public List<AccountBillDetailsAC> ViewAccountBillDetails(long empBillId, long userId)
		{
			List<AccountBillDetailsAC> _searchDataList = new List<AccountBillDetailsAC>();
			try
			{
				SortedList sl = new SortedList();
				sl.Add("EmpBillId", empBillId);

				DataSet ds = _objDalmysql.GetDataSet("usp_GetEmpBillDetails", sl);
				if (ds != null)
				{
					if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
					{
						_searchDataList = _objDal.ConvertDataTableToGenericList<AccountBillDetailsAC>(ds.Tables[0]).ToList();
					}
				}

				return _searchDataList;

			}
			catch (Exception e)
			{
				return new List<AccountBillDetailsAC>();
			}

		}


		public JqueryDataTablesPagedResults<MemoReportListAC> GetAccountMemoReport(JqueryDataWithExtraParameterAC param)
		{
			JqueryDataTablesPagedResults<MemoReportListAC> _searchData = new JqueryDataTablesPagedResults<MemoReportListAC>();
			int totalsize = 0;
			List<MemoReportListAC> _searchDataList = new List<MemoReportListAC>();

			int skip = (param.DataTablesParameters.Start / param.DataTablesParameters.Length) * param.DataTablesParameters.Length;
			int take = param.DataTablesParameters.Length;

			SortedList sl = new SortedList();
			sl.Add("SkipRecord", skip);
			sl.Add("Length", take);
			sl.Add("SearchValue", param.DataTablesParameters.Search.Value);
			sl.Add("SMonth", param.Month);
			sl.Add("SYear", param.Year);
			sl.Add("ProviderId", param.ProviderId);

			DataSet ds = _objDalmysql.GetDataSet("usp_GetMemoListReportWithPagging", sl);
			if (ds != null)
			{
				if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
				{
					_searchDataList = _objDal.ConvertDataTableToGenericList<MemoReportListAC>(ds.Tables[0]).ToList();
				}
				if (ds.Tables[1].Rows.Count > 0 && ds.Tables[1] != null)
				{
					totalsize = Convert.ToInt16(ds.Tables[1].Rows[0]["TotalSize"]);
				}
			}

			return new JqueryDataTablesPagedResults<MemoReportListAC>
			{
				Items = _searchDataList,
				TotalSize = totalsize
			};
		}

		public List<MemoReportListAC> ExportAccountMemoReportList(SearchReportAC param)
		{
			List<MemoReportListAC> searchDataList =new List<MemoReportListAC>();
			SortedList sl = new SortedList();
			sl.Add("SearchValue", param.SearchValue);
			sl.Add("SMonth", param.Month);
			sl.Add("SYear", param.Year);
			sl.Add("ProviderId", param.ProviderId);

			DataSet ds = _objDalmysql.GetDataSet("usp_GetMemoListReportExport", sl);
			if (ds != null)
			{
				if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
				{
					searchDataList = _objDal.ConvertDataTableToGenericList<MemoReportListAC>(ds.Tables[0]).ToList();
				}
			}
			return searchDataList;
		}

		public List<MemoBillDetailAC> ViewMemoBillsDetail(long memoId) {
			List<MemoBillDetailAC> searchDataList = new List<MemoBillDetailAC>();
			SortedList sl = new SortedList();
			sl.Add("SMemoId", memoId);

			DataSet ds = _objDalmysql.GetDataSet("usp_GetMemoBillDetails", sl);
			if (ds != null)
			{
				if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
				{
					searchDataList = _objDal.ConvertDataTableToGenericList<MemoBillDetailAC>(ds.Tables[0]).ToList();
				}
			}
			return searchDataList;
		}

		public JqueryDataTablesPagedResults<TransferDeActivatedReportAC> GetTransferredDeactivatedReport(JqueryDataTablesParameters param) {
			
			int totalsize = 0;
			List<TransferDeActivatedReportAC> searchDataList = new List<TransferDeActivatedReportAC>();

			int skip = (param.Start / param.Length) * param.Length;
			int take = param.Length;

			SortedList sl = new SortedList();
			sl.Add("SkipRecord", skip);
			sl.Add("Length", take);
			sl.Add("SearchValue", param.Search.Value);
			
			DataSet ds = _objDalmysql.GetDataSet("usp_GetTransferredDeactivatedWithPagging", sl);
			if (ds != null)
			{
				if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
				{
					searchDataList = _objDal.ConvertDataTableToGenericList<TransferDeActivatedReportAC>(ds.Tables[0]).ToList();
				}
				if (ds.Tables[1].Rows.Count > 0 && ds.Tables[1] != null)
				{
					totalsize = Convert.ToInt16(ds.Tables[1].Rows[0]["TotalSize"]);
				}
			}

			return new JqueryDataTablesPagedResults<TransferDeActivatedReportAC>
			{
				Items = searchDataList,
				TotalSize = totalsize
			};
		}

		public List<TransferDeActivatedReportAC> ExportTransferredDeactivatedList(SearchReportAC searchReportAC){

			List<TransferDeActivatedReportAC> searchDataList = new List<TransferDeActivatedReportAC>();
			SortedList sl = new SortedList();
			sl.Add("SearchValue", searchReportAC.SearchValue);
		

			DataSet ds = _objDalmysql.GetDataSet("usp_GetTransferredDeactivatedExport", sl);
			if (ds != null)
			{
				if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
				{
					searchDataList = _objDal.ConvertDataTableToGenericList<TransferDeActivatedReportAC>(ds.Tables[0]).ToList();
				}
			}
			return searchDataList;
		}

		public JqueryDataTablesPagedResults<VendorWisePackageDetailsAC> VendorWisePackageDetailReport(JqueryDataTablesParameters param) {

			int skip = (param.Start / param.Length) * param.Length;
			int take = param.Length;
			int totalsize = 0;

			List<VendorWisePackageDetailsAC> searchDataList = new List<VendorWisePackageDetailsAC>();
			List<PackageDetailListAC> packageDetailListACs = new List<PackageDetailListAC>();

			SortedList sl = new SortedList();
			sl.Add("SkipRecord", skip);
			sl.Add("Length", take);
			sl.Add("SearchValue", param.Search.Value);

			DataSet ds = _objDalmysql.GetDataSet("usp_GetVendorWisePackageReportWithPagging", sl);
			if (ds != null)
			{
				if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
				{
					packageDetailListACs = _objDal.ConvertDataTableToGenericList<PackageDetailListAC>(ds.Tables[0]).ToList();
				}
				if (ds.Tables[1].Rows.Count > 0 && ds.Tables[1] != null)
				{
					totalsize = Convert.ToInt16(ds.Tables[1].Rows[0]["TotalSize"]);
				}
			}

			var providedrWiseGroup = packageDetailListACs.GroupBy(x=>x.ProviderName);
			foreach(var item in providedrWiseGroup)
			{
				VendorWisePackageDetailsAC vendorWisePackageDetailsAC = new VendorWisePackageDetailsAC();
				vendorWisePackageDetailsAC.ProviderName = item.Key;
				vendorWisePackageDetailsAC.PackageDetailListAC = new List<PackageDetailListAC>();
				foreach(var subitem in item) {
					vendorWisePackageDetailsAC.PackageDetailListAC.Add(subitem);
				}
				searchDataList.Add(vendorWisePackageDetailsAC);
			}

			return new JqueryDataTablesPagedResults<VendorWisePackageDetailsAC>
			{
				Items = searchDataList,
				TotalSize = totalsize
			};
		}

		public List<PackageDetailListAC> ExportVendorWisePackageDetailList(SearchReportAC searchReportAC) {

			SortedList sl = new SortedList();
			sl.Add("SearchValue", searchReportAC.SearchValue);

			List<PackageDetailListAC> searchDataList = new List<PackageDetailListAC>();
			DataSet ds = _objDalmysql.GetDataSet("usp_GetVendorWisePackageExport", sl);
			if (ds != null)
			{
				if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0] != null)
				{
					searchDataList = _objDal.ConvertDataTableToGenericList<PackageDetailListAC>(ds.Tables[0]).ToList();
				}
			}
			return searchDataList;
		}

		#endregion
	}
}
