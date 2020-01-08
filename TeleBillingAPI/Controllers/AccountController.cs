using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TeleBillingRepository.Repository.Account;
using TeleBillingRepository.Service;
using TeleBillingRepository.Service.Constants;
using TeleBillingRepository.Service.LogMangement;
using TeleBillingUtility.ApplicationClass;
using TeleBillingUtility.Helpers.Enums;

namespace TeleBillingAPI.Controllers
{
    [EnableCors("CORS")]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        #region "Private Variable(s)"
        private readonly IAccountRepository _iAccountRepository;
        private readonly IStringConstant _iStringConstant;
        private readonly IEmailSender _iEmailSender;
        private readonly ILogManagement _iLogManagement;
        public IConfiguration Configuration { get; }

        #endregion

        #region "Constructor"
        public AccountController(IAccountRepository iAccountRepository, IStringConstant iStringConstant, IEmailSender iEmailSender, IConfiguration configuration, ILogManagement iLogManagement)
        {
            _iAccountRepository = iAccountRepository;
            _iStringConstant = iStringConstant;
            _iEmailSender = iEmailSender;
            Configuration = configuration;
            _iLogManagement = iLogManagement;

        }
        #endregion

        #region "Public Method(s)"

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody]LoginAC loginAC)
        {
            LoginReponseAC loginResponseAc = new LoginReponseAC();
            try
            {
                var userexists = await _iAccountRepository.checkEmployeeIsActive(loginAC.EmailOrPfNumber);
                if (userexists != null)
                {
                    if (userexists.IsActive)
                    {
                        var user = await _iAccountRepository.GetEmployeeBy(loginAC.EmailOrPfNumber);
                        if (user != null)
                        {

                            if (await _iAccountRepository.CheckUserCredentail(user.EmailId, user.EmpPfnumber, loginAC.Password))
                            {

                                var claims = new[] {
                                new Claim("user_id", user.UserId.ToString()),
                                new Claim("role_id", user.RoleId.ToString()),
                                new Claim("fullname", user.FullName),
                                };

                                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SigninKey"]));
                                var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
                                var tokeOptions = new JwtSecurityToken(
                                    issuer: Configuration["Issuer"],
                                    audience: Configuration["Audience"],
                                    claims: claims,
                                    expires: DateTime.Now.AddMinutes(60),
                                    signingCredentials: signinCredentials
                                );

                                await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.Login, user.FullName, user.UserId, "", (int)EnumList.ActionTemplateTypes.Login, user.UserId);
								loginResponseAc.RoleId = user.RoleId;
								loginResponseAc.AccessToken = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
                                loginResponseAc.StatusCode = Convert.ToInt32(EnumList.ResponseType.Success);
                            }
                            else
                            {
                                loginResponseAc.StatusCode = Convert.ToInt32(EnumList.ResponseType.Error);
                                loginResponseAc.Message = _iStringConstant.LoginCredentailWrong;
                            }
                        }
                        else
                        {
                            loginResponseAc.StatusCode = Convert.ToInt32(EnumList.ResponseType.NotFound);
                            loginResponseAc.Message = _iStringConstant.EmailOrPfNumberNotValid;
                        }
                    }
                    else
                    {
                        loginResponseAc.StatusCode = Convert.ToInt32(EnumList.ResponseType.NotFound);
                        loginResponseAc.Message = _iStringConstant.UserAccountDeactivated;
                    }
                }
                else
                {
                    loginResponseAc.StatusCode = Convert.ToInt32(EnumList.ResponseType.NotFound);
                    loginResponseAc.Message = _iStringConstant.EmailOrPfNumberNotValid;
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return Ok(loginResponseAc);
        }

        [HttpGet]
        [Route("forgotpassword")]
        public async Task<IActionResult> ForgotPassword(string emailOrPfNumber)
        {
            try
            {
                //string EmailOrPfNumber = loginAC.EmailOrPfNumber;
                ForgotPasswordResponseAC ResponseAc = new ForgotPasswordResponseAC();
                var user = await _iAccountRepository.GetEmployeeBy(emailOrPfNumber);
                if (user != null)
                {
                    if (user.IsSystemUser)
                    {
                        string EmailtoSend = user.EmailId;
                        ResponseAc.ReceiverName = user.FullName;
                        if (string.IsNullOrEmpty(user.EmailId))
                        {
                            string linemanagerEmail = await _iAccountRepository.GetLineManagerEmail(user.LineManagerId.ToString());
                            if (linemanagerEmail == null || linemanagerEmail == "")
                            {
                                ResponseAc.StatusCode = Convert.ToInt32(EnumList.ResponseType.NotFound);
                                ResponseAc.Message = _iStringConstant.EmailNotFound;
                                return Ok(ResponseAc);
                            }
                            else
                            {
                                EmailtoSend = linemanagerEmail;
                                ResponseAc.ReceiverName = "Linemanager";
                            }
                        }

                        #region --> SetForgot password Mail SetUp
                        ResponseAc.ReceiverEmail = EmailtoSend;
                        ResponseAc.RestPasswordUrl = "https://www.google.com/";
                        ResponseAc.ForEmployee = user.FullName;
                        ResponseAc.ForPFNumber = Convert.ToString(user.EmpPfnumber);
                        #endregion

                        #region Send Email for Reset password Link
                        string redirect = ResponseAc.RestPasswordUrl;
                        string lnk1 = "<a href=" + redirect + "/";
                        string lnk2 = "  target='_blank'>Click here</a>";
                        string lnk = lnk1 + lnk2;

                        Dictionary<string, string> replacement = new Dictionary<string, string>();
                        replacement.Add("{UserName}", ResponseAc.ReceiverName);
                        replacement.Add("{ForEmployee}", ResponseAc.ForEmployee);
                        replacement.Add("{PFNumber}", ResponseAc.ForPFNumber);
                        replacement.Add("{Password}", user.Password);
                        replacement.Add("{PageLink}", lnk);

                        bool issent = await _iEmailSender.SendEmail(Convert.ToInt64(EnumList.EmailTemplateType.ForgotPassword), replacement, ResponseAc.ReceiverEmail);
                        if (issent)
                        {
                            ResponseAc.StatusCode = Convert.ToInt32(EnumList.ResponseType.Success);
                            ResponseAc.Message = _iStringConstant.EmailSent;

                            await _iLogManagement.SaveAuditActionLog((int)EnumList.AuditLogActionType.ForgotPassword, user.FullName, user.UserId, "", (int)EnumList.ActionTemplateTypes.ForgotPassword, user.UserId);
                        }
                        else
                        {
                            ResponseAc.StatusCode = Convert.ToInt32(EnumList.ResponseType.Error);
                            ResponseAc.Message = "Email not sent";
                        }
                        #endregion
                    }
                    else
                    {
                        ResponseAc.StatusCode = Convert.ToInt32(EnumList.ResponseType.Error);
                        ResponseAc.Message = "Employee is not system user.";
                    }
                }
                else
                {
                    ResponseAc.StatusCode = Convert.ToInt32(EnumList.ResponseType.NotFound);
                    ResponseAc.Message = _iStringConstant.EmailOrPfNumberNotValid;
                }


                return Ok(ResponseAc);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion


    }
}