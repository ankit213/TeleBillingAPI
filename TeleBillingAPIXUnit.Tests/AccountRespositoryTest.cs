using System;
using System.Threading.Tasks;
using TeleBillingRepository.Repository.Account;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace TeleBillingAPIXUnit.Tests
{
	public class AccountRespositoryTest : BaseProvider
	{
		#region Private Variables
		private readonly IAccountRepository _accountRespository;
		#endregion

		#region Construtor
		public AccountRespositoryTest() : base()
		{
			_accountRespository = serviceProvider.GetService<IAccountRepository>();
		}
		#endregion


		#region Test Case
		[Fact]
		public async Task GetEmployeeByPFNumber()
		{
			string pfNumber = "123456";
			var employeeDetail = await _accountRespository.GetEmployeeBy(pfNumber);
			Assert.Equal(employeeDetail.EmpPfnumber, pfNumber);
		}
		

		[Fact]
		public async Task CheckUserCredentailNotMatch()
		{
			string email = "12345678";
			string passWord = "123456";
			var result = await _accountRespository.CheckUserCredentail(email, passWord);
			Assert.DoesNotMatch(result.ToString().ToLower(), "true");
		}

		[Fact]
		public async Task CheckUserCredentailUsingEmail()
		{
			string email = "bhanvadiaankit@gmail.com";
			string passWord = "123456";
			var result = await _accountRespository.CheckUserCredentail(email, passWord);
			Assert.Matches(result.ToString().ToLower(), "true");
		}

		[Fact]
		public async Task GetLineManagerEmail()
		{
			var result = await _accountRespository.GetLineManagerEmail("1");
			Assert.Matches(result.ToString(),"bhanvadiaankit@gmail.com");
		}


		#endregion
	}
}
