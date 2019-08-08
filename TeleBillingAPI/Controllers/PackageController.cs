using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using TeleBillingRepository.Repository.Package;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingAPI.Controllers
{
	[EnableCors("CORS")]
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class PackageController : ControllerBase {

		#region "Private Variable(s)"
		private readonly IPackageRepository _iPackageRepository;
		#endregion

		#region "Constructor"
		public PackageController(IPackageRepository iPackageRepository) {
			_iPackageRepository = iPackageRepository;
		}
		#endregion

		#region "Public Method(s)"

		[HttpGet]
		[Route("list")]
		public async Task<IActionResult> GetPacakgeList()
		{
			return Ok(await _iPackageRepository.GetPackageList());
		}


		[HttpPost]
		[Route("add")]
		public async Task<IActionResult> AddPackage(PackageDetailAC packageDetailAC)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iPackageRepository.AddPackage(Convert.ToInt64(userId),packageDetailAC));
		}

		[HttpGet]
		[Route("delete/{id}")]
		public async Task<IActionResult> DeletePackage(long id)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iPackageRepository.DeletePackage(Convert.ToInt64(userId), id));
		}


		[HttpGet]
		[Route("changestatus/{id}")]
		public async Task<IActionResult> ChangePackageStatus(long id)
		{
			var currentUser = HttpContext.User;
			string userId = currentUser.Claims.FirstOrDefault(c => c.Type == "user_id").Value;
			return Ok(await _iPackageRepository.ChangePackageStatus(Convert.ToInt64(userId), id));
		}

		[HttpGet]
		[Route("{id}")]
		public async Task<IActionResult> GetPackage(long id)
		{
			return Ok(await _iPackageRepository.GetPackageById(id));
		}

		#endregion

	}
}