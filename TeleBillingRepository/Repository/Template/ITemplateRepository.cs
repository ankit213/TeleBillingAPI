using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TeleBillingUtility.ApplicationClass;

namespace TeleBillingRepository.Repository.Template
{
	public interface ITemplateRepository
	{
		/// <summary>
		/// This method used for get template list
		/// </summary>
		/// <returns></returns>
		Task<List<TemplateAC>> GetTemplateList();

		/// <summary>
		/// This method used for update exists template		
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="templateDetailAC"></param>
		/// <returns></returns>
		Task<ResponseAC> UpdateTemplate(long userId, TemplateDetailAC templateDetailAC);



		/// <summary>
		/// This method used for add new template		
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="templateDetailAC"></param>
		/// <returns></returns>
		Task<ResponseAC> AddTemplate(long userId, TemplateDetailAC templateDetailAC);
		
		
		/// <summary>
		/// This method used for get template by id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<TemplateDetailAC> GetTemplateById(long id);
	}
}
