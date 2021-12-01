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

        [Description("User Login")]
        [HttpPost("auth/userAuth")]
        public BaseResponse Login([FromBody] UserCredential login)
        {
            try
            {
                BaseResponse response = _jwtAuth.Authentication(login);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Add or Update User")]
        [HttpPost("auth/SaveUser")]
        public BaseResponse SaveUser([FromBody] RegisterCredential register)
        {
            try
            {
                BaseResponse response = null;
                register.UserName = register.PrimaryEmail;

                //////////////////////////// Generate Password //////////////////////////////
                var strongPassword = HelperExtension.CreateRandomPassword(register.FirstName);
                var hashPswd = HelperExtension.Encrypt(strongPassword);
                /////////////////////////////////////////////////////////////////////////////

                register.Password = hashPswd;

                //byte[] bytes = System.IO.File.ReadAllBytes(@"D:\pic.jpg");
                //register.UserImage = bytes;

                string result = _jwtAuth.SaveUser(register);

                if (result != null)
                {
                    if (result.Equals(UserEnums.Created.ToString()))
                    {
                        response = new BaseResponse() { Status = HttpStatusCode.OK, Message = "User created successfully" + " \n UserName: " + register.UserName + " \n Password: " + strongPassword };
                    }
                    else if (result.Equals(UserEnums.Updated.ToString()))
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
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }


        #region Reset Password
        [Description("Send Mail on Forget Password")]
        [HttpGet("auth/forgetpassword/{username}/{url}")]
        public BaseResponse ForgetPassword(string username, string url)
        {
            var response = new BaseResponse();
            var result = _jwtAuth.SendResetPasswordMail(username, url);
            if (result != null)
            {
                if (result.Equals(StatusEnum.Success.ToString()))
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

        [HttpPost("auth/SendTwoFactorAuthenticationCode")]
        public BaseResponse SendVerificationCode([FromBody] RequestTwoFactorAuthenticationCode Authentication)
        {
            try
            {
                BaseResponse response = _jwtAuth.TwoFactorAuthentication(Authentication);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Reset User Password")]
        [HttpPost("auth/resetpassword")]
        public BaseResponse ResetPassword([FromBody] UserCredential credential)
        {
            var response = new BaseResponse();
            var result = _jwtAuth.ResetPassword(credential);
            if (result != null)
            {
                if (result.Equals(StatusEnum.Success.ToString()))
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
    }
}
