
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
                return this._usersService.GetUserDetails(UserId);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.Message.ToString(), Body = ex.ToString() };
            }
           
        }
        [HttpPost("user/SaveUser")]
        public BaseResponse SaveUser(UserVM user)
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
        public BaseResponse UpdatePassword(UserVM password)
        {
            try
            {
                return this._usersService.UpdatePassword(password);
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
                return this._usersService.CheckIfUsernameAvailable(Username);
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