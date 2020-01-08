using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

namespace TeleBillingRepository.Repository.Dashboard
{
	public class DashboardRepository : IDashboardRepository
	{

		#region "Private Variable(s)"
		private readonly telebilling_v01Context _dbTeleBilling_V01Context;
		private readonly ILogManagement _iLogManagement;
		private readonly IStringConstant _iStringConstant;
		private readonly IMapper _mapper;
		private readonly DAL _objDal = new DAL();
		private readonly DALMySql _objDalmysql = new DALMySql();
		#endregion

		#region "Constructor"
		public DashboardRepository(telebilling_v01Context dbTeleBilling_V01Context, IStringConstant iStringConstant,
			ILogManagement iLogManagement, IMapper mapper)
		{
			_dbTeleBilling_V01Context = dbTeleBilling_V01Context;
			_iStringConstant = iStringConstant;
			_iLogManagement = iLogManagement;
			_mapper = mapper;
		}
		#endregion


		#region "Public Method(s)"

		public UserDashoboarAC getUserBillDataForPieChart(long employeeid)
		{
			UserDashoboarAC userSummaryData = new UserDashoboarAC();
			List<UserMobileBillAC> _searchDataList = new List<UserMobileBillAC>();
			List<StaffEmployeeAC> _staffEmployeeList = new List<StaffEmployeeAC>();
			List<UserSkypeMocDataAC> _skypeMocDetailList = new List<UserSkypeMocDataAC>();
			List<UsertransTypeTotalAC> _searchDataTranstypeList = new List<UsertransTypeTotalAC>();
			List<PieChartAC> _pieChartDatalist = new List<PieChartAC>();


			List<UserMobileBillAC> _CurrentBillList = new List<UserMobileBillAC>();
			List<UsertransTypeTotalAC> _currentBillTranstypeList = new List<UsertransTypeTotalAC>();

			try
			{

				if (employeeid > 0)
				{

					userSummaryData.UserId = employeeid;

					#region ---> GET USER CLOSE BILL DATA

					SortedList sl = new SortedList();
					sl.Add("UserId", employeeid);
					DataSet ds = _objDalmysql.GetDataSet("usp_GetUserBillDataForDashboard", sl);
					if (ds != null)
					{
						if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
						{
							_searchDataList = _objDal.ConvertDataTableToGenericList<UserMobileBillAC>(ds.Tables[0]).ToList();
						}

						if (ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
						{
							_searchDataTranstypeList = _objDal.ConvertDataTableToGenericList<UsertransTypeTotalAC>(ds.Tables[1]).ToList();
						}

						if (ds.Tables[2] != null && ds.Tables[2].Rows.Count > 0)
						{
							_staffEmployeeList = _objDal.ConvertDataTableToGenericList<StaffEmployeeAC>(ds.Tables[2]).ToList();
						}

						if (ds.Tables[3] != null && ds.Tables[3].Rows.Count > 0)
						{
							_skypeMocDetailList = _objDal.ConvertDataTableToGenericList<UserSkypeMocDataAC>(ds.Tables[3]).ToList();
						}


						if ((_searchDataList != null && _searchDataList.Count() > 0) && (_searchDataTranstypeList != null && _searchDataTranstypeList.Count() > 0))
						{
							#region Bind PieChartData
							foreach (var item in _searchDataList)
							{
								PieChartAC _pieChart = new PieChartAC();
								var tanstypelist = _searchDataTranstypeList.Where(x => x.EmpBillId == item.Id).ToList();

								_pieChart.TelephoneNumber = item.TelephoneNumber;
								_pieChart.dataList = tanstypelist.Select(x => x.TransType).ToList();
								_pieChart.dataArray = tanstypelist.Select(x => x.TransType).ToArray();

								foreach (var subitem in tanstypelist.ToList())
								{
									DataListValue dataListValue = new DataListValue();
									dataListValue.Name = subitem.TransType;
									dataListValue.Value = Convert.ToDecimal(subitem.TranstypeTotal);

									_pieChart.datalistvalues.Add(dataListValue);

								}
								_pieChartDatalist.Add(_pieChart);

							}
							#endregion


							if (_pieChartDatalist != null && _pieChartDatalist.Count() > 0)
							{
								userSummaryData.PieChartDataList = _pieChartDatalist;
							}

							if (_searchDataList != null && _searchDataList.Count() > 0)
							{
								userSummaryData.userMobileBills = _searchDataList;
							}

						}


						if ((_staffEmployeeList != null && _staffEmployeeList.Count() > 0))
						{
							userSummaryData.StaffEmployeeList = _staffEmployeeList;
						}

						if ((_skypeMocDetailList != null && _skypeMocDetailList.Count() > 0))
						{
							userSummaryData.SkypeMocDatalist = _skypeMocDetailList;
						}
					}

					#endregion

					#region ---> GET USER OPEN BILL DATA
					SortedList sl2 = new SortedList();
					sl2.Add("UserId", employeeid);
					DataSet ds2 = _objDalmysql.GetDataSet("usp_GetUserCurrentBillForDashboard", sl2);
					if (ds2 != null)
					{
						if (ds2.Tables[0] != null && ds2.Tables[0].Rows.Count > 0)
						{
							_CurrentBillList = _objDal.ConvertDataTableToGenericList<UserMobileBillAC>(ds2.Tables[0]).ToList();
						}

						// we are not fatching current bill trasType Chart
						//if (ds2.Tables[1] != null && ds2.Tables[1].Rows.Count > 0)
						//{
						//    _currentBillTranstypeList = _objDal.ConvertDataTableToGenericList<UsertransTypeTotalAC>(ds2.Tables[1]).ToList();
						//}
					}

					if ((_CurrentBillList != null && _CurrentBillList.Count() > 0))
					{
						userSummaryData.userCurrentBills = _CurrentBillList;
					}

					#endregion


					return userSummaryData;
				}
				else
				{
					return new UserDashoboarAC();
				}

			}
			catch (Exception e)
			{
				return new UserDashoboarAC();
			}

		}


		#region Dashboard - New
		public async Task<List<ProviderWiseClosedBillAC>> GetProviderWiseLastClosedBillDetails() {
			var providerdata = (from provider in _dbTeleBilling_V01Context.Provider
								join
								providerservice in _dbTeleBilling_V01Context.Providerservice on provider.Id equals providerservice.ProviderId
								where provider.IsDelete == false && providerservice.IsDelete == false && (providerservice.ServiceTypeId == 1 || providerservice.ServiceTypeId == 2 || providerservice.ServiceTypeId == 6 || providerservice.ServiceTypeId == 12)
								select new TeleBillingUtility.Models.Provider
								{
									Id = provider.Id,
									Name = provider.Name
								}).ToList();


			List<ProviderWiseClosedBillAC> providerWiseClosedBillACs = new List<ProviderWiseClosedBillAC>();

			foreach (var item in providerdata)
			{
				if (!providerWiseClosedBillACs.Any(x => x.ProviderId == item.Id))
				{
					ProviderWiseClosedBillAC providerWiseClosedBillAC = new ProviderWiseClosedBillAC();
					int billClosed = Convert.ToInt16(EnumList.BillStatus.BillClosed);
					int memoCreated = Convert.ToInt16(EnumList.BillStatus.MemoCreated);

					Billmaster billmasters = await _dbTeleBilling_V01Context.Billmaster.Where(x => x.ProviderId == item.Id && !x.IsDelete && (x.BillStatusId == billClosed || x.BillStatusId == memoCreated)).Include(x => x.Currency).OrderByDescending(x => x.Id).FirstOrDefaultAsync();
					if (billmasters != null)
					{
						decimal? totalDeductableAmount = 0;
						List<Employeebillmaster> employeebillmasters = await _dbTeleBilling_V01Context.Employeebillmaster.Where(x => !x.IsDelete && x.BillMasterId == billmasters.Id).Include(x => x.Employeebillservicepackage).ToListAsync();
						foreach (var subItem in employeebillmasters)
						{
							totalDeductableAmount += subItem.Employeebillservicepackage.Sum(x => x.DeductionAmount);
						}
						providerWiseClosedBillAC.BillNumber = billmasters.BillNumber;
						EnumList.Month month = (EnumList.Month)billmasters.BillMonth;
						providerWiseClosedBillAC.MonthYear = month.ToString() + " " + billmasters.BillYear;
						providerWiseClosedBillAC.ProviderId = item.Id;
						providerWiseClosedBillAC.ProviderName = item.Name;
						providerWiseClosedBillAC.TotalBillAmount = billmasters.BillAmount;
						providerWiseClosedBillAC.EmployeeDeducatable = Convert.ToDecimal(totalDeductableAmount);
						providerWiseClosedBillAC.CompanyPayable = providerWiseClosedBillAC.TotalBillAmount - providerWiseClosedBillAC.EmployeeDeducatable;
						providerWiseClosedBillAC.Currency = billmasters.Currency.Code;
						providerWiseClosedBillACs.Add(providerWiseClosedBillAC);
					}
				}
			}
			return providerWiseClosedBillACs;
		}

		public async Task<List<ProviderBillChartDetailAC>> GetChartDetailByProvider(long providerid)
		{
			int billClosed = Convert.ToInt16(EnumList.BillStatus.BillClosed);
			int memoCreated = Convert.ToInt16(EnumList.BillStatus.MemoCreated);

			List<ProviderBillChartDetailAC> providerBillChartDetailACs = new List<ProviderBillChartDetailAC>();
			List<Billmaster> billmasters = await _dbTeleBilling_V01Context.Billmaster.Where(x => !x.IsDelete && x.ProviderId == providerid && (x.BillStatusId == billClosed || x.BillStatusId == memoCreated)).OrderByDescending(x => x.Id).Take(3).Include(x => x.Billdetails).Include(x=>x.Currency).ToListAsync();
			List<Billdetails> billdetails = new List<Billdetails>();
			if (billmasters.Count > 0)
			{
				foreach (var item in billmasters)
				{
					billdetails.AddRange(item.Billdetails);
				}
			}

			List<TransTypeDetailsAC> transTypeDetailsACs = new List<TransTypeDetailsAC>();
			List<string> trasnTypes = billdetails.GroupBy(x => x.TransType).Select(x => x.Key).ToList();

			foreach (var item in billmasters)
			{
				ProviderBillChartDetailAC providerBillChartDetailAC = new ProviderBillChartDetailAC();
				EnumList.Month month = (EnumList.Month)item.BillMonth;
				providerBillChartDetailAC.MonthYears = (month.ToString() + " " + item.BillYear);
				providerBillChartDetailAC.Currency = item.Currency.Code;
				providerBillChartDetailAC.transTypeDetailsACs = GetTransTypeDetailsAC(trasnTypes);
				
				decimal? totalAmount = 0;
				foreach (var subItem in item.Billdetails.GroupBy(x => x.TransType))
				{
					var transTypeDetails = providerBillChartDetailAC.transTypeDetailsACs.FirstOrDefault(x => x.TransType == subItem.Key);
					transTypeDetails.BillAmount = 0;
					if (transTypeDetails != null)
					{
						transTypeDetails.BillAmount = subItem.Sum(x => x.CallAmount);
					}
					else
					{
						transTypeDetails.BillAmount = 0;
					}

					totalAmount += transTypeDetails.BillAmount;
				}

			   providerBillChartDetailAC.TotalAmount = Convert.ToDecimal(totalAmount);
			   providerBillChartDetailACs.Add(providerBillChartDetailAC);
			}

			return providerBillChartDetailACs;
		}

		private List<TransTypeDetailsAC> GetTransTypeDetailsAC(List<string>  trasnTypes)
		{
			List <TransTypeDetailsAC> transTypeDetailsACs = new List<TransTypeDetailsAC>();
			foreach (var item in trasnTypes)
			{
				TransTypeDetailsAC transTypeDetailsAC = new TransTypeDetailsAC();
				transTypeDetailsAC.TransType = item;
				string colorCode = string.Empty;
				do
				{
					colorCode = CommonFunction.GetRandomColorFromList();
				}
				while (transTypeDetailsACs.Any(x => x.BackGroundColor == colorCode));

				transTypeDetailsAC.BackGroundColor = colorCode;
				transTypeDetailsAC.BillAmount = 0;
				transTypeDetailsACs.Add(transTypeDetailsAC);
			}
			return transTypeDetailsACs;
		}


		//private async Task<List<ProviderWiseOpenBillAC>> GetProviderWiseLastOpenBillDetails()
		//{
		//	return null;
		//}
		#endregion

		#endregion

	}
}
