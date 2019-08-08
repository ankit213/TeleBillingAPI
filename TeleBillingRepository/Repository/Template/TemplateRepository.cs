using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleBillingRepository.Service.Constants;
using TeleBillingRepository.Service.LogMangement;
using TeleBillingUtility.ApplicationClass;
using TeleBillingUtility.Helpers.Enums;
using TeleBillingUtility.Models;

namespace TeleBillingRepository.Repository.Template
{
	public class TemplateRepository : ITemplateRepository
	{
		#region "Private Variable(s)"
		private readonly TeleBilling_V01Context _dbTeleBilling_V01Context;
		private readonly ILogManagement _iLogManagement;
		private readonly IStringConstant _iStringConstant;
		private IMapper _mapper;
		#endregion

		#region "Constructor"
		public TemplateRepository(TeleBilling_V01Context dbTeleBilling_V01Context, IMapper mapper, IStringConstant iStringConstant,
			ILogManagement iLogManagement)
		{
			_dbTeleBilling_V01Context = dbTeleBilling_V01Context;
			_iStringConstant = iStringConstant;
			_iLogManagement = iLogManagement;
			_mapper = mapper;
		}
		#endregion
		
		#region Public Method(s)

		public async Task<List<TemplateAC>> GetTemplateList() {
				List<EmailTemplate> emailTemplateList = await _dbTeleBilling_V01Context.EmailTemplate.Include(x => x.EmailTemplateType).ToListAsync();
				return _mapper.Map<List<TemplateAC>>(emailTemplateList);
		}


		public async Task<ResponseAC> AddTemplate(long userId, TemplateDetailAC templateDetailAC) {
			ResponseAC responseAC = new ResponseAC();
			if(!_dbTeleBilling_V01Context.EmailTemplate.Any(x=>x.EmailTemplateTypeId == templateDetailAC.EmailTemplateTypeId)) {
				EmailTemplate emailTemplate = _mapper.Map<EmailTemplate>(templateDetailAC);
				emailTemplate.CreatedBy = userId;
				emailTemplate.CreatedDate = DateTime.Now;
				emailTemplate.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
				await _dbTeleBilling_V01Context.AddAsync(emailTemplate);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
				responseAC.Message = _iStringConstant.TemplateAddedSuccessfully;
			}
			else
			{
				responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
				responseAC.Message = _iStringConstant.TemplateTypeAlreadyExists;
			}
			return responseAC;
		}


		public async Task<ResponseAC> UpdateTemplate(long userId, TemplateDetailAC templateDetailAC) {
			ResponseAC responseAC = new ResponseAC();
			if (!_dbTeleBilling_V01Context.EmailTemplate.Any(x => x.EmailTemplateTypeId == templateDetailAC.EmailTemplateTypeId && x.Id != templateDetailAC.Id))
			{
				EmailTemplate emailTemplate = await _dbTeleBilling_V01Context.EmailTemplate.FirstOrDefaultAsync(x=>x.Id == templateDetailAC.Id);
				#region Transaction Log Entry
				if (emailTemplate.TransactionId == null)
					emailTemplate.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();

				var jsonSerailzeObj = JsonConvert.SerializeObject(emailTemplate);
				await _iLogManagement.SaveRequestTraseLog(Convert.ToInt64(emailTemplate.TransactionId), userId, Convert.ToInt64(EnumList.TransactionTraseLog.UpdateRecord), jsonSerailzeObj);
				#endregion

				emailTemplate = _mapper.Map(templateDetailAC, emailTemplate);
				emailTemplate.UpdatedBy = userId;
				emailTemplate.UpdatedDate = DateTime.Now;

				_dbTeleBilling_V01Context.Update(emailTemplate);
				await _dbTeleBilling_V01Context.SaveChangesAsync();

				responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
				responseAC.Message = _iStringConstant.TemplateUpdateSuccessfully;
			}
			else
			{
				responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
				responseAC.Message = _iStringConstant.TemplateTypeAlreadyExists;
			}
			return responseAC;
		}


		public async Task<TemplateDetailAC> GetTemplateById(long id) {
			EmailTemplate emailTemplate = await _dbTeleBilling_V01Context.EmailTemplate.FirstOrDefaultAsync(x=>x.Id == id);
			return _mapper.Map<TemplateDetailAC>(emailTemplate);
		}

		#endregion
	}
}
