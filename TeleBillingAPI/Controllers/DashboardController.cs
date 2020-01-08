using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;
using TeleBillingRepository.Repository.Dashboard;
using TeleBillingRepository.Service.LogMangement;

namespace TeleBillingAPI.Controllers
{
    [EnableCors("CORS")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : Controller
    {
        #region "Private Variable(s)"
        private readonly IDashboardRepository _idashboardRepository;
        private readonly ILogManagement _iLogManagement;
        private readonly Logger _logger = LogManager.GetLogger("logger");
        private readonly IHostingEnvironment _hostingEnvironment;
        #endregion

        #region "Constructor"

        #region "Constructor"
        public DashboardController(IDashboardRepository dashboardRepository
            , IHostingEnvironment hostingEnvironment
            )
        {
            _idashboardRepository = dashboardRepository;
            _hostingEnvironment = hostingEnvironment;

        }
        #endregion
        #endregion

        #region "Public Method(s)"


        [HttpGet]
        [Route("userbilldataforpiechart")]
        public async Task<IActionResult> UserBillDataForPieChart()
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
            return Ok(_idashboardRepository.getUserBillDataForPieChart(Convert.ToInt64(userId)));
        }


		#region Dashboard-New

		[HttpGet]
		[Route("providerwiselastclosedbill")]
		public async Task<IActionResult> GetProviderWiseLastClosedBillDetails()
		{
			return Ok(await _idashboardRepository.GetProviderWiseLastClosedBillDetails());
		}

		[HttpGet]
		[Route("providerwiseclosebills/{providerid}")]
		public async Task<IActionResult> GetChartDetailByProvider(long providerid)
		{
			return Ok(await _idashboardRepository.GetChartDetailByProvider(providerid));
		}

		//[HttpGet]
		//[Route("providerwiseopenbill")]
		//public async Task<IActionResult> GetProviderWiseLastOpendBillDetails()
		//{
		//	return Ok(await _idashboardRepository.GetProviderWiseLastOpenBillDetails());
		//}


		#endregion

		#endregion
	}
}