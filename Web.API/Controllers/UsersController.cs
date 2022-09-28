
using ElmahCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Web.API.Helper;
using Web.Model;
using Web.Model.Common;
using Web.Services.Interfaces;

namespace Web.API.Controllers
{
    [Authorize]
    [RequestHandler]
    public class UsersController : Controller
    {
        private readonly Logger _logger;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _hostEnvironment;

        private IUsersService _usersService;
        public UsersController(IConfiguration config, IWebHostEnvironment environment, IUsersService usersService)
        {
            this._config = config;
            this._hostEnvironment = environment;
            this._logger = new Logger(_hostEnvironment, config);
            this._usersService = usersService;
        }
        [HttpGet("user/GetUserDetails")]
        public BaseResponse GetUserDetails(int UserId)
        {
            try
            {
                var response = this._usersService.GetUserDetails(UserId);
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "Data returned", Body = response };
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.Message.ToString(), Body = ex.ToString() };
            }

        }
        [HttpPost("user/SaveUser")]
        public BaseResponse SaveUser([FromBody] UserVM user)
        {
            try
            {
                return this._usersService.SaveUser(user);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.Message.ToString(), Body = ex.ToString() };
            }
        }
        [HttpPost("user/UpdatePassword")]
        public BaseResponse UpdatePassword([FromBody] UserVM user)
        {
            try
            {
                return this._usersService.UpdatePassword(user);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.Message.ToString(), Body = ex.ToString() };
            }
        }
        [HttpGet("user/CheckIfUsernameAvailable")]
        public BaseResponse CheckIfUsernameAvailable(string Username)
        {
            try
            {
                bool userNameAvailable = this._usersService.CheckIfUsernameAvailable(Username);
                if (!userNameAvailable)
                {
                    return new BaseResponse { Status = HttpStatusCode.OK, Message = "Username already exists", Body = new { usernameAvailable = false, message = "Username already exists" } };
                }
                else
                {
                    return new BaseResponse { Status = HttpStatusCode.OK, Message = "Username available", Body = new { usernameAvailable = true, message = "Username available" } };

                }
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.Message.ToString(), Body = ex.ToString() };
            }
        }
        [HttpGet("user/GetAllUser")]
        public BaseResponse GetAllUser()
        {
            try
            {
                var responseList = this._usersService.GetAllUser();
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "User's list returned", Body = responseList };
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.Message.ToString(), Body = ex.ToString() };
            }
        }
    }
}