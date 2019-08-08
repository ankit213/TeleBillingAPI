
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using TeleBillingRepository.Repository.Account;
using TeleBillingUtility.Models;

namespace TeleBillingAPIXUnit.Tests
{
	public class BaseProvider
	{
		
		public IServiceProvider serviceProvider { get; set; }
		
		public BaseProvider() {

			var randomString = Guid.NewGuid().ToString();
			var services = new ServiceCollection();
			services.AddEntityFrameworkInMemoryDatabase();

			services.AddDbContext<TeleBilling_V01Context>(opt => opt.UseInMemoryDatabase(randomString), ServiceLifetime.Transient);
			services.AddScoped<IAccountRepository, AccountRepository>();

			serviceProvider = services.BuildServiceProvider();
			AddEmploye(serviceProvider);

		}


		private async void AddEmploye(IServiceProvider serviceProvider)
		{
			var _dbTeleBilling_V01Context = serviceProvider.GetService<TeleBilling_V01Context>();

			#region Added Role
			MstRole mstRole = new MstRole();
			mstRole.IsActive = true;
			mstRole.RoleName = "Super Admin";
			mstRole.CreatedBy = 1;
			mstRole.CreatedDate = DateTime.Now;
			mstRole.TransactionId = 080820191635;

			await _dbTeleBilling_V01Context.AddAsync(mstRole);
			await _dbTeleBilling_V01Context.SaveChangesAsync();
			#endregion

			#region Added Business Unit 
			MstBusinessUnit mstBusinessUnit = new MstBusinessUnit();
			mstBusinessUnit.IsActive = true;
			mstBusinessUnit.Name = "RISK MANAGEMENT & CONTROL";
			mstBusinessUnit.CreatedBy = 1;
			mstBusinessUnit.CreatedDate = DateTime.Now;

			await _dbTeleBilling_V01Context.AddAsync(mstBusinessUnit);
			await _dbTeleBilling_V01Context.SaveChangesAsync();
			#endregion

			#region Added Department
			MstDepartment mstDepartment = new MstDepartment();
			mstDepartment.IsActive = true;
			mstDepartment.Name = "Business";
			mstDepartment.BusinessUnitId = mstBusinessUnit.Id;
			mstDepartment.CreatedBy = 1;
			mstDepartment.CreatedDate = DateTime.Now;

			await _dbTeleBilling_V01Context.AddAsync(mstDepartment);
			await _dbTeleBilling_V01Context.SaveChangesAsync();
			#endregion

			#region Added CostCenter
			MstCostCenter mstCostCenter = new MstCostCenter();
			mstCostCenter.BusinessUnitid = mstBusinessUnit.Id;
			mstCostCenter.CostCenterCode = "001";
			mstCostCenter.Name = "Vadodra Center";
			mstCostCenter.CreatedBy = 1;
			mstCostCenter.CreatedDate = DateTime.Now;
			

			await _dbTeleBilling_V01Context.AddAsync(mstCostCenter);
			await _dbTeleBilling_V01Context.SaveChangesAsync();
			#endregion

			#region Added Dummy User in Employee

			MstEmployee mstEmployee = new MstEmployee();
			mstEmployee.FullName = "Super Admin";
			mstEmployee.ExtensionNumber = "08082019";
			mstEmployee.EmpPfnumber = "123456";
			mstEmployee.EmailId = "bhanvadiaankit@gmail.com";
			mstEmployee.Password = "123456";
			mstEmployee.RoleId = mstRole.RoleId;
			mstEmployee.IsActive = true;
			mstEmployee.IsSystemUser = true;
			mstEmployee.DepartmentId = mstDepartment.Id;
			mstEmployee.Designation = "Administrator";
			mstEmployee.Description = "super admin role";
			mstEmployee.BusinessUnitId = mstBusinessUnit.Id;
			mstEmployee.CostCenterId = mstCostCenter.Id;
			mstEmployee.CountryId = 1;
			mstEmployee.IsPresidentOffice = true;
			mstEmployee.LineManagerId = 1;
			mstEmployee.CreatedBy = 1;
			mstEmployee.CreatedDate = DateTime.Now;
			mstEmployee.TransactionId = 080820191636;

			await _dbTeleBilling_V01Context.AddAsync(mstEmployee);
			await _dbTeleBilling_V01Context.SaveChangesAsync();
			#endregion
		}
	}
}
