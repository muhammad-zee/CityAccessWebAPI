using ElmahCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;
using System.Net;
using Web.API.Helper;
using Web.Model;
using Web.Model.Common;
using Web.Services.Enums;
using Web.Services.Interfaces;

namespace Web.API.Controllers
{

    public class UserAuthenticationController : Controller
    {
        private readonly IJwtAuthService _jwtAuth;
        private IConfiguration _config;
        Logger _logger;
        private IWebHostEnvironment _hostEnvironment;
        public UserAuthenticationController(IJwtAuthService jwtAuth, IConfiguration config, IWebHostEnvironment environment)
        {
            _jwtAuth = jwtAuth;
            _config = config;
            _hostEnvironment = environment;
            _logger = new Logger(_hostEnvironment);
        }

        [AllowAnonymous]
        [Description("User Login")]
        [HttpPost("auth/UserAuth")]
        public BaseResponse Login([FromBody] UserCredentialVM login)
        {
            try
            {
                BaseResponse response = _jwtAuth.Authentication(login);
                return response;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }
        [Description("Refresh Authenticaiton Token")]
        [HttpGet("auth/RefreshToken")]
        public BaseResponse RefreshToken(int UserId)
        {
            try
            {
                BaseResponse response = _jwtAuth.RefreshToken(UserId);
                return response;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [AllowAnonymous]
        [Description("Update Password")]
        [HttpPost("auth/UpdatePassword")]
        public BaseResponse UpdatePassword([FromBody] UserCredentialVM model) 
        {
            try
            {
                //////////////////////////// Encrypt Password //////////////////////////////
                var hashPswd = HelperExtension.Encrypt(model.password);
                /////////////////////////////////////////////////////////////////////////////
                model.password = hashPswd;

                var result = _jwtAuth.ConfirmPassword(model);
                return result;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        
        }

        //[Authorize]
        [Description("Add or Update User")]
        [HttpPost("auth/SaveUser")]
        public BaseResponse SaveUser([FromBody] RegisterCredentialVM register)
        {
            try
            {
                BaseResponse response = null;

                //////////////////////////// Generate Password //////////////////////////////
                var strongPassword = HelperExtension.CreateRandomPassword(register.FirstName);
                var hashPswd = HelperExtension.Encrypt(register.Password);
                /////////////////////////////////////////////////////////////////////////////

                register.Password = hashPswd;

                //byte[] bytes = System.IO.File.ReadAllBytes(@"D:\pic.jpg");
                //register.UserImageByte = bytes;

                string result = _jwtAuth.SaveUser(register);

                if (result != null)
                {
                    if (result.Equals(StatusEnums.Created.ToString()))
                    {
                        response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "User created successfully" + " \n UserName: " + register.UserName + " \n Password: " + strongPassword };
                    }
                    else if (result.Equals(StatusEnums.Updated.ToString()))
                    {
                        response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "User updated successfully" };
                    }
                    else
                    {
                        response = new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = "User already exist against this Email." };
                    }

                }
                else
                {
                    response = new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = "Model State is not valid." };
                }
                return response;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }


        #region Reset Password
        [AllowAnonymous]
        [Description("Send Mail on Forget Password")]
        [HttpGet("auth/ForgetPassword/{username}/{url}")]
        public BaseResponse ForgetPassword(string username, string url)
        {
            BaseResponse response = null;
            var result = _jwtAuth.SendResetPasswordMail(username, url);
            if (result != null)
            {
                if (result.Equals(StatusEnums.Success.ToString()))
                {
                    response = new BaseResponse()
                    {
                        Status = HttpStatusCode.OK,
                        Message = "Mail send successfully",
                    };
                }
                else
                {
                    response = new BaseResponse()
                    {
                        Status = HttpStatusCode.BadRequest,
                        Message = result
                    };
                }
            }
            else
            {
                response = new BaseResponse()
                {
                    Status = HttpStatusCode.NotFound,
                    Message = "User not found",
                };
            }
            return response;
        }

        [AllowAnonymous]
        [Description("Reset User Password")]
        [HttpPost("auth/ResetPassword")]
        public BaseResponse ResetPassword([FromBody] UserCredentialVM credential)
        {
            var response = new BaseResponse();
            var result = _jwtAuth.ResetPassword(credential);
            if (result != null)
            {
                if (result.Equals(StatusEnums.Success.ToString()))
                {
                    response = new BaseResponse()
                    {
                        Status = HttpStatusCode.OK,
                        Message = "Password changed successfully",
                    };
                }
                else
                {
                    response = new BaseResponse()
                    {
                        Status = HttpStatusCode.BadRequest,
                        Message = result
                    };
                }
            }
            else
            {
                response = new BaseResponse()
                {
                    Status = HttpStatusCode.NotFound,
                    Message = "User not found",
                };
            }
            return response;
        }
        #endregion


        #region Two Factor Authentication
        [AllowAnonymous]
        [HttpPost("auth/SendTwoFactorAuthenticationCode")]
        public BaseResponse SendAuthenticationCode([FromBody] RequestTwoFactorAuthenticationCode Authentication)
        {
            try
            {
                BaseResponse response = _jwtAuth.TwoFactorAuthentication(Authentication);
                return response;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }
        [AllowAnonymous]
        [HttpPost("auth/VerifyTwoFactorAuthenticationCode")]
        public BaseResponse VerifyAuthenticationCode([FromBody] VerifyTwoFactorAuthenticationCode verifyCode)
        {
            try
            {
                BaseResponse response = _jwtAuth.VerifyTwoFactorAuthentication(verifyCode);
                return response;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };

            }
        }
        #endregion
    }
}
