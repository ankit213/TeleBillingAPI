using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly telebilling_v01Context _dbTeleBilling_V01Context;
        private readonly ILogManagement _iLogManagement;
        private readonly IStringConstant _iStringConstant;
        private IMapper _mapper;
        #endregion

        #region "Constructor"
        public TemplateRepository(telebilling_v01Context dbTeleBilling_V01Context, IMapper mapper, IStringConstant iStringConstant,
            ILogManagement iLogManagement)
        {
            _dbTeleBilling_V01Context = dbTeleBilling_V01Context;
            _iStringConstant = iStringConstant;
            _iLogManagement = iLogManagement;
            _mapper = mapper;
        }
        #endregion

        #region Public Method(s)

        public async Task<List<TemplateAC>> GetTemplateList()
        {
            List<Emailtemplate> emailTemplateList = await _dbTeleBilling_V01Context.Emailtemplate.Include(x => x.EmailTemplateType).OrderByDescending(x => x.Id).ToListAsync();
            return _mapper.Map<List<TemplateAC>>(emailTemplateList);
        }


        public async Task<ResponseAC> AddTemplate(long userId, TemplateDetailAC templateDetailAC, string loginUserName)
        {
            ResponseAC responseAC = new ResponseAC();
            if (await _dbTeleBilling_V01Context.Emailtemplate.FirstOrDefaultAsync(x => x.EmailTemplateTypeId == templateDetailAC.EmailTemplateTypeId) == null)
            {
                Emailtemplate emailTemplate = _mapper.Map<Emailtemplate>(templateDetailAC);
                emailTemplate.CreatedBy = userId;
                emailTemplate.CreatedDate = DateTime.Now;
                emailTemplate.TransactionId = _iLogManagement.GenerateTeleBillingTransctionID();
                await _dbTeleBilling_V01Context.AddAsync(emailTemplate);
                await _dbTeleBilling_V01Context.SaveChangesAsync();

                responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Success);
                responseAC.Message = _iStringConstant.TemplateAddedSuccessfully;

                await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.AddEmailTemplate, loginUserName, userId, "Email template(" + emailTemplate.Subject + ")", (int)EnumList.ActionTemplateTypes.Add, emailTemplate.Id);
            }
            else
            {
                responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                responseAC.Message = _iStringConstant.TemplateTypeAlreadyExists;
            }
            return responseAC;
        }


        public async Task<ResponseAC> UpdateTemplate(long userId, TemplateDetailAC templateDetailAC, string loginUserName)
        {
            ResponseAC responseAC = new ResponseAC();
            if (await _dbTeleBilling_V01Context.Emailtemplate.FirstOrDefaultAsync(x => x.EmailTemplateTypeId == templateDetailAC.EmailTemplateTypeId && x.Id != templateDetailAC.Id) == null)
            {
                Emailtemplate emailTemplate = await _dbTeleBilling_V01Context.Emailtemplate.FirstOrDefaultAsync(x => x.Id == templateDetailAC.Id);
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

                await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.EditEmailTemplate, loginUserName, userId, "Email template(" + emailTemplate.Subject + ")", (int)EnumList.ActionTemplateTypes.Edit, emailTemplate.Id);
            }
            else
            {
                responseAC.StatusCode = Convert.ToInt16(EnumList.ResponseType.Error);
                responseAC.Message = _iStringConstant.TemplateTypeAlreadyExists;
            }
            return responseAC;
        }


        public async Task<TemplateDetailAC> GetTemplateById(long id)
        {
            Emailtemplate emailTemplate = await _dbTeleBilling_V01Context.Emailtemplate.FirstOrDefaultAsync(x => x.Id == id);
            return _mapper.Map<TemplateDetailAC>(emailTemplate);
        }

        #endregion
    }
}
