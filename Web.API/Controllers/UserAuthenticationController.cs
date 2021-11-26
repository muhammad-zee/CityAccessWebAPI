using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Web.API.Helper;
using Web.Model;
using Web.Model.Common;
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
       
        [HttpPost("auth/userAuth")]
        public BaseResponse Login([FromBody] UserCredential login)
        {
            try
            {
                BaseResponse response = new BaseResponse();
                response = _jwtAuth.Authentication(login);
                return response;                
            }
            catch(Exception ex)
            {
                _logger.LogExceptions(ex);
                return null;
            }
        }
   
        [HttpPost("auth/register")]
        public BaseResponse Register([FromBody] RegisterCredential register)
        {
            try
            {
                BaseResponse response = new BaseResponse();
                register.UserName = register.PrimaryEmail;

                //////////////// Generate Password ///////////////////////
                var strongPassword = HelperExtension.CreateRandomPassword(register.FirstName);
                var hashPswd = HelperExtension.Encrypt(strongPassword);
                //////////////////////////////////////////////////////////

                register.Password = hashPswd;
                string result = _jwtAuth.Register(register);
                
                if (!string.IsNullOrEmpty(result))
                {
                    if (result.Equals("Created"))
                    {
                        response = new BaseResponse() { Success = true, Message = result + " \nUserName: " + register.UserName + " \n Password: " + strongPassword };
                    }
                    else 
                    {
                        response = new BaseResponse() { Success = false, Message = "User is already exist against this Email." };
                    }
                    
                }
                else {
                    response = new BaseResponse() { Success = false, Message = "Model State is not valid." };
                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogExceptions(ex);
                return null;
            }
        }

    }
}
